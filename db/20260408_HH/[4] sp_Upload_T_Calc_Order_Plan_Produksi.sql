USE [RFIDP2P3_DB]
GO
/****** Object:  StoredProcedure [dbo].[sp_Upload_T_Calc_Order_Plan_Produksi]    Script Date: 08/04/2026 15:56:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_Upload_T_Calc_Order_Plan_Produksi]
	@UserLogin		nvarchar(50)

AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE		@Now		datetime,
				@Periode_ID	nvarchar(8)

	SELECT @Now = GETDATE()

	DELETE T_Calc_Order_Plan_Produksi_Temp
	
	INSERT INTO T_Calc_Order_Plan_Produksi_Temp
	(	
		ID_Plan_Prod,
		Plan_Prod_Date,
		Shift_Code,
		LineOrderCode,
		Unique_No,
		Qty,
		Periode_ID,
		Upload_Date,
		Upload_By
	)
	SELECT	'PP-'+a.C1+'-'+REPLACE(a.A1,'-','') +'-'+LEFT(a.B1,1)+
			RIGHT('0000' + CAST(ROW_NUMBER()OVER(PARTITION BY a.A1, LEFT(a.C1,1) ORDER BY a.No_Urut) as nvarchar(10)),4) as ID_Plan_Prod, 
			a.A1 as Plan_Prod_Date,
			LEFT(a.B1,1) as Shift_Code,
			a.C1,
			a.D1,
			a.E1,
			a.F1,
			@Now,
			@UserLogin
	FROM	DataExcel a
			LEFT JOIN T_Calc_Order_Plan_Produksi b ON REPLACE(a.A1,'-','') = REPLACE(b.Plan_Prod_Date,'-','') 
										and LEFT(a.B1,1) = LEFT(b.Shift_Code,1) 
										and a.C1 = b.LineOrderCode 
										and a.D1 = b.Unique_No
	WHERE	a.Entry_User  = @UserLogin
			and b.Plan_Prod_Date IS NULL
			and a.A1 <> '' and a.A1 <> 'Plan Prod Date (yyyy-MM-dd)'
	ORDER BY	a.No_Urut

	SELECT TOP (1) @Periode_ID = Periode_ID + '01' FROM T_Calc_Order_Plan_Produksi_Temp WHERE Upload_By = @UserLogin

	IF EXISTS(
		SELECT	*
		FROM	T_Calc_Order_Plan_Produksi_Temp
		WHERE	FORMAT(Plan_Prod_Date, 'yyyyMM') <> Periode_ID
	)
	BEGIN
		select 'Plan Prod Date and Periode not match' as remarks
	END
	ELSE IF EXISTS(
		SELECT	*
		FROM	T_Calc_Order_Plan_Produksi_Temp a
				LEFT JOIN M_Line_Order b on a.LineOrderCode = b.LineOrderCode
		WHERE	ISNULL(b.LineOrderCode,'') = ''
	)
	BEGIN
		select 'Line order not exists in Master Line Order' as remarks
	END
	ELSE IF EXISTS(
		SELECT	*
		FROM	T_Calc_Order_Plan_Produksi_Temp a
				LEFT JOIN M_Progress_Lane b on a.LineOrderCode = b.LineOrderCode
		WHERE	ISNULL(b.LineOrderCode,'') = ''
	)
	BEGIN
		select 'Line order not exists in Master Progress Lane' as remarks
	END
	ELSE IF EXISTS(
		SELECT	*
		FROM	T_Calc_Order_Plan_Produksi_Temp a
		WHERE	Shift_Code NOT IN ('Day', 'Night', 'D', 'N')
	)
	BEGIN
		select 'Shift code not exists' as remarks
	END
	ELSE IF EXISTS(
		SELECT	*
		FROM	T_Calc_Order_Plan_Produksi_Temp a
				LEFT JOIN M_Finish_Goods b on a.Unique_No = b.UniqueNumber
		WHERE	ISNULL(b.UniqueNumber,'') = ''
	)
	BEGIN
		select 'Unique No not exists' as remarks
	END
	ELSE IF EXISTS (
		SELECT	a.ID_Plan_Prod
		FROM	T_Calc_Order_Plan_Produksi_Temp a
				LEFT JOIN dbo.fnCheckCalendar(@Periode_ID) b on a.LineOrderCode = b.LineOrderCode and a.Plan_Prod_Date = b.CalendarDate and a.Shift_Code = LEFT(b.Shift,1)
		WHERE	a.Upload_By = @UserLogin
				and b.CalendarDate IS NULL
				and a.Qty > 0
	)
	BEGIN
		select 'Line + Plan Prod Date + Shift is OFF' as remarks
	END
	ELSE IF EXISTS (
		SELECT	*
		FROM	T_Calc_Order_Plan_Produksi_Temp a
				LEFT JOIN T_Calc_Order_Plan_Produksi b on a.LineOrderCode = b.LineOrderCode and a.Plan_Prod_Date = b.Plan_Prod_Date
					and a.Shift_Code = b.Shift_Code and a.Unique_No = b.Unique_No and a.Periode_ID = b.Periode_ID
		WHERE	a.Upload_By  = @UserLogin
				and ISNULL(b.ID_Plan_Prod,'') <> ''
				and a.Plan_Prod_Date < CAST(GETDATE() as date)
	)
	BEGIN
		select 'Can''t update past Plan Prod data' as remarks
	END
	ELSE
	BEGIN
		UPDATE	b
		SET		Qty = a.Qty,
				Upload_Date = a.Upload_Date,
				Upload_By = a.Upload_By
		FROM	T_Calc_Order_Plan_Produksi_Temp a
				LEFT JOIN T_Calc_Order_Plan_Produksi b on a.LineOrderCode = b.LineOrderCode and a.Plan_Prod_Date = b.Plan_Prod_Date
					and a.Shift_Code = b.Shift_Code and a.Unique_No = b.Unique_No and a.Periode_ID = b.Periode_ID
		WHERE	a.Upload_By  = @UserLogin and ISNULL(b.ID_Plan_Prod, '') <> '' and a.Plan_Prod_Date >= CAST(GETDATE() as date)

		INSERT INTO T_Calc_Order_Plan_Produksi
		(	
			ID_Plan_Prod,
			Plan_Prod_Date,
			Shift_Code,
			LineOrderCode,
			Unique_No,
			Qty,
			Periode_ID,
			Upload_Date,
			Upload_By
		)
		SELECT	a.ID_Plan_Prod,
				a.Plan_Prod_Date,
				a.Shift_Code,
				a.LineOrderCode,
				a.Unique_No,
				a.Qty,
				a.Periode_ID,
				a.Upload_Date,
				a.Upload_By
		FROM	T_Calc_Order_Plan_Produksi_Temp a
				LEFT JOIN T_Calc_Order_Plan_Produksi b on a.LineOrderCode = b.LineOrderCode and a.Plan_Prod_Date = b.Plan_Prod_Date
					and a.Shift_Code = b.Shift_Code and a.Unique_No = b.Unique_No and a.Periode_ID = b.Periode_ID
		WHERE	a.Upload_By  = @UserLogin and ISNULL(b.ID_Plan_Prod, '') = ''
		ORDER BY	a.ID_Plan_Prod
		
		select '' as remarks
	END
END
