//////////////////////////////////////////////////////////////////////////
//
// WorldModuleClient
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
using UMF.Net.Module;
using UMP.CSCommon.Packet;
using UMF.Core;

namespace UMP.Module.WorldModule
{
	public class WorldModuleClient : ModuleNetConnector
	{
		public override string ModuleName => WorldModuleCommon.MODULE_NAME;
		protected override short ProtocolVersion => WorldModuleCommon.ProtocolVersion;
		public override Type SendPacketIdType => typeof( WorldModuleCommon.NPID_C2L );
		public sealed override Type NSendPacketIdType => typeof( WorldModuleCommon.NPID_C2L );
		public override Type RecvPacketIdType => typeof( WorldModuleCommon.NPID_L2C );
		public sealed override Type NRecvPacketIdType => typeof( WorldModuleCommon.NPID_L2C );

		//------------------------------------------------------------------------
		public WorldModuleClient( Connector connector )
			: base( connector )
		{
		}

		//------------------------------------------------------------------------		
		[PacketRecvInterruptHandler( PacketType = typeof( NL2C_LoginAck ) )]
		protected virtual void NL2C_LoginAckHandleInterrupt( Session session, object _packet )
		{
			NL2C_LoginAck packet = _packet as NL2C_LoginAck;
			CS_WorldContainer container = packet.GetExpandPacketData<CS_WorldContainer>( session );
			PlayerWorldModuleData.Instance.WorldContainer = container;
		}

		//------------------------------------------------------------------------
		[PacketHandler(PacketType = typeof( NL2C_GetWorldListAck ) )]
		private void NL2C_GetWorldListAckHandler( Connector session, object _packet )
		{
			NL2C_GetWorldListAck packet = _packet as NL2C_GetWorldListAck;

			PlayerWorldModuleData.Instance.WorldContainer = packet.update_container;
		}
	}
}
