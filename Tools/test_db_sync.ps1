$connString = "Server=(localdb)\MSSQLLocalDB;Database=QuanLyDoanVien;Integrated Security=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "UPDATE MenuItems SET MenuName = 'TEST_DB_SYNC' WHERE Url = '/members'"
$rows = $cmd.ExecuteNonQuery()
$conn.Close()
Write-Host "Updated $rows rows."

