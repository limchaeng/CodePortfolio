//////////////////////////////////////////////////////////////////////////
//
// DBHandlerObject
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
using System.Collections;
using System.Collections.Generic;
using UMF.Core;

namespace UMF.Database
{
	public class DBHandlerObject
	{
		protected List<CacheLock> locks = new List<CacheLock>();

		public void PushLock( CacheLock dblock )
		{
			locks.Add( dblock );
		}

		public void Unlock()
		{
			while( locks.Count > 0 )
			{
				locks[0].Dispose();
				locks.RemoveAt( 0 );
			}
		}

		public DatabaseMain database;
		public PROCEDURE_READ_BASE readObject;
		public long recvIndex;
		public int procedureIndex;
		public int procedureIndexInternal = 0;
		public bool done = false;
		public ISqlCommand sql_command;
		public IEnumerator procedure_handler;
		public DBHandlerExecute.CallbackProcedure callback;
		public object[] callback_data;
		public object packet;
		//
		public DBEnumerator session_packet_handler;
		//
		public bool dberror_logging = false;

		//------------------------------------------------------------------------
		public virtual void ThrowPacketException( Exception ex ) { }
	}
}
