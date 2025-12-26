//////////////////////////////////////////////////////////////////////////
//
// DBConnection_LocalMDF
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

namespace UMF.Database.MSSql
{
	public class DBConnection_LocalMDF : DBConnection_MSSql
	{
		public DBConnection_LocalMDF( DatabaseMain db ) 
			: base( db )
		{
		}

		//------------------------------------------------------------------------
		public override void UpdateConnectionString( DatabaseMain.DatabaseConfig config )
		{
			mConnectionString = config.CustomConnectionString;
		}
	}
}
