DROP PROCEDURE IF EXISTS sp_M_Add_Calendar_Upload;
DROP TYPE IF EXISTS AddCalendarType;

CREATE TYPE AddCalendarType AS TABLE(
    LineOrderCode nvarchar(50) NULL,
    CalendarDate date NULL,
    Shift nvarchar(50) NULL,
    CalendarStatus int NULL,
    WorkingTime int NULL,
    OEE decimal(18, 2) NULL,
    CT decimal(18, 2) NULL,
    EarlyOvertime int NULL,
    EndOvertime int NULL,
    MandatoryPdt int NULL,
    OtherPdt int NULL,
    TimePdt int NULL,
    Remarks nvarchar(255) NULL,
    ProductionTarget int NULL
);