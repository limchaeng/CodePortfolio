//////////////////////////////////////////////////////////////////////////
//
// SqlCommand_MSSql
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
using UMF.Core;
using System.Data;

namespace UMF.Database.MSSql
{
	//------------------------------------------------------------------------
	public class SqlCommand_MSSql : ISqlCommand
	{
		SqlConnection mSqlConnection = null;
		SqlCommand mSqlCommand = null;

		//------------------------------------------------------------------------		
		static public SqlDbType ConvertToSqlDbType( Type type )
		{
			SqlParameter param = new SqlParameter();
			System.ComponentModel.TypeConverter tc = System.ComponentModel.TypeDescriptor.GetConverter( param.DbType );
			param.DbType = (DbType)tc.ConvertFrom( type.Name );
			return param.SqlDbType;
		}

		public SqlCommand_MSSql( SqlConnection _connectcion )
		{
			this.mSqlConnection = _connectcion;
		}

		//------------------------------------------------------------------------
		public void Begin<CallT, ReadT>( string sp_name, int cmd_timeout, CallT callObject, DBHandlerObject data ) where ReadT : PROCEDURE_READ_BASE
		{
			try
			{
				mSqlCommand = new SqlCommand( sp_name );
				mSqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
				mSqlCommand.CommandTimeout = cmd_timeout;
				ProcedureWriteFormatter.Instance.Serialize<ReadT, CallT>( callObject, data.database, this );
			}
			catch( System.Exception ex )
			{
				throw ex;
			}
		}

		//------------------------------------------------------------------------		
		public IAsyncResult Execute( eProcedureExecute execute_type )
		{
			if( mSqlCommand == null )
				return null;

			mSqlCommand.Connection = mSqlConnection;
			mSqlConnection.InfoMessage += OnSqlConnectionInfoMessage;

			IAsyncResult result = null;
			if( execute_type == eProcedureExecute.NonQuery )
				result = mSqlCommand.BeginExecuteNonQuery();
			else
				result = mSqlCommand.BeginExecuteReader();

			return result;
		}

		//------------------------------------------------------------------------
		public PROCEDURE_READ_BASE Read<ReadT>( eProcedureExecute execute_type, IAsyncResult result ) where ReadT : PROCEDURE_READ_BASE
		{
			SqlDataReader reader = null;

			if( execute_type == eProcedureExecute.NonQuery )
				mSqlCommand.EndExecuteNonQuery( result );
			else
				reader = mSqlCommand.EndExecuteReader( result );

			return ProcedureReadFormatter.Instance.Serialize<ReadT>( reader, mSqlCommand );
		}

		//------------------------------------------------------------------------
		public void End()
		{
			mSqlConnection.InfoMessage -= OnSqlConnectionInfoMessage;
			mSqlConnection.Close();
		}

		//------------------------------------------------------------------------		
		public void Close()
		{
			if( mSqlConnection != null )
				mSqlConnection.Close();
		}

		//------------------------------------------------------------------------
		public void Dump( string prefix )
		{
			if( mSqlCommand != null )
			{
				try
				{
					Log.WriteDB( SqlDump_MSSql.GetCommandDump( prefix, mSqlCommand ) );
				}
				catch( System.Exception ex )
				{
					Log.WriteError( ex.ToString() );
				}
			}
		}

		//------------------------------------------------------------------------		
		void OnSqlConnectionInfoMessage( object sender, SqlInfoMessageEventArgs e )
		{
			if( e == null || e.Errors == null || e.Errors.Count <= 0 )
				return;

			foreach( SqlError sql_error in e.Errors )
			{
				if( DBHandlerExecute.IsIgnoreSqlInfoMessageError( sql_error.Number ) )
					continue;

				if( DBErrorShared.ParseErrorCode( sql_error.Message ) < DBErrorShared.DB_CUSTOM_ERROR_BEGIN )
				{
					Log.WriteError( "SqlError:{0}", sql_error.Message );
					Log.SendNotification( string.Format( "SqlError:{0}", sql_error.Message ) );
				}
			}
			Log.WriteError( "SqlInfoMessage:{0}", e.Message );
		}

		//------------------------------------------------------------------------		
		public void AddWithValue( string parameterName, object value )
		{
			mSqlCommand.Parameters.AddWithValue( "@" + parameterName, value );
		}

		//------------------------------------------------------------------------		
		public void AddTableValued( string parameterName, string type_name, object value )
		{
			SqlParameter param = mSqlCommand.Parameters.Add( "@" + parameterName, SqlDbType.Structured );
			param.Direction = ParameterDirection.Input;
			param.TypeName = type_name;
			param.Value = value;
		}

		//------------------------------------------------------------------------		
		/// <summary>
		///   none_min_max : 0=none, 1=min, 2=max
		/// </summary>
		public void AddDateTime( string parameterName, int none_min_max, object value )
		{
			if( none_min_max == 0 )
				AddWithValue( parameterName, value );
			else if( none_min_max == 1 )
				AddWithValue( parameterName, System.Data.SqlTypes.SqlDateTime.MinValue );
			else if( none_min_max == 2 )
				AddWithValue( parameterName, System.Data.SqlTypes.SqlDateTime.MaxValue );
		}

		//------------------------------------------------------------------------		
		public void AddReturnValue( string parameterName, Type field_type )
		{
			SqlParameter param = mSqlCommand.Parameters.Add( "@" + parameterName, ConvertToSqlDbType( field_type ) );
			param.Direction = ParameterDirection.ReturnValue;
		}

		//------------------------------------------------------------------------		
		public void AddOutputValue( string parameterName, Type field_type )
		{
			SqlParameter param = mSqlCommand.Parameters.Add( "@" + parameterName, ConvertToSqlDbType( field_type ) );
			param.Direction = ParameterDirection.Output;
		}
	}
}
