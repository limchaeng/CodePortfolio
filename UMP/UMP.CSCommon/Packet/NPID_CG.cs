//////////////////////////////////////////////////////////////////////////
//
// NPID_CG
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
using System;
using System.Collections.Generic;

namespace UMP.CSCommon.Packet
{
	//------------------------------------------------------------------------	
	[PacketVersion( Version = NPacketVersion.Version )]
	public enum NPID_C2G : short
	{
		__BEGIN = short.MinValue + 1,
		//
		AccountLogin,
	}

	//------------------------------------------------------------------------
	[PacketVersion( Version = NPacketVersion.Version )]
	public enum NPID_G2C : short
	{
		__BEGIN = short.MinValue + 1,
		//
		AccountFastLoginAck,
		AccountLoginAck,
		LocalizeTextFastUpdateNotify,
		TBLDataUpdateNotify,
	}

	//////////////////////////////////////////////////////////////////////////
	///
	//------------------------------------------------------------------------	
	public class _P_FastLoginData
	{
		public long account_idx;
		public int prev_socket_peerindex;
		public byte fast_login_verify_key;
	}
	[Packet(NPID_C2G.AccountLogin)]
	public class NC2G_AccountLogin : ExpandPacketBase
	{
		public string localize;
		public short app_store_type;
		public CS_AccountDeviceData device_data;
		public CS_AuthPlatformData auth_data;
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public _P_FastLoginData fast_login_data;
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public List<_P_TBLClientVersionData> tbl_version_list;
	}

	//------------------------------------------------------------------------
	[System.Flags]
	public enum eAccountLoginHasFlags : int
	{
		None				= 0x0000,
	}

	//------------------------------------------------------------------------
	[Packet(NPID_G2C.AccountFastLoginAck)]
	public class NG2C_AccountFastLoginAck : ExpandPacketBase
	{
		public DateTime server_time;
		public byte fast_login_verify_key;
		public int socket_peer_index;
		public eAccountLoginHasFlags account_login_has_flags;
	}

	//------------------------------------------------------------------------
	[Packet(NPID_G2C.AccountLoginAck)]
	public class NG2C_AccountLoginAck : ExpandPacketBase
	{
		public long account_idx;
		public string account_code;
		public string nickname;
		public DateTime account_created_time;
		public DateTime account_withdrawal_process_time;
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public List<CS_AuthPlatformRegistData> platform_regist_list;
		public byte fast_login_verify_key;
		public int socket_peer_index;
		public long gameserver_guid;
		public Version server_version;
		public DateTime server_time;		
		public CSSecurity.eSecurityFlags account_security_flags;
		public eAccountLoginHasFlags account_login_has_flags;

		// config
		public CS_AppConfig app_config;
		public CS_LocalizationConfig localization_config;
	}

	//------------------------------------------------------------------------
	public class _P_LocalizeTextFastUpdateNotifyData
	{
		public string key;
		public List<string> string_list;
	}
	[Packet( NPID_G2C.LocalizeTextFastUpdateNotify )]
	public class NG2C_LocalizeTextFastUpdateNotify
	{
		public List<_P_LocalizeTextFastUpdateNotifyData> update_list;
	}

	//------------------------------------------------------------------------
	public class _P_TBLClientVersionData
	{
		public string tbl_id;
		public int version;
	}
	public class _P_TBLUpdateData
	{
		public bool is_eof;
		public string tbl_id;
		public string tbl_base64;
	}
	//------------------------------------------------------------------------	
	[Packet( NPID_G2C.TBLDataUpdateNotify )]
	public class NG2C_TBLDataUpdateNotify
	{
		public byte total_count;
		public bool is_endoflist;
		public _P_TBLUpdateData tbl_data;
	}
}
