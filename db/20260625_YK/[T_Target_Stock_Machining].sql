CREATE TABLE [dbo].[T_Target_Stock_Machining] (
    [TargetMachID] [int] IDENTITY(1,1) NOT NULL,
    [Periode] [nvarchar](7) NOT NULL,
    [EngineBase] [nvarchar](50) NOT NULL,
    [EngineType] [nvarchar](50) NULL,
    [PartName] [nvarchar](150) NULL,
    [UniqueCode] [nvarchar](50) NOT NULL,
    
    [PlanMonthly] [int] NOT NULL DEFAULT 0,
    [PlanDaily] [int] NOT NULL DEFAULT 0,
    
    [MinDay] [int] NOT NULL DEFAULT 0,
    [MinUnit] [int] NOT NULL DEFAULT 0,
    
    [StdDay] [int] NOT NULL DEFAULT 0,
    [StdUnit] [int] NOT NULL DEFAULT 0,
    
    [MaxDay] [int] NOT NULL DEFAULT 0,
    [MaxUnit] [int] NOT NULL DEFAULT 0,
    
    [AdvanceUnit] [int] NOT NULL DEFAULT 0,
    
    [CreatedAt] [datetime] DEFAULT GETDATE(),
    [CreatedBy] [nvarchar](50),
    [UpdatedAt] [datetime] NULL,
    [UpdatedBy] [nvarchar](50) NULL,
    CONSTRAINT [PK_T_Target_Stock_Machining] PRIMARY KEY CLUSTERED ([TargetMachID] ASC)
) ON [PRIMARY];
GO

CREATE NONCLUSTERED INDEX [IX_T_Target_Stock_Machining_Periode] ON [dbo].[T_Target_Stock_Machining] ([Periode], [UniqueCode]);
GO