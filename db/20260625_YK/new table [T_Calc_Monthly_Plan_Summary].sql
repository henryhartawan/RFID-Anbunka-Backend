CREATE TABLE T_Calc_Monthly_Plan_Summary (
    SummaryID bigint IDENTITY(1,1) NOT NULL,
    Periode nvarchar(7) NOT NULL,
    LineCode nvarchar(50) NOT NULL,
    UniqueCode nvarchar(50) NOT NULL,
    TotalOrderQty int NOT NULL,
    RankOrder int NOT NULL,
    IsHighestOrder bit NOT NULL,
	RevisionNo INT NOT NULL DEFAULT 0,
    CreatedAt datetime DEFAULT GETDATE(),
    CreatedBy nvarchar(50),
    CONSTRAINT PK_T_Calc_Monthly_Plan_Summary PRIMARY KEY CLUSTERED (SummaryID ASC)
) ON [PRIMARY];
GO