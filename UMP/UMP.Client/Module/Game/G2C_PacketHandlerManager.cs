//////////////////////////////////////////////////////////////////////////
//
// G2C_PacketHandlerManager
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMF.Net;
using UMP.CSCommon.Packet;

namespace UMP.Client
{
	//------------------------------------------------------------------------
	public class G2C_PacketHandlerManagerStandard : G2C_PacketHandlerManager<GameConnector>
	{
		public G2C_PacketHandlerManagerStandard()
			: base( typeof( NPID_G2C ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class G2C_PacketHandlerManager<ST> : PacketHandlerManager<ST> where ST : GameConnector
	{
		public G2C_PacketHandlerManager( Type packet_id_type )
			: base( packet_id_type, typeof( NPID_G2C ) )
		{
		}

		//////////////////////////////////////////////////////////////////////////
		///
		//------------------------------------------------------------------------
		[PacketHandler(PacketType = typeof( NG2C_AccountFastLoginAck ) )]
		protected virtual void NG2C_AccountFastLoginAckHandler( ST session, object _packet )
		{
			NG2C_AccountFastLoginAck packet = _packet as NG2C_AccountFastLoginAck;
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NG2C_AccountLoginAck ) )]
		protected virtual void NG2C_AccountLoginAckHandler( ST session, object _packet )
		{
			NG2C_AccountLoginAck packet = _packet as NG2C_AccountLoginAck;
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NG2C_LocalizeTextFastUpdateNotify ) )]
		protected virtual void NG2C_LocalizeTextFastUpdateNotifyHandler( ST session, object _packet )
		{
			NG2C_LocalizeTextFastUpdateNotify packet = _packet as NG2C_LocalizeTextFastUpdateNotify;

		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NG2C_TBLDataUpdateNotify ) )]
		protected virtual void NG2C_TBLDataUpdateNotifyHandler( ST session, object _packet )
		{
			NG2C_TBLDataUpdateNotify packet = _packet as NG2C_TBLDataUpdateNotify;

		}
	}
}
