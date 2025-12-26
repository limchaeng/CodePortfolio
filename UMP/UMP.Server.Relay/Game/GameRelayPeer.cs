//////////////////////////////////////////////////////////////////////////
//
// GameRelayPeer
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
using UMF.Core;
using UMP.CSCommon;
using UMP.CSCommon.Packet;
using System.Net.Sockets;
using UMF.Server;

namespace UMP.Server.Relay
{
	//------------------------------------------------------------------------	
	public class GameRelayPeer : AppSSRelayPeer
	{
		public long m_GameGUID = 0;
		public string m_GameHostName = "";
		public short m_GamePort = 0;

		//------------------------------------------------------------------------
		public override void Init( UMPServerApplication application, PeerManagerBase peerManager, Socket socket )
		{
			base.Init( application, peerManager, socket );

			if( mUMPApplication.IsShutdown )
				Disconnect( (int)eDisconnectErrorCode.ServerMaintenance, "relay server shutdown" );
		}
	}
}
