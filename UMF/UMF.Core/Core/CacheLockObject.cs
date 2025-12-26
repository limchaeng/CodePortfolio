//////////////////////////////////////////////////////////////////////////
//
// CacheLockObject
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
using System.Collections;
using System.Diagnostics;

namespace UMF.Core
{
	//------------------------------------------------------------------------	
	public class CacheLockObject
	{
		Queue<CacheLock> locks = new Queue<CacheLock>();

		public bool ContainsLock { get { return locks.Count > 0; } }

		public void CheckErrorContainsLock()
		{
			if( ContainsLock == true )
			{
				Log.WriteError( "ContainsLock{0}", locks.Peek().stack_frames );
			}
		}

		public void AddLock( CacheLock dblock )
		{
			locks.Enqueue( dblock );
			UpdateLock();
		}

		public void RemoveLock( CacheLock dblock )
		{
#if DEBUG
            if (locks.Peek() != dblock)
                throw new System.Exception("RemoveLock fault in " + dblock.LockObjectType);
#endif
			locks.Dequeue();

			UpdateLock();
		}

		void UpdateLock()
		{
			if( locks.Count > 0 )
				locks.Peek().SetLock();
		}
	}

	//------------------------------------------------------------------------	
	public class CacheLock : IDisposable
	{
		CacheLockObject m_LockObject;
		bool m_Lock = false;
		public void SetLock() { m_Lock = true; }

		public Type LockObjectType { get { return m_LockObject.GetType(); } }

		StackTrace stackTrace = null;
		public string stack_frames
		{
			get
			{
				if( stackTrace == null )
					return "";
				return stackTrace.ToString();
			}
		}

		public IEnumerator GetLock( bool use_timeout, int timeout_seconds )
		{
			DateTime timeout = DateTime.Now.AddSeconds( timeout_seconds );
			while( m_Lock == false )
			{
				if( use_timeout == true && DateTime.Now > timeout )
					throw new System.Exception( "GetLock Timeout in " + LockObjectType );

				yield return null;
			}
			yield return this;
		}

		object[] param = null;
		void SetLockObject( CacheLockObject lockObject, params object[] param )
		{
#if DEBUG
            stackTrace = new StackTrace(2, true);           // get call stack
			this.param = param;
#endif

			m_LockObject = lockObject;
			m_LockObject.AddLock( this );
		}

		public CacheLock()
		{
		}

		public CacheLock( CacheLockObject lockObject, params object[] param )
		{
			SetLockObject( lockObject, param );
		}

		public void Dispose()
		{
			if( m_LockObject != null )
			{
				m_LockObject.RemoveLock( this );
				m_LockObject = null;
			}
		}
	}
}