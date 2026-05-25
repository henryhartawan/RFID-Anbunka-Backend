USE [RFIDP2P3_Anbunka]
GO

/****** Object:  Table [dbo].[T_Status_Capacity]    Script Date: 5/21/2026 11:27:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[T_Status_Capacity](
	[Calc_Groups] [nvarchar](50) NOT NULL,
	[Calc_Status] [nvarchar](50) NULL,
	[Error_Message] [nvarchar](200) NULL,
	[Periode_ID] [nvarchar](8) NULL,
	[Calc_Date] [datetime] NULL,
	[Calc_By] [nvarchar](50) NULL
) ON [PRIMARY]
GO


