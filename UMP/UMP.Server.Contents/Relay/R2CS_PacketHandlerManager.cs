//////////////////////////////////////////////////////////////////////////
//
// R2CS_PacketHandlerManager
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

namespace UMP.Server.Contents
{
	//------------------------------------------------------------------------
	public class R2CS_PacketHandlerManagerStandard : R2CS_PacketHandlerManager<ContentsRelayConnector>
	{
		public R2CS_PacketHandlerManagerStandard( ContentsServerApplication application )
			: base( application, typeof( NPID_R2CS ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class R2CS_PacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : ContentsRelayConnector
	{
		protected ContentsServerApplication mApplication = null;
		public ContentsServerApplication Application
		{
			get { return mApplication; }
		}

		public R2CS_PacketHandlerManager( ContentsServerApplication application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_R2CS ) )
		{
			mApplication = application;
				 
		}
	}
}
