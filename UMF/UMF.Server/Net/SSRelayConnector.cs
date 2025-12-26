//////////////////////////////////////////////////////////////////////////
//
// SSRelayConnector
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using System;
using UMF.Net;
using System.IO;
using UMF.Core;

namespace UMF.Server.Net
{
	public class SSRelayConnector : Connector
	{
		protected PeerManagerBase mRelayPeerManager;

		public SSRelayConnector( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type n_send_packet_id_type, PeerManagerBase relay_peer_manager )
			: base( service_type, config_file, packetHandlerManager, send_packet_id_type, n_send_packet_id_type )
		{
			mRelayPeerManager = relay_peer_manager;
		}

		//------------------------------------------------------------------------		
		protected override void handle_packet( PacketContainer packet )
		{
			try
			{
				if( packet.packet is MemoryStream )
				{
					MemoryStream stream = packet.packet as MemoryStream;
					short packetId = BitConverter.ToInt16( stream.GetBuffer(), 2 );
					stream.Seek( 4, SeekOrigin.Begin );

					if( packetId == SSPacketId.SSDisconnectPeerPacketId )
					{
						SSPeerDisconnect disconnect = PacketReadFormatter.Instance.Serialize<SSPeerDisconnect>( new BinaryReader( stream ), GetPacketFormatterConfig );
						mRelayPeerManager.DisconnectPeer( disconnect.peer_index, disconnect.error_code, disconnect.error_string, disconnect.error_detail_string );
						return;
					}
				}
				base.handle_packet( packet );
			}
			catch( PeerDisconnectException ex )
			{
				mRelayPeerManager.DisconnectPeer( ex.PeerIndex, ex.ErrorCode, ex.Message, ex.ToString() );
			}
			catch( System.Exception ex )
			{
				if( packet.packet.GetType().IsSubclassOf( typeof( PacketWithSenderIndex ) ) == true )
				{
					mRelayPeerManager.DisconnectPeer( ( (PacketWithSenderIndex)packet.packet ).senderIndex, (int)eDisconnectErrorCode.SystemError, ex.Message, ex.ToString() );
				}
				else
					throw ex;
			}
		}
	}
}
