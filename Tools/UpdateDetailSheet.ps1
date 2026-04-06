$file = "c:\QuanLyDoanVien\QuanLyDoanVien.Web\Api\UnitsApiController.cs"
$content = Get-Content $file -Raw 

$oldDetail = @"
        private void WriteUnitDetailSheet(OfficeOpenXml.ExcelWorksheet ws, UnitSummary summary)
        {
            string[] headers = {
                "STT","Họ và tên","Giới tính","Ngày sinh","Tuổi","Dân tộc","Tôn giáo",
                "Nghề nghiệp","Học vấn","Chuyên môn",
                "Đảng viên DB","Đảng viên CT",
                "Tuổi 18-25","Tuổi 26-30","Tuổi 31+",
                "Cấp ủy cấp trên","Cấp ủy cơ sở",
                "Chức vụ chủ chốt"
            };
            for (int c = 0; c < headers.Length; c++)
            {
                ws.Cells[1, c + 1].Value = headers[c];
                ws.Cells[1, c + 1].Style.Font.Bold = true;
                ws.Cells[1, c + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[1, c + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(68, 114, 196));
                ws.Cells[1, c + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            int row = 2;
            foreach (var m in summary.MemberRows)
            {
                int age = m.Age ?? 0;
                ws.Cells[row, 1].Value  = row - 1;
                ws.Cells[row, 2].Value  = m.FullName;
                ws.Cells[row, 3].Value  = m.Gender;
                ws.Cells[row, 4].Value  = m.DateOfBirth?.ToString("dd/MM/yyyy");
                ws.Cells[row, 5].Value  = m.Age;
                ws.Cells[row, 6].Value  = m.Ethnicity;
                ws.Cells[row, 7].Value  = m.Religion;
                ws.Cells[row, 8].Value  = m.Profession;
                ws.Cells[row, 9].Value  = m.Education;
                ws.Cells[row, 10].Value = m.Expertise;
                ws.Cells[row, 11].Value = m.PartyDateProbationary?.ToString("dd/MM/yyyy");
                ws.Cells[row, 12].Value = m.PartyDateOfficial?.ToString("dd/MM/yyyy");
                ws.Cells[row, 13].Value = (age >= 18 && age <= 25) ? "x" : "";
                ws.Cells[row, 14].Value = (age >= 26 && age <= 30) ? "x" : "";
                ws.Cells[row, 15].Value = (age >= 31)              ? "x" : "";
                ws.Cells[row, 16].Value = m.CommunistAboveBase ? "x" : "";
                ws.Cells[row, 17].Value = m.CommunistBase      ? "x" : "";
                ws.Cells[row, 18].Value = m.PositionRoles;
                row++;
            }
            ws.Cells.AutoFitColumns();
        }
"@

$newDetail = @"
        private void WriteUnitDetailSheet(OfficeOpenXml.ExcelWorksheet ws, UnitSummary summary)
        {
            string[] headers = {
                "STT","Họ và tên","Giới tính","Ngày sinh","Tuổi","Dân tộc","Tôn giáo",
                "Nghề nghiệp","Học vấn","Chuyên môn","Lý luận chính trị",
                "Đảng viên DB","Đảng viên CT",
                "Tuổi 18-25","Tuổi 26-30","Tuổi 31+",
                "Cấp ủy cấp trên","Cấp ủy cơ sở",
                "Chức vụ chủ chốt"
            };
            for (int c = 0; c < headers.Length; c++)
            {
                ws.Cells[1, c + 1].Value = headers[c];
                ws.Cells[1, c + 1].Style.Font.Bold = true;
                ws.Cells[1, c + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[1, c + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(68, 114, 196));
                ws.Cells[1, c + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            int row = 2;
            foreach (var m in summary.MemberRows)
            {
                int age = m.Age ?? 0;
                ws.Cells[row, 1].Value  = row - 1;
                ws.Cells[row, 2].Value  = m.FullName;
                ws.Cells[row, 3].Value  = m.Gender;
                ws.Cells[row, 4].Value  = m.DateOfBirth?.ToString("dd/MM/yyyy");
                ws.Cells[row, 5].Value  = m.Age;
                ws.Cells[row, 6].Value  = m.Ethnicity;
                ws.Cells[row, 7].Value  = m.Religion;
                ws.Cells[row, 8].Value  = m.Profession;
                ws.Cells[row, 9].Value  = m.Education;
                ws.Cells[row, 10].Value = m.Expertise;
                ws.Cells[row, 11].Value = m.PoliticalTheory;
                ws.Cells[row, 12].Value = m.PartyDateProbationary?.ToString("dd/MM/yyyy");
                ws.Cells[row, 13].Value = m.PartyDateOfficial?.ToString("dd/MM/yyyy");
                ws.Cells[row, 14].Value = (age >= 18 && age <= 25) ? "x" : "";
                ws.Cells[row, 15].Value = (age >= 26 && age <= 30) ? "x" : "";
                ws.Cells[row, 16].Value = (age >= 31)              ? "x" : "";
                ws.Cells[row, 17].Value = m.CommunistAboveBase ? "x" : "";
                ws.Cells[row, 18].Value = m.CommunistBase      ? "x" : "";
                ws.Cells[row, 19].Value = m.PositionRoles;
                row++;
            }
            ws.Cells.AutoFitColumns();
        }
"@

$content = $content.Replace($oldDetail, $newDetail)
Set-Content $file $content
