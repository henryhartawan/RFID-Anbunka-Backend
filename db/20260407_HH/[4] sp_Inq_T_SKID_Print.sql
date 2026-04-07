USE [RFIDP2P3_DB]
GO

/****** Object:  StoredProcedure [dbo].[sp_Inq_T_SKID_Print]    Script Date: 06/04/2026 01:17:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Inq_T_SKID_Print]
	@SKID_ID	nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	c.SupplierCode,
			c.SupplierName,
			b.Packaging_Group,
			b.Doc_Code,
			b.LineOrderCode,
			b.DN_No,
			FORMAT(b.DN_Schedule_Delivery_Date, 'dd-MMM-yyyy') as DeliveryDate,
			b.CycleArrival,
			b.Lane_No,
			b.TimeArrival,
			a.SKID_ID,
			b.Group_Route,
			b.PI_No,
			a.ExCore,
			a.Job_No,
			b.Part_Name,
			COUNT(a.SKID_ID) as QtyBox
	FROM	T_SKID a
			JOIN T_DN b on a.DN_No = b.DN_No and a.Part_No = b.Part_No and a.Job_No = b.Job_No and a.ExCore = b.ExCore
			JOIN M_Supplier c on b.Supplier_Code = c.SupplierCode
	WHERE	a.SKID_ID = @SKID_ID
	GROUP BY c.SupplierCode,
			c.SupplierName,
			b.Packaging_Group,
			b.Doc_Code,
			b.LineOrderCode,
			b.DN_No,
			FORMAT(b.DN_Schedule_Delivery_Date, 'dd-MMM-yyyy'),
			b.CycleArrival,
			b.Lane_No,
			b.TimeArrival,
			a.SKID_ID,
			b.Group_Route,
			b.PI_No,
			a.ExCore,
			a.Job_No,
			b.Part_Name
END
GO


