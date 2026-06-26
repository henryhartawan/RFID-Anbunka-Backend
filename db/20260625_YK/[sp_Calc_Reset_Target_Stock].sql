USE [RFIDP2P3_Anbunka]
GO
/****** Object:  StoredProcedure [dbo].[sp_Calc_Reset_Target_Stock]    Script Date: 6/22/2026 7:18:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:	MIT
-- Description:	[sp_Calc_Reset_Target_Stock]
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_Calc_Reset_Target_Stock]
    @Periode VARCHAR(6),
    @User_Login NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @DBPeriode VARCHAR(7) = LEFT(@Periode, 4) + '-' + RIGHT(@Periode, 2);
    DECLARE @Err NVARCHAR(MAX);

	BEGIN TRY
        BEGIN TRANSACTION;

		IF EXISTS (SELECT 1 FROM T_Target_Stock_Machining WHERE Periode = @DBPeriode)
           OR EXISTS (SELECT 1 FROM T_Target_Stock_KLine WHERE Periode = @DBPeriode)
        BEGIN

			UPDATE M_MinMax_Stock_Unique
			SET MinStock = 0, 
				MaxStock = 0, 
				UpdatedBy = @User_Login, 
				UpdatedAt = GETDATE()
			WHERE Periode = @DBPeriode;

			DELETE FROM T_Target_Stock_Machining WHERE Periode = @DBPeriode;
            DELETE FROM T_Target_Stock_KLine WHERE Periode = @DBPeriode;

            INSERT INTO T_Status_Monthly_Plan 
			VALUES ('Reset Target Stock', 'Success', NULL, @Periode, GETDATE(), @User_Login);

            COMMIT TRANSACTION;
            SELECT 'success' AS Result;

        END
        ELSE
        BEGIN
            THROW 50003, 'No data found to be reset for this period.', 1;
        END
    END TRY
    BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        SET @Err = ERROR_MESSAGE();
        
        INSERT INTO T_Status_Monthly_Plan 
        VALUES ('Reset Target Stock', 'Error', @Err, @Periode, GETDATE(), @User_Login);
        
		SELECT @Err AS Result;
    END CATCH
END
GO