//////////////////////////////////////////////////////////////////////////
//
// NPID_DM
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
using UMP.CSCommon;
using System.Collections.Generic;

namespace UMP.Server
{
	//------------------------------------------------------------------------	
	public enum NPID_D2M : short
	{
		__BEGIN = short.MinValue + 1,
		//
		DaemonStatup,
		ProcessCheckAck,
		ProcessDumpAck,
	}

	//------------------------------------------------------------------------
	public enum NPID_M2D : short
	{
		__BEGIN = short.MinValue + 1,
		//
		ProcessCheck,
		ProcessDump,
		StartProcess,
		SetDaemonConfig,
		CMD_root,
		CMD_server,
	}

	//////////////////////////////////////////////////////////////////////////
	///
	//------------------------------------------------------------------------
	[Packet(NPID_D2M.DaemonStatup)]
	public class ND2M_DaemonStatup
	{
		
	}

	//------------------------------------------------------------------------
	[Packet(NPID_M2D.ProcessCheck, eCoreLogType.Important)]
	public class NM2D_ProcessCheck
	{

	}
	[Packet(NPID_D2M.ProcessCheckAck, eCoreLogType.Important)]
	public class ND2M_ProcessCheckAck
	{
		public string reuslt;	// TODO : data	
	}

	//------------------------------------------------------------------------
	public enum eDaemonDumpSetType
	{
		none = 0,
		mini = 1,
		full = 2,
	}
	[Packet( NPID_M2D.ProcessDump, eCoreLogType.Important )]
	public class NM2D_ProcessDump
	{
		public string sub_command;
		public eDaemonDumpSetType set_type;
	}
	[Packet( NPID_D2M.ProcessDumpAck, eCoreLogType.Important )]
	public class ND2M_ProcessDumpAck
	{
		public string result;
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public List<string> dump_file_names;
	}

	//------------------------------------------------------------------------
	[Packet( NPID_M2D.StartProcess )]
	public class NM2D_StartProcess
	{
		public List<eServerType> processes;
	}

	//------------------------------------------------------------------------
	[Packet( NPID_M2D.SetDaemonConfig )]
	public class NM2D_SetDaemonConfig
	{
		public int process_check_timeout;
		public int process_check_count;
	}

	//------------------------------------------------------------------------

	[Packet( NPID_M2D.CMD_root )]
	public class NM2D_CMD_root
	{
		public string sub_command;
		public int int_value;
	}

	//------------------------------------------------------------------------

	[Packet( NPID_M2D.CMD_server )]
	public class NM2D_CMD_server
	{
		public string sub_command;
		public int int_value;
	}
}
