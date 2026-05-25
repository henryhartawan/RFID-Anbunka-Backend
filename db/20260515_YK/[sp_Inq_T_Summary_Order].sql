ALTER PROCEDURE [dbo].[sp_Inq_T_Summary_Order]
    @Periode_ID VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(@Periode_ID) = 6 AND CHARINDEX('-', @Periode_ID) = 0
    BEGIN
        SET @Periode_ID = SUBSTRING(@Periode_ID, 1, 4) + '-' + SUBSTRING(@Periode_ID, 5, 2);
    END

    DECLARE @cols AS NVARCHAR(MAX);
    DECLARE @query AS NVARCHAR(MAX);

    SELECT @cols = STUFF((SELECT DISTINCT ',' + QUOTENAME(MonthOffsetLabel)
                FROM T_Calc_Order_Firm
                WHERE Periode = @Periode_ID
        FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)')
    ,1,1,'');

    IF @cols IS NULL
    BEGIN
        SELECT 'No Data Found' AS Message;
        RETURN;
    END

    SET @query = N'
    WITH ActualData AS (
        SELECT
            ISNULL(suq.OrderFrom, ''Uncategorized'') AS OrderFrom,
            ISNULL(fgoods.EngineGrouping, calcOf.Suffix) AS EngineGrouping,
            ISNULL(fgoods.EngineBase, ''NR'') AS EngineBase,
            calcOf.MonthOffsetLabel,
            calcOf.Qty
        FROM T_Calc_Order_Firm calcOf
        LEFT JOIN M_Suffix_to_Unique suq ON calcOf.Suffix = suq.SuffixCode
        LEFT JOIN M_Finish_Goods fgoods ON suq.UniqueCode = fgoods.UniqueNumber
        WHERE calcOf.Periode = @Periode_ID
    ),
    ActiveOrderFroms AS (
        SELECT DISTINCT OrderFrom
        FROM M_Suffix_to_Unique
        WHERE OrderFrom IS NOT NULL AND OrderFrom <> ''''
    ),
    PeriodLabels AS (
        SELECT DISTINCT MonthOffsetLabel 
        FROM T_Calc_Order_Firm 
        WHERE Periode = @Periode_ID
    ),
    BaseData AS (
        SELECT 
            a.OrderFrom,
            CASE a.OrderFrom
                WHEN ''SAP'' THEN 1 
                WHEN ''KAP'' THEN 2 
                WHEN ''TMMIN'' THEN 3 
                WHEN ''DCWA'' THEN 4 
                ELSE 5 
            END AS GroupOrder,
            a.EngineGrouping,
            a.EngineBase,
            a.MonthOffsetLabel,
            a.Qty
        FROM ActualData a

        UNION ALL

        SELECT 
            mof.OrderFrom,
            CASE mof.OrderFrom
                WHEN ''SAP'' THEN 1 
                WHEN ''KAP'' THEN 2 
                WHEN ''TMMIN'' THEN 3 
                WHEN ''DCWA'' THEN 4 
                WHEN ''KUO (DCWA)'' THEN 4
                ELSE 5 
            END AS GroupOrder,
            ''-'' AS EngineGrouping,
            ''-'' AS EngineBase,
            p.MonthOffsetLabel,
            0 AS Qty
        FROM ActiveOrderFroms mof
        CROSS JOIN PeriodLabels p
        WHERE NOT EXISTS (
            SELECT 1 FROM ActualData a WHERE a.OrderFrom = mof.OrderFrom
        )
    ),
    SummaryLabels AS (
        SELECT 
            EngineBase,
            EngineBase + '' - '' + STUFF((
                SELECT DISTINCT '', '' + b2.EngineGrouping
                FROM BaseData b2
                WHERE b2.EngineBase = b1.EngineBase 
                  AND b2.EngineBase <> ''-'' -- Baris dummy tidak boleh ikut dirangkai
                FOR XML PATH(''''), TYPE
            ).value(''.'', ''NVARCHAR(MAX)''), 1, 2, '''') AS CustomEngineGrouping
        FROM BaseData b1
        WHERE EngineBase <> ''-'' 
        GROUP BY EngineBase
    )
    SELECT 
        OrderFrom, GroupOrder, EngineGrouping, EngineBase, ' + @cols + N'
    FROM
    (
        SELECT OrderFrom, GroupOrder, EngineGrouping, EngineBase, MonthOffsetLabel, SUM(Qty) AS Qty
        FROM BaseData
        GROUP BY OrderFrom, GroupOrder, EngineGrouping, EngineBase, MonthOffsetLabel

        UNION ALL

        SELECT
            ''RESUME ENGINE'' AS OrderFrom,
            99 AS GroupOrder,
            sl.CustomEngineGrouping AS EngineGrouping, 
            b.EngineBase,
            b.MonthOffsetLabel,
            SUM(b.Qty) AS Qty
        FROM BaseData b
        JOIN SummaryLabels sl ON b.EngineBase = sl.EngineBase
        WHERE b.EngineBase <> ''-'' -- Jangan hitung baris dummy di Resume Engine
        GROUP BY sl.CustomEngineGrouping, b.EngineBase, b.MonthOffsetLabel
    ) src
    PIVOT
    (
        SUM(Qty)
        FOR MonthOffsetLabel IN (' + @cols + N')
    ) pvt
    ORDER BY GroupOrder, EngineGrouping;'

    EXEC sp_executesql 
        @stmt = @query, 
        @params = N'@Periode_ID VARCHAR(10)', 
        @Periode_ID = @Periode_ID;
END