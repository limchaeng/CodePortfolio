//////////////////////////////////////////////////////////////////////////
//
// ModuleTestClient
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
#if UMCLIENT

using System;
using UMF.Core;
using UMF.Net;
using UMF.Net.Module;

namespace UMM.Module.ModuleTest
{
	public class ModuleTestClient : ModuleNetConnector
	{
		public override string ModuleName => ModuleTestCommon.MODULE_NAME;
		protected override short ProtocolVersion => ModuleTestCommon.ProtocolVersion;
		public override Type SendPacketIdType => typeof( ModuleTestCommon.eNC2L );
		public sealed override Type NSendPacketIdType => typeof( ModuleTestCommon.eNC2L );
		public override Type RecvPacketIdType => typeof( ModuleTestCommon.eNL2C );
		public sealed override Type NRecvPacketIdType => typeof( ModuleTestCommon.eNL2C );

		//------------------------------------------------------------------------
		public ModuleTestClient( Connector connector )
			: base( connector )
		{
			//AddPacketHandler<NL2C_TestC2LAck>( NL2C_TestC2LAckHandler );
		}

		//------------------------------------------------------------------------
		public void SendTest()
		{
			NC2L_TestC2L _NC2L_TestC2L = new NC2L_TestC2L();
			_NC2L_TestC2L.send = 9999;

			SendPacket( _NC2L_TestC2L );
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NL2C_TestC2LAck ) )]
		protected virtual void NL2C_TestC2LAckHandler( Connector session, object _packet )
		{
			NL2C_TestC2LAck packet = _packet as NL2C_TestC2LAck;

			Log.WriteImportant( "Module : NL2C_TestC2LAckHandler : {0} send_ack:{1}", session.GetType().FullName, packet.send_ack );
		}
	}
}

#endif
