//////////////////////////////////////////////////////////////////////////
//
// ClientPeer
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
	public class ClientPeer : AppPeer
    {
		protected override short VerifyPacketId => CSPacketId.ClientVerifyPacketId;
		protected override string VerifyPacketLog( object packet )
		{
			return PacketLogFormatter.Instance.Serialize<ClientPacketVerify>( packet, false );
		}

		protected override PacketVerify VerifyPacket(short packetId, MemoryStream stream)
        {
			if( packetId == CSPacketId.ClientVerifyPacketId )
				return PacketReadFormatter.Instance.Serialize<ClientPacketVerify>(new BinaryReader(stream), GetPacketFormatterConfig);

            throw new Exception("Not correct verify packet : " + packetId.ToString());
        }
    }
}