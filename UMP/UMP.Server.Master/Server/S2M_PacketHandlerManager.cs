//////////////////////////////////////////////////////////////////////////
//
// S2M_PacketHandlerManager
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
using System.Text;
using System.Collections.Generic;
using UMP.CSCommon;
using UMF.Core;
using UMF.Net;

namespace UMP.Server.Master
{
	//------------------------------------------------------------------------
	public class S2M_PacketHandlerManagerStandard : S2M_PacketHandlerManager<ServerMasterPeer>
	{
		public S2M_PacketHandlerManagerStandard( MasterServerApplication application )
			: this( application, typeof( NPID_S2M ) )
		{
		}
		public S2M_PacketHandlerManagerStandard( MasterServerApplication application, Type packet_id_type )
			: base( application, packet_id_type )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class S2M_PacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : ServerMasterPeer
	{
		protected MasterServerApplication mApplication = null;
		public MasterServerApplication Application
		{
			get { return mApplication; }
		}

		public S2M_PacketHandlerManager( MasterServerApplication  application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_S2M ) )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NS2M_MasterCommandResponse ) )]
		protected virtual void NS2M_MasterCommandResponseHandler( ST session, object _packet )
		{
			NS2M_MasterCommandResponse packet = _packet as NS2M_MasterCommandResponse;

			string req_id = packet.req_id;
			bool to_tool = ( string.IsNullOrEmpty( req_id ) == false );

			StringBuilder sb = new StringBuilder();

			string ip = "";
			if( session.Socket != null )
				ip = ( (System.Net.IPEndPoint)session.Socket.RemoteEndPoint ).Address.ToString();

			sb.AppendLine( "<=" );
			if( session.ServerInfoData != null )
				sb.AppendLine( string.Format( "Server:{0} IP:{1}:{2} Command:{3}", session.ServerType, ip, session.ServerInfoData.notify_port, packet.command ) );
			else
				sb.AppendLine( string.Format( "Server:{0} IP:{1} Command:{2}", session.ServerType, ip, packet.command ) );
			sb.AppendLine( "-" );
			sb.AppendLine( packet.response );
			sb.AppendLine( "=>" );

// 			if( to_tool )
// 				ToolServer.Instance.CommandResponse( req_id, sb.ToString() );
// 			else
				Log.WriteImportant( sb.ToString() );
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NS2M_ServerConnectionInfo ) )]
		protected virtual void NS2M_ServerConnectionInfoHandler( ST session, object _packet )
		{
			NS2M_ServerConnectionInfo packet = _packet as NS2M_ServerConnectionInfo;

			session.ServerInfoData = packet.info_data;
			ServerManager.Instance.UpdateServeInfo( session, packet );

			NM2S_UpdateServerInfoToLogin _NM2S_UpdateGameServerInfoToLogin = new NM2S_UpdateServerInfoToLogin();
			_NM2S_UpdateGameServerInfoToLogin.server_info_list = new List<NP_ServerInfoData>() { packet.info_data };

			mApplication.ServerPeerManager.SendToServers( _NM2S_UpdateGameServerInfoToLogin, 0, eServerType.Login );
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NS2M_ServerConnectionKeyUpdate ) )]
		protected virtual void NS2M_ServerConnectionKeyUpdateHandler( ST session, object _packet )
		{
			NS2M_ServerConnectionKeyUpdate packet = _packet as NS2M_ServerConnectionKeyUpdate;

			ServerManager.ServerInfo server = ServerManager.Instance.FindServer( packet.server_type, session );
			if( server != null )
			{
				server.info_data.peer_count = packet.peer_count;
				server.info_data.connection_key = packet.update_connection_key;
			}

			NM2S_UpdateServerStatusToLogin _NM2S_UpdateServerStatusToLogin = new NM2S_UpdateServerStatusToLogin();
			_NM2S_UpdateServerStatusToLogin.server_type = packet.server_type;
			_NM2S_UpdateServerStatusToLogin.server_guid = session.GUID;
			_NM2S_UpdateServerStatusToLogin.world_idn = session.WorldIDN;
			_NM2S_UpdateServerStatusToLogin.peer_count = packet.peer_count;
			_NM2S_UpdateServerStatusToLogin.connection_key = packet.update_connection_key;

			mApplication.ServerPeerManager.SendToServers( _NM2S_UpdateServerStatusToLogin, session.WorldIDN, eServerType.Login );
		}
	}
}
