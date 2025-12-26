CREATE TABLE [dbo].[AppVerifyCode](
	[idx] [int] IDENTITY(1,1) NOT NULL,
	[world_idn] [int] NOT NULL,
	[verify_type] [tinyint] NOT NULL,
	[verify_code] [nvarchar](max) NOT NULL,
	[validation_kind] [tinyint] NOT NULL,
	[app_version] [nvarchar](50) NULL,
	[deleted] [bit] NOT NULL,
	[created_time] [datetime2](7) NOT NULL,
	[update_time] [datetime2](7) NULL,
 CONSTRAINT [PK_AppVerifyCodeV3] PRIMARY KEY CLUSTERED 
(
	[idx] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
