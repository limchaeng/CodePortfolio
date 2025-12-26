//////////////////////////////////////////////////////////////////////////
//
// PrefabManagerInspector
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
	[CustomEditor(typeof(PrefabManager))]
	public class PrefabManagerInspector : Editor
	{
		string[] mPrefabNameList = null;
		PrefabManager mInstance = null;
		int mSelectedIdx = 0;

		struct LoadData
		{
			public int index;
			public string resource_path;
			public string parefab_name;
			public GameObject go_parent;
		}

		List<LoadData> mIndexedDataList = new List<LoadData>();

		private void OnEnable()
		{
			mInstance = target as PrefabManager;
			LoadPrefabNames();
		}

		private void OnDisable()
		{
			mInstance = null;
		}

		void LoadPrefabNames()
		{
			List<string> name_list = new List<string>();
			mIndexedDataList.Clear();

			char unicode_slash = '\u2215';

			foreach(PrefabManager.ManagedPrefabData data in mInstance.m_ManagedPrefabList)
			{
				string res_path = string.Format( "Assets/Resources/{0}", data.m_ResourcePath );
				if( Directory.Exists( res_path ) )
				{
					string[] files = Directory.GetFiles( res_path, "*.prefab" );
					if( files != null )
					{
						foreach( string file_path in files )
						{
							string name = $"{data.m_ResourcePath}/{Path.GetFileNameWithoutExtension( file_path )}".Replace( '/', unicode_slash );
							name_list.Add( name );

							LoadData load_data = new LoadData();
							load_data.index = mIndexedDataList.Count;
							load_data.resource_path = data.m_ResourcePath;
							load_data.parefab_name = Path.GetFileNameWithoutExtension( file_path );
							load_data.go_parent = data.m_RootParent;

							mIndexedDataList.Add( load_data );
						}
					}
				}

				mPrefabNameList = name_list.ToArray();
			}
		}

		public override void OnInspectorGUI()
		{
			DrawCustom();
			base.OnInspectorGUI();
		}

		//------------------------------------------------------------------------
		void DrawCustom()
		{
			if( mInstance != null && mPrefabNameList != null )
			{
				GUILayout.BeginHorizontal();
				if( GUILayout.Button( new GUIContent("R", "Refresh"), GUILayout.MaxWidth(30) ) )
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
			if( mSelectedIdx < 0 || mSelectedIdx >= mPrefabNameList.Length || mSelectedIdx >= mIndexedDataList.Count )
				return;

			LoadData load_data = mIndexedDataList[mSelectedIdx];

			if( load_data.go_parent == null )
			{
				Debug.Log( "RootParent is null" );
				return;
			}

			string p_name = load_data.parefab_name;

			int child_count = load_data.go_parent.transform.childCount;
			if( child_count > 0 )
			{
				for( int i = 0; i < child_count; i++ )
				{
					Transform child = load_data.go_parent.transform.GetChild( i );
					if( child.gameObject.name == p_name )
					{
						Debug.Log( $"PrefabManager: already instantiate prefab {p_name}" );
						EditorGUIUtility.PingObject( child.gameObject );
						return;
					}
				}
			}

			string full_path = string.Format( "{0}/{1}", load_data.resource_path, p_name );
			GameObject prefab_obj = Resources.Load<GameObject>( full_path );
			if( prefab_obj == null )
			{
				EditorUtility.DisplayDialog( "PrefabManager", $"Can not load prefab {full_path}", "OK" );
				return;
			}

			Object new_obj = PrefabUtility.InstantiatePrefab( prefab_obj, load_data.go_parent.transform );
			EditorGUIUtility.PingObject( new_obj );
		}
	}
}
