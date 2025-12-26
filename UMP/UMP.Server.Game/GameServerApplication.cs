//////////////////////////////////////////////////////////////////////////
//
// GameServerApplication
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

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------	
	public class GameServerApplication : UMPServerApplication
	{
		public PeerManagerBase ClientPeerManager { get; set; } = null;
		public GameRelayConnector RelayConnector { get; set; } = null;
		public DatabaseMain DBWorld { get; private set; } = null;
		public DatabaseMain DBAuth { get; private set; } = null;
		public DatabaseMain DBCommon { get; private set; } = null;

		public int ClientLimitCount { get; set; } = 5000;

		public GameServerApplication( string server_name, eServiceType service_type, string config_file, DatabaseMain world_db, DatabaseMain auth_db, DatabaseMain common_db, string game_db_config, string[] args )
			: base( server_name, eServerType.Game, service_type, config_file, args )
		{
			DBWorld = world_db;
			if( world_db != null )
			{
				world_db.TestConnection();
				AddUpdater( world_db.Update );
				mDBDic.Add( eUMPAppDBType.World, world_db );
			}

			DBAuth = auth_db;
			if( auth_db != null )
			{
				auth_db.TestConnection();
				AddUpdater( auth_db.Update );
				mDBDic.Add( eUMPAppDBType.Auth, auth_db );

				GameDBManager.Instance.UpdateDBConnectInfo( this, auth_db, game_db_config );
			}

			DBCommon = common_db;
			if( common_db != null )
			{
				common_db.TestConnection();
				AddUpdater( common_db.Update );
				mDBDic.Add( eUMPAppDBType.Common, common_db );
			}

			ConnectionKeyManager.Instance.OnUpdatedConnectionKey = OnUpdatedConnectionKey;
			AddUpdater( ConnectionKeyManager.Instance.Update );
			AddUpdater( GameDBManager.Instance.Update );

			AccountManager.Instance.Init( this );
		}

		//------------------------------------------------------------------------
		protected virtual void OnUpdatedConnectionKey(long new_connecton_key)
		{
			if( mMasterConnector != null && mMasterConnector.Connected )
			{
				NS2M_ServerConnectionKeyUpdate _NS2M_ServerConnectionKeyUpdate = new NS2M_ServerConnectionKeyUpdate();
				_NS2M_ServerConnectionKeyUpdate.server_type = mServerType;
				_NS2M_ServerConnectionKeyUpdate.peer_count = ClientPeerManager.PeerCount;
				_NS2M_ServerConnectionKeyUpdate.update_connection_key = new_connecton_key;

				mMasterConnector.SendPacket( _NS2M_ServerConnectionKeyUpdate );
			}
		}

		//------------------------------------------------------------------------
		protected override bool IsFinishedForShutdown()
		{
			if( base.IsFinishedForShutdown() == false )
				return false;

			if( GameDBManager.Instance.IsFinish() == false )
				return false;

			if( ClientPeerManager.PeerCount > 0 )
				return false;

			// TODO : LOG DB

			return true;
		}
	}
}
