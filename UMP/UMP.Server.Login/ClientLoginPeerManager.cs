//////////////////////////////////////////////////////////////////////////
//
// ClientLoginPeerManager
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
using UMP.CSCommon.Packet;

namespace UMP.Server.Login
{
	//------------------------------------------------------------------------	
	/// <summary>
	///  기본 Peer 타입 변경시 CreateNewPeer override해서 타입별로 create 해줘야함
	/// </summary>
	public class ClientLoginPeerManager : AppPeerManager<ClientLoginPeer>
	{
		protected LoginServerApplication mApplication = null;
		public LoginServerApplication Application
		{
			get { return mApplication; }
		}

		public ClientLoginPeerManager( LoginServerApplication application, string config_file )
			: this( application, config_file, typeof( NPID_L2C ), new C2L_PacketHandlerManagerStandard( application ) )
		{
		}
		public ClientLoginPeerManager( LoginServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packet_handler )
			: base( application, config_file, packet_handler, send_packet_id_type, typeof( NPID_L2C ) )
		{
			mApplication = application;
			mApplication.ClientPeerManager = this;
		}
	}
}
