//////////////////////////////////////////////////////////////////////////
//
// LoginMasterConnector
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
using UMF.Net;

namespace UMP.Server.Login
{
	//------------------------------------------------------------------------	
	public class LoginMasterConnector : ServerMasterConnector
	{
		protected LoginServerApplication mApplication = null;
		public LoginServerApplication Application
		{
			get { return mApplication; }
		}

		public LoginMasterConnector( LoginServerApplication application, string config_file )
			: this( application, config_file, typeof( NPID_S2M ), new M2L_PacketHandlerManagerStandard( application ) )
		{

		}
		public LoginMasterConnector( LoginServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packetHandlerManager )
			: base( application, config_file, send_packet_id_type, typeof( NPID_S2M ), packetHandlerManager )
		{
			mApplication = application;
		}
	}
}
