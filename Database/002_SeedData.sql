USE QuanLyDauTu;
GO

-- =============================================
-- ROLES
-- =============================================
INSERT INTO Roles (RoleCode, RoleName, Description) VALUES
('ADMIN',       N'Quản trị hệ thống',           N'Toàn quyền hệ thống'),
('MANAGER',     N'Cán bộ quản lý',              N'Quản lý dữ liệu các module'),
('STAFF',       N'Cán bộ nhập liệu',            N'Nhập liệu và xem báo cáo'),
('VIEWER',      N'Người xem',                   N'Chỉ xem dữ liệu'),
('DOAN_VIEN',   N'Quản lý đoàn viên',           N'Quản lý thông tin đoàn viên');

-- =============================================
-- PERMISSIONS
-- =============================================
INSERT INTO Permissions (PermissionCode, PermissionName, Module) VALUES
-- User management
('USER_VIEW',       N'Xem người dùng',          'HE_THONG'),
('USER_CREATE',     N'Tạo người dùng',          'HE_THONG'),
('USER_EDIT',       N'Sửa người dùng',          'HE_THONG'),
('USER_DELETE',     N'Xóa người dùng',          'HE_THONG'),
('ROLE_MANAGE',     N'Quản lý vai trò',         'HE_THONG'),
('AUDIT_VIEW',      N'Xem nhật ký thao tác',    'HE_THONG'),
('MENU_MANAGE',     N'Quản lý menu',            'HE_THONG'),
-- Files
('FILE_UPLOAD',     N'Upload file',             'HE_THONG'),
('FILE_VIEW',       N'Xem file đính kèm',       'HE_THONG'),
('FILE_DELETE',     N'Xóa file',                'HE_THONG'),
-- Module 1 - Dau tu cong
('M1_VIEW',         N'Xem dự án đầu tư công',   'MODULE1'),
('M1_CREATE',       N'Tạo dự án đầu tư công',   'MODULE1'),
('M1_EDIT',         N'Sửa dự án đầu tư công',   'MODULE1'),
('M1_DELETE',       N'Xóa dự án đầu tư công',   'MODULE1'),
('M1_FUND',         N'Quản lý kế hoạch vốn',    'MODULE1'),
('M1_DISBURSE',     N'Quản lý giải ngân',        'MODULE1'),
('M1_REPORT',       N'Báo cáo đầu tư công',     'MODULE1'),
('M1_IMPORT',       N'Import Excel đầu tư công', 'MODULE1'),
-- Module 2 - Dau tu ngoai ngan sach
('M2_VIEW',         N'Xem dự án ngoài ngân sách','MODULE2'),
('M2_CREATE',       N'Tạo dự án ngoài ngân sách','MODULE2'),
('M2_EDIT',         N'Sửa dự án ngoài ngân sách','MODULE2'),
('M2_DELETE',       N'Xóa dự án ngoài ngân sách','MODULE2'),
('M2_INVESTOR',     N'Quản lý nhà đầu tư',      'MODULE2'),
('M2_REPORT',       N'Báo cáo dự án ngoài NS',  'MODULE2'),
('M2_IMPORT',       N'Import Excel ngoài NS',    'MODULE2'),
-- Module 3 - Quyet toan NS
('M3_VIEW',         N'Xem quyết toán ngân sách', 'MODULE3'),
('M3_CREATE',       N'Nhập quyết toán ngân sách','MODULE3'),
('M3_EDIT',         N'Sửa quyết toán ngân sách', 'MODULE3'),
('M3_REPORT',       N'Báo cáo quyết toán NS',   'MODULE3'),
('M3_IMPORT',       N'Import Excel quyết toán',  'MODULE3'),
-- Module 4 - Ban do
('M4_VIEW',         N'Xem bản đồ đầu tư',       'MODULE4'),
('M4_MANAGE',       N'Quản lý bản đồ đầu tư',   'MODULE4'),
-- Module 5 - KTXH
('M5_VIEW',         N'Xem chỉ tiêu KTXH',       'MODULE5'),
('M5_CREATE',       N'Nhập chỉ tiêu KTXH',      'MODULE5'),
('M5_MANAGE',       N'Quản lý danh mục KTXH',   'MODULE5'),
('M5_REPORT',       N'Báo cáo KTXH',            'MODULE5'),
-- Doan vien
('DV_VIEW',         N'Xem đoàn viên',           'DOAN_VIEN'),
('DV_CREATE',       N'Thêm đoàn viên',          'DOAN_VIEN'),
('DV_EDIT',         N'Sửa đoàn viên',           'DOAN_VIEN'),
('DV_DELETE',       N'Xóa đoàn viên',           'DOAN_VIEN'),
('DV_IMPORT',       N'Import đoàn viên Excel',  'DOAN_VIEN');

-- =============================================
-- ROLE PERMISSIONS - ADMIN gets all
-- =============================================
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id FROM Roles r, Permissions p WHERE r.RoleCode = 'ADMIN';

-- MANAGER gets most permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id FROM Roles r
CROSS JOIN Permissions p
WHERE r.RoleCode = 'MANAGER'
  AND p.PermissionCode IN (
    'USER_VIEW','FILE_UPLOAD','FILE_VIEW','AUDIT_VIEW',
    'M1_VIEW','M1_CREATE','M1_EDIT','M1_FUND','M1_DISBURSE','M1_REPORT','M1_IMPORT',
    'M2_VIEW','M2_CREATE','M2_EDIT','M2_INVESTOR','M2_REPORT','M2_IMPORT',
    'M3_VIEW','M3_CREATE','M3_EDIT','M3_REPORT','M3_IMPORT',
    'M4_VIEW','M4_MANAGE',
    'M5_VIEW','M5_CREATE','M5_MANAGE','M5_REPORT',
    'DV_VIEW','DV_CREATE','DV_EDIT','DV_IMPORT'
  );

-- STAFF gets limited permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id FROM Roles r
CROSS JOIN Permissions p
WHERE r.RoleCode = 'STAFF'
  AND p.PermissionCode IN (
    'FILE_UPLOAD','FILE_VIEW',
    'M1_VIEW','M1_CREATE','M1_IMPORT',
    'M2_VIEW','M2_CREATE','M2_IMPORT',
    'M3_VIEW','M3_CREATE','M3_IMPORT',
    'M4_VIEW','M5_VIEW','M5_CREATE',
    'DV_VIEW','DV_CREATE','DV_EDIT','DV_IMPORT'
  );

-- VIEWER gets only view permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id FROM Roles r
CROSS JOIN Permissions p
WHERE r.RoleCode = 'VIEWER'
  AND p.PermissionCode IN (
    'FILE_VIEW','M1_VIEW','M2_VIEW','M3_VIEW','M4_VIEW','M5_VIEW','DV_VIEW'
  );

-- DOAN_VIEN role
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id FROM Roles r
CROSS JOIN Permissions p
WHERE r.RoleCode = 'DOAN_VIEN'
  AND p.PermissionCode IN ('DV_VIEW','DV_CREATE','DV_EDIT','DV_DELETE','DV_IMPORT','FILE_UPLOAD','FILE_VIEW');

-- =============================================
-- MENU ITEMS
-- =============================================
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(NULL, N'Dashboard', '#!/dashboard', 'fa-tachometer', 1, NULL, NULL),
(NULL, N'Hệ thống', NULL, 'fa-cogs', 2, NULL, 'HE_THONG'),
(NULL, N'Module 1: ĐT Công', NULL, 'fa-building', 3, 'M1_VIEW', 'MODULE1'),
(NULL, N'Module 2: ĐT Ngoài NS', NULL, 'fa-industry', 4, 'M2_VIEW', 'MODULE2'),
(NULL, N'Module 3: Quyết toán NS', NULL, 'fa-money', 5, 'M3_VIEW', 'MODULE3'),
(NULL, N'Module 4: Bản đồ', NULL, 'fa-map-marker', 6, 'M4_VIEW', 'MODULE4'),
(NULL, N'Module 5: KTXH', NULL, 'fa-bar-chart', 7, 'M5_VIEW', 'MODULE5'),
(NULL, N'Đoàn viên', NULL, 'fa-users', 8, 'DV_VIEW', 'DOAN_VIEN');

-- Sub-menus for HE_THONG (ParentId=2)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(2, N'Quản lý tài khoản', '#!/users', 'fa-user', 1, 'USER_VIEW', 'HE_THONG'),
(2, N'Phân quyền vai trò', '#!/roles', 'fa-shield', 2, 'ROLE_MANAGE', 'HE_THONG'),
(2, N'Danh mục dùng chung', '#!/categories', 'fa-list', 3, 'USER_VIEW', 'HE_THONG'),
(2, N'Nhật ký thao tác', '#!/audit', 'fa-history', 4, 'AUDIT_VIEW', 'HE_THONG'),
(2, N'Tệp đính kèm', '#!/files', 'fa-paperclip', 5, 'FILE_VIEW', 'HE_THONG');

-- Sub-menus for MODULE1 (ParentId=3)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(3, N'Danh sách dự án', '#!/projects', 'fa-list-alt', 1, 'M1_VIEW', 'MODULE1'),
(3, N'Thêm dự án', '#!/projects/create', 'fa-plus', 2, 'M1_CREATE', 'MODULE1'),
(3, N'Kế hoạch vốn', '#!/project-funds', 'fa-dollar', 3, 'M1_FUND', 'MODULE1'),
(3, N'Theo dõi giải ngân', '#!/disbursements', 'fa-line-chart', 4, 'M1_DISBURSE', 'MODULE1'),
(3, N'Import Excel', '#!/projects/import', 'fa-upload', 5, 'M1_IMPORT', 'MODULE1'),
(3, N'Báo cáo', '#!/projects/report', 'fa-file-text', 6, 'M1_REPORT', 'MODULE1');

-- Sub-menus for MODULE2 (ParentId=4)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(4, N'Danh sách dự án', '#!/outside-projects', 'fa-list-alt', 1, 'M2_VIEW', 'MODULE2'),
(4, N'Nhà đầu tư', '#!/investors', 'fa-briefcase', 2, 'M2_INVESTOR', 'MODULE2'),
(4, N'Import Excel', '#!/outside-projects/import', 'fa-upload', 3, 'M2_IMPORT', 'MODULE2'),
(4, N'Báo cáo', '#!/outside-projects/report', 'fa-file-text', 4, 'M2_REPORT', 'MODULE2');

-- Sub-menus for MODULE3 (ParentId=5)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(5, N'Dự toán ngân sách', '#!/budget-plans', 'fa-calculator', 1, 'M3_VIEW', 'MODULE3'),
(5, N'Thu ngân sách', '#!/budget-revenues', 'fa-arrow-down', 2, 'M3_VIEW', 'MODULE3'),
(5, N'Chi ngân sách', '#!/budget-expenditures', 'fa-arrow-up', 3, 'M3_VIEW', 'MODULE3'),
(5, N'Import Excel', '#!/budget/import', 'fa-upload', 4, 'M3_IMPORT', 'MODULE3'),
(5, N'Báo cáo', '#!/budget/report', 'fa-file-text', 5, 'M3_REPORT', 'MODULE3');

-- Sub-menus for MODULE4 (ParentId=6)  
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(6, N'Bản đồ dự án', '#!/map', 'fa-globe', 1, 'M4_VIEW', 'MODULE4'),
(6, N'Quản lý điểm', '#!/map-projects', 'fa-map-pin', 2, 'M4_MANAGE', 'MODULE4');

-- Sub-menus for MODULE5 (ParentId=7)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(7, N'Dashboard KTXH', '#!/ktxh/dashboard', 'fa-bar-chart', 1, 'M5_VIEW', 'MODULE5'),
(7, N'Danh mục chỉ tiêu', '#!/ktxh/indicators', 'fa-th-list', 2, 'M5_MANAGE', 'MODULE5'),
(7, N'Nhập số liệu', '#!/ktxh/reports', 'fa-pencil', 3, 'M5_CREATE', 'MODULE5'),
(7, N'Báo cáo', '#!/ktxh/report', 'fa-file-text', 4, 'M5_REPORT', 'MODULE5');

-- Sub-menus for DOAN_VIEN (ParentId=8)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(8, N'Danh sách đoàn viên', '#!/members', 'fa-users', 1, 'DV_VIEW', 'DOAN_VIEN'),
(8, N'Thêm đoàn viên', '#!/members/create', 'fa-user-plus', 2, 'DV_CREATE', 'DOAN_VIEN'),
(8, N'Nhóm đoàn viên', '#!/member-groups', 'fa-object-group', 3, 'DV_VIEW', 'DOAN_VIEN'),
(8, N'Import Excel', '#!/members/import', 'fa-upload', 4, 'DV_IMPORT', 'DOAN_VIEN');

-- =============================================
-- CATEGORIES (Danh muc dung chung)
-- =============================================
INSERT INTO Categories (CategoryCode, CategoryName, Description) VALUES
('TINH_TRANG_DA',   N'Tình trạng dự án ĐTC',   N'Trạng thái dự án đầu tư công'),
('LINH_VUC_DA',     N'Lĩnh vực đầu tư',         N'Nhóm lĩnh vực dự án'),
('NGUON_VON',       N'Nguồn vốn đầu tư',         N'Các loại nguồn vốn'),
('QUAN_HUYEN',      N'Quận/Huyện Lâm Đồng',     N'Địa bàn hành chính'),
('LOAI_GIOI_TINH',  N'Giới tính',               N'Nam/Nữ/Khác');

-- Project status
INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, OrderIndex)
SELECT Id, 'CHUAN_BI', N'Chuẩn bị đầu tư', 1         FROM Categories WHERE CategoryCode='TINH_TRANG_DA'
UNION ALL SELECT Id, 'DANG_TRIEN_KHAI', N'Đang triển khai', 2    FROM Categories WHERE CategoryCode='TINH_TRANG_DA'
UNION ALL SELECT Id, 'HOAN_THANH', N'Hoàn thành', 3   FROM Categories WHERE CategoryCode='TINH_TRANG_DA'
UNION ALL SELECT Id, 'TAM_DUNG', N'Tạm dừng', 4       FROM Categories WHERE CategoryCode='TINH_TRANG_DA'
UNION ALL SELECT Id, 'HUY_BO', N'Hủy bỏ', 5           FROM Categories WHERE CategoryCode='TINH_TRANG_DA';

-- Fields
INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, OrderIndex)
SELECT Id, 'GTVT', N'Giao thông vận tải', 1           FROM Categories WHERE CategoryCode='LINH_VUC_DA'
UNION ALL SELECT Id, 'Y_TE', N'Y tế', 2               FROM Categories WHERE CategoryCode='LINH_VUC_DA'
UNION ALL SELECT Id, 'GD_DT', N'Giáo dục đào tạo', 3  FROM Categories WHERE CategoryCode='LINH_VUC_DA'
UNION ALL SELECT Id, 'NONG_NGHIEP', N'Nông nghiệp', 4 FROM Categories WHERE CategoryCode='LINH_VUC_DA'
UNION ALL SELECT Id, 'MOI_TRUONG', N'Môi trường', 5   FROM Categories WHERE CategoryCode='LINH_VUC_DA'
UNION ALL SELECT Id, 'KHAC', N'Khác', 6               FROM Categories WHERE CategoryCode='LINH_VUC_DA';

-- Districts of Lam Dong
INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, OrderIndex)
SELECT Id, 'DA_LAT', N'Thành phố Đà Lạt', 1       FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'BAO_LOC', N'Thành phố Bảo Lộc', 2  FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DAM_RONG', N'Huyện Đam Rông', 3    FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'LAC_DUONG', N'Huyện Lạc Dương', 4  FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DON_DUONG', N'Huyện Đơn Dương', 5  FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DUC_TRONG', N'Huyện Đức Trọng', 6  FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DI_LINH', N'Huyện Di Linh', 7      FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'BAO_LAM', N'Huyện Bảo Lâm', 8      FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DA_HUOAI', N'Huyện Đạ Huoai', 9    FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DA_TEH', N'Huyện Đạ Tẻh', 10       FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'CAT_TIEN', N'Huyện Cát Tiên', 11   FROM Categories WHERE CategoryCode='QUAN_HUYEN';

-- Gender
INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, OrderIndex)
SELECT Id, 'NAM', N'Nam', 1           FROM Categories WHERE CategoryCode='LOAI_GIOI_TINH'
UNION ALL SELECT Id, 'NU', N'Nữ', 2  FROM Categories WHERE CategoryCode='LOAI_GIOI_TINH'
UNION ALL SELECT Id, 'KHAC', N'Khác', 3 FROM Categories WHERE CategoryCode='LOAI_GIOI_TINH';

-- Member groups seed
INSERT INTO MemberGroups (GroupCode, GroupName, Description) VALUES
('DOAN_CO_SO', N'Đoàn cơ sở', N'Đoàn cơ sở tại đơn vị'),
('DOAN_PHUONG_XA', N'Đoàn phường/xã', N'Đoàn phường xã địa phương');

-- KTXH Indicator Groups
INSERT INTO KTXHIndicatorGroups (GroupCode, GroupName, OrderIndex) VALUES
('KINH_TE', N'Kinh tế', 1),
('XA_HOI', N'Xã hội', 2),
('MOI_TRUONG', N'Môi trường', 3);

-- Sample KTXH Indicators
INSERT INTO KTXHIndicators (Code, Name, GroupId, Unit, Period, IsActive, OrderIndex)
SELECT 'GDP_TANG_TRUONG', N'Tốc độ tăng trưởng GDP', Id, '%', N'Năm', 1, 1 FROM KTXHIndicatorGroups WHERE GroupCode='KINH_TE'
UNION ALL SELECT 'GDPPC', N'GDP bình quân đầu người', Id, N'Triệu đồng', N'Năm', 1, 2 FROM KTXHIndicatorGroups WHERE GroupCode='KINH_TE'
UNION ALL SELECT 'XUAT_KHAU', N'Kim ngạch xuất khẩu', Id, N'Triệu USD', N'Năm', 1, 3 FROM KTXHIndicatorGroups WHERE GroupCode='KINH_TE'
UNION ALL SELECT 'DAN_SO', N'Dân số', Id, N'Người', N'Năm', 1, 1 FROM KTXHIndicatorGroups WHERE GroupCode='XA_HOI'
UNION ALL SELECT 'HO_NGHEO', N'Tỷ lệ hộ nghèo', Id, '%', N'Năm', 1, 2 FROM KTXHIndicatorGroups WHERE GroupCode='XA_HOI';

-- Sample projects (Module 1)
INSERT INTO Projects (ProjectCode, ProjectName, InvestorName, ProjectGroup, Field, Location, District, TotalInvestment, FundingSource, StartDate, EndDate, Status) VALUES
(N'DA001', N'Đường tỉnh 720 - Đoạn Đà Lạt - Đơn Dương', N'Sở GTVT Lâm Đồng', 'B', N'Giao thông vận tải', N'Đà Lạt - Đơn Dương', N'Đơn Dương', 125.500, N'Ngân sách tỉnh', '2023-01-01', '2025-12-31', N'DangTrienKhai'),
(N'DA002', N'Bệnh viện đa khoa huyện Bảo Lâm', N'UBND huyện Bảo Lâm', 'B', N'Y tế', N'Huyện Bảo Lâm', N'BaoLam', 85.000, N'Ngân sách tỉnh', '2022-06-01', '2024-12-31', N'DangTrienKhai'),
(N'DA003', N'Trường THPT Lạc Dương', N'Sở GD&ĐT Lâm Đồng', 'C', N'Giáo dục đào tạo', N'Huyện Lạc Dương', N'LacDuong', 22.000, N'Ngân sách NS', '2023-09-01', '2025-08-31', N'ChuanBi');

-- Sample map projects
INSERT INTO MapProjects (ProjectCode, ProjectName, InvestorName, Location, District, Latitude, Longitude, Status, Field, TotalCapital, IsPublic) VALUES
(N'MAP001', N'Khu du lịch sinh thái Đà Lạt', N'Cty TNHH Sinh thái Lâm Đồng', N'Phường 3, Đà Lạt', N'Đà Lạt', 11.9404, 108.4580, N'DangThucHien', N'Du lịch', 250.000, 1),
(N'MAP002', N'Khu công nghiệp Phú Hội', N'Ban Quản lý KKT tỉnh', N'Huyện Đức Trọng', N'Đức Trọng', 11.7200, 108.3500, N'DaHoatDong', N'Công nghiệp', 500.000, 1);

PRINT 'Seed data inserted successfully!';
PRINT 'NOTE: Admin account will be created automatically on first application startup.';
PRINT 'Default admin credentials: Username=admin / Password=Admin@2024';
GO
