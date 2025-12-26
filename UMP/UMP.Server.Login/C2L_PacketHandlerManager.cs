//////////////////////////////////////////////////////////////////////////
//
// C2L_PacketHandlerManager
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

namespace UMP.Server.Login
{
	//------------------------------------------------------------------------
	public class C2L_PacketHandlerManagerStandard : C2L_PacketHandlerManager<ClientLoginPeer>
	{
		public C2L_PacketHandlerManagerStandard(LoginServerApplication application)
			: base( application, typeof( NPID_C2L ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class C2L_PacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : ClientLoginPeer
	{
		protected LoginServerApplication mApplication = null;
		public LoginServerApplication Application
		{
			get { return mApplication; }
		}
		public C2L_PacketHandlerManager( LoginServerApplication application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_C2L ) )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------
		[PacketHandler(PacketType = typeof(NC2L_Login))]
		protected virtual void NC2L_LoginHandler(ST session, object _packet)
		{
			NC2L_Login packet = _packet as NC2L_Login;

			session.CurrLanguage = packet.curr_localize;

			NL_FastConnectionData fast_data = mApplication.CheckFastLogin( packet.world_idn, packet.gameserver_guid );

			NL2C_LoginAck _L2C_LoginAck = new NL2C_LoginAck();
			_L2C_LoginAck.gameserver_connection_data = null;
			_L2C_LoginAck.fast_connection_data = fast_data;
			if( fast_data == null )
				_L2C_LoginAck.gameserver_connection_data = mApplication.FindFreeGameServer( packet.world_idn );

			session.SendPacket( _L2C_LoginAck );
		}
	}
}
