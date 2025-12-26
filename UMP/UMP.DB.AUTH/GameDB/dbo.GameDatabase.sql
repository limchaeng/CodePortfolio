CREATE TABLE [dbo].[GameDatabase]
(
	[db_idx] [int] NOT NULL,
	[db_name] [nvarchar](50) NOT NULL,
	[db_server] [nvarchar](100) NOT NULL,
	[db_connection_string] [nvarchar](MAX) NOT NULL,
    CONSTRAINT [PK_GameDatabase] PRIMARY KEY ([db_idx]),
)
