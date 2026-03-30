CREATE TYPE dbo.CustomerOrderType AS TABLE
(
    Periode VARCHAR(7),
    Source VARCHAR(50),
    Suffix VARCHAR(50),
    DayNumber INT,
    ValueData DECIMAL(18,2)
)
GO