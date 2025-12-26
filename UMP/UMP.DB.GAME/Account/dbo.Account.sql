CREATE TABLE [dbo].[Account]
(
	[account_idx] [bigint] NOT NULL PRIMARY KEY,
	[account_code] [nvarchar](10) NOT NULL,
	[nickname] [nvarchar](100) NOT NULL,
	[game_db_idx] [int] NOT NULL,
	[application_identifier] [nvarchar](50) NOT NULL,
	[device_type] [nvarchar](50) NOT NULL,
	[device_id] [nvarchar](50) NOT NULL,	
	[device_location] [nvarchar](10) NOT NULL,
	[device_os_version] [nvarchar](30) NOT NULL,
	[device_language] [nvarchar](50) NOT NULL,	
	[device_advertising_id] [nvarchar](200) NOT NULL,
	[device_package_id] [nvarchar](50) NOT NULL,	
	[client_version] [nvarchar](20) NOT NULL,	
	[security_flags] [int] NOT NULL,
	[special_flags] [int] NOT NULL,
	[agreement_flags] [tinyint] NOT NULL,
	[last_login_time] [datetime2](7) NULL,
	[last_logout_time] [datetime2](7) NULL,
	[created_time] [datetime2](7) NOT NULL,
	[update_time] [datetime2](7) NULL, 
)


