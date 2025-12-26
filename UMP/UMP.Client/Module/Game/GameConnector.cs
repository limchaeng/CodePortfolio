//////////////////////////////////////////////////////////////////////////
//
// GameConnector
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
using UMP.Client.Net;

namespace UMP.Client
{
	public class GameConnector : ClientKeyConnector
	{
		public GameConnector( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type n_send_packet_id_type, string version, short application_identifier, short runtime_platform, string device_language, int revision, string localize ) 
			: base( service_type, config_file, packetHandlerManager, send_packet_id_type, n_send_packet_id_type, version, application_identifier, runtime_platform, device_language, revision, localize )
		{
		}
	}
}
