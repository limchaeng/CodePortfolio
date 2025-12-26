//////////////////////////////////////////////////////////////////////////
//
// NPID_RG
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

namespace UMP.Server
{
	//------------------------------------------------------------------------	
	public enum NPID_G2R : short
	{
		__BEGIN = short.MinValue + 1,
		//
		ConnectionInfo,
		AccountLoginNotify,
		AccountLogoutNotify,
		PlayerLoginNotify,
	}

	//------------------------------------------------------------------------
	public enum NPID_R2G : short
	{
		__BEGIN = short.MinValue + 1,
		//
		ConnectionInfoAck,
		AccountLoginNotify,
		AccountLogoutNotify,
		PlayerLoginNotify,
	}

	//////////////////////////////////////////////////////////////////////////
	///
	//------------------------------------------------------------------------	
	[Packet( NPID_G2R.ConnectionInfo )]
	public class NG2R_ConnectionInfo
	{
		public long guid;
		public string host_name;
		public short port;
	}
	[Packet( NPID_R2G.ConnectionInfoAck )]
	public class NR2G_ConnectionInfoAck
	{
		public int index_from_relay;
	}

	//------------------------------------------------------------------------
	[Packet( NPID_G2R.AccountLoginNotify )]
	public class NG2R_AccountLoginNotify
	{
		public long account_idx;
		public string nickname;
		public int game_server_idx;
		public int peer_index;		
		public int gamedb_idx;
	}
	[Packet( NPID_R2G.AccountLoginNotify )]
	public class NR2G_AccountLoginNotify
	{
		public long account_idx;
		public int game_server_idx;
		public int peer_index;
		public string nickname;
		public int gamedb_idx;
	}

	//------------------------------------------------------------------------
	[Packet( NPID_G2R.AccountLogoutNotify )]
	public class NG2R_AccountLogoutNotify
	{
		public long account_idx;
	}
	[Packet( NPID_R2G.AccountLogoutNotify )]
	public class NR2G_AccountLogoutNotify
	{
		public long account_idx;
	}

	//------------------------------------------------------------------------
	[Packet( NPID_G2R.PlayerLoginNotify )]
	public class NG2R_PlayerLoginNotify
	{
		public long account_idx;
		public int player_idx;
		public string nickname;
	}
	[Packet( NPID_R2G.PlayerLoginNotify )]
	public class NR2G_PlayerLoginNotify
	{
		public long account_idx;
		public int player_idx;
		public string nickname;
	}
}
