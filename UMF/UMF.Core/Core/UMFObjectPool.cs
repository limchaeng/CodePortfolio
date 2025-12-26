//////////////////////////////////////////////////////////////////////////
//
// AbstractFactory
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

using System.Collections.Generic;
using System;
using System.Linq;

namespace UMF.Core
{
	public interface IUMFObjectPoolData
	{
		void Init();
		void UnInit();
	}

	public class UMFObjectPool<T> where T : IUMFObjectPoolData, new()
	{
		Queue<T> mPoolQueue = new Queue<T>();

		Action<T> mOnCreatedCallback = null;
		public Action<T> OnCreatedCallback { set { mOnCreatedCallback = value; } }

		//------------------------------------------------------------------------
		public UMFObjectPool( int pre_count, Action<T> _oncreate_callback = null )
		{
			mOnCreatedCallback = _oncreate_callback;
			for( int i = 0; i < pre_count; i++ )
			{
				Return( Get() );
			}
		}

		//------------------------------------------------------------------------
		public void Destroy()
		{
			mPoolQueue.Clear();
		}

		//------------------------------------------------------------------------
		public T Get()
		{
			if( mPoolQueue.Count > 0 )
			{
				return mPoolQueue.Dequeue();
			}
			else
			{
				T obj = new T();
				obj.Init();
				if( mOnCreatedCallback != null )
					mOnCreatedCallback( obj );

				return obj;
			}
		}

		//------------------------------------------------------------------------
		public void Return( T obj )
		{
			if( obj != null )
			{
				obj.UnInit();
				mPoolQueue.Enqueue( obj );
			}
		}
	}

	//------------------------------------------------------------------------
	// list pool
	public class UMFListPool<T>
	{
		Queue<List<T>> mPoolQueue = new Queue<List<T>>();

		//------------------------------------------------------------------------
		public void Destroy()
		{
			mPoolQueue.Clear();
		}

		//------------------------------------------------------------------------
		public List<T> Get()
		{
			if( mPoolQueue.Count > 0 )
			{
				return mPoolQueue.Dequeue();
			}
			else
			{
				return new List<T>();
			}
		}

		//------------------------------------------------------------------------
		public void Return( List<T> list )
		{
			if( list != null )
			{
				list.Clear();
				mPoolQueue.Enqueue( list );
			}
		}
	}

}
