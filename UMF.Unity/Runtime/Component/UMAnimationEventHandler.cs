//////////////////////////////////////////////////////////////////////////
//
// UMAnimationEventHandler
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
	public class UMAnimationEventHandler : MonoBehaviour
	{
		// base event handler

		System.Action mHandler = null;
		public void OnEventCall()
		{
			Debug.Log( $"OnEventCall" );
			mHandler?.Invoke();
		}

		System.Action<int> mHandler_INT = null;
		public void OnEventCall_INT( int v )
		{
			Debug.Log( $"OnEventCall_INT : {v}" );
			mHandler_INT?.Invoke( v );
		}

		System.Action<string> mHandler_STRING = null;			
		public void OnEventCall_STRING( string v )
		{
			Debug.Log( $"OnEventCall_STRING : {v}" );
			mHandler_STRING?.Invoke( v );
		}

		System.Action<float> mHandler_FLOAT = null;
		public void OnEventCall_FLOAT( float v )
		{
			Debug.Log( $"OnEventCall_FLOAT : {v}" );
			mHandler_FLOAT?.Invoke( v );
		}

		//------------------------------------------------------------------------
		public void Clear()
		{
			mHandler = null;
			mHandler_INT = null;
			mHandler_STRING = null;
			mHandler_FLOAT = null;
		}

		//------------------------------------------------------------------------
		// runtime handler set
		public void SetHandler( System.Action handler )
		{
			mHandler = handler;
		}
		public void SetHandler_INT( System.Action<int> handler )
		{
			mHandler_INT = handler;
		}
		public void SetHandler_STRING( System.Action<string> handler )
		{
			mHandler_STRING = handler;
		}
		public void SetHandler_FLOAT( System.Action<float> handler )
		{
			mHandler_FLOAT = handler;
		}
	}
}