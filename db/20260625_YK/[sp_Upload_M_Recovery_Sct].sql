USE [RFIDP2P3_Anbunka]
GO
/****** Object:  StoredProcedure [dbo].[sp_Upload_M_Recovery_Sct]    Script Date: 6/15/2026 7:34:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		MIT
-- Description: [sp_Upload_M_Recovery_Sct]
-- =============================================
CREATE or ALTER PROCEDURE [dbo].[sp_Upload_M_Recovery_Sct]
    @LineOrderCode VARCHAR(50),
    @TargetDate DATE,
    @SpecialCycleTime DECIMAL(18,2) = NULL,
    @RecoveryDay DECIMAL(18,2) = 0,
    @RecoveryNight DECIMAL(18,2) = 0,
    @Remarks VARCHAR(255) = NULL,
    @UserLogin VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
	BEGIN TRY
        BEGIN TRANSACTION;

        -- =========================================================================
        -- 1. PROSES TABEL SPECIAL CT (Level Harian)
        -- =========================================================================
        IF EXISTS (SELECT 1 FROM M_Special_CT WHERE LineOrderCode = @LineOrderCode AND TargetDate = @TargetDate)
        BEGIN
            UPDATE M_Special_CT
            SET SpecialCycleTime = @SpecialCycleTime, 
                Remarks = @Remarks,
                UpdatedBy = @UserLogin, 
                UpdatedAt = GETDATE()
            WHERE LineOrderCode = @LineOrderCode AND TargetDate = @TargetDate;
        END
        ELSE IF @SpecialCycleTime IS NOT NULL
        BEGIN
            INSERT INTO M_Special_CT (LineOrderCode, TargetDate, SpecialCycleTime, Remarks, CreatedBy)
            VALUES (@LineOrderCode, @TargetDate, @SpecialCycleTime, @Remarks, @UserLogin);
        END

        -- =========================================================================
        -- 2. PROSES TABEL RECOVERY PLAN (Shift Day)
        -- =========================================================================
        IF EXISTS (SELECT 1 FROM M_Recovery_Plan WHERE LineOrderCode = @LineOrderCode AND TargetDate = @TargetDate AND Shift = 'Day')
        BEGIN
            UPDATE M_Recovery_Plan
            SET RecoveryTime = @RecoveryDay, 
                Remarks = @Remarks,
                UpdatedBy = @UserLogin, 
                UpdatedAt = GETDATE()
            WHERE LineOrderCode = @LineOrderCode AND TargetDate = @TargetDate AND Shift = 'Day';
        END
        ELSE IF @RecoveryDay >= 0
        BEGIN
            INSERT INTO M_Recovery_Plan (LineOrderCode, TargetDate, Shift, RecoveryTime, Remarks, CreatedBy)
            VALUES (@LineOrderCode, @TargetDate, 'Day', @RecoveryDay, @Remarks, @UserLogin);
        END

        -- =========================================================================
        -- 3. PROSES TABEL RECOVERY PLAN (Shift Night)
        -- =========================================================================
        IF EXISTS (SELECT 1 FROM M_Recovery_Plan WHERE LineOrderCode = @LineOrderCode AND TargetDate = @TargetDate AND Shift = 'Night')
        BEGIN
            UPDATE M_Recovery_Plan
            SET RecoveryTime = @RecoveryNight, 
                Remarks = @Remarks,
                UpdatedBy = @UserLogin, 
                UpdatedAt = GETDATE()
            WHERE LineOrderCode = @LineOrderCode AND TargetDate = @TargetDate AND Shift = 'Night';
        END
        ELSE IF @RecoveryNight >= 0
        BEGIN
            INSERT INTO M_Recovery_Plan (LineOrderCode, TargetDate, Shift, RecoveryTime, Remarks, CreatedBy)
            VALUES (@LineOrderCode, @TargetDate, 'Night', @RecoveryNight, @Remarks, @UserLogin);
        END

        COMMIT TRANSACTION;
        SELECT 'SUCCESS' AS Status, 'Data adjustment successfully saved.' AS Message;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 'ERROR' AS Status, ERROR_MESSAGE() AS Message;
    END CATCH

END