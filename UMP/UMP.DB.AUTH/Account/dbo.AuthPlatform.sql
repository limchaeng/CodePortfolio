CREATE TABLE [dbo].[AuthPlatform]
(
	[account_idx] [bigint] NOT NULL,
	[platform_type] [smallint] NOT NULL,
	[platform_id] [nvarchar](200) NOT NULL,
	[platform_key] [nvarchar](200) NOT NULL,
	[platform_name] [nvarchar](200) NOT NULL,
	[platform_extra] [nvarchar](MAX) NOT NULL,
	[regist_time] [datetime2](7) NOT NULL,
	[unregist_time] [datetime2](7) NULL, 
	[withdrawal_time] [datetime2](7) NULL,
	[created_time] [datetime2](7) NOT NULL,
	[update_time] [datetime2](7) NULL 
)

GO


CREATE INDEX [IX_AuthPlatform_account_idx] ON [dbo].[AuthPlatform] (account_idx)
