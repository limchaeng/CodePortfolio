//////////////////////////////////////////////////////////////////////////
//
// M2D_PacketHandlerManager
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
using UMF.Core;
using UMF.Net;

namespace UMP.Server.Daemon
{
	//------------------------------------------------------------------------
	public class M2D_PacketHandlerManagerStandard : M2D_PacketHandlerManager<DaemonMasterConnector>
	{
		public M2D_PacketHandlerManagerStandard( DaemonServerApplication application )
			: base( application, typeof( NPID_M2D ) )
		{
		}
		public M2D_PacketHandlerManagerStandard( DaemonServerApplication application, Type packet_id_type )
			: base( application, packet_id_type )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class M2D_PacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : MasterConnector
	{
		protected DaemonServerApplication mApplication = null;
		public DaemonServerApplication Application
		{
			get { return mApplication; }
		}
		public M2D_PacketHandlerManager( DaemonServerApplication application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_M2D ) )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NM2D_ProcessCheck ) )]
		protected virtual void NM2D_ProcessCheckHandler( ST session, object _packet)
		{
			NM2D_ProcessCheck packet = _packet as NM2D_ProcessCheck;

			string result = string.Format( "== DAEMON killdump:{0}\n", mApplication.m_ProcessKillDumpType );
			result += mApplication.GetProcessStatus();

			ND2M_ProcessCheckAck _ND2M_ProcessCheckAck = new ND2M_ProcessCheckAck();
			_ND2M_ProcessCheckAck.reuslt = result;
			session.SendPacket( _ND2M_ProcessCheckAck );			
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NM2D_ProcessDump ) )]
		protected virtual void NM2D_ProcessDumpHandler( ST session, object _packet)
		{
			NM2D_ProcessDump packet = _packet as NM2D_ProcessDump;

			ND2M_ProcessDumpAck _ND2M_ProcessDumpAck = new ND2M_ProcessDumpAck();
			_ND2M_ProcessDumpAck.result = "[OK]";
			_ND2M_ProcessDumpAck.dump_file_names = null;

			switch(packet.sub_command)
			{
				case MasterSubCommandName.daemon_dump_set:
					mApplication.m_ProcessKillDumpType = packet.set_type;
					break;

				case MasterSubCommandName.daemon_dump_exec:
					_ND2M_ProcessDumpAck.dump_file_names = mApplication.ExecuteProcessDumpAll( packet.set_type );
					break;
			}
			session.SendPacket( _ND2M_ProcessDumpAck );
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NM2D_StartProcess ) )]
		protected virtual void NM2D_StartProcessHandler( ST session, object _packet )
		{
			NM2D_StartProcess packet = _packet as NM2D_StartProcess;

			mApplication.ExecuteProcesses( packet.processes );
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NM2D_SetDaemonConfig ) )]
		protected virtual void NM2D_SetDaemonConfigHandler( ST session, object _packet )
		{
			NM2D_SetDaemonConfig packet = _packet as NM2D_SetDaemonConfig;

			mApplication.m_CheckProcessTimeoutSeconds = packet.process_check_timeout;
			mApplication.m_CheckProcessTimeoutCount = packet.process_check_count;
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NM2D_CMD_root ) )]
		protected virtual void NM2D_CMD_rootHandler( ST session, object _packet )
		{
			NM2D_CMD_root packet = _packet as NM2D_CMD_root;

			if( packet.sub_command == MasterSubCommandName.root_1_shutdown || packet.sub_command == MasterSubCommandName.root_q_shutdownquit )
			{
				mApplication.Shutdown( ( packet.int_value == 1 ? true : false ) );
			}
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NM2D_CMD_server ) )]
		protected virtual void NM2D_CMD_serverHandler( ST session, object _packet )
		{
			NM2D_CMD_server packet = _packet as NM2D_CMD_server;

			if( packet.sub_command == MasterSubCommandName.server_maintenance )
			{
				mApplication.IsMaintenance = ( packet.int_value == 1 ? true : false );
			}
		}
	}
}
