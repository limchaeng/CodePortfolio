//////////////////////////////////////////////////////////////////////////
//
// CS2R_PacketHandlerManager
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

namespace UMP.Server.Relay
{
	//------------------------------------------------------------------------
	public class CS2R_PacketHandlerManagerStandard : CS2R_PacketHandlerManager<ContentsRelayPeer>
	{
		public CS2R_PacketHandlerManagerStandard( RelayServerApplication application )
			: base( application, typeof( NPID_CS2R ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class CS2R_PacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : ContentsRelayPeer
	{
		protected RelayServerApplication mApplication = null;
		public RelayServerApplication Application
		{
			get { return mApplication; }
		}


		public CS2R_PacketHandlerManager( RelayServerApplication application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_CS2R ) )
		{
			mApplication = application;
		}
	}
}
