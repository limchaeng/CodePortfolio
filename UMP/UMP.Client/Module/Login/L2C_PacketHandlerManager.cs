//////////////////////////////////////////////////////////////////////////
//
// L2C_PacketHandlerManager
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

namespace UMP.Client
{
	//------------------------------------------------------------------------
	public class L2C_PacketHandlerManagerStandard : L2C_PacketHandlerManager<LoginConnector>
	{
		public L2C_PacketHandlerManagerStandard()
			: base( typeof( NPID_L2C ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class L2C_PacketHandlerManager<ST> : PacketHandlerManager<ST> where ST : LoginConnector
	{
		public L2C_PacketHandlerManager( Type packet_id_type )
			: base( packet_id_type, typeof( NPID_L2C ) )
		{
		}

		//////////////////////////////////////////////////////////////////////////
		///
		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NL2C_LoginAck ) )]
		protected virtual void NL2C_LoginAckHandler(ST session, object _packet)
		{
			NL2C_LoginAck packet = _packet as NL2C_LoginAck;
		}
	}
}
