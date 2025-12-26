//////////////////////////////////////////////////////////////////////////
//
// Account
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
using UMP.CSCommon;
using UMF.Database;
using UMP.CSCommon.Packet;
using UMF.Net;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UMF.Core.I18N;

namespace UMP.Server.Game
{
	public class Account : CacheLockObject
	{
		protected GameServerApplication mApplication = null;
		// session data
		public ClientGamePeer ClientSession { get; private set; } = null;
		public ClientGamePeer.SessionData ClientSessionData { get; private set; } = new ClientGamePeer.SessionData();
		public string CurrLanguage { get; private set; } = "";
		public int PeerIndex { get; private set; } = 0;

		// client data
		public CS_AccountDeviceData Device { get; private set; } = null;
		public CS_AuthPlatformData AuthPlatform { get; private set; } = null;

		// auth data
		public long AccountIDX { get; private set; } = 0;
		public string AccountCode { get; private set; } = "";
		public int GameDBIdx { get; private set; } = 0;
		public byte FastLoginVerifyKey { get; set; } = 0;
		public DateTime PendingReLoginTimeout { get; private set; } = DateTime.MinValue;
		protected List<DB_AuthPlatformData> mRegisteredPlatformList = null;
		public DateTime WithdrawalProcessTime { get; private set; } = DateTime.MinValue;

		// game data
		public string Nickname { get; private set; } = "";
		public DateTime LastLoginTime { get; set; } = DateTime.MinValue;
		public DateTime CreatedTime { get; private set; } = DateTime.MinValue;

		public CS_AgreementData.eTypeFlag AgreementFlags { get; set; } = CS_AgreementData.eTypeFlag.None;
		public CSSecurity.eSecurityFlags SecurityFlags { get; private set; }
		public bool HasSecurityFlags(CSSecurity.eSecurityFlags flags) { return CSSecurity.IsAllow( SecurityFlags, flags ); }

		// for each Game
		protected int mSpecialFlagsDBValue = 0;

		protected List<DB_AccountPlayerData> mPlayerList = null;
		protected DB_AccountPlayerData mCurrentPlayer = null;
		public DB_AccountPlayerData CurrentPlayer { get { return mCurrentPlayer; } }

		// dynamic game database
		private DatabaseMain mGameDatabase = null;

		// cache modules
		List<IAccountModuleCacheBase> mModuleCacheList = new List<IAccountModuleCacheBase>();

		//------------------------------------------------------------------------
		public Account( GameServerApplication application, string account_code, ClientGamePeer session, NC2G_AccountLogin c2g_packet, SP_AccountAuth_ACK auth_db_data, SP_AccountGameLogin_ACK game_db_data, SP_Common_AccountLogin_ACK common_db_data )
		{
			mApplication = application;

			UpdateSession( session );

			Device = c2g_packet.device_data;
			AuthPlatform = c2g_packet.auth_data;

			AccountIDX = auth_db_data.account_idx;
			AccountCode = account_code;
			GameDBIdx = auth_db_data.game_db_idx;

			mRegisteredPlatformList = auth_db_data.registered_platform_list;

			Nickname = game_db_data.nickname;
			LastLoginTime = DateTime.Now;
			CreatedTime = game_db_data.created_time;

			AgreementFlags = game_db_data.agreement_flags;
			SecurityFlags = game_db_data.security_flags;
			mSpecialFlagsDBValue = game_db_data.special_flags;

			mPlayerList = game_db_data.player_list;
			mCurrentPlayer = null;

			// TODO : for CommonDB
			if( common_db_data != null )
			{

			}				
		}

		//------------------------------------------------------------------------
		public void UpdateSession(ClientGamePeer session)
		{
			ClientSession = session;
			if( session != null )
			{
				ClientSessionData = session.session_data;
				session.account_idx = AccountIDX;

				CurrLanguage = session.CurrLanguage;
				PeerIndex = session.PeerIndex;
			}
		}

		//------------------------------------------------------------------------	
		public void SendPacket<PacketType>( PacketType packet ) where PacketType : class
		{
			if( ClientSession != null )
			{
				ClientSession.SendPacket<PacketType>( packet );
			}
		}

		//------------------------------------------------------------------------
		public DatabaseMain GameDatabase
		{
			get
			{
				if( mGameDatabase == null )
					mGameDatabase = GameDBManager.Instance.GetDatabase( GameDBIdx );

				if( mGameDatabase != null && mGameDatabase.DBEnabled )
					return mGameDatabase;

				return null;
			}
		}

		//------------------------------------------------------------------------	
		public string GetText( string id, params object[] parms )
		{
			if( string.IsNullOrEmpty( id ) )
				return "";

			if( ClientSession != null )
			{
				return ClientSession.GetText( id, parms );
			}
			else
			{
				return I18NTextMultiLanguage.Instance.GetText( CurrLanguage, id, parms );
			}
		}

		//------------------------------------------------------------------------	
		void SetPendingRelogin()
		{
			PendingReLoginTimeout = DateTime.Now.AddSeconds( AccountConfig.Instance.ReloginTimeoutSeconds );
		}

		//------------------------------------------------------------------------		
		public virtual bool OnDisconnected( int error_code, string error_string, string error_detail )
		{
			ClientSession = null;

			// from prev connection : do not logout process
			if( error_code == (int)eDisconnectErrorCode.UnknownSocketDisconnect )
				return false;

			DateTime server_time = DateTime.Now;
			mCurrentPlayer.last_logout_time = server_time;

			DatabaseMain db = GameDatabase;
			if( db != null )
			{
// 				SP_Player_Logout _SP_Player_Logout = new SP_Player_Logout();
// 				_SP_Player_Logout.account_idx = account_idx;
// 				_SP_Player_Logout.total_playing_time = Math.Min( total_playing_time + (long)( ( server_time - last_login_time ).TotalSeconds ), long.MaxValue );
// 				_SP_Player_Logout.use_addiction_protect_v2 = protect_v2;
// 				_SP_Player_Logout.protect_v2_playing_time = protect_v2_playing_time;
// 				_SP_Player_Logout.server_time = DateTime.Now;
// 				DBHandlerBase.ExecuteDirectNonWaiting<SP_Player_Logout, PROCEDURE_READ_DEFAULT>( _SP_Player_Logout, db );

// 				SP_Common_Logout _SP_Common_Logout = new SP_Common_Logout();
// 				_SP_Common_Logout.account_idx = account_idx;
// 				_SP_Common_Logout.guild_idx = m_GuildCache.guild_idx;
// 				_SP_Common_Logout.server_time = server_time;
// 				DBHandlerBase.ExecuteDirectNonWaiting<SP_Common_Logout, PROCEDURE_READ_DEFAULT>( _SP_Common_Logout, GameServerMain.g_DatabaseCOMMON );

				// LOG
				{
// 					spl_logout _spl_logout = CreateLog<spl_logout>( server_time );
// 					_spl_logout.gameserver_no = GameServerMain.g_ServerInfoManager.m_GameServerID;
// 					_spl_logout.gameserver_ip = string.Format( "{0}:{1}", GameServerMain.g_UserManager.BoundHost.ToString(), GameServerMain.g_UserManager.BoundPort );
// 					_spl_logout.application_identifier = application_identifier.ToString();
// 					_spl_logout.channel_user_id = platform_id;
// 					_spl_logout.device_key = device_key;
// 					_spl_logout.play_time_sec = (int)( server_time - last_login_time ).TotalSeconds;
// 					_spl_logout.error_code = error_code;
// 					if( string.IsNullOrEmpty( error_string ) == false || string.IsNullOrEmpty( error_detail ) == false )
// 					{
// 						if( error_string == error_detail )
// 							_spl_logout.reason = error_string;
// 						else
// 							_spl_logout.reason = error_string + "(E)" + error_detail;
// 					}
// 					else
// 						_spl_logout.reason = "";
// 					_spl_logout.client_ip = mSessionRemoteEndPoint;
// 					_spl_logout.location = mSessionCountryCode;
// 					GameServerMain.ExecuteLog( _spl_logout );
				}
			}

			OnLogout();
			AccountStateManager.Instance.AccountLogout( AccountIDX );

			bool is_pending = false;
			// to relay
			if( mApplication.RelayConnector != null )
			{
				NG2R_AccountLogoutNotify _NG2R_AccountLogoutNotify = new NG2R_AccountLogoutNotify();
				_NG2R_AccountLogoutNotify.account_idx = AccountIDX;
				mApplication.RelayConnector.SendPacket( _NG2R_AccountLogoutNotify );
			}

			if( Session.IsDisconnectedFromNetworkIssue( error_code ) )
			{
				SetPendingRelogin();
				is_pending = true;
			}

			return is_pending;
		}

		//------------------------------------------------------------------------
		public void AddModuleCache(IAccountModuleCacheBase module_cache)
		{
			if( mModuleCacheList.Exists( a => a.ModuleName == module_cache.ModuleName ) )
				throw new Exception( $"Account:AddModuleCache already exist {module_cache.ModuleName}" );

			mModuleCacheList.Add( module_cache );
			mModuleCacheList.Sort( ( a, b ) =>
			{
				return a.Priority.CompareTo( b.Priority );
			} );
		}

		//------------------------------------------------------------------------
		public List<CS_AuthPlatformRegistData> GetAuthPlatformRegistList()
		{
			if( mRegisteredPlatformList != null && mRegisteredPlatformList.Count > 0 )
			{
				return mRegisteredPlatformList.Select( a => a.ToCS() ).ToList();
			}

			return null;
		}

		//------------------------------------------------------------------------
		public IEnumerator OnAccountLogin()
		{
			foreach (IAccountModuleCacheBase cache in mModuleCacheList )
			{
				yield return cache.OnAccountLogin( this );
			}
		}

		//------------------------------------------------------------------------		
		public void OnFastLogin()
		{
			foreach( IAccountModuleCacheBase cache in mModuleCacheList )
			{
				cache.OnFastLogin( this );
			}
		}

		//------------------------------------------------------------------------
		public void OnFinishedLogin( bool is_fast_login )
		{
			AccountManager.Instance.AccountLoginNotify( this, mApplication.ServerNetIndex );

			if( is_fast_login )
				LastLoginTime = DateTime.Now;

			foreach( IAccountModuleCacheBase cache in mModuleCacheList )
			{
				cache.OnFinishedLogin( is_fast_login );
			}

			// TODO : LOG
		}

		//------------------------------------------------------------------------
		protected void OnLogout()
		{
			mModuleCacheList.ForEach( a => a.OnLogout() );
		}

		//------------------------------------------------------------------------
		public IEnumerator OnPlayerLogin( int player_idx)
		{
			mCurrentPlayer = mPlayerList.Find( a => a.player_idx == player_idx );
			if( mCurrentPlayer == null )
				throw new PeerDisconnectException( PeerIndex, (int)eDisconnectErrorCode.PlayerInvalid, "Invalid player" );

			foreach( IAccountModuleCacheBase cache in mModuleCacheList )
			{
				yield return cache.OnPlayerLogin( player_idx );
			}
		}

		//------------------------------------------------------------------------
		public virtual void LocalizeFastUpdateCheck()
		{
			if( LocalizationConfig.Instance.FastUpdate != null )
			{
				List<_P_LocalizeTextFastUpdateNotifyData> list = LocalizationConfig.Instance.FastUpdate.GetFastUpdateData( ClientSessionData.client_version, ClientSessionData.application_identifier, Device.device_package_id, CurrLanguage );
				if( list != null )
				{
					NG2C_LocalizeTextFastUpdateNotify _NG2C_LocalizeTextFastUpdateNotify = new NG2C_LocalizeTextFastUpdateNotify();
					_NG2C_LocalizeTextFastUpdateNotify.update_list = list;
					SendPacket( _NG2C_LocalizeTextFastUpdateNotify );
				}
			}
		}

		//------------------------------------------------------------------------
		public virtual void TBLVersionCheck( List<_P_TBLClientVersionData> tbl_version_list )
		{
			if( tbl_version_list == null || AppConfig.Instance.TBLSendEnable == false )
				return;

			List<_P_TBLUpdateData> update_list = null;
			foreach(_P_TBLClientVersionData tbl_version in tbl_version_list)
			{
				int curr_ver = DataListManager.Instance.GetVersion( tbl_version.tbl_id );
				if( curr_ver > 0 && curr_ver != tbl_version.version )
				{
					string base64_string = "";
					if( DataListManager.Instance.GetServerData( tbl_version.tbl_id, ref base64_string ) )
					{
						if( string.IsNullOrEmpty( base64_string ) == false )
						{
							_P_TBLUpdateData data = new _P_TBLUpdateData();
							data.tbl_id = tbl_version.tbl_id;
							data.tbl_base64 = base64_string;

							if( update_list == null )
								update_list = new List<_P_TBLUpdateData>();

							update_list.Add( data );
						}
					}
				}
			}

			int size_max = AppConfig.Instance.TBLSendMaxByte;
			List<_P_TBLUpdateData> send_tbl_list = null;
			if( update_list != null )
			{
				foreach( _P_TBLUpdateData data in update_list )
				{
					List<string> tbl_base64_splits = StringUtil.StringSplit( data.tbl_base64, size_max );
					for( int i = 0; i < tbl_base64_splits.Count; i++ )
					{
						_P_TBLUpdateData cs_data = new _P_TBLUpdateData();
						cs_data.is_eof = ( i == tbl_base64_splits.Count - 1 );
						cs_data.tbl_id = data.tbl_id;
						cs_data.tbl_base64 = tbl_base64_splits[i];

						if( send_tbl_list == null )
							send_tbl_list = new List<_P_TBLUpdateData>();

						send_tbl_list.Add( cs_data );
					}
				}
			}

			if( send_tbl_list != null )
			{
				int total_count = send_tbl_list.Count;
				for( int i = 0; i < send_tbl_list.Count; i++ )
				{
					NG2C_TBLDataUpdateNotify _NG2C_TBLDataUpdateNotify = new NG2C_TBLDataUpdateNotify();
					_NG2C_TBLDataUpdateNotify.total_count = (byte)total_count;
					_NG2C_TBLDataUpdateNotify.is_endoflist = ( i == total_count - 1 );
					_NG2C_TBLDataUpdateNotify.tbl_data = send_tbl_list[i];
					SendPacket( _NG2C_TBLDataUpdateNotify );
				}
			}
		}
	}
}
