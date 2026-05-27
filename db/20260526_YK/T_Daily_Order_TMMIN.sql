CREATE TABLE T_Daily_Order_TMMIN (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    UploadDate DATE NOT NULL,
    OrderNo VARCHAR(50),
    
    PartNo VARCHAR(50), 
    PcsPerKbn INT, 
    TotalPcs INT, 
    Kanban INT, 
    OrderDate DATETIME2, 
    Cycle VARCHAR(20),
    
    RawDataJSON NVARCHAR(MAX), 
    
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy VARCHAR(50),
    UpdatedDate DATETIME2,
    UpdatedBy VARCHAR(50)
);
GO


CREATE TYPE dbo.DailyOrderTMMIN_Type AS TABLE (
    OrderNo VARCHAR(50),
    PartNo VARCHAR(50),
    PcsPerKbn INT,
    TotalPcs INT,
    Kanban INT,
    OrderDate DATETIME2,
    Cycle VARCHAR(20),
    RawDataJSON NVARCHAR(MAX)
);
GO