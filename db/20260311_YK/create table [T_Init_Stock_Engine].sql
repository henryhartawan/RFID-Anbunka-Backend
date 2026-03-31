CREATE TABLE [dbo].[T_Init_Stock_Engine](
    [StockEngineId] INT IDENTITY(1,1) PRIMARY KEY,
    [Periode_ID] [nvarchar](8) NOT NULL,
    [UniqueCode] [nvarchar](20) NOT NULL,
    [QtyLastStock] [float] NULL,
    [Calc_By] [nvarchar](50) NULL,
    [Calc_Date] [datetime] NULL
) ON [PRIMARY]
GO