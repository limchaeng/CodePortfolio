//////////////////////////////////////////////////////////////////////////
//
// ContentsRelayConnector
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

namespace UMP.Server.Contents
{
	//------------------------------------------------------------------------	
	public class ContentsRelayConnector : AppSSRelayConnector
	{
		protected ContentsServerApplication mApplication = null;
		public ContentsServerApplication Application
		{
			get { return mApplication; }
		}


		public ContentsRelayConnector( ContentsServerApplication application, string config_file, PeerManagerBase relay_peer_manager )
			: this( application, config_file, typeof( NPID_CS2R ), new R2CS_PacketHandlerManagerStandard( application ), relay_peer_manager )
		{

		}
		public ContentsRelayConnector( ContentsServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packetHandlerManager, PeerManagerBase relay_peer_manager )
			: base( application, config_file, packetHandlerManager, send_packet_id_type, typeof( NPID_CS2R ), relay_peer_manager )
		{
			mApplication = application;
		}
	}
}
