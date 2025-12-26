//////////////////////////////////////////////////////////////////////////
//
// UMPPacektCore
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
using System.Net;
using System.Collections.Generic;
using UMF.Net;
using UMP.CSCommon;
using UMF.Core;

namespace UMP.Server
{
	//------------------------------------------------------------------------
	public struct UMPServerPacketId
	{
		public const short MasterVerifyPacketId = -351;
	}

	//------------------------------------------------------------------------	
	[Packet( UMPServerPacketId.MasterVerifyPacketId, eCoreLogType.Always )]
	public class MasterPacketVerify : PacketVerify
	{
		public eServerType server_type;
		public int world_idn;
		public long guid;
	}
}
