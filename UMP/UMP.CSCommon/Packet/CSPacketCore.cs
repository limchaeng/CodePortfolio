//////////////////////////////////////////////////////////////////////////
//
// CSVerifyPacket
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

using UMF.Core;
using UMF.Net;

namespace UMP.CSCommon.Packet
{
	//------------------------------------------------------------------------	
	public struct CSPacketId
	{
		public const short ClientVerifyPacketId = -401;
		public const short ClientKeyVerifyPacketId = -402;
		public const short ContentsClientKeyVerifyPacketId = -403;
	}

	//------------------------------------------------------------------------	
	[Packet( CSPacketId.ClientVerifyPacketId, eCoreLogType.Always )]
	public class ClientPacketVerify : PacketVerify
	{
		public short application_identifier;
		public string version;
		public int revision;
		public int runtime_platform;
		public string device_language;
		public string app_language;
	}

	//------------------------------------------------------------------------	
	[Packet( CSPacketId.ClientKeyVerifyPacketId, eCoreLogType.Always )]
	public class ClientKeyPacketVerify : PacketVerify
	{
		public string version;
		public int revision;
		public short application_identifier;
		public int runtime_platform;
		public string device_language;
		public string app_language;
		public long connection_key;
	}

	//------------------------------------------------------------------------
	[Packet(CSPacketId.ContentsClientKeyVerifyPacketId, eCoreLogType.Always)]
	public class ContentsClientKeyVerify : ClientKeyPacketVerify
	{
		public long account_idx;
		public int contents_flag;
	}

}
