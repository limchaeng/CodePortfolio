//////////////////////////////////////////////////////////////////////////
//
// ContentsServerApplication
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
using UMF.Net;

namespace UMP.Server.Contents
{
	//------------------------------------------------------------------------	
	public class ContentsServerApplication : UMPServerApplication
	{
		public PeerManagerBase ClientPeerManager { get; set; } = null;

		public ContentsServerApplication( string server_name, eServiceType service_type, string config_file, string[] args )
			: base( server_name, eServerType.Contents, service_type, config_file, args )
		{
			AddUpdater( ConnectionKeyManager.Instance.Update );
		}
	}
}
