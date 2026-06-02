CREATE TABLE T_Daily_Order_DDMI_Headers(
    Id INT IDENTITY(1,1) PRIMARY KEY, -- Auto increment ID
    DnNo NVARCHAR(100) NOT NULL,       -- DN NO (misal: DN1960A00021-DDMI01/05/26)
    SupplierCode NVARCHAR(50),         -- Supplier Code (misal: A0002)
    SupplierName NVARCHAR(150),        -- Supplier Name (misal: PT ASTRA DAIHATSU MOTOR)
    Location NVARCHAR(50),             -- Location (misal: EG)
    OrderDate DATE,                   -- Order Date (misal: 12-May-26)
    OrderTime TIME,                   -- Order Time/SEQ (misal: 09:54:00)
    DeliveryDate DATE,                -- Delivery Date (misal: 15-May-26)
    DeliveryTime TIME,                -- Delivery Time (misal: 10:00:00)
    Distribution NVARCHAR(100),        -- Distribution (misal: A00021-DDMI01)
    DockCode NVARCHAR(50),             -- Dock Code (misal: DD)
    CycleIssue INT,                   -- Cycle Issue (misal: 3)
    Rev NVARCHAR(50),                  -- Menyimpan informasi revisi (jika ada)
    Page INT,                         -- Menyimpan nomor halaman dokumen
    Remark NVARCHAR(500),              -- Menyimpan catatan/keterangan tambahan

	SupplierApprovedBy NVARCHAR(100), 
    SupplierPreparedBy NVARCHAR(100),

    TransporterDeliveryBy NVARCHAR(100),
    TransporterReceiverBy NVARCHAR(150),

    DdmiReceiverBy NVARCHAR(100),
    DdmiOrderBy NVARCHAR(100),
    
    UploadDate DATETIME DEFAULT GETDATE(),
	UploadBy NVARCHAR(100)
    
    CONSTRAINT UQ_DnNo UNIQUE (DnNo) 
);

CREATE TABLE T_Daily_Order_DDMI_Details (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- Auto increment ID
    HeaderId INT NOT NULL,            -- Foreign Key ke tabel DeliveryNoteHeaders
    ItemNo INT,                       -- Kolom "No" di CSV (misal: 1, 2, 3)
    BackNo NVARCHAR(50),               -- Kolom "Back No" (misal: 500)
    PartNo NVARCHAR(100) NOT NULL,     -- Kolom "Part No" (misal: 35411-BZ010)
    PartName NVARCHAR(255),            -- Kolom "Part Name" (misal: BODY, LWR VALVE)
    QtyPerBox INT,                    -- Kolom Qty/Box (Pcs) (misal: 60)
    TotalKanban INT,                  -- Kolom Total Kanban (misal: 6)
    TotalQty INT,                     -- Kolom Total Qty (Pcs) (misal: 360)
	ActualKanban INT,      -- Untuk mengisi nilai "Act" (Actual) saat serah terima
    LackKanban INT,
    
    -- Relasi ke tabel Header
    CONSTRAINT FK_T_Daily_Order_DDMI_Details_Headers 
        FOREIGN KEY (HeaderId) 
        REFERENCES T_Daily_Order_DDMI_Headers(Id) 
        ON DELETE CASCADE -- Jika header dihapus, detail otomatis terhapus
);