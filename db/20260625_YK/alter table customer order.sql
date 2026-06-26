ALTER TABLE M_Customer_Order 
ADD RevisionNo INT NOT NULL DEFAULT 0;

ALTER TABLE M_Customer_Order
ALTER COLUMN ValueData DECIMAL(18,2) NULL;
GO

DROP PROCEDURE IF EXISTS sp_M_Customer_Order_Upload;
DROP TYPE IF EXISTS CustomerOrderType;

CREATE TYPE CustomerOrderType AS TABLE(
    Periode NVARCHAR(15) NULL,
    Source NVARCHAR(50) NULL,
    Suffix NVARCHAR(50) NULL,
    DayNumber INT NULL,
    ValueData DECIMAL(18,2) NULL
)