//////////////////////////////////////////////////////////////////////////
//
// GameDBManager
// 
// Created by LCY.
//
// Copyright 2022 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UMF.Core;
using UMF.Database;
using System.Linq;

namespace UMP.Server
{
	public class GameDBManager : Singleton<GameDBManager>
	{
		//------------------------------------------------------------------------	
		[Procedure( "SP_GameDatabase_Get", eProcedureExecute.Reader )]
		public class SP_GameDatabase_Get { }

		public class GAMEDB_INFO
		{
			public int db_idx;
			public string db_name;
			public string db_server;
			public string db_connection_string;
		}

		public class SP_GameDatabase_Get_Ack : PROCEDURE_READ_BASE
		{
			public List<GAMEDB_INFO> db_info_list;
		}

		public class DBInfo
		{
			public GAMEDB_INFO m_DBInfo;
			public DatabaseMain m_Database;

			public DBInfo( UMPServerApplication application, string config_file, GAMEDB_INFO db_info )
			{
				m_DBInfo = db_info;
				m_Database = new DatabaseMain( config_file );
				m_Database.Config.HostIP = m_DBInfo.db_server;
				m_Database.Config.DatabaseName = m_DBInfo.db_name;
				m_Database.Config.CustomConnectionString = m_DBInfo.db_connection_string;
				m_Database.UpdateConnectionInfo();
				m_Database.TestConnection();
			}

			public bool IsFinish()
			{
				return m_Database.IsFinish;
			}

			public void Update()
			{
				if( m_Database != null )
					m_Database.Update();
			}
		}

		private Dictionary<int, DBInfo> mDBDic = new Dictionary<int, DBInfo>();

		//------------------------------------------------------------------------
		public void Update()
		{
			foreach(DBInfo info in mDBDic.Values)
			{
				if( info.m_Database != null )
					info.m_Database.Update();
			}
		}

		//------------------------------------------------------------------------		
		public bool AddDatabase( UMPServerApplication application, string config_file, GAMEDB_INFO db_info )
		{
			if( mDBDic.ContainsKey( db_info.db_idx ) )
				return false;

			mDBDic.Add( db_info.db_idx, new DBInfo( application, config_file, db_info ) );
			return true;
		}

		//------------------------------------------------------------------------		
		public bool IsFinish( int db_idx )
		{
			if( mDBDic.ContainsKey( db_idx ) )
			{
				return mDBDic[db_idx].IsFinish();
			}

			return false;
		}

		//------------------------------------------------------------------------		
		public bool IsFinish()
		{
			foreach( DBInfo db in mDBDic.Values )
			{
				if( db.IsFinish() == false )
					return false;
			}

			return true;
		}

		//------------------------------------------------------------------------		
		public DatabaseMain GetDatabase( int db_idx )
		{
			DBInfo info;
			if( mDBDic.TryGetValue( db_idx, out info ) )
				return info.m_Database;

			return null;
		}

		//------------------------------------------------------------------------		
		public void SetLogType( eCoreLogType log_type )
		{
			foreach( DBInfo dbinfo in mDBDic.Values )
			{
				if( dbinfo.m_Database != null )
					dbinfo.m_Database.Config.CoreLogType = log_type;
			}
		}

		//------------------------------------------------------------------------		
		public void UpdateDBConnectInfo(UMPServerApplication application, DatabaseMain database, string gamedb_config_file)
		{
			SP_GameDatabase_Get _SP_GameDatabase_Get = new SP_GameDatabase_Get();
			SP_GameDatabase_Get_Ack ackData = DBHandlerExecute.ExecuteSync<SP_GameDatabase_Get, SP_GameDatabase_Get_Ack>( _SP_GameDatabase_Get, database );
			if( ackData.return_code == 0 )
			{
				foreach( GAMEDB_INFO info in ackData.db_info_list )
				{
					AddDatabase( application, gamedb_config_file, info );
				}
			}
		}

		//------------------------------------------------------------------------		
		public void UpdateSqlExceptionNotify( bool bNotify )
		{
			foreach( DBInfo dbinfo in mDBDic.Values )
			{
				if( dbinfo.m_Database != null )
					dbinfo.m_Database.Config.SqlExceptionNotify = bNotify;
			}
		}

		//------------------------------------------------------------------------	
		public string ShowInfo()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine( "GAME : " );
			foreach( DBInfo dbinfo in mDBDic.Values )
			{
				sb.AppendLine( string.Format( "- {0}:{1} SqlExNotify:{2}", dbinfo.m_DBInfo.db_idx, dbinfo.m_Database.Config.DatabaseName, dbinfo.m_Database.Config.SqlExceptionNotify ) );
			}

			return sb.ToString();
		}
	}
}
