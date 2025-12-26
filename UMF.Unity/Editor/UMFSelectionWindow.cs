//////////////////////////////////////////////////////////////////////////
//
// UMFSelectionWindow
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
using System.Reflection;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace UMF.Unity.EditorUtil
{
    public class UMFSelectionWindow : EditorWindow
    {
        public class SelectionData
        {
            public class SceneObjctData
            {
                public string prefix = "";
                public string hierarchy_path = "";
                public System.Action<Object> fixed_handler = null;
            }

            public Object unity_object = null;
            public string description = "";
            public List<SceneObjctData> scene_obj_list = null;

            // runtime
            public bool _is_scene_actived = false;
        }

        string mTitle = "";
        List<SelectionData> mDataList = null;

        Object mCurrentSelected = null;
        SelectionData.SceneObjctData mSelectedSceneObjectData = null;

        readonly float BUTTON_WIDTH = 300f;

        //------------------------------------------------------------------------	
        [MenuItem( "UMF/Window/Selection Window" )]
        public static void Open()
        {
            Show( new List<SelectionData>() );
        }

        //------------------------------------------------------------------------		
        public static void Show<T>( List<T> go_list, string title = "" ) where T : Object
        {
            List<SelectionData> data_list = new List<SelectionData>();
            if( go_list != null )
            {
                foreach( T go in go_list )
                {
                    SelectionData data = new SelectionData();
                    data.unity_object = go;
                    data.description = "";
                    data_list.Add( data );
                }
            }

            Show( data_list, title );
        }
        public static void Show( List<Object> go_list, string title = "" )
        {
            List<SelectionData> data_list = new List<SelectionData>();
            if( go_list != null )
            {
                foreach( Object go in go_list )
                {
                    SelectionData data = new SelectionData();
                    data.unity_object = go;
                    data.description = "";
                    data_list.Add( data );
                }
            }

            Show( data_list, title );
        }
        public static void Show( List<SelectionData> data_list, string title = "" )
        {
            UMFSelectionWindow window = EditorWindow.CreateWindow<UMFSelectionWindow>( "Selected Objects" );
            window.mTitle = title;
            window.mDataList = data_list;
            window.mCurrentSelected = null;
            window.mSelectedSceneObjectData = null;
            window.CheckSceneObj();
        }

        //------------------------------------------------------------------------		
        void CheckSceneObj( string exclude_name = "" )
        {
            if( mDataList == null || mDataList.Exists( a => a.unity_object.GetType() == typeof( SceneAsset ) ) == false )
                return;

            int scene_count = EditorSceneManager.sceneCount;
            if( scene_count <= 0 )
                return;

            List<string> curr_scene_names = new List<string>();
            for( int i = 0; i < scene_count; i++ )
            {
                Scene scene = EditorSceneManager.GetSceneAt( i );
                if( scene != null )
                    curr_scene_names.Add( scene.name );
            }

            curr_scene_names.RemoveAll( a => a == exclude_name );

            foreach( SelectionData data in mDataList )
            {
                if( data.unity_object.GetType() == typeof( SceneAsset ) )
                {
                    if( curr_scene_names.Contains( data.unity_object.name ) )
                        data._is_scene_actived = true;
                    else
                        data._is_scene_actived = false;
                }
            }
        }

        //------------------------------------------------------------------------		
        void OnEnable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneClosed += OnSceneClosed;
        }

        //------------------------------------------------------------------------		
        void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
        }

        //------------------------------------------------------------------------
        private void OnSceneClosed( Scene scene )
        {
            //Debug.Log( $"UMFSelectionWindow:OnSceneClosewd : {scene.name}" );
            CheckSceneObj( scene.name );
        }

        private void OnSceneOpened( Scene scene, OpenSceneMode mode )
        {
            //Debug.Log( $"UMFSelectionWindow:OnSceneLoaded : {scene.name} mode={mode}" );
            CheckSceneObj();
        }

        //------------------------------------------------------------------------		
        Vector2 mScrollPos = Vector2.zero;
        bool closed = false;
        void OnGUI()
        {
            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            if( GUILayout.Button( "CLOSE", GUILayout.Width( 150f ) ) )
            {
                mDataList = null;
                closed = true;
            }
            if( GUILayout.Button( "Export..", GUILayout.Width( 100f ) ) )
            {
                string export_key = "_UMFSelectionWiondowExportPath_";
                string pref_path = EditorPrefs.GetString( export_key, "Assets" );

                string folder = EditorUtility.SaveFolderPanel( "Export", pref_path, "" );
                if( System.IO.Directory.Exists( folder ) )
                {
                    EditorPrefs.SetString( export_key, folder );
                }
            }
            GUILayout.EndHorizontal();
            if( closed )
            {
                closed = false;
                Close();
                return;
            }

            if( mDataList == null )
                return;

            GUILayout.BeginHorizontal();
            if( GUILayout.Button( "All Select" ) )
            {
                Selection.objects = mDataList.Select( a => a.unity_object ).ToArray();
            }
            if( GUILayout.Button( "UnSelect" ) )
            {
                Selection.objects = new Object[0];
            }
            GUILayout.EndHorizontal();
            GUILayout.Label( $"{mTitle} DATA Count = {mDataList.Count}" );

            GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 2f ) );

            DrawSorting( 1 );

            mScrollPos = GUILayout.BeginScrollView( mScrollPos, false, true );
            for( int i = 0; i < mDataList.Count; i++ )
            {
                Color _color = GUI.color;
                Object selected = mDataList[i].unity_object;
                if( mCurrentSelected == selected )
                    GUI.color = Color.green;

                GUILayout.BeginHorizontal();
                if( GUILayout.Button( selected.name, GUILayout.Width( BUTTON_WIDTH ) ) )
                    SelectedObject( selected );

                GUI.color = _color;

                if( mDataList[i]._is_scene_actived )
                {
                    GUI.color = Color.green;

                    if( mDataList[i].scene_obj_list != null )
                    {
                        GUILayout.BeginVertical();
                        foreach( SelectionData.SceneObjctData s_data in mDataList[i].scene_obj_list )
                        {
                            Color col2 = GUI.color;
                            if( mSelectedSceneObjectData == s_data )
                                GUI.color = Color.yellow;

                            GUILayout.BeginHorizontal();
                            if( GUILayout.Button( "P", GUILayout.Width( 30f ) ) )
                            {
                                mSelectedSceneObjectData = s_data;

                                Scene s_scene = EditorSceneManager.GetSceneByName( mDataList[i].unity_object.name );
                                if( s_scene != null )
                                {
                                    GameObject[] roots = s_scene.GetRootGameObjects();
                                    foreach( GameObject root in roots )
                                    {
                                        if( s_data.hierarchy_path == root.name )
                                        {
                                            EditorGUIUtility.PingObject( root );
                                            Selection.activeGameObject = root.gameObject;
                                            break;
                                        }

                                        Transform t = root.transform.Find( s_data.hierarchy_path.Replace( root.name + "/", "" ) );
                                        if( t != null )
                                        {
                                            EditorGUIUtility.PingObject( t );
                                            Selection.activeGameObject = t.gameObject;
                                            break;
                                        }
                                    }
                                }
                            }

                            if( s_data.fixed_handler != null )
                            {
                                Color col3 = GUI.color;
                                GUI.color = Color.red;
                                if( GUILayout.Button( "Fix", GUILayout.Width( 40f ) ) )
                                {
                                    if( Selection.activeGameObject != null && Selection.activeGameObject.name == Path.GetFileNameWithoutExtension( s_data.hierarchy_path ) )
                                    {
                                        s_data.fixed_handler( Selection.activeGameObject );
                                    }
                                }

                                GUI.color = col3;
                            }

                            if( string.IsNullOrEmpty( s_data.prefix ) == false )
                            {
                                float width = GUI.skin.button.CalcSize( UMFEditorUtil.GetTempGUIContents( s_data.prefix ) ).x;
                                GUILayout.Button( s_data.prefix, GUILayout.Width( width ) );
                            }
                            GUILayout.TextField( s_data.hierarchy_path );
                            GUILayout.EndHorizontal();

                            GUI.color = col2;
                        }
                        GUILayout.EndVertical();
                    }
                    GUI.color = _color;
                }
                else
                {
                    if( mDataList[i].description.Length > 0 )
                        GUILayout.TextField( mDataList[i].description );
                }

                // custom 
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        void SelectedObject( Object obj )
        {
            EditorGUIUtility.PingObject( obj );
            Selection.activeObject = obj;
            mCurrentSelected = obj;
        }

        bool mSortAscending = true;
        void DrawSorting( int sort_column )
        {
            GUILayout.BeginHorizontal();
            for( int i = 0; i < sort_column; i++ )
            {
                if( GUILayout.Button( string.Format( "{0}", mSortAscending ? "\u25B2" : "\u25BC" ), GUILayout.Width( BUTTON_WIDTH ) ) )
                {
                    DoSort( i );
                }
            }
            GUILayout.EndHorizontal();
        }

        void DoSort( int sort_col )
        {
            if( mDataList == null )
                return;

            mSortAscending = !mSortAscending;

            if( sort_col == 0 )
            {
                if( mSortAscending )
                    mDataList = mDataList.OrderBy( o => o.unity_object.name ).ToList();
                else
                    mDataList = mDataList.OrderByDescending( o => o.unity_object.name ).ToList();
            }
            else if( sort_col == 1 )
            {
            }
        }
    }
}