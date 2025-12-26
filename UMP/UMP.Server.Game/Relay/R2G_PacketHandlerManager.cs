//////////////////////////////////////////////////////////////////////////
//
// R2G_PacketHandlerManager
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

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------
	public class R2G_PacketHandlerManagerStandard : R2G_PacketHandlerManager<GameRelayConnector>
	{
		public R2G_PacketHandlerManagerStandard( GameServerApplication application)
			: base( application, typeof( NPID_R2G ) )
		{
		}
	}

	//------------------------------------------------------------------------	
	public class R2G_PacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : GameRelayConnector
	{
		protected GameServerApplication mApplication;
		public GameServerApplication Application { get { return mApplication; } }

		public R2G_PacketHandlerManager( GameServerApplication application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_R2G ) )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NR2G_ConnectionInfoAck ) )]
		protected virtual void NR2G_ConnectionInfoAckHandler( ST session, object _packet )
		{
			NR2G_ConnectionInfoAck packet = _packet as NR2G_ConnectionInfoAck;

			mApplication.ServerNetIndex = packet.index_from_relay;
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NR2G_AccountLoginNotify ) )]
		protected virtual void NR2G_AccountLoginNotifyHandler( ST session, object _packet )
		{
			NR2G_AccountLoginNotify packet = _packet as NR2G_AccountLoginNotify;

			AccountManager.Instance.CheckDuplicateLogin( packet.account_idx );
			AccountStateManager.Instance.AccountLogin( packet.account_idx, packet.game_server_idx, packet.peer_index, packet.nickname, packet.gamedb_idx );
		}

		//------------------------------------------------------------------------		
		[PacketHandler( PacketType = typeof( NR2G_AccountLogoutNotify ) )]
		protected void NR2G_AccountLogoutNotifyHandler( ST session, object _packet )
		{
			NR2G_AccountLogoutNotify packet = _packet as NR2G_AccountLogoutNotify;

			AccountStateManager.Instance.AccountLogout( packet.account_idx );
		}


		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( NR2G_PlayerLoginNotify ) )]
		protected virtual void NR2G_PlayerLoginNotifyHandler( ST session, object _packet )
		{
			NR2G_PlayerLoginNotify packet = _packet as NR2G_PlayerLoginNotify;

			AccountStateManager.Instance.PlayerLogin( packet.account_idx, packet.player_idx, packet.nickname );
		}
	}
}
