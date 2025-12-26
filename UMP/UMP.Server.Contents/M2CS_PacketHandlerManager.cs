//////////////////////////////////////////////////////////////////////////
//
// M2CS_PacketHandlerManager
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

namespace UMP.Server.Contents
{
	public class M2CS_PacketHandlerManagerStandard : M2CS_PacketHandlerManager<MasterConnector>
	{
		public M2CS_PacketHandlerManagerStandard( ContentsServerApplication application )
			: base( application, typeof( NPID_M2S ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class M2CS_PacketHandlerManager<ST> : NM2S_PacketHandlerManager<ST> where ST : MasterConnector
	{
		protected ContentsServerApplication mApplication = null;
		public ContentsServerApplication Application
		{
			get { return mApplication; }
		}

		public M2CS_PacketHandlerManager( ContentsServerApplication application, Type packet_id_type )
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

				foreach( ClientContentsPeer peer in mApplication.ClientPeerManager )
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
			}
		}
	}
}
