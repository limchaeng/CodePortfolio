//////////////////////////////////////////////////////////////////////////
//
// ModuleTestServer
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
#if UMSERVER

using UMF.Net.Module;
using UMF.Net;
using System;
using UMF.Core;
using UMP.CSCommon.Packet;

namespace UMM.Module.ModuleTest
{
	public class ModuleTestServer : ModuleNetPeer
	{
		public override string ModuleName => ModuleTestCommon.MODULE_NAME;
		protected override short ProtocolVersion => ModuleTestCommon.ProtocolVersion;
		public override Type SendPacketIdType => typeof( ModuleTestCommon.eNL2C );
		public override Type NSendPacketIdType => typeof( ModuleTestCommon.eNL2C );
		public override Type RecvPacketIdType => typeof( ModuleTestCommon.eNC2L );
		public override Type NRecvPacketIdType => typeof( ModuleTestCommon.eNC2L );


		//------------------------------------------------------------------------		
		public ModuleTestServer( PeerManagerBase peer_manager )
			: base( peer_manager )
		{
			//AddPacketHandler<NC2L_TestC2L>( NC2L_TestC2LHandler );
		}

		[PacketHandler( PacketType = typeof( NC2L_TestC2L ) )]
		private void NC2L_TestC2LHandler( Peer session, object _packet )
		{
			NC2L_TestC2L packet = _packet as NC2L_TestC2L;

			Log.WriteImportant( "Module : NC2L_TestC2LHandler : {0} send:{1}", session.GetType().FullName, packet.send );

			NL2C_TestC2LAck _NL2C_TestC2LAck = new NL2C_TestC2LAck();
			_NL2C_TestC2LAck.send_ack = packet.send + 10000;

			SendPacket( _NL2C_TestC2LAck, session );
		}

		[PacketRecvInterruptHandler(PacketType = typeof( NC2L_Login ) )]
		void NC2L_LoginInterrupt( Session session, object _packet )
		{
			NC2L_Login packet = _packet as NC2L_Login;

			Log.WriteImportant( "Module : NC2L_LoginInterrupt : {0}", session.GetType().FullName );
		}

		[PacketSendInterruptHandler(PacketType = typeof(NL2C_LoginAck))]
		void NL2C_LoginAckSendInterrupt( object _packet, Session session)
		{
			Log.WriteImportant( "Module : NL2C_LoginAckSendInterrupt : {0}", session.GetType().FullName );
		}
	}
}

#endif