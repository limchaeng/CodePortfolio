//////////////////////////////////////////////////////////////////////////
//
// DBError
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
using UMF.Core;

namespace UMF.Database
{
	//------------------------------------------------------------------------	
	public enum DBCoreErrorCode : int
	{
		Success = 0,

		// read errors
		ReadError = -2,
		NoReturnValue = -3,
		SystemError = -4,
		NoGameServer = -1001,
	}

	//------------------------------------------------------------------------	
	public class DBErrorShared
	{
		public const int ERROR_PREFIX = 100000;             // in SP : Catch Process ERROR_NUMBER() + 100000
		public const int DB_CUSTOM_ERROR_BEGIN = 50000;     // in SP : custom throw error begin 50000
		public static bool ErrorDetail { get; set; }

		public delegate string delCustomErrorString( int error_code );
		static delCustomErrorString mCustomErrorString = null;
		public static delCustomErrorString CustomErrorHandler { set { mCustomErrorString = value; } }

		static Dictionary<int, string> mDBErrorCollection = new Dictionary<int, string>();

		//------------------------------------------------------------------------
		public static void AddDBError(int code, string name)
		{
			if( mDBErrorCollection.ContainsKey( code ) )
			{
				Log.WriteWarning( $"DBError already exist code {code}({name})" );
				string exist_name = mDBErrorCollection[code];
				mDBErrorCollection[code] = string.Format( "{0},{1}", exist_name, name );
			}
			else
			{
				mDBErrorCollection.Add( code, name );
			}
		}
		public static void AddDBError( Type enum_type )
		{
			foreach( int value in Enum.GetValues( enum_type ) )
			{
				AddDBError( value, Enum.GetName( enum_type, value ) );
			}
		}

		//------------------------------------------------------------------------		
		public static string GetErrorString( int error_code )
		{
			string error_string = error_code.ToString();
			if( ErrorDetail )
			{
				if( error_code < (int)DB_CUSTOM_ERROR_BEGIN )
				{
					if( Enum.IsDefined( typeof( DBCoreErrorCode ), error_code ) == true )
						error_string = $"{Enum.GetName( typeof( DBCoreErrorCode ), error_code )}({error_code})";
				}
				else
				{
					if( mDBErrorCollection.ContainsKey( error_code ) )
						error_string = $"{mDBErrorCollection[error_code]}({error_code})";

					if( mCustomErrorString != null )
						error_string = mCustomErrorString( error_code );
				}
			}

			return $"DB Error : {error_string}";
		}

		//------------------------------------------------------------------------		
		public static int ParseErrorCode( string error_message )
		{
			if( string.IsNullOrEmpty( error_message ) )
				return DB_CUSTOM_ERROR_BEGIN;

			// ErrorMessage : ErrorCode:nnnnnn,Message~~

			string[] splists = error_message.Split( ',' );
			if( splists != null && splists.Length > 0 )
			{
				string parse_ErrorCode = splists[0];
				int code;
				if( int.TryParse( parse_ErrorCode.Replace( "ErrorCode:", "" ), out code ) )
					return code - ERROR_PREFIX;
			}

			return DB_CUSTOM_ERROR_BEGIN;
		}

	}
}
