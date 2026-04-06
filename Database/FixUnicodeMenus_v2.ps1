$connString = "Server=(localdb)\MSSQLLocalDB;Database=QuanLyDoanVien;Integrated Security=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()

# Use UTF-8 explicitly for the script file and let PowerShell handle strings
$sql = @"
DELETE FROM MenuItems;
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Dashboard', '/dashboard', 'fa-tachometer', 1, 1, 'HE_THONG');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Đoàn Viên', '/members', 'fa-users', 2, 1, 'DOAN_VIEN');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Nhập Excel', '/members/import', 'fa-upload', 3, 1, 'DOAN_VIEN');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Người Dùng', '/users', 'fa-user', 4, 1, 'HE_THONG');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Phân Quyền', '/roles', 'fa-shield', 5, 1, 'HE_THONG');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Nhật Ký Hoạt Động', '/audit', 'fa-history', 6, 1, 'HE_THONG');
"@

$cmd.CommandText = $sql
$cmd.ExecuteNonQuery()
$conn.Close()
Write-Host "Done."

