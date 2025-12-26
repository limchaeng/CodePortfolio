//////////////////////////////////////////////////////////////////////////
//
// CommonEnums
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

namespace UMP.CSCommon
{
	//------------------------------------------------------------------------
	public enum eServiceType
	{
		Local,
		Dev,
		QA,
		Review,
		Live,
	}

	//------------------------------------------------------------------------	
	public enum eServerType
	{
		Unknown,
		Master,
		Daemon,
		Login,
		World,
		Game,
		Relay,
		Contents,
	}

	//------------------------------------------------------------------------	
	public enum eLoginPlatformInternal : short
	{
		// DO NOT CHANGE ENUM VALUE
		None = 0,
		Guest = -1,
		ID_PW = -2,
		ID_PW_REGIST = -3,
	}

	//------------------------------------------------------------------------
	public enum eAppStoreTypeInternal : short
	{
		// DO NOT CHANGE ENUM VALUE
		None = 0,
		Develop = -1,
	}
}
