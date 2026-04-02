$file = "c:\QuanLyDoanVien\QuanLyDoanVien.Web\Api\UnitsApiController.cs"
$content = Get-Content $file -Raw 

$oldSummary = @"
        private void WriteUnitSummarySheet(OfficeOpenXml.ExcelWorksheet ws, UnitSummary summary, string unitName, DateTime? importedAt)
        {
            var titleStyle = ws.Cells[1, 1];
            ws.Cells[1, 1].Value = `"TỔNG HỢP ĐƠN VỊ: {unitName.ToUpper()}`";
            ws.Cells[1, 1, 1, 4].Merge = true;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;

            ws.Cells[2, 1].Value = `"Ngày tổng hợp: {(importedAt ?? DateTime.Now):dd/MM/yyyy HH:mm}`";
            ws.Cells[2, 1, 2, 4].Merge = true;

            int r = 4;
            void Row(string label, object val)
            {
                ws.Cells[r, 1].Value = label; ws.Cells[r, 1].Style.Font.Bold = true;
                ws.Cells[r, 2].Value = val;
                r++;
            }

            Row("Tổng số đoàn viên",            summary.TotalMembers);
            Row("Nam",                            summary.MaleCount);
            Row("Nữ",                             summary.FemaleCount);
            r++;
            Row("Dân tộc Kinh",                  summary.EthnicityKinh);
            Row("Dân tộc Khác",                  summary.EthnicityOther);
            r++;
            Row("Tôn giáo",                      "");
            foreach (var kv in summary.Religions)  { ws.Cells[r, 2].Value = kv.Key; ws.Cells[r, 3].Value = kv.Value; r++; }
            r++;
            Row("Đoàn viên là đảng viên (Dự bị)",  summary.PartyProbationaryCount);
            Row("Đoàn viên là đảng viên (CT)",      summary.PartyOfficialCount);
            r++;
            Row("Độ tuổi 18-25",                  summary.Age18To25);
            Row("Độ tuổi 26-30",                  summary.Age26To30);
            Row("Độ tuổi 31 trở lên",             summary.Age31Plus);
            r++;
            Row("Nghề nghiệp",                    "");
            foreach (var kv in summary.Professions) { ws.Cells[r, 2].Value = kv.Key; ws.Cells[r, 3].Value = kv.Value; r++; }
            r++;
            Row("Học vấn",                        "");
            foreach (var kv in summary.Educations)  { ws.Cells[r, 2].Value = kv.Key; ws.Cells[r, 3].Value = kv.Value; r++; }
            r++;
            Row("Chuyên môn",                     "");
            foreach (var kv in summary.Expertises)  { ws.Cells[r, 2].Value = kv.Key; ws.Cells[r, 3].Value = kv.Value; r++; }
            r++;
            Row("Tham gia cấp ủy cấp trên cơ sở", summary.CommunistAboveBase);
            Row("Tham gia cấp ủy cơ sở",          summary.CommunistBase);
            r++;
            Row("Chức vụ chủ chốt",               "");
            foreach (var kv in summary.PositionRoles) { ws.Cells[r, 2].Value = kv.Key; ws.Cells[r, 3].Value = kv.Value; r++; }

            ws.Cells.AutoFitColumns();
        }
"@

$newSummary = @"
        private void WriteUnitSummarySheet(OfficeOpenXml.ExcelWorksheet ws, UnitSummary summary, string unitName, DateTime? importedAt)
        {
            ws.Cells[1, 1].Value = `"TỔNG HỢP ĐƠN VỊ: {unitName.ToUpper()}`";
            ws.Cells[1, 1, 1, 3].Merge = true;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;

            ws.Cells[2, 1].Value = `"Ngày tổng hợp: {(importedAt ?? DateTime.Now):dd/MM/yyyy HH:mm}`";
            ws.Cells[2, 1, 2, 3].Merge = true;

            int r = 4;
            void Row(string label, object val)
            {
                ws.Cells[r, 1].Value = label; ws.Cells[r, 1].Style.Font.Bold = true;
                ws.Cells[r, 2].Value = val;
                r++;
            }
            void SubRow(string label, object val)
            {
                ws.Cells[r, 1].Value = $"        {label}";
                ws.Cells[r, 2].Value = val;
                r++;
            }

            Row("Tổng số đoàn viên", summary.TotalMembers);
            Row("Nam", summary.MaleCount);
            Row("Nữ", summary.FemaleCount);
            r++;
            Row("Dân tộc Kinh", summary.EthnicityKinh);
            Row("Dân tộc Khác", summary.EthnicityOther);
            r++;
            Row("Tôn giáo", "");
            foreach (var kv in summary.Religions) SubRow(kv.Key, kv.Value);
            r++;
            Row("Đoàn viên là đảng viên (Dự bị)", summary.PartyProbationaryCount);
            Row("Đoàn viên là đảng viên (CT)", summary.PartyOfficialCount);
            r++;
            Row("Độ tuổi 18-25", summary.Age18To25);
            Row("Độ tuổi 26-30", summary.Age26To30);
            Row("Độ tuổi 31 trở lên", summary.Age31Plus);
            r++;
            Row("Nghề nghiệp", "");
            foreach(var kv in new[]{"Công chức", "Viên chức", "Sinh viên", "Khác"}) SubRow(kv, summary.Professions.ContainsKey(kv)? summary.Professions[kv] : 0);
            r++;
            Row("Học vấn", "");
            foreach(var kv in new[]{"THCS", "THPT", "Khác"}) SubRow(kv, summary.Educations.ContainsKey(kv)? summary.Educations[kv] : 0);
            r++;
            Row("Chuyên môn", "");
            foreach(var kv in new[]{"Tiến sĩ", "Thạc sĩ", "Đại học", "Cao đẳng", "Khác"}) SubRow(kv, summary.Expertises.ContainsKey(kv)? summary.Expertises[kv] : 0);
            r++;
            Row("Lý luận chính trị", "");
            foreach(var kv in new[]{"Sơ cấp", "Trung cấp", "Cao cấp", "Cử nhân", "Khác"}) SubRow(kv, summary.PoliticalTheories.ContainsKey(kv)? summary.PoliticalTheories[kv] : 0);
            r++;
            Row("Tham gia cấp ủy cấp trên cơ sở", summary.CommunistAboveBase);
            Row("Tham gia cấp ủy cơ sở", summary.CommunistBase);
            r++;
            Row("Chức vụ chủ chốt", "");
            foreach(var kv in new[]{"Ban chấp hành", "Ban thường vụ", "Bí thư", "Phó Bí thư", "Cấp trưởng", "Cấp phó"}) SubRow(kv, summary.PositionRoles.ContainsKey(kv)? summary.PositionRoles[kv] : 0);

            // Giao diện Border và Autofit
            using (var range = ws.Cells[4, 1, r - 1, 2])
            {
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }
            ws.Column(1).Width = 45;
            ws.Column(2).Width = 15;
        }
"@

$content = $content.Replace($oldSummary, $newSummary)
Set-Content $file $content
