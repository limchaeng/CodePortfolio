//////////////////////////////////////////////////////////////////////////
//
// RelayServerApplication
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

using UMP.CSCommon;
using UMF.Net;
using UMF.Database;

namespace UMP.Server.Relay
{
	//------------------------------------------------------------------------	
	public class RelayServerApplication : UMPServerApplication
	{
		public PeerManagerBase GamePeerManager { get; set; } = null;
		public PeerManagerBase ContentsPeerManager { get; set; } = null;

		public DatabaseMain DBWorld { get; private set; } = null;
		public DatabaseMain DBCommon { get; private set; } = null;		

		public RelayServerApplication( string server_name, eServiceType service_type, DatabaseMain world_db, DatabaseMain common_db, string config_file, string[] args )
			: base( server_name, eServerType.Relay, service_type, config_file, args )
		{
			DBWorld = world_db;
			if( world_db != null )
			{
				world_db.TestConnection();
				AddUpdater( world_db.Update );
			}

			DBCommon = common_db;
			if( common_db != null )
			{
				common_db.TestConnection();
				AddUpdater( common_db.Update );
			}
		}

		//------------------------------------------------------------------------
		protected override bool IsFinishedForShutdown()
		{
			if( base.IsFinishedForShutdown() == false )
				return false;

			if( GamePeerManager.PeerCount > 0 || ContentsPeerManager.PeerCount > 0 )
				return false;

			if( DBWorld != null && DBWorld.IsFinish == false )
				return false;

			if( DBCommon != null && DBCommon.IsFinish == false )
				return false;

			return true;
		}
	}
}
