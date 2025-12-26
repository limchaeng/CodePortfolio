//////////////////////////////////////////////////////////////////////////
//
// DaemonMasterPeerManager
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

namespace UMP.Server.Master
{
	//------------------------------------------------------------------------
	/// <summary>
	///  기본 Peer 타입 변경시 CreateNewPeer override해서 타입별로 create 해줘야함
	/// </summary>
	public class DaemonMasterPeerManager : AppPeerManager<DaemonMasterPeer>
	{
		protected MasterServerApplication mApplication = null;
		public MasterServerApplication Application
		{
			get { return mApplication; }
		}

		public DaemonMasterPeerManager( MasterServerApplication application, string config_file )
			: this( application, config_file, typeof( NPID_M2D ), new D2M_PacketHandlerManagerStandard( application ) )
		{
		}
		public DaemonMasterPeerManager( MasterServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packetHandlerManager ) 
			: base( application, config_file, packetHandlerManager, send_packet_id_type, typeof( NPID_M2D ) )
		{
			mApplication = application;
			mApplication.DaemonPeerManager = this;
		}

		//------------------------------------------------------------------------		
		public void BroadcastPacketTo<PacketType>( PacketType packet, int world ) where PacketType : class
		{
			foreach( DaemonMasterPeer daemon in mPeersDic.Values )
			{
				if( world == 0 || daemon.WorldIDN == world )
					daemon.SendPacket( packet );
			}
		}

		//------------------------------------------------------------------------
		public DaemonMasterPeer FindDaemon( int world_idn, int daemon_idx )
		{
			foreach( DaemonMasterPeer daemon in mPeersDic.Values )
			{
				if( daemon.WorldIDN == world_idn && daemon.PeerIndex == daemon_idx )
					return daemon;
			}

			return null;
		}

		//------------------------------------------------------------------------		
		public string ShowInfo()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine( "== Daemon INFO ==" );
			foreach( DaemonMasterPeer daemon in mPeersDic.Values )
			{
				sb.AppendLine( daemon.GetInfo() );
			}
			sb.AppendLine( "=" );
			return sb.ToString();
		}

	}
}
