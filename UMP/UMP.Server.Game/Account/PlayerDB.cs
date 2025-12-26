//////////////////////////////////////////////////////////////////////////
//
// PlayerDB
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

using UMF.Database;

namespace UMP.Server.Game
{
	//------------------------------------------------------------------------
	[Procedure( "SP_PlayerLogin", eProcedureExecute.Reader )]
	public class SP_PlayerLogin
	{
		public long account_idx;
		public int player_idx;
	}
	public class SP_PlayerLogin_ACK : PROCEDURE_READ_BASE
	{

	}
}
