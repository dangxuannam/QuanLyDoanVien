[Reflection.Assembly]::LoadFrom('c:\QuanLyDauTu\QuanLyDauTu.Web\bin\EPPlus.dll') | Out-Null
$dir = 'c:\QuanLyDauTu\QuanLyDauTu.Web\uploads\2026\03'
$files = Get-ChildItem -Path $dir -Filter d8779b154edc4540ab9a5645f7d762f0.xlsx
foreach ($f in $files) {
    Write-Host "File: $($f.Name)"
    $pkg = New-Object OfficeOpenXml.ExcelPackage(New-Object IO.FileInfo($f.FullName))
    foreach ($ws in $pkg.Workbook.Worksheets) {
        Write-Host "  Sheet: $($ws.Name)"
        if ($ws.Dimension -eq $null) { continue }
        for ($r = 1; $r -le 10; $r++) {
            $line = ""
            for ($c = 1; $c -le 15; $c++) {
                $line += $ws.Cells.Item($r, $c).Text + " | "
            }
            Write-Host "    Row $r` : $line"
        }
    }
    $pkg.Dispose()
}
