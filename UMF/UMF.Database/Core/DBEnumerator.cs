//////////////////////////////////////////////////////////////////////////
//
// DBEnumerator
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

using System.Collections;
using System.Collections.Generic;
using UMF.Core;

namespace UMF.Database
{
	public class DBEnumerator
	{
		Stack<IEnumerator> stacks = new Stack<IEnumerator>();

		IEnumerator m_cur = null;
		DBHandlerObject m_obj = null;

		public DBEnumerator( IEnumerator co, DBHandlerObject obj )
		{
			m_cur = co;
			m_obj = obj;
		}

		public bool MoveNext()
		{
			if( m_cur.MoveNext() == false )
			{
				m_cur = null;
				if( stacks.Count == 0 )
					return false;

				m_cur = stacks.Pop();
			}
			else
			{
				if( m_cur.Current != null )
				{
					if( m_cur.Current is CacheLock )
						m_obj.PushLock( (CacheLock)m_cur.Current );
					else if( m_cur.Current is IEnumerator )
					{
						stacks.Push( m_cur );
						m_cur = (IEnumerator)m_cur.Current;
					}
				}
			}
			return true;
		}
	}
}
