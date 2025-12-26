//////////////////////////////////////////////////////////////////////////
//
// DatabaseMain
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

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UMF.Core;

namespace UMF.Database
{
	public class DatabaseMain
	{
		public enum eDBMS
		{
			LocalMDF,
			MSSql,
			MySql,
		}

		public class DatabaseConfig : EnvConfig
		{
			public bool IsThrowSqlException = true;
			public eCoreLogType CoreLogType = eCoreLogType.Detail;
			public bool UseParallel = true;
			public bool UseConcurrentWaitQueue = true;
			public bool IgnoreConnectionException = true;
			public bool SqlExceptionNotify = true;
			public int QueryTooLongTimeMilli = 5000;

			public eDBMS DBMS = eDBMS.MSSql;
			public string HostIP = "";
			public string ID = "";
			public string Password = "";
			public string DatabaseName = "";
			public int PoolSize = 400;
			//
			public int Timeout = 10;
			public bool Async = true;
			public int CommandTimeout = 60;
			//
			public string CharacterSet = "";
			// 
			public string CustomConnectionString = "";

			protected override void ConfigLoad()
			{
				base.ConfigLoad();
				if( UseParallel == false )
					UseConcurrentWaitQueue = false;
			}
		}
		DatabaseConfig mConfig = new DatabaseConfig();
		public DatabaseConfig Config { get { return mConfig; } }
		public bool DBEnabled { get; set; }

		DBConnectionBase mDBConnection = null;
		public DBConnectionBase DBConnection { get { return mDBConnection; } }

		Queue<DBHandlerObject> mSessionPacketHandlers = new Queue<DBHandlerObject>();
		Queue<DBHandlerObject> mWaitProcedureHandlers = new Queue<DBHandlerObject>();
 		ConcurrentQueue<DBHandlerObject> mConcurrentWaitProcedureHandlers = new ConcurrentQueue<DBHandlerObject>();
 		Queue<DBHandlerObject> mProcedureHandlers = new Queue<DBHandlerObject>();
		
		public DatabaseMain( string config_file )
		{
			DBEnabled = true;
			mConfig.ConfigLoad( GlobalConfig.EnvDBPath( config_file ) );

			switch(mConfig.DBMS)
			{
				case eDBMS.LocalMDF:
					mDBConnection = new MSSql.DBConnection_LocalMDF( this );
					break;

				case eDBMS.MSSql:
					mDBConnection = new MSSql.DBConnection_MSSql( this );
					break;

				case eDBMS.MySql:
					mDBConnection = new MySql.DBConnection_MySql( this );
					break;
			}

			Log.WriteDB( mConfig.ToString() );
			Log.WriteDB( "" );
		}

		//------------------------------------------------------------------------
		public void UpdateConnectionInfo()
		{
			mDBConnection.UpdateConnectionString( mConfig );
			Log.WriteDB( "# Updated : {0}", mDBConnection.ConnectionString );
			Log.WriteDB( "" );
		}

		//------------------------------------------------------------------------		
		public void AddSessionPacketHandler( DBHandlerObject handler )
		{
			mSessionPacketHandlers.Enqueue( handler );
		}

		//------------------------------------------------------------------------		
		int m_ProcedureIndex = 0;
		public void AddDBHandler( DBHandlerObject obj )
		{
			obj.procedureIndex = ++m_ProcedureIndex;

			if( m_ProcedureIndex >= int.MaxValue )
				m_ProcedureIndex = 0;

			if( mConfig.UseConcurrentWaitQueue )
				mConcurrentWaitProcedureHandlers.Enqueue( obj );
			else
				mWaitProcedureHandlers.Enqueue( obj );
		}

		//------------------------------------------------------------------------
		public void AddProcedureHandler(DBHandlerObject obj)
		{
			mProcedureHandlers.Enqueue( obj );
		}

		//------------------------------------------------------------------------		
		public void TestConnection()
		{
			Log.WriteDB( "Connect to database : " + mDBConnection.ConnectionString );
			mDBConnection.TestConnection();
		}

		//------------------------------------------------------------------------
		bool ExecuteSessionPacketHandler( DBHandlerObject obj )
		{
			try
			{
				return obj.session_packet_handler.MoveNext();
			}
			catch( Exception ex )
			{
				obj.Unlock();
				obj.ThrowPacketException( ex );
			}

			return false;
		}

		//------------------------------------------------------------------------		
		public bool IsFinish
		{
			get
			{
				if( mConfig.UseConcurrentWaitQueue )
					return ( mProcedureHandlers.Count == 0 && mConcurrentWaitProcedureHandlers.IsEmpty );
				else
					return ( mProcedureHandlers.Count == 0 && mWaitProcedureHandlers.Count == 0 );
			}
		}

		//------------------------------------------------------------------------		
		public void Update()
		{
			// use_parallel do not use
			if( mSessionPacketHandlers.Count > 0 )
			{
				Queue<DBHandlerObject> session_packet_handlers = new Queue<DBHandlerObject>();
				foreach( DBHandlerObject obj in mSessionPacketHandlers )
				{
					if( ExecuteSessionPacketHandler( obj ) == true )
						session_packet_handlers.Enqueue( obj );
				}
				mSessionPacketHandlers = session_packet_handlers;
			}

			if( mProcedureHandlers.Count > 0 )
			{
				if( mConfig.UseParallel )
				{
					System.Collections.Concurrent.ConcurrentQueue<DBHandlerObject> procedure_handlers = new System.Collections.Concurrent.ConcurrentQueue<DBHandlerObject>();
					System.Threading.Tasks.Parallel.ForEach( mProcedureHandlers, obj =>
					{
						if( obj.procedure_handler.MoveNext() == true )
							procedure_handlers.Enqueue( obj );
						else
							obj.done = true;
					} );

					mProcedureHandlers = new Queue<DBHandlerObject>( procedure_handlers );
				}
				else
				{
					Queue<DBHandlerObject> procedure_handlers = new Queue<DBHandlerObject>();
					foreach( DBHandlerObject obj in mProcedureHandlers )
					{
						if( obj.procedure_handler.MoveNext() == true )
							procedure_handlers.Enqueue( obj );
						else
							obj.done = true;
					}
					mProcedureHandlers = procedure_handlers;
				}
			}

			bool has_wait = false;
			if( mConfig.UseConcurrentWaitQueue )
				has_wait = ( mConcurrentWaitProcedureHandlers.IsEmpty == false );
			else
				has_wait = ( mWaitProcedureHandlers.Count > 0 );

			while( has_wait && mProcedureHandlers.Count < mConfig.PoolSize )
			{
				ISqlCommand sql_command = mDBConnection.CreateSqlCommand( false );
				if( sql_command == null )
				{
					Log._LogError( string.Format( "CreateSqlCommand null" ) );
					break;
				}

				DBHandlerObject obj;
				if( mConfig.UseConcurrentWaitQueue )
				{
					if( mConcurrentWaitProcedureHandlers.TryDequeue( out obj ) == false )
					{
						Log.WriteError( "?? WaitProcedureHandlers TryDequeue false!" );
						sql_command.Close();
						has_wait = ( mConcurrentWaitProcedureHandlers.IsEmpty == false );
						continue;
					}

					has_wait = ( mConcurrentWaitProcedureHandlers.IsEmpty == false );
				}
				else
				{
					obj = mWaitProcedureHandlers.Dequeue();
					has_wait = ( mWaitProcedureHandlers.Count > 0 );
				}

				if( obj == null )
				{
					sql_command.Close();
					Log.WriteError( "?? WaitProcedureHandlers obj is null!" );
					continue;
				}

				obj.sql_command = sql_command;
				mProcedureHandlers.Enqueue( obj );
			}
		}

		//------------------------------------------------------------------------		
		public void ExecuteCommandText( string text )
		{
			int result_row = mDBConnection.ExecuteCommandText( text );
			Log.WriteWarning( string.Format( "ExecuteCommandText : {0}", result_row ) );
		}
	}
}
