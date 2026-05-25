CREATE TABLE [dbo].[T_Calc_Capacity] (

    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [CalcCapacityCode] VARCHAR(50) NOT NULL,
    [LineOrderCode] VARCHAR(50) NOT NULL,
    [Suffix] VARCHAR(50),
    [Periode] VARCHAR(7) NOT NULL,
    [MonthOffsetLabel] VARCHAR(10),

    [AdvanceQty] INT DEFAULT 0,
    [MandatoryTime] INT NOT NULL,
    [HotShiftOvertime] INT DEFAULT 0,

    [ShiftCount] INT DEFAULT 2,
    [LoadingCustomerQty] INT DEFAULT 0,
    [WorkingDay] INT,
    [WorkingTimePerShift] INT,
    [CT_Min] DECIMAL(18,2),
    [EFF_Pct] DECIMAL(18,2),

    [ProdPlanMonthly] INT,
    [ProdPlanDaily] INT,
    [ProdPlanTaktTime] DECIMAL(10,1),

    [LoadingTime] INT,
    [TotalOperatingTime] INT,
    [CapacityNormal] INT,
    [CapacityOT_2h] INT,
    [CapacityHOT_4s] INT,
    [CapacityTotal] INT,

    [Overtime_DOT_Hours] DECIMAL(10,2),
    [Overtime_Pct] DECIMAL(10,4),
    [IdlePerDay_Min] DECIMAL(10,2),

    [CreatedUser] VARCHAR(50),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedUser] VARCHAR(50),
    [UpdatedAt] DATETIME
);

CREATE NONCLUSTERED INDEX IX_CalcCapacity_Periode_Line ON [dbo].[T_Calc_Capacity] ([Periode], [LineOrderCode]);
CREATE NONCLUSTERED INDEX IX_CalcCapacity_Code ON [dbo].[T_Calc_Capacity] ([CalcCapacityCode]);