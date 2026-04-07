USE [RFIDP2P3_DB]
GO

/****** Object:  Table [dbo].[T_SKID]    Script Date: 31/03/2026 20:23:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[T_SKID](
	[SKID_ID] [nvarchar](50) NOT NULL,
	[KanbanNo] [nvarchar](50) NOT NULL,
	[PI_No] [nvarchar](50) NOT NULL,
	[DN_No] [nvarchar](24) NOT NULL,
	[Part_No] [nvarchar](50) NOT NULL,
	[ExCore] [nvarchar](50) NOT NULL,
	[Job_No] [nvarchar](50) NOT NULL,
	[KanbanSeq] [nvarchar](3) NOT NULL,
	[ScanBy] [nvarchar](50) NOT NULL,
	[ScanDate] [datetime] NOT NULL,
 CONSTRAINT [PK_T_SKID] PRIMARY KEY CLUSTERED 
(
	[SKID_ID] ASC,
	[KanbanNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


