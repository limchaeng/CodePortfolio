//////////////////////////////////////////////////////////////////////////
//
// MasterServerApplication
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

using UMP.CSCommon;
using System.Collections.Generic;
using UMF.Net;

namespace UMP.Server.Master
{
	//--------------------------l----------------------------------------------	
	public class MasterServerApplication : UMPServerApplication
	{
		readonly string ARG_KEY_CUSTOM_CONFIG_PATH = "-tblpath";

		public DaemonMasterPeerManager DaemonPeerManager { get; set; } = null;
		public ServerMasterPeerManager ServerPeerManager { get; set; } = null;
		public MasterCommand CommandTool { get; set; } = null;

		public MasterServerApplication( string server_name, eServiceType service_type, string config_file, string[] args )
			: base( server_name, eServerType.Master, service_type, config_file, args )
		{
		}

		//------------------------------------------------------------------------
		protected override bool IsFinishedForShutdown()
		{
			if( base.IsFinishedForShutdown() == false )
				return false;

			if( DaemonPeerManager.PeerCount > 0 || ServerPeerManager.PeerCount > 0 )
				return false;

			return true;
		}
	}
}
