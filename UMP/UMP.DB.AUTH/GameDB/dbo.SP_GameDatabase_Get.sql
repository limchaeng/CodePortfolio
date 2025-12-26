CREATE PROCEDURE [dbo].[SP_GameDatabase_Get]
AS
BEGIN
	SET NOCOUNT ON

    BEGIN TRY

		SELECT db_idx, db_name, db_server, db_connection_string
		FROM dbo.GameDatabase WITH(NOLOCK)

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
