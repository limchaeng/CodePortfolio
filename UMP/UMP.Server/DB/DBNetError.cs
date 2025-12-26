//////////////////////////////////////////////////////////////////////////
//
// DBNetError
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
using UMF.Net;
using UMF.Database;

namespace UMP.Server
{
	//------------------------------------------------------------------------		
	public class DBErrorException : PacketException
	{
		public DBErrorException( int db_return_code )
			: base( (int)eDisconnectErrorCode.DatabaseError, DBErrorShared.GetErrorString( db_return_code ) )
		{
		}
	}

	//------------------------------------------------------------------------		
	public class PeerDBErrorException : PeerDisconnectException
	{
		public PeerDBErrorException( int userIndex, int db_return_code )
			: base( userIndex, (int)eDisconnectErrorCode.DatabaseError, DBErrorShared.GetErrorString( db_return_code ) )
		{
		}
	}
}
