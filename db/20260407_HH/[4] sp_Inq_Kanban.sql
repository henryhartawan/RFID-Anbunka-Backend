USE [RFIDP2P3_DB]
GO

/****** Object:  StoredProcedure [dbo].[sp_Inq_Kanban]    Script Date: 01/04/2026 00:58:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Inq_Kanban] 
	@KanbanNo	nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		IF NOT EXISTS (SELECT * FROM T_DN WHERE DN_No = LEFT(@KanbanNo,16) and ISNULL(DN_Status,1) = 1)
		BEGIN
			RAISERROR('DN No. not exists', 16, 1)
		END
		ELSE IF NOT EXISTS (SELECT * FROM M_Part_Order WHERE PartNumber = SUBSTRING(@KanbanNo, 17, 14))
		BEGIN
			RAISERROR('Part No. not exists in Master Part Order', 16, 1)
		END
		ELSE IF EXISTS (SELECT * FROM T_DN WHERE DN_No = LEFT(@KanbanNo,16) and ISNULL(DN_Status,1) = 1 and ISNULL(PI_No,'') = '')
		BEGIN
			RAISERROR('DN not yet print packing instruction', 16, 1)
		END
		ELSE IF EXISTS (SELECT * FROM T_SKID WHERE KanbanNo = @KanbanNo)
		BEGIN
			RAISERROR('Box Label already scanned', 16, 1)
		END
		ELSE IF ((SELECT SUM(Total_Kanban) FROM T_DN WHERE ISNULL(DN_Status,1) = 1 and DN_No = LEFT(@KanbanNo,16) and Part_No = SUBSTRING(@KanbanNo, 17, 14)) < CAST(RIGHT(@KanbanNo,3) as int))
		BEGIN
			RAISERROR('Total box scan exceed total box DN', 16, 1)
		END
		ELSE
		BEGIN
			SELECT	DISTINCT PI_No,
					DN_No,
					Part_No,
					ExCore,
					Job_No,
					RIGHT(@KanbanNo,3) as KanbanSeq,
					@KanbanNo as KanbanNo,
					'success' as Remarks
			FROM	T_DN
			WHERE	ISNULL(DN_Status,1) = 1
					and ISNULL(PI_No,'') <> ''
					and DN_No = LEFT(@KanbanNo,16)
					and Part_No = SUBSTRING(@KanbanNo, 17, 14)
		END
	END TRY

	BEGIN CATCH
		SELECT	'' as PI_No,
				'' as DN_No,
				'' as Part_No,
				'' as ExCore,
				'' as Job_No,
				'' as KanbanSeq,
				'' as KanbanNo,
				ERROR_MESSAGE() as Remarks
	END CATCH
END
GO


