CREATE PROCEDURE [dbo].[SP_AppVerifyCode_Get]
    @world_idn			INT
,	@all_get			BIT
,	@last_get_time		DATETIME2
,	@app_version		NVARCHAR(50)
,	@has_unknown		BIT
,	@new_unknown_list	tv_tinystr_list READONLY
AS 
BEGIN
    SET NOCOUNT ON

    BEGIN TRY

		DECLARE @curr_time DATETIME2 = GETDATE()

		-- insert 실패해도 무방하여 트랜젝션 안걸음

		-- validation_kind
		-- 	Unknown = 0,		// 서버에서 검출 : GMTool 에서 수동으로 kind 변경
		-- 	IsValid = 1,		// 유효함
		-- 	IsInvalid = 2,		// 유효하지 않음
		-- 	IsWarning = 3,		// 경고만 사용

		-- 새로 발견된 코드 추가
		IF @has_unknown = 1
		BEGIN
			INSERT INTO AppVerifyCode 
			(
				world_idn, verify_type, verify_code, validation_kind, app_version, deleted, created_time, update_time
			)
			SELECT @world_idn, n, str, 0, @app_version, 0, @curr_time, NULL
			FROM @new_unknown_list;
		END

		-- DB 시간
		SELECT @curr_time AS last_db_time

		-- 검증에 사용하는 타입 리턴
		SELECT verify_type, all_warning,fixed_valid_verify FROM dbo.AppVerifyUse WITH(NOLOCK) WHERE is_enabled = 1

		-- @last_get_time 이후로 변경된 데이터 : GMTool 에서 변경시 update_time 을 꼭 셋팅해줘야함)
		SELECT idx, verify_type, verify_code, validation_kind
		FROM dbo.AppVerifyCode
		WHERE 
			world_idn IN ( 0, @world_idn )
		AND deleted = 0
		AND ( @all_get = 1 OR ( update_time IS NOT NULL AND update_time > @last_get_time ) )
				
		-- @last_get_time 이후로 삭제된 데이터 : GMTool 에서 변경시 update_time 을 꼭 셋팅해줘야함)
		SELECT idx AS n 
		FROM dbo.AppVerifyCode WITH(NOLOCK)
		WHERE 
			world_idn IN ( 0, @world_idn )
		AND deleted = 1
		AND ( @all_get = 1 OR ( update_time IS NOT NULL AND update_time > @last_get_time ) )

        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrNum int, @ErrMsg nvarchar(4000);
        SELECT @ErrNum = ERROR_NUMBER()+100000, @ErrMsg = ERROR_PROCEDURE() + N'(' + CAST(ERROR_LINE()+7 AS NVARCHAR(100)) + N'): ' + ERROR_MESSAGE()
        IF @ErrNum < 160000
            PRINT 'ErrorCode:' + CONVERT(nvarchar, @ErrNum) + ', ' + @ErrMsg
        RETURN @ErrNum;
    END CATCH
END
