//////////////////////////////////////////////////////////////////////////
//
// PrefabRootManagerBase
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
	//------------------------------------------------------------------------
	public class PrefabRootManagerBase : MonoBehaviour
	{
		[Header( "[PrefabRootManagerBase]" )]
		public string m_PrefabResourcePath = "";
		public GameObject m_RootParent;

		//------------------------------------------------------------------------
		public T Load<T>( string _name, GameObject custom_new_parent = null ) where T : PrefabRootBehaviour
		{
			GameObject prefab = Resources.Load<GameObject>( string.Format( "{0}/{1}", m_PrefabResourcePath, _name ) );
			if( prefab == null )
			{
				Debug.LogWarning( $"Prefab load failed : prefab({_name}) can not load!" );
				return null;
			}

			GameObject parent = m_RootParent;
			if( custom_new_parent != null )
				parent = custom_new_parent;

			GameObject go = Instantiate( prefab, parent.transform );
			go.transform.SetUniform( parent );

			T comp = go.GetComponent<T>();
			if( comp == null )
			{
				Debug.LogWarning( $"Prefab load failed : prefab({_name}) without {typeof( T ).Name}" );
				DestroyGameobject( go );
			}

			prefab = null;
			return comp;
		}

		//------------------------------------------------------------------------
		public void Unload( PrefabRootBehaviour prefab_root )
		{
			if( prefab_root == null )
				return;

			prefab_root.P_PreUnInit();
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

	//------------------------------------------------------------------------
	public class TPrefabRootManagerBase<MT> : PrefabRootManagerBase where MT : MonoBehaviour
	{
		protected static MT _instance = null;
		public static MT Instance
		{
			get
			{
				if( _instance == null )
				{
#if UNITY_6000_0_OR_NEWER
					_instance = FindFirstObjectByType<MT>( FindObjectsInactive.Include );
#else
					_instance = FindObjectOfType<MT>();
#endif
				}

				if( _instance == null )
					Debug.LogError( $"{typeof( MT ).Name} instancing failed!" );

				return _instance;
			}
		}

		protected virtual void Awake()
		{
			_instance = this as MT;
		}

		//------------------------------------------------------------------------
		public virtual void Init() { }
	}
}
