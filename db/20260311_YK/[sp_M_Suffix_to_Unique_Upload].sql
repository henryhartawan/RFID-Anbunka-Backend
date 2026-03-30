USE [RFIDP2P3_DB]
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Suffix_to_Unique_Upload]    Script Date: 11/03/2026 21:33:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ==============================================
-- 4. SP Upload Suffix to Unique (UPLOAD)
-- ==============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_M_Suffix_to_Unique_Upload]
    @SuffixCode VARCHAR(5),
    @UniqueCode VARCHAR(20),
    @ModelGroup NVARCHAR(25),
    @LineOrderCode NVARCHAR(10),
    @UserLogin VARCHAR(50),
    @Remarks VARCHAR(100) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY

        SET @SuffixCode = TRIM(@SuffixCode);
        SET @UniqueCode = TRIM(@UniqueCode);
        SET @LineOrderCode = TRIM(@LineOrderCode);
        SET @ModelGroup = TRIM(@ModelGroup);

        IF EXISTS (
            SELECT 1 FROM M_Suffix_to_Unique 
            WHERE SuffixCode = @SuffixCode 
              AND UniqueCode = @UniqueCode 
              AND LineOrderCode = @LineOrderCode
        )
        BEGIN
            SET @Remarks = 'Combination of Suffix, Unique, and Line already exists in Database!';
            RETURN;
        END

        DECLARE @NewId VARCHAR(10);
        DECLARE @LastId VARCHAR(10) = (SELECT TOP 1 SuffixId FROM M_Suffix_to_Unique ORDER BY SuffixId DESC);
        
        IF @LastId IS NULL 
            SET @NewId = 'SFUC001';
        ELSE 
        BEGIN
            DECLARE @NextNum INT = CAST(RIGHT(@LastId, 3) AS INT) + 1;
            SET @NewId = 'SFUC' + RIGHT('000' + CAST(@NextNum AS VARCHAR), 3);
        END

        INSERT INTO M_Suffix_to_Unique (
            SuffixId, SuffixCode, UniqueCode, ModelGroup, LineOrderCode, CreatedBy, CreatedDate
        )
        VALUES (
            @NewId, @SuffixCode, @UniqueCode, @ModelGroup, @LineOrderCode, @UserLogin, GETDATE()
        );

        SET @Remarks = '';

    END TRY
    BEGIN CATCH
        SET @Remarks = ERROR_MESSAGE(); 
    END CATCH
END
GO