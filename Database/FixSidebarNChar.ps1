$connString = "Server=(localdb)\MSSQLLocalDB;Database=QuanLyDoanVien;Integrated Security=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()

function Update-Menu($url, $sqlName) {
    $cmd.CommandText = "UPDATE MenuItems SET MenuName = $sqlName WHERE Url = '$url'"
    $cmd.ExecuteNonQuery()
}

# Using NCHAR() directly in SQL ensures DB-level Unicode integrity
Update-Menu "/members" "NCHAR(272) + N'o' + NCHAR(224) + N'n viÃªn'"
Update-Menu "/members/import" "N'Nh' + NCHAR(7853) + N'p Excel'"
Update-Menu "/users" "N'Ng' + NCHAR(432) + NCHAR(7901) + N'i dÃ¹ng'"
Update-Menu "/roles" "N'PhÃ¢n quy' + NCHAR(7873) + N'n'"
Update-Menu "/audit" "N'Nh' + NCHAR(7853) + N't kÃ½ ho' + NCHAR(7841) + N't ' + NCHAR(273) + NCHAR(7897) + N'ng'"

$conn.Close()
Write-Host "Sidebar updated with NCHAR SQL."

