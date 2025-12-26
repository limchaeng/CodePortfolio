//////////////////////////////////////////////////////////////////////////
//
// ServerManager
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

using UMF.Core;
using System.Collections.Generic;
using UMP.CSCommon;
using UMF.Net;
namespace UMP.Server.Master
{
	public class ServerManager : Singleton<ServerManager>
	{
		public class ServerInfo
		{
			public ServerMasterPeer peer = null;

			public int peer_index = 0;

			public NP_ServerInfoData info_data = null;

			public ServerInfo( ServerMasterPeer _peer )
			{
				peer = _peer;
				peer_index = peer.PeerIndex;
				info_data = null;
			}
		}

		List<ServerInfo> mServerInfo = new List<ServerInfo>();

		//------------------------------------------------------------------------
		public ServerInfo UpdateServeInfo( ServerMasterPeer peer,  NS2M_ServerConnectionInfo packet)
		{
			ServerInfo data = mServerInfo.Find( a => a.peer.ServerType == peer.ServerType && a.peer.PeerIndex == peer.PeerIndex );
			if( data == null )
			{
				data = new ServerInfo( peer );
				mServerInfo.Add( data );
			}

			data.info_data = packet.info_data;

			return data;
		}

		//------------------------------------------------------------------------
		public ServerInfo FindServer( eServerType server_type, ServerMasterPeer peer)
		{
			return mServerInfo.Find( a => a.peer.ServerType == server_type && a.peer.PeerIndex == peer.PeerIndex );
		}

		//------------------------------------------------------------------------
		public void RemoveInfo( int peer_index)
		{
			mServerInfo.RemoveAll( a => a.peer_index == peer_index );
		}

		//------------------------------------------------------------------------		
		public List<NP_ServerInfoData> GetServerInfoAll()
		{
			List<NP_ServerInfoData> list = null;
			foreach( ServerInfo s in mServerInfo )
			{
				if( s.info_data != null )
				{
					if( list == null )
						list = new List<NP_ServerInfoData>();

					list.Add( s.info_data );
				}
			}

			return list;
		}
	}
}
