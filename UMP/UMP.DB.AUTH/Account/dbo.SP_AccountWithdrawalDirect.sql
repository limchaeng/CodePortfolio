CREATE PROCEDURE [dbo].[SP_AccountWithdrawalDirect]
	@account_idx BIGINT,
	@server_time DATETIME2
AS
BEGIN
	SET NOCOUNT ON

	BEGIN TRY
		BEGIN TRAN AccountWithdrawalDirect

			UPDATE dbo.AuthPlatform
			SET withdrawal_time = @server_time
			WHERE account_idx = @account_idx

			UPDATE dbo.AccountAuth
			SET withdrawal_execute_time = @server_time
			WHERE account_idx = @account_idx
			
			-- TODO other set

		COMMIT TRAN AccountWithdrawalDirect
		RETURN 0;
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN AccountWithdrawalDirect
		DECLARE @ErrNum int, @ErrMsg nvarchar(4000);
		SELECT @ErrNum = ERROR_NUMBER()+100000, @ErrMsg = ERROR_PROCEDURE() + N'(' + CAST(ERROR_LINE() AS NVARCHAR(100)) + N'): ' + ERROR_MESSAGE()
		IF @ErrNum < 150000
			PRINT 'ErrorCode:' + CONVERT(nvarchar, @ErrNum) + ', ' + @ErrMsg
		RETURN @ErrNum;
	END CATCH
END
