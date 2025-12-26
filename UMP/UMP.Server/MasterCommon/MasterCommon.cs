//////////////////////////////////////////////////////////////////////////
//
// MasterConnector
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

namespace UMP.Server
{
	//------------------------------------------------------------------------
	public class MasterCommandBase
	{
		public string req_id;
		public string sub_command;

		public void Copy( MasterCommandBase cmd_base )
		{
			this.req_id = cmd_base.req_id;
			this.sub_command = cmd_base.sub_command;
		}
	}

	//------------------------------------------------------------------------
	public class MasterSubCommandName
	{
		public const string root_1_shutdown = "1_shutdown";
		public const string root_q_shutdownquit = "q_shutdownquit";

		public const string server_maintenance = "maintenance";
		public const string server_clientlimit = "clientlimit";

		public const string reload_all = "all";
		public const string reload_data = "data";

		public const string daemon_dump_set = "set";
		public const string daemon_dump_exec = "exec";
	}
}