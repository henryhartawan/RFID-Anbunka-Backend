USE [RFIDP2P3_DB]
GO

/****** Object:  StoredProcedure [dbo].[sp_Submit_GR]    Script Date: 01/04/2026 01:05:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE PROCEDURE [dbo].[sp_Submit_SKID]
(
	@SKID_ID	nvarchar(50),
	@KanbanNo	nvarchar(50),
	@PI_No		nvarchar(50),
	@DN_No		nvarchar(50),
	@Part_No	nvarchar(50),
	@ExCore		nvarchar(50),
	@Job_No		nvarchar(50),
	@KanbanSeq	nvarchar(3),
	@EntryDate	datetime,
	@UserLogin	varchar(50),
	@Remarks	varchar(100) output
)

AS
BEGIN
	SET NOCOUNT ON

	BEGIN TRY
		DECLARE @SupplierID nvarchar(10)
		SELECT @SupplierID = SupplierCode FROM M_Part_Order WHERE PartNumber = @Part_No

		IF (@SKID_ID = '')
		BEGIN
			SELECT	@SKID_ID = FORMAT(@EntryDate, 'yyMMdd') + @SupplierID + RIGHT('00000' + CAST(ISNULL(MAX(RIGHT(SKID_ID,5)),0) + 1 as nvarchar), 5)
			FROM	T_SKID
			WHERE	LEFT(SKID_ID, 6) = FORMAT(@EntryDate, 'yyMMdd')
		END

		INSERT	INTO T_SKID
		(
			SKID_ID, KanbanNo, PI_No, DN_No, Part_No, ExCore, Job_No, KanbanSeq, ScanBy, ScanDate
		)
		SELECT	@SKID_ID,
				@KanbanNo,
				@PI_No,
				@DN_No,
				@Part_No,
				@ExCore,
				@Job_No,
				@KanbanSeq,
				@UserLogin,
				@EntryDate

		SET @Remarks = 'success~' + @SKID_ID
	END TRY

	BEGIN CATCH
		SET @Remarks = 'error~' + ERROR_MESSAGE()
	END CATCH
END

GO


