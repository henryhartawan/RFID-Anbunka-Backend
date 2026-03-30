CREATE TABLE M_Customer_Order (
    CustomerOrderID INT IDENTITY(1,1) PRIMARY KEY,
    Periode VARCHAR(7),
    Source VARCHAR(50),
    Suffix VARCHAR(50),
    DayNumber INT, 
    ValueData DECIMAL(18,2)
)