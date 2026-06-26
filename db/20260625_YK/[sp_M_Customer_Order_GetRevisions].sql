USE [RFIDP2P3_Anbunka]
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Customer_Order_GetRevisions]    Script Date: 6/25/2026 9:42:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:	MIT
-- Description: [sp_M_Customer_Order_GetRevisions]
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_M_Customer_Order_GetRevisions]
    @Periode NVARCHAR(7)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT RevisionNo 
    FROM M_Customer_Order 
    WHERE Periode = @Periode
    ORDER BY RevisionNo ASC;
END
GO