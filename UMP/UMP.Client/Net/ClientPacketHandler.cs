//////////////////////////////////////////////////////////////////////////
//
// PacketHandler
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

namespace UMP.Client.Net
{
	public delegate void delegatePacketReceivedCallback( System.Type packet_id_type, short packet_id, object packet );

	//------------------------------------------------------------------------	
	public class ClientPacketHandler<ST, PT> : PacketHandler<ST, PT> where ST : Session where PT : class
	{
		protected short mPacketId = 0;
		protected System.Type mPacketIdType = null;
		delegatePacketReceivedCallback mReceivedCompactCallback = null;

		public ClientPacketHandler( DelegatePacketHandler<ST, PT> handler, System.Type packet_id_type, short packet_id, delegatePacketReceivedCallback received_compact_callback ) 
			: base( handler )
		{
			mPacketIdType = packet_id_type;
			mPacketId = packet_id;			
			mReceivedCompactCallback = received_compact_callback;
		}

		//------------------------------------------------------------------------
		public override void handle_packet( Session session, PacketContainer packet_container )
		{
			base.handle_packet( session, packet_container );

			if( mReceivedCompactCallback != null )
				mReceivedCompactCallback( mPacketIdType, mPacketId, packet_container.packet );
		}
	}
}
