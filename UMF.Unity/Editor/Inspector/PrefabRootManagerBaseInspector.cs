//////////////////////////////////////////////////////////////////////////
//
// PrefabRootManagerBaseInspector
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
using System.IO;

namespace UMF.Unity.EditorUtil
{
	[CustomEditor(typeof( PrefabRootManagerBase ), true)]
	public class PrefabRootManagerBaseInspector : Editor
	{
		string[] mPrefabNameList = null;
		PrefabRootManagerBase mInstance = null;
		int mSelectedIdx = 0;

		private void OnEnable()
		{
			mInstance = target as PrefabRootManagerBase;
			LoadPrefabNames();
		}

		private void OnDisable()
		{
			mInstance = null;
		}

		void LoadPrefabNames()
		{
			mPrefabNameList = InspectorUtil.GetResourcePrefabNameList( mInstance.m_PrefabResourcePath );
		}

		public override void OnInspectorGUI()
		{
			DrawCustom();
			base.OnInspectorGUI();
		}

		//------------------------------------------------------------------------
		void DrawCustom()
		{
			if( mInstance != null )
			{
				GUILayout.BeginHorizontal();
				if( GUILayout.Button( new GUIContent( "R", "Refresh" ), GUILayout.MaxWidth( 30 ) ) )
				{
					LoadPrefabNames();
				}
				if( GUILayout.Button( "Instantiate" ) )
				{
					DoInstantiatePrefab();
				}

				mSelectedIdx = EditorGUILayout.Popup( mSelectedIdx, mPrefabNameList );
				GUILayout.EndHorizontal();
			}
		}

		//------------------------------------------------------------------------
		void DoInstantiatePrefab()
		{
			if( mSelectedIdx < 0 || mSelectedIdx >= mPrefabNameList.Length )
				return;

			if( mInstance.m_RootParent == null )
			{
				Debug.Log( "RootParent is null" );
				return;
			}

			string p_name = mPrefabNameList[mSelectedIdx];
			InspectorUtil.DoInstantiatePrefab( mInstance.m_PrefabResourcePath, p_name, mInstance.m_RootParent );
		}
	}
}