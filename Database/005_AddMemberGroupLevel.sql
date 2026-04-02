-- ============================================================
-- 005_AddMemberGroupLevel.sql
-- Thêm cột Level (Cấp bậc) vào bảng MemberGroups
-- ============================================================
USE [QuanLyDoanVien];
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'MemberGroups' AND COLUMN_NAME = 'Level'
)
BEGIN
    ALTER TABLE [dbo].[MemberGroups]
    ADD [Level] NVARCHAR(100) NULL;
    PRINT N'Đã thêm cột Level vào MemberGroups.';
END
ELSE
BEGIN
    PRINT N'Cột Level đã tồn tại trong MemberGroups.';
END
GO
