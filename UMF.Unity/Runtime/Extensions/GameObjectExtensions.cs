//////////////////////////////////////////////////////////////////////////
//
// GameObjectExtensions
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
	public static class GameObjectExtensions
	{
        //------------------------------------------------------------------------
        public static T GetOrCreateCached<T>( int idx, List<T> cached_list, T src, GameObject parent = null ) where T : Component
        {
            if( idx < cached_list.Count )
                return cached_list[idx];

			T child = null;
			if( parent != null )
                child = parent.AddChildWithComponent<T>( src.gameObject );
			else
                child = src.transform.parent.gameObject.AddChildWithComponent<T>( src.gameObject );

            cached_list.Add( child );

            return child;
        }

        //------------------------------------------------------------------------
        public static void SetParent( this Component go, Component parent, bool deactive = false, bool worldposition_stay = false )
		{
			SetParent( go.gameObject, parent.gameObject, deactive, worldposition_stay );
		}
        public static void SetParent( this GameObject go, GameObject parent, bool deactive = false, bool worldposition_stay = false )
		{
			go.transform.SetParent( parent.transform );

            if( worldposition_stay == false )
            {
                go.transform.localPosition = Vector3.zero;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.transform.gameObject.layer = parent.layer;
            }

            if( deactive )
			{
                go.gameObject.SetActive( false );
			}
		}

        //------------------------------------------------------------------------
        public static GameObject AddChildName( this GameObject parent, string child_name )
		{
			GameObject go = parent.AddChild( null );
			go.name = child_name;

			return go;
		}
		public static GameObject AddChild( this GameObject parent )
		{
			return parent.AddChild( null );
		}
		public static GameObject AddChild( this GameObject parent, GameObject source )
		{
			GameObject new_go = null;
			if( source != null )
				new_go = GameObject.Instantiate( source, parent.transform );
			else
				new_go = new GameObject( $"{parent.name}_child" );

			new_go.transform.SetParent( parent.transform );
			new_go.transform.localPosition = Vector3.zero;
			new_go.transform.localScale = Vector3.one;
			new_go.transform.localRotation = Quaternion.identity;
			new_go.transform.gameObject.layer = parent.layer;

			return new_go;
		}

		//------------------------------------------------------------------------
		public static T AddChildWithComponent<T>( this GameObject parent ) where T : Component
		{
			GameObject new_go = parent.AddChild();

			T component =  new_go.GetComponent<T>();
			if( component == null )
				component = new_go.AddComponent<T>();

			return component;
		}
        public static T AddChildWithComponent<T>( this GameObject parent, T source ) where T : Component
		{
			return parent.AddChildWithComponent<T>( source.gameObject );
		}
        public static T AddChildWithComponent<T>( this GameObject parent, GameObject source ) where T : Component
		{
			GameObject new_go = parent.AddChild( source );

			T component =  new_go.GetComponent<T>();
			if( component == null )
				component = new_go.AddComponent<T>();

			return component;
		}

		//------------------------------------------------------------------------
		public static List<T> GetComponentsInChildrenIncludeThis<T>( this GameObject go, bool include_inactive = true ) where T : Component
		{
			List<T> list = null;
			//if( go.activeSelf || include_inactive )
			//{
			//	T this_comp = go.GetComponent<T>();
			//	if( this_comp != null )
			//	{
			//		if( list == null )
			//			list = new List<T>();

			//		list.Add( this_comp );
			//	}
			//}

			T[] comp_list = go.GetComponentsInChildren<T>( include_inactive );
			if( comp_list != null && comp_list.Length > 0 )
			{
				if( list == null )
					list = new List<T>();

				foreach( T comp in comp_list )
					list.Add( comp );
			}

			return list;
		}

		//------------------------------------------------------------------------
		public static List<GameObject> GetChildrenList( this GameObject go, bool include_inactive, bool inclide_sub_children )
        {
			List<Transform> children_list = go.transform.GetChildrenList( include_inactive, inclide_sub_children );

			return children_list.Select( a => a.gameObject ).ToList();
		}

		//------------------------------------------------------------------------
		public static string GetHierarchyPath( this GameObject go )
		{
			string h_name = go.name;
			_GetHierarchyName( go, ref h_name );

			return h_name;
		}
		static void _GetHierarchyName( GameObject go, ref string h_name )
		{
			if( go.transform.parent != null )
			{
				h_name = go.transform.parent.name + "/" + h_name;
				_GetHierarchyName( go.transform.parent.gameObject, ref h_name );
			}
		}

        //------------------------------------------------------------------------
        public static void ConvertLayerRecursive( GameObject go, int convert_layer, int check_layer = -1 )
        {
            if( check_layer == -1 || go.layer == check_layer )
                go.layer = convert_layer;

            foreach( Transform child in go.transform )
            {
                ConvertLayerRecursive( child.gameObject, convert_layer, check_layer );
            }
        }

        //------------------------------------------------------------------------
        public class BlinkUtil
        {
            Coroutine _blink_routine = null;
            bool _blink_on = false;

			MonoBehaviour mMono;
			System.Action<bool> mOnBlinkCallback = null;
			float mTimeout = -1f;

            public BlinkUtil( MonoBehaviour p_mono )
			{
				mMono = p_mono;
			}

			public void BeginBlink( System.Action<bool> callback, float interval = 0.5f, float timeout = -1f )
            {
				mTimeout = timeout;
                StopBlink();
				mOnBlinkCallback = callback;
                _blink_routine = mMono.StartCoroutine( DoBlink( interval ) );
            }

            public void StopBlink()
            {
                _blink_on = false;
                if( _blink_routine != null )
                {
                    mMono.StopCoroutine( _blink_routine );
                    _blink_routine = null;
					mOnBlinkCallback?.Invoke( false );
                }
            }

            IEnumerator DoBlink( float _interval )
            {
                float interval = _interval;
                float time = interval;
				float timeout = mTimeout;

                while( true )
                {
                    yield return null;

					if( timeout > 0f )
					{
						timeout -= Time.deltaTime;
						if( timeout <= 0f )
						{
							StopBlink();
							time = 0f;
							break;
						}
					}

                    time -= Time.deltaTime;
                    if( time <= 0f )
                    {
                        time = interval;

                        if( _blink_on )
                        {
                            mOnBlinkCallback?.Invoke( true );
                        }
                        else
                        {
                            mOnBlinkCallback?.Invoke( false );
                        }

                        _blink_on = !_blink_on;
                    }
                }
            }
        }
    }
}
