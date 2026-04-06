$sql = "DELETE FROM MenuItems;
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Dashboard', '/dashboard', 'fa-tachometer', 1, 1, 'HE_THONG');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'ÄoÃ n viÃªn', '/members', 'fa-users', 2, 1, 'DOAN_VIEN');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Nháº­p Excel', '/members/import', 'fa-upload', 3, 1, 'DOAN_VIEN');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'NgÆ°á»i dÃ¹ng', '/users', 'fa-user', 4, 1, 'HE_THONG');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'PhÃ¢n quyá»n', '/roles', 'fa-shield', 5, 1, 'HE_THONG');
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) VALUES (N'Nháº­t kÃ½ hoáº¡t Ä‘á»™ng', '/audit', 'fa-history', 6, 1, 'HE_THONG');"

$sql | Out-File -FilePath UpdateMenus_UTF16.sql -Encoding Unicode
sqlcmd -S "(localdb)\MSSQLLocalDB" -d QuanLyDoanVien -i UpdateMenus_UTF16.sql

