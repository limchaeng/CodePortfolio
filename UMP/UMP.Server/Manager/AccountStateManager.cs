//////////////////////////////////////////////////////////////////////////
//
// AccountStateManager
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
using UMF.Core;
using System.Collections.Generic;

namespace UMP.Server
{
	//------------------------------------------------------------------------		
	public class AccountStateData
	{
		public long m_AccountIDX = 0;
		public string m_AccountNickname = "";

		public int m_PlayerIDX = 0;
		public string m_PlayerNickname = "";

		public int m_GameServerIDX = 0;
		public int m_GameserverPeerIndex = 0;		
		public int m_GameDBIdx = 0;

		public DateTime m_LoginTime = DateTime.MinValue;
		public DateTime m_LogoutTime = DateTime.MinValue;
	}

	//------------------------------------------------------------------------	
	public class AccountStateManager : Singleton<AccountStateManager>
	{
		Dictionary<long, AccountStateData> mAccountStateByAccountIdx = new Dictionary<long, AccountStateData>();

		//------------------------------------------------------------------------	
		public int CCU()
		{
			return mAccountStateByAccountIdx.Count;
		}

		//------------------------------------------------------------------------
		public AccountStateData FindAccount(long account_idx)
		{
			AccountStateData data;
			if( mAccountStateByAccountIdx.TryGetValue( account_idx, out data ) )
				return data;

			return null;
		}

		//------------------------------------------------------------------------	
		public AccountStateData AccountLogin( long account_idx, int game_server_idx, int peer_idx, string nickname, int gamedb_idx )
		{
			AccountStateData data = FindAccount( account_idx );
			if( data == null )
			{
				data = new AccountStateData();
				data.m_AccountIDX = account_idx;

				mAccountStateByAccountIdx.Add( account_idx, data );
			}

			data.m_AccountNickname = nickname;

			data.m_GameServerIDX = game_server_idx;
			data.m_GameserverPeerIndex = peer_idx;			
			data.m_GameDBIdx = gamedb_idx;

			data.m_LoginTime = DateTime.Now;
			data.m_LogoutTime = DateTime.MinValue;

			return data;
		}

		//------------------------------------------------------------------------
		public AccountStateData PlayerLogin(long account_idx, int player_idx, string player_nickname)
		{
			AccountStateData data = FindAccount( account_idx );
			if( data != null )
			{
				data.m_PlayerIDX = player_idx;
				data.m_PlayerNickname = player_nickname;
			}

			return data;
		}

		//------------------------------------------------------------------------	
		public AccountStateData AccountLogout( long account_idx )
		{
			AccountStateData data = FindAccount( account_idx );
			if( data != null )
			{
				data.m_LogoutTime = DateTime.Now;
			}

			return data;
		}

		//------------------------------------------------------------------------	
		public void UpdateAccountNickname( long account_idx, string new_nickname )
		{
			AccountStateData data = FindAccount( account_idx );
			if( data != null )
			{
				data.m_AccountNickname = new_nickname;
			}
		}
		
		//------------------------------------------------------------------------		
		public void UpdatePlayerNickname( long account_idx, int player_idx, string new_nickname )
		{
			AccountStateData data = FindAccount( account_idx );
			if( data != null && data.m_PlayerIDX == player_idx )
			{
				data.m_PlayerNickname = new_nickname;
			}
		}
	}
}
