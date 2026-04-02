-- ============================================================
-- 004_AddUnitsTable.sql
-- Tạo bảng Units và thêm menu Quản lý đơn vị vào sidebar
-- ============================================================

-- Tạo bảng Units
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Units')
BEGIN
    CREATE TABLE [dbo].[Units] (
        [Id]               INT            IDENTITY(1,1) PRIMARY KEY,
        [UnitCode]         NVARCHAR(50)   NOT NULL,
        [UnitName]         NVARCHAR(300)  NOT NULL,
        [Description]      NVARCHAR(500)  NULL,
        [IsActive]         BIT            NOT NULL DEFAULT 1,
        [CreatedBy]        INT            NULL,
        [CreatedAt]        DATETIME       NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]        DATETIME       NULL,

        -- Dữ liệu tổng hợp (JSON) sau khi import Excel
        [SummaryJson]      NVARCHAR(MAX)  NULL,

        -- Metadata lần import cuối
        [LastImportFileId] INT            NULL,
        [LastImportAt]     DATETIME       NULL,
        [LastImportBy]     INT            NULL,
        [TotalMembers]     INT            NULL DEFAULT 0,

        CONSTRAINT [UQ_Units_UnitCode] UNIQUE ([UnitCode])
    );
    PRINT N'Đã tạo bảng Units.';
END
ELSE
    PRINT N'Bảng Units đã tồn tại.';
GO

-- ============================================================
-- Thêm menu "Quản lý đơn vị" vào sidebar (MenuItems)
-- ============================================================

-- Menu cha: "Quản lý đơn vị"
IF NOT EXISTS (SELECT 1 FROM [dbo].[MenuItems] WHERE [Url] = '/units' OR [MenuName] = N'Quản lý đơn vị')
BEGIN
    INSERT INTO [dbo].[MenuItems]
        ([MenuName], [Url], [Icon], [OrderIndex], [IsActive], [RequiredPermission], [Module])
    VALUES
        (N'Quản lý đơn vị', N'/units', N'business', 30, 1, N'DV_VIEW', N'DON_VI');
    PRINT N'Đã thêm menu "Quản lý đơn vị".';
END
ELSE
    PRINT N'Menu "Quản lý đơn vị" đã tồn tại.';
GO

-- ============================================================
-- Thêm permissions cho module DON_VI (nếu chưa có)
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = 'DV_VIEW')
    INSERT INTO [dbo].[Permissions] ([PermissionCode],[PermissionName],[Module],[IsActive])
    VALUES ('DV_VIEW', N'Xem đoàn viên / đơn vị', 'DON_VI', 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = 'DV_CREATE')
    INSERT INTO [dbo].[Permissions] ([PermissionCode],[PermissionName],[Module],[IsActive])
    VALUES ('DV_CREATE', N'Thêm / Import đoàn viên / đơn vị', 'DON_VI', 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = 'DV_EDIT')
    INSERT INTO [dbo].[Permissions] ([PermissionCode],[PermissionName],[Module],[IsActive])
    VALUES ('DV_EDIT', N'Sửa đoàn viên / đơn vị', 'DON_VI', 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = 'DV_DELETE')
    INSERT INTO [dbo].[Permissions] ([PermissionCode],[PermissionName],[Module],[IsActive])
    VALUES ('DV_DELETE', N'Xóa đoàn viên / đơn vị', 'DON_VI', 1);
GO

-- ============================================================
-- Gán quyền DV_VIEW + DV_CREATE + DV_EDIT + DV_DELETE vào tất cả Role hiện có
-- (Admin đã tự động bypass, nên chỉ cần gán cho role thường)
-- ============================================================
DECLARE @dvView   INT = (SELECT Id FROM Permissions WHERE PermissionCode = 'DV_VIEW')
DECLARE @dvCreate INT = (SELECT Id FROM Permissions WHERE PermissionCode = 'DV_CREATE')
DECLARE @dvEdit   INT = (SELECT Id FROM Permissions WHERE PermissionCode = 'DV_EDIT')
DECLARE @dvDelete INT = (SELECT Id FROM Permissions WHERE PermissionCode = 'DV_DELETE')

INSERT INTO [dbo].[RolePermissions] (RoleId, PermissionId)
SELECT r.Id, p.PermissionId
FROM [dbo].[Roles] r
CROSS JOIN (SELECT @dvView AS PermissionId UNION ALL
            SELECT @dvCreate              UNION ALL
            SELECT @dvEdit                UNION ALL
            SELECT @dvDelete) p
WHERE r.IsActive = 1
  AND NOT EXISTS (
    SELECT 1 FROM [dbo].[RolePermissions] rp
    WHERE rp.RoleId = r.Id AND rp.PermissionId = p.PermissionId
  );

PRINT N'Đã gán quyền DON_VI cho tất cả role.';
GO

PRINT N'Hoàn tất script 004_AddUnitsTable.sql';
GO
