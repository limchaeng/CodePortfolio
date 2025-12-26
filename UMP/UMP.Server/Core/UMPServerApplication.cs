//////////////////////////////////////////////////////////////////////////
//
// UMPServerApplication
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

using UMF.Server;
using UMP.CSCommon;
using System.Linq;
using UMF.Core;
using UMF.Net;
using System.Collections.Generic;
using UMF.Database;

namespace UMP.Server
{
	//------------------------------------------------------------------------
	public enum eUMPAppDBType
	{
		None = 0,
		Auth,
		World,
		Common,
		Game,
	}

	//------------------------------------------------------------------------	
	public class UMPServerApplication : ServerApplication
	{
		static UMPServerApplication mUMPInstance = null;
		public static UMPServerApplication UMPInstance { get { return mUMPInstance; } }

		protected eServiceType mServiceType = eServiceType.Local;
		public eServiceType ServiceType { get { return mServiceType; } }
		protected eServerType mServerType = eServerType.Unknown;
		public eServerType ServerType { get { return mServerType; } }

		protected Dictionary<eUMPAppDBType, DatabaseMain> mDBDic = new Dictionary<eUMPAppDBType, DatabaseMain>();

		protected Connector mMasterConnector = null;
		public Connector MasterConnector { get { return mMasterConnector; } set { mMasterConnector = value; } }

		//------------------------------------------------------------------------		
		public UMPServerApplication( string server_name, eServerType server_type, eServiceType service_type, string config_file, string[] args ) 
			: base( server_name, server_type.ToString(), service_type.ToString(), config_file, args )
		{
			mUMPInstance = this;

			mServiceType = service_type;
			mServerType = server_type;

			switch(service_type)
			{
				case eServiceType.Local:
					DataReloader.Instance.ReloadDataSave = false;
					IsMaintenance = false;
					break;

				case eServiceType.Dev:
					IsMaintenance = false;
					break;

				default:
					IsMaintenance = true;
					break;
			}
		}

		//------------------------------------------------------------------------
		protected override bool IsFinishedForShutdown()
		{
			if( base.IsFinishedForShutdown() == false )
				return false;

			foreach( DatabaseMain db in mDBDic.Values )
			{
				if( db.IsFinish == false )
					return false;
			}

			return true;
		}

		//------------------------------------------------------------------------
		public virtual DatabaseMain GetDB( eUMPAppDBType db_type )
		{
			DatabaseMain db;
			if( mDBDic.TryGetValue( db_type, out db ) )
				return db;

			return null;
		}			
	}
}
