//////////////////////////////////////////////////////////////////////////
//
// TransformExtensions
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
	public static class TransformExtensions
	{
		//------------------------------------------------------------------------		
		public static void SetTransform( this Transform trans, Transform dest )
		{
			trans.localPosition = dest.position;
			trans.localScale = dest.localScale;
			trans.localRotation = dest.localRotation;
		}
		
		//------------------------------------------------------------------------		
		public static void SetUniform( this Transform trans, GameObject parent = null )
		{
			if( parent != null )
				trans.SetParent( parent.transform );				

			trans.localPosition = Vector3.zero;
			trans.localScale = Vector3.one;
			trans.localRotation = Quaternion.identity;
			trans.gameObject.layer = parent.layer;
		}

		//------------------------------------------------------------------------
		static void _GetChildrenList( Transform t, bool include_inactive, bool inclide_sub_children, ref List<Transform> list )
		{
			int child_count = t.childCount;
			for( int i = 0; i < child_count; ++i )
			{
				Transform child = t.GetChild( i );
				if( include_inactive || child.gameObject.activeInHierarchy == true )
				{
					list.Add( child );
				}

				if( inclide_sub_children )
					_GetChildrenList( child, include_inactive, inclide_sub_children, ref list );
			}
		}

		//------------------------------------------------------------------------
		public static List<Transform> GetChildrenList( this Transform trans, bool include_inactive, bool inclide_sub_children )
		{
			List<Transform> list = new List<Transform>();
			_GetChildrenList( trans, include_inactive, inclide_sub_children, ref list );
			return list;
		}

		//------------------------------------------------------------------------
		public static bool IsParent( this Transform trans, Transform parent)
		{
			if( trans.parent == null )
				return false;

			if( trans.parent == parent )
				return true;

			return trans.parent.IsParent( parent );
		}

		//------------------------------------------------------------------------
		public static List<Transform> GetParentList( this Transform trans, System.Type break_type = null )
		{
			List<Transform> list = null;

			_GetParentList( trans, break_type, ref list );

			return list;
		}
		static void _GetParentList( Transform t, System.Type break_type, ref List<Transform> list )
		{
			Transform pt = t.parent;
			if( pt != null )
			{
				if( list == null )
					list = new List<Transform>();

				list.Add( pt );

				if( break_type == null || pt.GetComponent( break_type ) == null )
					_GetParentList( pt, break_type, ref list );
			}
		}

		//------------------------------------------------------------------------
		public static void SetPosition( this Transform t, Vector3 position, bool ignore_z )
		{
			if( ignore_z )
			{
				Vector3 vpos = t.position;
				vpos.x = position.x;
				vpos.y = position.y;
				t.position = vpos;
			}
			else
			{
				t.position = position;
			}
		}
    }
}
