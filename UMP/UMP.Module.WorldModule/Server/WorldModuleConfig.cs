//////////////////////////////////////////////////////////////////////////
//
// WorldModuleConfig
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

using System.Collections.Generic;
using System.Xml;
using UMF.Core;
using UMP.Server;

namespace UMP.Module.WorldModule
{
	//------------------------------------------------------------------------
	public class WorldModuleConfig : ReloadConfig<WorldModuleConfig>
	{
		public const string CONFIG_ID = "WorldModuleConfig";

		//------------------------------------------------------------------------
		public class Text 
		{
			public string Maintenance { get; private set; }
			public string Smooth { get; private set; }
			public string Normal { get; private set; }
			public string Busy { get; private set; }

			//------------------------------------------------------------------------
			public Text( XmlNode node )
			{
				Maintenance = XMLUtil.ParseAttribute<string>( node, "Maintenance", "" );
				Smooth = XMLUtil.ParseAttribute<string>( node, "Smooth", "" );
				Normal = XMLUtil.ParseAttribute<string>( node, "Normal", "" );
				Busy = XMLUtil.ParseAttribute<string>( node, "Busy", "" );
			}
			public Text( XmlNode node, Text def_text )
			{
				Maintenance = XMLUtil.ParseAttribute<string>( node, "Maintenance", def_text.Maintenance );
				Smooth = XMLUtil.ParseAttribute<string>( node, "Smooth", def_text.Smooth );
				Normal = XMLUtil.ParseAttribute<string>( node, "Normal", def_text.Normal );
				Busy = XMLUtil.ParseAttribute<string>( node, "Busy", def_text.Busy );

			}
		}

		//------------------------------------------------------------------------
		public class Data
		{
			public int Order { get; private set; }
			public int WorldIDN { get; private set; }
			public string NameKey { get; private set; }
			public bool IsRecommend { get; private set; }
			public bool IsNew { get; private set; }
			public string FixedStateKey { get; private set; }
			public int SmoothCountMax { get; private set; }
			public int NormalCountMax { get; private set; }
			public int BusyCountMax { get; private set; }

			public Text TextData { get; private set; }

			public Data( XmlNode node, Text def_text )
			{
				Order = XMLUtil.ParseAttribute<int>( node, "Order", 1 );
				WorldIDN = XMLUtil.ParseAttribute<int>( node, "WorldIDN", 1 );
				NameKey = XMLUtil.ParseAttribute<string>( node, "NameKey", "" );
				IsRecommend = XMLUtil.ParseAttribute<bool>( node, "IsRecommend", false );
				IsNew = XMLUtil.ParseAttribute<bool>( node, "IsNew", false );
				FixedStateKey = XMLUtil.ParseAttribute<string>( node, "FixedStateKey", "" );
				SmoothCountMax = XMLUtil.ParseAttribute<int>( node, "SmoothCountMax", 500 );
				NormalCountMax = XMLUtil.ParseAttribute<int>( node, "NormalCountMax", 1000 );
				BusyCountMax = XMLUtil.ParseAttribute<int>( node, "BusyCountMax", 2900 );

				XmlNode text_node = node.SelectSingleNode( "Text" );
				if( text_node == null )
					TextData = def_text;
				else
					TextData = new Text( text_node, def_text );
			}
		}

		public int RefreshTimeout { get; private set; }
		public bool Enabled { get; private set; }

		public List<Data> Data_List { get; private set; }

		public Text DefTextData { get; private set; }

		public override string ROOT_NODE => CONFIG_ID;
		public override string RELOAD_DATA_ID => CONFIG_ID;

		//------------------------------------------------------------------------
		protected override void LoadConfigData( XmlNode node )
		{
			RefreshTimeout = XMLUtil.ParseAttribute<int>( node, "RefreshTimeout", 10 );
			Enabled = XMLUtil.ParseAttribute<bool>( node, "Enabled", false );

			DefTextData = new Text( node.SelectSingleNode( "DefaultText" ) );

			Data_List = new List<Data>();
			foreach( XmlNode child in node.SelectNodes( "Data" ) )
			{
				if( child.NodeType == XmlNodeType.Comment )
					continue;

				Data_List.Add( new Data( child, DefTextData ) );
			}

		}

		//------------------------------------------------------------------------
		public Data Find_Data( int order )
		{
			return Data_List.Find( a => a.Order == order );
		}
	}
}

#endif