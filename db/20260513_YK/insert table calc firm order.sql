CREATE TABLE T_Calc_Order_Firm (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
    Periode VARCHAR(7) NOT NULL,
    Suffix VARCHAR(10) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    MonthOffsetLabel VARCHAR(10) NOT NULL,
    Qty INT NOT NULL DEFAULT 0,
    FirmID AS CAST(
        'FO' + '-' +                                  
        UPPER(Suffix) + '-' +                         
        UPPER(LEFT(Status, 4)) + '-' +                
        REPLACE(Periode, '-', '') + '-' +             
        'N' + RIGHT('0000' + CAST(
            CASE 
                WHEN MonthOffsetLabel = 'N' THEN 1
                ELSE ISNULL(TRY_CAST(REPLACE(MonthOffsetLabel, 'N+', '') AS INT), 0) + 1
            END 
        AS VARCHAR(10)), 4)
    AS VARCHAR(50)) PERSISTED,
    CreatedUser VARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedUser VARCHAR(50) NULL,
    UpdatedAt DATETIME NULL
);
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Order_Firm_CustomID 
ON T_Calc_Order_Firm(FirmID);
GO