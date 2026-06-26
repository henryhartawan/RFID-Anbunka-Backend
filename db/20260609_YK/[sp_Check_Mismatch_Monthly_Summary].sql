USE [RFIDP2P3_Anbunka]
GO
/****** Object:  StoredProcedure [dbo].[sp_Check_Mismatch_Monthly_Summary]    Script Date: 6/9/2026 10:34:14 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =========================================================================
-- Author:      MIT (Modified)
-- Description: Check Mismatch between Monthly Order and Summary Firm Order
-- =========================================================================

CREATE or ALTER PROCEDURE [dbo].[sp_Check_Mismatch_Monthly_Summary]
    @Periode_ID VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(@Periode_ID) = 6 AND CHARINDEX('-', @Periode_ID) = 0
    BEGIN
        SET @Periode_ID = SUBSTRING(@Periode_ID, 1, 4) + '-' + SUBSTRING(@Periode_ID, 5, 2);
    END

    DECLARE @Periode_Summary VARCHAR(7);
    SET @Periode_Summary = CONVERT(VARCHAR(7), DATEADD(MONTH, -1, CAST(@Periode_ID + '-01' AS DATE)), 126);

    SELECT 
        COALESCE(m.Suffix, s.Suffix) AS SuffixCode,
        ISNULL(m.TotalMonthly, 0) AS Monthly_Qty,
        ISNULL(s.TotalSummary, 0) AS Summary_N_Qty
    FROM 
        (
            SELECT Suffix, SUM(ValueData) AS TotalMonthly 
            FROM M_Customer_Order 
            WHERE Periode = @Periode_ID 
            GROUP BY Suffix
        ) m
    FULL OUTER JOIN 
        (
            SELECT Suffix, SUM(Qty) AS TotalSummary 
            FROM T_Calc_Order_Firm 
            WHERE Periode = @Periode_Summary AND MonthOffsetLabel = 'N' 
            GROUP BY Suffix
        ) s 
        ON m.Suffix = s.Suffix
    WHERE ISNULL(m.TotalMonthly, 0) <> ISNULL(s.TotalSummary, 0);

END
GO