//////////////////////////////////////////////////////////////////////////
//
// Exceptions
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

namespace UMF.Core
{
	//------------------------------------------------------------------------	
	public class PacketException : System.Exception
	{
		protected int errorCode;
		public int ErrorCode { get { return errorCode; } }

		public PacketException( int errorCode, string message )
			: base( message )
		{
			this.errorCode = errorCode;
		}
	}

	//------------------------------------------------------------------------	
	public class PacketDataException : System.Exception
	{
		int errorCode;
		public int ErrorCode { get { return errorCode; } }
		public object packet { get; private set; }

		public PacketDataException( int errorCode, string message, object packet )
			: base( message )
		{
			this.packet = packet;
			this.errorCode = errorCode;
		}
	}

	//------------------------------------------------------------------------	
	public class PeerDisconnectException : PacketException
	{
		int peerIndex;
		public int PeerIndex { get { return peerIndex; } }

		public PeerDisconnectException( int peer_index, int errorCode, string message )
			: base( errorCode, message )
		{
			this.peerIndex = peer_index;
		}
	}
}