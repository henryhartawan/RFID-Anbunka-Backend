USE [RFIDP2P3_Anbunka]
GO

CREATE TABLE T_Daily_Order_DDMI_History_Headers(
    Id INT IDENTITY(1,1) NOT NULL,
    UploadBatchKey NVARCHAR(50) NOT NULL,
    UploadDate DATETIME NOT NULL,
    UploadBy NVARCHAR(100) NOT NULL, 
    DnNo NVARCHAR(100) NULL,
    SupplierCode NVARCHAR(50) NULL,
    SupplierName NVARCHAR(250) NULL,
    OrderDate DATE NULL,
    DeliveryDate DATE NULL,
    CycleIssue INT NULL,
    CycleNumber INT NULL,
    Page INT NULL,
	Status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    DeletedBy NVARCHAR(100) NULL,
    DeletedDate DATETIME NULL,
    CONSTRAINT PK_T_Daily_Order_DDMI_History_Headers PRIMARY KEY CLUSTERED (Id ASC)
);
GO

CREATE TABLE T_Daily_Order_DDMI_History_Details(
    Id INT IDENTITY(1,1) NOT NULL,
    HistoryHeaderId INT NOT NULL,
    ItemNo INT NULL,
    BackNo NVARCHAR (50) NULL,
    PartNo NVARCHAR(100) NULL,
    PartName NVARCHAR(250) NULL,
    QtyPerBox INT NULL,
    TotalKanban INT NULL,
    TotalQty INT NULL,
    ActualKanban INT NULL,
    LackKanban INT NULL,
    CONSTRAINT PK_T_Daily_Order_DDMI_History_Details PRIMARY KEY CLUSTERED (Id ASC)
);
GO