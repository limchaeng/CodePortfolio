//////////////////////////////////////////////////////////////////////////
//
// ContentsMasterConnector
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
	public class ContentsMasterConnector : ServerMasterConnector
	{
		protected ContentsServerApplication mApplication = null;
		public ContentsServerApplication Application
		{
			get { return mApplication; }
		}

		public ContentsMasterConnector( ContentsServerApplication application, string config_file )
			: this( application, config_file, typeof( NPID_S2M ), new NM2S_PacketHandlerManager<ContentsMasterConnector>( application ) )
		{

		}
		public ContentsMasterConnector( ContentsServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packetHandlerManager )
			: base( application, config_file, send_packet_id_type, typeof( NPID_S2M ), packetHandlerManager )
		{
			mApplication = application;
		}
	}
}
