USE [RFIDP2P3_Anbunka]
GO
/****** Object:  StoredProcedure [dbo].[sp_Inq_T_Daily_Order_DDMI_History]    Script Date: 6/24/2026 10:10:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:	MIT
-- Description: [sp_Inq_T_Daily_Order_DDMI_History]
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Inq_T_Daily_Order_DDMI_History]
    @Periode NVARCHAR(7)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATE = CAST(@Periode + '-01' AS DATE);
    DECLARE @EndDate DATE = DATEADD(MONTH, 1, @StartDate);

    SELECT 
        hh.UploadBatchKey,
        hh.UploadDate,
        hh.UploadBy,
        hh.DnNo,
        hh.SupplierCode,
        hh.SupplierName,
        hh.OrderDate,
        hh.DeliveryDate,
        hh.CycleIssue,
        hh.CycleNumber,
        hh.[Page],
        hh.[Status],
        hh.DeletedBy,
        hh.DeletedDate,
        hd.ItemNo,
        hd.BackNo,
        hd.PartNo,
        hd.PartName,
        hd.QtyPerBox,
        hd.TotalKanban,
        hd.TotalQty,
        hd.ActualKanban,
        hd.LackKanban
    FROM T_Daily_Order_DDMI_History_Headers hh
    INNER JOIN T_Daily_Order_DDMI_History_Details hd ON hh.Id = hd.HistoryHeaderId
    WHERE hh.UploadDate >= @StartDate AND hh.UploadDate < @EndDate
    ORDER BY hh.UploadDate DESC, hh.DnNo, hd.ItemNo;
END
GO