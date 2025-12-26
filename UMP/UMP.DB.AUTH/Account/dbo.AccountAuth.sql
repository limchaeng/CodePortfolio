CREATE TABLE [dbo].[AccountAuth]
(
	[account_idx] [bigint] NOT NULL,
	[game_db_idx][int] NOT NULL,
	[last_auth_platform_type] [smallint] NULL,
	[last_auth_platform_id] [nvarchar](200) NULL,
	[block_expire_time] [datetime2](7) NULL,
	[withdrawal_request_time] [datetime2](7) NULL,
	[withdrawal_execute_time] [datetime2](7) NULL,
	[withdrawal_process_time] [datetime2](7) NULL,
	[created_time] [datetime2](7) NULL,
	[update_time] [datetime2](7) NULL, 
    CONSTRAINT [PK_AccountAuth] PRIMARY KEY ([account_idx]), 
)


