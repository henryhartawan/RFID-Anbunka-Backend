USE [RFIDP2P3_DB]
GO
/****** Object:  StoredProcedure [dbo].[sp_Move_OEE_Data]    Script Date: 17/03/2026 22:39:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==============================================
-- procedure move data OEE TT Future to OEE TT
-- ==============================================

CREATE or ALTER PROCEDURE [dbo].[sp_Move_OEE_Data]
AS
BEGIN
    SET NOCOUNT ON; 

    DECLARE @CurrMonth VARCHAR(7) = FORMAT(GETDATE(), 'yyyy-MM');
    DECLARE @PrevMonth VARCHAR(7) = FORMAT(DATEADD(MONTH, -1, GETDATE()), 'yyyy-MM');

	BEGIN TRY
        BEGIN TRANSACTION;

		-- =====================================================================
        -- PART 1: BACKUP PROCESS (Runs only if previous month backup doesn't exist)
        -- =====================================================================
		IF NOT EXISTS (SELECT 1 FROM M_OEE_TT_Future WHERE Periode = @PrevMonth)
        BEGIN
            INSERT INTO M_OEE_TT_Future
                (Periode, LineOrderCode, OEE, CT, OEETTStatus, UserUpdate, DateUpdate)
            SELECT 
                @PrevMonth, 
                LineOrderCode, 
                OEE, 
                CT, 
                OEETTStatus, 
                UserUpdate, 
                DateUpdate
            FROM M_OEE_TT;
        END

		-- =====================================================================
        -- PART 2: MOVE DATA PROCESS (Runs if current month data is ready)
        -- =====================================================================
        IF EXISTS (SELECT 1 FROM M_OEE_TT_Future WHERE Periode = @CurrMonth)
        BEGIN
            TRUNCATE TABLE M_OEE_TT;

            INSERT INTO M_OEE_TT
                (LineOrderCode, OEE, CT, ImplementDate, OEETTStatus, UserUpdate, DateUpdate)
            SELECT
                LineOrderCode, 
                OEE, 
                CT, 
                GETDATE(), 
                OEETTStatus, 
                UserUpdate, 
                DateUpdate
            FROM M_OEE_TT_Future
            WHERE Periode = @CurrMonth;
        END
		ELSE
        BEGIN
            SELECT 'Update process skipped: Data for ' + @CurrMonth + ' has not been prepared in M_OEE_TT_Future.' AS remarks;
        END
		COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrorMessage, 16, 1);
    END CATCH
END