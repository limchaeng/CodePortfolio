//////////////////////////////////////////////////////////////////////////
//
// Command_Reload
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
using System.Collections.Generic;
using UMF.Server;

namespace UMP.Server.Master
{
	public class Command_Reload : MasterSubCommand
	{
		public Command_Reload( MasterCommand master, MasterServerApplication application ) : base( master, application )
		{
			Command reloadCommand = mMasterCommand.AddRootCommand( "reload", eCommandAuthority.Reload );
			{
				reloadCommand.AddParamCommand( MasterSubCommandName.reload_all, new ParamCommand1<int>( reload_all ) );
				reloadCommand.AddParamCommand( MasterSubCommandName.reload_data, new ParamCommand2<int, string>( reload_data ) );
			}
		}

		//------------------------------------------------------------------------
		void reload_all( string command, int world_idn )
		{
			NM2S_CMD_reload _NM2S_CMD_reload = mMasterCommand.MakePacket<NM2S_CMD_reload>( command );
			_NM2S_CMD_reload.reload_id_list = null;
			mApplication.ServerPeerManager.SendToServers( _NM2S_CMD_reload, world_idn );
			mMasterCommand.RequestCount = mApplication.ServerPeerManager.MultipleSendCount;
		}

		void reload_data( string command, int world_idn, string parm )
		{
			string[] ids = parm.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

			NM2S_CMD_reload _NM2S_CMD_reload = mMasterCommand.MakePacket<NM2S_CMD_reload>( command );
			_NM2S_CMD_reload.reload_id_list = new List<string>();
			foreach( string id in ids )
			{
				_NM2S_CMD_reload.reload_id_list.Add( id );
			}

			mApplication.ServerPeerManager.SendToServers( _NM2S_CMD_reload, world_idn );
			mMasterCommand.RequestCount = mApplication.ServerPeerManager.MultipleSendCount;
		}

	}
}
