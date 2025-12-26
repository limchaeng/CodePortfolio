#define USE_UGUI
//////////////////////////////////////////////////////////////////////////
//
// UMFFindWindow
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
#if USE_UGUI
using UnityEngine.UI;
#endif
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.Events;
using System.Linq;
using UnityEditor.ShaderKeywordFilter;
using Codice.Client.BaseCommands.FastExport;

namespace UMF.Unity.EditorUtil
{
	public class UMFFindWindow : EditorWindow
	{
		static UMFFindWindow instance;

		public const string TITLE = "UMF Find Window";
		public const string PREFAB_PATH_PREFS_KEY = "UMFFINDWINDOW_PREFABPATH";

		public enum eToolType
		{
			None,
			Component,
			Sprite,
			UnityEvent,
		}

		[MenuItem( "UMF/Window/Find Tool" )]
		static public void ShowEditor()
		{
			if( instance != null )
			{
				instance.ShowUtility();
				instance.Focus();
				instance.Init();
				return;
			}

			instance = (UMFFindWindow)EditorWindow.GetWindow( typeof( UMFFindWindow ), false, TITLE );

			instance.ShowUtility();
			instance.Focus();
			instance.Init();
		}

		Rect mWindowSceneRect;
		Rect mWindowProjectRect;

		public class FindObjectData
		{
			public string title;
			public Object obj;
			public List<Object> sub_obj_list = null;
			public List<string> sub_obj_name_list = null;

			public FindObjectData( Object o )
				: this( o.name, o )
			{
			}
			public FindObjectData( string t, Object o )
			{
				title = t;
				obj = o;
				sub_obj_list = null;
			}

			public void AddSub( Object o )
			{
				if( sub_obj_list == null )
					sub_obj_list = new List<Object>();

				sub_obj_list.Add( o );
			}

			public void AddSubName( string _name )
			{
				if( sub_obj_name_list == null )
					sub_obj_name_list = new List<string>();

				sub_obj_name_list.Add( _name );
			}
		}

		List<FindObjectData> mFindInScene = null;
		List<FindObjectData> mFindInProject = null;

		public delegate void delegateSetObjects( List<FindObjectData> sceneObjs, List<FindObjectData> projectObjs );
		eToolType mSelectedToolType = eToolType.None;
		UMFFindWindowTypeBase mSelectedTool = null;
		Dictionary<eToolType, UMFFindWindowTypeBase> mToolCache = new Dictionary<eToolType, UMFFindWindowTypeBase>();

		string mPrefabPath = "";
		public string PrefabPath { get { return mPrefabPath; } }

		public void OnCloseWindow()
		{
		}

		void Init()
		{
			mWindowSceneRect = new Rect( 0, 0, 0, 0 );
			mWindowProjectRect = new Rect( 0, 0, 0, 0 );

			mPrefabPath = PlayerPrefs.GetString( PREFAB_PATH_PREFS_KEY );
		}

		//------------------------------------------------------------------------	
		void SetObjects( List<FindObjectData> sceneObjs, List<FindObjectData> projectObjs )
		{
			mFindInScene = sceneObjs;
			mFindInProject = projectObjs;
		}

		//------------------------------------------------------------------------	
		UMFFindWindowTypeBase GetTool( eToolType tooltype, delegateSetObjects callback )
		{
			if( tooltype == eToolType.None )
				return null;

			UMFFindWindowTypeBase tool = null;
			if( mToolCache.ContainsKey( tooltype ) )
			{
				tool = mToolCache[tooltype];
				if( tool != null )
					tool.SetCallback( callback );
				return tool;
			}

			switch( tooltype )
			{
				case eToolType.Component:
					tool = new UMFFindWindow_Component();
					break;

				case eToolType.Sprite:
					tool = new UMFFindWindow_Sprite();
					break;

				case eToolType.UnityEvent:
					tool = new UMFFindWindow_UnityEvent();
					break;
			}

			mToolCache.Add( tooltype, tool );
			tool.SetCallback( callback );
			return tool;
		}

		void OnGUI()
		{
			float ypos = 10f;
			//float xpos = 0f;

			GUILayout.BeginHorizontal();
			GUILayout.Label( "Find Tool", GUILayout.Width( 150f ) );

			Object activeObject = Selection.activeObject;
			if( activeObject != null )
			{
				GUILayout.Label( string.Format( "Active Object:{0}({1})", activeObject.name, activeObject.GetType() ) );
			}
			else
			{
				GUILayout.Label( "Active Object NULL" );
			}
			GUILayout.Label( "ToolType:", GUILayout.Width( 70f ) );
			eToolType selectedtooltype = (eToolType)EditorGUILayout.EnumPopup( "", mSelectedToolType, GUILayout.Width( 300f ) );
			if( mSelectedToolType != selectedtooltype )
			{
				mSelectedToolType = selectedtooltype;
				mSelectedTool = null;
				mFindInScene = null;
				mFindInProject = null;
			}

			if( mSelectedTool == null )
				mSelectedTool = GetTool( mSelectedToolType, SetObjects );

			if( GUILayout.Button( "Close", GUILayout.Width( 100f ) ) )
			{
				GUILayout.EndHorizontal();
				Close();
				return;
			}
			GUILayout.EndHorizontal();

			string _prefab_pth = EditorGUILayout.TextField( "Prefab Path:", mPrefabPath );
			if( mPrefabPath != _prefab_pth )
			{
				mPrefabPath = _prefab_pth;
				PlayerPrefs.SetString( PREFAB_PATH_PREFS_KEY, mPrefabPath );
			}

			GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 2f ) );

			if( mSelectedTool != null )
				mSelectedTool.DrawGUI( this );

			GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 2f ) );

			ypos += 80f;
			Rect areaRect = new Rect( 0, ypos, position.width, position.height - ypos - 30 );
			GUILayout.BeginArea( areaRect );
			{
				BeginWindows();
				{
					float scene_ratio = 0.5f;
					mWindowProjectRect.x = 5;
					mWindowProjectRect.y = 0;
					mWindowProjectRect.width = areaRect.width * 0.5f - 10;
					mWindowProjectRect.height = areaRect.height - 5;
					mWindowProjectRect = GUILayout.Window( 0, mWindowProjectRect, DoWindowProjectGUI, "In Project" );

					mWindowSceneRect.x = areaRect.width * scene_ratio + 5;
					mWindowSceneRect.y = 0;
					mWindowSceneRect.width = areaRect.width * scene_ratio - 10;
					mWindowSceneRect.height = areaRect.height - 5;
					mWindowSceneRect = GUILayout.Window( 1, mWindowSceneRect, DoWindowSceneGUI, "In Scene" );
				}
				EndWindows();
			}
			GUILayout.EndArea();

			GUILayout.BeginArea( new Rect( 0, position.height - 20, position.width, 20 ) );
			GUILayout.Label( " Copyright 2025 FN All rights reserved." );
			GUILayout.EndArea();
		}

		//-----------------------------------------------------------------------------
		Vector2 _win_project_scroll = Vector2.zero;
		void DoWindowProjectGUI( int windowId )
		{
			if( mFindInProject == null )
			{
				GUILayout.Label( "Find:-" );
				return;
			}

			GUILayout.Label( "Find:" + mFindInProject.Count );

			_win_project_scroll = GUILayout.BeginScrollView( _win_project_scroll );
			foreach( FindObjectData o_data in mFindInProject )
			{
				GUILayout.BeginHorizontal();
				if( GUILayout.Button( o_data.title ) )
				{
					EditorGUIUtility.PingObject( o_data.obj );
				}
				if( GUILayout.Button( "OPEN", GUILayout.Width( 100f ) ) )
				{
					AssetDatabase.OpenAsset( o_data.obj );
				}
				GUILayout.EndHorizontal();

				if( o_data.sub_obj_list != null )
				{
					foreach( Object sub_o in o_data.sub_obj_list )
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space( 30f );
						if( GUILayout.Button( sub_o.name ) )
						{
							EditorGUIUtility.PingObject( sub_o );
						}
						GUILayout.EndHorizontal();
					}
				}

				if( o_data.sub_obj_name_list != null )
				{
					foreach( string sub_o in o_data.sub_obj_name_list )
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space( 30f );
						if( GUILayout.Button( sub_o ) )
						{
						}
						GUILayout.EndHorizontal();
					}
				}
			}
			GUILayout.EndScrollView();
		}

		//-----------------------------------------------------------------------------
		Vector2 _win_scene_scroll = Vector2.zero;
		void DoWindowSceneGUI( int windowId )
		{
			if( mFindInScene == null )
			{
				GUILayout.Label( "Find:-" );
				return;
			}

			GUILayout.Label( "Find:" + mFindInScene.Count );

			Object active_obj = Selection.activeObject;
			_win_scene_scroll = GUILayout.BeginScrollView( _win_scene_scroll );
			foreach( FindObjectData o_data in mFindInScene )
			{
				GUILayout.BeginHorizontal();
				Color curr_color = GUI.color;
				if( active_obj == o_data.obj )
					GUI.color = Color.green;

				if( GUILayout.Button( o_data.title ) )
				{
					EditorGUIUtility.PingObject( o_data.obj );
				}
				if( GUILayout.Button( "Select", GUILayout.Width( 100 ) ) )
				{
					EditorGUIUtility.PingObject( o_data.obj );
					Selection.activeObject = o_data.obj;
				}

				GUI.color = curr_color;

				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// find class
	public abstract class UMFFindWindowTypeBase
	{
		protected UMFFindWindow.delegateSetObjects mCallback = null;

		public abstract UMFFindWindow.eToolType GetToolType();
		public abstract void DrawGUI( UMFFindWindow find_tool );

		public void SetCallback( UMFFindWindow.delegateSetObjects callback )
		{
			mCallback = callback;
		}
	}

	//------------------------------------------------------------------------
	public class UMFFindWindow_UnityEvent : UMFFindWindowTypeBase
	{
		string mMethodName = "";
		bool mFindCorrectName = false;

		public class EventData
		{
			public class TargetData
			{
				public Object target;
				public string method_name;
			}

			public UnityEventBase unity_event = null;
			public List<TargetData> target_list = new List<TargetData>();

			public List<TargetData> FindMethod( string method, bool is_correct )
			{
				if( is_correct )
					return target_list.FindAll( a => a.method_name.ToLower() == method.ToLower() );
				else
                    return target_list.FindAll( a => a.method_name.ToLower().Contains( method.ToLower() ) );
            }
		}

		Dictionary<MonoBehaviour, List<EventData>> mCacheDic = null;

		public UMFFindWindow_UnityEvent()
		{
			RefreshCache( true );
		}

        public override UMFFindWindow.eToolType GetToolType()
        {
			return UMFFindWindow.eToolType.UnityEvent;
        }


        void RefreshCache( bool forced )
		{
			if( forced == false && mCacheDic != null )
				return;

			if( mCacheDic == null )
				mCacheDic = new Dictionary<MonoBehaviour, List<EventData>>();

			mCacheDic.Clear();

			int scene_count = EditorSceneManager.sceneCount;
            for( int i = 0; i < scene_count; i++ )
            {
				Scene scene = EditorSceneManager.GetSceneAt( i );
				GameObject[] roots = scene.GetRootGameObjects();
				foreach( GameObject root_go in roots )
				{
					List<MonoBehaviour> monos = root_go.GetComponentsInChildrenIncludeThis<MonoBehaviour>();
					if( monos == null )
						continue;

					foreach( MonoBehaviour b in monos )
					{
						TypeInfo type_info = b.GetType().GetTypeInfo();
						List<FieldInfo> evts = type_info.DeclaredFields.Where( a => a.FieldType.IsSubclassOf( typeof( UnityEventBase ) ) ).ToList();
						evts.AddRange( type_info.DeclaredFields.Where( a => CheckEnumeratedType( a.FieldType, typeof( UnityEventBase ) ) ).ToList() );

						foreach( var ev in evts )
						{
							List<UnityEventBase> evt_list = new List<UnityEventBase>();
							if( ev.FieldType.IsArray )
							{
                                UnityEventBase[] u_evt = ev.GetValue( b ) as UnityEventBase[];
								if( u_evt != null )
								{
									evt_list.AddRange( u_evt.ToList() );
								}
                            }
							else if( ev.FieldType.IsGenericType )
							{
								if( ev.FieldType.IsSubclassOf( typeof( IList ) ) )
								{
									List<UnityEventBase> list = ev.GetValue(b) as List<UnityEventBase>;
									evt_list.AddRange( list );
								}
							}
							else
							{
								UnityEventBase u_evt = ev.GetValue( b ) as UnityEventBase;
								if( u_evt == null )
									continue;

								evt_list.Add( u_evt );
							}

							foreach( UnityEventBase u_evt in evt_list )
							{
								EventData ev_data = new EventData();
								ev_data.unity_event = u_evt;

								if( mCacheDic.ContainsKey( b ) == false )
									mCacheDic.Add( b, new List<EventData>() );

								mCacheDic[b].Add( ev_data );

								int count = u_evt.GetPersistentEventCount();
								for( int c = 0; c < count; c++ )
								{
									Object o = u_evt.GetPersistentTarget( c );
									if( o == null )
										continue;

									EventData.TargetData target_data = new EventData.TargetData();
									target_data.target = o;
									target_data.method_name = u_evt.GetPersistentMethodName( c );

									ev_data.target_list.Add( target_data );
								}
							}
						}
					}
				}
			}
		}

        bool CheckEnumeratedType( System.Type type, System.Type check_type )
        {
            var elType = type.GetElementType();
			if( elType != null && elType.IsSubclassOf( check_type ) )
				return true;

            var elTypes = type.GetGenericArguments();
			if( elTypes.Length > 0 && elTypes[0] != null && elTypes[0].IsSubclassOf( check_type ) )
				return true;

			return false;
        }

        public override void DrawGUI( UMFFindWindow find_tool )
        {
            GUILayout.BeginHorizontal();
            mMethodName = EditorGUILayout.TextField( "Method Name:", mMethodName, GUILayout.Width( 500f ) );

            if( GUILayout.Button( "Find", GUILayout.Width( 100f ) ) )
            {
                List<UMFFindWindow.FindObjectData> find_scene = null;
                List<UMFFindWindow.FindObjectData> find_project = null;
                if( string.IsNullOrEmpty( mMethodName ) == false )
                {
					// in scene
					RefreshCache( false );

					foreach( var kvp in mCacheDic )
					{
						foreach( EventData evt_data in kvp.Value )
						{
							List<EventData.TargetData> exist_list = evt_data.FindMethod( mMethodName, mFindCorrectName );
							if( exist_list != null )
							{
								foreach( EventData.TargetData t_data in exist_list )
								{
									if( find_scene == null )
										find_scene = new List<UMFFindWindow.FindObjectData>();

									find_scene.Add( new UMFFindWindow.FindObjectData( $"{t_data.target}", kvp.Key.gameObject ) );
								}
                            }
                        }
					}
                }

                if( mCallback != null )
                    mCallback( find_scene, find_project );
            }

            if( GUILayout.Button( "Refresh", GUILayout.Width( 100f ) ) )
            {
				RefreshCache( true );
			}

			mFindCorrectName = GUILayout.Toggle( mFindCorrectName, "Correct Name" );

            GUILayout.EndHorizontal();
        }
    }

    //------------------------------------------------------------------------
    public class UMFFindWindow_Sprite : UMFFindWindowTypeBase
	{
		string mSpriteName = "";

		public override UMFFindWindow.eToolType GetToolType()
		{
			return UMFFindWindow.eToolType.Sprite;
		}

		public override void DrawGUI( UMFFindWindow find_tool )
		{
			GUILayout.BeginHorizontal();
			mSpriteName = EditorGUILayout.TextField( "Sprite Name:", mSpriteName, GUILayout.Width( 300f ) );

			if( GUILayout.Button( "Find Scene", GUILayout.Width( 200f ) ) )
			{
				List<UMFFindWindow.FindObjectData> find_scene = null;
				List<UMFFindWindow.FindObjectData> find_project = null;
				if( string.IsNullOrEmpty( mSpriteName ) == false )
				{
					// in scene
					GameObject activeObject = Selection.activeGameObject;
					if( activeObject != null )
					{
						SpriteRenderer[] sp_renderers = activeObject.GetComponentsInChildren<SpriteRenderer>( true );
						if( sp_renderers != null )
						{
							foreach( SpriteRenderer sr in sp_renderers )
							{
								if( sr.sprite != null && sr.sprite.name == mSpriteName )
								{
									if( find_scene == null )
										find_scene = new List<UMFFindWindow.FindObjectData>();

									find_scene.Add( new UMFFindWindow.FindObjectData( sr.gameObject ) );
								}
							}
						}

#if USE_UGUI
						Image[] ui_images = activeObject.GetComponentsInChildren<Image>( true );
						if( ui_images != null )
						{
							foreach( Image img in ui_images )
							{
								if( img.sprite != null && img.sprite.name == mSpriteName )
								{
									if( find_scene == null )
										find_scene = new List<UMFFindWindow.FindObjectData>();

									find_scene.Add( new UMFFindWindow.FindObjectData( img.gameObject ) );
								}
							}
						}
#endif
					}
				}

				if( mCallback != null )
					mCallback( find_scene, find_project );
			}

			GUILayout.EndHorizontal();
		}
	}

	//------------------------------------------------------------------------	
	public class UMFFindWindow_Component : UMFFindWindowTypeBase
	{
		string mComponentName = "";

		public override UMFFindWindow.eToolType GetToolType()
		{
			return UMFFindWindow.eToolType.Component;
		}

		System.Type FindTypeByName( string type_name )
		{
			System.Type _type = System.Type.GetType( type_name );
			if( _type != null )
				return _type;

			System.Reflection.Assembly curr_assembly = System.Reflection.Assembly.GetExecutingAssembly();
			System.Reflection.AssemblyName[] assem_names = curr_assembly.GetReferencedAssemblies();
			foreach( System.Reflection.AssemblyName a_name in assem_names )
			{
				System.Reflection.Assembly assem = System.Reflection.Assembly.Load( a_name );
				if( assem != null )
				{
					_type = assem.GetType( type_name );
					if( _type != null )
						return _type;
				}
			}

			return null;
		}

		public override void DrawGUI( UMFFindWindow find_tool )
		{
			GUILayout.BeginHorizontal();
			mComponentName = EditorGUILayout.TextField( "Namespace.Type:", mComponentName, GUILayout.Width( 500f ) );

			if( GUILayout.Button( "Find", GUILayout.Width( 100f ) ) )
			{
				List<UMFFindWindow.FindObjectData> find_scene = null;
				List<UMFFindWindow.FindObjectData> find_project = null;
				if( string.IsNullOrEmpty( mComponentName ) == false )
				{
					System.Type find_type = FindTypeByName( mComponentName );
					if( find_type != null )
					{
						// in scene
						GameObject activeObject = Selection.activeGameObject;
						if( activeObject != null )
						{
							Component[] comps = activeObject.GetComponentsInChildren( find_type );
							foreach( Component comp in comps )
							{
								if( find_scene == null )
									find_scene = new List<UMFFindWindow.FindObjectData>();

								find_scene.Add( new UMFFindWindow.FindObjectData( comp.gameObject ) );
							}
						}

						// prefab
						if( string.IsNullOrEmpty( find_tool.PrefabPath ) == false )
						{
							find_project = FindComponentInPrefabs( find_tool.PrefabPath, find_type );
						}
					}
				}

				if( mCallback != null )
					mCallback( find_scene, find_project );
			}

			GUILayout.EndHorizontal();
		}

		//------------------------------------------------------------------------		
		public static List<UMFFindWindow.FindObjectData> FindComponentInPrefabs<T>( string prefab_path ) where T : Component
		{
			return FindComponentInPrefabs( prefab_path, typeof( T ) );
		}
		public static List<UMFFindWindow.FindObjectData> FindComponentInPrefabs( string prefab_path, System.Type type )
		{
			List<UMFFindWindow.FindObjectData> list = new List<UMFFindWindow.FindObjectData>();
			string[] dirs = Directory.GetDirectories( prefab_path );
			foreach( string d in dirs )
			{
				string[] paths = Directory.GetFiles( d, "*.prefab" );
				foreach( string full_path in paths )
				{
					string file = full_path.Replace( Application.dataPath, "Assets" );
					GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>( file );
					if( go != null )
					{
						UMFFindWindow.FindObjectData data = null;

						List<Transform> children = go.transform.GetChildrenList( true, true );
						children.Insert( 0, go.transform );

						foreach( Transform t in children )
						{
							Component[] comps = t.gameObject.GetComponents( type );
							foreach( Component comp in comps )
							{
								if( comp != null )
								{
									if( data == null )
										data = new UMFFindWindow.FindObjectData( go );
									data.AddSub( t.gameObject );
								}
							}
						}

						if( data != null )
							list.Add( data );
					}
				}
			}

			return list;
		}
	}
}

