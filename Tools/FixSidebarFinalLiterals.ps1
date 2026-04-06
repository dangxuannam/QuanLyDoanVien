$connString = "Server=(localdb)\MSSQLLocalDB;Database=QuanLyDoanVien;Integrated Security=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "UPDATE MenuItems SET MenuName = @name WHERE Url = @url"
$pName = $cmd.Parameters.Add("@name", [System.Data.SqlDbType]::NVarChar, 200)
$pUrl = $cmd.Parameters.Add("@url", [System.Data.SqlDbType]::NVarChar, 500)

function Update-Menu($url, $name) {
    $pUrl.Value = $url
    $pName.Value = $name
    $cmd.ExecuteNonQuery()
}

# Building strings using [char] to avoid script encoding issues
# ÄoÃ n viÃªn
$doanVien = [char]0x0110 + "o" + [char]0x00E0 + "n viÃªn"
# Nháº­p Excel
$nhapExcel = "Nh" + [char]0x1EAD + "p Excel"
# NgÆ°á»i dÃ¹ng
$nguoiDung = "Ng" + [char]0x01B0 + [char]0x1EDD + "i dÃ¹ng"
# PhÃ¢n quyá»n 
$phanQuyen = "PhÃ¢n quy" + [char]0x1EC1 + "n"
# Nháº­t kÃ½ hoáº¡t Ä‘á»™ng
$nhatKy = "Nh" + [char]0x1EAD + "t kÃ½ ho" + [char]0x1EA1 + "t " + [char]0x0111 + [char]0x1ED9 + "ng"

Update-Menu "/dashboard" "Dashboard"
Update-Menu "/members" $doanVien
Update-Menu "/members/import" $nhapExcel
Update-Menu "/users" $nguoiDung
Update-Menu "/roles" $phanQuyen
Update-Menu "/audit" $nhatKy

$conn.Close()
Write-Host "Sidebar updated using char literals."

