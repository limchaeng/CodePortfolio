CREATE PROCEDURE [dbo].[SP_Common_AccountLogin]
	@account_idx BIGINT,
	@game_db_idx INT
AS
BEGIN
	SET NOCOUNT ON

	BEGIN TRY
		BEGIN TRAN Common_AccountLogin

		COMMIT TRAN Common_AccountLogin
		RETURN 0;
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN Common_AccountLogin
		DECLARE @ErrNum int, @ErrMsg nvarchar(4000);
		SELECT @ErrNum = ERROR_NUMBER()+100000, @ErrMsg = ERROR_PROCEDURE() + N'(' + CAST(ERROR_LINE() AS NVARCHAR(100)) + N'): ' + ERROR_MESSAGE()
		IF @ErrNum < 150000
			PRINT 'ErrorCode:' + CONVERT(nvarchar, @ErrNum) + ', ' + @ErrMsg
		RETURN @ErrNum;
	END CATCH
END
