//////////////////////////////////////////////////////////////////////////
//
// HackConfig
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
	public class HackConfig : ReloadConfig<HackConfig>
	{
		public const string CONFIG_ID = "HackConfig";

		public int SpeedHackServerCheckTime { get; private set; }
		public int SpeedHackServerCheckTimeMin { get; private set; }
		public int SpeedHackServerCheckCount { get; private set; }
		public int FastLoginHackCheckTime { get; private set; }
		public int FastLoginHackCheckCount { get; private set; }


		public override string ROOT_NODE => CONFIG_ID;
		public override string RELOAD_DATA_ID => CONFIG_ID;

		//------------------------------------------------------------------------
		protected override void LoadConfigData( XmlNode node )
		{
			SpeedHackServerCheckTime = XMLUtil.ParseAttribute<int>( node, "SpeedHackServerCheckTime", 30 );
			SpeedHackServerCheckTimeMin = XMLUtil.ParseAttribute<int>( node, "SpeedHackServerCheckTimeMin", 2 );
			SpeedHackServerCheckCount = XMLUtil.ParseAttribute<int>( node, "SpeedHackServerCheckCount", 5 );
			FastLoginHackCheckTime = XMLUtil.ParseAttribute<int>( node, "FastLoginHackCheckTime", 30 );
			FastLoginHackCheckCount = XMLUtil.ParseAttribute<int>( node, "FastLoginHackCheckCount", 5 );

		}
	}
}
