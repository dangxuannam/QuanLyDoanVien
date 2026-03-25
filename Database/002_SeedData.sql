USE QuanLyDoanVien;
GO

-- =============================================
-- ROLES
-- =============================================
INSERT INTO Roles (RoleCode, RoleName, Description) VALUES
('ADMIN',       N'Quáº£n trá»‹ há»‡ thá»‘ng',           N'ToÃ n quyá»n há»‡ thá»‘ng'),
('MANAGER',     N'CÃ¡n bá»™ quáº£n lÃ½',              N'Quáº£n lÃ½ dá»¯ liá»‡u cÃ¡c module'),
('STAFF',       N'CÃ¡n bá»™ nháº­p liá»‡u',            N'Nháº­p liá»‡u vÃ  xem bÃ¡o cÃ¡o'),
('VIEWER',      N'NgÆ°á»i xem',                   N'Chá»‰ xem dá»¯ liá»‡u'),
('DOAN_VIEN',   N'Quáº£n lÃ½ Ä‘oÃ n viÃªn',           N'Quáº£n lÃ½ thÃ´ng tin Ä‘oÃ n viÃªn');

-- =============================================
-- PERMISSIONS
-- =============================================
INSERT INTO Permissions (PermissionCode, PermissionName, Module) VALUES
-- User management
('USER_VIEW',       N'Xem ngÆ°á»i dÃ¹ng',          'HE_THONG'),
('USER_CREATE',     N'Táº¡o ngÆ°á»i dÃ¹ng',          'HE_THONG'),
('USER_EDIT',       N'Sá»­a ngÆ°á»i dÃ¹ng',          'HE_THONG'),
('USER_DELETE',     N'XÃ³a ngÆ°á»i dÃ¹ng',          'HE_THONG'),
('ROLE_MANAGE',     N'Quáº£n lÃ½ vai trÃ²',         'HE_THONG'),
('AUDIT_VIEW',      N'Xem nháº­t kÃ½ thao tÃ¡c',    'HE_THONG'),
('MENU_MANAGE',     N'Quáº£n lÃ½ menu',            'HE_THONG'),
-- Files
('FILE_UPLOAD',     N'Upload file',             'HE_THONG'),
('FILE_VIEW',       N'Xem file Ä‘Ã­nh kÃ¨m',       'HE_THONG'),
('FILE_DELETE',     N'XÃ³a file',                'HE_THONG'),
-- Module 1 - Dau tu cong
('M1_VIEW',         N'Xem dá»± Ã¡n Ä‘áº§u tÆ° cÃ´ng',   'MODULE1'),
('M1_CREATE',       N'Táº¡o dá»± Ã¡n Ä‘áº§u tÆ° cÃ´ng',   'MODULE1'),
('M1_EDIT',         N'Sá»­a dá»± Ã¡n Ä‘áº§u tÆ° cÃ´ng',   'MODULE1'),
('M1_DELETE',       N'XÃ³a dá»± Ã¡n Ä‘áº§u tÆ° cÃ´ng',   'MODULE1'),
('M1_FUND',         N'Quáº£n lÃ½ káº¿ hoáº¡ch vá»‘n',    'MODULE1'),
('M1_DISBURSE',     N'Quáº£n lÃ½ giáº£i ngÃ¢n',        'MODULE1'),
('M1_REPORT',       N'BÃ¡o cÃ¡o Ä‘áº§u tÆ° cÃ´ng',     'MODULE1'),
('M1_IMPORT',       N'Import Excel Ä‘áº§u tÆ° cÃ´ng', 'MODULE1'),
-- Module 2 - Dau tu ngoai ngan sach
('M2_VIEW',         N'Xem dá»± Ã¡n ngoÃ i ngÃ¢n sÃ¡ch','MODULE2'),
('M2_CREATE',       N'Táº¡o dá»± Ã¡n ngoÃ i ngÃ¢n sÃ¡ch','MODULE2'),
('M2_EDIT',         N'Sá»­a dá»± Ã¡n ngoÃ i ngÃ¢n sÃ¡ch','MODULE2'),
('M2_DELETE',       N'XÃ³a dá»± Ã¡n ngoÃ i ngÃ¢n sÃ¡ch','MODULE2'),
('M2_INVESTOR',     N'Quáº£n lÃ½ nhÃ  Ä‘áº§u tÆ°',      'MODULE2'),
('M2_REPORT',       N'BÃ¡o cÃ¡o dá»± Ã¡n ngoÃ i NS',  'MODULE2'),
('M2_IMPORT',       N'Import Excel ngoÃ i NS',    'MODULE2'),
-- Module 3 - Quyet toan NS
('M3_VIEW',         N'Xem quyáº¿t toÃ¡n ngÃ¢n sÃ¡ch', 'MODULE3'),
('M3_CREATE',       N'Nháº­p quyáº¿t toÃ¡n ngÃ¢n sÃ¡ch','MODULE3'),
('M3_EDIT',         N'Sá»­a quyáº¿t toÃ¡n ngÃ¢n sÃ¡ch', 'MODULE3'),
('M3_REPORT',       N'BÃ¡o cÃ¡o quyáº¿t toÃ¡n NS',   'MODULE3'),
('M3_IMPORT',       N'Import Excel quyáº¿t toÃ¡n',  'MODULE3'),
-- Module 4 - Ban do
('M4_VIEW',         N'Xem báº£n Ä‘á»“ Ä‘áº§u tÆ°',       'MODULE4'),
('M4_MANAGE',       N'Quáº£n lÃ½ báº£n Ä‘á»“ Ä‘áº§u tÆ°',   'MODULE4'),
-- Module 5 - KTXH
('M5_VIEW',         N'Xem chá»‰ tiÃªu KTXH',       'MODULE5'),
('M5_CREATE',       N'Nháº­p chá»‰ tiÃªu KTXH',      'MODULE5'),
('M5_MANAGE',       N'Quáº£n lÃ½ danh má»¥c KTXH',   'MODULE5'),
('M5_REPORT',       N'BÃ¡o cÃ¡o KTXH',            'MODULE5'),
-- Doan vien
('DV_VIEW',         N'Xem Ä‘oÃ n viÃªn',           'DOAN_VIEN'),
('DV_CREATE',       N'ThÃªm Ä‘oÃ n viÃªn',          'DOAN_VIEN'),
('DV_EDIT',         N'Sá»­a Ä‘oÃ n viÃªn',           'DOAN_VIEN'),
('DV_DELETE',       N'XÃ³a Ä‘oÃ n viÃªn',           'DOAN_VIEN'),
('DV_IMPORT',       N'Import Ä‘oÃ n viÃªn Excel',  'DOAN_VIEN');

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
(NULL, N'Há»‡ thá»‘ng', NULL, 'fa-cogs', 2, NULL, 'HE_THONG'),
(NULL, N'Module 1: ÄT CÃ´ng', NULL, 'fa-building', 3, 'M1_VIEW', 'MODULE1'),
(NULL, N'Module 2: ÄT NgoÃ i NS', NULL, 'fa-industry', 4, 'M2_VIEW', 'MODULE2'),
(NULL, N'Module 3: Quyáº¿t toÃ¡n NS', NULL, 'fa-money', 5, 'M3_VIEW', 'MODULE3'),
(NULL, N'Module 4: Báº£n Ä‘á»“', NULL, 'fa-map-marker', 6, 'M4_VIEW', 'MODULE4'),
(NULL, N'Module 5: KTXH', NULL, 'fa-bar-chart', 7, 'M5_VIEW', 'MODULE5'),
(NULL, N'ÄoÃ n viÃªn', NULL, 'fa-users', 8, 'DV_VIEW', 'DOAN_VIEN');

-- Sub-menus for HE_THONG (ParentId=2)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(2, N'Quáº£n lÃ½ tÃ i khoáº£n', '#!/users', 'fa-user', 1, 'USER_VIEW', 'HE_THONG'),
(2, N'PhÃ¢n quyá»n vai trÃ²', '#!/roles', 'fa-shield', 2, 'ROLE_MANAGE', 'HE_THONG'),
(2, N'Danh má»¥c dÃ¹ng chung', '#!/categories', 'fa-list', 3, 'USER_VIEW', 'HE_THONG'),
(2, N'Nháº­t kÃ½ thao tÃ¡c', '#!/audit', 'fa-history', 4, 'AUDIT_VIEW', 'HE_THONG'),
(2, N'Tá»‡p Ä‘Ã­nh kÃ¨m', '#!/files', 'fa-paperclip', 5, 'FILE_VIEW', 'HE_THONG');

-- Sub-menus for MODULE1 (ParentId=3)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(3, N'Danh sÃ¡ch dá»± Ã¡n', '#!/projects', 'fa-list-alt', 1, 'M1_VIEW', 'MODULE1'),
(3, N'ThÃªm dá»± Ã¡n', '#!/projects/create', 'fa-plus', 2, 'M1_CREATE', 'MODULE1'),
(3, N'Káº¿ hoáº¡ch vá»‘n', '#!/project-funds', 'fa-dollar', 3, 'M1_FUND', 'MODULE1'),
(3, N'Theo dÃµi giáº£i ngÃ¢n', '#!/disbursements', 'fa-line-chart', 4, 'M1_DISBURSE', 'MODULE1'),
(3, N'Import Excel', '#!/projects/import', 'fa-upload', 5, 'M1_IMPORT', 'MODULE1'),
(3, N'BÃ¡o cÃ¡o', '#!/projects/report', 'fa-file-text', 6, 'M1_REPORT', 'MODULE1');

-- Sub-menus for MODULE2 (ParentId=4)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(4, N'Danh sÃ¡ch dá»± Ã¡n', '#!/outside-projects', 'fa-list-alt', 1, 'M2_VIEW', 'MODULE2'),
(4, N'NhÃ  Ä‘áº§u tÆ°', '#!/investors', 'fa-briefcase', 2, 'M2_INVESTOR', 'MODULE2'),
(4, N'Import Excel', '#!/outside-projects/import', 'fa-upload', 3, 'M2_IMPORT', 'MODULE2'),
(4, N'BÃ¡o cÃ¡o', '#!/outside-projects/report', 'fa-file-text', 4, 'M2_REPORT', 'MODULE2');

-- Sub-menus for MODULE3 (ParentId=5)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(5, N'Dá»± toÃ¡n ngÃ¢n sÃ¡ch', '#!/budget-plans', 'fa-calculator', 1, 'M3_VIEW', 'MODULE3'),
(5, N'Thu ngÃ¢n sÃ¡ch', '#!/budget-revenues', 'fa-arrow-down', 2, 'M3_VIEW', 'MODULE3'),
(5, N'Chi ngÃ¢n sÃ¡ch', '#!/budget-expenditures', 'fa-arrow-up', 3, 'M3_VIEW', 'MODULE3'),
(5, N'Import Excel', '#!/budget/import', 'fa-upload', 4, 'M3_IMPORT', 'MODULE3'),
(5, N'BÃ¡o cÃ¡o', '#!/budget/report', 'fa-file-text', 5, 'M3_REPORT', 'MODULE3');

-- Sub-menus for MODULE4 (ParentId=6)  
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(6, N'Báº£n Ä‘á»“ dá»± Ã¡n', '#!/map', 'fa-globe', 1, 'M4_VIEW', 'MODULE4'),
(6, N'Quáº£n lÃ½ Ä‘iá»ƒm', '#!/map-projects', 'fa-map-pin', 2, 'M4_MANAGE', 'MODULE4');

-- Sub-menus for MODULE5 (ParentId=7)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(7, N'Dashboard KTXH', '#!/ktxh/dashboard', 'fa-bar-chart', 1, 'M5_VIEW', 'MODULE5'),
(7, N'Danh má»¥c chá»‰ tiÃªu', '#!/ktxh/indicators', 'fa-th-list', 2, 'M5_MANAGE', 'MODULE5'),
(7, N'Nháº­p sá»‘ liá»‡u', '#!/ktxh/reports', 'fa-pencil', 3, 'M5_CREATE', 'MODULE5'),
(7, N'BÃ¡o cÃ¡o', '#!/ktxh/report', 'fa-file-text', 4, 'M5_REPORT', 'MODULE5');

-- Sub-menus for DOAN_VIEN (ParentId=8)
INSERT INTO MenuItems (ParentId, MenuName, Url, Icon, OrderIndex, RequiredPermission, Module) VALUES
(8, N'Danh sÃ¡ch Ä‘oÃ n viÃªn', '#!/members', 'fa-users', 1, 'DV_VIEW', 'DOAN_VIEN'),
(8, N'ThÃªm Ä‘oÃ n viÃªn', '#!/members/create', 'fa-user-plus', 2, 'DV_CREATE', 'DOAN_VIEN'),
(8, N'NhÃ³m Ä‘oÃ n viÃªn', '#!/member-groups', 'fa-object-group', 3, 'DV_VIEW', 'DOAN_VIEN'),
(8, N'Import Excel', '#!/members/import', 'fa-upload', 4, 'DV_IMPORT', 'DOAN_VIEN');

-- =============================================
-- CATEGORIES (Danh muc dung chung)
-- =============================================
INSERT INTO Categories (CategoryCode, CategoryName, Description) VALUES
('TINH_TRANG_DA',   N'TÃ¬nh tráº¡ng dá»± Ã¡n ÄTC',   N'Tráº¡ng thÃ¡i dá»± Ã¡n Ä‘áº§u tÆ° cÃ´ng'),
('LINH_VUC_DA',     N'LÄ©nh vá»±c Ä‘áº§u tÆ°',         N'NhÃ³m lÄ©nh vá»±c dá»± Ã¡n'),
('NGUON_VON',       N'Nguá»“n vá»‘n Ä‘áº§u tÆ°',         N'CÃ¡c loáº¡i nguá»“n vá»‘n'),
('QUAN_HUYEN',      N'Quáº­n/Huyá»‡n LÃ¢m Äá»“ng',     N'Äá»‹a bÃ n hÃ nh chÃ­nh'),
('LOAI_GIOI_TINH',  N'Giá»›i tÃ­nh',               N'Nam/Ná»¯/KhÃ¡c');

-- Project status
INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, OrderIndex)
SELECT Id, 'CHUAN_BI', N'Chuáº©n bá»‹ Ä‘áº§u tÆ°', 1         FROM Categories WHERE CategoryCode='TINH_TRANG_DA'
UNION ALL SELECT Id, 'DANG_TRIEN_KHAI', N'Äang triá»ƒn khai', 2    FROM Categories WHERE CategoryCode='TINH_TRANG_DA'
UNION ALL SELECT Id, 'HOAN_THANH', N'HoÃ n thÃ nh', 3   FROM Categories WHERE CategoryCode='TINH_TRANG_DA'
UNION ALL SELECT Id, 'TAM_DUNG', N'Táº¡m dá»«ng', 4       FROM Categories WHERE CategoryCode='TINH_TRANG_DA'
UNION ALL SELECT Id, 'HUY_BO', N'Há»§y bá»', 5           FROM Categories WHERE CategoryCode='TINH_TRANG_DA';

-- Fields
INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, OrderIndex)
SELECT Id, 'GTVT', N'Giao thÃ´ng váº­n táº£i', 1           FROM Categories WHERE CategoryCode='LINH_VUC_DA'
UNION ALL SELECT Id, 'Y_TE', N'Y táº¿', 2               FROM Categories WHERE CategoryCode='LINH_VUC_DA'
UNION ALL SELECT Id, 'GD_DT', N'GiÃ¡o dá»¥c Ä‘Ã o táº¡o', 3  FROM Categories WHERE CategoryCode='LINH_VUC_DA'
UNION ALL SELECT Id, 'NONG_NGHIEP', N'NÃ´ng nghiá»‡p', 4 FROM Categories WHERE CategoryCode='LINH_VUC_DA'
UNION ALL SELECT Id, 'MOI_TRUONG', N'MÃ´i trÆ°á»ng', 5   FROM Categories WHERE CategoryCode='LINH_VUC_DA'
UNION ALL SELECT Id, 'KHAC', N'KhÃ¡c', 6               FROM Categories WHERE CategoryCode='LINH_VUC_DA';

-- Districts of Lam Dong
INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, OrderIndex)
SELECT Id, 'DA_LAT', N'ThÃ nh phá»‘ ÄÃ  Láº¡t', 1       FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'BAO_LOC', N'ThÃ nh phá»‘ Báº£o Lá»™c', 2  FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DAM_RONG', N'Huyá»‡n Äam RÃ´ng', 3    FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'LAC_DUONG', N'Huyá»‡n Láº¡c DÆ°Æ¡ng', 4  FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DON_DUONG', N'Huyá»‡n ÄÆ¡n DÆ°Æ¡ng', 5  FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DUC_TRONG', N'Huyá»‡n Äá»©c Trá»ng', 6  FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DI_LINH', N'Huyá»‡n Di Linh', 7      FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'BAO_LAM', N'Huyá»‡n Báº£o LÃ¢m', 8      FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DA_HUOAI', N'Huyá»‡n Äáº¡ Huoai', 9    FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'DA_TEH', N'Huyá»‡n Äáº¡ Táº»h', 10       FROM Categories WHERE CategoryCode='QUAN_HUYEN'
UNION ALL SELECT Id, 'CAT_TIEN', N'Huyá»‡n CÃ¡t TiÃªn', 11   FROM Categories WHERE CategoryCode='QUAN_HUYEN';

-- Gender
INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, OrderIndex)
SELECT Id, 'NAM', N'Nam', 1           FROM Categories WHERE CategoryCode='LOAI_GIOI_TINH'
UNION ALL SELECT Id, 'NU', N'Ná»¯', 2  FROM Categories WHERE CategoryCode='LOAI_GIOI_TINH'
UNION ALL SELECT Id, 'KHAC', N'KhÃ¡c', 3 FROM Categories WHERE CategoryCode='LOAI_GIOI_TINH';

-- Member groups seed
INSERT INTO MemberGroups (GroupCode, GroupName, Description) VALUES
('DOAN_CO_SO', N'ÄoÃ n cÆ¡ sá»Ÿ', N'ÄoÃ n cÆ¡ sá»Ÿ táº¡i Ä‘Æ¡n vá»‹'),
('DOAN_PHUONG_XA', N'ÄoÃ n phÆ°á»ng/xÃ£', N'ÄoÃ n phÆ°á»ng xÃ£ Ä‘á»‹a phÆ°Æ¡ng');

-- KTXH Indicator Groups
INSERT INTO KTXHIndicatorGroups (GroupCode, GroupName, OrderIndex) VALUES
('KINH_TE', N'Kinh táº¿', 1),
('XA_HOI', N'XÃ£ há»™i', 2),
('MOI_TRUONG', N'MÃ´i trÆ°á»ng', 3);

-- Sample KTXH Indicators
INSERT INTO KTXHIndicators (Code, Name, GroupId, Unit, Period, IsActive, OrderIndex)
SELECT 'GDP_TANG_TRUONG', N'Tá»‘c Ä‘á»™ tÄƒng trÆ°á»Ÿng GDP', Id, '%', N'NÄƒm', 1, 1 FROM KTXHIndicatorGroups WHERE GroupCode='KINH_TE'
UNION ALL SELECT 'GDPPC', N'GDP bÃ¬nh quÃ¢n Ä‘áº§u ngÆ°á»i', Id, N'Triá»‡u Ä‘á»“ng', N'NÄƒm', 1, 2 FROM KTXHIndicatorGroups WHERE GroupCode='KINH_TE'
UNION ALL SELECT 'XUAT_KHAU', N'Kim ngáº¡ch xuáº¥t kháº©u', Id, N'Triá»‡u USD', N'NÄƒm', 1, 3 FROM KTXHIndicatorGroups WHERE GroupCode='KINH_TE'
UNION ALL SELECT 'DAN_SO', N'DÃ¢n sá»‘', Id, N'NgÆ°á»i', N'NÄƒm', 1, 1 FROM KTXHIndicatorGroups WHERE GroupCode='XA_HOI'
UNION ALL SELECT 'HO_NGHEO', N'Tá»· lá»‡ há»™ nghÃ¨o', Id, '%', N'NÄƒm', 1, 2 FROM KTXHIndicatorGroups WHERE GroupCode='XA_HOI';

-- Sample projects (Module 1)
INSERT INTO Projects (ProjectCode, ProjectName, InvestorName, ProjectGroup, Field, Location, District, TotalInvestment, FundingSource, StartDate, EndDate, Status) VALUES
(N'DA001', N'ÄÆ°á»ng tá»‰nh 720 - Äoáº¡n ÄÃ  Láº¡t - ÄÆ¡n DÆ°Æ¡ng', N'Sá»Ÿ GTVT LÃ¢m Äá»“ng', 'B', N'Giao thÃ´ng váº­n táº£i', N'ÄÃ  Láº¡t - ÄÆ¡n DÆ°Æ¡ng', N'ÄÆ¡n DÆ°Æ¡ng', 125.500, N'NgÃ¢n sÃ¡ch tá»‰nh', '2023-01-01', '2025-12-31', N'DangTrienKhai'),
(N'DA002', N'Bá»‡nh viá»‡n Ä‘a khoa huyá»‡n Báº£o LÃ¢m', N'UBND huyá»‡n Báº£o LÃ¢m', 'B', N'Y táº¿', N'Huyá»‡n Báº£o LÃ¢m', N'BaoLam', 85.000, N'NgÃ¢n sÃ¡ch tá»‰nh', '2022-06-01', '2024-12-31', N'DangTrienKhai'),
(N'DA003', N'TrÆ°á»ng THPT Láº¡c DÆ°Æ¡ng', N'Sá»Ÿ GD&ÄT LÃ¢m Äá»“ng', 'C', N'GiÃ¡o dá»¥c Ä‘Ã o táº¡o', N'Huyá»‡n Láº¡c DÆ°Æ¡ng', N'LacDuong', 22.000, N'NgÃ¢n sÃ¡ch NS', '2023-09-01', '2025-08-31', N'ChuanBi');

-- Sample map projects
INSERT INTO MapProjects (ProjectCode, ProjectName, InvestorName, Location, District, Latitude, Longitude, Status, Field, TotalCapital, IsPublic) VALUES
(N'MAP001', N'Khu du lá»‹ch sinh thÃ¡i ÄÃ  Láº¡t', N'Cty TNHH Sinh thÃ¡i LÃ¢m Äá»“ng', N'PhÆ°á»ng 3, ÄÃ  Láº¡t', N'ÄÃ  Láº¡t', 11.9404, 108.4580, N'DangThucHien', N'Du lá»‹ch', 250.000, 1),
(N'MAP002', N'Khu cÃ´ng nghiá»‡p PhÃº Há»™i', N'Ban Quáº£n lÃ½ KKT tá»‰nh', N'Huyá»‡n Äá»©c Trá»ng', N'Äá»©c Trá»ng', 11.7200, 108.3500, N'DaHoatDong', N'CÃ´ng nghiá»‡p', 500.000, 1);

PRINT 'Seed data inserted successfully!';
PRINT 'NOTE: Admin account will be created automatically on first application startup.';
PRINT 'Default admin credentials: Username=admin / Password=Admin@2024';
GO

