//////////////////////////////////////////////////////////////////////////
//
// GameRelayPeerManager
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
using UMP.CSCommon.Packet;
using UMF.Server;

namespace UMP.Server.Relay
{
	//------------------------------------------------------------------------
	public class GameRelayPeerManagerStandard : GameRelayPeerManager<GameRelayPeer>
	{
		public GameRelayPeerManagerStandard( RelayServerApplication application, string config_file, SSRelayPeerManager relay_peer_manager )
			: this( application, config_file, typeof( NPID_R2G ), new G2R_PacketHandlerManagerStandard( application ), relay_peer_manager )
		{
		}
		public GameRelayPeerManagerStandard( RelayServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packet_handler, SSRelayPeerManager relay_peer_manager )
			: base( application, config_file, send_packet_id_type, packet_handler, relay_peer_manager )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class GameRelayPeerManager<ST> : AppSSRelayPeerManager<ST> where ST : GameRelayPeer, new()
	{
		protected RelayServerApplication mApplication = null;
		public RelayServerApplication Application
		{
			get { return mApplication; }
		}

		public GameRelayPeerManager( RelayServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packet_handler, SSRelayPeerManager relay_peer_manager )
			: base( application, config_file, packet_handler, send_packet_id_type, typeof( NPID_R2G ), relay_peer_manager )
		{
			mApplication = application;
			mApplication.GamePeerManager = this;
		}
	}
}
