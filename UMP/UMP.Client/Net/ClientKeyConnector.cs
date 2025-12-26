//////////////////////////////////////////////////////////////////////////
//
// ClientKeyConnector
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
	public class ClientKeyConnector : Connector
	{
		//------------------------------------------------------------------------		
		public ClientKeyConnector( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type n_send_packet_id_type, string version, short application_identifier, short runtime_platform, string device_language, int revision, string localize )
            : base(service_type, config_file, packetHandlerManager, send_packet_id_type, n_send_packet_id_type)
        {
            ClientKeyPacketVerify _VersionKeyPacketVerify = (ClientKeyPacketVerify)mVerifyPacket;
            _VersionKeyPacketVerify.version = version;
			_VersionKeyPacketVerify.revision = revision;
            _VersionKeyPacketVerify.application_identifier = application_identifier;
            _VersionKeyPacketVerify.runtime_platform = runtime_platform;
            _VersionKeyPacketVerify.device_language = device_language;
			_VersionKeyPacketVerify.app_language = localize;
			_VersionKeyPacketVerify.connection_key = -1;
        }

		protected override PacketVerify CreateVerify() { return new ClientKeyPacketVerify(); }
		protected override short VerifyPacketId => CSPacketId.ClientKeyVerifyPacketId;
		protected override string VerifyPacketLog( object packet )
		{
			return PacketLogFormatter.Instance.Serialize<ClientKeyPacketVerify>( packet, false );
		}

		//------------------------------------------------------------------------		
		protected override void Verify()
		{
			SendStream( PacketWriteFormatter.Instance.Serialize<ClientKeyPacketVerify>( mVerifyPacket as ClientKeyPacketVerify, GetPacketFormatterConfig ) );
		}

		//------------------------------------------------------------------------		
		public virtual void ConnectTo(string hostname, int port, long connectionKey, string device_language, string curr_localize)
		{
			( (ClientKeyPacketVerify)mVerifyPacket ).connection_key = connectionKey;
			( (ClientKeyPacketVerify)mVerifyPacket ).device_language = device_language;
			( (ClientKeyPacketVerify)mVerifyPacket ).app_language = curr_localize;
			ConnectTo( hostname, port );
		}
	}
}
