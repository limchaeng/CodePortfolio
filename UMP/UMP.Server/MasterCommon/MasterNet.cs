//////////////////////////////////////////////////////////////////////////
//
// MasterNet
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
using System.IO;
using System.Net;
using System.Collections.Generic;
using UMF.Net;
using UMP.CSCommon;

namespace UMP.Server
{
	//------------------------------------------------------------------------	
	public class MasterConnector : AppConnector
	{
		protected override short VerifyPacketId => UMPServerPacketId.MasterVerifyPacketId;
		protected override string VerifyPacketLog( object packet )
		{
			return PacketLogFormatter.Instance.Serialize<MasterPacketVerify>( packet, false );
		}

		public MasterConnector( UMPServerApplication application, string config_file, Type send_packet_id_type, Type n_send_packet_id_type, PacketHandlerManagerBase packetHandlerManager )
			: base( application, config_file, packetHandlerManager, send_packet_id_type, n_send_packet_id_type )
		{
			MasterPacketVerify _MasterPacketVerify = mVerifyPacket as MasterPacketVerify;
			_MasterPacketVerify.server_type = application.ServerType;
			_MasterPacketVerify.world_idn = application.GetApplicationConfig.WorldIDN;
			_MasterPacketVerify.guid = application.GUID_LONG;

			mUMPApplication.MasterConnector = this;
		}

		//------------------------------------------------------------------------
		protected override PacketVerify CreateVerify()
		{
			return new MasterPacketVerify();
		}

		//------------------------------------------------------------------------
		protected override void Verify()
		{
			SendStream( PacketWriteFormatter.Instance.Serialize<MasterPacketVerify>( mVerifyPacket as MasterPacketVerify, GetPacketFormatterConfig ) );
		}

		//------------------------------------------------------------------------
		public void SendCommandResponse<PT>( PT packet, string response ) where PT : MasterCommandBase
		{
			NS2M_MasterCommandResponse _NS2M_MasterCommandResponse = new NS2M_MasterCommandResponse();
			_NS2M_MasterCommandResponse.Copy( packet );
			_NS2M_MasterCommandResponse.command = packet.GetType().ToString();
			_NS2M_MasterCommandResponse.response = response;
			SendPacket( _NS2M_MasterCommandResponse );
		}
	}

	//------------------------------------------------------------------------
	public class ServerMasterConnector : MasterConnector
	{
		public ServerMasterConnector( UMPServerApplication application, string config_file, Type send_packet_id_type, Type n_send_packet_id_type, PacketHandlerManagerBase packetHandlerManager )
			: base( application, config_file, send_packet_id_type, n_send_packet_id_type, packetHandlerManager )
		{
			
		}

		//------------------------------------------------------------------------		
		public override void OnConnected( bool bSuccessed )
		{
			base.OnConnected( bSuccessed );

			if( bSuccessed )
			{
				string ip = ( (IPEndPoint)m_Socket.LocalEndPoint ).Address.ToString();
				mUMPApplication.ServerIP = ip;

				SendServerConnectionInfo();
			}
		}

		//------------------------------------------------------------------------
		public virtual void SendServerConnectionInfo() { }
	}

	//------------------------------------------------------------------------
	public class MasterPeer : AppPeer
	{
		public long GUID { get; protected set; } = 0;
		public eServerType ServerType { get; protected set; } = eServerType.Unknown;
		public int WorldIDN { get; protected set; } = 0;

		protected override short VerifyPacketId => UMPServerPacketId.MasterVerifyPacketId;
		protected override string VerifyPacketLog( object packet )
		{
			return PacketLogFormatter.Instance.Serialize<MasterPacketVerify>( packet, false );
		}

		protected override PacketVerify VerifyPacket( short packetId, MemoryStream stream )
		{
			if( packetId != UMPServerPacketId.MasterVerifyPacketId )
				throw new Exception( "Not correct verify packet : " + packetId.ToString() );

			return PacketReadFormatter.Instance.Serialize<MasterPacketVerify>( new BinaryReader( stream ), GetPacketFormatterConfig );
		}

		protected override void OnVerified( object userInfo, PacketVerify verify )
		{
			base.OnVerified( userInfo, verify );

			MasterPacketVerify _MasterPacketVerify = (MasterPacketVerify)verify;
			ServerType = _MasterPacketVerify.server_type;
			WorldIDN = _MasterPacketVerify.world_idn;
			GUID = _MasterPacketVerify.guid;
		}
	}
}
