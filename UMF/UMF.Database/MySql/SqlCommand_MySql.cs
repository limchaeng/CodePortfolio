//////////////////////////////////////////////////////////////////////////
//
// SqlCommand_MySql
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
using System.Data;
using UMF.Core;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace UMF.Database.MySql
{
	//------------------------------------------------------------------------
	public class SqlCommand_MySql : ISqlCommand
	{
		MySqlConnection mSqlConnection = null;
		MySqlCommand mSqlCommand = null;

		//------------------------------------------------------------------------		
		static public MySqlDbType ConvertToSqlDbType( Type type )
		{
			MySqlParameter param = new MySqlParameter();
			System.ComponentModel.TypeConverter tc = System.ComponentModel.TypeDescriptor.GetConverter( param.DbType );
			param.DbType = (DbType)tc.ConvertFrom( type.Name );
			return param.MySqlDbType;
		}

		public SqlCommand_MySql( MySqlConnection _connectcion )
		{
			this.mSqlConnection = _connectcion;
		}

		//------------------------------------------------------------------------
		public void Begin<CallT, ReadT>( string sp_name, int cmd_timeout, CallT callObject, DBHandlerObject data ) where ReadT : PROCEDURE_READ_BASE
		{
			try
			{
				mSqlCommand = new MySqlCommand( sp_name );
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
			MySqlDataReader reader = null;

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
					Log.WriteDB( SqlDump_MySql.GetCommandDump( prefix, mSqlCommand ) );
				}
				catch( System.Exception ex )
				{
					Log.WriteError( ex.ToString() );
				}
			}
		}

		//------------------------------------------------------------------------		
		void OnSqlConnectionInfoMessage( object sender, MySqlInfoMessageEventArgs e )
		{
			if( e == null || e.errors == null || e.errors.Length <= 0 )
				return;

			foreach( MySqlError sql_error in e.errors )
			{
				if( DBHandlerExecute.IsIgnoreSqlInfoMessageError( sql_error.Code ) )
					continue;

				if( DBErrorShared.ParseErrorCode( sql_error.Message ) < DBErrorShared.DB_CUSTOM_ERROR_BEGIN )
				{
					Log.WriteError( "SqlError:{0}", sql_error.Message );
					Log.SendNotification( string.Format( "SqlError:{0}", sql_error.Message ) );
				}
			}

			Log.WriteError( "SqlInfoMessage:{0}", e.errors[0].Message );
		}

		//------------------------------------------------------------------------		
		public void AddWithValue( string parameterName, object value )
		{
			mSqlCommand.Parameters.AddWithValue( "@" + parameterName, value );
		}

		//------------------------------------------------------------------------		
		public void AddTableValued( string parameterName, string type_name, object value )
		{
			throw new Exception( "MySql does not support table-valued type!" );
		}

		//------------------------------------------------------------------------		
		/// <summary>
		///   none_min_max : 0=none, 1=min, 2=max
		/// </summary>
		public void AddDateTime( string parameterName, int none_min_max, object value )
		{
			// TODO : TEST need
			if( none_min_max == 0 )
				AddWithValue( parameterName, new MySqlDateTime( (DateTime)value ) );
			else if( none_min_max == 1 )
				AddWithValue( parameterName, new MySqlDateTime( DateTime.MinValue ) );
			else if( none_min_max == 2 )
				AddWithValue( parameterName, new MySqlDateTime( DateTime.MaxValue ) );
		}

		//------------------------------------------------------------------------		
		public void AddReturnValue( string parameterName, Type field_type )
		{
			MySqlParameter param = mSqlCommand.Parameters.Add( "@" + parameterName, ConvertToSqlDbType( field_type ) );
			param.Direction = ParameterDirection.ReturnValue;
		}

		//------------------------------------------------------------------------		
		public void AddOutputValue( string parameterName, Type field_type )
		{
			MySqlParameter param = mSqlCommand.Parameters.Add( "@" + parameterName, ConvertToSqlDbType( field_type ) );
			param.Direction = ParameterDirection.Output;
		}
	}
}
