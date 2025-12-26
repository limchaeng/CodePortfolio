CREATE TABLE [dbo].[Player]
(
	[account_idx] bigint not null ,
	[player_idx] int not null ,
	[nickname] [nvarchar](100) NOT NULL,
	[last_logout_time] [datetime2](7) NULL,
	[total_playing_time] [bigint] NOT NULL,
	[special_flags] [int] NOT NULL,
	[deleted_time] [datetime2] NULL,
	[created_time] [datetime2](7) NOT NULL,
	[update_time] [datetime2](7) NULL, 
    CONSTRAINT [PK_Player] PRIMARY KEY ([account_idx], [player_idx]), 
)
