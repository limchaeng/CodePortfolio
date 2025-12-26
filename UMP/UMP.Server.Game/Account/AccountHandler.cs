//////////////////////////////////////////////////////////////////////////
//
// AccountHandler
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

using System.Collections;
using UMF.Database;
using UMF.Core;
using UMF.Net;
using System;

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------	
	[AttributeUsage( AttributeTargets.Method )]
	public class AccountDBPacketHandlerAttribute : DBPacketHandlerAttribute
	{
		public AccountDBPacketHandlerAttribute()
			: base()
		{
		}
	}

	//------------------------------------------------------------------------	
	public class AccountDBPacketObjectHandler<ST> : DBHandlerBase where ST : ClientGamePeer
	{
		public delegate IEnumerator DelegateHandler( ST session, Account account, object packet, DBHandlerObject data );
		protected DelegateHandler _handler;

		public AccountDBPacketObjectHandler( DelegateHandler handler )
		{
			_handler = handler;
		}

		//------------------------------------------------------------------------	
		public void handler( ST session, object packet )
		{
			Account account = AccountManager.Instance.GetAccount_PeerIndex( session.PeerIndex );
			if( account == null )
				throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.CannotFindPeer, "can't find peer" );

			DatabaseMain database = account.GameDatabase;
			if( database == null || database.DBEnabled == false )
				throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.CannotFindDB, "can't find database" );

			DBNetHandlerObject data = new DBNetHandlerObject();

			data.database = database;
			data.procedureIndexInternal = 0;
			data.recvIndex = session.RecvHandleCount;
			data.packet = packet;
			data.session_packet_handler = new DBEnumerator( packet_handler( session, packet, data ), data );
			data.peer = session;
			database.AddSessionPacketHandler( data );
		}

		//------------------------------------------------------------------------	
		public IEnumerator packet_handler( ST session, object packet, DBHandlerObject data )
		{
			Account account = AccountManager.Instance.GetAccount_PeerIndex( session.PeerIndex );
			if( account == null )
				throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.CannotFindPeer, "can't find peer" );

			using( CacheLock dblock = new CacheLock( account, packet.GetType() ) )
			{
				yield return dblock.GetLock( false, 0 );

				IEnumerator handler = _handler( session, account, packet, data );

				yield return handler;
			}
		}
	}

	//------------------------------------------------------------------------	
	public class AccountDBHandler<RecvT> : DBHandlerBase
	{
		public delegate IEnumerator handler_delegate( ClientGamePeer session, Account account, RecvT packet, DBHandlerObject data );
		protected handler_delegate _handler;

		public AccountDBHandler( handler_delegate handler )
		{
			_handler = handler;
		}

		//------------------------------------------------------------------------	
		public void handler( ClientGamePeer session, RecvT packet )
		{
			Account account = AccountManager.Instance.GetAccount_PeerIndex( session.PeerIndex );
			if( account == null )
				throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.CannotFindPeer, "can't find peer" );

			DatabaseMain database = account.GameDatabase;
			if( database == null || database.DBEnabled == false )
				throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.CannotFindDB, "can't find database" );

			DBNetHandlerObject data = new DBNetHandlerObject();

			data.database = database;
			data.procedureIndexInternal = 0;
			data.recvIndex = session.RecvHandleCount;
			data.packet = packet;
			data.session_packet_handler = new DBEnumerator( packet_handler( session, packet, data ), data );
			data.peer = session;
			database.AddSessionPacketHandler( data );
		}

		//------------------------------------------------------------------------	
		public IEnumerator packet_handler( ClientGamePeer session, RecvT packet, DBHandlerObject data )
		{
			Account account = AccountManager.Instance.GetAccount_PeerIndex( session.PeerIndex );
			if( account == null )
				throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.CannotFindPeer, "can't find peer" );

			using( CacheLock dblock = new CacheLock( account, typeof( RecvT ) ) )
			{
				yield return dblock.GetLock( false, 0 );

				IEnumerator handler = _handler( session, account, packet, data );

				yield return handler;
			}
		}
	}

	//------------------------------------------------------------------------	
	[AttributeUsage( AttributeTargets.Method )]
	public class AccountPacketHandlerAttribute : PacketHandlerAttribute
	{
		public AccountPacketHandlerAttribute()
			: base()
		{
		}
	}

	//------------------------------------------------------------------------	
	public class AccountPacketObjectHandler<ST> where ST : ClientGamePeer
	{
		public delegate void DelegateHandler( ST session, Account account, object packet );
		protected DelegateHandler _handler;

		public AccountPacketObjectHandler( DelegateHandler handler )
		{
			_handler = handler;
		}

		//------------------------------------------------------------------------		
		public void handler( Session session, object packet )
		{
			ST peer = session as ST;

			Account account = AccountManager.Instance.GetAccount_PeerIndex( peer.PeerIndex );
			if( account == null )
				throw new PeerDisconnectException( peer.PeerIndex, (int)eDisconnectErrorCode.CannotFindPeer, "can't find peer" );

			_handler( peer, account, packet );
		}
	}

	//------------------------------------------------------------------------	
	public class AccountHandler<RecvT>
	{
		public delegate void handler_delegate( ClientGamePeer session, Account account, RecvT packet );
		protected handler_delegate _handler;

		public AccountHandler( handler_delegate handler )
		{
			_handler = handler;
		}

		//------------------------------------------------------------------------		
		public void handler( ClientGamePeer session, RecvT packet )
		{
			Account account = AccountManager.Instance.GetAccount_PeerIndex( session.PeerIndex );
			if( account == null )
				throw new PeerDisconnectException( session.PeerIndex, (int)eDisconnectErrorCode.CannotFindPeer, "can't find peer" );

			_handler( session, account, packet );
		}
	}
}
