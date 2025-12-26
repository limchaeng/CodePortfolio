//////////////////////////////////////////////////////////////////////////
//
// NetEnums
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

namespace UMF.Net
{
	//------------------------------------------------------------------------	
	public enum eCheckEmptyHandlerType
	{
		Log,
		Exception,
		Manual,
	}

	//------------------------------------------------------------------------	
	public enum eDisconnectErrorCode : int
	{
		Normal = 0,

		SystemError = -1,
		DatabaseError = -2,
		PacketInputError = -3,
		PacketHandleError = -4,
		VerifyError = -5,
		HeartbeatFirstError = -6,
		HeartbeatError = -7,
		ProtocolVersionTooLow = -8,
		DecryptError = -9,
		PacketOutputError = -10,
		UnknownSocketDisconnect = -11,

		ClientException = -100,
		ConnectFail = -101,
		ClientQuit = -102,
		ClientDisconnect = -103,

		GameServerDown = -1000,
		Shutdown = -1001,
		WaitConnectionTimeout = -1002,

		VersionCheckError = 11,
		BackToLogin = 12,
		CannotFindPeer = 13,
		LoginDuplicated = 14,
		CannotFindDB = 15,

		LoginAuthPlatformError = 16,
		LoginAuthBlocked = 17,
		LoginWithdrawalAccount = 18,
		LoginAuthFailed = 19,
		LoginCheckFailed = 20,
		PlayerInvalid = 21,

		UnknownError = 100,
		InGameError = 101,
		NoWorldServer = 102,
		NoGameServer = 103,
		NoDatabase = 104,
		VersionUpdated = 105,
		ClientLimit = 106,
		ServerMaintenance = 107,
		InvalidConnectionKey = 108,
		PingCheckError = 109,
		KickClient = 110,
		VersionDisallow = 111,
		VersionExpire = 112,
		VersionMax = 113,
		VersionToLive = 114,
		HackDetected = 115,
		ClientInvalid = 116,
		AddictionProtectPlayTime = 117,
		NeedReloginFromTitle = 118,

		BackgroundEnterDisconnect = 1001,

		NewVersionToReview = 1111,

		// custom error code : over 2000
		Err_CustomBegin = 2000,
	}

	//------------------------------------------------------------------------	
	public enum eDisconnectType
	{
		Disconnect,
		Disconnected,
	}

	//------------------------------------------------------------------------	
	public enum eSendPacketResult
	{
		Success,
		VersionLower,
		NotConnected,
		SessionInvalid,
	}
}
