$file = "c:\QuanLyDoanVien\QuanLyDoanVien.Web\Services\ExcelService.cs"
$content = Get-Content $file -Raw 

# Use regex to replace the if condition for header
$pattern = 'if \(txt == "há»  tÃªn".*?\)'
$replacement = 'if (txt == "stt" || txt == "họ tên" || txt == "họ và tên" || txt == "full name" || txt.Contains("họ và") || txt.Contains("hoten") || txt.Contains("tên") || txt.Contains("há»"))'

$content = [System.Text.RegularExpressions.Regex]::Replace($content, $pattern, $replacement)
Set-Content $file $content
