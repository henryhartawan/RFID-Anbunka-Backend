CREATE TABLE T_Parameter_Capacity (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Periode VARCHAR(7) NOT NULL,
    LineCode VARCHAR(50) NOT NULL,
    MonthOffsetLabel NVARCHAR(10),
    Advance INT DEFAULT 0,
    Mandatory INT DEFAULT 0,
    OvertimeHOT INT DEFAULT 0,
    CreatedUser NVARCHAR(50),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO