//////////////////////////////////////////////////////////////////////////
//
// C2CS_PacketHandlerManager
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
using UMP.CSCommon.Packet;

namespace UMP.Server.Contents
{
	//------------------------------------------------------------------------
	public class C2CS_PacketHandlerManagerStandard : C2CS_PacketHandlerManager<ClientContentsPeer>
	{
		public C2CS_PacketHandlerManagerStandard( ContentsServerApplication application )
			: base( application, typeof( NPID_C2CS ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class C2CS_PacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : ClientContentsPeer
	{
		protected ContentsServerApplication mApplication = null;
		public ContentsServerApplication Application
		{
			get { return mApplication; }
		}

		public C2CS_PacketHandlerManager( ContentsServerApplication application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_C2CS ) )
		{
			mApplication = application;
		}
	}
}
