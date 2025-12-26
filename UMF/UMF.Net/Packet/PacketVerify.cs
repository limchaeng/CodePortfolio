//////////////////////////////////////////////////////////////////////////
//
// PacketVerify
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using UMF.Core;

namespace UMF.Net
{
	public class PacketVerifyID
	{
		// verify 301 ~ 309
		public const short VerifiedPacketId = -301;
		public const short VerifyPacketId = -302;
	}

	//------------------------------------------------------------------------	
	[Packet( PacketVerifyID.VerifyPacketId, eCoreLogType.Always )]
	public class PacketVerify
	{
		public string verify_string;
		public short protocol_version;
	}

	//------------------------------------------------------------------------	
	[Packet( PacketVerifyID.VerifiedPacketId, eCoreLogType.Always )]
	public class PacketVerified
	{
		public short protocol_version;
	}
}
