//////////////////////////////////////////////////////////////////////////
//
// AppVerifyModuleConfig
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
#if UMSERVER

using System.Xml;
using System.Collections.Generic;
using UMF.Core;
using UMP.Server;

namespace UMP.Module.AppVerifyModule
{
	//------------------------------------------------------------------------
	public class AppVerifyModuleConfig : ReloadConfig<AppVerifyModuleConfig>
	{
		public const string CONFIG_ID = "AppVerifyModuleConfig";

		//------------------------------------------------------------------------
		public class Data
		{
			public string AppIdentifier { get; private set; }
			public bool Enabled { get; private set; }
			public eAppVerifyRequestTypeFlag RequestTypeFlag { get; private set; }
			public bool HasRequestTypeFlag( eAppVerifyRequestTypeFlag flags )
			{
				return ( ( RequestTypeFlag & flags ) != 0 );
			}

			public Data( XmlNode node )
			{
				AppIdentifier = XMLUtil.ParseAttribute<string>( node, "AppIdentifier", "" );
				Enabled = XMLUtil.ParseAttribute<bool>( node, "Enabled", false );
				RequestTypeFlag = XMLUtil.ParseAttribute<eAppVerifyRequestTypeFlag>( node, "RequestTypeFlag", eAppVerifyRequestTypeFlag.None );

			}
		}

		public eAppVerifyRequestTypeFlag NotifyToLogin { get; private set; }
		public eAppVerifyRequestTypeFlag RequestTypeFlag { get; private set; }
		public bool HasRequestTypeFlag( eAppVerifyRequestTypeFlag flags )
		{
			return ( ( RequestTypeFlag & flags ) != 0 );
		}

		public List<Data> Data_List { get; private set; }

		public override string ROOT_NODE => CONFIG_ID;
		public override string RELOAD_DATA_ID => CONFIG_ID;

		//------------------------------------------------------------------------
		protected override void LoadConfigData( XmlNode node )
		{
			NotifyToLogin = XMLUtil.ParseAttribute<eAppVerifyRequestTypeFlag>( node, "NotifyToLogin", eAppVerifyRequestTypeFlag.None );
			RequestTypeFlag = XMLUtil.ParseAttribute<eAppVerifyRequestTypeFlag>( node, "RequestTypeFlag", eAppVerifyRequestTypeFlag.None );

			Data_List = null;
			foreach( XmlNode child in node.SelectNodes( "Data" ) )
			{
				if( child.NodeType == XmlNodeType.Comment )
					continue;

				if( Data_List == null )
					Data_List = new List<Data>();

				Data_List.Add( new Data( child ) );
			}

		}

		//------------------------------------------------------------------------
		public List<string> FindAppFromVerifyType( eAppVerifyRequestTypeFlag v_type )
		{
			if( Data_List == null )
				return null;

			List<string> list = null;
			foreach( Data p_app in Data_List )
			{
				if( p_app.Enabled && ( p_app.RequestTypeFlag & v_type ) != 0 )
				{
					if( list == null )
						list = new List<string>();

					list.Add( p_app.AppIdentifier );
				}
			}

			return list;
		}

	}

}

#endif