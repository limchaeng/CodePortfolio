//////////////////////////////////////////////////////////////////////////
//
// DBNetHandler
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
using UMF.Database;
using UMF.Net;
using System.Collections;
using UMF.Core;
using UMF.Server;
using System.Reflection;

namespace UMP.Server
{
	//------------------------------------------------------------------------
	public class DBNetHandlerObject : DBHandlerObject
	{
		public Peer peer;

		public override void ThrowPacketException( Exception ex )
		{
			base.ThrowPacketException( ex );

			if( peer == null )
				return;

			if( ex is PeerDisconnectException )
			{
				PeerDisconnectException peer_ex = ex as PeerDisconnectException;
				if( peer != null )
				{
					string packet_log = "";
					if( Log.g_UserDisconnectWithPacketLog )
						packet_log = " [P]" + PacketLogFormatter.Instance.SerializeDirect( packet );

					if( peer is SSPeer )
					{
						( (SSPeer)peer ).DisconnectPeerSend( peer_ex.PeerIndex, peer_ex.ErrorCode, peer_ex.Message, peer_ex.ToString() + packet_log );
					}
					else
					{
						peer.Disconnect( peer_ex.ErrorCode, peer_ex.Message, peer_ex.ToString() + packet_log );
					}
				}
			}
			else if( ex is PacketException )
			{
				PacketException packet_ex = ex as PacketException;

				string packet_log = "";
				if( Log.g_UserDisconnectWithPacketLog )
					packet_log = " [P]" + PacketLogFormatter.Instance.SerializeDirect( packet );

				if( peer is SSPeer )
				{
					if( packet != null && packet.GetType().IsSubclassOf( typeof( PacketWithSenderIndex ) ) == true )
					{
						int sender_idx = ( (PacketWithSenderIndex)packet ).senderIndex;
						( (SSPeer)peer ).DisconnectPeerSend( sender_idx, packet_ex.ErrorCode, packet_ex.Message, packet_ex.ToString() + packet_log );
					}
					else
					{
						Log._LogError( string.Format( "ExecutePacketHandler On SSPeer PacketException:{0}:{1}:{2}", packet_ex.ErrorCode, packet_ex.Message, ex.ToString() + packet_log ) );
					}
				}
				else
				{
					peer.Disconnect( packet_ex.ErrorCode, packet_ex.Message, packet_ex.ToString() + packet_log );
				}
			}
			else 
			{
				if( peer != null )
				{
					if( peer is SSPeer )
					{
						Log._LogError( string.Format( "ExecutePacketHandler On SSPeer Exception:{0}:{1}", ex.Message, ex.ToString() ) );
					}
					else
					{
						peer.Disconnect( (int)eDisconnectErrorCode.SystemError, ex.Message, ex.ToString() );
					}
				}
			}
		}
	}

	//------------------------------------------------------------------------	
	public class DBNetHandler<ST, RecvT> : DBHandlerBase where ST : Peer
	{
		public delegate IEnumerator DelegateHandler( ST session, RecvT packet, DBHandlerObject data );

		protected DelegateHandler _handler;
		DatabaseMain database;

		public DBNetHandler( DelegateHandler handler, DatabaseMain database )
		{
			_handler = handler;
			this.database = database;
		}

		//------------------------------------------------------------------------		
		public void handler( ST session, RecvT packet )
		{
			DBNetHandlerObject data = new DBNetHandlerObject();
			data.database = database;
			data.procedureIndexInternal = 0;
			data.recvIndex = session.RecvHandleCount;
			data.packet = packet;
			data.session_packet_handler = new DBEnumerator( _handler( session, packet, data ), data );

			data.peer = session;
			database.AddSessionPacketHandler( data );
		}
	}

	//------------------------------------------------------------------------	
	public class DBNetPacketObjectHandler<ST> : DBHandlerBase where ST : Peer
	{
		public delegate IEnumerator DelegateHandler( ST session, object packet, DBHandlerObject data );

		protected DelegateHandler mHandler;
		protected UMPServerApplication mApplication;
		protected eUMPAppDBType mDBType;

		public DBNetPacketObjectHandler( DelegateHandler handler, UMPServerApplication application, eUMPAppDBType db_type )
		{
			mHandler = handler;
			mApplication = application;
			mDBType = db_type;
		}

		//------------------------------------------------------------------------		
		public void handler( ST session, object packet )
		{
			if( mDBType != eUMPAppDBType.None && mApplication == null )
			{
				Log.WriteError( "DBNetHandler : application not found:{0}", mDBType );
				return;
			}

			DatabaseMain database = mApplication.GetDB( mDBType );
			if( database == null )
			{
				Log.WriteError( "DBNetHandler : database not found:{0}", mDBType );
				return;
			}

			DBNetHandlerObject data = new DBNetHandlerObject();
			data.database = database;
			data.procedureIndexInternal = 0;
			data.recvIndex = session.RecvHandleCount;
			data.packet = packet;
			data.session_packet_handler = new DBEnumerator( mHandler( session, packet, data ), data );

			data.peer = session;
			database.AddSessionPacketHandler( data );
		}
	}

	//------------------------------------------------------------------------	
	[AttributeUsage( AttributeTargets.Method )]
	public class DBPacketHandlerAttribute : PacketHandlerAttribute
	{
		public eUMPAppDBType DBType { get; set; }
		public DBPacketHandlerAttribute()
			: base()
		{
			DBType = eUMPAppDBType.None;
		}
	}

	//------------------------------------------------------------------------
	public class DBPacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : Peer
	{
		public DBPacketHandlerManager( UMPServerApplication application, Type packet_id_type, Type n_packet_id_type )
			: base( application, packet_id_type, n_packet_id_type )
		{
		}

		//------------------------------------------------------------------------
		public void AddDBHandler<RecvT>( DBNetHandler<ST, RecvT>.DelegateHandler handler, DatabaseMain database ) where RecvT : class
		{
			AddHandler<RecvT>( new DBNetHandler<ST, RecvT>( handler, database ).handler );
		}

		//------------------------------------------------------------------------
		public override PacketObjectHandler<ST> CreateAutoHandler( UMPServerApplication application, MethodInfo method, PacketHandlerAttribute handler_attr )
		{
			if( handler_attr.GetType().Equals( typeof( DBPacketHandlerAttribute ) ) )
			{
				DBPacketHandlerAttribute db_attr = handler_attr as DBPacketHandlerAttribute;
				Type handler_delegate_type = typeof( DBNetPacketObjectHandler<ST>.DelegateHandler );
				DBNetPacketObjectHandler<ST>.DelegateHandler handler_delegate = (DBNetPacketObjectHandler<ST>.DelegateHandler)Delegate.CreateDelegate( handler_delegate_type, this, method );
				return new PacketObjectHandler<ST>( db_attr.PacketType, new DBNetPacketObjectHandler<ST>( handler_delegate, application, db_attr.DBType ).handler );
			}
			else
			{
				return base.CreateAutoHandler( application, method, handler_attr );
			}
		}
	}
}
