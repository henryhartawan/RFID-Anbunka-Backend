CREATE TYPE [dbo].[AddCalendarType] AS TABLE(
    [LineOrderCode] NVARCHAR(50),
    [CalendarDate] DATE,
    [Shift] NVARCHAR(50),
    [CalendarStatus] NVARCHAR(1),
    [WorkingTime] INT,
    [Overtime] INT,
    [MandatoryPdt] INT,
    [OtherPdt] INT,
    [TimePdt] INT,
    [Remarks] NVARCHAR(255)
);
GO