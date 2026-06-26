USE [RFIDP2P3_Anbunka]
GO
/****** Object:  StoredProcedure [dbo].[sp_Delete_T_Calc_Order_Firm]    Script Date: 6/25/2026 9:42:07 AM ******/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      MIT
-- Description: [sp_Delete_T_Calc_Order_Firm]
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_Delete_T_Calc_Order_Firm]
(
    @Periode NVARCHAR(7),
    @Remarks NVARCHAR(MAX) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET @Remarks = '';

    BEGIN TRY
        BEGIN TRANSACTION

        IF NOT EXISTS (SELECT 1 FROM T_Calc_Order_Firm WHERE Periode = @Periode)
        BEGIN
            SET @Remarks = 'No data found for period ' + @Periode + ' to delete.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        DELETE FROM T_Calc_Order_Firm 
        WHERE Periode = @Periode;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SET @Remarks = 'Database Error: ' + ERROR_MESSAGE(); 
    END CATCH
END
GO