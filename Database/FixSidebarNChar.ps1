$connString = "Server=(localdb)\MSSQLLocalDB;Database=QuanLyDauTu;Integrated Security=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()

function Update-Menu($url, $sqlName) {
    $cmd.CommandText = "UPDATE MenuItems SET MenuName = $sqlName WHERE Url = '$url'"
    $cmd.ExecuteNonQuery()
}

# Using NCHAR() directly in SQL ensures DB-level Unicode integrity
Update-Menu "/members" "NCHAR(272) + N'o' + NCHAR(224) + N'n viên'"
Update-Menu "/members/import" "N'Nh' + NCHAR(7853) + N'p Excel'"
Update-Menu "/users" "N'Ng' + NCHAR(432) + NCHAR(7901) + N'i dùng'"
Update-Menu "/roles" "N'Phân quy' + NCHAR(7873) + N'n'"
Update-Menu "/audit" "N'Nh' + NCHAR(7853) + N't ký ho' + NCHAR(7841) + N't ' + NCHAR(273) + NCHAR(7897) + N'ng'"

$conn.Close()
Write-Host "Sidebar updated with NCHAR SQL."
