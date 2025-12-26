//////////////////////////////////////////////////////////////////////////
//
// PacketReceiver
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
using System.Collections.Generic;

namespace UMF.Net
{
	public delegate void delegatePacketReceiverHandler<PT>( PT packet );

	//------------------------------------------------------------------------		
	public abstract class PacketReceiverHandlerBase
	{
		public abstract Type PacketType { get; }
		public abstract void receive_packet( object packet );
		public abstract void Add( Delegate handler );
		public abstract void Remove( Delegate handler );
		public abstract void Set( Delegate handler );

		public abstract bool IsEmpty { get; }
	}

	//------------------------------------------------------------------------		
	public class PacketReceiverHandler<PT> : PacketReceiverHandlerBase
	{
		private delegatePacketReceiverHandler<PT> mHandler;

		public override Type PacketType { get { return typeof( PT ); } }

		//------------------------------------------------------------------------		
		public PacketReceiverHandler( delegatePacketReceiverHandler<PT> handler )
		{
			mHandler = handler;
		}

		//------------------------------------------------------------------------		
		public override void Add( Delegate handler )
		{
			mHandler = mHandler + (delegatePacketReceiverHandler<PT>)handler;
		}

		//------------------------------------------------------------------------		
		public override void Remove( Delegate handler )
		{
			if( mHandler == null )
				return;

			mHandler = mHandler - (delegatePacketReceiverHandler<PT>)handler;
		}

		//------------------------------------------------------------------------		
		public override void Set( Delegate handler )
		{
			mHandler = (delegatePacketReceiverHandler<PT>)handler;
		}

		//------------------------------------------------------------------------		
		public override void receive_packet( object packet )
		{
			if( mHandler != null )
				mHandler( (PT)packet );
		}

		//------------------------------------------------------------------------		
		public override bool IsEmpty
		{
			get { return ( mHandler == null ); }
		}
	}

	//------------------------------------------------------------------------
	public class PacketReceiverManager
	{
		Dictionary<short, PacketReceiverHandlerBase> mPacketReceiverDic = new Dictionary<short, PacketReceiverHandlerBase>();
		Dictionary<short, PacketReceiverHandlerBase> mPacketReceiverCompactDic = new Dictionary<short, PacketReceiverHandlerBase>();

		//------------------------------------------------------------------------
		public void Clear(bool is_compact_only)
		{
			mPacketReceiverCompactDic.Clear();
			if( is_compact_only == false )
				mPacketReceiverDic.Clear();
		}

		//-----------------------------------------------------------------------------
		public void Add_Receiver<PT>( delegatePacketReceiverHandler<PT> handler, bool is_compact )
		{
			PacketAttribute attr = PacketAttribute.GetAttrRaw( typeof( PT ) );
			short packet_id = attr.GetPacketIdRaw();

			Dictionary<short, PacketReceiverHandlerBase> root_dic = null;
			if( is_compact )
				root_dic = mPacketReceiverCompactDic;
			else
				root_dic = mPacketReceiverDic;

			PacketReceiverHandlerBase i_handler;
			if( root_dic.TryGetValue( packet_id, out i_handler ) )
				i_handler.Set( handler );
			else
				root_dic.Add( packet_id, new PacketReceiverHandler<PT>( handler ) );
		}

		//-----------------------------------------------------------------------------
		public void Remove_Receiver<PT>( delegatePacketReceiverHandler<PT> handler )
		{
			PacketAttribute attr = PacketAttribute.GetAttrRaw( typeof( PT ) );
			short packet_id = attr.GetPacketIdRaw();

			PacketReceiverHandlerBase i_handler;
			if( mPacketReceiverDic.TryGetValue( packet_id, out i_handler ) )
			{
				i_handler.Remove( handler );
				if( i_handler.IsEmpty )
					mPacketReceiverDic.Remove( packet_id );
			}

			if( mPacketReceiverCompactDic.TryGetValue( packet_id, out i_handler ) )
			{
				mPacketReceiverCompactDic.Remove( packet_id );
				if( i_handler.IsEmpty )
					mPacketReceiverCompactDic.Remove( packet_id );
			}
		}

		//-----------------------------------------------------------------------------
		public void Received_Packet( short packet_id, object packet )
		{
			PacketReceiverHandlerBase i_handler;
			if( mPacketReceiverCompactDic.TryGetValue( packet_id, out i_handler ) )
			{
				mPacketReceiverCompactDic.Remove( packet_id );
				i_handler.receive_packet( packet );
			}

			if( mPacketReceiverDic.TryGetValue( packet_id, out i_handler ) )
			{
				i_handler.receive_packet( packet );
			}
		}
	}
}
