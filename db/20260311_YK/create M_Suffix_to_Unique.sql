CREATE TABLE M_Suffix_to_Unique (
	SuffixId NVARCHAR(10) NOT NULL PRIMARY KEY,
    SuffixCode NVARCHAR(5) NOT NULL,
    UniqueCode NVARCHAR(20) NOT NULL,
    ModelGroup NVARCHAR(25) NULL,
    LineOrderCode NVARCHAR(10) NOT NULL,

	CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50) NOT NULL,
    UpdatedDate DATETIME NULL, 
    UpdatedBy VARCHAR(50) NULL,

	CONSTRAINT UQ_MSuffix_Combination UNIQUE (SuffixCode, UniqueCode, LineOrderCode)
);