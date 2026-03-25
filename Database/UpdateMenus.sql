-- Clear existing menus
DELETE FROM MenuItems;

-- Insert new menus
-- Parent menus
INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) 
VALUES (N'Dashboard', '/dashboard', 'fa-tachometer', 1, 1, 'HE_THONG');

INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) 
VALUES (N'Đoàn viên', '/members', 'fa-users', 2, 1, 'DOAN_VIEN');

INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) 
VALUES (N'Nhập Excel', '/members/import', 'fa-upload', 3, 1, 'DOAN_VIEN');

INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) 
VALUES (N'Người dùng', '/users', 'fa-user', 4, 1, 'HE_THONG');

INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) 
VALUES (N'Phân quyền', '/roles', 'fa-shield', 5, 1, 'HE_THONG');

INSERT INTO MenuItems (MenuName, Url, Icon, OrderIndex, IsActive, Module) 
VALUES (N'Nhật ký hoạt động', '/audit', 'fa-history', 6, 1, 'HE_THONG');

-- Optional: If you want submenus for members
-- DECLARE @MemberId INT = (SELECT Id FROM MenuItems WHERE MenuName = N'Đoàn viên');
-- INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, IsActive) 
-- VALUES (@MemberId, N'Danh sách', '/members', 'fa-list', 1, 1);
-- INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, IsActive) 
-- VALUES (@MemberId, N'Nhóm đoàn viên', '/member-groups', 'fa-object-group', 2, 1);
