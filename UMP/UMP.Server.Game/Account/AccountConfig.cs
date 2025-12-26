//////////////////////////////////////////////////////////////////////////
//
// AccountConfig
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
using UMF.Core;

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------
	public class AccountConfig : ReloadConfig<AccountConfig>
	{
		public const string CONFIG_ID = "AccountConfig";
		public override string ROOT_NODE => CONFIG_ID;
		public override string RELOAD_DATA_ID => CONFIG_ID;

		public int ReloginTimeoutSeconds { get; private set; }
		public int UseMultiplePlayer { get; private set; }
		public bool UseCommonLogin { get; private set; }

		//------------------------------------------------------------------------
		protected override void LoadConfigData( XmlNode node )
		{
			ReloginTimeoutSeconds = XMLUtil.ParseAttribute<int>( node, "ReloginTimeoutSeconds", 600 );
			UseMultiplePlayer = XMLUtil.ParseAttribute<int>( node, "UseMultiplePlayer", 1 );
			UseCommonLogin = XMLUtil.ParseAttribute<bool>( node, "UseCommonLogin", true );
		}
	}
}
