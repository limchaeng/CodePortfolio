//////////////////////////////////////////////////////////////////////////
//
// SSPacketCore
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
using UMF.Net;
using UMF.Core;

namespace UMF.Server
{
	//------------------------------------------------------------------------	
	public struct SSPacketId
	{
		public const short SSDisconnectPeerPacketId = -562;
	}

	//------------------------------------------------------------------------	
	[Packet( SSPacketId.SSDisconnectPeerPacketId, eCoreLogType.Always )]
	public class SSPeerDisconnect : PacketDisconnect
	{
		public int peer_index;
	}

	//------------------------------------------------------------------------	
	public class RelayPeerDisconnectException : PacketException
	{
		int relay_peer_index;
		int peer_index;
		public int RelayPeerIndex { get { return relay_peer_index; } }
		public int PeerIndex { get { return peer_index; } }

		public RelayPeerDisconnectException( int relay_peer_index, int peer_index, int errorCode, string message )
			: base( errorCode, message )
		{
			this.relay_peer_index = relay_peer_index;
			this.peer_index = peer_index;
		}
	}

	//------------------------------------------------------------------------		
	public class PacketWithSenderIndex
	{
		public int senderIndex;
	}

	//------------------------------------------------------------------------
	public class PacketWithRelaySenderIndex
	{
		public int relay_index;
		public int sender_index;
	}
}
