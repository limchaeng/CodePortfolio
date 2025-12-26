//////////////////////////////////////////////////////////////////////////
//
// DBHandlerExecute
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
using System.Data.SqlClient;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UMF.Core;

namespace UMF.Database
{
	//------------------------------------------------------------------------	
	public static class DBHandlerExecute
	{
		public delegate void CallbackProcedure( DBHandlerObject data );

		//------------------------------------------------------------------------		
		static List<int> mIgnoreSqlInfoMessageLogErrors = new List<int>();
		public static void AddIgnoreSqlInfoMessageLogErrors( int error_code )
		{
			if( mIgnoreSqlInfoMessageLogErrors.Contains( error_code ) == false )
				mIgnoreSqlInfoMessageLogErrors.Add( error_code );
		}
		public static bool IsIgnoreSqlInfoMessageError( int error_number )
		{
			if( mIgnoreSqlInfoMessageLogErrors.Count > 0 )
			{
				if( mIgnoreSqlInfoMessageLogErrors.Contains( error_number ) )
					return true;
			}

			return false;
		}

		//------------------------------------------------------------------------
		static void ExceptionError( string ex_name, Exception ex, DBHandlerObject data, int return_code )
		{
			Log.WriteError( "{0},{1}", ex_name, ex.ToString() );
			if( data.database.Config.SqlExceptionNotify )
				Log.SendNotification( string.Format( "{0}:{1}", ex_name, ex.Message ) );

			if( data.database.Config.IsThrowSqlException == false )
			{
				data.readObject = new PROCEDURE_READ_DEFAULT();
				data.readObject.return_code = return_code;
				if( data.callback != null )
					data.callback( data );

				data.callback = null;
			}
		}

		//------------------------------------------------------------------------		
		static IEnumerator ProcedureHandler<CallT, ReadT>( CallT callObject, DBHandlerObject data ) where ReadT : PROCEDURE_READ_BASE
		{
			Stopwatch queryTime, totalTime = Stopwatch.StartNew();

			ProcedureAttribute attr = PROCEDURE<CallT>.Attr;
			string spname = data.database.Config.DatabaseName + attr.GetString( typeof( CallT ) );
			ISqlCommand sql_command = data.sql_command;
			System.Exception _procedure_exception = null;

			bool write_log = false;
			try
			{
				if( data.database.Config.CoreLogType != eCoreLogType.None || attr.LogType != eCoreLogType.None )
				{
					write_log = true;
					Log.WriteDB( "[{0}:{1}-{2}.{3}] Begin {4}", spname, data.procedureIndex, data.recvIndex, data.procedureIndexInternal, ProcedureLogFormatter.Instance.Serialize<CallT>( callObject, false ) );
				}
			}
			catch( System.Exception ex )
			{
				_procedure_exception = ex;
			}

			if( _procedure_exception != null )
			{
				Log.WriteError( "ProcedureHandlerException:" + spname + ", " + _procedure_exception.ToString() );
				if( data.database.Config.IsThrowSqlException == true )
				{
					throw _procedure_exception;
				}
				else
				{
					data.readObject = new PROCEDURE_READ_DEFAULT();
					data.readObject.return_code = -4;
					if( data.callback != null )
						data.callback( data );

					data.callback = null;
					yield break;
				}
			}

			try
			{
				sql_command.Begin<CallT, ReadT>( spname, data.database.Config.CommandTimeout, callObject, data );
				if( write_log )
					sql_command.Dump( "[Begin]" );
			}
			catch( System.Exception ex )
			{
				sql_command.Close();
				ExceptionError( "SqlCommandBeginException:" + spname, ex, data, -4 );
				if( data.database.Config.IsThrowSqlException )
					throw ex;
				else
					yield break;
			}

			queryTime = Stopwatch.StartNew();
			IAsyncResult result = sql_command.Execute( attr.ExecuteType );
			if( result == null )
			{
				Exception ex = new Exception( "sql command result is null" );
				ExceptionError( "SqlCommandExecuteException:" + spname, ex, data, -5 );
				if( data.database.Config.IsThrowSqlException )
					throw ex;
				else
					yield break;
			}

			while( result.IsCompleted == false )
				yield return null;

			SqlException sql_exception = null;
			Exception localSystemEx = null;

			try
			{
				data.readObject = sql_command.Read<ReadT>( attr.ExecuteType, result );
			}
			catch( SqlException ex )
			{
				sql_exception = ex;
			}
			catch( System.Exception system_ex )
			{
				localSystemEx = system_ex;
			}

			queryTime.Stop();
			sql_command.End();

			if( data.readObject == null )
				data.readObject = new PROCEDURE_READ_DEFAULT();

			if( sql_exception != null )
			{
				data.readObject.return_code = sql_exception.Number;
			}
			else if( localSystemEx != null )
			{
				data.readObject.return_code = -5;
			}

			if( localSystemEx != null )
			{
				Log.WriteError( "SqlSystemException:{0}\n{1}", localSystemEx.ToString(), ProcedureLogFormatter.Instance.Serialize<CallT>( callObject, false ) );
				sql_command.Dump( "SqlSystemException" );

				if( data.database.Config.SqlExceptionNotify )
					Log.SendNotification( string.Format( "SqlSystemException:{0}:{1}", spname, localSystemEx.Message ) );

				if( data.database.Config.IsThrowSqlException == true )
					throw localSystemEx;
			}

			if( sql_exception != null && ( attr.ExecuteType == eProcedureExecute.NonQuery || data.readObject.return_code < DBErrorShared.DB_CUSTOM_ERROR_BEGIN ) )
			{
				Log.WriteError( "SqlException:{0}\n{1}", sql_exception.ToString(), ProcedureLogFormatter.Instance.Serialize<CallT>( callObject, false ) );
				sql_command.Dump( "SqlException" );

				if( data.database.Config.SqlExceptionNotify )
					Log.SendNotification( string.Format( "SqlException:{0}:{1}", spname, sql_exception.Message ) );

				if( data.database.Config.IsThrowSqlException == true )
					throw sql_exception;
			}

			if( localSystemEx == null && sql_exception == null && data.readObject.return_code != 0 )
			{
				Log.WriteError( "DBReturnThrow:{0}\n{1}", data.readObject.return_code, ProcedureLogFormatter.Instance.Serialize<CallT>( callObject, false ) );
				sql_command.Dump( "DBReturnThrow" );
			}

			totalTime.Stop();

			if( write_log )
				Log.WriteDB( "[{0}:{1}-{2}.{3}] time:{4}ms, query:{5}, return:{6} data:{7}", spname, data.procedureIndex, data.recvIndex, data.procedureIndexInternal,
					totalTime.ElapsedMilliseconds.ToString(), queryTime.ElapsedMilliseconds.ToString(),
					DBErrorShared.GetErrorString( data.readObject.return_code ),
					ProcedureLogFormatter.Instance.Serialize<ReadT>( data.readObject, false ) );

			if( queryTime.ElapsedMilliseconds > data.database.Config.QueryTooLongTimeMilli )
			{
				Log.WriteError( "[{0}:{1}-{2}.{3}] query_time:{4}ms TooLong {5}", spname, data.procedureIndex, data.recvIndex, data.procedureIndexInternal,
					queryTime.ElapsedMilliseconds.ToString(),
					ProcedureLogFormatter.Instance.Serialize<CallT>( callObject, false ) );
				Log.SendNotification( string.Format( "SP:{0} QueryTooLong", spname ) );
			}

			if( data.callback != null )
			{
				data.callback( data );
			}
			else if( data.readObject.return_code > 0 && data.readObject.return_code < DBErrorShared.DB_CUSTOM_ERROR_BEGIN )
			{
				if( data.dberror_logging )
					Log.WriteError( "DBReturnError {0}-{1}", data.readObject.return_code, ProcedureLogFormatter.Instance.Serialize<CallT>( callObject, false ) );
			}

			data.callback = null;
		}

		//------------------------------------------------------------------------		
		public static DBHandlerObject CreateData( DatabaseMain db )
		{
			DBHandlerObject data = new DBHandlerObject();
			data.database = db;
			data.procedureIndexInternal = 0;
			data.recvIndex = 0;
			data.done = false;

			return data;
		}

		//------------------------------------------------------------------------		
		public static IEnumerator Execute<CallT, ReadT>( CallT callObject, DBHandlerObject data ) where ReadT : PROCEDURE_READ_BASE
		{
			IEnumerator handler = ProcedureHandler<CallT, ReadT>( callObject, data );
			data.done = false;
			data.procedureIndexInternal++;
			data.procedure_handler = handler;
			data.database.AddDBHandler( data );

			while( data.done == false )
				yield return null;
		}

		//------------------------------------------------------------------------		
		public static void ExecuteNonWaiting<CallT, ReadT>( CallT callObject, DBHandlerObject data ) where ReadT : PROCEDURE_READ_BASE
		{
			IEnumerator handler = ProcedureHandler<CallT, ReadT>( callObject, data );
			data.done = false;
			data.procedureIndexInternal++;
			data.procedure_handler = handler;
			data.database.AddDBHandler( data );
		}

		//------------------------------------------------------------------------		
		public static void ExecuteCallback<CallT, ReadT>( CallT callObject, DBHandlerObject data, CallbackProcedure callback, params object[] callback_data ) where ReadT : PROCEDURE_READ_BASE
		{
			IEnumerator handler = ProcedureHandler<CallT, ReadT>( callObject, data );
			data.done = false;
			data.procedureIndexInternal++;
			data.procedure_handler = handler;
			data.callback = callback;
			data.callback_data = callback_data;
			data.database.AddDBHandler( data );
		}

		//------------------------------------------------------------------------		
		public static IEnumerator ExecuteDirect<CallT, ReadT>( CallT callObject, DatabaseMain db ) where ReadT : PROCEDURE_READ_BASE
		{
			DBHandlerObject data = new DBHandlerObject();
			data.database = db;
			data.procedureIndexInternal = 0;
			data.recvIndex = 0;
			data.done = false;

			IEnumerator handler = ProcedureHandler<CallT, ReadT>( callObject, data );
			data.procedure_handler = handler;
			data.database.AddDBHandler( data );

			while( data.done == false )
				yield return null;
		}

		//------------------------------------------------------------------------		
		public static void ExecuteDirectNonWaiting<CallT, ReadT>( CallT callObject, DatabaseMain db ) where ReadT : PROCEDURE_READ_BASE
		{
			DBHandlerObject data = new DBHandlerObject();
			data.database = db;
			data.procedureIndexInternal = 0;
			data.recvIndex = 0;
			data.done = false;
			data.dberror_logging = true;

			IEnumerator handler = ProcedureHandler<CallT, ReadT>( callObject, data );
			data.done = false;
			data.procedureIndexInternal++;
			data.procedure_handler = handler;
			data.database.AddDBHandler( data );
		}

		//------------------------------------------------------------------------		
		public static void ExecuteDirectCallback<CallT, ReadT>( CallT callObject, DatabaseMain db, CallbackProcedure callback, params object[] callback_data ) where ReadT : PROCEDURE_READ_BASE
		{
			DBHandlerObject data = new DBHandlerObject();
			data.database = db;
			data.procedureIndexInternal = 0;
			data.recvIndex = 0;
			data.done = false;

			IEnumerator handler = ProcedureHandler<CallT, ReadT>( callObject, data );
			data.done = false;
			data.procedureIndexInternal++;
			data.procedure_handler = handler;
			data.callback = callback;
			data.callback_data = callback_data;
			data.database.AddDBHandler( data );
		}

		//------------------------------------------------------------------------		
		public static ReadT ExecuteSync<CallT, ReadT>( CallT callObject, DatabaseMain db ) where ReadT : PROCEDURE_READ_BASE
		{
			DBHandlerObject data = new DBHandlerObject();
			data.database = db;
			data.procedureIndexInternal = 0;
			data.recvIndex = 0;
			data.sql_command = db.DBConnection.CreateSqlCommand( true );

			IEnumerator handler = ProcedureHandler<CallT, ReadT>( callObject, data );

			DateTime timeout = DateTime.Now.AddSeconds( 10 );

			while( handler.MoveNext() == true )
			{
				if( DateTime.Now > timeout )
					throw new System.Exception( "ExecuteSync Timeout in " + typeof( CallT ).Name );

				System.Threading.Thread.Sleep( 1 );
			}

			return (ReadT)data.readObject;
		}
	}
}
