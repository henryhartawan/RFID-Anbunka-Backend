CREATE TABLE T_Control_Order_Firm (
    Periode VARCHAR(7) PRIMARY KEY,
    IsCalculated BIT NOT NULL DEFAULT 0,
    LockedDate DATETIME NULL,
    LockedBy VARCHAR(50) NULL
);