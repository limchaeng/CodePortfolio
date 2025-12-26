//////////////////////////////////////////////////////////////////////////
//
// LoginServerApplication
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
using UMP.CSCommon.Packet;

namespace UMP.Server.Login
{
	//------------------------------------------------------------------------	
	public class LoginServerApplication : UMPServerApplication
	{
		protected ClientLoginPeerManager mClientPeerManager = null;
		public ClientLoginPeerManager ClientPeerManager
		{
			get { return mClientPeerManager; }
			set { mClientPeerManager = value; }
		}

		public LoginServerApplication( string server_name, eServiceType service_type, string config_file, string[] args )
			: base( server_name, eServerType.Login, service_type, config_file, args )
		{
		}

		//------------------------------------------------------------------------
		public NL_FastConnectionData CheckFastLogin(int world_idn, long server_guid)
		{
			NL_FastConnectionData fast_data = null;
			if( world_idn > 0 && server_guid > 0 )
			{
				NP_ServerInfoData gameserver = ServerConnectionManager.Instance.FindServer( world_idn, server_guid );
				if( gameserver != null )
				{
					fast_data = new NL_FastConnectionData();
					fast_data.host_name = gameserver.notify_host_name;
					fast_data.port = gameserver.notify_port;
					fast_data.connection_key = gameserver.connection_key;
				}
			}

			return fast_data;
		}

		//------------------------------------------------------------------------
		public NP_GameServerConnectionData FindFreeGameServer( int world_idn )
		{
			NP_GameServerConnectionData data = null;
			NP_ServerInfoData free_gameserver = ServerConnectionManager.Instance.FindFreeServer( eServerType.Game, world_idn );
			if( free_gameserver != null )
			{
				data = new NP_GameServerConnectionData();
				data.gameserver_host_name = free_gameserver.notify_host_name;
				data.gameserver_port = free_gameserver.notify_port;
				data.connection_key = free_gameserver.connection_key;
			}

			return data;
		}
	}
}
