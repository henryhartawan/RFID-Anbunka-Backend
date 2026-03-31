USE [RFIDP2P3_DB]
GO

/****** Object:  StoredProcedure [dbo].[sp_Inq_Monitoring_SKID]    Script Date: 28/03/2026 11:47:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Inq_Monitoring_SKID] 
	@DeliveryDate date
AS
BEGIN
	SET NOCOUNT ON;

	WITH DataSum AS (
		SELECT	PI_No,
				DN_No,
				ExCore,
				Job_No,
				SUM(Total_Kanban) AS Total_Kanban
		FROM	T_DN
		WHERE	DN_Schedule_Delivery_Date = @DeliveryDate
				and ISNULL(PI_No,'') <> ''
		GROUP BY PI_No, DN_No, ExCore, Job_No
	)
	SELECT	x.PI_No,
			x.DN_No,
			x.ExCore,
			x.Job_No,
			RIGHT('000' + CAST(x.rn AS VARCHAR(3)), 3) AS KanbanSeq,
			x.DN_No + ' | ' + x.ExCore + ' | ' + x.Job_No as Groups,
			'' as SKID_ID,
			'' as PI_No_Act,
			'' as DN_No_Act,
			'' as ExCore_Act,
			'' as Job_No_Act,
			'' as KanbanSeq_Act,
			0 as Qty_PI_No_Act,
			0 as Qty_Groups_Act
	FROM	(
		SELECT	d.PI_No,
				d.DN_No,
				d.ExCore,
				d.Job_No,
				ROW_NUMBER() OVER (
					PARTITION BY d.PI_NO, d.DN_No, d.ExCore, d.Job_No
					ORDER BY m.number
				) AS rn
		FROM	DataSum d
				CROSS APPLY (
					SELECT TOP (d.Total_Kanban) number
					FROM MasterNumber m
					ORDER BY number
				) m
	) x
	ORDER BY x.PI_No, x.DN_No, x.ExCore, x.Job_No, x.rn
END
GO


