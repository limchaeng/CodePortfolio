//////////////////////////////////////////////////////////////////////////
//
// CSCommonData
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
using UMF.Core;

namespace UMP.CSCommon
{
	//------------------------------------------------------------------------
	public class AppIdentifier : IdentifierShort<AppIdentifier> { }
	public class AuthPlatformIdentifier : IdentifierShort<AuthPlatformIdentifier> { }
	public class AppStoreIdentifier : IdentifierShort<AppStoreIdentifier> { }

	//------------------------------------------------------------------------
	public class CS_AccountDeviceData
	{
		public string device_type;
		public string device_id;
		public string device_location;
		public string device_os_version;
		public string device_language;
		public string device_advertising_id;
		public string device_package_id;
	}

	//------------------------------------------------------------------------
	public class CS_AuthPlatformData
	{
		public short auth_platform_type;
		public string auth_platform_id;
		public string auth_platform_key;
		public string auth_platform_name;
	}

	//------------------------------------------------------------------------	
	public class CS_AuthPlatformRegistData
	{
		public short platform_type;
		public string platform_name;
		public string platform_extra;
		public DateTime regist_time;
	}

	//------------------------------------------------------------------------
	public static class CSSecurity
	{
		[System.Flags]
		public enum eSecurityFlags : int
		{
			None = 0x0000,
			MaintenanceLogin = 0x0001,
		}

		public static bool IsAllow( eSecurityFlags value, eSecurityFlags flags )
		{
			return ( ( value & flags ) != 0 );
		}
	}
}

