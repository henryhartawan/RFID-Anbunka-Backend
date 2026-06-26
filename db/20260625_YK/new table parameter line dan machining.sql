-- Tabel Parameter K-Line
CREATE TABLE M_Param_Stock_KLine (
    ParamKLineID int IDENTITY(1,1) NOT NULL,
    Periode nvarchar(7) NOT NULL,
    OrderFrom nvarchar(50) NOT NULL,
    CycleDelivery int NOT NULL,
	ProdMonth int,
	ProdDaily int,
	QtyperTruck int,
	Cycle int,
    CreatedAt datetime DEFAULT GETDATE(),
    CreatedBy nvarchar(50),
    UpdatedAt datetime NULL,
    UpdatedBy nvarchar(50) NULL,
    CONSTRAINT PK_M_Param_Stock_KLine PRIMARY KEY CLUSTERED (ParamKLineID ASC)
);

-- Tabel Parameter Machining Line
CREATE TABLE M_Param_Stock_Machining (
    ParamMachiningID int IDENTITY(1,1) NOT NULL,
    Periode nvarchar(7) NOT NULL,
    StandardDay int NOT NULL,
    CreatedAt datetime DEFAULT GETDATE(),
    CreatedBy nvarchar(50),
    UpdatedAt datetime NULL,
    UpdatedBy nvarchar(50) NULL,
    CONSTRAINT PK_M_Param_Stock_Machining PRIMARY KEY CLUSTERED (ParamMachiningID ASC)
);