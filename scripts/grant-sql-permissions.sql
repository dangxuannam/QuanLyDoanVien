-- =============================================================================
-- grant-sql-permissions.sql
-- Cấp quyền cho IIS Application Pool Identity vào SQL Server
-- Chạy trong SQL Server Management Studio (SSMS) với quyền sa hoặc sysadmin
--
-- Kịch bản: AppPool dùng ApplicationPoolIdentity → tài khoản Windows là
--           "IIS APPPOOL\QuanLyDoanVienPool"
-- =============================================================================

USE [master]
GO

-- ─── Bước 1: Tạo Login ở cấp Server ─────────────────────────────────────────
-- Kiểm tra xem Login đã tồn tại chưa trước khi tạo
IF NOT EXISTS (
    SELECT name FROM sys.server_principals
    WHERE name = N'IIS APPPOOL\QuanLyDoanVienPool'
)
BEGIN
    CREATE LOGIN [IIS APPPOOL\QuanLyDoanVienPool] FROM WINDOWS
        WITH DEFAULT_DATABASE = [QuanLyDoanVien];
    PRINT 'Đã tạo Login: IIS APPPOOL\QuanLyDoanVienPool';
END
ELSE
BEGIN
    PRINT 'Login đã tồn tại: IIS APPPOOL\QuanLyDoanVienPool';
END
GO

-- ─── Bước 2: Cấp quyền ở cấp Database ───────────────────────────────────────
USE [QuanLyDoanVien]
GO

-- Tạo User trong database nếu chưa có
IF NOT EXISTS (
    SELECT name FROM sys.database_principals
    WHERE name = N'IIS APPPOOL\QuanLyDoanVienPool'
)
BEGIN
    CREATE USER [IIS APPPOOL\QuanLyDoanVienPool]
        FOR LOGIN [IIS APPPOOL\QuanLyDoanVienPool];
    PRINT 'Đã tạo User trong database QuanLyDoanVien';
END
ELSE
BEGIN
    PRINT 'User đã tồn tại trong database';
END
GO

-- Gán role đọc dữ liệu
ALTER ROLE [db_datareader] ADD MEMBER [IIS APPPOOL\QuanLyDoanVienPool];
GO

-- Gán role ghi dữ liệu
ALTER ROLE [db_datawriter] ADD MEMBER [IIS APPPOOL\QuanLyDoanVienPool];
GO

-- Cấp quyền thực thi Stored Procedure (nếu có)
GRANT EXECUTE TO [IIS APPPOOL\QuanLyDoanVienPool];
GO

-- ─── Bước 3: Cập nhật Connection String ──────────────────────────────────────
-- Sau khi cấp quyền xong, cập nhật Web.config để dùng Integrated Security:
--
-- <add name="QuanLyDoanVienContext"
--      connectionString="Data Source=.\SQLEXPRESS;
--                        Initial Catalog=QuanLyDoanVien;
--                        Integrated Security=True;
--                        MultipleActiveResultSets=True;
--                        Connect Timeout=30"
--      providerName="System.Data.SqlClient" />
--
-- Thay ".\SQLEXPRESS" bằng tên SQL Server thực của bạn
-- (dùng SQL Server Configuration Manager để xem tên instance)

PRINT '============================================';
PRINT 'Hoàn tất cấp quyền cho IIS AppPool Identity';
PRINT 'Kiểm tra: SELECT * FROM sys.database_role_members';
PRINT '============================================';
GO

-- Xem kết quả: User có quyền gì
SELECT
    dp.name AS UserName,
    r.name  AS RoleName
FROM sys.database_role_members drm
JOIN sys.database_principals dp ON drm.member_principal_id = dp.principal_id
JOIN sys.database_principals r  ON drm.role_principal_id   = r.principal_id
WHERE dp.name = 'IIS APPPOOL\QuanLyDoanVienPool';
GO
