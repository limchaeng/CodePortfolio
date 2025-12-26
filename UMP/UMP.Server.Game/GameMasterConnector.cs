//////////////////////////////////////////////////////////////////////////
//
// GameMasterConnector
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
using System;
using UMF.Net;

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------	
	public class GameMasterConnector : ServerMasterConnector
	{
		protected GameServerApplication mApplication = null;
		public GameServerApplication Application { get { return mApplication; } }

		public GameMasterConnector( GameServerApplication application, string config_file )
			: this( application, config_file, typeof( NPID_S2M ), new NM2S_PacketHandlerManager<GameMasterConnector>( application ) )
		{

		}
		public GameMasterConnector( GameServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packetHandlerManager )
			: base( application, config_file, send_packet_id_type, typeof( NPID_S2M ), packetHandlerManager )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------
		public override void SendServerConnectionInfo()
		{
			NS2M_ServerConnectionInfo _NS2M_ServerConnectionInfo = new NS2M_ServerConnectionInfo();
			_NS2M_ServerConnectionInfo.info_data = new NP_ServerInfoData();
			_NS2M_ServerConnectionInfo.info_data.server_type = mApplication.ServerType;
			_NS2M_ServerConnectionInfo.info_data.world_idn = mApplication.GetApplicationConfig.WorldIDN;
			_NS2M_ServerConnectionInfo.info_data.server_net_index = mApplication.ServerNetIndex;
			_NS2M_ServerConnectionInfo.info_data.server_guid = mApplication.GUID_LONG;
			_NS2M_ServerConnectionInfo.info_data.notify_host_name = mApplication.ClientPeerManager.NotifyHostName;
			_NS2M_ServerConnectionInfo.info_data.notify_port = mApplication.ClientPeerManager.BoundPort;
			_NS2M_ServerConnectionInfo.info_data.peer_count = mApplication.ClientPeerManager.PeerCount;
			_NS2M_ServerConnectionInfo.info_data.connection_key = ConnectionKeyManager.Instance.LastConnectionKey;

			SendPacket( _NS2M_ServerConnectionInfo );
		}
	}
}
