//////////////////////////////////////////////////////////////////////////
//
// G2R_PacketHandlerManager
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

namespace UMP.Server.Relay
{
	//------------------------------------------------------------------------
	public class G2R_PacketHandlerManagerStandard : G2R_PacketHandlerManager<GameRelayPeer>
	{
		public G2R_PacketHandlerManagerStandard( RelayServerApplication application )
			: base( application, typeof( NPID_G2R ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class G2R_PacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : GameRelayPeer
	{
		protected RelayServerApplication mApplication = null;
		public RelayServerApplication Application { get { return mApplication; } }

		public G2R_PacketHandlerManager( RelayServerApplication application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_G2R ) )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NG2R_ConnectionInfo ) )]
		protected virtual void NG2R_ConnectionInfoHandler( ST session, object _packet )
		{
			NG2R_ConnectionInfo packet = _packet as NG2R_ConnectionInfo;

			session.m_GameGUID = packet.guid;
			session.m_GameHostName = packet.host_name;
			session.m_GamePort = packet.port;

			NR2G_ConnectionInfoAck _NR2G_ConnectionInfoAck = new NR2G_ConnectionInfoAck();
			_NR2G_ConnectionInfoAck.index_from_relay = session.PeerIndex;
			session.SendPacket( _NR2G_ConnectionInfoAck );
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NG2R_AccountLoginNotify ) )]
		protected virtual void NG2R_AccountLoginNotifyHandler( ST session, object _packet )
		{
			NG2R_AccountLoginNotify packet = _packet as NG2R_AccountLoginNotify;

			AccountStateManager.Instance.AccountLogin( packet.account_idx, packet.game_server_idx, packet.peer_index, packet.nickname, packet.gamedb_idx );

			NR2G_AccountLoginNotify _NR2G_AccountLoginNotify = new NR2G_AccountLoginNotify();
			_NR2G_AccountLoginNotify.account_idx = packet.account_idx;
			_NR2G_AccountLoginNotify.game_server_idx = packet.game_server_idx;
			_NR2G_AccountLoginNotify.peer_index = packet.peer_index;
			_NR2G_AccountLoginNotify.nickname = packet.nickname;
			_NR2G_AccountLoginNotify.gamedb_idx = packet.gamedb_idx;

			mApplication.GamePeerManager.BroadcastPacket( _NR2G_AccountLoginNotify, session.PeerIndex );
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NG2R_AccountLogoutNotify ) )]
		protected virtual void NG2R_AccountLogoutNotifyHandler( ST session, object _packet )
		{
			NG2R_AccountLogoutNotify packet = _packet as NG2R_AccountLogoutNotify;

			AccountStateManager.Instance.AccountLogout( packet.account_idx );

			NR2G_AccountLogoutNotify _NR2G_AccountLogoutNotify = new NR2G_AccountLogoutNotify();
			_NR2G_AccountLogoutNotify.account_idx = packet.account_idx;

			mApplication.GamePeerManager.BroadcastPacket( _NR2G_AccountLogoutNotify, session.PeerIndex );
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NG2R_PlayerLoginNotify ) )]
		protected virtual void NG2R_PlayerLoginNotifyHandler( ST session, object _packet )
		{
			NG2R_PlayerLoginNotify packet = _packet as NG2R_PlayerLoginNotify;

			AccountStateManager.Instance.PlayerLogin( packet.account_idx, packet.player_idx, packet.nickname );

			NR2G_PlayerLoginNotify _NR2G_PlayerLoginNotify = new NR2G_PlayerLoginNotify();
			_NR2G_PlayerLoginNotify.account_idx = packet.account_idx;
			_NR2G_PlayerLoginNotify.player_idx = packet.player_idx;
			_NR2G_PlayerLoginNotify.nickname = packet.nickname;

			mApplication.GamePeerManager.BroadcastPacket( _NR2G_PlayerLoginNotify, session.PeerIndex );
		}

	}
}
