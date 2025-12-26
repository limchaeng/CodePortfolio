//////////////////////////////////////////////////////////////////////////
//
// ContentsRelayPeer
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
using System.IO;

namespace UMP.Server.Relay
{
	//------------------------------------------------------------------------	
	public class ContentsRelayPeer : AppSSRelayPeer 
	{
		public string NotifyHost { get; set; }
		public short NotifyPort { get; set; }

		//------------------------------------------------------------------------
		public override void Init( UMPServerApplication application, PeerManagerBase peerManager, Socket socket )
		{
			base.Init( application, peerManager, socket );

			NotifyHost = "";
			NotifyPort = 0;

			if( mUMPApplication.IsShutdown )
				Disconnect( (int)eDisconnectErrorCode.ServerMaintenance, "relay server shutdown" );
		}
	}
}
