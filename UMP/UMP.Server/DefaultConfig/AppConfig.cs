//////////////////////////////////////////////////////////////////////////
//
// AppConfig 
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
using System.Collections.Generic;
using System.Xml;
using UMF.Core;
using UMF.Core.I18N;
using UMF.Net;
using UMP.CSCommon;

namespace UMP.Server
{
	//------------------------------------------------------------------------
	public class AppConfig : ReloadConfig<AppConfig>
	{
		public const string CONFIG_ID = "AppConfig";

		//------------------------------------------------------------------------
		public class Shutdown
		{
			//------------------------------------------------------------------------
			public class PerApp
			{
				public string AppIdentifier { get; private set; }
				public string URL { get; private set; }
				public string TextKey { get; private set; }

				public PerApp( XmlNode node )
				{
					AppIdentifier = XMLUtil.ParseAttribute<string>( node, "AppIdentifier", "" );
					URL = XMLUtil.ParseAttribute<string>( node, "URL", "" );
					TextKey = XMLUtil.ParseAttribute<string>( node, "TextKey", "" );

				}
			}

			public DateTime BeginTime { get; private set; }
			public DateTime EndTime { get; private set; }
			public string URL { get; private set; }
			public string TextKey { get; private set; }

			public List<PerApp> PerApp_List { get; private set; }

			public Shutdown( XmlNode node )
			{
				BeginTime = XMLUtil.ParseAttribute<DateTime>( node, "BeginTime", DateTime.MinValue );
				EndTime = XMLUtil.ParseAttribute<DateTime>( node, "EndTime", DateTime.MaxValue );
				URL = XMLUtil.ParseAttribute<string>( node, "URL", "" );
				TextKey = XMLUtil.ParseAttribute<string>( node, "TextKey", "" );

				PerApp_List = null;
				foreach( XmlNode child in node.SelectNodes( "PerApp" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					if( PerApp_List == null )
						PerApp_List = new List<PerApp>();

					PerApp_List.Add( new PerApp( child ) );
				}

			}

			//------------------------------------------------------------------------
			public PerApp Find_PerApp(string app_id)
			{
				if( PerApp_List != null )
					return PerApp_List.Find( a => a.AppIdentifier == app_id );

				return null;
			}

			//------------------------------------------------------------------------
			public void UpdateTime( DateTime begin_time, DateTime end_time )
			{
				this.BeginTime = begin_time;
				this.EndTime = end_time;
			}

			//------------------------------------------------------------------------	
			public string ToTimeText(string language)
			{
				if( BeginTime != DateTime.MinValue && EndTime != DateTime.MaxValue )
				{
					if( BeginTime.Month != EndTime.Month || BeginTime.Day != EndTime.Day )
						return I18NTextMultiLanguage.Instance.GetTextWithCulture( language, SystemTextKey.TIME_FORMAT2, BeginTime, EndTime );
					else
						return I18NTextMultiLanguage.Instance.GetTextWithCulture( language, SystemTextKey.TIME_FORMAT, BeginTime, EndTime );
				}

				if( BeginTime == DateTime.MinValue )
					return I18NTextMultiLanguage.Instance.GetTextWithCulture( language, SystemTextKey.TIME_FORMAT2, "", EndTime );

				if( EndTime == DateTime.MaxValue )
					return I18NTextMultiLanguage.Instance.GetTextWithCulture( language, SystemTextKey.TIME_FORMAT2, BeginTime, "");

				return "";
			}

		}

		//------------------------------------------------------------------------
		public class VersionInfo
		{
			//------------------------------------------------------------------------
			public class PerApp
			{
				//------------------------------------------------------------------------
				public class Text
				{
					public string ClientUpdateText { get; private set; }
					public string MaintenanceText { get; private set; }
					public string MaxVersionText { get; private set; }
					public string ExpireVersionText { get; private set; }

					public Text( XmlNode node )
					{
						ClientUpdateText = XMLUtil.ParseAttribute<string>( node, "ClientUpdateText", "" );
						MaintenanceText = XMLUtil.ParseAttribute<string>( node, "MaintenanceText", "" );
						MaxVersionText = XMLUtil.ParseAttribute<string>( node, "MaxVersionText", "" );
						ExpireVersionText = XMLUtil.ParseAttribute<string>( node, "ExpireVersionText", "" );

					}
				}

				//------------------------------------------------------------------------
				public class Review
				{
					public Version ReviewVersion { get; private set; }
					public string LoginHost { get; private set; }
					public int LoginPort { get; private set; }

					public Review( XmlNode node )
					{
						string review_version = XMLUtil.ParseAttribute<string>( node, "ReviewVersion", "" );

						if( EnvironmentProperty.IsEnvironmentPropertyKey( review_version ) )
							review_version = GlobalEnv.EnvProp.GetEnvironmentProperty( review_version );

						ReviewVersion = StringUtil.SafeParse<Version>( review_version, null );

						LoginHost = XMLUtil.ParseAttribute<string>( node, "LoginHost", "" );
						LoginPort = XMLUtil.ParseAttribute<int>( node, "LoginPort", 0 );

					}
				}

				public string AppIdentifier { get; private set; }
				public bool IsCheck { get; private set; }
				public Version ClientVersion { get; private set; }
				public int ClientRevision { get; private set; }
				public Version MaxVersion { get; private set; }
				public Version MinVersion { get; private set; }
				public string ClientUpdateLink { get; private set; }
				public DateTime ExpireTime { get; private set; }

				public Text Text_Data { get; private set; }
				public Review Review_Data { get; private set; }

				public PerApp( XmlNode node, VersionInfo parent )
				{
					AppIdentifier = XMLUtil.ParseAttribute<string>( node, "AppIdentifier", "" );
					IsCheck = XMLUtil.ParseAttribute<bool>( node, "IsCheck", true );

					string client_version = XMLUtil.ParseAttribute<string>( node, "ClientVersion", "" );
					if( EnvironmentProperty.IsEnvironmentPropertyKey( client_version ) )
						client_version = GlobalEnv.EnvProp.GetEnvironmentProperty( client_version );

					ClientVersion = StringUtil.SafeParse<Version>( client_version, new Version( 0, 0, 0, 1 ) );
					ClientRevision = XMLUtil.ParseAttribute<int>( node, "ClientRevision", 0 );
					MaxVersion = XMLUtil.ParseAttribute<Version>( node, "MaxVersion", null );
					MinVersion = XMLUtil.ParseAttribute<Version>( node, "MinVersion", null );
					ClientUpdateLink = XMLUtil.ParseAttribute<string>( node, "ClientUpdateLink", "" );
					ExpireTime = XMLUtil.ParseAttribute<DateTime>( node, "ExpireTime", DateTime.MaxValue );

					Text_Data = null;
					XmlNode _text_node = node.SelectSingleNode( "Text" );
					if( _text_node != null )
					{
						Text_Data = new Text( _text_node );
					}
					if( Text_Data == null )
						Text_Data = parent.DefaultText_Data;

					Review_Data = null;
					XmlNode _review_node = node.SelectSingleNode( "Review" );
					if( _review_node != null )
					{
						Review_Data = new Review( _review_node );
					}
				}
			}

			public Version ServerVersion { get; private set; }
			public PerApp.Text DefaultText_Data { get; private set; }

			public List<PerApp> PerApp_List { get; private set; }

			public VersionInfo( XmlNode node )
			{
				string server_version = XMLUtil.ParseAttribute<string>( node, "ServerVersion", "" );
				if( EnvironmentProperty.IsEnvironmentPropertyKey( server_version ) )
					server_version = GlobalEnv.EnvProp.GetEnvironmentProperty( server_version );

				ServerVersion = StringUtil.SafeParse<Version>( server_version, new Version( 0, 0, 0, 1 ) );

				DefaultText_Data = new PerApp.Text( node );

				PerApp_List = new List<PerApp>();
				foreach( XmlNode child in node.SelectNodes( "PerApp" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					PerApp_List.Add( new PerApp( child, this ) );
				}

			}

			//------------------------------------------------------------------------
			public PerApp Find_PerApp( string appidentifier )
			{
				return PerApp_List.Find( a => a.AppIdentifier == appidentifier );
			}
		}

		//------------------------------------------------------------------------
		public class MaintenanceWhitelist
		{
			//------------------------------------------------------------------------
			public class IPADDR
			{
				public string IP { get; private set; }

				string[] _IP_SPLIT = null;

				public IPADDR( XmlNode node )
				{
					IP = XMLUtil.ParseAttribute<string>( node, "IP", "" );

					_IP_SPLIT = IP.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
				}

				//------------------------------------------------------------------------	
				public bool AllowIP( string[] ip_split )
				{
					if( _IP_SPLIT == null || ip_split == null || _IP_SPLIT.Length != ip_split.Length )
						return false;

					for( int i = 0; i < _IP_SPLIT.Length; i++ )
					{
						if( _IP_SPLIT[i] == "*" )
							continue;

						if( _IP_SPLIT[i] != ip_split[i] )
							return false;
					}

					return true;
				}

			}

			//------------------------------------------------------------------------
			public class VERSION
			{
				public Version VER { get; private set; }

				public VERSION( XmlNode node )
				{
					VER = XMLUtil.ParseAttribute<Version>( node, "VER", null );

				}
			}

			//------------------------------------------------------------------------
			public class APPID
			{
				public string AppIdentifier { get; private set; }

				public APPID( XmlNode node )
				{
					AppIdentifier = XMLUtil.ParseAttribute<string>( node, "AppIdentifier", "" );

				}
			}

			public bool IgnoreSecurityLevel { get; private set; }

			public List<IPADDR> IPADDR_List { get; private set; }
			public List<VERSION> VERSION_List { get; private set; }
			public List<APPID> APPID_List { get; private set; }

			public MaintenanceWhitelist( XmlNode node )
			{
				IgnoreSecurityLevel = XMLUtil.ParseAttribute<bool>( node, "IgnoreSecurityLevel", false );

				IPADDR_List = null;
				foreach( XmlNode child in node.SelectNodes( "IPADDR" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					if( IPADDR_List == null )
						IPADDR_List = new List<IPADDR>();

					IPADDR_List.Add( new IPADDR( child ) );
				}

				VERSION_List = null;
				foreach( XmlNode child in node.SelectNodes( "VERSION" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					if( VERSION_List == null )
						VERSION_List = new List<VERSION>();

					VERSION_List.Add( new VERSION( child ) );
				}

				APPID_List = null;
				foreach( XmlNode child in node.SelectNodes( "APPID" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					if( APPID_List == null )
						APPID_List = new List<APPID>();

					APPID_List.Add( new APPID( child ) );
				}

			}

			//------------------------------------------------------------------------
			public IPADDR Find_IPADDR( string ip )
			{
				if( IPADDR_List != null )
					return IPADDR_List.Find( a => a.IP == ip );

				return null;
			}

			//------------------------------------------------------------------------
			public VERSION Find_VERSION( Version ver )
			{
				if( VERSION_List != null )
					return VERSION_List.Find( a => a.VER == ver );

				return null;
			}

			//------------------------------------------------------------------------
			public APPID Find_APPID( string appidentifier )
			{
				if( APPID_List != null )
					return APPID_List.Find( a => a.AppIdentifier == appidentifier );

				return null;
			}

			//------------------------------------------------------------------------
			public bool ContainWhiteList( System.Net.IPAddress address, Version client_version, string app_id )
			{
				if( IPADDR_List != null )
				{
					if( IPADDR_List.Exists( i => i.IP.ToLower() == address.ToString().ToLower() ) )
						return true;

					string[] ip_split = address.ToString().Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
					if( IPADDR_List.Exists( i => i.AllowIP( ip_split ) ) )
						return true;
				}

				if( VERSION_List != null && VERSION_List.Exists( v => v.VER == client_version ) )
					return true;

				if( APPID_List != null && APPID_List.Exists( a => a.AppIdentifier == app_id ) )
					return true;

				return false;
			}
		}

		//------------------------------------------------------------------------
		public class URLConfig
		{
			//------------------------------------------------------------------------
			public class Data
			{
				//------------------------------------------------------------------------
				public class LanguageData
				{
					public string Name { get; private set; }
					public string Url { get; private set; }

					public LanguageData( XmlNode node )
					{
						Name = XMLUtil.ParseAttribute<string>( node, "Name", "" );
						Url = XMLUtil.ParseAttribute<string>( node, "Url", "" );
					}
				}

				public CS_URLConfig.eURLType Type { get; private set; }
				public string Url { get; private set; }

				public List<LanguageData> LanguageList { get; private set; }

				public Data( XmlNode node )
				{
					Type = XMLUtil.ParseAttribute<CS_URLConfig.eURLType>( node, "Type", CS_URLConfig.eURLType.None );
					Url = XMLUtil.ParseAttribute<string>( node, "Url", "" );

					LanguageList = null;
					foreach (XmlNode child in node.SelectNodes("Language"))
					{
						if( child.NodeType == XmlNodeType.Comment )
							continue;

						if( LanguageList == null )
							LanguageList = new List<LanguageData>();

						LanguageList.Add( new LanguageData( child ) );
					}
				}

				public string FindLanguageUrl(string language)
				{
					if( LanguageList != null )
					{
						LanguageData lan_data = LanguageList.Find( a => a.Name == language );
						if( lan_data != null )
							return lan_data.Url;
					}

					return Url;
				}
			}

			public List<Data> Data_List { get; private set; }

			public URLConfig(XmlNode node)
			{
				Data_List = new List<Data>();
				foreach( XmlNode child in node.SelectNodes( "Data" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					Data_List.Add( new Data( child ) );
				}
			}

			//------------------------------------------------------------------------
			public Data FindData(CS_URLConfig.eURLType url_type)
			{
				return Data_List.Find( a => a.Type == url_type );
			}

			//------------------------------------------------------------------------			
			public List<CS_URLConfig> ToCS(string language)
			{
				if( Data_List.Count <= 0 )
					return null;

				List<CS_URLConfig> list = null;

				foreach(Data data in Data_List)
				{
					if( string.IsNullOrEmpty( data.Url ) )
						continue;

					CS_URLConfig cs_data = new CS_URLConfig();
					cs_data.url_type = data.Type;
					cs_data.url = data.Url;

					if( data.LanguageList != null )
					{
						Data.LanguageData lan = data.LanguageList.Find( a => a.Name == language );
						if( lan != null && string.IsNullOrEmpty( lan.Url ) == false )
							cs_data.url = lan.Url;
					}

					if( list == null )
						list = new List<CS_URLConfig>();

					list.Add( cs_data );						
				}

				return list;
			}
		}

		//------------------------------------------------------------------------
		public class AgreementConfirm
		{
			//------------------------------------------------------------------------
			public class Data
			{
				public CS_AgreementData.eTypeFlag Type { get; private set; }
				public string Text { get; private set; }
				public string Link { get; private set; }
				public bool IsOption { get; private set; }
				public bool Enabled { get; private set; }

				public Data( XmlNode node )
				{
					Type = XMLUtil.ParseAttribute<CS_AgreementData.eTypeFlag>( node, "Type", CS_AgreementData.eTypeFlag.None );
					Text = XMLUtil.ParseAttribute<string>( node, "Text", "" );
					Link = XMLUtil.ParseAttribute<string>( node, "Link", "" );
					IsOption = XMLUtil.ParseAttribute<bool>( node, "IsOption", false );
					Enabled = XMLUtil.ParseAttribute<bool>( node, "Enabled", true );
				}
			}

			public bool Enabled { get; private set; }

			public List<Data> Data_List { get; private set; }

			public AgreementConfirm( XmlNode node )
			{
				Enabled = XMLUtil.ParseAttribute<bool>( node, "Enabled", true );

				Data_List = new List<Data>();
				foreach( XmlNode child in node.SelectNodes( "Data" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					Data_List.Add( new Data( child ) );
				}

			}

			//------------------------------------------------------------------------
			public Data Find_Data( CS_AgreementData.eTypeFlag type )
			{
				return Data_List.Find( a => a.Type == type );
			}

			//------------------------------------------------------------------------
			public List<CS_AgreementData> ToCS( CS_AgreementData.eTypeFlag agreement_flags, string language)
			{
				if( Enabled == false || Data_List.Count <= 0 )
					return null;

				List<CS_AgreementData> list = null;

				bool need_agree = false;
				foreach( Data data in Data_List )
				{
					if( data.Enabled == false || data.IsOption )
						continue;

					if( ( agreement_flags & data.Type ) == 0 )
					{
						need_agree = true;
						break;
					}
				}

				if( need_agree )
				{
					foreach( Data data in Data_List )
					{
						if( data.Enabled == false )
							continue;

						if( data.Type != CS_AgreementData.eTypeFlag.None )
						{
							CS_AgreementData cs_data = new CS_AgreementData();
							cs_data.agreement_type = data.Type;
							cs_data.text = I18NTextMultiLanguage.Instance.GetTextBase( language, data.Text );
							cs_data.link_url = data.Link;
							if( string.IsNullOrEmpty( cs_data.link_url ) )
							{
								switch( data.Type )
								{
									case CS_AgreementData.eTypeFlag.TermsOfService:
										{
											URLConfig.Data url_data = Instance.URL_Data.FindData( CS_URLConfig.eURLType.TermsOfService );
											if( url_data != null )
											{
												cs_data.link_url = url_data.FindLanguageUrl( language );
											}
										}
										break;

									case CS_AgreementData.eTypeFlag.PrivacyPolicy:
										{
											URLConfig.Data url_data = Instance.URL_Data.FindData( CS_URLConfig.eURLType.PrivacyPolicy );
											if( url_data != null )
											{
												cs_data.link_url = url_data.FindLanguageUrl( language );
											}
										}
										break;
								}
							}
							cs_data.is_option = data.IsOption;

							if( list == null )
								list = new List<CS_AgreementData>();

							list.Add( cs_data );
						}
					}
				}

				return list;
			}
		}

		public int TargetFrameRate { get; private set; }
		public bool NetworkUnknownDisconnectIgnorePendingLogin { get; private set; }

		public bool TBLSendEnable { get; private set; }
		public int TBLSendMaxByte { get; private set; }
		public int ClientFrameRate { get; private set; }

		public Shutdown Shutdown_Data { get; private set; }
		public VersionInfo VersionInfo_Data { get; private set; }
		public MaintenanceWhitelist MaintenanceWhitelist_Data { get; private set; }

		public URLConfig URL_Data { get; private set; }
		public AgreementConfirm AgreementConfirm_Data { get; private set; }


		public override string ROOT_NODE => CONFIG_ID;
		public override string RELOAD_DATA_ID => CONFIG_ID;

		//------------------------------------------------------------------------
		protected override void LoadConfigData( XmlNode node )
		{
			TargetFrameRate = XMLUtil.ParseAttribute<int>( node, "TargetFrameRate", 30 );
			NetworkUnknownDisconnectIgnorePendingLogin = XMLUtil.ParseAttribute<bool>( node, "NetworkUnknownDisconnectIgnorePendingLogin", true );

			TBLSendEnable = XMLUtil.ParseAttribute<bool>( node, "TBLSendEnable", false );
			TBLSendMaxByte = XMLUtil.ParseAttribute<int>( node, "TBLSendMaxByte", 10000 );

			ClientFrameRate = XMLUtil.ParseAttribute<int>( node, "ClientFrameRate", 0 );

			Shutdown_Data = new Shutdown( node.SelectSingleNode( "Shutdown" ) );
			VersionInfo_Data = new VersionInfo( node.SelectSingleNode( "VersionInfo" ) );
			MaintenanceWhitelist_Data = new MaintenanceWhitelist( node.SelectSingleNode( "MaintenanceWhitelist" ) );

			URL_Data = new URLConfig( node.SelectSingleNode( "URL" ) );
			AgreementConfirm_Data = new AgreementConfirm( node.SelectSingleNode( "AgreementConfirm" ) );

		}

		//------------------------------------------------------------------------
		public virtual void ServerShutdownCheck( UMPServerApplication server_app, string application_identifier, string language )
		{
			if( server_app.IsShutdown )
			{
				string time_text = Shutdown_Data.ToTimeText( language );
				string text_key = Shutdown_Data.TextKey;
				string url = Shutdown_Data.URL;

				Shutdown.PerApp per_app = Shutdown_Data.Find_PerApp( application_identifier );
				if( per_app != null )
				{
					text_key = per_app.TextKey;
					url = per_app.URL;
				}

				if( string.IsNullOrEmpty( text_key ) )
					text_key = SystemTextKey.SHUTDOWN;

				throw new PacketException( (int)eDisconnectErrorCode.ServerMaintenance, I18NTextMultiLanguage.Instance.GetText( language, text_key, time_text, url ) );
			}
		}

		//------------------------------------------------------------------------
		public virtual void VersionCheck( UMPServerApplication server_app, string application_identifier, Version client_version, int client_revision, string localize)
		{
			VersionInfo.PerApp version_data = VersionInfo_Data.Find_PerApp( application_identifier );
			if( version_data == null )
				throw new PacketException( (int)eDisconnectErrorCode.VersionDisallow, I18NTextMultiLanguage.Instance.GetText( localize, SystemTextKey.VERSION_DISALLLOW ) );

			if( version_data.Review_Data != null && version_data.Review_Data.ReviewVersion != null && server_app.ServiceType != eServiceType.Review )
			{
				if( client_version >= version_data.Review_Data.ReviewVersion )
				{
					throw new PacketException( (int)eDisconnectErrorCode.NewVersionToReview, $"{version_data.Review_Data.LoginHost}:{version_data.Review_Data.LoginPort}" );
				}
			}

			if( version_data.IsCheck && version_data.MinVersion != null && client_version <= version_data.MinVersion )
				throw new PacketException( (int)eDisconnectErrorCode.VersionExpire, I18NTextMultiLanguage.Instance.GetText( localize, SystemTextKey.VERSION_DISALLLOW ) );

			if( version_data.IsCheck && DateTime.Now >= version_data.ExpireTime )
				throw new PacketException( (int)eDisconnectErrorCode.VersionExpire, I18NTextMultiLanguage.Instance.GetText( localize, SystemTextKey.VERSION_EXPIRE ) );

			if( version_data.IsCheck == true && ( client_version < version_data.ClientVersion || client_revision < version_data.ClientRevision ) )
			{
				string updateStringKey = version_data.Text_Data.ClientUpdateText;
				if( string.IsNullOrEmpty( updateStringKey ) )
					updateStringKey = SystemTextKey.VERSION_UPDATE;

				throw new PacketException( (int)eDisconnectErrorCode.VersionUpdated, I18NTextMultiLanguage.Instance.GetText( localize, updateStringKey, version_data.ClientUpdateLink ) );
			}

			if( version_data.MaxVersion != null && client_version > version_data.MaxVersion )
			{
				string str_key = version_data.Text_Data.MaxVersionText;
				if( string.IsNullOrEmpty( str_key ) )
					str_key = SystemTextKey.VERSION_MAX;

				throw new PacketException( (int)eDisconnectErrorCode.VersionMax, I18NTextMultiLanguage.Instance.GetText( localize, str_key ) );
			}
		}

		//------------------------------------------------------------------------
		public virtual void MaintenanceCheck(UMPServerApplication server_app, string application_identifier, string language )
		{
			if( server_app.IsMaintenance )
			{
				string time_text = Shutdown_Data.ToTimeText( language );
				string url = Shutdown_Data.URL;

				Shutdown.PerApp per_app = Shutdown_Data.Find_PerApp( application_identifier );
				if( per_app != null )
				{
					time_text = per_app.TextKey;
					url = per_app.URL;
				}

				VersionInfo.PerApp version_data = VersionInfo_Data.Find_PerApp( application_identifier );
				string maintenance_text_key = version_data.Text_Data.MaintenanceText;
				if( string.IsNullOrEmpty( maintenance_text_key ) )
					maintenance_text_key = SystemTextKey.MAINTENANCE;

				throw new PacketException( (int)eDisconnectErrorCode.ServerMaintenance, I18NTextMultiLanguage.Instance.GetText( language, maintenance_text_key, time_text, url ) );
			}
		}

		//------------------------------------------------------------------------
		public CS_AppConfig ToCS( CS_AgreementData.eTypeFlag agreement_flags, string language)
		{
			CS_AppConfig config = new CS_AppConfig();
			config.flags = CS_AppConfig.eFlags.None;
			config.client_frame_rate = ClientFrameRate;
			config.url_list = URL_Data.ToCS( language );
			config.agreement_list = AgreementConfirm_Data.ToCS( agreement_flags, language );

			return config;
		}
	}
}
