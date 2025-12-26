//////////////////////////////////////////////////////////////////////////
//
// TimeConfig
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

namespace UMP.Server
{
	//------------------------------------------------------------------------
	public class TimeConfig : ReloadConfig<TimeConfig>
	{
		public const string CONFIG_ID = "TimeConfig";

		public bool TimeSyncEnable { get; private set; }
		public int TimeSyncInterval { get; private set; }


		public override string ROOT_NODE => CONFIG_ID;
		public override string RELOAD_DATA_ID => CONFIG_ID;

		//------------------------------------------------------------------------
		protected override void LoadConfigData( XmlNode node )
		{
			TimeSyncEnable = XMLUtil.ParseAttribute<bool>( node, "TimeSyncEnable", true );
			TimeSyncInterval = XMLUtil.ParseAttribute<int>( node, "TimeSyncInterval", 60 );
		}
	}
}
