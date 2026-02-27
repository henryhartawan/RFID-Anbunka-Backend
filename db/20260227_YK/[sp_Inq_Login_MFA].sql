USE [RFIDP2P3_Anbunka]
GO
/****** Object:  StoredProcedure [dbo].[sp_Inq_Login_MFA]    Script Date: 27/02/2026 08:46:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:	MIT
-- Description:	sp_Inq_Login_MFA
-- =============================================
ALTER PROCEDURE [dbo].[sp_Inq_Login_MFA] 
	@Ftype		NVARCHAR(30),
	@userLogin	NVARCHAR(50) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF @Ftype = 'GetEmailUser'
	BEGIN
		SELECT	Email 
		FROM	M_UserSetup 
		WHERE	UserID = @userLogin;
	END
	ELSE IF @Ftype = 'GetSecretKey'
	BEGIN
		SELECT Secret FROM M_UserAuth WHERE UserID = @userLogin
	END
	ELSE IF @Ftype = 'GetUserData'
	BEGIN
		SELECT	a.UserID AS U_PIC_ID,
				a.UserName AS U_PIC_Name,
				a.Email,
				b.OTP
		FROM M_UserSetup a 
				LEFT OUTER JOIN M_UserAuth b ON a.UserID = b.UserID
		WHERE a.UserID = @userLogin
	END
END
