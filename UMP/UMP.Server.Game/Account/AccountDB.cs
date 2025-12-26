//////////////////////////////////////////////////////////////////////////
//
// AccountDB
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
using UMF.Database;
using System.Collections.Generic;
using UMP.CSCommon;

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------
	public enum eAccountDBErrorCode
	{
		AccountAuthIDExist		= 50011,
		AccountNotFound			= 50012,
		AccountLoginRecordError	= 50013,
	}

	//------------------------------------------------------------------------
	[Procedure( "SP_AccountAuth", eProcedureExecute.Reader )]
	public class SP_AccountAuth
	{
		public short platform_type;
		public string platform_id;
		public string platform_key;
		public string platform_username;
		public string platform_extra;
		public short platform_migration_type;
		public string platform_migration_id;
		public string platform_migration_key;
		public bool is_auto_regist;
		public bool check_exist_throw;
		public DateTime server_time;
	}
	public class DB_AuthPlatformData
	{
		public short platform_type;
		public string platform_id;
		public string platform_name;
		public string platform_extra;
		public DateTime regist_time;

		public CS_AuthPlatformRegistData ToCS()
		{
			CS_AuthPlatformRegistData data = new CS_AuthPlatformRegistData();
			data.platform_type = platform_type;
			data.platform_name = platform_name;
			data.platform_extra = platform_extra;
			data.regist_time = regist_time;

			return data;
		}
	}
	public class SP_AccountAuth_ACK : PROCEDURE_READ_BASE
	{
		public long account_idx;
		public bool is_new_account;
		public int game_db_idx;
		[ProcedureValue( eProcedureValueType.SerializeNullable )]
		public DateTime block_expire_time;
		[ProcedureValue( eProcedureValueType.SerializeNullable )]
		public DateTime withdrawal_process_time;
		public List<DB_AuthPlatformData> registered_platform_list;
	}

	//------------------------------------------------------------------------	
	[Procedure( "SP_AuthPlatformRegist", eProcedureExecute.Reader )]
	public class SP_AuthPlatformRegist
	{
		public long account_idx;
		public short platform_type;
		public string platform_id;
		public string platform_key;
		public string platform_username;
		public string platform_extra;
		public short platform_migration_type;
		public string platform_migration_id;
		public string platform_migration_key;
		public DateTime server_time;
	}
	public enum eAccountPlatformRegistResult
	{
		AlreadyRegistered	= -1,
		Success				= 0,
		MigrationSuccess	= 1,
	}
	public class SP_AccountPlatformRegist_ACK : PROCEDURE_READ_BASE
	{
		public eAccountPlatformRegistResult result;
		public long exists_account_idx;
	}

	//------------------------------------------------------------------------
	[Procedure("SP_AuthPlatformUnregist", eProcedureExecute.Reader)]
	public class SP_AuthPlatformUnregist
	{
		public long account_idx;
		public short platform_type;
		public string platform_id;
		public DateTime server_time;
	}

	//------------------------------------------------------------------------
	[Procedure( "SP_AccountWithdrawalDirect", eProcedureExecute.Reader )]
	public class SP_AccountWithdrawalDirect
	{
		public long account_idx;
		public DateTime server_time;
	}
	
	//------------------------------------------------------------------------
	[Procedure( "SP_AccountGameLogin", eProcedureExecute.Reader )]
	public class SP_AccountGameLogin
	{
		public long account_idx;
		public string account_code;
		public int game_db_idx;
		public string application_identifier;
		public string device_type;
		public string device_id;
		public string device_location;
		public string device_os_version;
		public string device_language;
		public string device_advertising_id;
		public string device_package_id;
		public string client_version;
		public DateTime server_time;
		public bool is_new_account;
		public bool use_multiple_player;
	}

	//------------------------------------------------------------------------
	public class DB_AccountPlayerData
	{
		public int player_idx;
		public string nickname;
		[ProcedureValue( eProcedureValueType.SerializeNullable )]
		public DateTime last_logout_time;
		public long total_playing_time;
	}

	//------------------------------------------------------------------------	
	public class SP_AccountGameLogin_ACK : PROCEDURE_READ_BASE
	{
		public string nickname;
		public CS_AgreementData.eTypeFlag agreement_flags;
		public CSSecurity.eSecurityFlags security_flags;
		public int special_flags;
		public DateTime created_time;
		public List<DB_AccountPlayerData> player_list;
	}

	//------------------------------------------------------------------------
	[Procedure( "SP_Common_AccountLogin", eProcedureExecute.Reader )]
	public class SP_Common_AccountLogin
	{
		public long account_idx;
		public int game_db_idx;
	}
	public class SP_Common_AccountLogin_ACK : PROCEDURE_READ_BASE
	{
		// TODO
	}
}
