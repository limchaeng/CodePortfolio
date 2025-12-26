//////////////////////////////////////////////////////////////////////////
//
// ClientConnector
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
using UMP.CSCommon.Packet;

namespace UMP.Client.Net
{
	//------------------------------------------------------------------------	
	public class ClientConnector : Connector
    {
		//------------------------------------------------------------------------		
        public ClientConnector( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type n_send_packet_id_type, string version, short application_identifier, int runtime_platform, string device_language, int revision, string localize)
            : base(service_type, config_file, packetHandlerManager, send_packet_id_type, n_send_packet_id_type)
        {
            ClientPacketVerify _ClientPacketVerify = (ClientPacketVerify)mVerifyPacket;
            _ClientPacketVerify.version = version;
			_ClientPacketVerify.revision = revision;
            _ClientPacketVerify.application_identifier = application_identifier;
            _ClientPacketVerify.runtime_platform = runtime_platform;
			_ClientPacketVerify.device_language = device_language;
			_ClientPacketVerify.app_language = localize;
        }

		protected override PacketVerify CreateVerify() { return new ClientPacketVerify(); }
		protected override short VerifyPacketId => CSPacketId.ClientVerifyPacketId;
		protected override string VerifyPacketLog( object packet )
		{
			return PacketLogFormatter.Instance.Serialize<ClientPacketVerify>( packet, false );
		}

		//------------------------------------------------------------------------		
		protected override void Verify()
		{
			SendStream( PacketWriteFormatter.Instance.Serialize<ClientPacketVerify>( mVerifyPacket as ClientPacketVerify, GetPacketFormatterConfig ) );
		}

		//------------------------------------------------------------------------		
		public virtual void ConnectTo( string device_language, string curr_localize )
		{
			( (ClientPacketVerify)mVerifyPacket ).device_language = device_language;
			( (ClientPacketVerify)mVerifyPacket ).app_language = curr_localize;
			Connect();
		}
	}
}
