-- Cảnh báo: Chạy script này sẽ xóa hoàn toàn các bảng và dữ liệu của Module 1-5 khỏi database QuanLyDauTu
USE QuanLyDauTu;
GO

PRINT 'Bắt đầu quá trình xóa Dữ liệu và Bảng Module 1-5...';

-- 1. Xóa các quyền (Permissions) và MenuItems liên quan đến Module 1-5
PRINT 'Xóa Permissions và MenuItems...';

-- MenuItems có Module là M1, M2, M3, M4, M5
DELETE FROM MenuItems WHERE Module IN ('M1', 'M2', 'M3', 'M4', 'M5');

-- Permissions có Module là MODULE1, MODULE2, MODULE3, MODULE4, MODULE5
DELETE FROM Permissions WHERE Module IN ('MODULE1', 'MODULE2', 'MODULE3', 'MODULE4', 'MODULE5');
-- Xóa luôn cả RolePermissions và UserPermissions liên kết thông qua CASCADE delete của DB.
-- Nếu DB ko có cascade trên PermissionsId thì cần thêm 2 lệnh này trước:
-- DELETE FROM RolePermissions WHERE PermissionId IN (SELECT Id FROM Permissions WHERE Module LIKE 'M%');
-- DELETE FROM UserPermissions WHERE PermissionId IN (SELECT Id FROM Permissions WHERE Module LIKE 'M%');

-- 2. Xóa các bảng của Module 1-5 (phải xóa bảng con trước, bảng cha sau để không lỗi khóa ngoại)
PRINT 'Xóa các bảng Module 4...';
IF OBJECT_ID('dbo.MapProjects', 'U') IS NOT NULL DROP TABLE dbo.MapProjects;

PRINT 'Xóa các bảng Module 5...';
IF OBJECT_ID('dbo.KTXHAlerts', 'U') IS NOT NULL DROP TABLE dbo.KTXHAlerts;
IF OBJECT_ID('dbo.KTXHReports', 'U') IS NOT NULL DROP TABLE dbo.KTXHReports;
IF OBJECT_ID('dbo.KTXHIndicators', 'U') IS NOT NULL DROP TABLE dbo.KTXHIndicators;
IF OBJECT_ID('dbo.KTXHIndicatorGroups', 'U') IS NOT NULL DROP TABLE dbo.KTXHIndicatorGroups;

PRINT 'Xóa các bảng Module 3...';
IF OBJECT_ID('dbo.BudgetExpenditures', 'U') IS NOT NULL DROP TABLE dbo.BudgetExpenditures;
IF OBJECT_ID('dbo.BudgetRevenues', 'U') IS NOT NULL DROP TABLE dbo.BudgetRevenues;
IF OBJECT_ID('dbo.BudgetPlans', 'U') IS NOT NULL DROP TABLE dbo.BudgetPlans;
IF OBJECT_ID('dbo.BudgetCategories', 'U') IS NOT NULL DROP TABLE dbo.BudgetCategories;
-- BudgetUnits (nếu có dùng chung thì cân nhắc, nhưng theo plan là xóa m3)
IF OBJECT_ID('dbo.BudgetUnits', 'U') IS NOT NULL DROP TABLE dbo.BudgetUnits;

PRINT 'Xóa các bảng Module 2...';
IF OBJECT_ID('dbo.OutsideProjectDocuments', 'U') IS NOT NULL DROP TABLE dbo.OutsideProjectDocuments;
IF OBJECT_ID('dbo.OutsideProjectProgress', 'U') IS NOT NULL DROP TABLE dbo.OutsideProjectProgress;
IF OBJECT_ID('dbo.OutsideProjects', 'U') IS NOT NULL DROP TABLE dbo.OutsideProjects;
IF OBJECT_ID('dbo.InvestorCertificates', 'U') IS NOT NULL DROP TABLE dbo.InvestorCertificates;
IF OBJECT_ID('dbo.Investors', 'U') IS NOT NULL DROP TABLE dbo.Investors;

PRINT 'Xóa các bảng Module 1...';
IF OBJECT_ID('dbo.ProjectDocuments', 'U') IS NOT NULL DROP TABLE dbo.ProjectDocuments;
IF OBJECT_ID('dbo.ProjectIssues', 'U') IS NOT NULL DROP TABLE dbo.ProjectIssues;
IF OBJECT_ID('dbo.ProjectProgress', 'U') IS NOT NULL DROP TABLE dbo.ProjectProgress;
IF OBJECT_ID('dbo.ProjectDisbursements', 'U') IS NOT NULL DROP TABLE dbo.ProjectDisbursements;
IF OBJECT_ID('dbo.ProjectFunds', 'U') IS NOT NULL DROP TABLE dbo.ProjectFunds;
IF OBJECT_ID('dbo.Projects', 'U') IS NOT NULL DROP TABLE dbo.Projects;

PRINT 'Hoàn thành xóa Module 1-5.';
GO
