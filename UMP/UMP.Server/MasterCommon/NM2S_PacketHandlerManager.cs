//////////////////////////////////////////////////////////////////////////
//
// NM2S_PacketHandlerManager
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

namespace UMP.Server
{
	//------------------------------------------------------------------------	
	public class NM2S_PacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : MasterConnector
	{
		public NM2S_PacketHandlerManager( UMPServerApplication application )
			: this( application, typeof( NPID_M2S ) )
		{
		}
		public NM2S_PacketHandlerManager( UMPServerApplication application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_M2S ) )
		{
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NM2S_UpdateServerStatusToLogin ) )]
		protected virtual void NM2S_UpdateServerStatusToLoginHandler( ST session, object _packet ) { }

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NM2S_UpdateServerInfoToLogin ) )]
		protected virtual void NM2S_UpdateServerInfoToLoginHandler( ST session, object _packet ) { }

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NM2S_CMD_root ) )]
		protected virtual void NM2S_CMD_rootHandler( ST session, object _packet )
		{
			NM2S_CMD_root packet = _packet as NM2S_CMD_root;

			if( packet.sub_command == MasterSubCommandName.root_1_shutdown || packet.sub_command == MasterSubCommandName.root_q_shutdownquit )
			{
				mUMPApplication.Shutdown( ( packet.int_value == 1 ? true : false ) );
				session.SendCommandResponse( packet, mUMPApplication.IsShutdown.ToString() );
			}
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NM2S_CMD_reload ) )]
		protected virtual void NM2S_CMD_reloadHandler( ST session, object _packet )
		{
			NM2S_CMD_reload packet = _packet as NM2S_CMD_reload;

			string response = "";
			switch( packet.sub_command )
			{
				case MasterSubCommandName.reload_all:
					response = DataReloader.Instance.ReloadData( null );
					break;

				case MasterSubCommandName.reload_data:
					response = DataReloader.Instance.ReloadData( packet.reload_id_list );
					break;
			}

			mUMPApplication.ServerVersion = AppConfig.Instance.VersionInfo_Data.ServerVersion;
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NM2S_CMD_server ) )]
		protected virtual void NM2S_CMD_serverHandler( ST session, object _packet )
		{
			NM2S_CMD_server packet = _packet as NM2S_CMD_server;

			if( packet.sub_command == MasterSubCommandName.server_maintenance )
			{
				mUMPApplication.IsMaintenance = ( packet.int_value == 1 ? true : false );
				mUMPApplication.RefreshTitleString = true;

				session.SendCommandResponse( packet, mUMPApplication.IsMaintenance.ToString() );
			}
		}
	}
}
