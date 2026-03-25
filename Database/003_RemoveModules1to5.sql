-- Cáº£nh bÃ¡o: Cháº¡y script nÃ y sáº½ xÃ³a hoÃ n toÃ n cÃ¡c báº£ng vÃ  dá»¯ liá»‡u cá»§a Module 1-5 khá»i database QuanLyDoanVien
USE QuanLyDoanVien;
GO

PRINT 'Báº¯t Ä‘áº§u quÃ¡ trÃ¬nh xÃ³a Dá»¯ liá»‡u vÃ  Báº£ng Module 1-5...';

-- 1. XÃ³a cÃ¡c quyá»n (Permissions) vÃ  MenuItems liÃªn quan Ä‘áº¿n Module 1-5
PRINT 'XÃ³a Permissions vÃ  MenuItems...';

-- MenuItems cÃ³ Module lÃ  M1, M2, M3, M4, M5
DELETE FROM MenuItems WHERE Module IN ('M1', 'M2', 'M3', 'M4', 'M5');

-- Permissions cÃ³ Module lÃ  MODULE1, MODULE2, MODULE3, MODULE4, MODULE5
DELETE FROM Permissions WHERE Module IN ('MODULE1', 'MODULE2', 'MODULE3', 'MODULE4', 'MODULE5');
-- XÃ³a luÃ´n cáº£ RolePermissions vÃ  UserPermissions liÃªn káº¿t thÃ´ng qua CASCADE delete cá»§a DB.
-- Náº¿u DB ko cÃ³ cascade trÃªn PermissionsId thÃ¬ cáº§n thÃªm 2 lá»‡nh nÃ y trÆ°á»›c:
-- DELETE FROM RolePermissions WHERE PermissionId IN (SELECT Id FROM Permissions WHERE Module LIKE 'M%');
-- DELETE FROM UserPermissions WHERE PermissionId IN (SELECT Id FROM Permissions WHERE Module LIKE 'M%');

-- 2. XÃ³a cÃ¡c báº£ng cá»§a Module 1-5 (pháº£i xÃ³a báº£ng con trÆ°á»›c, báº£ng cha sau Ä‘á»ƒ khÃ´ng lá»—i khÃ³a ngoáº¡i)
PRINT 'XÃ³a cÃ¡c báº£ng Module 4...';
IF OBJECT_ID('dbo.MapProjects', 'U') IS NOT NULL DROP TABLE dbo.MapProjects;

PRINT 'XÃ³a cÃ¡c báº£ng Module 5...';
IF OBJECT_ID('dbo.KTXHAlerts', 'U') IS NOT NULL DROP TABLE dbo.KTXHAlerts;
IF OBJECT_ID('dbo.KTXHReports', 'U') IS NOT NULL DROP TABLE dbo.KTXHReports;
IF OBJECT_ID('dbo.KTXHIndicators', 'U') IS NOT NULL DROP TABLE dbo.KTXHIndicators;
IF OBJECT_ID('dbo.KTXHIndicatorGroups', 'U') IS NOT NULL DROP TABLE dbo.KTXHIndicatorGroups;

PRINT 'XÃ³a cÃ¡c báº£ng Module 3...';
IF OBJECT_ID('dbo.BudgetExpenditures', 'U') IS NOT NULL DROP TABLE dbo.BudgetExpenditures;
IF OBJECT_ID('dbo.BudgetRevenues', 'U') IS NOT NULL DROP TABLE dbo.BudgetRevenues;
IF OBJECT_ID('dbo.BudgetPlans', 'U') IS NOT NULL DROP TABLE dbo.BudgetPlans;
IF OBJECT_ID('dbo.BudgetCategories', 'U') IS NOT NULL DROP TABLE dbo.BudgetCategories;
-- BudgetUnits (náº¿u cÃ³ dÃ¹ng chung thÃ¬ cÃ¢n nháº¯c, nhÆ°ng theo plan lÃ  xÃ³a m3)
IF OBJECT_ID('dbo.BudgetUnits', 'U') IS NOT NULL DROP TABLE dbo.BudgetUnits;

PRINT 'XÃ³a cÃ¡c báº£ng Module 2...';
IF OBJECT_ID('dbo.OutsideProjectDocuments', 'U') IS NOT NULL DROP TABLE dbo.OutsideProjectDocuments;
IF OBJECT_ID('dbo.OutsideProjectProgress', 'U') IS NOT NULL DROP TABLE dbo.OutsideProjectProgress;
IF OBJECT_ID('dbo.OutsideProjects', 'U') IS NOT NULL DROP TABLE dbo.OutsideProjects;
IF OBJECT_ID('dbo.InvestorCertificates', 'U') IS NOT NULL DROP TABLE dbo.InvestorCertificates;
IF OBJECT_ID('dbo.Investors', 'U') IS NOT NULL DROP TABLE dbo.Investors;

PRINT 'XÃ³a cÃ¡c báº£ng Module 1...';
IF OBJECT_ID('dbo.ProjectDocuments', 'U') IS NOT NULL DROP TABLE dbo.ProjectDocuments;
IF OBJECT_ID('dbo.ProjectIssues', 'U') IS NOT NULL DROP TABLE dbo.ProjectIssues;
IF OBJECT_ID('dbo.ProjectProgress', 'U') IS NOT NULL DROP TABLE dbo.ProjectProgress;
IF OBJECT_ID('dbo.ProjectDisbursements', 'U') IS NOT NULL DROP TABLE dbo.ProjectDisbursements;
IF OBJECT_ID('dbo.ProjectFunds', 'U') IS NOT NULL DROP TABLE dbo.ProjectFunds;
IF OBJECT_ID('dbo.Projects', 'U') IS NOT NULL DROP TABLE dbo.Projects;

PRINT 'HoÃ n thÃ nh xÃ³a Module 1-5.';
GO

