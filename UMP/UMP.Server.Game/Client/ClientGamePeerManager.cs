//////////////////////////////////////////////////////////////////////////
//
// ClientGamePeerManager
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
using System.Xml;
using System.IO;
using UMF.Core;
using System.Diagnostics;

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------
	public class ClientGamePeerManagerStandard : ClientGamePeerManager<ClientGamePeer>
	{
		public ClientGamePeerManagerStandard( GameServerApplication application, string config_file )
			: this( application, config_file, typeof( NPID_G2C ), new C2G_PacketHandlerManagerStandard( application ) )
		{
		}
		public ClientGamePeerManagerStandard( GameServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packet_handler )
			: base( application, config_file, send_packet_id_type, packet_handler )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class ClientGamePeerManager<ST> : AppPeerManager<ST> where ST : ClientGamePeer, new()
	{
		protected GameServerApplication mApplication = null;
		public GameServerApplication Application { get { return mApplication; } }

		protected override string PEER_INDEX_SAVE_FILE_NAME => "_index_save_game.xml";

		public ClientGamePeerManager( GameServerApplication application, string config_file )
			: this( application, config_file, typeof( NPID_G2C ), new C2G_PacketHandlerManagerStandard( application ) )
		{
		}
		public ClientGamePeerManager( GameServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packet_handler )
			: base( application, config_file, packet_handler, send_packet_id_type, typeof( NPID_G2C ) )
		{
			mApplication = application;
			mApplication.ClientPeerManager = this;
		}
	}
}
