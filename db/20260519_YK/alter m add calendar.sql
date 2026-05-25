EXEC sp_rename 'dbo.M_Add_Calendar.Overtime', 'EarlyOvertime', 'COLUMN';

ALTER TABLE M_Add_Calendar
ADD EndOvertime INT 

ALTER TABLE M_Add_Calendar
ALTER COLUMN CalendarStatus NVARCHAR(1)

ALTER TABLE M_Add_Calendar
ADD DescCalendarStatus NVARCHAR(25)

ALTER TABLE M_Add_Calendar
ADD OEE DECIMAL(18, 2)

ALTER TABLE M_Add_Calendar
ADD CT DECIMAL(18, 2)

IF OBJECT_ID('dbo.sp_M_Add_Calendar_Upload', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_M_Add_Calendar_Upload;
END
GO

DROP TYPE IF EXISTS dbo.AddCalendarType;

CREATE TYPE [dbo].[AddCalendarType] AS TABLE(
    [LineOrderCode] NVARCHAR(50),
    [CalendarDate] DATE,
    [Shift] NVARCHAR(50),
    [CalendarStatus] NVARCHAR(1),
    [WorkingTime] INT,
    [OEE] DECIMAL(18, 2),
    [CT] DECIMAL(18, 2),
    [EarlyOvertime] INT,
    [EndOvertime] INT,
    [MandatoryPdt] INT,
    [OtherPdt] INT,
    [TimePdt] INT,
    [Remarks] NVARCHAR(255)
);
GO