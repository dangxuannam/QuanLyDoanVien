[Reflection.Assembly]::LoadFrom('c:\QuanLyDoanVien\QuanLyDoanVien.Web\bin\EPPlus.dll') | Out-Null
$file = 'c:\QuanLyDoanVien\QuanLyDoanVien.Web\uploads\2026\03\d8779b154edc4540ab9a5645f7d762f0.xlsx'
$pkg = New-Object OfficeOpenXml.ExcelPackage(New-Object IO.FileInfo($file))
$ws = $pkg.Workbook.Worksheets[1]
$headers = ""
for ($c = 1; $c -le 50; $c++) {
    $txt = $ws.Cells.Item(6, $c).Text
    $headers += "Col ${c}: ${txt}`r`n"
}
$headers | Out-File "c:\QuanLyDoanVien\headers.txt"
$pkg.Dispose()

