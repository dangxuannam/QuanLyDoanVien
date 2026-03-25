$connString = "Server=(localdb)\MSSQLLocalDB;Database=QuanLyDauTu;Integrated Security=True"
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
Update-Menu "/members" "Đoàn viên"
Update-Menu "/members/import" "Nhập Excel"
Update-Menu "/users" "Người dùng"
Update-Menu "/roles" "Phân quyền"
Update-Menu "/audit" "Nhật ký hoạt động"

$conn.Close()
Write-Host "Sidebar updated successfully with proper Unicode strings."
