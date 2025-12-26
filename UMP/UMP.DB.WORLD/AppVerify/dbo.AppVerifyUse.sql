CREATE TABLE [dbo].[AppVerifyUse](
	[verify_type] [tinyint] NOT NULL,
	[all_warning] [bit] NOT NULL,
	[fixed_valid_verify] [bit] NOT NULL,
	[is_enabled] [bit] NOT NULL,
	[update_time] [datetime2](7) NULL
) ON [PRIMARY]