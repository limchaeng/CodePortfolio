//////////////////////////////////////////////////////////////////////////
//
// AppVerifyModuleDB
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
#if UMSERVER

using System;
using System.Collections.Generic;
using UMF.Database;

namespace UMP.Module.AppVerifyModule.DB
{
	//------------------------------------------------------------------------
	// DO NOT CHANGE ENUM VALUE:USED DB
	public enum eSP_APP_VERIFY_TYPE : byte
	{
		KeySign = 1,
		MD5 = 2,
		CRC = 3,
		HackFile = 4,
		Installer = 5,
		// iOS
		iOSJailbreak = 51,
		iOSLibCheck = 52,
	}

	//------------------------------------------------------------------------
	// DO NOT CHANGE ENUM VALUE:USED DB
	public enum eSP_APP_VERIFY_ValidationKind : byte
	{
		Unknown = 0,
		IsValid = 1,
		IsInvalid = 2,
		IsWarning = 3,
	}

	//------------------------------------------------------------------------
	public class SP_APP_VERIFY_USE
	{
		public eSP_APP_VERIFY_TYPE verify_type;
		public bool all_warning;
		public bool fixed_valid_verify;
	}

	//------------------------------------------------------------------------
	public class SP_APP_VERIFY_DATA
	{
		public int idx;
		public eSP_APP_VERIFY_TYPE verify_type;
		public string verify_code;
		public eSP_APP_VERIFY_ValidationKind validation_kind;

		[ProcedureValue( eProcedureValueType.None )]
		public bool runtime_unknown_added;

		public bool IsEqual( string v_data )
		{
			return ( v_data.ToLower().Equals( verify_code.ToLower() ) );
		}

		public eAppVerifyRequestTypeFlag ToFlag()
		{
			switch( verify_type )
			{
				case eSP_APP_VERIFY_TYPE.KeySign:
					return eAppVerifyRequestTypeFlag.KeySign;

				case eSP_APP_VERIFY_TYPE.MD5:
					return eAppVerifyRequestTypeFlag.MD5;

				case eSP_APP_VERIFY_TYPE.CRC:
					return eAppVerifyRequestTypeFlag.CRC;

				case eSP_APP_VERIFY_TYPE.HackFile:
					return eAppVerifyRequestTypeFlag.HackFile;

				case eSP_APP_VERIFY_TYPE.Installer:
					return eAppVerifyRequestTypeFlag.Installer;

				case eSP_APP_VERIFY_TYPE.iOSJailbreak:
					return eAppVerifyRequestTypeFlag.IOSJB;

				case eSP_APP_VERIFY_TYPE.iOSLibCheck:
					return eAppVerifyRequestTypeFlag.IOSLIB;
			}

			return eAppVerifyRequestTypeFlag.None;
		}
	}

	public class SP_APP_VERIFY_DELETED
	{
		public int idx;
	}

	//------------------------------------------------------------------------		
	[Procedure( "SP_AppVerifyCode_Get", eProcedureExecute.Reader )]
	public class SP_AppVerifyCode_Get
	{
		public int world_idn;
		public bool all_get;
		public DateTime last_get_time;
		public string app_version;
		public bool has_unknown;
		public tv_tinystr_list new_unknown_list;
	}
	public class SP_AppVerifyCode_Get_ACK : PROCEDURE_READ_BASE
	{
		public DateTime last_db_time;
		public List<SP_APP_VERIFY_USE> used_list;
		public List<SP_APP_VERIFY_DATA> updated_code_list;
		public List<SP_APP_VERIFY_DELETED> deleted_list;
	}
}

#endif
