CREATE TABLE M_MinMax_Stock_Unique (
    StockID INT IDENTITY(1,1) PRIMARY KEY,
    Periode NVARCHAR(7) NOT NULL,
    UniqueCode NVARCHAR(50) NOT NULL,
    MinStock INT NOT NULL DEFAULT 0,
    MaxStock INT NOT NULL DEFAULT 0,
    
    CreatedBy NVARCHAR(100) DEFAULT 'System',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME NULL
);

CREATE UNIQUE NONCLUSTERED INDEX IX_M_MinMax_Stock_Unique_Lookup
ON M_MinMax_Stock_Unique (Periode, UniqueCode);