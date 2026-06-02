CREATE TYPE dbo.T_Daily_Order_DDMI_Type AS TABLE (
    -- Header Data
    DnNo NVARCHAR(100),
    SupplierCode NVARCHAR(50),
    SupplierName NVARCHAR(150),
    Location NVARCHAR(50),
    OrderDate DATE,
    OrderTime TIME,
    DeliveryDate DATE,
    DeliveryTime TIME,
    Distribution NVARCHAR(100),
    DockCode NVARCHAR(50),
    CycleIssue INT,
    Rev NVARCHAR(50),
    Page INT,
    Remark NVARCHAR(500),
    SupplierApprovedBy NVARCHAR(100),
    SupplierPreparedBy NVARCHAR(100),
    TransporterDeliveryBy NVARCHAR(100),
    TransporterReceiverBy NVARCHAR(150),
    DdmiReceiverBy NVARCHAR(100),
    DdmiOrderBy NVARCHAR(100),
    
    -- Detail Data
    ItemNo INT,
    BackNo NVARCHAR(50),
    PartNo NVARCHAR(100),
    PartName NVARCHAR(255),
    QtyPerBox INT,
    TotalKanban INT,
    TotalQty INT,
    ActualKanban INT,
    LackKanban INT
);