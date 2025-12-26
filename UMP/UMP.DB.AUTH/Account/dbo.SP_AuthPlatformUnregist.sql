CREATE PROCEDURE [dbo].[SP_AuthPlatformUnregist]
	@account_idx BIGINT,
	@platform_type SMALLINT,
	@platform_id NVARCHAR(200),
	@server_time DATETIME2
AS
BEGIN
	SET NOCOUNT ON

    BEGIN TRY
        BEGIN TRAN AuthPlatformUnregist

			UPDATE dbo.AuthPlatform
			SET unregist_time = @server_time
			,	update_time = GETDATE()
			WHERE account_idx = @account_idx
			AND platform_type = @platform_type
			AND platform_id = @platform_id

        COMMIT TRAN AuthPlatformUnregist
        RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN AuthPlatformUnregist
        DECLARE @ErrNum int, @ErrMsg nvarchar(4000);
        SELECT @ErrNum = ERROR_NUMBER()+100000, @ErrMsg = ERROR_PROCEDURE() + N'(' + CAST(ERROR_LINE() AS NVARCHAR(100)) + N'): ' + ERROR_MESSAGE()
        IF @ErrNum < 150000
            PRINT 'ErrorCode:' + CONVERT(nvarchar, @ErrNum) + ', ' + @ErrMsg
        RETURN @ErrNum;
    END CATCH	
END
