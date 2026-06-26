-- =========================================================================
-- 1. TABLE CALC MONTHLY PER LINE
-- =========================================================================
CREATE TABLE T_Calc_Monthly_Plan_Line (
    CalcLineID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Periode NVARCHAR(7) NOT NULL,
    TargetDate DATE NOT NULL,
    LineCode NVARCHAR(50) NOT NULL,

    CycleTime DECIMAL(18,2) NOT NULL DEFAULT 0,
    SpecialCycleTime DECIMAL(18,2) NOT NULL DEFAULT 0,
    OEE DECIMAL(5,2) NOT NULL DEFAULT 0,

    MandatoryDay INT NOT NULL DEFAULT 0,
    MandatoryNight INT NOT NULL DEFAULT 0,
	
    PdtOtherDay INT NOT NULL DEFAULT 0,
    PdtOtherNight INT NOT NULL DEFAULT 0,
	
    AmPmDay INT NOT NULL DEFAULT 0,
    AmPmNight INT NOT NULL DEFAULT 0,

	TotalPdt INT NULL,
	
    OvertimeDay INT NOT NULL DEFAULT 0,
    OvertimeNight INT NOT NULL DEFAULT 0,
    OvertimeTotal INT NOT NULL DEFAULT 0,
	
    RecoveryDay INT NOT NULL DEFAULT 0,
    RecoveryNight INT NOT NULL DEFAULT 0,
    RecoveryTotal INT NOT NULL DEFAULT 0,
	
    NormalWorkingDay INT NOT NULL DEFAULT 0,
    NormalWorkingNight INT NOT NULL DEFAULT 0,
    NormalWorkingTotal INT NOT NULL DEFAULT 0,
    OperatingTime INT NOT NULL DEFAULT 0,
    
    OutputProduksiDay INT NOT NULL DEFAULT 0,
    OutputProduksiNight INT NOT NULL DEFAULT 0,
    OutputProduksiTotal INT NOT NULL DEFAULT 0,
    TotalEndStock INT NOT NULL DEFAULT 0,

	RevisionNo INT NOT NULL DEFAULT 0,

    CreatedBy NVARCHAR(100) DEFAULT 'System',
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE UNIQUE INDEX IX_T_Calc_Monthly_Plan_Line_Uqk 
ON T_Calc_Monthly_Plan_Line (Periode, TargetDate, LineCode, RevisionNo);
GO


-- =========================================================================
-- 2. TABLE CALC MONTHLY PER UNIQUE
-- =========================================================================
CREATE TABLE T_Calc_Monthly_Plan_Unique (
    CalcUniqueID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Periode NVARCHAR(7) NOT NULL,
    TargetDate DATE NOT NULL,
    LineCode NVARCHAR(50) NOT NULL,
    UniqueCode NVARCHAR(50) NOT NULL,

    MinStock INT NOT NULL DEFAULT 0,
    MaxStock INT NOT NULL DEFAULT 0,
    
    BeginStock INT NOT NULL DEFAULT 0,
    OrderQty INT NOT NULL DEFAULT 0,
	
	SystemPlanQty INT NOT NULL DEFAULT 0,
    ManualPlanQty INT NOT NULL DEFAULT 0,
    FinalPlanQty INT NOT NULL DEFAULT 0,
    IsManual BIT NOT NULL DEFAULT 0,
	
    EndStock INT NOT NULL DEFAULT 0,

	RevisionNo INT NOT NULL DEFAULT 0,
	
    CreatedBy NVARCHAR(100) DEFAULT 'System',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME NULL
);

CREATE NONCLUSTERED INDEX IX_T_Monthly_Plan_Unique_Opt 
ON T_Calc_Monthly_Plan_Unique (Periode, TargetDate, UniqueCode);
GO