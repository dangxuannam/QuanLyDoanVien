USE QuanLyDoanVien;
GO

-- 1. Thêm cột UnitId vào bảng Members
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Members' AND COLUMN_NAME = 'UnitId'
)
BEGIN
    ALTER TABLE Members 
    ADD UnitId INT NULL;
    
    -- Tạo khóa ngoại
    ALTER TABLE Members
    ADD CONSTRAINT FK_Members_Units
    FOREIGN KEY (UnitId) REFERENCES Units(Id);
    
    PRINT 'Đã thêm cột UnitId vào bảng Members.';
END
ELSE
BEGIN
    PRINT 'Cột UnitId đã tồn tại trong bảng Members.';
END
GO
