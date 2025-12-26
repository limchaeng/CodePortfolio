//////////////////////////////////////////////////////////////////////////
//
// ServerConfig
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

using System.Collections.Generic;
using System.Xml;
using UMF.Core;
using UMP.CSCommon;

namespace UMP.Server
{
	//------------------------------------------------------------------------
	public class ServerConfig : ReloadConfig<ServerConfig>
	{
		public const string CONFIG_ID = "ServerConfig";

		//------------------------------------------------------------------------
		public class Data
		{
			public eServerType ServerType { get; private set; }
			public int InfoUpdateInterval { get; private set; }
			public int ConnectionKeyUpdateInterval { get; private set; }
			public int ConnectionKeySaveMaxCount { get; private set; }
			public bool ConnectionKeyCheck { get; private set; }

			public Data( XmlNode node )
			{
				ServerType = XMLUtil.ParseAttribute<eServerType>( node, "ServerType", eServerType.Unknown );

				InfoUpdateInterval = XMLUtil.ParseAttribute<int>( node, "InfoUpdateInterval", 30 );

				ConnectionKeyUpdateInterval = XMLUtil.ParseAttribute<int>( node, "ConnectionKeyUpdateInterval", 0 );
				ConnectionKeySaveMaxCount = XMLUtil.ParseAttribute<int>( node, "ConnectionKeySaveMaxCount", 0 );
				ConnectionKeyCheck = XMLUtil.ParseAttribute<bool>( node, "ConnectionKeyCheck", false );
			}
		}


		public List<Data> Data_List { get; private set; }
		public override string ROOT_NODE => CONFIG_ID;
		public override string RELOAD_DATA_ID => CONFIG_ID;

		//------------------------------------------------------------------------
		protected override void LoadConfigData( XmlNode node )
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
		public Data FindData(eServerType server_type)
		{
			return Data_List.Find( a => a.ServerType == server_type );
		}
	}
}
