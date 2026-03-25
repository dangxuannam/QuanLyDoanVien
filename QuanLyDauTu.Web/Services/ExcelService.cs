using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace QuanLyDauTu.Services
{
    public class ExcelService
    {

        public ExcelParseResult ParseExcel(string filePath)
        {
            var result = new ExcelParseResult { Sheets = new List<ExcelSheetData>() };

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                foreach (var ws in package.Workbook.Worksheets)
                {
                    var sheet = new ExcelSheetData
                    {
                        SheetName = ws.Name,
                        Headers = new List<string>(),
                        Rows = new List<List<string>>()
                    };

                    int totalRows = ws.Dimension?.Rows ?? 0;
                    int totalCols = ws.Dimension?.Columns ?? 0;

                    if (totalRows == 0 || totalCols == 0)
                    {
                        sheet.IsEmpty = true;
                        result.Sheets.Add(sheet);
                        continue;
                    }

                    // First row = headers
                    for (int col = 1; col <= totalCols; col++)
                    {
                        var header = ws.Cells[1, col].Text?.Trim() ?? $"Cột {col}";
                        sheet.Headers.Add(string.IsNullOrEmpty(header) ? $"Cột {col}" : header);
                    }

                    sheet.TotalRows = totalRows - 1;

                    // Data rows (limit to 1000 for display)
                    int maxDisplayRows = Math.Min(totalRows, 1001);
                    for (int row = 2; row <= maxDisplayRows; row++)
                    {
                        var rowData = new List<string>();
                        for (int col = 1; col <= totalCols; col++)
                        {
                            var cell = ws.Cells[row, col];
                            string val = "";
                            if (cell.Value != null)
                            {
                                if (cell.Value is DateTime dt)
                                    val = dt.ToString("dd/MM/yyyy");
                                else if (cell.Value is double d && cell.Style.Numberformat.Format.Contains("%"))
                                    val = (d * 100).ToString("F2") + "%";
                                else
                                    val = cell.Text?.Trim() ?? cell.Value.ToString();
                            }
                            rowData.Add(val);
                        }
                        sheet.Rows.Add(rowData);
                    }

                    sheet.HasMore = totalRows > 1001;
                    result.Sheets.Add(sheet);
                }
            }

            result.SheetCount = result.Sheets.Count;
            return result;
        }
        public List<Dictionary<string, string>> ReadSheetForImport(string filePath, string sheetName)
        {
            var result = new List<Dictionary<string, string>>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet ws = null;
                try { ws = package.Workbook.Worksheets[sheetName]; } catch { }
                if (ws == null && package.Workbook.Worksheets.Count > 0)
                    ws = package.Workbook.Worksheets[0];
                if (ws == null || ws.Dimension == null) return result;

                int totalRows = ws.Dimension.Rows;
                int totalCols = ws.Dimension.Columns;

                // Find header row containing key identifiers
                int headerRow = 1;
                for (int r = 1; r <= Math.Min(totalRows, 10); r++)
                {
                    for (int c = 1; c <= totalCols; c++)
                    {
                        string txt = ws.Cells[r, c].Text?.ToLower().Trim() ?? "";
                        if (txt == "họ tên" || txt == "họ và tên" || txt == "full name" || txt == "stt")
                        {
                            headerRow = r;
                            goto foundHeader;
                        }
                    }
                }
                foundHeader:

                var mergedParent = BuildMergedCellMap(ws, headerRow, totalCols);

                bool hasTwoRowHeaders = false;
                if (headerRow + 1 < totalRows)
                {
                    int subHeaderNonEmpty = 0;
                    for (int c = 1; c <= Math.Min(totalCols, 30); c++)
                    {
                        string sub = ws.Cells[headerRow + 1, c].Text?.Trim();
                        if (!string.IsNullOrEmpty(sub)) subHeaderNonEmpty++;
                    }
                    if (subHeaderNonEmpty > 3) hasTwoRowHeaders = true;
                }

                var colNames = new string[totalCols + 1];
                var colParents = new string[totalCols + 1]; 
                var colSubs = new string[totalCols + 1];    

                string lastParent = "";
                for (int c = 1; c <= totalCols; c++)
                {
                    // Use merged cell parent if available, otherwise cell text
                    string parent = mergedParent.ContainsKey(c) ? mergedParent[c] : (ws.Cells[headerRow, c].Text?.Trim() ?? "");
                    string sub = hasTwoRowHeaders ? (ws.Cells[headerRow + 1, c].Text?.Trim() ?? "") : "";

                    // If parent is empty but we have a sub-header, inherit from previous
                    if (string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(lastParent))
                    {
                        if (!string.IsNullOrEmpty(sub) || mergedParent.ContainsKey(c))
                        {
                            parent = lastParent;
                        }
                    }
                    if (!string.IsNullOrEmpty(parent))
                    {
                        lastParent = parent;
                    }

                    colParents[c] = parent;
                    colSubs[c] = sub;

                    string name;
                    if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(sub))
                        name = parent + " - " + sub;
                    else if (!string.IsNullOrEmpty(parent))
                        name = parent;
                    else if (!string.IsNullOrEmpty(sub))
                        name = sub;
                    else
                        name = $"Col{c}";

                    colNames[c] = name;
                }

                int dataStartRow = hasTwoRowHeaders ? headerRow + 2 : headerRow + 1;

                for (int row = dataStartRow; row <= totalRows; row++)
                {
                    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    bool hasData = false;

                    // First pass: fill all columns with their values
                    for (int col = 1; col <= totalCols; col++)
                    {
                        var cell = ws.Cells[row, col];
                        string val = "";
                        if (cell.Value is DateTime dt)
                            val = dt.ToString("dd/MM/yyyy");
                        else if (cell.Value != null)
                            val = cell.Text?.Trim() ?? cell.Value.ToString().Trim();

                        string colKey = colNames[col];
                        if (!dict.ContainsKey(colKey))
                            dict[colKey] = val;
                        else
                            dict[colKey + "_" + col] = val;

                        if (!string.IsNullOrEmpty(val)) hasData = true;
                    }

                    var parentGroups = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
                    for (int c = 1; c <= totalCols; c++)
                    {
                        if (!string.IsNullOrEmpty(colParents[c]) && !string.IsNullOrEmpty(colSubs[c]))
                        {
                            if (!parentGroups.ContainsKey(colParents[c]))
                                parentGroups[colParents[c]] = new List<int>();
                            parentGroups[colParents[c]].Add(c);
                        }
                    }

                    foreach (var group in parentGroups)
                    {
                        string parentName = group.Key;
                        if (dict.ContainsKey(parentName) && !string.IsNullOrEmpty(dict[parentName]))
                        {
                            continue;
                        }

                        foreach (int c in group.Value)
                        {
                            string cellVal = ws.Cells[row, c].Text?.Trim() ?? "";
                            if (IsChecked(cellVal))
                            {
                                string subName = colSubs[c];
                                dict[parentName] = subName;

                                if (parentName.ToLower().Contains("ngày") && parentName.ToLower().Contains("sinh"))
                                {
                                }
                                break;
                            }
                        }

                        bool isDateGroup = parentName.ToLower().Contains("ngày") || 
                                          parentName.ToLower().Contains("kết nạp") ||
                                          parentName.ToLower().Contains("năm sinh");
                        if (isDateGroup)
                        {
                            foreach (int c in group.Value)
                            {
                                string cellVal = ws.Cells[row, c].Text?.Trim() ?? "";
                                if (!string.IsNullOrEmpty(cellVal) && !IsChecked(cellVal))
                                {
                                    if (!dict.ContainsKey(parentName) || string.IsNullOrEmpty(dict[parentName]) || IsChecked(dict[parentName]))
                                    {
                                        dict[parentName] = cellVal;                                      
                                        dict[parentName + "_gender"] = colSubs[c]; // "Nam" or "Nữ"
                                    }
                                }
                            }
                        }
                    }

                    if (!dict.ContainsKey("Họ và tên"))
                    {
                        string name = FindValue(dict, "Họ tên", "HOTEN", "FullName");
                        if (!string.IsNullOrEmpty(name)) dict["Họ và tên"] = name;
                    }

                    if (hasData) result.Add(dict);
                }
            }

            return result;
        }

        private Dictionary<int, string> BuildMergedCellMap(ExcelWorksheet ws, int headerRow, int totalCols)
        {
            var map = new Dictionary<int, string>();

            for (int c = 1; c <= totalCols; c++)
            {
                string txt = ws.Cells[headerRow, c].Text?.Trim() ?? "";
                if (!string.IsNullOrEmpty(txt))
                    map[c] = txt;
            }

            if (ws.MergedCells != null)
            {
                foreach (var mergeAddr in ws.MergedCells)
                {
                    var range = ws.Cells[mergeAddr];
                    if (range.Start.Row <= headerRow && range.End.Row >= headerRow)
                    {
                        string parentVal = ws.Cells[range.Start.Row, range.Start.Column].Text?.Trim() ?? "";
                        if (!string.IsNullOrEmpty(parentVal))
                        {
                            for (int c = range.Start.Column; c <= Math.Min(range.End.Column, totalCols); c++)
                            {
                                map[c] = parentVal;
                            }
                        }
                    }
                }
            }

            return map;
        }

        private bool IsChecked(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            val = val.ToLower().Trim();
            return val == "x" || val == "✓" || val == "✔" || val == "v" || val == "có" || val == "1" || val == "true";
        }

        private string FindValue(Dictionary<string, string> row, params string[] keys)
        {
            foreach (var k in keys)
            {
                var normK = System.Text.RegularExpressions.Regex.Replace(k, @"[^a-zA-Z0-9\u00C0-\u1EF9]", "").ToLower();
                var match = row.Keys.FirstOrDefault(x =>
                {
                    var normX = System.Text.RegularExpressions.Regex.Replace(x, @"[^a-zA-Z0-9\u00C0-\u1EF9]", "").ToLower();
                    return normX.Equals(normK);
                });
                if (match != null && !string.IsNullOrEmpty(row[match])) return row[match]?.Trim();
            }
            return null;
        }
    }

    public class ExcelParseResult
    {
        public int SheetCount { get; set; }
        public List<ExcelSheetData> Sheets { get; set; }
    }

    public class ExcelSheetData
    {
        public string SheetName { get; set; }
        public List<string> Headers { get; set; }
        public List<List<string>> Rows { get; set; }
        public int TotalRows { get; set; }
        public bool IsEmpty { get; set; }
        public bool HasMore { get; set; }
    }
}
