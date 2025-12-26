//////////////////////////////////////////////////////////////////////////
//
// IPlatformAuthModule
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
using System.Collections;
using UMF.Core;

namespace UMP.Server
{
	//------------------------------------------------------------------------
	public class PlatformAuthResult
	{
		public int error_code;
		public string error_message;
		public string platform_name;
		public string platform_extra;
	}

	//------------------------------------------------------------------------	
	public interface IPlatformAuthModule
	{
		IEnumerator VerifyAuth( short platform_type, string platform_id, string platform_key, string platform_name, ClassObjectContainer<PlatformAuthResult> result );
		bool IsMigrationFromGuest { get; }
		bool IsAutoRegist { get; }
		bool CheckExistThrow { get; }
	}
}
