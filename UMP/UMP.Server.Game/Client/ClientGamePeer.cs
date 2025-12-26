//////////////////////////////////////////////////////////////////////////
//
// ClientGamePeer
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
using UMF.Net;
using UMF.Core;
using UMP.CSCommon;
using UMP.CSCommon.Packet;
using System.Net.Sockets;
using UMF.Core.I18N;

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------	
	public class ClientGamePeer : ClientKeyPeer
	{
		protected GameServerApplication mApplication = null;

		public class SessionData
		{
			public short application_identifier_code = 0;
			public Version client_version = null;
			public int client_revision = 0;
			public int client_runtime_platform_code = 0;
			public string device_language = "";
			public string app_language = "";

			public string application_identifier = "";
			public System.Net.IPAddress client_ip = null;
			public string country_iso_code = "";
			public int peer_index = 0;
		}

		public SessionData session_data = new SessionData();
		public long account_idx = 0;		

		public override string CurrLanguage
		{
			set
			{
				mCurrLanguage = LocalizationConfig.Instance.AvailableLanguage( session_data.application_identifier, value );
			}

			get { return base.CurrLanguage; }
		}

		protected override bool IsUseErrorDetail
		{
			get
			{
				return ( mUMPApplication.ServiceType != eServiceType.Live );
			}
		}

		//------------------------------------------------------------------------
		public override void Init( UMPServerApplication application, PeerManagerBase peerManager, Socket socket )
		{
			base.Init( application, peerManager, socket );

			mApplication = application as GameServerApplication;

			session_data.country_iso_code = GeoIP.Instance.FindGeoIPIsoCode( ( (System.Net.IPEndPoint)Socket.RemoteEndPoint ).Address );
			strRemoteEndPoint += "+" + session_data.country_iso_code;
		}

		//------------------------------------------------------------------------		
		protected override object Verify( PacketVerify verify )
		{
			ClientKeyPacketVerify v = verify as ClientKeyPacketVerify;

			session_data.application_identifier_code = v.application_identifier;
			session_data.application_identifier = AppIdentifier.Instance.Get( session_data.application_identifier_code );
			if( string.IsNullOrEmpty( session_data.application_identifier ) )
				throw new PacketException( (int)eDisconnectErrorCode.SystemError, $"invalid application code:{session_data.application_identifier_code}" );

			Version client_version;
			if( Version.TryParse( v.version, out client_version ) )
				session_data.client_version = client_version;
			session_data.client_revision = v.revision;
			session_data.client_runtime_platform_code = v.runtime_platform;
			session_data.device_language = v.device_language;
			session_data.app_language = v.app_language;
			
			session_data.client_ip = ( (System.Net.IPEndPoint)m_Socket.RemoteEndPoint ).Address;
			session_data.peer_index = PeerIndex;

			Log.Write( "VERIFY : A:{0} CV:{1} CR:{2} IP:{3} P:{4} L:{5} DL:{6}",
				session_data.application_identifier_code,
				session_data.client_version,
				session_data.client_revision,
				session_data.client_ip,
				session_data.client_runtime_platform_code,
				session_data.app_language,
				session_data.device_language );

			CurrLanguage = session_data.app_language;

			// shutdown check
			AppConfig.Instance.ServerShutdownCheck( mUMPApplication, session_data.application_identifier, CurrLanguage );

			// version check
			AppConfig.Instance.VersionCheck( mUMPApplication, session_data.application_identifier, session_data.client_version, session_data.client_revision, CurrLanguage );

			bool is_white_client = AppConfig.Instance.MaintenanceWhitelist_Data.ContainWhiteList( session_data.client_ip, session_data.client_version, session_data.application_identifier );
			if( is_white_client == false )
			{
				AppConfig.Instance.MaintenanceCheck( mUMPApplication, session_data.application_identifier, CurrLanguage );

				if( ConnectionKeyManager.Instance.CheckConnectionKey( v.connection_key ) == false )
					throw new PacketException( (int)eDisconnectErrorCode.InvalidConnectionKey, v.connection_key.ToString() );

				if( mApplication.ClientPeerManager.PeerCount >= mApplication.ClientLimitCount )
					throw new PacketException( (int)eDisconnectErrorCode.ClientLimit, GetText( SystemTextKey.CLIENT_LIMIT ) );
			}

			base.Verify( verify );

			return true;
		}

		//------------------------------------------------------------------------	
		public override bool Disconnect( int error_code, string error_string, string error_detail_string )
		{
			if( error_code == (int)eDisconnectErrorCode.UnknownSocketDisconnect )
			{
				if( PeerIndex != -1 )
				{
					AccountManager.Instance.OnDisconnectedAccount( PeerIndex, error_code, error_string, error_detail_string );
					PeerIndex = -1;
				}
			}

			return base.Disconnect( error_code, GetText( error_string ), GetText( error_detail_string ) );
		}

		//------------------------------------------------------------------------
		protected override void OnDisconnected()
		{
			string log_msg = $"[{SessionName}] OnDisconnected({strRemoteEndPoint},{PeerIndex}) version:{session_data.client_version} app_id:{session_data.application_identifier} account:{account_idx}";
			if( m_Disconnect.error_code != (int)eDisconnectErrorCode.ClientQuit && m_Disconnect.error_code != (int)eDisconnectErrorCode.Normal )
				Log.WriteWarning( log_msg );
			else
				Log.Write( log_msg );

			base.OnDisconnected();

			if( PeerIndex != -1 )
				AccountManager.Instance.OnDisconnectedAccount( PeerIndex, m_Disconnect.error_code, m_Disconnect.error_string, m_Disconnect.error_detail_string );
		}

		//------------------------------------------------------------------------	
		public string GetText( string id, params object[] parms )
		{
			return I18NTextMultiLanguage.Instance.GetText( mCurrLanguage, id, parms );
		}
	}
}
