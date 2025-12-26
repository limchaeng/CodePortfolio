//////////////////////////////////////////////////////////////////////////
//
// ClientContentsPeerManager
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

namespace UMP.Server.Contents
{
	//------------------------------------------------------------------------
	public class ClientContentsPeerManagerStandard : ClientContentsPeerManager<ClientContentsPeer>
	{
		public ClientContentsPeerManagerStandard( ContentsServerApplication application, string config_file )
			: this( application, config_file, typeof( NPID_CS2C ), new C2CS_PacketHandlerManagerStandard( application ) )
		{
		}
		public ClientContentsPeerManagerStandard( ContentsServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packet_handler )
			: base( application, config_file, send_packet_id_type, packet_handler )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class ClientContentsPeerManager<ST> : AppPeerManager<ST> where ST : ClientContentsPeer, new()
	{
		protected ContentsServerApplication mApplication = null;
		public ContentsServerApplication Application { get { return mApplication; } }
		protected override string PEER_INDEX_SAVE_FILE_NAME => "_index_save_contents.xml";

		public ClientContentsPeerManager( ContentsServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packet_handler )
			: base( application, config_file, packet_handler, send_packet_id_type, typeof( NPID_CS2C ) )
		{
			mApplication = application;
			mApplication.ClientPeerManager = this;
		}
	}
}
