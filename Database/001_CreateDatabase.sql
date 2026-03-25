-- =============================================
-- HE THONG QUAN LY DAU TU TINH LAM DONG
-- Database Schema - SQL Server 2019/2022
-- Windows Authentication | DB: QuanLyDoanVien
-- =============================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyDoanVien')
BEGIN
    ALTER DATABASE QuanLyDoanVien SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyDoanVien;
END
GO

CREATE DATABASE QuanLyDoanVien
    COLLATE Vietnamese_CI_AS;
GO

USE QuanLyDoanVien;
GO

-- =============================================
-- SHARED / COMMON TABLES
-- =============================================

CREATE TABLE Roles (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    RoleCode    NVARCHAR(50)  NOT NULL UNIQUE,
    RoleName    NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Users (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(256) NOT NULL,
    PasswordSalt    NVARCHAR(100) NOT NULL,
    FullName        NVARCHAR(200) NOT NULL,
    Email           NVARCHAR(200),
    Phone           NVARCHAR(20),
    DonVi           NVARCHAR(300),
    IsActive        BIT NOT NULL DEFAULT 1,
    IsAdmin         BIT NOT NULL DEFAULT 0,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME,
    LastLoginAt     DATETIME,
    CreatedBy       INT
);

CREATE TABLE UserRoles (
    UserId  INT NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    RoleId  INT NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    PRIMARY KEY (UserId, RoleId)
);

CREATE TABLE UserTokens (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Token       NVARCHAR(256) NOT NULL UNIQUE,
    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE(),
    ExpiresAt   DATETIME NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1
);

CREATE TABLE Permissions (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PermissionCode  NVARCHAR(100) NOT NULL UNIQUE,
    PermissionName  NVARCHAR(200) NOT NULL,
    Module          NVARCHAR(100),
    IsActive        BIT NOT NULL DEFAULT 1
);

CREATE TABLE RolePermissions (
    RoleId          INT NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    PermissionId    INT NOT NULL REFERENCES Permissions(Id) ON DELETE CASCADE,
    PRIMARY KEY (RoleId, PermissionId)
);

CREATE TABLE UserPermissions (
    UserId          INT NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    PermissionId    INT NOT NULL REFERENCES Permissions(Id) ON DELETE CASCADE,
    IsGranted       BIT NOT NULL DEFAULT 1,
    PRIMARY KEY (UserId, PermissionId)
);

CREATE TABLE MenuItems (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    ParentId            INT REFERENCES MenuItems(Id),
    MenuName            NVARCHAR(200) NOT NULL,
    Url                 NVARCHAR(500),
    Icon                NVARCHAR(100),
    OrderIndex          INT NOT NULL DEFAULT 0,
    IsActive            BIT NOT NULL DEFAULT 1,
    RequiredPermission  NVARCHAR(100),
    Module              NVARCHAR(100)
);

CREATE TABLE AuditLogs (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT REFERENCES Users(Id),
    Username    NVARCHAR(50),
    Action      NVARCHAR(100) NOT NULL,
    Module      NVARCHAR(100),
    Description NVARCHAR(1000),
    IpAddress   NVARCHAR(50),
    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Categories (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    CategoryCode    NVARCHAR(50) NOT NULL UNIQUE,
    CategoryName    NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(500),
    IsActive        BIT NOT NULL DEFAULT 1
);

CREATE TABLE CategoryItems (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId  INT NOT NULL REFERENCES Categories(Id) ON DELETE CASCADE,
    ItemCode    NVARCHAR(50) NOT NULL,
    ItemName    NVARCHAR(300) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    OrderIndex  INT NOT NULL DEFAULT 0,
    UNIQUE (CategoryId, ItemCode)
);

CREATE TABLE FileAttachments (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    FileName        NVARCHAR(500) NOT NULL,
    OriginalName    NVARCHAR(500) NOT NULL,
    FilePath        NVARCHAR(1000) NOT NULL,
    FileSize        BIGINT,
    ContentType     NVARCHAR(200),
    Module          NVARCHAR(100),
    RelatedId       INT,
    Description     NVARCHAR(500),
    SheetCount      INT,
    SheetNames      NVARCHAR(MAX),  -- JSON array of sheet names
    UploadedBy      INT REFERENCES Users(Id),
    UploadedAt      DATETIME NOT NULL DEFAULT GETDATE()
);

-- Excel import data storage
CREATE TABLE ExcelImportLogs (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    FileAttachmentId INT NOT NULL REFERENCES FileAttachments(Id),
    SheetName       NVARCHAR(200),
    TotalRows       INT,
    ImportedRows    INT,
    FailedRows      INT,
    Status          NVARCHAR(50),  -- Pending, Success, Failed, PartialSuccess
    ErrorLog        NVARCHAR(MAX),
    ImportedBy      INT REFERENCES Users(Id),
    ImportedAt      DATETIME NOT NULL DEFAULT GETDATE()
);

-- =============================================
-- DOAN VIEN (Member Management)
-- =============================================

CREATE TABLE MemberGroups (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    GroupCode   NVARCHAR(50) NOT NULL UNIQUE,
    GroupName   NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Members (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MemberCode      NVARCHAR(50) NOT NULL UNIQUE,
    FullName        NVARCHAR(200) NOT NULL,
    DateOfBirth     DATE,
    Gender          NVARCHAR(10),   -- Nam/Nu/Khac
    Phone           NVARCHAR(20),
    Email           NVARCHAR(200),
    Address         NVARCHAR(500),
    JoinDate        DATE,
    GroupId         INT REFERENCES MemberGroups(Id),
    Position        NVARCHAR(200),
    CardNumber      NVARCHAR(50),
    IsActive        BIT NOT NULL DEFAULT 1,
    Notes           NVARCHAR(1000),
    CreatedBy       INT REFERENCES Users(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME
);

-- =============================================
-- MODULE 1: DU AN DAU TU CONG
-- =============================================

CREATE TABLE Projects (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ProjectCode     NVARCHAR(50) NOT NULL UNIQUE,
    ProjectName     NVARCHAR(500) NOT NULL,
    InvestorName    NVARCHAR(300),
    ProjectGroup    NVARCHAR(10),   -- A, B, C
    Field           NVARCHAR(200),  -- Linh vuc
    Location        NVARCHAR(500),  -- Dia diem
    District        NVARCHAR(200),
    TotalInvestment DECIMAL(18,3),  -- Tong muc dau tu (ty dong)
    FundingSource   NVARCHAR(500),  -- Nguon von
    StartDate       DATE,
    EndDate         DATE,
    Status          NVARCHAR(50),   -- ChuanBi/DangTrienKhai/HoanThanh/TamDung
    Description     NVARCHAR(MAX),
    DecisionNo      NVARCHAR(200),  -- So quyet dinh phe duyet
    DecisionDate    DATE,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedBy       INT REFERENCES Users(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME
);

CREATE TABLE ProjectFunds (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId       INT NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    Year            INT NOT NULL,
    PlanFund        DECIMAL(18,3),  -- Ke hoach von (ty dong)
    AdjustedFund    DECIMAL(18,3),  -- Dieu chinh
    Notes           NVARCHAR(500),
    CreatedBy       INT REFERENCES Users(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE ProjectDisbursements (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId   INT NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    Year        INT NOT NULL,
    Month       INT,
    Quarter     INT,
    Amount      DECIMAL(18,3) NOT NULL,
    Notes       NVARCHAR(500),
    CreatedBy   INT REFERENCES Users(Id),
    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE ProjectProgress (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId       INT NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    ReportDate      DATE NOT NULL,
    CompletionRate  DECIMAL(5,2),   -- %
    Notes           NVARCHAR(1000),
    CreatedBy       INT REFERENCES Users(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE ProjectIssues (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId       INT NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    IssueType       NVARCHAR(100),  -- VuongMac/KienNghi
    Description     NVARCHAR(MAX),
    Status          NVARCHAR(50),   -- DangXuLy/DaXuLy
    Resolution      NVARCHAR(MAX),
    CreatedBy       INT REFERENCES Users(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME
);

CREATE TABLE ProjectDocuments (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId       INT NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    FileAttachmentId INT REFERENCES FileAttachments(Id),
    DocumentType    NVARCHAR(100),
    Description     NVARCHAR(500),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

-- =============================================
-- MODULE 2: DU AN DAU TU NGOAI NGAN SACH
-- =============================================

CREATE TABLE Investors (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    InvestorCode    NVARCHAR(50) NOT NULL UNIQUE,
    InvestorName    NVARCHAR(300) NOT NULL,
    InvestorType    NVARCHAR(100),   -- TrongNuoc/NuocNgoai
    Representative  NVARCHAR(200),
    Phone           NVARCHAR(20),
    Email           NVARCHAR(200),
    Address         NVARCHAR(500),
    TaxCode         NVARCHAR(50),
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE InvestorCertificates (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    InvestorId      INT NOT NULL REFERENCES Investors(Id) ON DELETE CASCADE,
    CertificateNo   NVARCHAR(100),
    IssueDate       DATE,
    Notes           NVARCHAR(500)
);

CREATE TABLE OutsideProjects (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ProjectCode     NVARCHAR(50) NOT NULL UNIQUE,
    ProjectName     NVARCHAR(500) NOT NULL,
    InvestorId      INT REFERENCES Investors(Id),
    InvestorName    NVARCHAR(300),
    Status          NVARCHAR(50),   -- ChoXetDuyet/DaPheDuyet/DangThucHien/HoanThanh
    Field           NVARCHAR(200),
    Location        NVARCHAR(500),
    District        NVARCHAR(200),
    LandArea        DECIMAL(18,4),  -- Dien tich dat (ha)
    TotalCapital    DECIMAL(18,3),  -- Tong von (ty dong)
    StartDate       DATE,
    EndDate         DATE,
    IsPublic        BIT NOT NULL DEFAULT 0,
    Description     NVARCHAR(MAX),
    CreatedBy       INT REFERENCES Users(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME
);

CREATE TABLE OutsideProjectProgress (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId   INT NOT NULL REFERENCES OutsideProjects(Id) ON DELETE CASCADE,
    ReportDate  DATE NOT NULL,
    Revenue     DECIMAL(18,3),
    Tax         DECIMAL(18,3),
    Workers     INT,
    Notes       NVARCHAR(1000),
    CreatedBy   INT REFERENCES Users(Id),
    CreatedAt   DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE OutsideProjectDocuments (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId       INT NOT NULL REFERENCES OutsideProjects(Id) ON DELETE CASCADE,
    FileAttachmentId INT REFERENCES FileAttachments(Id),
    DocumentType    NVARCHAR(100),
    Description     NVARCHAR(500),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

-- =============================================
-- MODULE 3: QUYET TOAN NGAN SACH NHA NUOC
-- =============================================

CREATE TABLE BudgetCategories (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Code        NVARCHAR(50) NOT NULL UNIQUE,
    Name        NVARCHAR(300) NOT NULL,
    Type        NVARCHAR(20),       -- Thu/Chi/Chuong/Loai/Khoan/Muc/TieuMuc
    ParentId    INT REFERENCES BudgetCategories(Id),
    Level       INT NOT NULL DEFAULT 1,
    IsActive    BIT NOT NULL DEFAULT 1,
    OrderIndex  INT NOT NULL DEFAULT 0
);

CREATE TABLE BudgetUnits (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UnitCode    NVARCHAR(50) NOT NULL UNIQUE,
    UnitName    NVARCHAR(300) NOT NULL,
    ParentId    INT REFERENCES BudgetUnits(Id),
    Level       INT NOT NULL DEFAULT 1,
    IsActive    BIT NOT NULL DEFAULT 1
);

CREATE TABLE BudgetPlans (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Year            INT NOT NULL,
    BudgetCategoryId INT NOT NULL REFERENCES BudgetCategories(Id),
    UnitId          INT REFERENCES BudgetUnits(Id),
    PlanAmount      DECIMAL(18,3) NOT NULL DEFAULT 0,
    AdjustedAmount  DECIMAL(18,3),
    Notes           NVARCHAR(500),
    CreatedBy       INT REFERENCES Users(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE BudgetRevenues (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Year            INT NOT NULL,
    Month           INT,
    Quarter         INT,
    BudgetCategoryId INT NOT NULL REFERENCES BudgetCategories(Id),
    UnitId          INT REFERENCES BudgetUnits(Id),
    ActualAmount    DECIMAL(18,3) NOT NULL DEFAULT 0,
    Notes           NVARCHAR(500),
    CreatedBy       INT REFERENCES Users(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE BudgetExpenditures (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Year            INT NOT NULL,
    Month           INT,
    Quarter         INT,
    BudgetCategoryId INT NOT NULL REFERENCES BudgetCategories(Id),
    UnitId          INT REFERENCES BudgetUnits(Id),
    ActualAmount    DECIMAL(18,3) NOT NULL DEFAULT 0,
    Notes           NVARCHAR(500),
    CreatedBy       INT REFERENCES Users(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

-- =============================================
-- MODULE 4: BAN DO XUC TIEN DAU TU
-- =============================================

CREATE TABLE MapProjects (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ProjectCode     NVARCHAR(50),
    ProjectName     NVARCHAR(500) NOT NULL,
    InvestorName    NVARCHAR(300),
    Location        NVARCHAR(500),
    District        NVARCHAR(200),
    Latitude        DECIMAL(10,7),
    Longitude       DECIMAL(10,7),
    Status          NVARCHAR(50),
    Field           NVARCHAR(200),
    TotalCapital    DECIMAL(18,3),
    IsPublic        BIT NOT NULL DEFAULT 1,
    Description     NVARCHAR(MAX),
    OutsideProjectId INT REFERENCES OutsideProjects(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME
);

-- =============================================
-- MODULE 5: CHI TIEU KINH TE XA HOI
-- =============================================

CREATE TABLE KTXHIndicatorGroups (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    GroupCode   NVARCHAR(50) NOT NULL UNIQUE,
    GroupName   NVARCHAR(200) NOT NULL,
    IsActive    BIT NOT NULL DEFAULT 1,
    OrderIndex  INT NOT NULL DEFAULT 0
);

CREATE TABLE KTXHIndicators (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Code        NVARCHAR(50) NOT NULL UNIQUE,
    Name        NVARCHAR(300) NOT NULL,
    GroupId     INT REFERENCES KTXHIndicatorGroups(Id),
    Unit        NVARCHAR(100),      -- Don vi tinh
    Period      NVARCHAR(50),       -- Thang/Quy/Nam
    Formula     NVARCHAR(500),
    Source      NVARCHAR(300),
    IsActive    BIT NOT NULL DEFAULT 1,
    OrderIndex  INT NOT NULL DEFAULT 0
);

CREATE TABLE KTXHReports (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    IndicatorId     INT NOT NULL REFERENCES KTXHIndicators(Id),
    Year            INT NOT NULL,
    Month           INT,
    Quarter         INT,
    Value           DECIMAL(18,4),
    UnitId          INT REFERENCES BudgetUnits(Id),
    Notes           NVARCHAR(500),
    CreatedBy       INT REFERENCES Users(Id),
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE KTXHAlerts (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    IndicatorId     INT NOT NULL REFERENCES KTXHIndicators(Id),
    AlertType       NVARCHAR(50),   -- TangCao/GiamThap
    Threshold       DECIMAL(18,4),  -- %
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME NOT NULL DEFAULT GETDATE()
);

-- =============================================
-- INDEXES
-- =============================================

CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_UserTokens_Token ON UserTokens(Token);
CREATE INDEX IX_AuditLogs_UserId_CreatedAt ON AuditLogs(UserId, CreatedAt);
CREATE INDEX IX_Projects_Status ON Projects(Status);
CREATE INDEX IX_Projects_ProjectCode ON Projects(ProjectCode);
CREATE INDEX IX_OutsideProjects_Status ON OutsideProjects(Status);
CREATE INDEX IX_OutsideProjects_ProjectCode ON OutsideProjects(ProjectCode);
CREATE INDEX IX_Members_MemberCode ON Members(MemberCode);
CREATE INDEX IX_Members_GroupId ON Members(GroupId);
CREATE INDEX IX_FileAttachments_Module ON FileAttachments(Module);
CREATE INDEX IX_MapProjects_IsPublic ON MapProjects(IsPublic);

PRINT 'Database schema created successfully!';
GO

