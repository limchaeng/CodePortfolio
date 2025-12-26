//////////////////////////////////////////////////////////////////////////
//
// WaitForFinish
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
using UnityEngine;

namespace UMF.Unity
{
	public class WaitForFinish : IEnumerator
	{
		static Queue<WaitForFinish> mWaitQueue = new Queue<WaitForFinish>();

		//------------------------------------------------------------------------		
		public static WaitForFinish POP()
		{
			if( mWaitQueue.Count > 0 )
			{
				WaitForFinish wait = mWaitQueue.Dequeue();
				wait.Reset();
				return wait;
			}

			return new WaitForFinish();
		}

		//------------------------------------------------------------------------		
		public static void Release( WaitForFinish wait )
		{
			if( wait == null )
				return;

			wait.Reset();
			mWaitQueue.Enqueue( wait );
		}

		public delegate void delegateWaitState( int waitCount, float waitTime );
		delegateWaitState mOnWaitHandler = null;
		int mWaitCount = 0;
		public float WAITING_TIMEOUT = 30f;
		public bool IgnoreWaitingTimeout { get; set; } = false;
		public void SetIgnoreWaitingTimeout() { IgnoreWaitingTimeout = true; }
        public void UnSetIgnoreWaitingTimeout() { IgnoreWaitingTimeout = false; }


        float mInternalWaitingTime = 0f;
		List<float> mWaitTimeList = new List<float>();
		float mWaitTime = 0f;
		public bool ignoreTimeScale { get; set; } = false;

		List<IEnumerator> mTempEnumeratorList = new List<IEnumerator>();
		List<IEnumerator> mEnumeratorList = new List<IEnumerator>();

#if UNITY_EDITOR
		Stack<string> mDebugCallstack = new Stack<string>();
#endif
		Dictionary<int, System.Action> mDecrementCallbackDic = new Dictionary<int, System.Action>();

		//------------------------------------------------------------------------
		public void SetHandler( delegateWaitState waitHandler )
		{
			mOnWaitHandler = waitHandler;
		}

        //------------------------------------------------------------------------
        public void IncrementWaitCount( System.Action decrement_callback, int n = 0 )
		{
			if( mDecrementCallbackDic.ContainsKey( mWaitCount ) )
				mDecrementCallbackDic[mWaitCount] = decrement_callback;
			else
				mDecrementCallbackDic.Add( mWaitCount, decrement_callback );

			IncrementWaitCount( n );
		}

        public void IncrementWaitCount( int n = 0)
		{
			if( n > 0 )
				mWaitCount += n;
			else
				mWaitCount++;

#if UNITY_EDITOR
			System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
			mDebugCallstack.Push( st.ToString() );
#endif
			CallbackState();
		}

		//------------------------------------------------------------------------		
		public void DecrementWaitCount()
		{
			mWaitCount--;

			if( mDecrementCallbackDic.ContainsKey( mWaitCount ) )
			{
				System.Action callback = mDecrementCallbackDic[mWaitCount];
				mDecrementCallbackDic.Remove( mWaitCount );
				if( callback != null )
					callback();
			}

#if UNITY_EDITOR
			if( mDebugCallstack.Count > 0 )
			{
				mDebugCallstack.Pop();
			}
#endif

			if( mWaitCount < 0 )
				mWaitCount = 0;

			CallbackState();
		}

		//------------------------------------------------------------------------
		public void AddWaitTime( float add_time )
		{
			if( add_time <= 0f )
				return;

			mWaitTimeList.Add( add_time );

			float end_time = add_time;
			if( end_time > mWaitTime )
				mWaitTime = end_time;

			CallbackState();
		}

        //------------------------------------------------------------------------
        // sub process 의 wait 은 yield return wait 이 적용되지 않음
        // IEnumerator 내부의 wait 은 wait.MoveNext() 로 대기해야함
        public void AddEnumerator( IEnumerator enumerator )
		{
			mEnumeratorList.Add( enumerator );
		}

		//------------------------------------------------------------------------
		public void OnFinished()
		{
#if UNITY_EDITOR
			if( mDebugCallstack.Count > 0 )
			{
				mDebugCallstack.Pop();
				//Debug.LogWarning( string.Format( "RES COUNT:{0} - {1}", mWaitCount, mDebugCallstack.Pop() ) );
			}
#endif

			mWaitCount--;

			if( mWaitCount < 0 )
				mWaitCount = 0;

			CallbackState();
		}

		//------------------------------------------------------------------------
		public void CallbackState()
		{
			if( mOnWaitHandler != null )
				mOnWaitHandler( mWaitCount, mWaitTime );
		}

		//------------------------------------------------------------------------		
		public object Current => null;

		public bool MoveNext()
		{
			if( mEnumeratorList.Count > 0 )
			{
				mTempEnumeratorList.Clear();
				foreach( IEnumerator process in mEnumeratorList )
				{
					if( process.MoveNext() )
						mTempEnumeratorList.Add( process );
				}
				mEnumeratorList.Clear();

				if( mTempEnumeratorList.Count > 0 )
				{
					mEnumeratorList.AddRange( mTempEnumeratorList );
					return true;
				}
			}

			if( mWaitCount <= 0 && mWaitTime <= 0f )
			{
				Reset();
				return false;
			}

			if( mWaitTime > 0f )
			{
				mWaitTime -= ( ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime );
			}
			else if( mWaitCount > 0 )
			{
				mInternalWaitingTime += Time.unscaledDeltaTime;
				if( IgnoreWaitingTimeout == false && mInternalWaitingTime > WAITING_TIMEOUT )
				{
					Debug.LogWarning( string.Format( "???????? Waiting time over {0} seconds!", WAITING_TIMEOUT ) );
#if UNITY_EDITOR
					System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
					Debug.LogWarning( st.ToString() );

					while( mDebugCallstack.Count > 0 )
					{
						string callstack = string.Format( ":::{0}\n{1}", mDebugCallstack.Count, mDebugCallstack.Pop() );
						Debug.LogWarning( callstack );
					}
#endif
					Reset();
					return false;
				}
			}

			return true;
		}

		//------------------------------------------------------------------------		
		public void Reset()
		{
			WAITING_TIMEOUT = 30f;
			mWaitCount = 0;
			mWaitTime = 0f;
			mWaitTimeList.Clear();
			ignoreTimeScale = false;
			mInternalWaitingTime = 0f;
			IgnoreWaitingTimeout = false;
			mEnumeratorList.Clear();
			mTempEnumeratorList.Clear();
			mDecrementCallbackDic.Clear();
#if UNITY_EDITOR
			mDebugCallstack.Clear();
#endif
		}
	}
}

