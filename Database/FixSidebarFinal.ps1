$connString = "Server=(localdb)\MSSQLLocalDB;Database=QuanLyDoanVien;Integrated Security=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()

function Update-Menu($url, $name) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "UPDATE MenuItems SET MenuName = @name WHERE Url = @url"
    $cmd.Parameters.AddWithValue("@name", $name) | Out-Null
    $cmd.Parameters.AddWithValue("@url", $url) | Out-Null
    $cmd.ExecuteNonQuery() | Out-Null
}

Update-Menu "/dashboard" "Dashboard"
Update-Menu "/members" "ÄoÃ n viÃªn"
Update-Menu "/members/import" "Nháº­p Excel"
Update-Menu "/users" "NgÆ°á»i dÃ¹ng"
Update-Menu "/roles" "PhÃ¢n quyá»n"
Update-Menu "/audit" "Nháº­t kÃ½ hoáº¡t Ä‘á»™ng"

$conn.Close()
Write-Host "Sidebar updated successfully with proper Unicode strings."

