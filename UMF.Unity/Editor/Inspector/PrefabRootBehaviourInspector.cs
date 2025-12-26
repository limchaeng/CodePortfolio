//////////////////////////////////////////////////////////////////////////
//
// PrefabRootBehaviourInspector
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
using UnityEditor;

namespace UMF.Unity.EditorUtil
{
	[CustomEditor( typeof( PrefabRootBehaviour ), true )]
	public class PrefabRootBehaviourInspector : Editor
	{
		bool mDrawPrefabControl = true;
		bool mIsRemoved = false;

		private void OnEnable()
		{
			mDrawPrefabControl = true;

			PrefabRootBehaviour root = target as PrefabRootBehaviour;
			if( root == null )
				return;

			PrefabRootBehaviour[] comps = root.gameObject.GetComponents<PrefabRootBehaviour>();
			for(int i=0; i<comps.Length; i++ )
			{
				if( comps[i] == root )
				{
					if( i == 0 )
						mDrawPrefabControl = true;
					else
						mDrawPrefabControl = false;

					break;
				}				
			}

			PostOnEnable();
		}

		public virtual void PostOnEnable() { }

		public override void OnInspectorGUI()
		{
			PrefabRootBehaviour prefab_root = target as PrefabRootBehaviour;
			if( mDrawPrefabControl )
				InspectorUtil.Draw_PrefabControl( prefab_root.gameObject, OnPrefabSaved );
			else
				GUILayout.Label( "=== Hided prefab control!" );

			if( mIsRemoved == false )
			{
				PreDraw();
				prefab_root.m_EditorBaseInspectorExpand = InspectorUtil.DrawHeaderFoldable( "Base Inspector", prefab_root.m_EditorBaseInspectorExpand );
				if( prefab_root.m_EditorBaseInspectorExpand )
				{
					base.OnInspectorGUI();
				}
				PostDraw();
			}
		}

		public virtual void PreDraw() { }
		public virtual void PostDraw() { }
		public virtual void OnPrefabSaved(bool is_removed)
		{
			mIsRemoved = is_removed;
		}
	}
}
