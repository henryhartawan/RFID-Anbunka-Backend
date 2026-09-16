CREATE TABLE M_Parameter_Mandatory_Option (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    LineType NVARCHAR(20) NOT NULL,
    Shift INT NOT NULL,
    MandatoryValue INT NOT NULL,
    Description NVARCHAR(150) NULL,
    IsDefault BIT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 1
);

CREATE INDEX IX_MandatoryOption_Lookup 
ON M_Parameter_Mandatory_Option (LineType, Shift);