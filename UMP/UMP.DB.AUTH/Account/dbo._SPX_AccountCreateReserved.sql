CREATE PROCEDURE [dbo].[SPX_AccountCreateReserved]
    @begin_idx		INT
,   @end_idx		INT
,   @game_db_idx	INT
AS
    DECLARE
        @exist_count INT = 0

    BEGIN TRY

        SELECT @exist_count = COUNT(account_idx)
        FROM AccountAuth
        WHERE account_idx >= @begin_idx

        IF( @exist_count > 0 )
            THROW 70001, 'already exist seq', 1;

        INSERT dbo.AccountAuth
        (
            account_idx
        ,   game_db_idx
        )
        SELECT
            B.RN
        ,   @game_db_idx
        FROM
        (
            SELECT A.RN
            FROM
            (
                SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RN 
                FROM sys.sysobjects A, sys.sysobjects B, sys.sysobjects C, sys.sysobjects D
            ) A
            WHERE
                A.RN >= @begin_idx
            AND A.RN <= @end_idx
        ) B

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
RETURN 0
