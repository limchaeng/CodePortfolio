//////////////////////////////////////////////////////////////////////////
//
// PacketReceivedCallback
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

using UMF.Core;
using System.Collections.Generic;
using UMF.Net;
using UMP.CSCommon;
using System;

namespace UMP.Client.Net
{
	public class NetworkHandler : Singleton<NetworkHandler>
	{
		/*
		public delegate void delegateOnReceivedPacket<PT>( PT packet );

		//------------------------------------------------------------------------		
		public abstract class IReceivedPacketHander
		{
			public abstract System.Type PacketType { get; }
			public abstract void handle_packet( object packet );
			public abstract void Add( System.Delegate handler );
			public abstract void Remove( System.Delegate handler );
			public abstract void Set( System.Delegate handler );

			public abstract bool IsEmpty { get; }
		}

		//------------------------------------------------------------------------		
		public class ReceivedPacketHandler<PT> : IReceivedPacketHander
		{
			private delegateOnReceivedPacket<PT> mHandler;

			override public System.Type PacketType { get { return typeof( PT ); } }

			public ReceivedPacketHandler( delegateOnReceivedPacket<PT> handler )
			{
				mHandler = handler;
			}

			override public void Add( System.Delegate handler )
			{
				mHandler = mHandler + (delegateOnReceivedPacket<PT>)handler;
			}

			override public void Remove( System.Delegate handler )
			{
				if( mHandler == null )
					return;

				mHandler = mHandler - (delegateOnReceivedPacket<PT>)handler;
			}

			public override void Set( Delegate handler )
			{
				mHandler = (delegateOnReceivedPacket<PT>)handler;
			}

			override public void handle_packet( object packet )
			{
				if( mHandler != null )
					mHandler( (PT)packet );
			}

			public override bool IsEmpty
			{
				get { return ( mHandler == null ); }
			}
		}

		Dictionary<System.Type, Dictionary<short, IReceivedPacketHander>> mPacketHandlerDic = new Dictionary<System.Type, Dictionary<short, IReceivedPacketHander>>();
		Dictionary<System.Type, Dictionary<short, IReceivedPacketHander>> mPacketHandlerCompactDic = new Dictionary<System.Type, Dictionary<short, IReceivedPacketHander>>();

		//------------------------------------------------------------------------	
		public void OnceHandlerClear()
		{
			mPacketHandlerCompactDic.Clear();
		}

		//-----------------------------------------------------------------------------
		public void Add_Handler<PT>( delegateOnReceivedPacket<PT> handler, bool is_compact = false )
		{
			try
			{
				PacketAttribute attr = PacketAttribute.GetAttrRaw( typeof( PT ) );
				short packet_id = attr.GetPacketIdRaw();

				Dictionary<System.Type, Dictionary<short, IReceivedPacketHander>> root_dic = null;
				if( is_compact )
					root_dic = mPacketHandlerCompactDic;
				else
					root_dic = mPacketHandlerDic;

				Dictionary<short, IReceivedPacketHander> dic;
				if( root_dic.TryGetValue( attr.GetPacketIdType(), out dic ) )
				{
					IReceivedPacketHander i_handler;
					if( dic.TryGetValue( packet_id, out i_handler ) )
						i_handler.Set( handler );
					else
						dic.Add( packet_id, new ReceivedPacketHandler<PT>( handler ) );
				}
				else
				{
					dic = new Dictionary<short, IReceivedPacketHander>();
					dic.Add( packet_id, new ReceivedPacketHandler<PT>( handler ) );

					root_dic.Add( attr.GetPacketIdType(), dic );
				}
			}
			catch( System.Exception ex )
			{
				//GameMain.Instance.UnHandledException( ex.ToString() );
			}
		}

		//-----------------------------------------------------------------------------
		public void Remove_Handler<PT>( delegateOnReceivedPacket<PT> handler )
		{
			try
			{
				PacketAttribute attr = PacketAttribute.GetAttrRaw( typeof( PT ) );
				short packet_id = attr.GetPacketIdRaw();

				Dictionary<short, IReceivedPacketHander> dic;
				if( mPacketHandlerDic.TryGetValue( attr.GetPacketIdType(), out dic ) )
				{
					IReceivedPacketHander i_handler;
					if( dic.TryGetValue( packet_id, out i_handler ) )
					{
						i_handler.Remove( handler );
						if( i_handler.IsEmpty )
							dic.Remove( packet_id );
					}
				}
			}
			catch( System.Exception ex )
			{
				//GameMain.Instance.UnHandledException( ex.ToString() );
			}
		}

		//-----------------------------------------------------------------------------
		public void Received_Packet<PT>( object packet )
		{
			try
			{
				PacketAttribute attr = PacketAttribute.GetAttrRaw( typeof( PT ) );
				short packet_id = attr.GetPacketIdRaw();

				Dictionary<short, IReceivedPacketHander> dic;
				if( mPacketHandlerDic.TryGetValue( attr.GetPacketIdType(), out dic ) )
				{
					IReceivedPacketHander i_handler;
					if( dic.TryGetValue( packet_id, out i_handler ) )
					{
						i_handler.handle_packet( packet );
					}
				}
			}
			catch( System.Exception ex )
			{
				//GameMain.Instance.UnHandledException( ex.ToString() );
			}
		}
		public void Received_PacketCompact( System.Type packet_id_type, short packet_id, object packet )
		{
			try
			{
				Dictionary<short, IReceivedPacketHander> dic;
				if( mPacketHandlerCompactDic.TryGetValue( packet_id_type, out dic ) )
				{
					IReceivedPacketHander i_handler;
					if( dic.TryGetValue( packet_id, out i_handler ) )
					{
						dic.Remove( packet_id );
						i_handler.handle_packet( packet );
					}
				}

				dic = null;
				if( mPacketHandlerDic.TryGetValue( packet_id_type, out dic ) )
				{
					IReceivedPacketHander i_handler;
					if( dic.TryGetValue( packet_id, out i_handler ) )
					{
						i_handler.handle_packet( packet );
					}
				}
			}
			catch( System.Exception ex )
			{
				//GameMain.Instance.UnHandledException( ex.ToString() );
			}
		}
		*/
		//-----------------------------------------------------------------------------
		// Connect / Disconnect handler
		//-----------------------------------------------------------------------------
		public delegate void delegateOnServerConnected( eServerType server );
		public delegate void delegateOnServerDisconnected( eServerType server, int error, string err_msg );
		public delegate void delegateOnServerVerified( eServerType server );

		public delegateOnServerConnected OnServerConnectedHandler;
		public delegateOnServerDisconnected OnServerDisconnectedHandler;
		public delegateOnServerVerified OnServerVerifiedHandler;

		//------------------------------------------------------------------------		
		public void ServerConnected( eServerType server )
		{
			if( OnServerConnectedHandler != null )
				OnServerConnectedHandler( server );
		}

		//------------------------------------------------------------------------		
		public void ServerDisconnected( eServerType server, int error, string err_msg, string err_detail )
		{
			if( OnServerDisconnectedHandler != null )
				OnServerDisconnectedHandler( server, error, err_msg );
		}

		//------------------------------------------------------------------------		
		public void ServerVerified( eServerType server )
		{
			if( OnServerVerifiedHandler != null )
				OnServerVerifiedHandler( server );
		}
	}
}
