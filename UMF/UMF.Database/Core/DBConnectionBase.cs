//////////////////////////////////////////////////////////////////////////
//
// DBConnectionBase
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
using System;


namespace UMF.Database
{
	//------------------------------------------------------------------------	
	public abstract class DBConnectionBase
	{
		protected DatabaseMain mDatabase = null;

		public DBConnectionBase(DatabaseMain db)
		{
			mDatabase = db;
			UpdateConnectionString( db.Config );
		}

		public abstract string ConnectionString { get; }
		public abstract void UpdateConnectionString( DatabaseMain.DatabaseConfig config );
		public abstract ISqlCommand CreateSqlCommand( bool is_sync );
		public abstract int ExecuteCommandText( string text );
		public virtual void TestConnection()
		{
			Log.Write( "Test Connection not implement!" );
		}
	}
}
