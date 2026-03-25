Add-Type -AssemblyName System.Data
$conn = New-Object System.Data.SqlClient.SqlConnection("Server=(localdb)\MSSQLLocalDB;Database=QuanLyDauTu;Integrated Security=True")
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
Update-Menu "/members" "$([char]0x0110)oàn viên"
Update-Menu "/members/import" "Nhập Excel"
Update-Menu "/users" "Người dùng"
Update-Menu "/roles" "Phân quyền"
Update-Menu "/audit" "Nhật ký hoạt động"

$conn.Close()
Write-Host "Sidebar menus updated with Unicode character support."
