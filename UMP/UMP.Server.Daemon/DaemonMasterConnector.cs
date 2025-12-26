//////////////////////////////////////////////////////////////////////////
//
// DaemonMasterConnector
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
using System.Net;

namespace UMP.Server.Daemon
{
	//------------------------------------------------------------------------	
	public class DaemonMasterConnector : MasterConnector
	{
		protected DaemonServerApplication mApplication = null;
		public DaemonServerApplication Application
		{
			get { return mApplication; }
		}


		public DaemonMasterConnector( DaemonServerApplication application, string config_file )
			: this( application, config_file, typeof( NPID_D2M ), new M2D_PacketHandlerManagerStandard( application ) )
		{

		}
		public DaemonMasterConnector( DaemonServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packetHandlerManager )
			: base( application, config_file, send_packet_id_type, typeof( NPID_D2M ), packetHandlerManager )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------		
		public override void OnConnected( bool bSuccessed )
		{
			base.OnConnected( bSuccessed );

			if( bSuccessed )
			{
				string ip = ( (IPEndPoint)m_Socket.LocalEndPoint ).Address.ToString();
				mApplication.ServerIP = ip;
				mApplication.SendServerNotification( $"Daemon startup ip:{ip}" );

				ND2M_DaemonStatup _ND2M_DaemonStatup = new ND2M_DaemonStatup();

				SendPacket( _ND2M_DaemonStatup );
			}
		}
	}
}
