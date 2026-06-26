USE [RFIDP2P3_Anbunka]
GO

CREATE TABLE [dbo].[T_Target_Stock_KLine] (
    [TargetKLineID] [int] IDENTITY(1,1) NOT NULL,
    [Periode] [nvarchar](7) NOT NULL,
    [OrderFrom] [nvarchar](50) NOT NULL,
    [UniqueCode] [nvarchar](50) NOT NULL,
    [EngineType] [nvarchar](50) NULL,
	[EngineBase] [nvarchar](100) NULL,
    [PartName] [nvarchar](150) NULL,
    [LoadingRatio] [decimal](8, 1) NULL,
    [VolMonthly] [int] NOT NULL DEFAULT 0,
    [VolDaily] [int] NOT NULL DEFAULT 0,
    [Trip] [int] NOT NULL DEFAULT 0,
    [BufferHours] [decimal](5, 2) NULL,
    [OrderCycle] [decimal](5, 2) NULL,
    
    [TotalMinim] [int] NOT NULL DEFAULT 0,
    [TotalMax] [int] NOT NULL DEFAULT 0,
    
    [CreatedAt] [datetime] DEFAULT GETDATE(),
    [CreatedBy] [nvarchar](50),
    [UpdatedAt] [datetime] NULL,
    [UpdatedBy] [nvarchar](50) NULL,
    CONSTRAINT [PK_T_Target_Stock_KLine] PRIMARY KEY CLUSTERED ([TargetKLineID] ASC)
) ON [PRIMARY];
GO

CREATE NONCLUSTERED INDEX [IX_T_Target_Stock_KLine_Periode] ON [dbo].[T_Target_Stock_KLine] ([Periode], [UniqueCode]);
GO