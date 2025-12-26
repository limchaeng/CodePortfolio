//////////////////////////////////////////////////////////////////////////
//
// NPID_SM
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
using UMF.Net;
using UMF.Core;
using System.Collections.Generic;
using UMP.CSCommon;

namespace UMP.Server
{
	//------------------------------------------------------------------------	
	public enum NPID_S2M : short
	{
		__BEGIN = short.MinValue + 1,
		//
		MasterCommandResponse,
		ServerConnectionInfo,
		ServerConnectionKeyUpdate,
	}

	//------------------------------------------------------------------------
	public enum NPID_M2S : short
	{
		__BEGIN = short.MinValue + 1,
		//
		UpdateServerInfoToLogin,
		UpdateServerStatusToLogin,
		//
		CMD_root,
		CMD_server,
		CMD_reload,
	}

	//////////////////////////////////////////////////////////////////////////
	///
	//------------------------------------------------------------------------
	[Packet( NPID_S2M.MasterCommandResponse )]
	public class NS2M_MasterCommandResponse : MasterCommandBase
	{
		public string command;
		public string response;
	}

	//------------------------------------------------------------------------
	public class NP_ServerInfoData
	{
		public eServerType server_type;
		public int world_idn;
		public int server_net_index;
		public long server_guid;
		public string notify_host_name;
		public short notify_port;
		public int peer_count;
		public long connection_key;

		public string GetInfo()
		{
			return $"[W:{world_idn}][GUID:{server_guid}][Host:{notify_host_name}:{notify_port}][peer:{peer_count}][key:{connection_key}]";
		}
	}
	[Packet( NPID_S2M.ServerConnectionInfo )]
	public class NS2M_ServerConnectionInfo
	{
		public NP_ServerInfoData info_data;
	}

	//------------------------------------------------------------------------

	[Packet( NPID_M2S.UpdateServerInfoToLogin )]
	public class NM2S_UpdateServerInfoToLogin : MasterCommandBase
	{
		public List<NP_ServerInfoData> server_info_list;
	}

	//------------------------------------------------------------------------

	[Packet( NPID_M2S.UpdateServerStatusToLogin )]
	public class NM2S_UpdateServerStatusToLogin
	{
		public eServerType server_type;
		public long server_guid;
		public int world_idn;
		public int peer_count;
		public long connection_key;
	}

	//------------------------------------------------------------------------
	[Packet(NPID_S2M.ServerConnectionKeyUpdate)]
	public class NS2M_ServerConnectionKeyUpdate
	{
		public eServerType server_type;
		public int peer_count;
		public long update_connection_key;
	}

	//------------------------------------------------------------------------
	[Packet( NPID_M2S.CMD_root )]
	public class NM2S_CMD_root : MasterCommandBase
	{
		public int int_value;
	}

	//------------------------------------------------------------------------

	[Packet( NPID_M2S.CMD_server )]
	public class NM2S_CMD_server : MasterCommandBase
	{
		public int int_value;
	}

	//------------------------------------------------------------------------	
	[Packet( NPID_M2S.CMD_reload )]
	public class NM2S_CMD_reload : MasterCommandBase
	{
		[PacketValue(Type = PacketValueType.SerializeNullable)]			
		public List<string> reload_id_list;
	}
}
