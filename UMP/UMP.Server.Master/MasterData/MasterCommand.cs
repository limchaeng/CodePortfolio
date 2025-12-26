//////////////////////////////////////////////////////////////////////////
//
// MasterCommand
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
using System.Runtime.Serialization;
using UMF.Server;
using UMF.Core;
using UMP.CSCommon;

namespace UMP.Server.Master
{
	//------------------------------------------------------------------------
	public class MasterSubCommand
	{
		protected MasterCommand mMasterCommand;
		protected MasterServerApplication mApplication;

		public MasterSubCommand(MasterCommand master, MasterServerApplication application)
		{
			mMasterCommand = master;
			mApplication = application;
		}
	}

	//------------------------------------------------------------------------	
	public class MasterCommand : CommandManager
	{
		protected string mRequesterID = "";
		public string RequesterID { set { mRequesterID = value; } }
		public int RequestCount { get; set; } = 0;

		protected MasterServerApplication mApplication = null;

		public MasterCommand( MasterServerApplication application, bool console_output )
			: base( console_output )
		{
			mApplication = application;
			mApplication.CommandTool = this;
			mApplication.AddUpdater( Update );

			AddRootParamCommand( MasterSubCommandName.root_1_shutdown, new ParamCommand1<int>( root_1_shutdown ), eCommandAuthority.Server );
			AddRootCommand( MasterSubCommandName.root_q_shutdownquit, root_q_shutdownquit, eCommandAuthority.Server );
			Command shutdown_command = AddRootCommand( "2_reserve_shutdown", eCommandAuthority.Server );
			{
				shutdown_command.AddParamCommand( "set", new ParamCommand3<int, string, bool>( root_reserveshutdown_set ) );
				shutdown_command.AddParamCommand( "cancel", new ParamCommand1<int>( root_reserveshutdown_cancel ) );
				shutdown_command.AddCommand( "info", root_reserveshutdown_info );
			}

			Command masterCommand = AddRootCommand( "master", eCommandAuthority.View );
			{
				masterCommand.AddCommand( "server", ( string sub_command ) =>
				{
					SendResponseMessage( mApplication.ServerPeerManager.ShowInfo() );
				} );
				masterCommand.AddCommand( "daemon", ( string sub_command ) =>
				{
					SendResponseMessage( mApplication.DaemonPeerManager.ShowInfo() );
				} );
				masterCommand.AddCommand( "crashtest", ( string sub_command ) =>
				{
					throw new Exception( "Crash" );
				}, eCommandAuthority.Server );
			}

			new Command_Server( this, mApplication );
			new Command_Reload( this, mApplication );
			new Command_Daemon( this, mApplication );
		}

		//------------------------------------------------------------------------	
		[DataContract]
		public class ExportCommand
		{
			[DataMember]
			public string cmd;
			[DataMember]
			public List<ExportCommand> subs;
			[DataMember( Name = "params" )]
			public string parms;
			[DataMember( Name = "param_desc" )]
			public string param_desc;

		}
		public string ToJSON( string prefix, eCommandAuthority authrity )
		{
			if( root == null || root.SubCommands == null )
				return prefix;

			string response = prefix;
			try
			{
				ExportCommand exp_root = new ExportCommand();
				exp_root.cmd = "ROOT";
				exp_root.subs = ExportCmd( root.SubCommands, authrity );
				return string.Format( "{0}{1}", prefix, JsonUtil.EncodeJson<ExportCommand>( exp_root ) );
			}
			catch( System.Exception ex )
			{
				response += ex.ToString();
			}

			return response;
		}
		List<ExportCommand> ExportCmd( Dictionary<string, Command> subs, eCommandAuthority authority )
		{
			if( subs == null )
				return null;

			List<ExportCommand> list = null;
			foreach( KeyValuePair<string, Command> kvp in subs )
			{
				Command cmd = kvp.Value;

				if( cmd.AllowAuthority( authority ) == false )
					continue;

				ExportCommand data = new ExportCommand();
				data.cmd = kvp.Key;
				data.subs = ExportCmd( cmd.SubCommands, authority );
				if( data.subs == null && cmd.IsParamCommand )
				{
					data.parms = ( (ParamCommandBase)cmd ).GetParamString();
					data.param_desc = "";
					if( cmd.GetOnCommand != null )
					{
						Delegate[] invoke_list = cmd.GetOnCommand.GetInvocationList();
						if( invoke_list != null && invoke_list.Length > 0 && invoke_list[0].Method != null )
						{
							System.Reflection.ParameterInfo[] param_infos = invoke_list[0].Method.GetParameters();
							if( param_infos != null )
							{
								foreach( System.Reflection.ParameterInfo info in param_infos )
								{
									if( string.IsNullOrEmpty( data.param_desc ) )
										data.param_desc += info.Name;
									else
										data.param_desc += "," + info.Name;
								}
							}
						}
					}
				}

				if( list == null )
					list = new List<ExportCommand>();

				list.Add( data );
			}

			return list;
		}

		//------------------------------------------------------------------------	
		public void SendResponseMessage( string message )
		{
			string msg = string.Format( "<=\n{0}\n=>", message );

			if( string.IsNullOrEmpty( mRequesterID ) )
				Log.WriteImportant( msg );
// 			else
// 				ToolServer.Instance.CommandResponse( mRequesterID, msg );
		}

		//------------------------------------------------------------------------	
		public PacketType MakePacket<PacketType>(string sub_command) where PacketType : MasterCommandBase, new()
		{
			PacketType packet = new PacketType();
			packet.req_id = mRequesterID;
			packet.sub_command = sub_command;

			return packet;
		}

		//------------------------------------------------------------------------
		public override void Update()
		{
			base.Update();

			if( mShutdownReserved )
			{
				if( mShutdownReservedTime <= DateTime.Now )
				{
					mShutdownReserved = false;
					root_1_shutdown( MasterSubCommandName.root_1_shutdown, mShutdownReservedWorld );
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		/// COMMAND
		/// 
		//------------------------------------------------------------------------		
		void root_1_shutdown( string command, int world_idn )
		{
			if( world_idn == 0 )
				mApplication.Shutdown( false );

			// first : daemon shutdown
			NM2D_CMD_root _NM2D_CMD_root = new NM2D_CMD_root();
			_NM2D_CMD_root.sub_command = command;
			_NM2D_CMD_root.int_value = 0;
			mApplication.DaemonPeerManager.BroadcastPacketTo( _NM2D_CMD_root, world_idn );

			// second : server shutdown with maintenance info updated
			NM2S_CMD_reload _NM2S_CMD_reload = MakePacket<NM2S_CMD_reload>( MasterSubCommandName.reload_data );
			_NM2S_CMD_reload.reload_id_list = new List<string>();
			_NM2S_CMD_reload.reload_id_list.Add( "ServerText" );
			_NM2S_CMD_reload.reload_id_list.Add( "AppConfig" );
			mApplication.ServerPeerManager.SendToServers( _NM2S_CMD_reload, world_idn );

			NM2S_CMD_root _NM2S_CMD_root = MakePacket<NM2S_CMD_root>( command );
			_NM2S_CMD_root.int_value = 0;
			mApplication.ServerPeerManager.SendToServers( _NM2S_CMD_root, world_idn );
			RequestCount = mApplication.ServerPeerManager.MultipleSendCount;
		}

		void root_q_shutdownquit( string command )
		{
			mApplication.Shutdown( true );

			NM2D_CMD_root _NM2D_CMD_root = new NM2D_CMD_root();
			_NM2D_CMD_root.sub_command = command;
			_NM2D_CMD_root.int_value = 1;
			mApplication.DaemonPeerManager.BroadcastPacketTo( _NM2D_CMD_root, 0 );

			NM2S_CMD_root _NM2S_CMD_root = MakePacket<NM2S_CMD_root>( command );
			_NM2S_CMD_root.int_value = 1;
			mApplication.ServerPeerManager.SendToServers( _NM2S_CMD_root, 0 );
			RequestCount = mApplication.ServerPeerManager.MultipleSendCount;
		}

		bool mShutdownReserved = false;
		DateTime mShutdownReservedTime = DateTime.MinValue;
		int mShutdownReservedWorld = 0;
		bool mShutdownReservedChatNotice = false;
		void root_reserveshutdown_set( string command, int world_idn, string datetime_txt, bool chat_notice )
		{
			// datetim_txt = "2017-05-10_00:00:00"
			DateTime parsetime;
			if( DateTime.TryParse( datetime_txt.Replace( "_", " " ), out parsetime ) == false )
			{
				SendResponseMessage( "datetim_txt invalid(yyyy-MM-dd_hh:mm:ss)" );
				return;
			}

			if( parsetime <= DateTime.Now )
			{
				SendResponseMessage( string.Format( "Time invalid: {0} > now:{1}", parsetime, DateTime.Now ) );
				return;
			}

			mShutdownReserved = true;
			mShutdownReservedTime = parsetime;
			mShutdownReservedWorld = world_idn;
			mShutdownReservedChatNotice = chat_notice;

			SendResponseMessage( string.Format( "## SHUTDOWN RESERVE TIME:{0} CHATNOTICE:{1} WORLD:{2}", mShutdownReservedTime, mShutdownReservedChatNotice, mShutdownReservedWorld ) );
		}

		void root_reserveshutdown_cancel( string command, int world_idn )
		{
			mShutdownReserved = false;
			mShutdownReservedTime = DateTime.MinValue;
		}

		void root_reserveshutdown_info( string command )
		{
			SendResponseMessage( string.Format( "## SHUTDOWN RESERVE({0}) TIME:{1} CHATNOTICE:{2} WORLD:{3}", mShutdownReserved, mShutdownReservedTime, mShutdownReservedChatNotice, mShutdownReservedWorld ) );
		}
	}
}
