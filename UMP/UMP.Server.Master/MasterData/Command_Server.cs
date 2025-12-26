//////////////////////////////////////////////////////////////////////////
//
// Command_Server
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
	public class Command_Server : MasterSubCommand
	{
		public Command_Server( MasterCommand master, MasterServerApplication application ) : base( master, application )
		{
			Command server_command = mMasterCommand.AddRootCommand( "server", eCommandAuthority.Server );
			{
				server_command.AddParamCommand( MasterSubCommandName.server_maintenance, new ParamCommand2<int, bool>( server_maintenance ) );
				server_command.AddParamCommand( MasterSubCommandName.server_clientlimit, new ParamCommand2<int, int>( server_clientlimit ) );
			}
		}

		//------------------------------------------------------------------------
		void server_maintenance( string command, int world_idn, bool bSetMaintenance )
		{
			if( world_idn == 0 )
				mApplication.IsMaintenance = bSetMaintenance;

			if( bSetMaintenance )
			{
				NM2S_CMD_reload _NM2S_CMD_reload = mMasterCommand.MakePacket<NM2S_CMD_reload>( command );
				_NM2S_CMD_reload.reload_id_list = new List<string>();
				_NM2S_CMD_reload.reload_id_list.Add( "ServerText" );
				_NM2S_CMD_reload.reload_id_list.Add( "AppConfig" );
				mApplication.ServerPeerManager.SendToServers( _NM2S_CMD_reload, world_idn );
			}

			NM2S_CMD_server _NM2S_CMD_server = mMasterCommand.MakePacket<NM2S_CMD_server>( command );
			_NM2S_CMD_server.int_value = ( bSetMaintenance ? 1 : 0 );
			mApplication.ServerPeerManager.SendToServers( _NM2S_CMD_server, world_idn );
			mMasterCommand.RequestCount = mApplication.ServerPeerManager.MultipleSendCount;

			NM2D_CMD_server _NM2D_CMD_server = new NM2D_CMD_server();
			_NM2D_CMD_server.sub_command = command;
			_NM2D_CMD_server.int_value = ( bSetMaintenance ? 1 : 0 );
			mApplication.DaemonPeerManager.BroadcastPacketTo( _NM2D_CMD_server, world_idn );
		}

		void server_clientlimit( string command, int world_idn, int limit )
		{
			ClientLimit.Instance.SetDefaultClientLimit( limit );

			NM2S_CMD_server _NM2S_CMD_server = mMasterCommand.MakePacket<NM2S_CMD_server>( command );
			_NM2S_CMD_server.int_value = ClientLimit.Instance.DefaultLimit;
			mApplication.ServerPeerManager.SendToServers( _NM2S_CMD_server, world_idn );
			mMasterCommand.RequestCount = mApplication.ServerPeerManager.MultipleSendCount;
		}
	}
}
