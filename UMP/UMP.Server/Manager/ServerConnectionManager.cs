//////////////////////////////////////////////////////////////////////////
//
// ServerConnectionManager
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

using System.Collections.Generic;
using System.Linq;
using UMF.Core;
using UMP.CSCommon;

namespace UMP.Server
{
	public class ServerConnectionManager : Singleton<ServerConnectionManager>
	{
		List<NP_ServerInfoData> mServerList = new List<NP_ServerInfoData>();
		public List<NP_ServerInfoData> ServerList { get { return mServerList; } }

		//------------------------------------------------------------------------
		public void UpdateServerInfo( List<NP_ServerInfoData> server_info_list )
		{
			foreach( NP_ServerInfoData server_info in server_info_list )
			{
				mServerList.RemoveAll( a => a.world_idn == server_info.world_idn && a.server_guid == server_info.server_guid );
				mServerList.Add( server_info );
			}
		}

		//------------------------------------------------------------------------
		public void UpdateStatus( int world_idn, long guid, int peer_count, long connection_key )
		{
			NP_ServerInfoData data = FindServer( world_idn, guid );
			if( data != null )
			{
				data.peer_count = peer_count;
				data.connection_key = connection_key;
			}
		}

		//------------------------------------------------------------------------
		public void RemoveServer(int world_ind, long guid)
		{
			mServerList.RemoveAll( a => a.world_idn == world_ind && a.server_guid == guid );
		}

		//------------------------------------------------------------------------
		public NP_ServerInfoData FindServer( int world_idn, long guid )
		{
			return mServerList.Find( a => a.world_idn == world_idn && a.server_guid == guid );
		}

		//------------------------------------------------------------------------
		public NP_ServerInfoData FindFreeServer( eServerType server_type, int world_idn )
		{
			return mServerList.Where( a => a.server_type == server_type && a.connection_key > 0 && a.notify_port > 0 && a.world_idn == world_idn ).OrderBy( g2 => g2.peer_count ).FirstOrDefault();
		}

		//------------------------------------------------------------------------
		public int GetPeerCountPerWorld( int world_idn )
		{
			if( mServerList.Count > 0 )
				return mServerList.Where( a => a.world_idn == world_idn ).Sum( a => a.peer_count );

			return 0;
		}
	}
}
