[Reflection.Assembly]::LoadFrom('c:\QuanLyDoanVien\QuanLyDoanVien.Web\bin\EPPlus.dll') | Out-Null
$file = 'c:\QuanLyDoanVien\QuanLyDoanVien.Web\uploads\2026\03\d8779b154edc4540ab9a5645f7d762f0.xlsx'
$pkg = New-Object OfficeOpenXml.ExcelPackage(New-Object IO.FileInfo($file))
$ws = $pkg.Workbook.Worksheets[1]
if ($ws -eq $null) { $ws = $pkg.Workbook.Worksheets["Sheet1"] }
if ($ws -eq $null) { Write-Host "Sheet not found"; exit }

for ($r = 6; $r -le 10; $r++) {
    $line = "Row ${r}: "
    for ($c = 1; $c -le 50; $c++) {
        $val = $ws.Cells[$r, $c].Text
        if (!$val) { $val = "-" }
        $line += "$val | "
    }
    Write-Host $line
}
$pkg.Dispose()

