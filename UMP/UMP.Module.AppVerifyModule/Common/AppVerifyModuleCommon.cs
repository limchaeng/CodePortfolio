//////////////////////////////////////////////////////////////////////////
//
// AppVerifyModuleCommon
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

namespace UMP.Module.AppVerifyModule
{
	public class AppVerifyModuleCommon
	{
		public const string MODULE_NAME = "UMP.Module.AppVerifyModule";
		public const short ProtocolVersion = 1;
	}

	//------------------------------------------------------------------------	
	[System.Flags]
	public enum eAppVerifyRequestTypeFlag : int
	{
		None = 0x0000,
		KeySign = 0x0001,
		CRC = 0x0002,
		MD5 = 0x0004,
		HackFile = 0x0008,
		Installer = 0x0010,
		IOSJB = 0x0020,
		IOSLIB = 0x0040,
	}

	//------------------------------------------------------------------------	
	public class CS_AppVerifyData
	{
		public string v1;
		public string v2;
		public string v3;
		public string v4;
		public string v5;
	}

	//------------------------------------------------------------------------
	public class CS_AppVerifyNotifyData
	{
		public eAppVerifyRequestTypeFlag app_verify_types;
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public List<string> app_verify_notify_list;
	}
}
