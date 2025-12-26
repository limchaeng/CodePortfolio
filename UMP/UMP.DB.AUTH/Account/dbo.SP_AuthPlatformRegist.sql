CREATE PROCEDURE [dbo].[SP_AuthPlatformRegist]
	@account_idx BIGINT,
	@platform_type SMALLINT,
	@platform_id NVARCHAR(200),
	@platform_key NVARCHAR(200),
	@platform_username NVARCHAR(200),
	@platform_extra NVARCHAR(MAX),
	@platform_migration_type SMALLINT,
	@platform_migration_id NVARCHAR(200),
	@platform_migration_key NVARCHAR(200),
	@server_time DATETIME2
AS
BEGIN
	SET NOCOUNT ON

    BEGIN TRY
        BEGIN TRAN AuthPlatformRegist

			DECLARE @result INT = 0
			,		@exists_account_idx BIGINT = 0
			
			-- result
			-- AlreadyRegistered	= -1,
			-- Success				= 0,
			-- MigrationSuccess		= 1,			

			-- 체크
			SELECT @exists_account_idx = account_idx
			FROM dbo.AuthPlatform WITH(NOLOCK)
			WHERE platform_type = @platform_type
			AND platform_id = @platform_id
			AND platform_key = @platform_key
			AND unregist_time IS NULL

			IF @exists_account_idx > 0 
				SET @result = -1

			IF @result = 0
			BEGIN
				-- 마이그레이션  연동
				IF @platform_migration_type <> 0 -- 0 = none, negative = internal platform
				BEGIN
					IF EXISTS( SELECT * FROM dbo.AuthPlatform WITH(NOLOCK) WHERE account_idx = @account_idx AND platform_type = @platform_migration_type AND platform_id = @platform_migration_id )
					BEGIN
						SET @result = 1

						UPDATE dbo.AuthPlatform
						SET platform_type = @platform_type
						,	platform_id = @platform_id
						,	platform_key = @platform_key
						,	platform_name = @platform_username
						,	platform_extra = @platform_extra
						,	regist_time = @server_time
						,	unregist_time = NULL
						,	withdrawal_time = NULL
						,	update_time = GETDATE()
						WHERE account_idx = @account_idx 
						AND platform_type = @platform_migration_type
						AND platform_id = @platform_migration_id
					END
				END

				-- 추가 연동
				If @result = 0
				BEGIN
					INSERT dbo.AuthPlatform
					(
						account_idx
					,	platform_type
					,	platform_id
					,	platform_key
					,	platform_name
					,	platform_extra
					,	regist_time
					,	unregist_time
					,	withdrawal_time
					,	created_time
					,	update_time
					)
					VALUES
					(
						@account_idx
					,	@platform_type
					,	@platform_id
					,	@platform_key
					,	@platform_username
					,	@platform_extra
					,	@server_time
					,	NULL
					,	NULL
					,	GETDATE()
					,	NULL
					)
				END
			END

			-- 리턴
			SELECT @result AS result, @exists_account_idx AS exists_account_idx

        COMMIT TRAN AuthPlatformRegist
        RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN AuthPlatformRegist
        DECLARE @ErrNum int, @ErrMsg nvarchar(4000);
        SELECT @ErrNum = ERROR_NUMBER()+100000, @ErrMsg = ERROR_PROCEDURE() + N'(' + CAST(ERROR_LINE() AS NVARCHAR(100)) + N'): ' + ERROR_MESSAGE()
        IF @ErrNum < 150000
            PRINT 'ErrorCode:' + CONVERT(nvarchar, @ErrNum) + ', ' + @ErrMsg
        RETURN @ErrNum;
    END CATCH	
END
