//////////////////////////////////////////////////////////////////////////
//
// SSRelayPeerManager
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
using System.Net.Sockets;
using UMF.Net;
using UMF.Core;
using System.IO;

namespace UMF.Server
{
	//------------------------------------------------------------------------		
	public abstract class SSRelayPeerManager : PeerManager
	{
		protected SSRelayPeerManager mRelayPeerManager;

		//------------------------------------------------------------------------		
		public SSRelayPeerManager( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type N_send_packet_id_type, SSRelayPeerManager relay_peer_manager )
			: base( service_type, config_file, packetHandlerManager, send_packet_id_type, N_send_packet_id_type )
		{
			mRelayPeerManager = relay_peer_manager;
		}

		//------------------------------------------------------------------------		
		/// <summary>
		///   target_peer < 0 : broadcast
		/// </summary>
		public void DisconnectPeerRelay( int relay_peer_index, int target_peer_index, int error_code, string error_string )
		{
			DisconnectPeerRelay( relay_peer_index, target_peer_index, error_code, error_string, error_string );
		}

		//------------------------------------------------------------------------		
		/// <summary>
		///   target_peer < 0 : broadcast
		/// </summary>
		public void DisconnectPeerRelay( int relay_peer_index, int target_peer_index, int error_code, string error_string, string error_detail_string )
		{
			if( error_code < 0 )
				Log.WriteWarning( string.Format( "[{0}] DisconnectPeerTo({1}->{2}) {3}, {4}", ListenerName, relay_peer_index, target_peer_index, error_string, error_detail_string ) );
			else
				Log.Write( string.Format( "[{0}] DisconnectPeerTo({1}->{2}) {3}, {4}", ListenerName, relay_peer_index, target_peer_index, error_string, error_detail_string ) );

			if( mRelayPeerManager != null )
			{
				SSPeerDisconnect _SSPeerDisconnect = new SSPeerDisconnect();
				_SSPeerDisconnect.error_code = error_code;
				_SSPeerDisconnect.error_string = error_string;
				_SSPeerDisconnect.error_detail_string = error_detail_string;
				_SSPeerDisconnect.peer_index = target_peer_index;

				PacketFormatterConfig config = new PacketFormatterConfig();
				config.protocol_version = -1;
				config.UseConvertDatetime = mListenerConfig.UseConvertDatetime;
				config.IgnoreTimeZone = mListenerConfig.IgnoreTimeZone;

				if( relay_peer_index < 0 )
				{
					mRelayPeerManager.BroadcastStream( PacketWriteFormatter.Instance.Serialize( _SSPeerDisconnect, config ) );
				}
				else
				{
					Peer relay_peer = mRelayPeerManager.GetPeer( relay_peer_index );
					if( relay_peer != null )
					{
						relay_peer.SendStream( PacketWriteFormatter.Instance.Serialize( _SSPeerDisconnect, config ) );
					}
				}
			}
		}
	}

	//------------------------------------------------------------------------	
	public abstract class TSSRelayPeerManager<ST> : SSRelayPeerManager where ST : SSPeer, new()
	{
		public override Type SessionType { get { return typeof( ST ); } }

		public TSSRelayPeerManager( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type N_send_packet_id_type, SSRelayPeerManager relay_peer_manager )
			: base( service_type, config_file, packetHandlerManager, send_packet_id_type, N_send_packet_id_type, relay_peer_manager )
		{
		}

		//------------------------------------------------------------------------	
		protected override Peer CreateNewPeer( Socket socket )
		{
			Peer peer = new ST();
			peer.Init( this, socket );
			return peer;
		}

		//------------------------------------------------------------------------	
		public new ST GetPeer( int id )
		{
			Peer peer;
			if( mPeersDic.TryGetValue( id, out peer ) == false )
				return null;

			return peer as ST;
		}
	}
}
