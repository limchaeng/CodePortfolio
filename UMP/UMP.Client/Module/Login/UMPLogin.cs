//////////////////////////////////////////////////////////////////////////
//
// UMPLogin
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
using UMP.CSCommon;
using System.Collections.Generic;
using UMP.CSCommon.Packet;

namespace UMP.Client.Module.Login
{
	public class UMPLogin : Singleton<UMPLogin>
	{
		public enum eLoginProgress
		{
			PlatformLogin,
			LoginServerConnect,
			Login,
			Finished,
			FastFinished,
		}

		public delegate void delegatePlatformLoginCallback( string error_msg, string user_id, string auth_key, string display_name, string extra_data );
		public delegate void delegateLoginProcessHandler( bool bSuccess, eLoginProgress progress, int error, string error_msg );
		public delegate void delegatePlatformConnectHandler( bool bSuccess );

		delegateLoginProcessHandler mLoginHandler = null;

		//eLoginPlatformReserved mPlatformLoginType = eLoginPlatformReserved.None;
		bool mIsIDPWRegist = false;
		string mPlatformUserID = "";
		public string PlatformUserID { get { return mPlatformUserID; } }
		string mPlatformAuthKey = "";
		public string PlatformAuthKey { get { return mPlatformAuthKey; } }
		string mPlatformUserName = "";
		public string PlatformUserName { get { return mPlatformUserName; } }
		string mPlatformExtraData = "";
		public string PlatformExtraData { get { return mPlatformExtraData; } }


		string mPlatformConnectLoginID = "";
		string mPlatformConnectLoginKey = "";
		string mPlatformConnectLoginName = "";

		long mGameServerGUID = 0;
		int mGameServerID = 0;
		public long GameServerGUID { get { return mGameServerGUID; } }
		public int GameServerID { get { return mGameServerID; } }

		bool mFastLogin = false;
		byte mFastLoginUniqueIDX = 0;
		int mPrevSocketUserIndex = 0;

// 		eAppVerifyRequestTypeFlag mAppVerifyTypeFlag = eAppVerifyRequestTypeFlag.None;
// 		List<CS_AppVerifyNotifyData> mAppVerifyNotifyList = null;

		//------------------------------------------------------------------------
		public UMPLogin()
		{
			mGameServerGUID = 0;
			mGameServerID = 0;
			mFastLogin = false;
			mFastLoginUniqueIDX = 0;

			//NetworkHandler.Instance.Add_Handler<NL2C_LoginAck>( L2C_LoginAckHandler );
		}

		//------------------------------------------------------------------------	
		public void Login(ILoginPlatform platform, delegateLoginProcessHandler process_callback)
		{
			platform.Login( ( string error_msg, string user_id, string auth_key, string display_name, string extra_data ) =>
			{
				if( string.IsNullOrEmpty(error_msg) )
				{
					mPlatformUserID = user_id;
					mPlatformAuthKey = auth_key;
					mPlatformUserName = display_name;
					mPlatformExtraData = extra_data;

					process_callback( true, eLoginProgress.PlatformLogin, 0, "" );

					// connect to login

				}
				else
				{
					process_callback( false, eLoginProgress.PlatformLogin, -1, error_msg );
				}
			} );
		}

		//------------------------------------------------------------------------
		public void Logout()
		{

		}

		//////////////////////////////////////////////////////////////////////////
		///
		void L2C_LoginAckHandler(NL2C_LoginAck packet)
		{

		}
	}
}
