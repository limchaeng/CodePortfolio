CREATE PROCEDURE [dbo].[SP_AccountAuth]
	@platform_type SMALLINT,
	@platform_id NVARCHAR(200),
	@platform_key NVARCHAR(200),
	@platform_username NVARCHAR(200),
	@platform_extra NVARCHAR(MAX),
	@platform_migration_type SMALLINT,
	@platform_migration_id NVARCHAR(200),
	@platform_migration_key NVARCHAR(200),
	@is_auto_regist BIT,
	@check_exist_throw BIT,
	@server_time DATETIME2
AS
BEGIN
    SET NOCOUNT ON
  
    BEGIN TRY
        BEGIN TRAN AccountAuth

			DECLARE
                @account_idx BIGINT = 0
			,	@is_new_account	BIT = 0

			SELECT
				@account_idx = account_idx
			FROM dbo.AuthPlatform WITH(NOLOCK)
			WHERE
				platform_type = @platform_type
			AND platform_id = @platform_id
			AND platform_key = @platform_key
			AND unregist_time IS NULL
			AND withdrawal_time IS NULL

			IF @check_exist_throw = 1 AND @account_idx > 0 
				THROW 50011, 'Account auth id exist', 1

			-- 기존 정보 없으면 마이그레이션 시도
			IF @account_idx = 0 AND @platform_migration_type <> 0		-- 0 = none, negative = internal platform
			BEGIN
				SELECT
					@account_idx = account_idx
				FROM dbo.AuthPlatform WITH(NOLOCK)
				WHERE 
					platform_type = @platform_migration_type
				AND platform_id = @platform_migration_id
				AND platform_key = @platform_migration_key

				-- 마이그레이션 타입 존재하면 업데이트
				If @account_idx > 0 
				BEGIN
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

			-- 계정이 없고 자동 등록이 비활성 되어 있으면
			IF @account_idx = 0 AND @is_auto_regist = 0
				THROW 50012, 'AccountNotFound', 1

			-- 자동 등록
			IF @account_idx = 0
			BEGIN
				SET @is_new_account = 1

				SELECT TOP 1 @account_idx = account_idx
				FROM dbo.AccountAuth WITH(NOLOCK)
				WHERE created_time IS NULL
				ORDER BY account_idx

				UPDATE dbo.AccountAuth
				SET last_auth_platform_type = @platform_type
				,	last_auth_platform_id = @platform_id
				,	created_time = @server_time
				WHERE account_idx = @account_idx

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
			ELSE
			BEGIN
				UPDATE dbo.AccountAuth
				SET last_auth_platform_type = @platform_type
				,	last_auth_platform_id = @platform_id
				,	update_time = GETDATE()
				WHERE account_idx = @account_idx
			END

			-- 리턴
			SELECT 
				account_idx
			,	@is_new_account AS is_new_account
			,	game_db_idx
			,	block_expire_time
			,	withdrawal_process_time
			FROM dbo.AccountAuth WITH(NOLOCK) 
			WHERE account_idx = @account_idx

			-- 리턴 플렛폼 연동 정보
			SELECT
				platform_type
			,	platform_id
			,	platform_name
			,	platform_extra
			,	regist_time
			FROM dbo.AuthPlatform WITH(NOLOCK)			
			WHERE account_idx = @account_idx
			AND unregist_time IS NULL
			AND withdrawal_time IS NULL

        COMMIT TRAN AccountAuth
        RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN AccountAuth
        DECLARE @ErrNum int, @ErrMsg nvarchar(4000);
        SELECT @ErrNum = ERROR_NUMBER()+100000, @ErrMsg = ERROR_PROCEDURE() + N'(' + CAST(ERROR_LINE() AS NVARCHAR(100)) + N'): ' + ERROR_MESSAGE()
        IF @ErrNum < 150000
            PRINT 'ErrorCode:' + CONVERT(nvarchar, @ErrNum) + ', ' + @ErrMsg
        RETURN @ErrNum;
    END CATCH
END
