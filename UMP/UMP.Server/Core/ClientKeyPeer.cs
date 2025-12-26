//////////////////////////////////////////////////////////////////////////
//
// ClientKeyPeer
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
using System.IO;
using UMF.Net;
using UMP.CSCommon.Packet;

namespace UMP.Server
{
	//------------------------------------------------------------------------	
	public class ClientKeyPeer : AppPeer
	{
		protected override short VerifyPacketId => CSPacketId.ClientKeyVerifyPacketId;
		protected override string VerifyPacketLog( object packet )
		{
			return PacketLogFormatter.Instance.Serialize<ClientKeyPacketVerify>( packet, false );
		}

		protected override PacketVerify VerifyPacket(short packetId, MemoryStream stream)
		{
			if( packetId == CSPacketId.ClientKeyVerifyPacketId )
				return PacketReadFormatter.Instance.Serialize<ClientKeyPacketVerify>(new BinaryReader(stream), GetPacketFormatterConfig);

			throw new Exception("Not correct verify packet : " + packetId.ToString());
		}
	}

	//------------------------------------------------------------------------
	public class ContentsClientKeyPeer : ClientKeyPeer
	{
		protected override short VerifyPacketId => CSPacketId.ContentsClientKeyVerifyPacketId;
		protected override string VerifyPacketLog( object packet )
		{
			return PacketLogFormatter.Instance.Serialize<ContentsClientKeyVerify>( packet, false );
		}

		protected override PacketVerify VerifyPacket( short packetId, MemoryStream stream )
		{
			if( packetId == CSPacketId.ContentsClientKeyVerifyPacketId )
				return PacketReadFormatter.Instance.Serialize<ContentsClientKeyVerify>( new BinaryReader( stream ), GetPacketFormatterConfig );

			throw new Exception( "Not correct verify packet : " + packetId.ToString() );
		}
	}
}
