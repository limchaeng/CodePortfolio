//////////////////////////////////////////////////////////////////////////
//
// LocalizationConfig
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

using System.Xml;
using System.Collections.Generic;
using UMF.Core;
using System;
using UMP.CSCommon.Packet;
using UMP.CSCommon;

namespace UMP.Server
{
	//------------------------------------------------------------------------
	public class LocalizationConfig : ReloadConfig<LocalizationConfig>
	{
		public const string CONFIG_ID = "LocalizationConfig";

		//------------------------------------------------------------------------
		public class Data
		{
			//------------------------------------------------------------------------
			public class Info
			{
				public string Language { get; private set; }
				public string ShowName { get; private set; }
				public string Alias { get; private set; }
				public string IconName { get; private set; }
				public string FontName { get; private set; }
				public string CultureCode { get; private set; }

				public Info( XmlNode node )
				{
					Language = XMLUtil.ParseAttribute<string>( node, "Language", "" );
					ShowName = XMLUtil.ParseAttribute<string>( node, "ShowName", "" );
					Alias = XMLUtil.ParseAttribute<string>( node, "Alias", "" );
					IconName = XMLUtil.ParseAttribute<string>( node, "IconName", "" );
					FontName = XMLUtil.ParseAttribute<string>( node, "FontName", "" );
					CultureCode = XMLUtil.ParseAttribute<string>( node, "CultureCode", "" );

				}
			}

			public string AppIdentifier { get; private set; }
			public bool ReloginWhenLanguageChange { get; private set; }

			public List<Info> Info_List { get; private set; }

			public Data( XmlNode node )
			{
				AppIdentifier = XMLUtil.ParseAttribute<string>( node, "AppIdentifier", "" );
				ReloginWhenLanguageChange = XMLUtil.ParseAttribute<bool>( node, "ReloginWhenLanguageChange", true);

				Info_List = null;
				foreach( XmlNode child in node.SelectNodes( "Info" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					if( Info_List == null )
						Info_List = new List<Info>();

					Info_List.Add( new Info( child ) );
				}

			}

			//------------------------------------------------------------------------
			public string AvailableLocalize( string user_localize )
			{
				if( Info_List != null )
				{
					foreach( Info info in Info_List )
					{
						if( info.Language.ToLower() == user_localize.ToLower() )
							return info.Language;
					}
				}

				return null;
			}

			//------------------------------------------------------------------------
			public CS_LocalizationConfig ToCS()
			{
				CS_LocalizationConfig cs_data = new CS_LocalizationConfig();
				cs_data.support_list = new List<CS_LocalizationSupportData>();

				foreach( Info info in Info_List )
				{
					CS_LocalizationSupportData data = new CS_LocalizationSupportData();
					data.language = info.Language;
					data.alias = info.Alias;
					data.icon_name = info.IconName;
					data.font_asset_name = info.FontName;
					data.language_name = info.ShowName;
					data.culture_code = info.CultureCode;

					cs_data.support_list.Add( data );
				}

				return cs_data;
			}
		}

		//------------------------------------------------------------------------
		public class FastUpdateConfig 
		{
			public class LanguageData
			{
				public string Language { get; private set; }
				public string Text { get; private set; }

				public LanguageData( XmlNode node )
				{
					Language = XMLUtil.ParseAttribute<string>( node, "Type", "" );
					Text = node.InnerText.Replace( "\\n", "\n" );
				}

				public bool IsValid()
				{
					return ( string.IsNullOrEmpty( Text ) == false );
				}
			}

			public class ConditionData
			{
				public class Data
				{
					public string Key { get; private set; }
					public List<LanguageData> LanguageList { get; private set; }

					public Data( XmlNode node )
					{
						Key = XMLUtil.ParseAttribute<string>( node, "Key", "" );

						LanguageList = new List<LanguageData>();
						foreach( XmlNode child in node.SelectNodes( "Language" ) )
						{
							if( child.NodeType == XmlNodeType.Comment )
								continue;

							LanguageList.Add( new LanguageData( child ) );
						}
					}

					public _P_LocalizeTextFastUpdateNotifyData ToPacket( string localize )
					{
						if( string.IsNullOrEmpty( Key ) )
							return null;

						LanguageData l_data = LanguageList.Find( a => a.Language == localize );
						if( l_data == null || l_data.IsValid() == false )
							return null;

						_P_LocalizeTextFastUpdateNotifyData p_data = new _P_LocalizeTextFastUpdateNotifyData();
						p_data.key = Key;
						p_data.string_list = new List<string>();
						p_data.string_list.Add( l_data.Text );

						return p_data;
					}
				}

				public Version ClientVer { get; private set; }
				public string AppIdentifier { get; private set; }
				public string DevicePackage { get; private set; }
				public List<Data> DataList { get; private set; }

				public ConditionData( XmlNode node )
				{
					ClientVer = XMLUtil.ParseAttribute<Version>( node, "ClientVer", null );
					AppIdentifier = XMLUtil.ParseAttribute<string>( node, "AppIdentifier", "" );
					DevicePackage = XMLUtil.ParseAttribute<string>( node, "DevicePackage", "" );

					DataList = new List<Data>();
					foreach( XmlNode child in node.SelectNodes( "Data" ) )
					{
						if( child.NodeType == XmlNodeType.Comment )
							continue;

						DataList.Add( new Data( child ) );
					}
				}

				public bool IsMatch(Version version, string app_id, string device_package)
				{
					return ( ClientVer == version && AppIdentifier == app_id && DevicePackage == device_package );
				}
			}

			public List<ConditionData> ConditionDataList { get; private set; }

			//------------------------------------------------------------------------		
			public FastUpdateConfig(XmlNode node)
			{
				ConditionDataList = new List<ConditionData>();
				foreach( XmlNode child in node.SelectNodes( "Version" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					ConditionDataList.Add( new ConditionData( child ) );
				}
			}

			//------------------------------------------------------------------------
			public List<_P_LocalizeTextFastUpdateNotifyData> GetFastUpdateData( Version client_ver, string app_id, string device_package, string localize  )
			{
				if( ConditionDataList.Count <= 0 )
					return null;

				ConditionData v_data = ConditionDataList.Find( a => a.IsMatch( client_ver, app_id, device_package ) );
				if( v_data == null )
					v_data = ConditionDataList.Find( a => a.IsMatch( client_ver, app_id, "" ) );

				if( v_data == null )
					v_data = ConditionDataList.Find( a => a.IsMatch( client_ver, "", "" ) );

				if( v_data == null )
					return null;

				List<_P_LocalizeTextFastUpdateNotifyData> list = null;
				foreach( ConditionData.Data data in v_data.DataList )
				{
					_P_LocalizeTextFastUpdateNotifyData p_data = data.ToPacket( localize );
					if( p_data != null )
					{
						if( list == null )
							list = new List<_P_LocalizeTextFastUpdateNotifyData>();

						list.Add( p_data );
					}
				}

				return list;
			}
		}

		public string DefaultLanguage { get; private set; }
		public string DefaultCulture { get; private set; }

		public Data BaseData { get; private set; }
		public List<Data> Data_List { get; private set; }
		public FastUpdateConfig FastUpdate { get; private set; }


		public override string ROOT_NODE => CONFIG_ID;
		public override string RELOAD_DATA_ID => CONFIG_ID;

		//------------------------------------------------------------------------
		protected override void LoadConfigData( XmlNode node )
		{
			DefaultLanguage = XMLUtil.ParseAttribute<string>( node, "DefaultLanguage", "Korean" );
			DefaultCulture = XMLUtil.ParseAttribute<string>( node, "DefaultCulture", "ko-kr" );

			BaseData = new Data( node.SelectSingleNode( "Base" ) );

			Data_List = null;
			foreach( XmlNode child in node.SelectNodes( "Data" ) )
			{
				if( child.NodeType == XmlNodeType.Comment )
					continue;

				if( Data_List == null )
					Data_List = new List<Data>();

				Data_List.Add( new Data( child ) );
			}

			FastUpdate = null;
			XmlNode fast_node = node.SelectSingleNode( "FastUpdate" );
			if( fast_node != null )
				FastUpdate = new FastUpdateConfig( fast_node );
		}

		//------------------------------------------------------------------------	
		public string AvailableLanguage( string app_id, string user_localize )
		{
			string available_localize = null;
			if( Data_List != null )
			{
				Data data = Data_List.Find( a => a.AppIdentifier == app_id );
				if( data != null )
				{
					available_localize = data.AvailableLocalize( user_localize );
					if( available_localize != null )
						return available_localize;
				}
			}

			available_localize = BaseData.AvailableLocalize( user_localize );
			if( available_localize != null )
				return available_localize;

			available_localize = DefaultLanguage;
			return available_localize;
		}

		//------------------------------------------------------------------------
		public CS_LocalizationConfig ToCS( string app_id )
		{
			Data data = Data_List.Find( a => a.AppIdentifier == app_id );
			if( data == null )
				data = BaseData;

			return data.ToCS();
		}
	}

}
