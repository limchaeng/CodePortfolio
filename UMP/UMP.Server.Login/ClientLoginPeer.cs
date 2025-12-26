//////////////////////////////////////////////////////////////////////////
//
// ClientLoginPeer
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

namespace UMP.Server.Login
{
	//------------------------------------------------------------------------	
	public class ClientLoginPeer : ClientPeer
	{
		public short application_identifier_code;
		public Version client_version;
		public int client_revision;		
		public int client_runtime_platform_code;
		public string device_language;
		public string app_language;

		public string application_identifier;

		public override string CurrLanguage
		{
			get { return base.CurrLanguage; }
			set
			{
				mCurrLanguage = LocalizationConfig.Instance.AvailableLanguage( application_identifier, value );
			}
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

			strRemoteEndPoint += "+" + GeoIP.Instance.FindGeoIPIsoCode( ( (System.Net.IPEndPoint)Socket.RemoteEndPoint ).Address );
		}

		//------------------------------------------------------------------------		
		protected override object Verify( PacketVerify verify )
		{
			ClientPacketVerify v = verify as ClientPacketVerify;

			application_identifier_code = v.application_identifier;
			application_identifier = AppIdentifier.Instance.Get( application_identifier_code );
			if( string.IsNullOrEmpty( application_identifier ) )
				throw new PacketException( (int)eDisconnectErrorCode.SystemError, $"invalid application code:{application_identifier_code}" );

			Version.TryParse( v.version, out client_version );
			client_revision = v.revision;
			client_runtime_platform_code = v.runtime_platform;
			device_language = v.device_language;
			app_language = v.app_language;
			
			System.Net.IPAddress address = ( (System.Net.IPEndPoint)m_Socket.RemoteEndPoint ).Address;

			Log.Write( "VERIFY : A:{0} CV:{1} CR:{2} IP:{3} P:{4} L:{5} DL:{6}", application_identifier_code, client_version, client_revision, address, client_runtime_platform_code, app_language, device_language );

			CurrLanguage = app_language;

			base.Verify( verify );

			// shutdown check
			AppConfig.Instance.ServerShutdownCheck( mUMPApplication, application_identifier, CurrLanguage );
			// version check
			AppConfig.Instance.VersionCheck( mUMPApplication, application_identifier, client_version, client_revision, CurrLanguage );
			
			bool is_white_client = AppConfig.Instance.MaintenanceWhitelist_Data.ContainWhiteList( address, client_version, application_identifier );
			
			// maintenance check
			if( is_white_client == false )
			{
				AppConfig.Instance.MaintenanceCheck( mUMPApplication, application_identifier, CurrLanguage );
			}

			return true;
		}

		//------------------------------------------------------------------------	
		public override bool Disconnect( int error_code, string error_string, string error_detail_string )
		{
			return base.Disconnect( error_code, GetText( error_string ), GetText( error_detail_string ) );
		}

		//------------------------------------------------------------------------	
		public string GetText( string id, params object[] parms )
		{
			return I18NTextMultiLanguage.Instance.GetText( mCurrLanguage, id, parms );
		}
	}
}
