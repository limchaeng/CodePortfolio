//////////////////////////////////////////////////////////////////////////
//
// SpriteRendererGroup
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
#define UM_TMP_PRESENT

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UM_TMP_PRESENT
using TMPro;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UMF.Unity
{
	[ExecuteInEditMode]
	public class SpriteRendererGroup : MonoBehaviour
	{
		public static SpriteRendererGroup Begin( GameObject go )
		{
			SpriteRendererGroup comp = go.GetComponent<SpriteRendererGroup>();
			if( comp == null )
				comp = go.AddComponent<SpriteRendererGroup>();

			comp.UpdateCache();

			return comp;
		}

		[Range(0f, 1f)]
		public float m_Alpha = 1f;
		[Range( 0f, 1f )]
		public float m_AlphaWithoutIgnore = 1f;

		[Header( "Optional" )]
		public List<GameObject> m_AlphaIgnoreObjects = new List<GameObject>();
		public bool m_ForcedWithIgnore = false;

		bool mIsUpdatedCache = false;
		float mLastAlpha = 0f;
		float mLastAlphaWithout = 0f;

		class CacheDataBase
		{
			public float original_alpha = 1f;
			public bool is_parent_ignored = false;

			public GameObject cached_go = null;

			public virtual void SetAlpha( float alpha ) { }
			public virtual void SetAlphaWithout( float alpha ) { }
			public void CheckIgnored( List<GameObject> ignore_objs )
			{
				is_parent_ignored = false;
				if( cached_go == null || ignore_objs == null || ignore_objs.Count <= 0 )
					return;

				foreach( GameObject ignore_go in ignore_objs )
				{
					if( cached_go == ignore_go )
					{
						is_parent_ignored = true;
					}
					else if( cached_go.transform.IsParent( ignore_go.transform ) )
					{
						is_parent_ignored = true;
					}
				}
			}
		}

		class CacheData<T> : CacheDataBase where T : Component
		{
			public SpriteRenderer sprite = null;
			public T t_component = null;

			public CacheData( T comp, float org_alpha )
			{
				t_component = comp;
				cached_go = t_component.gameObject;
				original_alpha = org_alpha;
			}

			public override void SetAlpha( float alpha )
			{
				if( t_component is SpriteRenderer )
				{
					SpriteRenderer sprite = t_component as SpriteRenderer;
					Color color = sprite.color;
					color.a = original_alpha * alpha;
					sprite.color = color;
				}

#if UM_TMP_PRESENT
				if( t_component is TMP_Text )
				{
					TMP_Text tmp_text = t_component as TMP_Text;
					tmp_text.alpha = original_alpha * alpha;
				}
#endif
			}

			public override void SetAlphaWithout( float alpha )
			{
				if( is_parent_ignored == false )
					SetAlpha( alpha );
			}
		}

		List<CacheDataBase> mCachedDataList = new List<CacheDataBase>();

		//------------------------------------------------------------------------		
		private void Awake()
		{
			UpdateCache();
		}

		//------------------------------------------------------------------------
		[ContextMenu("Reset Cache")]
		public void UpdateCache()
		{
			mCachedDataList.Clear();

			SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
			if( sr != null )
				mCachedDataList.Add( AddCacheData( sr, sr.color.a ) );

			SpriteRenderer[] sr_childs = gameObject.GetComponentsInChildren<SpriteRenderer>( true );
			foreach( SpriteRenderer child in sr_childs )
			{
				mCachedDataList.Add( AddCacheData( child, child.color.a ) );
			}

#if UM_TMP_PRESENT
			TMP_Text tmp = gameObject.GetComponent<TMP_Text>();
			if( tmp != null )
				mCachedDataList.Add( AddCacheData( tmp, tmp.alpha ) );

			TMP_Text[] tmp_childs = gameObject.GetComponentsInChildren<TMP_Text>( true );
			foreach( TMP_Text child in tmp_childs )
			{
				mCachedDataList.Add( AddCacheData( child, child.alpha ) );
			}
#endif
			mCachedDataList.ForEach( a => a.CheckIgnored( m_AlphaIgnoreObjects ) );

			mIsUpdatedCache = true;
		}

		//------------------------------------------------------------------------
		CacheDataBase AddCacheData<T>( T comp, float org_alpha ) where T : Component
		{
			CacheDataBase exist = mCachedDataList.Find( a => a.cached_go == comp.gameObject );
			if( exist == null )
				exist = new CacheData<T>( comp, org_alpha );

			return exist;
		}

		//------------------------------------------------------------------------
		public float alpha
		{
			get { return m_Alpha; }
			set
			{
				m_Alpha = value;
				if( m_ForcedWithIgnore == false )
					mCachedDataList.ForEach( a => a.SetAlpha( m_Alpha ) );
				else
					mCachedDataList.ForEach( a => a.SetAlphaWithout( m_Alpha ) );
			}
		}

		public float alpha_without
		{
			get { return m_AlphaWithoutIgnore; }
			set
			{
				m_AlphaWithoutIgnore = value;
				mCachedDataList.ForEach( a => a.SetAlphaWithout( m_AlphaWithoutIgnore ) );
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			OnDidApplyAnimationProperties();
		}
#endif

		void OnDidApplyAnimationProperties()
		{
			if( mIsUpdatedCache == false )
				UpdateCache();

			if( mLastAlpha != m_Alpha )
			{
				mLastAlpha = m_Alpha;

				if( m_ForcedWithIgnore == false )
					mCachedDataList.ForEach( a => a.SetAlpha( m_Alpha ) );
				else
					mCachedDataList.ForEach( a => a.SetAlphaWithout( m_Alpha ) );
			}
			else if( mLastAlphaWithout != m_AlphaWithoutIgnore )
			{
				mLastAlphaWithout = m_AlphaWithoutIgnore;
				mCachedDataList.ForEach( a => a.SetAlphaWithout( m_AlphaWithoutIgnore ) );
			}
		}
	}

#if UNITY_EDITOR
	[CustomEditor( typeof( SpriteRendererGroup ) )]
	public class SpriteRendererGroupInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			SpriteRendererGroup srg = target as SpriteRendererGroup;

			Color gui_color = GUI.color;
			GUI.color = Color.green;
			if( GUILayout.Button( "Reset Cache" ) )
			{
				srg.UpdateCache();
			}
			GUI.color = gui_color;

			base.OnInspectorGUI();
		}
	}
#endif
}
