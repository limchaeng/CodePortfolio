//////////////////////////////////////////////////////////////////////////
//
// SqlDump_MySql
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
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;

namespace UMF.Database.MySql
{
	public class SqlDump_MySql
	{
		//------------------------------------------------------------------------		
		public static string GetCommandDump( string prefix, MySqlCommand sqc )
		{
			try
			{
				StringBuilder sbCommandText = new StringBuilder();

				sbCommandText.AppendLine( string.Format( "{0} {1}", prefix, sqc.CommandText ) );
				sbCommandText.AppendLine( "-- BEGIN COMMAND" );

				// params
				for( int i = 0; i < sqc.Parameters.Count; i++ )
					logParameterToSqlBatch( sqc.Parameters[i], sbCommandText );
				sbCommandText.AppendLine( "-- END PARAMS" );

				// command
				if( sqc.CommandType == CommandType.StoredProcedure )
				{
					sbCommandText.Append( "CALL " );

					bool hasReturnValue = false;
					for( int i = 0; i < sqc.Parameters.Count; i++ )
					{
						if( sqc.Parameters[i].Direction == ParameterDirection.ReturnValue )
							hasReturnValue = true;
					}
					if( hasReturnValue )
					{
						sbCommandText.Append( "@returnValue = " );
					}

					sbCommandText.Append( sqc.CommandText + " " );

					bool hasPrev = false;
					for( int i = 0; i < sqc.Parameters.Count; i++ )
					{
						var cParam = sqc.Parameters[i];
						if( cParam.Direction != ParameterDirection.ReturnValue )
						{
							if( hasPrev )
								sbCommandText.Append( ", " );

							sbCommandText.Append( cParam.ParameterName );
							sbCommandText.Append( " = " );
							sbCommandText.Append( cParam.ParameterName );

							if( cParam.Direction.HasFlag( ParameterDirection.Output ) )
								sbCommandText.Append( " OUTPUT" );

							hasPrev = true;
						}
					}
				}
				else
				{
					sbCommandText.AppendLine( sqc.CommandText );
				}

				sbCommandText.AppendLine( "-- RESULTS" );
				sbCommandText.Append( "SELECT 1 as Executed" );
				for( int i = 0; i < sqc.Parameters.Count; i++ )
				{
					var cParam = sqc.Parameters[i];

					if( cParam.Direction == ParameterDirection.ReturnValue )
					{
						sbCommandText.Append( ", @returnValue as ReturnValue" );
					}
					else if( cParam.Direction.HasFlag( ParameterDirection.Output ) )
					{
						sbCommandText.Append( ", " );
						sbCommandText.Append( cParam.ParameterName );
						sbCommandText.Append( " as [" );
						sbCommandText.Append( cParam.ParameterName );
						sbCommandText.Append( ']' );
					}
				}
				sbCommandText.AppendLine( ";" );

				sbCommandText.AppendLine( "-- END COMMAND" );
				return sbCommandText.ToString();
			}
			catch( System.Exception ex )
			{
				return string.Format( "{0}[{1}] {2}", prefix, sqc.CommandText, ex.ToString() );
			}
		}

		//------------------------------------------------------------------------		
		private static void logParameterToSqlBatch( MySqlParameter param, StringBuilder sbCommandText )
		{
			sbCommandText.Append( "SET " );
			if( param.Direction == ParameterDirection.ReturnValue )
			{
				sbCommandText.AppendLine( "@returnValue INT;" );
			}
			else
			{
				sbCommandText.Append( param.ParameterName );

				sbCommandText.Append( ' ' );
// 				if( param.MySqlDbType == MySqlDbType.Structured )
// 				{
// 					logStructuredParameter( param, sbCommandText );
// 				}
// 				else
				{
					logParameterType( param, sbCommandText );
					sbCommandText.Append( " = " );
					logQuotedParameterValue( param.Value, sbCommandText );

					sbCommandText.AppendLine( ";" );
				}
			}
		}

		//------------------------------------------------------------------------		
		// for table-valued
		private static void logStructuredParameter( MySqlParameter param, StringBuilder sbCommandText )
		{
			sbCommandText.AppendLine( string.Format( " {0} not support;", param.ParameterName ) );
			if( param.Value != null )
			{
				IProcedureTableValued tv_base = (IProcedureTableValued)param.Value;

				int last_row = -1;

				tv_base.GetDumpRecord( ( int row, int col, object value ) =>
				{
					if( last_row != row )
					{
						last_row = row;

						if( row > 0 )
							sbCommandText.AppendLine( ");" );

						sbCommandText.Append( "INSERT INTO " );
						sbCommandText.Append( param.ParameterName );
						sbCommandText.Append( " VALUES (" );
					}

					if( col > 0 )
						sbCommandText.Append( ", " );

					logQuotedParameterValue( value, sbCommandText );
				} );

				if( last_row != -1 )
					sbCommandText.AppendLine( ");" );
			}
		}

		//------------------------------------------------------------------------		
		// for DataTable
		private static void logStructuredParameter_forDataTable( MySqlParameter param, StringBuilder sbCommandText )
		{
			sbCommandText.AppendLine( " {List Type};" );
			var dataTable = (DataTable)param.Value;

			for( int rowNo = 0; rowNo < dataTable.Rows.Count; rowNo++ )
			{
				sbCommandText.Append( "INSERT INTO " );
				sbCommandText.Append( param.ParameterName );
				sbCommandText.Append( " VALUES (" );

				bool hasPrev = false;
				for( int colNo = 0; colNo < dataTable.Columns.Count; colNo++ )
				{
					if( hasPrev )
					{
						sbCommandText.Append( ", " );
					}
					logQuotedParameterValue( dataTable.Rows[rowNo].ItemArray[colNo], sbCommandText );
					hasPrev = true;
				}
				sbCommandText.AppendLine( ");" );
			}
		}

		//------------------------------------------------------------------------		
		const string DATETIME_FORMAT_ROUNDTRIP = "o";
		private static void logQuotedParameterValue( object value, StringBuilder sbCommandText )
		{
			try
			{
				if( value == null )
				{
					sbCommandText.Append( "NULL" );
				}
				else
				{
					value = unboxNullable( value );

					if( value is string
						|| value is char
						|| value is char[] )
					// 						|| value is System.Xml.Linq.XElement
					// 						|| value is System.Xml.Linq.XDocument )
					{
						sbCommandText.Append( "N'" );
						sbCommandText.Append( value.ToString().Replace( "'", "''" ) );
						sbCommandText.Append( '\'' );
					}
					else if( value is bool )
					{
						// True -> 1, False -> 0
						sbCommandText.Append( Convert.ToInt32( value ) );
					}
					else if( value is sbyte
						|| value is byte
						|| value is short
						|| value is ushort
						|| value is int
						|| value is uint
						|| value is long
						|| value is ulong
						|| value is float
						|| value is double
						|| value is decimal )
					{
						sbCommandText.Append( value.ToString() );
					}
					else if( value is DateTime )
					{
						sbCommandText.Append( "CAST('" );
						sbCommandText.Append( ( (DateTime)value ).ToString( DATETIME_FORMAT_ROUNDTRIP ) );
						sbCommandText.Append( "' AS DATETIME)" );
					}
					else if( value is DateTimeOffset )
					{
						sbCommandText.Append( '\'' );
						sbCommandText.Append( ( (DateTimeOffset)value ).ToString( DATETIME_FORMAT_ROUNDTRIP ) );
						sbCommandText.Append( '\'' );
					}
					else if( value is Guid )
					{
						sbCommandText.Append( '\'' );
						sbCommandText.Append( ( (Guid)value ).ToString() );
						sbCommandText.Append( '\'' );
					}
					else if( value is byte[] )
					{
						var data = (byte[])value;
						if( data.Length == 0 )
						{
							sbCommandText.Append( "NULL" );
						}
						else
						{
							sbCommandText.Append( "0x" );
							for( int i = 0; i < data.Length; i++ )
							{
								sbCommandText.Append( data[i].ToString( "h2" ) );
							}
						}
					}
					else
					{
						sbCommandText.Append( "/* UNKNOWN DATATYPE: " );
						sbCommandText.Append( value.GetType().ToString() );
						sbCommandText.Append( " *" + "/ N'" );
						sbCommandText.Append( value.ToString() );
						sbCommandText.Append( '\'' );
					}
				}
			}

			catch( System.Exception ex )
			{
				sbCommandText.AppendLine( "/* Exception occurred while converting parameter: " );
				sbCommandText.AppendLine( ex.ToString() );
				sbCommandText.AppendLine( "*/" );
			}
		}

		//------------------------------------------------------------------------		
		private static object unboxNullable( object value )
		{
			var typeOriginal = value.GetType();
			if( typeOriginal.IsGenericType
				&& typeOriginal.GetGenericTypeDefinition() == typeof( Nullable<> ) )
			{
				// generic value, unboxing needed
				return typeOriginal.InvokeMember( "GetValueOrDefault",
					System.Reflection.BindingFlags.Public |
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.InvokeMethod,
					null, value, null );
			}
			else
			{
				return value;
			}
		}

		//------------------------------------------------------------------------		
		private static void logParameterType( MySqlParameter param, StringBuilder sbCommandText )
		{
			switch( param.MySqlDbType )
			{
				// variable length
				case MySqlDbType.VarBinary:
					{
						sbCommandText.Append( param.MySqlDbType.ToString().ToUpper() );
						sbCommandText.Append( '(' );
						sbCommandText.Append( param.Size );
						sbCommandText.Append( ')' );
					}
					break;

				// max 255
				case MySqlDbType.TinyBlob:
				case MySqlDbType.VarChar:
				case MySqlDbType.TinyText:
					{
						sbCommandText.Append( param.MySqlDbType.ToString().ToUpper() );
						sbCommandText.Append( "(255 /* Specified as " );
						sbCommandText.Append( param.Size );
						sbCommandText.Append( " */)" );
					}
					break;
				// max 65535
				case MySqlDbType.VarString:
				case MySqlDbType.Blob:
				case MySqlDbType.Text:
					{
						sbCommandText.Append( param.MySqlDbType.ToString().ToUpper() );
						sbCommandText.Append( "(65535 /* Specified as " );
						sbCommandText.Append( param.Size );
						sbCommandText.Append( " */)" );
					}
					break;
				// max 16777215
				case MySqlDbType.MediumBlob:
				case MySqlDbType.MediumText:
					{
						sbCommandText.Append( param.MySqlDbType.ToString().ToUpper() );
						sbCommandText.Append( "(16777215 /* Specified as " );
						sbCommandText.Append( param.Size );
						sbCommandText.Append( " */)" );
					}
					break;
				// max 4294967295
				case MySqlDbType.LongBlob:
				case MySqlDbType.LongText:
					{
						sbCommandText.Append( param.MySqlDbType.ToString().ToUpper() );
						sbCommandText.Append( "(4294967295 /* Specified as " );
						sbCommandText.Append( param.Size );
						sbCommandText.Append( " */)" );
					}
					break;

				case MySqlDbType.Decimal:
				case MySqlDbType.Byte:
				case MySqlDbType.Int16:
				case MySqlDbType.Int32:
				case MySqlDbType.Float:
				case MySqlDbType.Double:
				case MySqlDbType.Timestamp:
				case MySqlDbType.Int64:
				case MySqlDbType.Int24:
				case MySqlDbType.Date:
				case MySqlDbType.Time:
				case MySqlDbType.DateTime:
				case MySqlDbType.Year:
				case MySqlDbType.Bit:
				case MySqlDbType.JSON:
				case MySqlDbType.NewDecimal:
				case MySqlDbType.Enum:
				case MySqlDbType.String:
				case MySqlDbType.Geometry:
				case MySqlDbType.UByte:
				case MySqlDbType.UInt16:
				case MySqlDbType.UInt32:
				case MySqlDbType.UInt64:
				case MySqlDbType.UInt24:
				case MySqlDbType.Binary:
				case MySqlDbType.Guid:
					sbCommandText.Append( param.MySqlDbType.ToString().ToUpper() );
					break;

				case MySqlDbType.Set:
					sbCommandText.Append( param.MySqlDbType.ToString().ToUpper() );
					break;

				// Unknown
				default:
					{
						sbCommandText.Append( "/* UNKNOWN DATATYPE: " );
						sbCommandText.Append( param.MySqlDbType.ToString().ToUpper() );
						sbCommandText.Append( " *" + "/ " );
						sbCommandText.Append( param.MySqlDbType.ToString().ToUpper() );
					}
					break;
			}
		}
	}
}
