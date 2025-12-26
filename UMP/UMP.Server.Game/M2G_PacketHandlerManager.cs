//////////////////////////////////////////////////////////////////////////
//
// M2G_PacketHandlerManager
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

namespace UMP.Server.Game
{
	public class M2G_PacketHandlerManagerStandard : M2G_PacketHandlerManager<MasterConnector>
	{
		public M2G_PacketHandlerManagerStandard( GameServerApplication application )
			: base( application, typeof( NPID_M2S ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class M2G_PacketHandlerManager<ST> : NM2S_PacketHandlerManager<ST> where ST : MasterConnector
	{
		protected GameServerApplication mApplication = null;
		public GameServerApplication Application { get { return mApplication; } }

		public M2G_PacketHandlerManager( GameServerApplication application, Type packet_id_type )
			: base( application, packet_id_type )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------
		protected override void NM2S_CMD_rootHandler( ST session, object _packet )
		{
			NM2S_CMD_root packet = _packet as NM2S_CMD_root;

			if( packet.sub_command == MasterSubCommandName.root_1_shutdown || packet.sub_command == MasterSubCommandName.root_q_shutdownquit )
			{
				mApplication.Shutdown( false );

				AppConfig appconfig = AppConfig.Instance;

				foreach( ClientGamePeer peer in mApplication.ClientPeerManager )
				{
					string time_text = appconfig.Shutdown_Data.ToTimeText( peer.CurrLanguage );
					string text_key = appconfig.Shutdown_Data.TextKey;
					string url = appconfig.Shutdown_Data.URL;

					AppConfig.Shutdown.PerApp per_app = appconfig.Shutdown_Data.Find_PerApp( peer.session_data.application_identifier );
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
			}
		}

		//------------------------------------------------------------------------
		protected override void NM2S_CMD_serverHandler( ST session, object _packet )
		{
			NM2S_CMD_server packet = _packet as NM2S_CMD_server;

			if( packet.sub_command == MasterSubCommandName.server_maintenance )
			{
				base.NM2S_CMD_serverHandler( session, _packet );
			}
			else if( packet.sub_command == MasterSubCommandName.server_clientlimit )
			{
				mApplication.ClientLimitCount = packet.int_value;

				session.SendCommandResponse( packet, mApplication.ClientLimitCount.ToString() );
			}				
		}
	}
}
