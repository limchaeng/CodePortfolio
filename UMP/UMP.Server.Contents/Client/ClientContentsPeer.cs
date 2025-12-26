//////////////////////////////////////////////////////////////////////////
//
// ClientContentsPeer
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
using System.Net.Sockets;
using UMP.CSCommon.Packet;
using UMF.Core.I18N;

namespace UMP.Server.Contents
{
	//------------------------------------------------------------------------	
	public class ClientContentsPeer : ContentsClientKeyPeer
	{
		protected ContentsServerApplication mApplication = null;

		public short application_identifier_code = 0;
		public Version client_version = null;
		public int client_revision = 0;
		public int client_runtime_platform_code = 0;
		public string device_language = "";
		public string app_language = "";

		public long account_idx;
		public int contents_flag;

		public string application_identifier = "";
		public System.Net.IPAddress client_ip = null;

		//------------------------------------------------------------------------		
		public override string CurrLanguage
		{
			get { return base.CurrLanguage; }
			set
			{
				mCurrLanguage = LocalizationConfig.Instance.AvailableLanguage( application_identifier, value );
			}
		}

		//------------------------------------------------------------------------		
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

			mApplication = application as ContentsServerApplication;
			strRemoteEndPoint += "+" + GeoIP.Instance.FindGeoIPIsoCode( ( (System.Net.IPEndPoint)Socket.RemoteEndPoint ).Address );
		}

		//------------------------------------------------------------------------	
		public string GetText( string id, params object[] parms )
		{
			return I18NTextMultiLanguage.Instance.GetText( mCurrLanguage, id, parms );
		}

		//------------------------------------------------------------------------
		public override bool Disconnect( int error_code, string error_string, string error_detail_string )
		{
			if( error_code == (int)eDisconnectErrorCode.UnknownSocketDisconnect )
			{
				if( _OnDisconnected != null )
				{
					_OnDisconnected( this, error_code, error_string );
					_OnDisconnected = null;
				}
			}

			return base.Disconnect( error_code, GetText( error_string ), GetText( error_detail_string ) );
		}

		//------------------------------------------------------------------------
		protected override void OnDisconnected()
		{
			base.OnDisconnected();
		}

		//------------------------------------------------------------------------
		protected override object Verify( PacketVerify verify )
		{
			ContentsClientKeyVerify v = verify as ContentsClientKeyVerify;

			application_identifier_code = v.application_identifier;
			application_identifier = AppIdentifier.Instance.Get( application_identifier_code );
			if( string.IsNullOrEmpty( application_identifier ) )
				throw new PacketException( (int)eDisconnectErrorCode.SystemError, $"invalid application code:{application_identifier_code}" );

			Version client_version;
			Version.TryParse( v.version, out client_version );
			client_revision = v.revision;
			client_runtime_platform_code = v.runtime_platform;
			device_language = v.device_language;
			app_language = v.app_language;

			account_idx = v.account_idx;
			contents_flag = v.contents_flag;

			client_ip = ( (System.Net.IPEndPoint)m_Socket.RemoteEndPoint ).Address;

			Log.Write( "VERIFY : A:{0} CV:{1} CR:{2} IP:{3} P:{4} L:{5} DL:{6}",
				application_identifier_code,
				client_version,
				client_revision,
				client_ip,
				client_runtime_platform_code,
				app_language,
				device_language );

			CurrLanguage = app_language;

			// shutdown check
			AppConfig.Instance.ServerShutdownCheck( mUMPApplication, application_identifier, CurrLanguage );

			// version check
			AppConfig.Instance.VersionCheck( mUMPApplication, application_identifier, client_version, client_revision, CurrLanguage );

			bool is_white_client = AppConfig.Instance.MaintenanceWhitelist_Data.ContainWhiteList( client_ip, client_version, application_identifier );
			if( is_white_client == false )
			{
				AppConfig.Instance.MaintenanceCheck( mUMPApplication, application_identifier, CurrLanguage );

				if( ConnectionKeyManager.Instance.CheckConnectionKey( v.connection_key ) == false )
					throw new PacketException( (int)eDisconnectErrorCode.InvalidConnectionKey, v.connection_key.ToString() );
			}

			base.Verify( verify );

			return true;
		}
	}
}
