CREATE TABLE M_Special_CT (
    SpecialCtID INT IDENTITY(1,1) PRIMARY KEY,
    LineOrderCode NVARCHAR(50) NOT NULL,
    TargetDate DATE NOT NULL,
    SpecialCycleTime DECIMAL(18,2) NULL,
    Remarks NVARCHAR(255) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME NULL
);

CREATE NONCLUSTERED INDEX IX_M_Special_CT_Lookup 
ON M_Special_CT (LineOrderCode, TargetDate);

CREATE TABLE M_Recovery_Plan (
    RecoveryID INT IDENTITY(1,1) PRIMARY KEY,
    LineOrderCode NVARCHAR(50) NOT NULL,
    TargetDate DATE NOT NULL,
    Shift NVARCHAR(10) NOT NULL, 
    RecoveryTime DECIMAL(18,2) NOT NULL DEFAULT 0,
    Remarks NVARCHAR(255) NULL,
    CreatedBy NVARCHAR(100) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME NULL
);

CREATE NONCLUSTERED INDEX IX_M_Recovery_Plan_Lookup 
ON M_Recovery_Plan (LineOrderCode, TargetDate, Shift);