//////////////////////////////////////////////////////////////////////////
//
// ServerMasterPeer
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

using UMF.Net;
using UMP.CSCommon;
using System.Collections.Generic;

namespace UMP.Server.Master
{
	//------------------------------------------------------------------------	
	public class ServerMasterPeer : MasterPeer 
	{
		public NP_ServerInfoData ServerInfoData { get; set; } = null;

		//------------------------------------------------------------------------
		protected override void OnVerified( object userInfo, PacketVerify verify )
		{
			base.OnVerified( userInfo, verify );

			{
				NM2S_CMD_server _NM2S_CMD_server = new NM2S_CMD_server();
				_NM2S_CMD_server.req_id = "";
				_NM2S_CMD_server.sub_command = MasterSubCommandName.server_clientlimit;
				_NM2S_CMD_server.int_value = ClientLimit.Instance.DefaultLimit;
				SendPacket( _NM2S_CMD_server );
			}

			{
				NM2S_CMD_server _NM2S_CMD_server = new NM2S_CMD_server();
				_NM2S_CMD_server.req_id = "";
				_NM2S_CMD_server.sub_command = MasterSubCommandName.server_maintenance;
				_NM2S_CMD_server.int_value = ( mUMPApplication.IsMaintenance ? 1 : 0 );
				SendPacket( _NM2S_CMD_server );
			}

			NM2S_CMD_reload _NM2S_CMD_reload = new NM2S_CMD_reload();
			_NM2S_CMD_reload.req_id = "";
			_NM2S_CMD_reload.sub_command = MasterSubCommandName.reload_all;
			_NM2S_CMD_reload.reload_id_list = null;
			SendPacket( _NM2S_CMD_reload );

			if( ServerType == eServerType.Login )
			{
				List<NP_ServerInfoData> list = ServerManager.Instance.GetServerInfoAll();
				if( list != null )
				{
					NM2S_UpdateServerInfoToLogin _NM2S_UpdateServerInfoToLogin = new NM2S_UpdateServerInfoToLogin();
					_NM2S_UpdateServerInfoToLogin.server_info_list = ServerManager.Instance.GetServerInfoAll();
					SendPacket( _NM2S_UpdateServerInfoToLogin );
				}
			}
		}

		//------------------------------------------------------------------------		
		public string GetInfo()
		{
			string ip = "null";
			if( m_Socket != null )
				ip = ( (System.Net.IPEndPoint)m_Socket.RemoteEndPoint ).Address.ToString();

			if( ServerInfoData == null )
				return $"[W:{WorldIDN}][Type:{ServerType}][GUID:{GUID}][IP:{ip}]";
			else
				return $"{ServerInfoData.GetInfo()}[IP:{ip}][Type:{ServerType}]";
		}

		//------------------------------------------------------------------------
		protected override void OnDisconnected()
		{
			base.OnDisconnected();

			ServerManager.Instance.RemoveInfo( mPeerIndex );
		}
	}
}
