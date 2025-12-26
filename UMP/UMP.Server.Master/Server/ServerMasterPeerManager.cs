//////////////////////////////////////////////////////////////////////////
//
// ServerMasterPeerManager
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
using System.Net.Sockets;
using System.Linq;
using UMF.Net;
using UMP.CSCommon;
using System.Collections.Generic;

namespace UMP.Server.Master
{
	//------------------------------------------------------------------------	
	public class ServerMasterPeerManager : AppPeerManager<ServerMasterPeer>
	{
		protected MasterServerApplication mApplication = null;
		public MasterServerApplication Application
		{
			get { return mApplication; }
		}

		public ServerMasterPeerManager( MasterServerApplication application, string config_file )
			: this( application, config_file, typeof( NPID_M2S ), new S2M_PacketHandlerManagerStandard( application ) )
		{
		}
		public ServerMasterPeerManager( MasterServerApplication application, string config_file, Type send_packet_id_type, PacketHandlerManagerBase packetHandlerManager )
			: base( application, config_file, packetHandlerManager, send_packet_id_type, typeof( NPID_M2S ) )
		{
			mApplication = application;
			mApplication.ServerPeerManager = this;
		}

		//------------------------------------------------------------------------		
		public string ShowInfo()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine( "== Server INFO ==" );
			foreach( ServerMasterPeer s in mPeersDic.Values )
			{
				sb.AppendLine( s.GetInfo() );
			}
			sb.AppendLine( "=" );

			return sb.ToString();
		}

		//------------------------------------------------------------------------
		public List<Peer> FindPeers(int world_idn, params eServerType[] toServers )
		{
			List<Peer> peers = null;

			foreach( ServerMasterPeer s in mPeersDic.Values )
			{
				if( toServers != null && toServers.Length > 0 && toServers.Contains( s.ServerType ) == false )
					continue;

				if( world_idn > 0 && s.WorldIDN != world_idn )
					continue;

				if( peers == null )
					peers = new List<Peer>();

				peers.Add( s );
			}

			return peers;
		}

		//------------------------------------------------------------------------		
		public void SendToServers<PacketType>( PacketType packet, int world_idn, params eServerType[] toServers ) where PacketType : class
		{
			if( world_idn == 0 && ( toServers == null || toServers.Length <= 0 ) )
			{
				BroadcastPacket( packet );
				return;					
			}

			List<Peer> peers = FindPeers( world_idn, toServers );
			if( peers != null )
				MulticastPacket( peers, packet );
		}
	}
}
