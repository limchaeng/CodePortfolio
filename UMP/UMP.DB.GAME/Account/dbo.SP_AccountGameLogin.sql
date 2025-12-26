CREATE PROCEDURE [dbo].[SP_AccountGameLogin]
	@account_idx BIGINT,
	@account_code NVARCHAR(10),
	@game_db_idx INT,
	@application_identifier NVARCHAR(50),
	@device_type NVARCHAR(50),
	@device_id NVARCHAR(50),
	@device_location NVARCHAR(10),
	@device_os_version NVARCHAR(30),
	@device_language NVARCHAR(50),
	@device_advertising_id NVARCHAR(200),
	@device_package_id NVARCHAR(50),
	@client_version NVARCHAR(20),
	@server_time DATETIME2,
	@is_new_account BIT,
	@use_multiple_player BIT
AS
BEGIN
	SET NOCOUNT ON

	BEGIN TRY
		BEGIN TRAN AccountGameLogin

            MERGE dbo.Account AS target
            USING (SELECT @account_idx) AS source (account_idx)    
            ON (target.account_idx = source.account_idx)
            WHEN MATCHED THEN
                UPDATE 
                SET account_code			= @account_code
				,	game_db_idx				= @game_db_idx
				,	application_identifier	= @application_identifier
				,	device_type				= @device_type
				,	device_id				= @device_id
				,	device_location			= @device_location
				,	device_os_version		= @device_os_version
				,	device_language			= @device_language
				,	device_advertising_id	= @device_advertising_id
				,	device_package_id		= @device_package_id
				,	client_version			= @client_version
				,	last_login_time			= @server_time
				,	update_time				= GETDATE()
            WHEN NOT MATCHED THEN    
                INSERT    
                (
					account_idx
				,	account_code
				,	nickname
				,	game_db_idx
				,	application_identifier
				,	device_type
				,	device_id
				,	device_location
				,	device_os_version
				,	device_language
				,	device_advertising_id
				,	device_package_id
				,	client_version
				,	security_flags
				,	special_flags
				,	agreement_flags
				,	last_login_time
				,	last_logout_time
				,	created_time
				,	update_time
                )
                VALUES    
                (
                    source.account_idx
				,	@account_code
				,	N''
				,	@game_db_idx
				,	@application_identifier
				,	@device_type
				,	@device_id
				,	@device_location
				,	@device_os_version
				,	@device_language
				,	@device_advertising_id
				,	@device_package_id
				,	@client_version
				,	0
				,	0
				,	0
				,	@server_time
				,	NULL
				,	GETDATE()
				,	NULL
                );
			
			IF @@ERROR != 0
				THROW 50013, 'AccountLoginRecordError', 1

			-- 멀티플레이어를 지원하지 않으면 기본 플레이어(player idx = 1)를 추가해준다. v2
			IF @use_multiple_player = 0
			BEGIN
				MERGE dbo.Player AS target
				USING ( SELECT @account_idx ) AS source( account_idx )
				ON ( target.account_idx = source.account_idx AND target.player_idx = 1 )
				WHEN MATCHED THEN
					UPDATE
					SET nickname			= N''
					,	last_logout_time	= NULL
					,	total_playing_time	= 0
					,	special_flags		= 0
					,	deleted_time		= NULL
					,	update_time			= GETDATE()
				WHEN NOT MATCHED THEN
					INSERT
					(
						account_idx
					,	player_idx
					,	nickname
					,	last_logout_time
					,	total_playing_time
					,	special_flags
					,	deleted_time
					,	created_time
					,	update_time
					)
					VALUES
					(
						@account_idx
					,	1
					,	N''
					,	NULL
					,	0
					,	0
					,	NULL
					,	GETDATE()
					,	NULL
					);
			END

			-- 리턴 계정 정보
			SELECT nickname
			,	agreement_flags
			,	security_flags
			,	special_flags
			,	created_time
			FROM dbo.Account WITH(NOLOCK)
			WHERE account_idx = @account_idx 

			-- 리턴 플레이어 정보
			SELECT player_idx
			,	nickname
			,	last_logout_time
			,	total_playing_time
			FROM dbo.Player WITH(NOLOCK)
			WHERE account_idx = @account_idx
			AND deleted_time IS NULL


		COMMIT TRAN AccountGameLogin
		RETURN 0;
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN AccountGameLogin
		DECLARE @ErrNum int, @ErrMsg nvarchar(4000);
		SELECT @ErrNum = ERROR_NUMBER()+100000, @ErrMsg = ERROR_PROCEDURE() + N'(' + CAST(ERROR_LINE() AS NVARCHAR(100)) + N'): ' + ERROR_MESSAGE()
		IF @ErrNum < 150000
			PRINT 'ErrorCode:' + CONVERT(nvarchar, @ErrNum) + ', ' + @ErrMsg
		RETURN @ErrNum;
	END CATCH
END
