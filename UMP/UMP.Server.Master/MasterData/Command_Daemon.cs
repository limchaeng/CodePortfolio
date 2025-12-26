//////////////////////////////////////////////////////////////////////////
//
// Command_Daemon
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
using UMP.CSCommon;

namespace UMP.Server.Master
{
	public class Command_Daemon : MasterSubCommand
	{
		public Command_Daemon( MasterCommand master, MasterServerApplication application ) : base( master, application )
		{
			Command daemonCommand = mMasterCommand.AddRootCommand( "daemon", eCommandAuthority.Server );
			{
				daemonCommand.AddParamCommand( "start", new ParamCommand3<int, int, eServerType>( ( string sub_command, int world_idn, int daemon_idx, eServerType server_type ) =>
				{
					DaemonMasterPeer daemon = mApplication.DaemonPeerManager.FindDaemon( world_idn, daemon_idx );
					if( daemon == null )
					{
						mMasterCommand.SendResponseMessage( $"can't find daemon : {world_idn} / {daemon_idx}" );
					}
					else
					{
						NM2D_StartProcess _NM2D_StartProcess = new NM2D_StartProcess();
						_NM2D_StartProcess.processes = new List<eServerType>();
						_NM2D_StartProcess.processes.Add( server_type );

						daemon.SendPacket( _NM2D_StartProcess );
					}

				} ) );

				daemonCommand.AddParamCommand( "processtime", new ParamCommand3<int, int, int>( ( string sub_command, int world_idn, int timeout, int check_count ) =>
				{
					if( timeout > 0 && check_count > 0 )
					{
						NM2D_SetDaemonConfig _NM2D_SetDaemonConfig = new NM2D_SetDaemonConfig();
						_NM2D_SetDaemonConfig.process_check_timeout = timeout;
						_NM2D_SetDaemonConfig.process_check_count = check_count;
						mApplication.DaemonPeerManager.BroadcastPacketTo( _NM2D_SetDaemonConfig, world_idn );
					}
					else
					{
						mMasterCommand.SendResponseMessage( "Need timeout|check_count > 0" );
					}
				} ) );
				daemonCommand.AddCommand( "processcheck", ( string sub_command ) =>
				{
					NM2D_ProcessCheck _NM2D_ProcessCheck = new NM2D_ProcessCheck();
					mApplication.DaemonPeerManager.BroadcastPacket( _NM2D_ProcessCheck );
				} );

				Command dump_command = daemonCommand.AddCommand( "dump" );
				{
					dump_command.AddParamCommand( MasterSubCommandName.daemon_dump_set, new ParamCommand3<int, int, eDaemonDumpSetType>( daemon_dump_set_exec ) );
					dump_command.AddParamCommand( MasterSubCommandName.daemon_dump_exec, new ParamCommand3<int, int, eDaemonDumpSetType>( daemon_dump_set_exec ) );
				}
			}
		}

		//------------------------------------------------------------------------
		void daemon_dump_set_exec( string sub_command, int world_idn, int daemon_idx, eDaemonDumpSetType set_type )
		{
			NM2D_ProcessDump _NM2D_ProcessDump = new NM2D_ProcessDump();
			_NM2D_ProcessDump.sub_command = sub_command;
			_NM2D_ProcessDump.set_type = set_type;

			if( daemon_idx < 0 )
			{
				mApplication.DaemonPeerManager.BroadcastPacketTo( _NM2D_ProcessDump, world_idn );
			}
			else
			{
				DaemonMasterPeer daemon = mApplication.DaemonPeerManager.FindDaemon( world_idn, daemon_idx );
				if( daemon != null )
				{
					daemon.SendPacket( _NM2D_ProcessDump );
				}
				else
				{
					mMasterCommand.SendResponseMessage( string.Format( "Not found Daemon W:{0} IDX:{1}", world_idn, daemon_idx ) );
				}
			}
		}

	}
}
