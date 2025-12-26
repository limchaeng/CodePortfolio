//////////////////////////////////////////////////////////////////////////
//
// ManagerInstance
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
	public class SingletonBehaviourBase : MonoBehaviour
	{

	}


    public class SingletonBehaviour<T> : SingletonBehaviourBase where T : MonoBehaviour
	{
		protected static T _instance = null;
		protected static object _lock = new object();

		public static T Instance
		{
			get
			{
				lock( _lock )
				{
					if( _instance == null )
					{
#if UNITY_6000_0_OR_NEWER
						_instance = FindFirstObjectByType<T>( FindObjectsInactive.Include );
#else
						_instance = FindObjectOfType<T>();
#endif
					}

					if( _instance == null )
					{
						Debug.LogWarning( $"{typeof( T ).Name} instancing is null!" );
					}

					return _instance;
				}
			}
		}
		

		protected virtual void Awake()
		{
			_instance = this as T;
		}
	}
}
