CREATE TABLE M_Add_Calendar (
    CalendarId INT IDENTITY(1,1) NOT NULL,
    LineOrderCode NVARCHAR(50) NOT NULL,
    CalendarDate DATE NOT NULL,
    Shift NVARCHAR(50) NOT NULL,
    CalendarStatus BIT NULL,
    WorkingTime INT NULL,
    Overtime INT NULL,
    MandatoryPdt INT NULL,
    OtherPdt INT NULL,
    TimePdt INT NULL,
    Remarks NVARCHAR(255) NULL,
    CreatedBy VARCHAR(50) NULL,
    CreatedDate DATETIME NULL,
    UpdatedBy VARCHAR(50) NULL,
    UpdatedDate DATETIME NULL,
    CONSTRAINT PK_M_Add_Calendar PRIMARY KEY CLUSTERED (CalendarId)
) ON [PRIMARY];
GO