//////////////////////////////////////////////////////////////////////////
//
// SSPeer
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
using UMF.Core;
using System.Net.Sockets;

namespace UMF.Server
{
	public class SSPeer : Peer
	{
		protected SSRelayPeerManager mRelayPeerManager = null;

		//------------------------------------------------------------------------
		public override void Init( PeerManagerBase peerManager, Socket socket )
		{
			if( peerManager is SSRelayPeerManager )
				mRelayPeerManager = (SSRelayPeerManager)peerManager;

			base.Init( peerManager, socket );
		}

		//------------------------------------------------------------------------	
		protected override void handle_packet( PacketContainer packet )
		{
			try
			{
				base.handle_packet( packet );
			}
			catch( PeerDisconnectException ex )
			{
				DisconnectPeerSend( ex.PeerIndex, ex.ErrorCode, ex.Message, ex.ToString() );
			}
			catch( RelayPeerDisconnectException ex )
			{
				if( mRelayPeerManager != null )
				{
					mRelayPeerManager.DisconnectPeerRelay( ex.RelayPeerIndex, ex.PeerIndex, ex.ErrorCode, ex.Message, ex.ToString() );
				}
				else
				{
					DisconnectPeerSend( ex.PeerIndex, ex.ErrorCode, ex.Message, ex.ToString() );
				}
			}
			catch( System.Exception ex )
			{
				if( packet.GetType().IsSubclassOf( typeof( PacketWithRelaySenderIndex ) ) == true )
				{
					PacketWithRelaySenderIndex p = (PacketWithRelaySenderIndex)packet.packet;
					if( mRelayPeerManager != null )
					{						
						mRelayPeerManager.DisconnectPeerRelay( p.relay_index, p.sender_index, (int)eDisconnectErrorCode.SystemError, ex.Message, ex.ToString() );
					}
					else
					{
						DisconnectPeerSend( p.sender_index, (int)eDisconnectErrorCode.SystemError, ex.Message, ex.ToString() );
					}
				}
				else if( packet.packet.GetType().IsSubclassOf( typeof( PacketWithSenderIndex ) ) == true )
				{
					DisconnectPeerSend( ( (PacketWithSenderIndex)packet.packet ).senderIndex, (int)eDisconnectErrorCode.SystemError, ex.Message, ex.ToString() );
				}
				else
					throw ex;
			}
		}

		//------------------------------------------------------------------------
		public override void handle_output()
		{
			try
			{
				base.handle_output();
			}
			catch( PacketDataException ex )
			{
				if( ex.packet != null )
				{
					if( ex.packet.GetType().IsSubclassOf( typeof( PacketWithRelaySenderIndex ) ) == true )
					{
						PacketWithRelaySenderIndex p = (PacketWithRelaySenderIndex)ex.packet;
						if( mRelayPeerManager != null )
						{							
							mRelayPeerManager.DisconnectPeerRelay( p.relay_index, p.sender_index, (int)eDisconnectErrorCode.SystemError, "packet output error in " + ex.packet.ToString() );
						}
						else
						{
							DisconnectPeerSend( p.sender_index, (int)eDisconnectErrorCode.SystemError, "packet output error in " + ex.packet.ToString() );
						}
						Log.WriteError( ex.ToString() );
					}
					else if( ex.packet.GetType().IsSubclassOf( typeof( PacketWithSenderIndex ) ) == true )
					{
						DisconnectPeerSend( ( (PacketWithSenderIndex)ex.packet ).senderIndex, (int)eDisconnectErrorCode.SystemError, "packet output error in " + ex.packet.ToString() );
						Log.WriteError( ex.ToString() );
					}
				}
			}
			catch( Exception ex )
			{
				throw ex;
			}
		}


		//------------------------------------------------------------------------	
		public void DisconnectPeerSend( int peer_index, int error_code, string error_string )
		{
			DisconnectPeerSend( peer_index, error_code, error_string, "" );
		}
		public void DisconnectPeerSend( int peer_index, int error_code, string error_string, string error_detail_string )
		{
			if( error_code >= 0 )
			{
				if( error_string == error_detail_string )
					Log.Write( string.Format( "[{0}] DisconnectPeerSend({1}) {2}, {3}", SessionName, peer_index, PacketDisconnect.GetErrorCodeString( error_code ), error_string ) );
				else
					Log.Write( string.Format( "[{0}] DisconnectPeerSend({1}) {2}, {3}, {4}", SessionName, peer_index, PacketDisconnect.GetErrorCodeString( error_code ), error_string, error_detail_string ) );
			}
			else
			{
				if( error_string == error_detail_string )
					Log.WriteWarning( string.Format( "[{0}] DisconnectPeerSend({1}) {2}, {3}", SessionName, peer_index, PacketDisconnect.GetErrorCodeString( error_code ), error_string ) );
				else
					Log.WriteWarning( string.Format( "[{0}] DisconnectPeerSend({1}) {2}, {3}, {4}", SessionName, peer_index, PacketDisconnect.GetErrorCodeString( error_code ), error_string, error_detail_string ) );
			}

			SSPeerDisconnect _SSPeerDisconnect = new SSPeerDisconnect();
			_SSPeerDisconnect.error_code = error_code;
			_SSPeerDisconnect.error_string = error_string;
			_SSPeerDisconnect.error_detail_string = error_detail_string;
			_SSPeerDisconnect.peer_index = peer_index;
			SendStream( PacketWriteFormatter.Instance.Serialize( _SSPeerDisconnect, GetPacketFormatterConfig ) );
		}
	}
}
