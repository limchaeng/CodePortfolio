//////////////////////////////////////////////////////////////////////////
//
// PrefabManager
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
using UnityEngine;

namespace UMF.Unity
{
	public class PrefabManager : SingletonBehaviour<PrefabManager>
	{
		[System.Serializable]
		public class ManagedPrefabData
		{
			public string m_ResourcePath;
			public GameObject m_RootParent;
		}

		public List<ManagedPrefabData> m_ManagedPrefabList = new List<ManagedPrefabData>();

		//------------------------------------------------------------------------
		public T ManagedLoad<T>( string _name ) where T : PrefabRootBehaviour
		{
			return ManagedLoad<T>( _name, null );
		}
		public T ManagedLoad<T>( string _name, GameObject use_parent ) where T : PrefabRootBehaviour
		{
			GameObject prefab = null;
			ManagedPrefabData managed_data = null;
			foreach( ManagedPrefabData managed in m_ManagedPrefabList )
			{
				prefab = Resources.Load<GameObject>( $"{managed.m_ResourcePath}/{_name}" );
				if( prefab != null )
				{
					managed_data = managed;
					break;
				}
			}

			if( prefab == null )
			{
				Debug.LogWarning( $"Prefab load failed : prefab({_name}) can not load!" );
				return null;
			}

			GameObject _parent = use_parent;
			if( _parent == null )
				_parent = managed_data.m_RootParent;

			GameObject go = GameObject.Instantiate( prefab, _parent.transform );
			go.transform.SetUniform( _parent );

			T comp = go.GetComponent<T>();
			if( comp == null )
			{
				Debug.LogWarning( $"Prefab managed load failed : prefab({_name}) without {typeof( T ).Name}" );
				DestroyGameobject( go );
				return null;
			}

			return comp;
		}

		//------------------------------------------------------------------------
		public T Load<T>( string _res_path, string _name, GameObject use_parent ) where T : PrefabRootBehaviour
		{
			GameObject prefab = Resources.Load<GameObject>( $"{_res_path}/{_name}" );
			if( prefab == null )
			{
				Debug.LogWarning( $"Prefab load failed : prefab({_res_path}/{_name}) can not load!" );
				return null;
			}

			GameObject _parent = use_parent;
			GameObject go = GameObject.Instantiate( prefab, _parent.transform );
			go.transform.SetUniform( _parent );

			T comp = go.GetComponent<T>();
			if( comp == null )
			{
				Debug.LogWarning( $"Prefab load failed : prefab({_name}) without {typeof( T ).Name}" );
				DestroyGameobject( go );
				return null;
			}

			return comp;
		}

		//------------------------------------------------------------------------
		public void Unload( PrefabRootBehaviour prefab_root )
		{
			if( prefab_root == null )
				return;

			prefab_root.P_UnInit();

			DestroyGameobject( prefab_root.gameObject );
		}

		//------------------------------------------------------------------------
		public void DestroyGameobject( GameObject go )
		{
			if( Application.isPlaying )
				GameObject.Destroy( go );
			else
				GameObject.DestroyImmediate( go );
		}
	}
}