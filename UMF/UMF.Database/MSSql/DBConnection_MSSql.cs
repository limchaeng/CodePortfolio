//////////////////////////////////////////////////////////////////////////
//
// DBConnection_MSSql
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

using System.Data.SqlClient;
using UMF.Core;

namespace UMF.Database.MSSql
{
	//------------------------------------------------------------------------	
	public class DBConnection_MSSql : DBConnectionBase
	{
		protected string mConnectionString = "";

		// interface
		public override string ConnectionString { get { return mConnectionString; } }

		public DBConnection_MSSql(DatabaseMain db)
			: base(db)
		{
		}

		//------------------------------------------------------------------------
		public override void UpdateConnectionString(DatabaseMain.DatabaseConfig config)
		{
			if( string.IsNullOrEmpty( config.CustomConnectionString ) )
			{
				mConnectionString = string.Format( "Server={0};UID={1};Password={2};Database={3};Asynchronous Processing={4};Max Pool Size={5};TimeOut={6};",
					config.HostIP, config.ID, config.Password, config.DatabaseName, config.Async, config.PoolSize + 1, config.Timeout );
			}
			else
			{
				mConnectionString = config.CustomConnectionString;
			}
		}

		//------------------------------------------------------------------------
		public override void TestConnection()
		{
			using( SqlConnection connection = new SqlConnection( mConnectionString ) )
			{
				try
				{
					connection.Open();
					string ver = connection.ServerVersion;
					Log.WriteDB( "Connected to MSSQL Server {0} Version {1}", connection.DataSource, ver );
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
			SqlConnection connection = _OpenConnection( is_sync );
			if( connection != null )
			{
				return new SqlCommand_MSSql( connection );
			}

			return null;
		}
		SqlConnection _OpenConnection(bool is_sync)
		{
			SqlConnection connection = new SqlConnection( mConnectionString );

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
			SqlConnection connection = _OpenConnection( true );
			if( connection != null )
			{
				SqlCommand cmd = new SqlCommand( text, connection );
				return cmd.ExecuteNonQuery();
			}

			return -1;
		}

		//------------------------------------------------------------------------
		public SqlCommand CreateCommand( string strProcedure )
		{
			SqlCommand cmd = new SqlCommand( strProcedure );
			cmd.CommandType = System.Data.CommandType.StoredProcedure;
			cmd.CommandTimeout = mDatabase.Config.CommandTimeout;
			return cmd;
		}
	}
}
