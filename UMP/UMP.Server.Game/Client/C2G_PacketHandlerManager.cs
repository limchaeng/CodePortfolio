//////////////////////////////////////////////////////////////////////////
//
// C2G_PacketHandlerManager
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
using UMP.CSCommon;
using UMP.CSCommon.Packet;
using UMF.Database;
using UMF.Net;
using System.Collections;
using UMF.Core.Module;
using UMF.Core;
using System.Collections.Generic;
using System.Reflection;

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------
	public class C2G_PacketHandlerManagerStandard : C2G_PacketHandlerManager<ClientGamePeer>
	{
		public C2G_PacketHandlerManagerStandard( GameServerApplication application)
			: base( application, typeof( NPID_C2G ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class C2G_PacketHandlerManager<ST> : DBPacketHandlerManager<ST> where ST : ClientGamePeer
	{
		protected GameServerApplication mApplication = null;
		public GameServerApplication Application { get { return mApplication; } }

		//------------------------------------------------------------------------
		public void AddAccountDBHandler<RecvT>( AccountDBHandler<RecvT>.handler_delegate handler ) where RecvT : class
		{
			AddHandler<RecvT>( new AccountDBHandler<RecvT>( handler ).handler );
		}

		//------------------------------------------------------------------------
		public void AddAccountHandler<RecvT>( AccountHandler<RecvT>.handler_delegate handler ) where RecvT : class
		{
			AddHandler<RecvT>( new AccountHandler<RecvT>( handler ).handler );
		}

		//------------------------------------------------------------------------		
		public C2G_PacketHandlerManager( GameServerApplication application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_C2G ) )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------
		public override PacketObjectHandler<ST> CreateAutoHandler( UMPServerApplication application, MethodInfo method, PacketHandlerAttribute handler_attr )
		{
			Type handler_type = handler_attr.GetType();
			if( handler_type.Equals( typeof( AccountDBPacketHandlerAttribute ) ) )
			{
				AccountDBPacketHandlerAttribute account_attr = handler_attr as AccountDBPacketHandlerAttribute;
				Type handler_delegate_type = typeof( AccountDBPacketObjectHandler<ST>.DelegateHandler );
				AccountDBPacketObjectHandler<ST>.DelegateHandler handler_delegate = (AccountDBPacketObjectHandler<ST>.DelegateHandler)Delegate.CreateDelegate( handler_delegate_type, this, method );
				return new PacketObjectHandler<ST>( handler_attr.PacketType, new AccountDBPacketObjectHandler<ST>( handler_delegate ).handler );
			}
			else if( handler_type.Equals( typeof( AccountPacketHandlerAttribute ) ) )
			{
				AccountPacketHandlerAttribute account_attr = handler_attr as AccountPacketHandlerAttribute;
				Type handler_delegate_type = typeof( AccountPacketObjectHandler<ST>.DelegateHandler );
				AccountPacketObjectHandler<ST>.DelegateHandler handler_delegate = (AccountPacketObjectHandler<ST>.DelegateHandler)Delegate.CreateDelegate( handler_delegate_type, this, method );
				return new PacketObjectHandler<ST>( handler_attr.PacketType, new AccountPacketObjectHandler<ST>( handler_delegate ).handler );
			}
			else
			{
				return base.CreateAutoHandler( application, method, handler_attr );
			}
		}

		//------------------------------------------------------------------------		
		[DBPacketHandler( PacketType = typeof( NC2G_AccountLogin ), DBType = eUMPAppDBType.Auth )]
		private IEnumerator NC2G_AccountLoginHandler( ST session, object _packet, DBHandlerObject dbdata )
		{
			NC2G_AccountLogin packet = _packet as NC2G_AccountLogin;

			session.CurrLanguage = packet.localize;

			if( string.IsNullOrEmpty( session.session_data.country_iso_code ) && string.IsNullOrEmpty( packet.localize ) == false )
				session.session_data.country_iso_code = $"L-{packet.localize}";

			DateTime server_time = DateTime.Now; 
			if( packet.fast_login_data != null && mApplication.IsMaintenance == false )
			{
				if( session.PeerIndex != packet.fast_login_data.prev_socket_peerindex )
					mApplication.ClientPeerManager.DisconnectPeer( packet.fast_login_data.prev_socket_peerindex, (int)eDisconnectErrorCode.UnknownSocketDisconnect, "UnknownSocketDisconnect" );

				Account relogin_account = AccountManager.Instance.GetAccount_PendingRelogin( packet.fast_login_data.account_idx );
				if( relogin_account != null && relogin_account.FastLoginVerifyKey == packet.fast_login_data.fast_login_verify_key )
				{
					relogin_account.UpdateSession( session );
					AccountManager.Instance.AddAccount( relogin_account );

					relogin_account.OnFastLogin();

					NG2C_AccountFastLoginAck _NG2C_AccountFastLoginAck = new NG2C_AccountFastLoginAck();
					_NG2C_AccountFastLoginAck.server_time = DateTime.Now;
					_NG2C_AccountFastLoginAck.fast_login_verify_key = relogin_account.FastLoginVerifyKey;
					_NG2C_AccountFastLoginAck.socket_peer_index = session.PeerIndex;
					_NG2C_AccountFastLoginAck.account_login_has_flags = AccountManager.Instance.GetLoginHasFlags();
					session.SendPacket( _NG2C_AccountFastLoginAck );

					relogin_account.OnFinishedLogin( true );

					yield break;
				}
			}

			// step : platform auth verify 
			CS_AuthPlatformData auth_data = packet.auth_data;

			bool is_migration_from_guest = false;
			string platform_extra = "";
			bool is_auto_regist = true;
			bool check_exist_throw = false;

			switch( auth_data.auth_platform_type )
			{
				case (short)eLoginPlatformInternal.Guest:
					{
						is_migration_from_guest = false;
					}
					break;

				case (short)eLoginPlatformInternal.ID_PW:
				case (short)eLoginPlatformInternal.ID_PW_REGIST:
					{
						is_migration_from_guest = false;
						if( auth_data.auth_platform_type == (short)eLoginPlatformInternal.ID_PW_REGIST )
							check_exist_throw = true;
						else
							is_auto_regist = false;

						if( string.IsNullOrEmpty( auth_data.auth_platform_id ) || string.IsNullOrEmpty( auth_data.auth_platform_key ) )
							throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.LoginAuthFailed, session.GetText( SystemTextKey.LoginIncorrectIDPW ) );
					}
					break;

				default:
					{
						string module_name = AuthPlatformIdentifier.Instance.Get( auth_data.auth_platform_type );

						IPlatformAuthModule auth_verify_module = ModuleManager.Instance.FindModuleInterface<IPlatformAuthModule>( module_name );
						if( auth_verify_module == null )
							throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.LoginAuthPlatformError, $"{module_name} auth module not found!" );

						ClassObjectContainer<PlatformAuthResult> auth_result = new ClassObjectContainer<PlatformAuthResult>();
						yield return auth_verify_module.VerifyAuth( auth_data.auth_platform_type, auth_data.auth_platform_id, auth_data.auth_platform_key, auth_data.auth_platform_name, auth_result );
						if( auth_result.obj.error_code > 0 )
							throw new PeerDisconnectException( session.PeerIndex, auth_result.obj.error_code, auth_result.obj.error_message );

						if( string.IsNullOrEmpty( auth_result.obj.platform_name ) == false )
							auth_data.auth_platform_name = auth_result.obj.platform_name;

						is_migration_from_guest = auth_verify_module.IsMigrationFromGuest;
						platform_extra = auth_result.obj.platform_extra;
						is_auto_regist = auth_verify_module.IsAutoRegist;
						check_exist_throw = auth_verify_module.CheckExistThrow;
						
					}
					break;
			}

			// step : auth db verify
			SP_AccountAuth _SP_AccountAuth = new SP_AccountAuth();
			_SP_AccountAuth.platform_type = auth_data.auth_platform_type;
			_SP_AccountAuth.platform_id = auth_data.auth_platform_id;
			_SP_AccountAuth.platform_key = auth_data.auth_platform_key;
			_SP_AccountAuth.platform_username = auth_data.auth_platform_name;
			_SP_AccountAuth.platform_extra = platform_extra;
			_SP_AccountAuth.platform_migration_type = 0;
			_SP_AccountAuth.platform_migration_id = "";
			_SP_AccountAuth.platform_migration_key = "";
			if( is_migration_from_guest )
			{
				_SP_AccountAuth.platform_migration_type = (short)eLoginPlatformInternal.Guest;
				_SP_AccountAuth.platform_migration_id = packet.device_data.device_id;
				_SP_AccountAuth.platform_migration_key = packet.device_data.device_id;
			}
			_SP_AccountAuth.is_auto_regist = is_auto_regist;
			_SP_AccountAuth.check_exist_throw = check_exist_throw;
			_SP_AccountAuth.server_time = server_time;

			yield return DBHandlerExecute.Execute<SP_AccountAuth, SP_AccountAuth_ACK>( _SP_AccountAuth, dbdata );

			if( dbdata.readObject.return_code != 0 )
			{
				switch(dbdata.readObject.return_code)
				{
					case (int)eAccountDBErrorCode.AccountAuthIDExist:
						throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.LoginAuthFailed, session.GetText( AccountStringKey.LOGIN_REGIST_EXIST_ID ) );

					case (int)eAccountDBErrorCode.AccountNotFound:
						throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.LoginAuthFailed, session.GetText( AccountStringKey.LOGIN_REGIST_ACCOUNT_NOT_FOUND ) );

					default:
						throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.LoginAuthFailed, session.GetText( AccountStringKey.LOGIN_REGIST_FAILED, dbdata.readObject.return_code ) );
				}
			}

			SP_AccountAuth_ACK auth_ack_data = (SP_AccountAuth_ACK)dbdata.readObject;

			// withdrawal check
			if( auth_ack_data.withdrawal_process_time != DateTime.MinValue )
			{
				if( auth_ack_data.withdrawal_process_time <= server_time )
				{
					// direct withdrawal process
					SP_AccountWithdrawalDirect _SP_AccountWithdrawalDirect = new SP_AccountWithdrawalDirect();
					_SP_AccountWithdrawalDirect.account_idx = auth_ack_data.account_idx;
					_SP_AccountWithdrawalDirect.server_time = server_time;
					yield return DBHandlerExecute.Execute<SP_AccountWithdrawalDirect, PROCEDURE_READ_DEFAULT>( _SP_AccountWithdrawalDirect, dbdata );

					throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.LoginWithdrawalAccount, session.GetText( SystemTextKey.LoginWithdrawl ) );
				}
			}

			// block check
			if( auth_ack_data.block_expire_time != DateTime.MinValue )
			{
				string cs_url = "";
				throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.LoginAuthBlocked, session.GetText( SystemTextKey.LoginBlocked, auth_ack_data.block_expire_time, cs_url ) );
			}

			// module check
			List<ILoginPreCheckModule> login_precheck_modules = ModuleManager.Instance.FindModuleInterface<ILoginPreCheckModule>();
			if( login_precheck_modules != null )
			{
				foreach( ILoginPreCheckModule module in login_precheck_modules )
				{
					string error = module.PreCheck( packet );
					if( string.IsNullOrEmpty(error) == false )
						throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.LoginCheckFailed, error );
				}
			}

			DatabaseMain game_db = GameDBManager.Instance.GetDatabase( auth_ack_data.game_db_idx );
			if( game_db == null )
				throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.NoDatabase, "Invalid Game Database" );

			string account_code = CodeGenerator.Encrypt( auth_ack_data.account_idx );
			// game login
			SP_AccountGameLogin _SP_Account_GameLogin = new SP_AccountGameLogin();
			_SP_Account_GameLogin.account_idx = auth_ack_data.account_idx;
			_SP_Account_GameLogin.account_code = account_code;
			_SP_Account_GameLogin.game_db_idx = auth_ack_data.game_db_idx;
			_SP_Account_GameLogin.application_identifier = session.session_data.application_identifier;

			_SP_Account_GameLogin.device_type = packet.device_data.device_type;
			_SP_Account_GameLogin.device_id = packet.device_data.device_id;
			_SP_Account_GameLogin.device_location = packet.device_data.device_location;
			_SP_Account_GameLogin.device_os_version = packet.device_data.device_os_version;
			_SP_Account_GameLogin.device_language = packet.device_data.device_language;
			_SP_Account_GameLogin.device_advertising_id = packet.device_data.device_advertising_id;
			_SP_Account_GameLogin.device_package_id = packet.device_data.device_package_id;

			_SP_Account_GameLogin.client_version = session.session_data.client_version.ToString();
			_SP_Account_GameLogin.server_time = server_time;
			_SP_Account_GameLogin.use_multiple_player = ( AccountConfig.Instance.UseMultiplePlayer > 1 );

			DBHandlerObject gamedb_data = DBHandlerExecute.CreateData( game_db );
			yield return DBHandlerExecute.Execute<SP_AccountGameLogin, SP_AccountGameLogin_ACK>( _SP_Account_GameLogin, gamedb_data );
			if( gamedb_data.readObject.return_code != 0 )
				throw new PeerDBErrorException( session.PeerIndex, gamedb_data.readObject.return_code );

			SP_AccountGameLogin_ACK gamelogin_ack_data = gamedb_data.readObject as SP_AccountGameLogin_ACK;

			AccountManager.Instance.CheckDuplicateLogin( auth_ack_data.account_idx );

			// check security user
			if( mApplication.IsMaintenance )
			{
				AppConfig app_config = AppConfig.Instance;

				bool is_white_client = app_config.MaintenanceWhitelist_Data.ContainWhiteList( session.session_data.client_ip, session.session_data.client_version, session.session_data.application_identifier );
				if( is_white_client == false || (
						app_config.MaintenanceWhitelist_Data.IgnoreSecurityLevel == false && 
						CSSecurity.IsAllow( (CSSecurity.eSecurityFlags)gamelogin_ack_data.security_flags, CSSecurity.eSecurityFlags.MaintenanceLogin) == false )
					)
				{
					app_config.MaintenanceCheck( mApplication, session.session_data.application_identifier, session.CurrLanguage );
				}
			}

			SP_Common_AccountLogin_ACK common_login_ack_data = null;
			DatabaseMain common_db = mApplication.DBCommon;
			if( common_db != null )
			{
				SP_Common_AccountLogin _SP_Common_AccountLogin = new SP_Common_AccountLogin();
				_SP_Common_AccountLogin.account_idx = auth_ack_data.account_idx;
				_SP_Common_AccountLogin.game_db_idx = auth_ack_data.game_db_idx;

				DBHandlerObject commondb_data = DBHandlerExecute.CreateData( common_db );
				yield return DBHandlerExecute.Execute<SP_Common_AccountLogin, SP_Common_AccountLogin_ACK>( _SP_Common_AccountLogin, commondb_data );
				if( commondb_data.readObject.return_code != 0 )
					throw new PeerDBErrorException( session.PeerIndex, commondb_data.readObject.return_code );

				common_login_ack_data = commondb_data.readObject as SP_Common_AccountLogin_ACK;
			}

			Account account = AccountManager.Instance.NewAccount( account_code, session, packet, auth_ack_data, gamelogin_ack_data, common_login_ack_data );

			// pre notfy data check
			account.LocalizeFastUpdateCheck();
			account.TBLVersionCheck( packet.tbl_version_list );

			// module login 
			yield return account.OnAccountLogin();

			NG2C_AccountLoginAck _NG2C_AccountLoginAck = new NG2C_AccountLoginAck();
			_NG2C_AccountLoginAck.account_idx = account.AccountIDX;
			_NG2C_AccountLoginAck.account_code = account.AccountCode;
			_NG2C_AccountLoginAck.nickname = account.Nickname;
			_NG2C_AccountLoginAck.account_created_time = account.CreatedTime;
			_NG2C_AccountLoginAck.account_withdrawal_process_time = account.WithdrawalProcessTime;
			_NG2C_AccountLoginAck.platform_regist_list = account.GetAuthPlatformRegistList();

			_NG2C_AccountLoginAck.fast_login_verify_key = account.FastLoginVerifyKey;
			_NG2C_AccountLoginAck.socket_peer_index = session.PeerIndex;
			_NG2C_AccountLoginAck.gameserver_guid = mApplication.GUID_LONG;
			_NG2C_AccountLoginAck.server_version = AppConfig.Instance.VersionInfo_Data.ServerVersion;
			_NG2C_AccountLoginAck.server_time = DateTime.Now;
			_NG2C_AccountLoginAck.account_security_flags = account.SecurityFlags;
			_NG2C_AccountLoginAck.account_login_has_flags = AccountManager.Instance.GetLoginHasFlags();

			// config
			_NG2C_AccountLoginAck.app_config = AppConfig.Instance.ToCS( account.AgreementFlags, account.CurrLanguage );
			_NG2C_AccountLoginAck.localization_config = LocalizationConfig.Instance.ToCS( account.ClientSessionData.application_identifier );

			session.SendPacket( _NG2C_AccountLoginAck );

			// player select
			if( AccountConfig.Instance.UseMultiplePlayer > 1 )
			{
				Log.WriteWarning( "?? NOT IMPLEMENT" );
				// TODO : multiple player(aka character?);
				// - Create
				// - Delete
				// - Select
				yield return _SelectPlayer( account, 1 );
			}
			else
			{
				yield return _SelectPlayer( account, 1 );
			}
		}

		//------------------------------------------------------------------------		
		IEnumerator _SelectPlayer( Account account, int player_idx)
		{
			yield return account.OnPlayerLogin( player_idx );

			// finished
			account.OnFinishedLogin( false );
		}
	}
}
