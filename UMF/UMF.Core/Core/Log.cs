//////////////////////////////////////////////////////////////////////////
//
// Log
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

namespace UMF.Core
{
	//------------------------------------------------------------------------	
	public enum eCoreLogType
	{
		None,
		NameOnly,
		Detail,
		Always,
		Important,
	}

	public static class Log
	{
		public static string LOG_PATH = "_Log";

		//------------------------------------------------------------------------		
		public static void Write( string strLog )
		{
			if( _Log == null )
				return;

			_Log( strLog );
		}

		//------------------------------------------------------------------------		
		public static void Write( string strLog, params object[] args )
		{
			if( _Log == null )
				return;

			_Log( string.Format( strLog, args ) );
		}

		//------------------------------------------------------------------------		
		public static void WriteWarning( string strLog )
		{
			if( _LogWarning != null )
				_LogWarning( strLog );
			else if( _Log != null )
				_Log( "Warning : " + strLog );
		}

		//------------------------------------------------------------------------		
		public static void WriteWarning( string strLog, params object[] args )
		{
			if( _LogWarning != null )
				_LogWarning( string.Format( strLog, args ) );
			else if( _Log != null )
				_Log( "Warning : " + string.Format( strLog, args ) );
		}

		//------------------------------------------------------------------------		
		public static void WriteError( string strLog )
		{
			if( _LogError != null )
				_LogError( strLog );
			else if( _Log != null )
				_Log( "Error : " + strLog );
		}

		//------------------------------------------------------------------------		
		public static void WriteError( string strLog, params object[] args )
		{
			if( _LogError != null )
				_LogError( string.Format( strLog, args ) );
			else if( _Log != null )
				_Log( "Error : " + string.Format( strLog, args ) );
		}

		//------------------------------------------------------------------------		
		public static void WriteImportant( string strLog )
		{
			if( _LogImportant != null )
				_LogImportant( strLog );
			else if( _Log != null )
				_Log( "Important : " + strLog );
		}

		//------------------------------------------------------------------------		
		public static void WriteImportant( string strLog, params object[] args )
		{
			if( _LogImportant != null )
				_LogImportant( string.Format( strLog, args ) );
			else if( _Log != null )
				_Log( "Important : " + string.Format( strLog, args ) );
		}

		//------------------------------------------------------------------------		
		public static void WriteDB( string strLog )
		{
			if( _LogDB != null )
				_LogDB( strLog );
			else if( _Log != null )
				_Log( "DB : " + strLog );
		}

		//------------------------------------------------------------------------		
		public static void WriteDB( string strLog, params object[] args )
		{
			if( _LogDB != null )
				_LogDB( string.Format( strLog, args ) );
			else if( _Log != null )
				_Log( "DB : " + string.Format( strLog, args ) );
		}

		//------------------------------------------------------------------------		
		public static void WriteUserLog( string strLog, params object[] args )
		{
			if( _LogUserLog != null )
				_LogUserLog( string.Format( strLog, args ) );
			else if( _Log != null )
				_Log( "UserLog : " + string.Format( strLog, args ) );
		}

		//------------------------------------------------------------------------		
		public static void SendNotification( string str_message )
		{
			if( _Notification != null )
				_Notification( str_message );
		}

		//------------------------------------------------------------------------		
		public delegate void _LogDelegate( string strLog );
		public static _LogDelegate _Log = null, _LogError = null, _LogWarning = null, _LogImportant = null, _LogDB = null, _LogUserLog = null, _Notification = null;

		//------------------------------------------------------------------------		
		public static void SetAll( _LogDelegate del )
		{
			_Log = del;
			_LogWarning = del;
			_LogImportant = del;
			_LogDB = del;
			_LogUserLog = del;
			_Notification = del;
		}

		//------------------------------------------------------------------------		
		public static bool g_UserDisconnectWithPacketLog = true;
	}
}