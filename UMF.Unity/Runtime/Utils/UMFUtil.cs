//////////////////////////////////////////////////////////////////////////
//
// UMFUtil
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
using System.Linq;
using UnityEngine;

namespace UMF.Unity
{
	public static class UMFUtil
	{
		//------------------------------------------------------------------------
		public static Vector3 Vector3Random( Vector3 min, Vector3 max )
		{
			return new Vector3( Random.Range( min.x, max.x ), Random.Range( min.y, max.y ), Random.Range( min.z, max.z ) );
		}

		//------------------------------------------------------------------------
		public static Vector2 Vector2Random( Vector2 min, Vector2 max )
		{
			return new Vector2( Random.Range( min.x, max.x ), Random.Range( min.y, max.y ) );
		}

		//------------------------------------------------------------------------
		public class IndexedCoroutineManager
		{
			public class Data
			{
				public int index;
				public Coroutine routine;

				public Data( int _index, Coroutine _routine )
				{
					this.index = _index;
					this.routine = _routine;
				}
			}

			int mUniqueIndex = 0;
			List<Data> mDataList = new List<Data>();

			//------------------------------------------------------------------------
			public int UniqueIndex
			{
				get
				{
					mUniqueIndex += 1;
					if( mUniqueIndex >= int.MaxValue )
						mUniqueIndex = 1;

					return mUniqueIndex;
				}
			}

			//------------------------------------------------------------------------
			public bool HasCoroutine
			{
				get { return mDataList.Count > 0; }
			}

			//------------------------------------------------------------------------
			public void Add( int idx, Coroutine routine )
			{
				mDataList.Add( new Data( idx, routine ) );
			}

			//------------------------------------------------------------------------
			public void Stop( int idx, MonoBehaviour mono )
			{
				Data exist = mDataList.Find( a => a.index == idx );
				if( exist != null )
				{
					mono.StopCoroutine( exist.routine );
					mDataList.Remove( exist );
				}
			}

			//------------------------------------------------------------------------
			public void StopAll( MonoBehaviour mono )
			{
				mono.StopAllCoroutines();
				mDataList.Clear();
			}
		}

		//------------------------------------------------------------------------
		static string[] _all_layer_names = null;
		public static string GetLayerMaskNames( int layer_mask )
		{
            string ret = "";
            for( int i = 0; i < 32; i++ )
            {
                if( ( layer_mask & ( 1 << i ) ) != 0 )
                {
					if( ret.Length > 0 )
						ret += "|";
					ret += LayerMask.LayerToName( i );
                }
            }

			return ret;
        }
	}
}
