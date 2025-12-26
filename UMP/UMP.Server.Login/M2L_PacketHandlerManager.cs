//////////////////////////////////////////////////////////////////////////
//
// M2L_PacketHandlerManager
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
using UMF.Net;

namespace UMP.Server.Login
{
	public class M2L_PacketHandlerManagerStandard : M2L_PacketHandlerManager<MasterConnector>
	{
		public M2L_PacketHandlerManagerStandard( LoginServerApplication application )
			: base( application, typeof( NPID_M2S ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class M2L_PacketHandlerManager<ST> : NM2S_PacketHandlerManager<ST> where ST : MasterConnector
	{
		protected LoginServerApplication mApplication = null;
		public LoginServerApplication Application
		{
			get { return mApplication; }
		}

		public M2L_PacketHandlerManager( LoginServerApplication application, Type packet_id_type ) 
			: base( application, packet_id_type )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------
		protected override void NM2S_UpdateServerInfoToLoginHandler( ST session, object _packet )
		{
			NM2S_UpdateServerInfoToLogin packet = _packet as NM2S_UpdateServerInfoToLogin;

			ServerConnectionManager.Instance.UpdateServerInfo( packet.server_info_list );
		}

		//------------------------------------------------------------------------
		protected override void NM2S_UpdateServerStatusToLoginHandler( ST session, object _packet )
		{
			NM2S_UpdateServerStatusToLogin packet = _packet as NM2S_UpdateServerStatusToLogin;

			ServerConnectionManager.Instance.UpdateStatus( packet.world_idn, packet.server_guid, packet.peer_count, packet.connection_key );
		}

		//------------------------------------------------------------------------
		protected override void NM2S_CMD_rootHandler( ST session, object _packet )
		{
			NM2S_CMD_root packet = _packet as NM2S_CMD_root;

			if( packet.sub_command == MasterSubCommandName.root_1_shutdown || packet.sub_command == MasterSubCommandName.root_q_shutdownquit )
			{
				mApplication.Shutdown( false );

				AppConfig appconfig = AppConfig.Instance;

				foreach( ClientLoginPeer peer in mApplication.ClientPeerManager )
				{
					string time_text = appconfig.Shutdown_Data.ToTimeText( peer.CurrLanguage );
					string text_key = appconfig.Shutdown_Data.TextKey;
					string url = appconfig.Shutdown_Data.URL;

					AppConfig.Shutdown.PerApp per_app = appconfig.Shutdown_Data.Find_PerApp( peer.application_identifier );
					if( per_app != null )
					{
						text_key = per_app.TextKey;
						url = per_app.URL;
					}

					if( string.IsNullOrEmpty( text_key ) )
						text_key = SystemTextKey.SHUTDOWN;

					peer.Disconnect( (int)eDisconnectErrorCode.ServerMaintenance, peer.GetText( text_key, time_text, url ) );
				}

				mApplication.RefreshTitleString = true;

				session.SendCommandResponse( packet, mApplication.IsShutdown.ToString() );

				if( packet.int_value == 1 )
					mApplication.StopLoop();
			}
		}
	}
}
