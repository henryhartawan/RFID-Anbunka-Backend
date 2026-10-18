USE RFIDP2P3_Anbunka
GO

CREATE TABLE M_DOT_Limit(
    Id int IDENTITY(1,1) NOT NULL,
    LineName varchar(50) NOT NULL,
    LimitDOT decimal(10, 2) NOT NULL,
    CreatedAt datetime NULL DEFAULT (GETDATE()),
    PRIMARY KEY CLUSTERED (Id ASC)
) ON [PRIMARY]
GO

INSERT INTO M_DOT_Limit (LineName, LimitDOT)
VALUES 
    ('K-Line', 4.5),
    ('Machining', 3.3);
GO