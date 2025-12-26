//////////////////////////////////////////////////////////////////////////
//
// ConnectionKeyManager
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
using System.Collections.Generic;
using System;
using UMF.Server;

namespace UMP.Server
{
	public class ConnectionKeyManager : Singleton<ConnectionKeyManager>
	{
		Queue<long> mKeyQueue = new Queue<long>();

		int mUpdateInterval = 30;
		int mMaxKeyQueue = 10;
		bool mCheckConnectionKey = true;

		DateTime mNextUpdateTime = DateTime.MinValue;
		bool mInit = false;

		public long LastConnectionKey { get; private set; }

		Action<long> _OnUpdatedConnectionKey = null;
		public Action<long> OnUpdatedConnectionKey { set { _OnUpdatedConnectionKey = value; } }

		//------------------------------------------------------------------------
		public ConnectionKeyManager()
		{
			LastConnectionKey = GenerateConnectionKey();
		}

		//------------------------------------------------------------------------	
		public void UpdateConfig(int interval, int max_key_queue, bool connection_key_check)
		{
			mInit = true;
			mUpdateInterval = interval;
			mMaxKeyQueue = max_key_queue;
			mCheckConnectionKey = connection_key_check;
			mNextUpdateTime = DateTime.Now.AddSeconds( mUpdateInterval );

			UpdateConnectionInfo();
		}

		//------------------------------------------------------------------------	
		public void Update()
		{
			if( DateTime.Now >= mNextUpdateTime )
			{
				UpdateConnectionInfo();
			}
		}

		//------------------------------------------------------------------------	
		public void UpdateConnectionInfo()
		{
			if( mInit == false )
				return;

			long connection_key = GenerateConnectionKey();
			LastConnectionKey = connection_key;
			mNextUpdateTime = DateTime.Now.AddSeconds( mUpdateInterval );

			if( _OnUpdatedConnectionKey != null )
				_OnUpdatedConnectionKey( connection_key );
		}

		//------------------------------------------------------------------------		
		long GenerateConnectionKey()
		{
			long connection_key = UMFRandom.Instance._Next();
			do
			{
				connection_key = UMFRandom.Instance._Next();
			} while( connection_key == 0 || mKeyQueue.Contains( connection_key ) );

			mKeyQueue.Enqueue( connection_key );
			if( mKeyQueue.Count > mMaxKeyQueue )
				mKeyQueue.Dequeue();

			return connection_key;
		}

		//------------------------------------------------------------------------		
		public bool CheckConnectionKey( long key )
		{
			return ( mCheckConnectionKey == false || mKeyQueue.Contains( key ) );
		}
	}
}
