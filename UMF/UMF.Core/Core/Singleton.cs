//////////////////////////////////////////////////////////////////////////
//
// Singleton
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

namespace UMF.Core
{
	public class Singleton<T> where T : class, new()
	{
		static T _Instance = null;
		static object _lock = new object();

		static public T Instance
		{
			get
			{
				if( _Instance == null )
				{
					lock( _lock )
					{
						MakeInstance();
					}
				}

				return _Instance;
			}
		}

		static public void MakeInstance()
		{
			if( _Instance == null )
				_Instance = new T();
		}

		static public void ClearInstance()
		{
			if( _Instance != null )
				_Instance = null;
		}
	}
}