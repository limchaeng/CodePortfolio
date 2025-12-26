//////////////////////////////////////////////////////////////////////////
//
// DaemonMasterPeer
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

namespace UMP.Server.Master
{
	//------------------------------------------------------------------------	
	public class DaemonMasterPeer : MasterPeer
	{
		//------------------------------------------------------------------------		
		protected override void OnVerified( object userInfo, PacketVerify verify )
		{
			base.OnVerified( userInfo, verify );

			ServerType = eServerType.Daemon;

			NM2D_CMD_server _NM2D_CMD_server = new NM2D_CMD_server();
			_NM2D_CMD_server.sub_command = MasterSubCommandName.server_maintenance;
			_NM2D_CMD_server.int_value = ( mUMPApplication.IsMaintenance ? 1 : 0 );
			SendPacket( _NM2D_CMD_server );
		}

		//------------------------------------------------------------------------		
		public string GetInfo()
		{
			return string.Format( "[W-{0}][IP:{1}][idx:{2}]", WorldIDN,
				( m_Socket != null ? ( (System.Net.IPEndPoint)m_Socket.RemoteEndPoint ).Address.ToString() : "null" ),
				PeerIndex );
		}

		//------------------------------------------------------------------------
		public virtual void SetDaemonStartup( ND2M_DaemonStatup packet )
		{
			// TODO
		}
	}
}
