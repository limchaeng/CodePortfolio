//////////////////////////////////////////////////////////////////////////
//
// AccountManager
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
using System.Linq;
using UMF.Core;
using System.Collections.Generic;
using UMP.CSCommon.Packet;
using UMF.Net;
using System.Collections;
using UMF.Database;

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------
	public interface IAccountModuleCacheBase
	{
		string ModuleName { get; }
		int Priority { get; }   // low priority first
		void OnFastLogin( Account account );
		IEnumerator OnAccountLogin( Account account );
		void OnFinishedLogin( bool is_fast_login );
		IEnumerator OnPlayerLogin( int player_idx );
		void OnLogout();
	}

	//------------------------------------------------------------------------
	public class AccountModuleCacheFactoryManager : InterfaceFactory<string, IAccountModuleCacheBase, AccountModuleCacheFactoryManager> 
	{
		//------------------------------------------------------------------------
		public void CreateModuleCache(Account account)
		{
			List<string> keys = GetKeyList();
			foreach (string key in keys)
			{
				account.AddModuleCache( Create( key ) );
			}
		}
	}

	//------------------------------------------------------------------------	
	public class AccountManager : Singleton<AccountManager>
	{
		Dictionary<int, Account> mAccountByPeerIdx = new Dictionary<int, Account>();
		Dictionary<long, Account> mAccountByAccountIdx = new Dictionary<long, Account>();
		List<Account> mAccountList = new List<Account>();
		//
		Dictionary<long, Account> mAccountByPendingRelogin = new Dictionary<long, Account>();

		GameServerApplication mApplication = null;

		//------------------------------------------------------------------------
		public void Init(GameServerApplication application)
		{
			mApplication = application;
			mApplication.AddUpdater( CheckPendingReloginAccount );
			DBErrorShared.AddDBError( typeof( eAccountDBErrorCode ) );
		}

		//------------------------------------------------------------------------		
		public Account GetAccount_PeerIndex( int peer_index )
		{
			Account data;
			if( mAccountByPeerIdx.TryGetValue( peer_index, out data ) == false )
				return null;

			return data;
		}

		//------------------------------------------------------------------------		
		public Account GetAccount_AccountIdx( long account_idx )
		{
			Account data;
			if( mAccountByAccountIdx.TryGetValue( account_idx, out data ) == false )
				return null;

			return data;
		}

		//------------------------------------------------------------------------		
		public Account GetAccount_PendingRelogin( long account_idx )
		{
			Account data;
			if( mAccountByPendingRelogin.TryGetValue( account_idx, out data ) )
				return data;

			return null;
		}

		//------------------------------------------------------------------------		
		void RemoveAccount_PeerIndex( int peer_index )
		{
			Account account = GetAccount_PeerIndex( peer_index );
			RemoveAccount( account );
		}

		//------------------------------------------------------------------------		
		void RemoveAccount( Account account )
		{
			if( account == null )
				return;

			mAccountByPeerIdx.Remove( account.PeerIndex );
			mAccountByAccountIdx.Remove( account.AccountIDX );
			mAccountList.Remove( account );
		}

		//------------------------------------------------------------------------		
		public Account NewAccount( string account_code, ClientGamePeer session, NC2G_AccountLogin c2g_packet, SP_AccountAuth_ACK auth_db_data, SP_AccountGameLogin_ACK game_db_data, SP_Common_AccountLogin_ACK common_db_data )
		{
			Account account = new Account( mApplication, account_code, session, c2g_packet, auth_db_data, game_db_data, common_db_data );
			AccountModuleCacheFactoryManager.Instance.CreateModuleCache( account );

			// exception 
			if( mAccountByAccountIdx.ContainsKey( account.AccountIDX ) )
			{
				Log.WriteWarning( "AccountByAccountIdx:Same AccountIdx:{0} deteceted!", account.AccountIDX );
				mAccountByAccountIdx.Remove( account.AccountIDX );
			}
			if( mAccountByPeerIdx.ContainsKey( account.PeerIndex ) )
			{
				Log.WriteWarning( "AccountByUserIndex:Same UserIndex({0}/{1}) deteceted!", account.AccountIDX, account.PeerIndex );
				mAccountByPeerIdx.Remove( account.PeerIndex );
			}

			AddAccount( account );

			// LOG
// 			{
// 				if( is_new )
// 				{
// 					spl_signup _spl_signup = new spl_signup();
// 					_spl_signup.log_time = DateTime.Now;
// 					_spl_signup.account_idx = account.account_idx;
// 					_spl_signup.application_identifier = account.application_identifier.ToString();
// 					_spl_signup.channel_user_id = account.platform_id;
// 					_spl_signup.device_key = account.device_key;
// 					_spl_signup.client_ip = account.SessionRemoteEndPoint;
// 					_spl_signup.login_platform = (int)account.login_platform;
// 					_spl_signup.location = account.SessionCountryCode;
// 					GameServerMain.ExecuteLog( _spl_signup );
// 				}
// 			}

			return account;
		}

		//------------------------------------------------------------------------	
		public void AddAccount( Account account )
		{
			mAccountByAccountIdx.Add( account.AccountIDX, account );
			mAccountByPeerIdx.Add( account.PeerIndex, account );
			mAccountList.Add( account );

			RemovePendingRelogin( account.AccountIDX );

			account.FastLoginVerifyKey = (byte)UMFRandom.Instance.NextRange( 1, 200 );
		}

		//------------------------------------------------------------------------
		public void AddPendingRelogin( Account disconnectedAccount )
		{
			if( mAccountByPendingRelogin.ContainsKey( disconnectedAccount.AccountIDX ) == false )
				mAccountByPendingRelogin.Add( disconnectedAccount.AccountIDX, disconnectedAccount );
			else
				mAccountByPendingRelogin[disconnectedAccount.AccountIDX] = disconnectedAccount;

			if( disconnectedAccount.PendingReLoginTimeout < tmpPendingReloginNextCheckTime )
				tmpPendingReloginNextCheckTime = disconnectedAccount.PendingReLoginTimeout;
		}

		//------------------------------------------------------------------------
		public void RemovePendingRelogin(long account_idx)
		{
			if( mAccountByPendingRelogin.ContainsKey( account_idx ) )
			{
				mAccountByPendingRelogin.Remove( account_idx );

				if( mAccountByPendingRelogin.Count > 0 )
				{
					tmpPendingReloginNextCheckTime = mAccountByPendingRelogin.Min( a => a.Value.PendingReLoginTimeout );
				}
				else
				{
					tmpPendingReloginNextCheckTime = DateTime.MaxValue;
				}
			}
		}

		//------------------------------------------------------------------------	
		DateTime tmpPendingReloginNextCheckTime = DateTime.MaxValue;
		List<long> tmpRemovePendingAccountList = new List<long>();
		void CheckPendingReloginAccount()
		{
			if( mAccountByPendingRelogin.Count > 0 )
			{
				if( tmpPendingReloginNextCheckTime <= DateTime.Now )
				{
					tmpPendingReloginNextCheckTime = DateTime.MaxValue;
					tmpRemovePendingAccountList.Clear();
					foreach( Account acc in mAccountByPendingRelogin.Values )
					{
						if( acc.PendingReLoginTimeout <= DateTime.Now )
						{
							tmpRemovePendingAccountList.Add( acc.AccountIDX );
						}
						else if( tmpPendingReloginNextCheckTime > acc.PendingReLoginTimeout )
						{
							tmpPendingReloginNextCheckTime = acc.PendingReLoginTimeout;
						}
					}

					if( tmpRemovePendingAccountList.Count > 0 )
					{
						foreach( long key in tmpRemovePendingAccountList )
						{
							mAccountByPendingRelogin.Remove( key );
						}
					}
					tmpRemovePendingAccountList.Clear();
				}
			}
		}

		//------------------------------------------------------------------------		
		public void CheckDuplicateLogin( long account_idx )
		{
			Account account = GetAccount_AccountIdx( account_idx );
			if( account != null )
			{
				RemoveAccount( account );
				mApplication.ClientPeerManager.DisconnectPeer(account.PeerIndex, (int)eDisconnectErrorCode.LoginDuplicated, account.GetText( AccountStringKey.LOGIN_DUPLICATED ) );
			}
		}

		//------------------------------------------------------------------------	
		public void AccountLoginNotify( Account account, int game_server_idx )
		{
			if( mApplication.RelayConnector != null )
			{
				NG2R_AccountLoginNotify _NG2R_AccountLoginNotify = new NG2R_AccountLoginNotify();
				_NG2R_AccountLoginNotify.account_idx = account.AccountIDX;
				_NG2R_AccountLoginNotify.nickname = account.Nickname;
				_NG2R_AccountLoginNotify.game_server_idx = game_server_idx;
				_NG2R_AccountLoginNotify.peer_index = account.ClientSession.PeerIndex;				
				_NG2R_AccountLoginNotify.gamedb_idx = account.GameDBIdx;
				mApplication.RelayConnector.SendPacket( _NG2R_AccountLoginNotify );
			}
		}

		//------------------------------------------------------------------------
		public void PlayerLoginNotify(Account account)
		{
			if( mApplication.RelayConnector != null && account.CurrentPlayer != null )
			{
				NG2R_PlayerLoginNotify _NG2R_PlayerLoginNotify = new NG2R_PlayerLoginNotify();
				_NG2R_PlayerLoginNotify.account_idx = account.AccountIDX;
				_NG2R_PlayerLoginNotify.player_idx = account.CurrentPlayer.player_idx;
				_NG2R_PlayerLoginNotify.nickname = account.CurrentPlayer.nickname;

				mApplication.RelayConnector.SendPacket( _NG2R_PlayerLoginNotify );
			}
		}

		//------------------------------------------------------------------------		
		public void DisconnectAccount( long account_idx, int error_code, string error_string )
		{
			Account acc = GetAccount_AccountIdx( account_idx );
			if( acc != null )
			{
				mApplication.ClientPeerManager.DisconnectPeer( acc.ClientSessionData.peer_index, error_code, error_string );
			}
		}

		//------------------------------------------------------------------------		
		public void OnDisconnectedAccount( int peer_index, int error_code, string error_string, string error_detail )
		{
			Account disconnectedAccount;
			if( mAccountByPeerIdx.TryGetValue( peer_index, out disconnectedAccount ) == false )
				return;

			bool is_pending = disconnectedAccount.OnDisconnected( error_code, error_string, error_detail );

			RemoveAccount( disconnectedAccount );

			if( is_pending )
				AddPendingRelogin( disconnectedAccount );
		}

		//------------------------------------------------------------------------
		public eAccountLoginHasFlags GetLoginHasFlags()
		{
			eAccountLoginHasFlags flags = eAccountLoginHasFlags.None;

			// TODO :

			return flags;
		}
	}
}
