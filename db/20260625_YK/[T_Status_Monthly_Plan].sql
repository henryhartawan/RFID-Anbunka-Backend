USE [RFIDP2P3_Anbunka]
GO

/****** Object:  Table [dbo].[T_Status_Monthly_Plan]    Script Date: 7/1/2026 1:58:35 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[T_Status_Monthly_Plan](
	[Calc_Groups] [nvarchar](50) NOT NULL,
	[Calc_Status] [nvarchar](50) NULL,
	[Error_Message] [nvarchar](200) NULL,
	[Periode_ID] [nvarchar](8) NULL,
	[Calc_Date] [datetime] NULL,
	[Calc_By] [nvarchar](50) NULL
) ON [PRIMARY]
GO


