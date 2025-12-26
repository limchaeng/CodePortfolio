//////////////////////////////////////////////////////////////////////////
//
// GameRelayConnector
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
	public class GameRelayConnector : AppSSRelayConnector
	{
		protected GameServerApplication mApplication = null;
		public GameServerApplication Application
		{
			get { return mApplication; }
		}
		bool mConnectionInfoSended = false;

		public GameRelayConnector( GameServerApplication application, string config_file, PeerManagerBase client_game_peer_manager )
			: this( application, config_file, typeof( NPID_G2R ), new R2G_PacketHandlerManagerStandard( application ), client_game_peer_manager )
		{

		}
		public GameRelayConnector( GameServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packetHandlerManager, PeerManagerBase client_game_peer_manager )
			: base( application, config_file, packetHandlerManager, send_packet_id_type, typeof( NPID_G2R ), client_game_peer_manager )
		{
			mApplication = application;

			client_game_peer_manager.OnListenerStarted = () =>
			{
				SendConnectionInfo();
			};
		}

		//------------------------------------------------------------------------
		protected override void OnDisconnected()
		{
			base.OnDisconnected();
			mConnectionInfoSended = false;
			mApplication.ServerNetIndex = 0;
		}

		//------------------------------------------------------------------------
		public override void OnConnected( bool bSuccessed )
		{
			base.OnConnected( bSuccessed );

			if( bSuccessed )
			{
				if( mRelayPeerManager.BoundHost != null )
					SendConnectionInfo();
			}
		}
		protected virtual void SendConnectionInfo()
		{
			if( mConnectionInfoSended )
				return;

			mConnectionInfoSended = true;

			NG2R_ConnectionInfo _NG2R_ConnectionInfo = new NG2R_ConnectionInfo();
			_NG2R_ConnectionInfo.guid = mApplication.GUID_LONG;
			_NG2R_ConnectionInfo.host_name = mRelayPeerManager.NotifyHostName;
			_NG2R_ConnectionInfo.port = mRelayPeerManager.BoundPort;
			SendPacket( _NG2R_ConnectionInfo );
		}
	}
}
