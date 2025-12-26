//////////////////////////////////////////////////////////////////////////
//
// ModuleTestCommon
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

using System.Collections.Generic;
using UMF.Net;

namespace UMM.Module.ModuleTest
{
	public class ModuleTestCommon
	{
		public const string MODULE_NAME = "UMM.Module.ModuleTest";
		public const short ProtocolVersion = 1;

		[PacketVersion( Version = ModuleTestCommon.ProtocolVersion )]
		public enum eNC2L : short
		{
			__BEGIN = short.MinValue + 1,
			TestC2L,
		}

		[PacketVersion( Version = ModuleTestCommon.ProtocolVersion )]
		public enum eNL2C : short
		{
			__BEGIN = short.MinValue + 1,
			TestC2LAck,
		}
	}

	[Packet( ModuleTestCommon.eNC2L.TestC2L )]
	public class NC2L_TestC2L
	{
		public int send;
	}

	[Packet( ModuleTestCommon.eNL2C.TestC2LAck )]
	public class NL2C_TestC2LAck
	{
		public int send_ack;
	}
}
