//////////////////////////////////////////////////////////////////////////
//
// WorldModuleServer
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
#if UMSERVER

using System;
using UMF.Net;
using UMF.Net.Module;
using UMP.CSCommon.Packet;
using UMF.Core;
using System.Collections.Generic;
using System.Linq;
using UMP.Server;
using UMP.Server.Login;
using UMP.CSCommon;
using UMF.Core.I18N;

namespace UMP.Module.WorldModule
{
	//------------------------------------------------------------------------	
	public class WorldModuleServer : ModuleNetPeer
	{
		public override string ModuleName => WorldModuleCommon.MODULE_NAME;
		protected override short ProtocolVersion => WorldModuleCommon.ProtocolVersion;
		public override Type SendPacketIdType => typeof( WorldModuleCommon.NPID_L2C );
		public sealed override Type NSendPacketIdType => typeof( WorldModuleCommon.NPID_L2C );
		public override Type RecvPacketIdType => typeof( WorldModuleCommon.NPID_C2L );
		public sealed override Type NRecvPacketIdType => typeof( WorldModuleCommon.NPID_C2L );

		protected LoginServerApplication mApplication = null;

		//------------------------------------------------------------------------
		public WorldModuleServer(LoginServerApplication login_application, PeerManagerBase peer_manager )
			: base( peer_manager )
		{
			mApplication = login_application;
			WorldModuleConfig.MakeInstance();

			//mPeerManager.AddPacketSendInterruptor<NL2C_LoginAck>( NL2C_LoginAckSendInterrupt );

			AddPacketHandler<NC2L_GetWorldList>( NC2L_GetWorldListHandler );			
		}

		//------------------------------------------------------------------------
		protected CS_WorldContainer GetWorldList(string curr_localize)
		{
			CS_WorldContainer container = new CS_WorldContainer();
			container.auto_refresh_timeout = WorldModuleConfig.Instance.RefreshTimeout;
			container.world_list = null;

			List<WorldModuleConfig.Data> world_config_list = WorldModuleConfig.Instance.Data_List.OrderByDescending( a => a.IsNew )
				.ThenByDescending( a => a.IsRecommend )
				.ThenBy( a => a.Order )
				.ToList();

			if( world_config_list != null && world_config_list.Count > 0 )
			{
				foreach( WorldModuleConfig.Data config in world_config_list )
				{
					CS_WorldData world_data = new CS_WorldData();
					world_data.world_idn = config.WorldIDN;
					world_data.world_name = I18NTextMultiLanguage.Instance.GetText( curr_localize, config.NameKey );
					world_data.status_text = "";
					world_data.flags = eWorldDataFlags.None;
					if( config.IsNew )
						world_data.flags |= eWorldDataFlags.New;
					if( config.IsRecommend )
						world_data.flags |= eWorldDataFlags.Recommend;

					NP_ServerInfoData free_server = ServerConnectionManager.Instance.FindFreeServer( eServerType.Game, config.WorldIDN );
					if( free_server == null )
					{
						world_data.status_text = I18NTextMultiLanguage.Instance.GetText( curr_localize, config.TextData.Maintenance );
						world_data.flags |= eWorldDataFlags.Maintenance;
					}
					else
					{
						world_data.connectionKey = free_server.connection_key;
						world_data.host_name = free_server.notify_host_name;
						world_data.port = free_server.notify_port;

						int user_count = ServerConnectionManager.Instance.GetPeerCountPerWorld( config.WorldIDN );
						if( user_count <= config.SmoothCountMax )
						{
							world_data.flags |= eWorldDataFlags.StatusSmooth;
							world_data.status_text = I18NTextMultiLanguage.Instance.GetText( curr_localize, config.TextData.Smooth );
						}
						else if( user_count <= config.NormalCountMax )
						{
							world_data.flags |= eWorldDataFlags.StatusNormal;
							world_data.status_text = I18NTextMultiLanguage.Instance.GetText( curr_localize, config.TextData.Normal );
						}
						else
						{
							world_data.flags |= eWorldDataFlags.StatusBusy;
							world_data.status_text = I18NTextMultiLanguage.Instance.GetText( curr_localize, config.TextData.Busy );
						}
					}

					if( string.IsNullOrEmpty( config.FixedStateKey ) == false )
						world_data.status_text = I18NTextMultiLanguage.Instance.GetText( curr_localize, config.FixedStateKey );

					if( container.world_list == null )
						container.world_list = new List<CS_WorldData>();

					container.world_list.Add( world_data );
				}
			}

			return container;
		}

		//------------------------------------------------------------------------		
		[PacketSendInterruptHandler( PacketType = typeof( NL2C_LoginAck ) )]
		protected virtual void NL2C_LoginAckSendInterrupt( object _packet, Session session )
		{
			NL2C_LoginAck packet = _packet as NL2C_LoginAck;

			if( packet.fast_connection_data == null && WorldModuleConfig.Instance.Enabled )
			{
				CS_WorldContainer world_container = GetWorldList( session.CurrLanguage );

				world_container = new CS_WorldContainer();
				world_container.auto_refresh_timeout = 10;
				world_container.world_list = null;

				packet.AddExpandPacketData( world_container, session );
			}
		}

		//------------------------------------------------------------------------
		protected virtual void NC2L_GetWorldListHandler( Peer session, NC2L_GetWorldList packet )
		{
			NL2C_GetWorldListAck _NL2C_GetWorldListAck = new NL2C_GetWorldListAck();
			_NL2C_GetWorldListAck.update_container = GetWorldList( session.CurrLanguage );

			SendPacket( _NL2C_GetWorldListAck, session );
		}
	}
}

#endif