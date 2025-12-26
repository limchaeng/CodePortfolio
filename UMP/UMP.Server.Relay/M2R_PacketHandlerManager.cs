//////////////////////////////////////////////////////////////////////////
//
// M2R_PacketHandlerManager
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

namespace UMP.Server.Relay
{
	public class M2R_PacketHandlerManagerStandard : M2R_PacketHandlerManager<MasterConnector>
	{
		public M2R_PacketHandlerManagerStandard( RelayServerApplication application )
			: base( application, typeof( NPID_M2S ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class M2R_PacketHandlerManager<ST> : NM2S_PacketHandlerManager<ST> where ST : MasterConnector
	{
		protected RelayServerApplication mApplication = null;
		public RelayServerApplication Application
		{
			get { return mApplication; }
		}

		public M2R_PacketHandlerManager( RelayServerApplication application, Type packet_id_type )
			: base( application, packet_id_type )
		{
			mApplication = application;
		}
	}
}
