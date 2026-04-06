$connString = "Server=(localdb)\MSSQLLocalDB;Database=QuanLyDoanVien;Integrated Security=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()

$sql = @"
DELETE FROM MenuItems;
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Dashboard', '/dashboard', 'fa-tachometer', 1, 1, 'HE_THONG');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Đoàn viên', '/members', 'fa-users', 2, 1, 'DOAN_VIEN');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Nhập Excel', '/members/import', 'fa-upload', 3, 1, 'DOAN_VIEN');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Người dùng', '/users', 'fa-user', 4, 1, 'HE_THONG');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Phân quyền', '/roles', 'fa-shield', 5, 1, 'HE_THONG');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Nhật ký hoạt động', '/audit', 'fa-history', 6, 1, 'HE_THONG');
"@

$cmd.CommandText = $sql
$cmd.ExecuteNonQuery()
$conn.Close()
Write-Host "Successfully updated MenuItems with Unicode support."

