USE [RFIDP2P3_Anbunka]
GO

/****** Object:  Table [dbo].[M_UserAuth]    Script Date: 27/02/2026 08:07:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[M_UserAuth](
	[UserID] [nvarchar](50) NOT NULL,
	[Secret] [varchar](64) NULL,
	[OTP] [nvarchar](5) NULL,
	[Auth_Attempt] [int] NULL,
	[Auth_Attempt_Time] [datetime] NULL,
	[Auth_Attempt_Flag] [int] NULL,
	[OTP_Attempt] [int] NULL,
	[OTP_Attempt_Time] [datetime] NULL,
	[OTP_Attempt_Flag] [int] NULL,
	[Email_Attempt] [int] NULL,
	[Email_Attempt_Time] [datetime] NULL,
	[Email_attempt_Flag] [int] NULL,
	[is_Logged_In] [int] NULL,
	[is_Logged_In_Date] [datetime] NULL,
	[exp_token] [nvarchar](100) NULL,
 CONSTRAINT [PK_T_User_Auth] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


