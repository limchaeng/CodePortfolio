//////////////////////////////////////////////////////////////////////////
//
// FXToolEditorWindow
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// FPlayBase 에디터 툴 : 에디터상에서 제작된 FX의 플레이 테스트, 마이그레이션, 오류체크등을 위함
//////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UMF.Unity.EditorUtil;
using UMF.Unity;
using System.IO;
using UnityEngine.Playables;
using DG.Tweening;
using System.Linq;

namespace U6FXV2
{
	public class FXToolEditorWindow : EditorWindow
	{
		static FXToolEditorWindow instance;

		public const string TITLE = "FX Tool Window";
		public const string SAVE_PREFS_KEY = "U6FXTOOLWINDOW_SAVE";
		public const string FX_PREFAB_PATH = "Assets/_FX/Resources/_fxprefabs";

		public List<string> mFXValidPathList = new List<string>()
		{
			"Resources/unity_builtin_extra",
		};

		[MenuItem( "U6/Window/FX Tool" )]
		public static void ShowEditor()
		{
			ShowEditor( null );
		}

		public static void ShowEditor( FXTool tool )
		{
			if( instance != null )
			{
				instance.ShowUtility();
				instance.Focus();
				instance.Init( tool );
				return;
			}

			instance = (FXToolEditorWindow)EditorWindow.GetWindow( typeof( FXToolEditorWindow ), false, TITLE );

			instance.ShowUtility();
			instance.Focus();
			instance.Init( tool );
		}

		AlphabetSortedContainer<Sprite> mBGBattleCached = null;
		AlphabetSortedContainer<Sprite> mBGQuestCached = null;
		AlphabetSortedContainer<Sprite> mCardCached = null;
		AlphabetSortedContainer<Sprite> mUICached = null;

		public class FXContainer
		{
			public string category = "";
			public AlphabetSortedContainer<GameObject> fx_prefab_cached = null;
		}
		List<FXContainer> mFXCached = null;

		GUIContent mBGBattleLoadText = new GUIContent( "BG Battle Load", "Load background image" );
		GUIContent mBGQuestLoadText = new GUIContent( "BG Quest Load", "Load background image" );
		GUIContent mCardLoadText = new GUIContent( "Card Load", "Load card image" );
		GUIContent mUILoadText = new GUIContent( "UI Load", "Load UI image" );
		GUIContent mFXLoadText = new GUIContent( "FX Load", "Load FX" );

		GUIContent mHideBtnText = new GUIContent( "H", "Hide" );
		GUIContent mShowBtnText = new GUIContent( "S", "Show" );
		GUIContent mSelectBtnText = new GUIContent( "G", "Select GameObject" );
		GUIContent mResourceSelectBtnText = new GUIContent( "R", "Select Resource" );

		GUIContent mDelayTimeText = new GUIContent( "Delay time", "play delay after seconds" );
		GUIContent mDelayTimeAddRandomText = new GUIContent( "- Delay Time X ~ Y", "add random value to DelayTime" );
		GUIContent mAddCallbackTime = new GUIContent( "Add callback time", "callback time add" );

		GUIContent mFXSoundText = new GUIContent( "Sound", "Ping sound object and load!" );

		GUIContent mProjectileTangentZText = new GUIContent( "Knot[0] TangentOut Z", "Knot[0]'s TangentOut Z * value" );
		GUIContent mProjectileUseAddRandomTimeText = new GUIContent( "Use Add RandomTime", "Use animate duration + random time" );
		GUIContent mProjectileAddRandomTimeText = new GUIContent( "- Add RandomTime", "Use animate duration + random time" );

		Color mBGColor = UMFColor.ColorFromHEX( "E2C1E2" );
		Color mCardColor = Color.cyan;
		Color mUIColor = UMFColor.ColorFromHEX( "A4DFB6" );
		Color mFXColor = Color.magenta;

		FXTool mTool = null;
		bool mSelectionToolExpand = true;
		bool mFXtoolExpand = true;
		bool mPrefabToolExpand = true;
		bool mBatchProcessExpand = false;
		Vector2 mScrollPos = Vector2.zero;

		GameObject mSelectedFXObject = null;
		FXMain mSelectedFXMain = null;
		SerializedObject mSelectedFXSerializeObject = null;
		Dictionary<FXMain.eFX_Type, SerializedObject> mSelectedFXType_SerializeObjectDic = new Dictionary<FXMain.eFX_Type, SerializedObject>();
		
		//------------------------------------------------------------------------
		void UpdateSerializeObject()
		{
			mSelectedFXSerializeObject = null;
			mSelectedFXType_SerializeObjectDic.Clear();

			if( mSelectedFXMain == null )
				return;

			mSelectedFXSerializeObject = new SerializedObject( mSelectedFXMain );
			GetSerializeObject( FXMain.eFX_Type.Cast, mSelectedFXMain.m_Cast );
			GetSerializeObject( FXMain.eFX_Type.Castloop, mSelectedFXMain.m_CastLoop );
			GetSerializeObject( FXMain.eFX_Type.Fire, mSelectedFXMain.m_Fire );
			GetSerializeObject( FXMain.eFX_Type.Projectile, mSelectedFXMain.m_Projectile );
			GetSerializeObject( FXMain.eFX_Type.Hit, mSelectedFXMain.m_Hit );
			GetSerializeObject( FXMain.eFX_Type.FuncHit, mSelectedFXMain.m_FuncHit );
			GetSerializeObject( FXMain.eFX_Type.Remove, mSelectedFXMain.m_Update );
			GetSerializeObject( FXMain.eFX_Type.Update, mSelectedFXMain.m_Remove );
		}
		SerializedObject GetSerializeObject( FXMain.eFX_Type fx_type, UnityEngine.Object obj )
		{
			if( obj == null )
				return null;

			SerializedObject so;
			if( mSelectedFXType_SerializeObjectDic.TryGetValue(fx_type, out so) == false )
			{
				so = new SerializedObject( obj );
				mSelectedFXType_SerializeObjectDic.Add( fx_type, so );
			}

			return so;
		}			

		//------------------------------------------------------------------------
		public void Init( FXTool tool )
		{
			if( tool != null )
				mTool = tool;

			mSelectionToolExpand = PlayerPrefs.GetInt( $"{SAVE_PREFS_KEY}_selection_expand", 1 ) == 1 ? true : false;
		}

		//------------------------------------------------------------------------
		private void OnEnable()
		{
			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
			EditorApplication.playModeStateChanged += OnPlayModeStateChange;

			CacheReset();
			OnSelectionChanged();
		}

		private void OnPlayModeStateChange( PlayModeStateChange state )
		{
			mTool = FXTool.Instance;
			OnSelectionChanged();
			Repaint();
		}

		//------------------------------------------------------------------------
		private void OnDisable()
		{
			Selection.selectionChanged -= OnSelectionChanged;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
		}

		private void OnSelectionChanged()
		{
			if( Selection.activeGameObject != null )
			{
				GameObject active_go = Selection.activeGameObject;

				if( active_go.scene.rootCount > 0 && mFXCached != null )
				{
					FXMain fx_main = active_go.GetComponent<FXMain>();
					if( fx_main != null )
					{
						mSelectedFXObject = active_go;

						if( mSelectedFXMain != null )
							mSelectedFXMain.Stop();

						mSelectedFXMain = fx_main;
						UpdateSerializeObject();

						mFXCheckedMessage = FX_ValidCheck( mSelectedFXObject, false );
						Repaint();
					}
				}
			}
		}

		//------------------------------------------------------------------------
		System.Text.StringBuilder mFXValidCheckMessageBuilder = new System.Text.StringBuilder();
		string mFXCheckedMessage = "";
		List<string> FX_TYPE_NAME_LIST = null;

		//------------------------------------------------------------------------
		void _FX_ValidCheck_FXType( FXMain fx_main, GameObject child, FXMain.eFX_Type fx_type, ref int msg_idx, bool fix )
		{
			FXPlayBase fX_base = child.GetComponent<FXPlayBase>();
			if( fX_base == null )
			{
				if( fix )
				{
					switch( fx_type )
					{
						case FXMain.eFX_Type.Cast:
							{
								FXPlayCast comp = child.GetComponent<FXPlayCast>();
								if( comp == null )
									comp = child.AddComponent<FXPlayCast>();

								if( fx_main != null )
									fx_main.m_Cast = comp;
							}
							break;

						case FXMain.eFX_Type.Castloop:
							{
								FXPlayCastLoop comp = child.GetComponent<FXPlayCastLoop>();
								if( comp == null )
									comp = child.AddComponent<FXPlayCastLoop>();

								if( fx_main != null )
									fx_main.m_CastLoop = comp;
							}
							break;

						case FXMain.eFX_Type.Fire:
							{
								FXPlayFire comp = child.GetComponent<FXPlayFire>();
								if( comp == null )
									comp = child.AddComponent<FXPlayFire>();

								if( fx_main != null )
									fx_main.m_Fire = comp;
							}
							break;

						case FXMain.eFX_Type.Projectile:
							{
								FXPlayProjectile comp = child.GetComponent<FXPlayProjectile>();
								if( comp == null )
									comp = child.AddComponent<FXPlayProjectile>();

								if( fx_main != null )
									fx_main.m_Projectile = comp;
							}
							break;

						case FXMain.eFX_Type.Hit:
							{
								FXPlayHit comp = child.GetComponent<FXPlayHit>();
								if( comp == null )
									comp = child.AddComponent<FXPlayHit>();

								if( fx_main != null )
									fx_main.m_Hit = comp;
							}
							break;

						case FXMain.eFX_Type.FuncHit:
							{
								FXPlayHit comp = child.GetComponent<FXPlayHit>();
								if( comp == null )
									comp = child.AddComponent<FXPlayHit>();

								if( fx_main != null )
									fx_main.m_FuncHit = comp;
							}
							break;

						case FXMain.eFX_Type.Remove:
							{
								FXPlayRemove comp = child.GetComponent<FXPlayRemove>();
								if( comp == null )
									comp = child.AddComponent<FXPlayRemove>();

								if( fx_main != null )
									fx_main.m_Remove = comp;
							}
							break;

						case FXMain.eFX_Type.Update:
							{
								FXPlayUpdate comp = child.GetComponent<FXPlayUpdate>();
								if( comp == null )
									comp = child.AddComponent<FXPlayUpdate>();

								if( fx_main != null )
									fx_main.m_Update = comp;
							}
							break;
					}
				}
				else
				{
					mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}.[{child.name}][{fx_type}] FXPlayBase component not found!" );
					msg_idx++;
				}
			}
			
			if( fX_base != null )
			{
				if( fX_base.m_MainParticle == null )
				{
					if( fix )
					{
						int child_count = fX_base.transform.childCount;
						if( child_count > 0 )
						{
							GameObject main_go = fX_base.transform.GetChild( 0 ).gameObject;
							ParticleSystem main_ps = main_go.GetComponent<ParticleSystem>();
							if( main_ps == null )
							{
								main_ps = main_go.AddComponent<ParticleSystem>();
								ParticleSystem.MainModule main_module = main_ps.main;
								main_module.duration = 0.05f;
								main_module.startLifetime = 0.0001f;
								main_module.loop = false;
								main_module.startDelay = 0f;
								main_module.playOnAwake = false;
								main_module.scalingMode = ParticleSystemScalingMode.Hierarchy;

								ParticleSystem.EmissionModule emission = main_ps.emission;
								emission.enabled = false;

								ParticleSystem.ShapeModule shape = main_ps.shape;
								shape.enabled = false;

								ParticleSystemRenderer renderer = main_ps.GetComponent<ParticleSystemRenderer>();
								if( renderer != null )
									renderer.enabled = false;
							}
							else
							{
								// fix
								ParticleSystem.MainModule main_module = main_ps.main;
								if( main_module.playOnAwake )
									main_module.playOnAwake = false;
							}

							fX_base.m_MainParticle = main_ps;
						}
					}
					else
					{
						mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}.[{child.name}][{fx_type}] FXPlayBase's main particle not attached!" );
						msg_idx++;
					}
				}
			}

			int child_childcount = child.transform.childCount;
			if( child_childcount > 1 )
			{
				mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}.[{child.name}][{fx_type}] FXPlayBase's main particle too many.. need only one." );
				msg_idx++;
			}
		}

		//------------------------------------------------------------------------
		void _CheckParticleSystemValid( GameObject selected_obj, ParticleSystem ps, bool fix, ref int msg_idx )
		{
			if( ps.main.playOnAwake )
			{
				if( fix )
				{
					ParticleSystem.MainModule module = ps.main;
					module.playOnAwake = false;						
				}
				else
				{
					mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}.[{ps.name}] play On Awake enabled. set to disable." );
					msg_idx++;
				}
			}

			if( ps.main.scalingMode != ParticleSystemScalingMode.Hierarchy )
			{
				if( fix )
				{
					ParticleSystem.MainModule module = ps.main;
					module.scalingMode = ParticleSystemScalingMode.Hierarchy;
				}
				else
				{
					mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}.[{ps.name}] scaling Mode Hierarchy not set." );
					msg_idx++;
				}
			}

			ParticleSystemRenderer renderer = ps.gameObject.GetComponent<ParticleSystemRenderer>();
			if( renderer.enabled )
			{
				if( renderer.sharedMaterial == null )
				{
					mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}.[{ps.name}] material invalid." );
					msg_idx++;
				}
				else
				{
					string mat_path = AssetDatabase.GetAssetPath( renderer.sharedMaterial );
					bool is_valid_path = false;
					foreach( string valid_path in mFXValidPathList )
					{
						if( mat_path.Contains( valid_path ) )
							is_valid_path = true;
					}

					if( is_valid_path == false )
					{
						if( selected_obj.scene.name == null )
						{
							// is a prefab
							string valid_path = Path.GetDirectoryName( AssetDatabase.GetAssetPath( selected_obj ) ).NormalizeSlashPath();
							string valid_mat_path = $"{valid_path}/Mat";
							if( mat_path.Contains( valid_mat_path ) == false )
							{
								mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}.[{ps.name}] material path invalid={mat_path} != {valid_path}" );
								msg_idx++;
							}
						}
					}
				}
			}
		}

		//------------------------------------------------------------------------		
		string FX_ValidCheck( GameObject selected_obj, bool fix )
		{
			if( selected_obj == null )
				return "";

			int msg_idx = 1;
			mFXValidCheckMessageBuilder.Clear();

			bool fx_main_is_new = false;
			FXMain fx_main = null;
			FXMain[] fx_main_list = selected_obj.GetComponents<FXMain>();
			if( fx_main_list == null || fx_main_list.Length == 0 )
			{
				if( fix )
				{
					fx_main = selected_obj.AddComponent<FXMain>();
				}
				else
				{
					mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}. FXMain not found!" );
					msg_idx++;
				}
			}
			else if( fx_main_list.Length > 1 )
			{
				fx_main = fx_main_list[0];
				if( fix )
				{
					for( int i = 1; i < fx_main_list.Length; i++ )
						DestroyImmediate( fx_main_list[i] );
				}
				else
				{
					mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}. FXMain multiple. only one used!" );
					msg_idx++;
				}
			}

			int child_count = selected_obj.transform.childCount;
			if( child_count <= 0 )
			{
				mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}. has not child( _Cast, _Fire, _Hit, ...)" );
				msg_idx++;
			}
			else
			{
				for( int i = 0; i < child_count; i++ )
				{
					GameObject child = selected_obj.transform.GetChild( i ).gameObject;
					string child_name_trim = child.name.Trim();
					if( FX_TYPE_NAME_LIST.Contains( child_name_trim ) )
					{
						if( FX_TYPE_NAME_LIST.Contains( child.name ) == false )
						{
							if( fix )
							{
								child.name = child_name_trim;
							}
							else
							{
								mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}.[{child.name}] invalid fx type root name SPACE." );
								msg_idx++;
							}
						}

						if( child_name_trim == FXMain.CAST_OBJ_NAME )
							_FX_ValidCheck_FXType( fx_main, child, FXMain.eFX_Type.Cast, ref msg_idx, fix );
						else if( child_name_trim == FXMain.FIRE_OBJ_NAME )
							_FX_ValidCheck_FXType( fx_main, child, FXMain.eFX_Type.Fire, ref msg_idx, fix );
						else if( child_name_trim == FXMain.PROJECTILE_OBJ_NAME )
							_FX_ValidCheck_FXType( fx_main, child, FXMain.eFX_Type.Projectile, ref msg_idx, fix );
						else if( child_name_trim == FXMain.HIT_OBJ_NAME )
							_FX_ValidCheck_FXType( fx_main, child, FXMain.eFX_Type.Hit, ref msg_idx, fix );
						else if( child_name_trim == FXMain.FHIT_OBJ_NAME )
							_FX_ValidCheck_FXType( fx_main, child, FXMain.eFX_Type.FuncHit, ref msg_idx, fix );
						else if( child_name_trim == FXMain.CASTLOOP_OBJ_NAME )
							_FX_ValidCheck_FXType( fx_main, child, FXMain.eFX_Type.Castloop, ref msg_idx, fix );
						else if( child_name_trim == FXMain.REMOVE_OBJ_NAME )
							_FX_ValidCheck_FXType( fx_main, child, FXMain.eFX_Type.Remove, ref msg_idx, fix );
						else if( child_name_trim == FXMain.UPDATE_OBJ_NAME )
							_FX_ValidCheck_FXType( fx_main, child, FXMain.eFX_Type.Update, ref msg_idx, fix );
					}
					else
					{
						mFXValidCheckMessageBuilder.AppendLine( $"{msg_idx}.[{child.name}] invalid fx type root name object." );
						msg_idx++;
					}
				}
			}

			ParticleSystem[] ps_list = selected_obj.GetComponentsInChildren<ParticleSystem>( true );
			if( ps_list != null )
			{
				foreach( ParticleSystem ps in ps_list )
				{
					_CheckParticleSystemValid( selected_obj, ps, fix, ref msg_idx );
				}
			}

			if( fix && fx_main != null && fx_main_is_new )
			{
				mSelectedFXMain = fx_main;
				UpdateSerializeObject();
			}

			return mFXValidCheckMessageBuilder.ToString().TrimEnd();
		}

		private void Update()
		{
			if( mSelectedFXMain != null )
			{
				mSelectedFXMain.OnEditorUpdate();
				EditorUtility.SetDirty( mSelectedFXMain );
			}
		}

		//------------------------------------------------------------------------
		bool tmp_do_close = false;
		private void OnGUI()
		{
			if( mTool == null )
			{
				EditorGUILayout.HelpBox( "FXTool invalid. open with FXTool inspector's open button or waiting refresh.", MessageType.Warning );
				GUILayout.BeginHorizontal();
				if( GUILayout.Button( "CLOSE" ) )
				{
					Close();
				}
				if( GUILayout.Button("Refresh"))
				{
					Repaint();
				}
				GUILayout.EndHorizontal();
				return;
			}

			mScrollPos = GUILayout.BeginScrollView( mScrollPos );
			InspectorUtil.BeginContents( "Editor Window" );
			GUILayout.BeginHorizontal();
			if( GUILayout.Button( "Resource Cache Reset", GUILayout.Width( 150f ) ) )
			{
				CacheReset( true );
			}

			tmp_do_close = false;
			if( GUILayout.Button( "Close", GUILayout.Width( 150f ) ) )
				tmp_do_close = true;

			GUILayout.EndHorizontal();
			InspectorUtil.EndContents();

			mSelectionToolExpand = InspectorUtil.DrawHeaderFoldable( "Selection", mSelectionToolExpand );
			if( mSelectionToolExpand )
			{
				DrawBGBattleSpriteSelect();
				DrawBGQuestSpriteSelect();
				DrawCardSelect();
				DrawUISelect();
				DrawFXSelect();
			}

			DrawPlayModeTool();
			DrawFXTool();
			DrawPrefabTool();
			DrawDevBatchProcess();

			GUILayout.EndScrollView();

			if( tmp_do_close )
				Close();
		}

		//------------------------------------------------------------------------
		void CacheReset( bool forced = false )
		{
			FX_TYPE_NAME_LIST = new List<string>();
			FX_TYPE_NAME_LIST.Add( FXMain.CAST_OBJ_NAME );
			FX_TYPE_NAME_LIST.Add( FXMain.CASTLOOP_OBJ_NAME );
			FX_TYPE_NAME_LIST.Add( FXMain.FIRE_OBJ_NAME );
			FX_TYPE_NAME_LIST.Add( FXMain.PROJECTILE_OBJ_NAME );
			FX_TYPE_NAME_LIST.Add( FXMain.HIT_OBJ_NAME );
			FX_TYPE_NAME_LIST.Add( FXMain.FHIT_OBJ_NAME );
			FX_TYPE_NAME_LIST.Add( FXMain.REMOVE_OBJ_NAME );
			FX_TYPE_NAME_LIST.Add( FXMain.UPDATE_OBJ_NAME );

			FXTool tool = FXTool.Instance;
			if( tool == null )
				return;

			if( forced )
			{
				mBGBattleCached = null;
				mBGQuestCached = null;
				mCardCached = null;
				mUICached = null;
				mFXCached = null;
			}

			if( mBGBattleCached == null )
				mBGBattleCached = AlphabetSortedAsset.CreateFindedAssetContainer<Sprite>( FXTool.BG_BATTLE_RESOURCE_ASSET_PATH, typeof( Sprite ).Name, "bg_battle_" );

			if( mBGQuestCached == null )
				mBGQuestCached = AlphabetSortedAsset.CreateFindedAssetContainer<Sprite>( FXTool.BG_QUEST_RESOURCE_ASSET_PATH, typeof( Sprite ).Name, "bg_quest_" );

			if( mCardCached == null )
				mCardCached = AlphabetSortedAsset.CreateFindedAssetContainer<Sprite>( FXTool.CARD_RESOURCE_ASSET_PATH, typeof( Sprite ).Name, "img_card_" );

			if( mUICached == null )
				mUICached = AlphabetSortedAsset.CreateFindedAssetContainer<Sprite>( FXTool.UI_RESOURCE_ASSET_PATH, typeof( Sprite ).Name, "img_ui_" );

			_CacheReset_FX( false );
		}
		void _CacheReset_FX( bool forced )
		{
			if( mFXCached == null || forced )
			{
				mFXCached = CreateFXContainerList();
			}
		}
		public static List<FXContainer> CreateFXContainerList()
		{
			List<FXContainer> list = new List<FXContainer>();

			string root_path = FXTool.FX_PREFAB_ASSET_PATH;
			string[] sub_dirs = Directory.GetDirectories( root_path );
			foreach( string sub in sub_dirs )
			{
				FXContainer container = new FXContainer();
				container.category = Path.GetFileName( sub );
				container.fx_prefab_cached = AlphabetSortedAsset.CreateFindedAssetContainer<GameObject>( sub, "prefab", "" );

				list.Add( container );
			}

			return list;
		}

		//------------------------------------------------------------------------
		void DrawSpriteBase( GameObject go, bool is_enabled, Sprite sprite )
		{
			GUILayout.Label( sprite != null ? sprite.name : "" );

			if( GUILayout.Button( is_enabled ? mHideBtnText : mShowBtnText, GUILayout.Width( 30f ) ) )
			{
				SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
				if( sr != null )
					sr.enabled = !sr.enabled;
				else
				{
					Image image = go.GetComponent<Image>();
					if( image != null )
						image.enabled = !image.enabled;
				}

			}

			if( GUILayout.Button( mSelectBtnText, GUILayout.Width( 30f ) ) )
			{
				EditorGUIUtility.PingObject( go );
				Selection.activeObject = go;
			}

			GUI.enabled = ( sprite != null );
			if( GUILayout.Button( mResourceSelectBtnText, GUILayout.Width( 30f ) ) )
			{
				EditorGUIUtility.PingObject( sprite );
				Selection.activeObject = sprite;
			}

			GUI.enabled = true;
		}

		//------------------------------------------------------------------------
		void DrawAlphabetSortedContainer<T>( AlphabetSortedContainer<T> container, GUIContent btn_text, Color color, System.Action other_draw, System.Action<T> update_callback ) where T : UnityEngine.Object
		{
			FXTool tool = FXTool.Instance;
			if( tool == null )
				return;

			Color gui_color = GUI.color;
			GUI.color = color;
			GUILayout.BeginHorizontal();

			if( EditorGUILayout.DropdownButton( btn_text, FocusType.Passive, GUILayout.Width( 150f ) ) )
			{
				if( container != null )
				{
					GenericMenu menu = new GenericMenu();

					foreach( AlphabetSortedContainerData<T> data in container.list )
					{
						foreach( AlphabetSortedAssetData<T> asset_data in data.asset_list )
						{
							menu.AddItem( new GUIContent( $"{data.alphabet}/{asset_data.short_name}", $"change {asset_data.short_name}" ), false, () =>
							{
								asset_data.LoadAsset();
								update_callback( asset_data.asset );
							} );
						}
					}

					menu.ShowAsContext();
				}
			}

			other_draw?.Invoke();

			GUILayout.EndHorizontal();

			GUI.color = gui_color;
		}

		//------------------------------------------------------------------------
		void DrawBGBattleSpriteSelect()
		{
			FXTool tool = FXTool.Instance;
			if( tool == null )
				return;

			CacheReset();
			DrawAlphabetSortedContainer( mBGBattleCached, mBGBattleLoadText, mBGColor, () =>
			{
				if( tool.m_BackgroundSprite.sprite != null && tool.m_BackgroundSprite.sprite.name.StartsWith( "bg_battle" ) )
					DrawSpriteBase( tool.m_BackgroundSprite.gameObject, tool.m_BackgroundSprite.enabled, tool.m_BackgroundSprite.sprite );
			}, ( Sprite sp ) =>
			{
				tool.UpdateBackground( sp );
			} );
		}
		void DrawBGQuestSpriteSelect()
		{
			FXTool tool = FXTool.Instance;
			if( tool == null )
				return;

			CacheReset();
			DrawAlphabetSortedContainer( mBGQuestCached, mBGQuestLoadText, mBGColor, () =>
			{
				if( tool.m_BackgroundSprite.sprite != null && tool.m_BackgroundSprite.sprite.name.StartsWith( "bg_quest" ) )
					DrawSpriteBase( tool.m_BackgroundSprite.gameObject, tool.m_BackgroundSprite.enabled, tool.m_BackgroundSprite.sprite );
			}, ( Sprite sp ) =>
			{
				tool.UpdateBackground( sp );
			} );
		}

		//------------------------------------------------------------------------
		void DrawCardSelect()
		{
			FXTool tool = FXTool.Instance;
			if( tool == null )
				return;

			CacheReset();
			DrawAlphabetSortedContainer( mCardCached, mCardLoadText, mCardColor, () =>
			{
				DrawSpriteBase( tool.m_CardImage.gameObject, tool.m_CardImage.enabled, tool.m_CardImage.sprite );
			}, ( Sprite sp ) =>
			{
				tool.UpdateCard( sp );
			} );
		}

		//------------------------------------------------------------------------
		void DrawUISelect()
		{
			FXTool tool = FXTool.Instance;
			if( tool == null )
				return;

			CacheReset();
			DrawAlphabetSortedContainer( mUICached, mUILoadText, mUIColor, () =>
			{
				DrawSpriteBase( tool.m_UIImage.gameObject, tool.m_UIImage.enabled, tool.m_UIImage.sprite );
			}, ( Sprite sp ) =>
			{
				tool.UpdateUIImage( sp );
			} );
		}

		//------------------------------------------------------------------------
		GameObject _GetOrCreateChildObj( GameObject parent, string child_name )
		{
			int child_count = parent.transform.childCount;
			for( int i=0; i<child_count; i++ )
			{
				GameObject child = parent.transform.GetChild( i ).gameObject;
				if( child.name == child_name )
				{
					return child;
				}
			}

			GameObject new_child = parent.AddChild();
			new_child.name = child_name;
			return new_child;
		}

		//------------------------------------------------------------------------
		void DrawFXSelect()
		{
			FXTool tool = FXTool.Instance;
			if( tool == null )
				return;

			if( Application.isPlaying )
				GUI.enabled = false;

			Color gui_color = GUI.color;
			GUI.color = mFXColor;
			GUILayout.BeginHorizontal();

			if( EditorGUILayout.DropdownButton( mFXLoadText, FocusType.Passive, GUILayout.Width( 150f ) ) )
			{
				CacheReset();

				if( mFXCached != null )
				{
					GenericMenu menu = new GenericMenu();

					foreach( FXContainer fx in mFXCached )
					{
						foreach( AlphabetSortedContainerData<GameObject> data in fx.fx_prefab_cached.list )
						{
							foreach( AlphabetSortedAssetData<GameObject> asset_data in data.asset_list )
							{
								menu.AddItem( new GUIContent( $"{fx.category}/{data.alphabet}/{asset_data.short_name}", $"change {asset_data.short_name}" ), false, () =>
								{
									asset_data.LoadAsset();
									if( asset_data.asset != null )
									{
										GameObject fx_parent = _GetOrCreateChildObj( mTool.m_FXParent, fx.category );

										List<GameObject> child_list = fx_parent.GetComponentsInChildren<Transform>( true ).Select(a => a.gameObject).ToList();
										GameObject load_fx = child_list.Find( a => a.name == asset_data.asset_name );
										if( load_fx == null )
											load_fx = PrefabUtility.InstantiatePrefab( asset_data.asset, fx_parent.transform ) as GameObject;

										Selection.activeObject = load_fx;
										EditorGUIUtility.PingObject( load_fx );
									}
								} );
							}
						}
					}

					menu.ShowAsContext();
				}
			}

			GUILayout.EndHorizontal();

			GUI.color = gui_color;
			GUI.enabled = true;
		}

		//------------------------------------------------------------------------
		void DrawPlayModeTool()
		{
			Color gui_color = GUI.color;

			InspectorUtil.BeginContents( $"Play Mode Tool" );
			
			bool is_forced_repaint = false;

			GUI.color = Color.red;
			if( Application.isPlaying )
			{
				if( GUILayout.Button( "PLAY MODE TEST STOP", GUILayout.Height( 30f ) ) )
				{
					EditorApplication.ExitPlaymode();
				}
			}
			else
			{
				if( GUILayout.Button( "PLAY MODE TEST START", GUILayout.Height( 30f ) ) )
				{
					mTool.m_TestFX = mSelectedFXMain;
					EditorUtility.SetDirty( mTool );
					EditorApplication.EnterPlaymode();
				}
			}
			GUI.color = gui_color;

			InspectorUtil.EndContents();

			if( is_forced_repaint )
			{
				Repaint();
			}
		}

		//------------------------------------------------------------------------
		void DrawFXTool()
		{
			Color gui_color = GUI.color;

			mFXtoolExpand = InspectorUtil.DrawHeaderFoldable( "FX Tool", mFXtoolExpand );
			if( mFXtoolExpand )
			{
				InspectorUtil.BeginContents();
				bool is_forced_repaint = false;
				bool root_gui_enabled = ( Application.isPlaying == false );
				GUI.enabled = root_gui_enabled;

				SceneView curr_scene = SceneView.lastActiveSceneView;
				if( curr_scene != null && curr_scene.sceneViewState.alwaysRefreshEnabled == false )
				{
					GUI.color = Color.red;
					EditorGUILayout.HelpBox( "Scene view : Always Refresh is disabled", MessageType.Warning );
					if( GUILayout.Button( "FIX" ) )
						curr_scene.sceneViewState.alwaysRefresh = true;

					GUI.color = gui_color;
				}

				if( mSelectedFXObject == null )
				{
					EditorGUILayout.HelpBox( "Selected FX = None", MessageType.Info );
				}
				else
				{
					GUI.color = Color.cyan;
					if( GUILayout.Button( $"Selected FX = {mSelectedFXObject.name}", GUILayout.Height( 30f ) ) )
					{
						EditorGUIUtility.PingObject( mSelectedFXObject );
					}
					GUI.color = gui_color;

					GUILayout.BeginHorizontal();

					// object 
					if( mFXCheckedMessage.Length > 0 )
						GUI.color = Color.red;
					if( GUILayout.Button( "Valid Check", GUILayout.Width( 80f ) ) )
					{
						mFXCheckedMessage = FX_ValidCheck( mSelectedFXObject, false );
					}
					GUI.color = gui_color;

					if( GUILayout.Button( "V2 FXMain SET or FIX", GUILayout.Width( 130f ) ) )
					{
						mFXCheckedMessage = FX_ValidCheck( mSelectedFXObject, true );
					}

					GUILayout.EndHorizontal();


					if( string.IsNullOrEmpty( mFXCheckedMessage ) == false )
						EditorGUILayout.HelpBox( mFXCheckedMessage, MessageType.Warning );

					if( mSelectedFXMain == null )
					{
						EditorGUILayout.HelpBox( "FXMain not found. can not control.", MessageType.Warning );
					}
					else
					{
						InspectorUtil.DrawLine( Color.gray );
						GUILayout.Label( "* FX Play Control" );
						mSelectedFXMain.m_EditorDeltaTimeScale = EditorGUILayout.Slider( "Timescale", mSelectedFXMain.m_EditorDeltaTimeScale, 0.1f, 1f );

						GUILayout.BeginHorizontal();

						// control
						GUI.color = Color.green;
						if( GUILayout.Button( "FXMain", GUILayout.Width( 80f ) ) )
						{
							EditorGUIUtility.PingObject( mSelectedFXMain );
						}
						GUILayout.Button( "", GUILayout.Width( 10f ) );

						if( GUILayout.Button( "\u25b6\u25ae Restart", GUILayout.Width( 80f ) ) )
						{
							SoundSetting.Instance.EditorAudioClipPlayHandler = UMFEditorUtil.PlayAudioClip;
							mSelectedFXMain.Stop();
							CheckFXProjectileTarget( mSelectedFXMain, null );
							mSelectedFXMain.Play();							
						}
						if( mSelectedFXMain.EditorIsPaused )
						{
							if( GUILayout.Button( "\u25af Resume", GUILayout.Width( 80f ) ) )
								mSelectedFXMain.Editor_PauseAll();
						}
						else if( mSelectedFXMain.IsPlaying )
						{
							if( GUILayout.Button( "\u25af Pause", GUILayout.Width( 80f ) ) )
								mSelectedFXMain.Editor_PauseAll();
						}
						else
						{
							if( GUILayout.Button( "\u25b6 Play", GUILayout.Width( 80f ) ) )
							{
								SoundSetting.Instance.EditorAudioClipPlayHandler = UMFEditorUtil.PlayAudioClip;
								CheckFXProjectileTarget( mSelectedFXMain, null );
								mSelectedFXMain.Play();
							}
						}
						if( GUILayout.Button( "\u25ae Stop", GUILayout.Width( 80f ) ) )
						{
							mSelectedFXMain.Stop();
						}
						GUI.color = gui_color;

						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						GUILayout.Space( 100f );
						GUILayout.Label( string.Format( "{0:F}s ( MaxDuration:{1:F}s )", mSelectedFXMain.PlayTime, mSelectedFXMain.MaxDuration ) );
						if( mSelectedFXMain.IsPlaying )
							is_forced_repaint = true;

						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Label( "Time:", GUILayout.Width( 40f ) );
						float slider = EditorGUILayout.Slider( mSelectedFXMain.PlayTime, 0f, mSelectedFXMain.MaxDuration );
						GUILayout.EndHorizontal();
						if( slider != mSelectedFXMain.PlayTime )
							mSelectedFXMain.Editor_SetPlayTime( slider );

						InspectorUtil.DrawLine( Color.gray, 1 );

						bool gui_enabled = root_gui_enabled;
						if( gui_enabled )
							gui_enabled = ( mSelectedFXMain.IsPlaying == false );

						_DrawFXPlayControl( mSelectedFXMain.m_Cast, "Cast", gui_enabled, ref is_forced_repaint );
						_DrawFXPlayControl( mSelectedFXMain.m_CastLoop, "Castloop", gui_enabled, ref is_forced_repaint );
						_DrawFXPlayControl( mSelectedFXMain.m_Fire, "Fire", gui_enabled, ref is_forced_repaint );
						_DrawFXPlayControl( mSelectedFXMain.m_Projectile, "Projectile", gui_enabled, ref is_forced_repaint );
						_DrawFXPlayControl( mSelectedFXMain.m_Hit, "Hit", gui_enabled, ref is_forced_repaint );
						_DrawFXPlayControl( mSelectedFXMain.m_FuncHit, "FHit", gui_enabled, ref is_forced_repaint );
						_DrawFXPlayControl( mSelectedFXMain.m_Remove, "Remove", gui_enabled, ref is_forced_repaint );
						_DrawFXPlayControl( mSelectedFXMain.m_Update, "Update", gui_enabled, ref is_forced_repaint );
					}
				}

				InspectorUtil.EndContents();
				GUI.enabled = true;
				if( is_forced_repaint )
					Repaint();
			}
		}

		//------------------------------------------------------------------------
		void _DrawFXPlayControl( FXPlayBase fx, string fx_type_name, bool gui_enabled, ref bool is_repaint )
		{
			if( fx == null )
				return;

			SerializedObject serialize_object = GetSerializeObject( fx.FXTYPE, fx );
			if( serialize_object != null )
				serialize_object.Update();

			GUI.enabled = gui_enabled;
			Color gui_color = GUI.color;

			GUILayout.BeginHorizontal();

			GUI.color = Color.yellow;
			if( GUILayout.Button( fx_type_name, GUILayout.Width( 80f ) ) )
			{
				EditorGUIUtility.PingObject( fx.gameObject );
			}
			GUILayout.Button( "", GUILayout.Width( 10f ) );
			GUI.color = gui_color;

			// control
			if( GUILayout.Button( "\u25b6\u25ae Restart", GUILayout.Width( 80f ) ) )
			{
				SoundSetting.Instance.EditorAudioClipPlayHandler = UMFEditorUtil.PlayAudioClip;
				fx.Stop();
				fx.SoundID = $"{mSelectedFXMain.name}_{fx.FXTYPE}";
				if( fx.FXTYPE == FXMain.eFX_Type.Projectile )
					CheckFXProjectileTarget( null, fx );
				fx.Play( true );
			}
			if( fx.IsPaused )
			{
				if( GUILayout.Button( "\u25af Resume", GUILayout.Width( 80f ) ) )
					fx.Pause( !fx.IsPaused );
			}
			else if( fx.IsPlaying )
			{
				if( GUILayout.Button( "\u25af Pause", GUILayout.Width( 80f ) ) )
					fx.Pause( !fx.IsPaused );
			}
			else
			{
				if( GUILayout.Button( "\u25b6 Play", GUILayout.Width( 80f ) ) )
				{
					SoundSetting.Instance.EditorAudioClipPlayHandler = UMFEditorUtil.PlayAudioClip;
					fx.SoundID = $"{mSelectedFXMain.name}_{fx.FXTYPE}";
					if( fx.FXTYPE == FXMain.eFX_Type.Projectile )
						CheckFXProjectileTarget( null, fx );
					fx.Play( true );
				}
			}
			if( GUILayout.Button( "\u25ae Stop", GUILayout.Width( 80f ) ) )
			{
				fx.Stop();
			}

			GUILayout.EndHorizontal();

			GUI.enabled = gui_enabled;

			GUILayout.BeginHorizontal();
			GUILayout.Space( 100f );

			float play_time = 0f;
			float ps_duration = 0f;
			float max_duration = 0f;
			play_time = fx.PlayTime;
			ps_duration = fx.ParticleMaxDuration;
			max_duration = fx.MaxDuration;

			GUILayout.Label( string.Format( "{0:F}s ( ParticleDuration:{1:F}s MaxDuration:{2:F}s )", play_time, ps_duration, max_duration ) );
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUI.color = Color.yellow;
			string sound_id = $"{mSelectedFXMain.name}_{fx.FXTYPE}";
			if( GUILayout.Button( mFXSoundText, GUILayout.Width( 80f ) ) )
			{
				string sound_asset_path = SoundSetting.Instance.GetSoundAssetPath( SoundEnums.eSoundCategory.fx, sound_id );
				AudioClip clip = SoundSetting.Instance.GetEditorAudioClipCache( sound_asset_path );
				if( clip != null )
					EditorGUIUtility.PingObject( clip );
			}
			GUILayout.Button( "", GUILayout.Width( 10f ) );
			GUI.color = gui_color;

			GUILayout.BeginVertical();
			GUILayout.Label( $"Sound ID : {sound_id}" );
			EditorGUI.BeginChangeCheck();

			fx.m_DelayTime = EditorGUILayout.FloatField( mDelayTimeText, fx.m_DelayTime );
			fx.m_UseDelayAddRandom = EditorGUILayout.ToggleLeft( "Use Delay time random add", fx.m_UseDelayAddRandom );
			if( fx.m_UseDelayAddRandom )
			{
				fx.m_DelayTimeAddRandomRange = EditorGUILayout.Vector2Field( mDelayTimeAddRandomText, fx.m_DelayTimeAddRandomRange );
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label( "Time:", GUILayout.Width( 40f ) );
			float slider = EditorGUILayout.Slider( fx.PlayTime, 0f, fx.MaxDuration );
			GUILayout.EndHorizontal();
			if( slider != fx.PlayTime )
				fx.Editor_SetPlayTime( slider );

			switch( fx.FXTYPE )
			{
				case FXMain.eFX_Type.Projectile:
					InspectorUtil.DrawHeader( "Property" );
					{
						FXPlayProjectile projectile_fx = fx as FXPlayProjectile;
						projectile_fx.m_Knot0_TangentOut_Z_Multiply = EditorGUILayout.FloatField( mProjectileTangentZText, projectile_fx.m_Knot0_TangentOut_Z_Multiply );
						projectile_fx.m_UseAddAnimateRandomTime = EditorGUILayout.Toggle( mProjectileUseAddRandomTimeText, projectile_fx.m_UseAddAnimateRandomTime );
						if( projectile_fx.m_UseAddAnimateRandomTime )
							projectile_fx.m_AddAnimateRandomTime = EditorGUILayout.Vector2Field( mProjectileAddRandomTimeText, projectile_fx.m_AddAnimateRandomTime );
					}
					break;

				case FXMain.eFX_Type.Hit:
				case FXMain.eFX_Type.FuncHit:
					InspectorUtil.DrawHeader( "Property" );
					{
						FXPlayHit hit_fx = fx as FXPlayHit;

						if( GUILayout.Button( mAddCallbackTime ) )
						{
							hit_fx.m_HitCallbackTimeList.Add( fx.PlayTime );
							hit_fx.m_HitCallbackTimeList.Sort( ( a, b ) => { return a.CompareTo( b ); } );
						}

						if( serialize_object != null )
						{
							SerializedProperty sp_hit_callback = serialize_object.FindProperty( "m_HitCallbackTimeList" );
							if( sp_hit_callback != null )
							{
								EditorGUILayout.PropertyField( sp_hit_callback );
							}
						}
					}
					break;
			}

			InspectorUtil.DrawLine( Color.gray, 1 );
			if( EditorGUI.EndChangeCheck() )
			{
				fx.UpdateTimeSet();
				mSelectedFXMain.Editor_UpdateTimeSet();
				is_repaint = true;

				if( serialize_object != null )
					serialize_object.ApplyModifiedProperties();

				EditorUtility.SetDirty( fx );
			}
			GUILayout.EndVertical();

			GUI.enabled = gui_enabled;
			GUILayout.EndHorizontal();

			if( fx.IsPlaying )
				is_repaint = true;
		}

		//------------------------------------------------------------------------
		void CheckFXProjectileTarget( FXMain fx_main, FXPlayBase fx_play_base )
		{
			FXPlayProjectile fx = fx_play_base as FXPlayProjectile;
			if( fx == null && fx_main != null )
			{
				fx = fx_main.m_Projectile;
			}

			if( fx != null && mTool != null && mTool.m_EditorCardDummyTarget != null )
			{
				fx.SetTargetPosition( mTool.m_EditorCardDummyTarget.transform );
			}
		}

		//------------------------------------------------------------------------
		void DrawPrefabTool()
		{
			mPrefabToolExpand = InspectorUtil.DrawHeaderFoldable( "Prefab Tool", mPrefabToolExpand );
			if( mPrefabToolExpand )
			{
				EditorGUILayout.HelpBox( "DO NOT APPLY! if you not developer!", MessageType.Warning );

				InspectorUtil.BeginContents();
				InspectorUtil.Draw_PrefabControl( mSelectedFXObject, FX_PREFAB_PATH, null, null );
				InspectorUtil.EndContents();
			}
		}			

		//------------------------------------------------------------------------
		void DrawDevBatchProcess()
		{
			mBatchProcessExpand = InspectorUtil.DrawHeaderFoldable( "Batch Process : program team only", mBatchProcessExpand );
			if( mBatchProcessExpand )
			{
				InspectorUtil.BeginContents();

				EditorGUILayout.HelpBox( "Batch process tool. Used only program team", MessageType.Warning );

				if( Application.isPlaying )
				{
					EditorGUILayout.HelpBox( "Playing mode not support.", MessageType.Error );
				}
				else
				{
					if( GUILayout.Button( "Check Valid FX ALL" ) )
					{
						string prev_saved = mFXCheckedMessage;
						_CacheReset_FX( true );
						List<UMFSelectionWindow.SelectionData> invalid_list = new List<UMFSelectionWindow.SelectionData>();
						foreach( FXContainer container in mFXCached )
						{
							foreach( AlphabetSortedContainerData<GameObject> container_data in container.fx_prefab_cached.list )
							{
								foreach( AlphabetSortedAssetData<GameObject> asset_data in container_data.asset_list )
								{
									asset_data.LoadAsset();
									if( asset_data.asset != null )
									{
										string checked_msg = FX_ValidCheck( asset_data.asset, false );
										if( string.IsNullOrEmpty( checked_msg ) == false )
										{
											UMFSelectionWindow.SelectionData s_data = new UMFSelectionWindow.SelectionData();
											s_data.game_object = asset_data.asset;
											s_data.description = checked_msg;

											invalid_list.Add( s_data );
										}
									}
								}
							}
						}

						mFXCheckedMessage = prev_saved;
						if( invalid_list.Count > 0 )
						{
							UMFSelectionWindow.Show( invalid_list );
						}
					}

					if( GUILayout.Button( "V2 FXMain SET ALL or FIX" ) )
					{
						_CacheReset_FX( true );
						List<UMFSelectionWindow.SelectionData> invalid_list = new List<UMFSelectionWindow.SelectionData>();
						foreach( FXContainer container in mFXCached )
						{
							foreach( AlphabetSortedContainerData<GameObject> container_data in container.fx_prefab_cached.list )
							{
								foreach( AlphabetSortedAssetData<GameObject> asset_data in container_data.asset_list )
								{
									asset_data.LoadAsset();
									if( asset_data.asset != null )
									{
										GameObject fx_parent = _GetOrCreateChildObj( mTool.m_FXParent, container.category );

										List<GameObject> child_list = fx_parent.GetComponentsInChildren<Transform>( true ).Select( a => a.gameObject ).ToList();
										GameObject load_fx = PrefabUtility.InstantiatePrefab( asset_data.asset, fx_parent.transform ) as GameObject;

										string checked_msg = FX_ValidCheck( load_fx, false );
										if( string.IsNullOrEmpty( checked_msg ) == false )
										{
											FX_ValidCheck( load_fx, true );
											Debug.Log( $"FXMain SET ALL {load_fx.name} save prefab" );

											PrefabUtility.ApplyPrefabInstance( load_fx, InteractionMode.AutomatedAction );
										}

										DestroyImmediate( load_fx );
									}
								}
							}
						}
					}
				}

				InspectorUtil.EndContents();
			}
		}
	}
}