//////////////////////////////////////////////////////////////////////////
//
// DBConnection_MySql
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using UMF.Core;
using MySql.Data.MySqlClient;

namespace UMF.Database.MySql
{
	//------------------------------------------------------------------------	
	public class DBConnection_MySql : DBConnectionBase
	{
		string mConnectionString = "";

		// interface
		public override string ConnectionString { get { return mConnectionString; } }

		public DBConnection_MySql( DatabaseMain db)
			: base(db)
		{
		}

		//------------------------------------------------------------------------
		public override void UpdateConnectionString(DatabaseMain.DatabaseConfig config)
		{
			if( string.IsNullOrEmpty( config.CustomConnectionString ) )
			{
				string added_string = "";
				if( string.IsNullOrEmpty( config.CharacterSet ) == false )
					added_string += string.Format( ";Charset={0}", config.CharacterSet );

				mConnectionString = string.Format( "server={0};uid={1};pwd={2};database={3};MaxPoolSize={4}{5}",
					config.HostIP, config.ID, config.Password, config.DatabaseName, config.PoolSize + 1, added_string );
			}
			else
			{
				mConnectionString = config.CustomConnectionString;
			}
		}

		//------------------------------------------------------------------------
		public override void TestConnection()
		{
			using( MySqlConnection connection = new MySqlConnection( mConnectionString ) )
			{
				try
				{
					connection.Open();
					string ver = connection.ServerVersion;
					Log.WriteDB( "Connected to MySQL Server {0} Version {1}", connection.DataSource, ver );
					Log.WriteDB( "" );
					return;
				}
				catch( System.Exception ex )
				{
					connection.Close();

					Log.WriteError( "Connect failed : " + ex.ToString() );
					Log.WriteError( "" );

					throw ex;
				}
			}
		}

		//------------------------------------------------------------------------
		public override ISqlCommand CreateSqlCommand( bool is_sync )
		{
			MySqlConnection connection = _OpenConnection( is_sync );
			if( connection != null )
			{
				return new SqlCommand_MySql( connection );
			}

			return null;
		}
		MySqlConnection _OpenConnection(bool is_sync)
		{
			MySqlConnection connection = new MySqlConnection( mConnectionString );

			if( mDatabase.Config.IgnoreConnectionException == false )
			{
				connection.Open();
			}
			else
			{
				try
				{
					connection.Open();
				}
				catch( System.Exception ex )
				{
					string ex_msg = string.Format( "OpenConnection Exception:{0}", ex.Message );
					Log.WriteError( ex_msg );
					Log.SendNotification( ex_msg );
					if( connection != null )
						connection.Close();

					connection = null;
				}
			}

			return connection;
		}

		//------------------------------------------------------------------------
		public override int ExecuteCommandText( string text )
		{
			MySqlConnection connection = _OpenConnection( true );
			if( connection != null )
			{
				MySqlCommand cmd = new MySqlCommand( text, connection );
				return cmd.ExecuteNonQuery();
			}

			return -1;
		}

		//------------------------------------------------------------------------
		public MySqlCommand CreateCommand( string strProcedure )
		{
			MySqlCommand cmd = new MySqlCommand( strProcedure );
			cmd.CommandType = System.Data.CommandType.StoredProcedure;
			cmd.CommandTimeout = mDatabase.Config.CommandTimeout;

			return cmd;
		}
	}
}
