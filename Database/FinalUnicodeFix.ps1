Add-Type -AssemblyName System.Data
$conn = New-Object System.Data.SqlClient.SqlConnection("Server=(localdb)\MSSQLLocalDB;Database=QuanLyDoanVien;Integrated Security=True")
$conn.Open()

function Update-Menu($url, $name) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "UPDATE MenuItems SET MenuName = @name WHERE Url = @url"
    $cmd.Parameters.AddWithValue("@name", $name) | Out-Null
    $cmd.Parameters.AddWithValue("@url", $url) | Out-Null
    $cmd.ExecuteNonQuery() | Out-Null
}

# Explicitly define names with char codes where needed (Vietnamese characters)
Update-Menu "/dashboard" "Dashboard"
Update-Menu "/members" "$([char]0x0110)oÃ n viÃªn"
Update-Menu "/members/import" "Nháº­p Excel"
Update-Menu "/users" "NgÆ°á»i dÃ¹ng"
Update-Menu "/roles" "PhÃ¢n quyá»n"
Update-Menu "/audit" "Nháº­t kÃ½ hoáº¡t Ä‘á»™ng"

$conn.Close()
Write-Host "Sidebar menus updated with Unicode character support."

