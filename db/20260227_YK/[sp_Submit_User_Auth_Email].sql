USE [RFIDP2P3_Anbunka]
GO
/****** Object:  StoredProcedure [dbo].[sp_Submit_User_Auth_Email]    Script Date: 27/02/2026 08:09:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Submit_User_Auth_Email]

@U_PIC_ID nvarchar(50),
@OTP nvarchar(10)
		
AS
BEGIN
	SET NOCOUNT ON;

	/*
		Email_Attempt_Flag explained: 
		1 = 5 minutes delay
		2 = 10 minutes delay
		3 = 30 minutes delay
		4 = 1 hour delay
	*/

	DECLARE @Email_Attempt int = (SELECT Email_Attempt FROM M_UserAuth WHERE UserID = @U_PIC_ID)
	DECLARE @Email_Attempt_Date datetime = (SELECT Email_Attempt_Time FROM M_UserAuth WHERE UserID = @U_PIC_ID)
	DECLARE @Email_Attempt_Flag int = (SELECT Email_attempt_Flag FROM M_UserAuth WHERE UserID = @U_PIC_ID)

	DECLARE @Delay_In_Minutes int
    DECLARE @Elapsed_Minutes int
	DECLARE @Remaining_Minutes int

	SET @Delay_In_Minutes = CASE 
        WHEN @Email_Attempt_Flag = 1 THEN 5
        WHEN @Email_Attempt_Flag = 2 THEN 10
        WHEN @Email_Attempt_Flag = 3 THEN 30
        WHEN @Email_Attempt_Flag = 4 THEN 60
        ELSE 0 -- Default to no delay if the flag is invalid
    END

	SET @Elapsed_Minutes = DATEDIFF(MINUTE, @Email_Attempt_Date, GETDATE())
	SET @Remaining_Minutes = @Delay_In_Minutes - DATEDIFF(MINUTE, @Email_Attempt_Date, GETDATE());

	IF NOT EXISTS ((Select UserID FROM M_UserAuth WHERE UserID = @U_PIC_ID))
	BEGIN
		INSERT INTO M_UserAuth (UserID, OTP, Email_Attempt, Email_Attempt_Time, Email_attempt_Flag) VALUES (@U_PIC_ID, @OTP, 1, GETDATE(), 0)
		SELECT 'Email has been send, please check your inbox'  AS Remarks, 0  AS Validation_Flag
		--add funcion to trigger send email 
	END
	ELSE IF EXISTS ((SELECT UserID FROM M_UserAuth WHERE UserID = @U_PIC_ID))
	BEGIN
		IF (@Elapsed_Minutes < @Delay_In_Minutes)
		BEGIN
			SELECT 'Please wait ' + CAST(@Remaining_Minutes as nvarchar) + ' minutes before attempting to send an email again.'  AS Remarks, 1  AS Validation_Flag
		END
		ELSE 
		BEGIN
			IF (@Email_Attempt < 5)
			BEGIN
				UPDATE M_UserAuth SET OTP = @OTP, Email_Attempt = @Email_Attempt + 1, Email_Attempt_Time = GETDATE() WHERE UserID = @U_PIC_ID
				--add funcion to trigger send email 
				SELECT 'Email has been send, please check your inbox'  AS Remarks, 0  AS Validation_Flag
			END
			ELSE
			BEGIN
				UPDATE M_UserAuth SET Email_attempt_Flag = @Email_Attempt_Flag + 1 WHERE UserID = @U_PIC_ID
				SELECT 'Email has been send, please check your inbox'  AS Remarks, 0  AS Validation_Flag
			END
		END
	END
END
