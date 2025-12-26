//////////////////////////////////////////////////////////////////////////
//
// I18NTextFontSetting_TMP_Inspector
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
using TMPro.EditorUtilities;
using TMPro;
using System.IO;
using System.Linq;

namespace UMF.Unity.EditorUtil
{
	[CustomEditor( typeof( I18NTextFontSetting_TMP ), true )]
	public class I18NTextFontSetting_TMP_Inspector : Editor
	{
		const string PRESET_MATERIAL_PATH = "_preset_mat";

		string mLanguageNameText = "";
		List<InspectorUtil.DynamicBtnListBoxData> mLanguageNameBtnList = new List<InspectorUtil.DynamicBtnListBoxData>();
		string mCategoryNameText = "";
		List<InspectorUtil.DynamicBtnListBoxData> mCategoryNameBtnList = new List<InspectorUtil.DynamicBtnListBoxData>();
		string mPresetNameText = "";
		List<InspectorUtil.DynamicBtnListBoxData> mPresetNameBtnList = new List<InspectorUtil.DynamicBtnListBoxData>();

		GUIContent mGUIContent_NameRemove = new GUIContent( "Remove", "remove this" );
		GUIContent mGUIContent_FindUsedPreset = new GUIContent( "Find Used", "find preset use object" );
		GUIContent mGUIContent_PingPresetMat = new GUIContent( "Ping Material", "ping material" );

		bool mDrawBaseInspector = false;
		I18NTextFontSetting_TMP mSetting = null;

		private void OnEnable()
		{
			mSetting = target as I18NTextFontSetting_TMP;

			mLanguageNameBtnList.Clear();
			mCategoryNameBtnList.Clear();
			mPresetNameBtnList.Clear();

			foreach( I18NTextFontSetting_TMP.LanguageFontData data in mSetting.m_DefaultFontList )
			{
				mLanguageNameBtnList.Add( CreateBtn( data.m_Language ) );
			}
			mLanguageNameBtnList = mLanguageNameBtnList.OrderBy( a => a.gui_content.text ).ToList();

			foreach( I18NTextFontSetting_TMP.FontCategoryData data in mSetting.m_CategoryFontList )
			{
				mCategoryNameBtnList.Add( CreateBtn( data.m_Category ) );
			}
			mCategoryNameBtnList = mCategoryNameBtnList.OrderBy( a => a.gui_content.text ).ToList();

			foreach( I18NTextFontSetting_TMP.MaterialPresetData data in mSetting.m_PresetMaterialList )
			{
				mPresetNameBtnList.Add( CreateBtn( data.m_PresetName ) );
			}
			mPresetNameBtnList = mPresetNameBtnList.OrderBy( a => a.gui_content.text ).ToList();
		}

		private void OnDisable()
		{
			mSetting = null;
		}

		InspectorUtil.DynamicBtnListBoxData CreateBtn( string name )
		{
			InspectorUtil.DynamicBtnListBoxData data = new InspectorUtil.DynamicBtnListBoxData();
			data.gui_content = new GUIContent( name, name );

			return data;
		}

		enum eNameType
		{
			Language,
			Category,
			PresetMaterial,
		}

		void DrawNameList( List<InspectorUtil.DynamicBtnListBoxData> btn_list, ref string text_field_text, eNameType name_type )
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label( "Name:", GUILayout.Width( 50f ) );
			text_field_text = GUILayout.TextField( text_field_text, GUILayout.Width( 200f ) );
			if( GUILayout.Button( "Add" ) )
			{
				string add_name = text_field_text;
				if( string.IsNullOrEmpty( add_name ) == false )
				{
					if( EditorUtility.DisplayDialog( "Confirm", $"Add {name_type} data?", "OK", "Cancel" ) )
					{
						switch( name_type )
						{
							case eNameType.Language:
								if( mSetting.m_DefaultFontList.Exists( a => a.m_Language == add_name ) == false )
									Add_Language( add_name, false );
								break;

							case eNameType.Category:
								if( mSetting.m_CategoryFontList.Exists( a => a.m_Category == add_name ) == false )
									Add_Category( add_name, false );
								break;

							case eNameType.PresetMaterial:
								if( mSetting.m_PresetMaterialList.Exists( a => a.m_PresetName == add_name ) == false )
									Add_PresetMaterial( add_name, false );
								break;
						}

						text_field_text = "";
					}
				}
			}

			GUILayout.EndHorizontal();
			InspectorUtil.DynamicBtnListBoxData btn = InspectorUtil.DrawDynamicButtonListBox( btn_list, InspectorUtil.eDynamicBtnListBoxFlag.DropDownBtn | InspectorUtil.eDynamicBtnListBoxFlag.AlignLeft, 20f );
			if( btn != null )
			{
				GenericMenu menu = new GenericMenu();

				menu.AddItem( mGUIContent_NameRemove, false, () =>
				{
					string remove_name = btn.gui_content.text;
					btn_list.Remove( btn );

					switch( name_type )
					{
						case eNameType.Language:
							Add_Language( remove_name, true );
							break;

						case eNameType.Category:
							Add_Category( remove_name, true );
							break;

						case eNameType.PresetMaterial:
							Add_PresetMaterial( remove_name, true );
							break;
					}
				} );

				switch( name_type )
				{
					case eNameType.PresetMaterial:
						menu.AddItem( mGUIContent_PingPresetMat, false, () =>
						{
							I18NTextFontSetting_TMP.MaterialPresetData mat_data = mSetting.m_PresetMaterialList.Find( a => a.m_PresetName == btn.gui_content.text );
							if( mat_data != null && mat_data.m_BaseMaterial != null )
								EditorGUIUtility.PingObject( mat_data.m_BaseMaterial );
						} );

						menu.AddItem( mGUIContent_FindUsedPreset, false, () =>
						{
							FindPresetUsedObjects( btn.gui_content.text );
						} );
						break;
				}

				menu.ShowAsContext();
			}
		}

		bool mIsDirty = false;
		public override void OnInspectorGUI()
		{
			if( mSetting == null )
				return;

			Color gui_color = GUI.color;				

			GUILayout.Label( "Util" );
			GUILayout.BeginHorizontal();
			if( GUILayout.Button( "Font Data Clear" ) )
			{
				mSetting.ClearFontAssetData();
				AssetDatabase.Refresh();
			}
			GUI.color = Color.red;
			if( GUILayout.Button( "SAVE!" ) )
			{
				mIsDirty = true;
			}
			GUI.color = gui_color;
			GUILayout.EndHorizontal();

			InspectorUtil.DrawHeader( "Manage" );
			InspectorUtil.BeginContentsScope( "Language", () =>
			{
				DrawNameList( mLanguageNameBtnList, ref mLanguageNameText, eNameType.Language );
			} );

			InspectorUtil.BeginContentsScope( "Category", () =>
			{
				DrawNameList( mCategoryNameBtnList, ref mCategoryNameText, eNameType.Category );
			} );

			InspectorUtil.BeginContentsScope( "Preset Material", () =>
			{
				DrawNameList( mPresetNameBtnList, ref mPresetNameText, eNameType.PresetMaterial );
			} );

			InspectorUtil.DrawHeader( "Setting" );
			InspectorUtil.BeginContentsScope( "Default Language Font List", () =>
			{
				foreach( I18NTextFontSetting_TMP.LanguageFontData font_data in mSetting.m_DefaultFontList )
				{
					GUILayout.Label( $"{font_data.m_Language}", GUILayout.Width( 150f ) );
					GUILayout.BeginHorizontal();
					GUILayout.Label( "  Font Asset", GUILayout.Width( 100f ) );
					EditorGUI.BeginChangeCheck();
					font_data.m_FontAsset = EditorGUILayout.ObjectField( font_data.m_FontAsset, typeof( TMP_FontAsset ), false ) as TMP_FontAsset;
					if( EditorGUI.EndChangeCheck() )
						mIsDirty = true;
					GUILayout.EndHorizontal();
				}
			} );

			InspectorUtil.BeginContentsScope( "Font Category", () =>
			{
				foreach( I18NTextFontSetting_TMP.FontCategoryData cat_data in mSetting.m_CategoryFontList )
				{
					cat_data._EditorExpand = InspectorUtil.DrawHeaderFoldable( cat_data.m_Category, cat_data._EditorExpand );
					InspectorUtil.BeginContentsScope( () =>
					{
						if( cat_data._EditorExpand )
						{
							foreach( I18NTextFontSetting_TMP.LanguageFontData font_data in cat_data.m_FontDataList )
							{
								GUILayout.Label( $"{font_data.m_Language}", GUILayout.Width( 150f ) );
								GUILayout.BeginHorizontal();
								GUILayout.Label( "  Font Asset", GUILayout.Width( 100f ) );
								EditorGUI.BeginChangeCheck();
								font_data.m_FontAsset = EditorGUILayout.ObjectField( font_data.m_FontAsset, typeof( TMP_FontAsset ), false ) as TMP_FontAsset;
								if( EditorGUI.EndChangeCheck() )
									mIsDirty = true;
								GUILayout.EndHorizontal();
							}
						}
					} );
				}
			} );

			InspectorUtil.BeginContentsScope( "Preset Material", () =>
			{
				foreach( I18NTextFontSetting_TMP.MaterialPresetData mat_data in mSetting.m_PresetMaterialList )
				{
					mat_data._EditorExpand = InspectorUtil.DrawHeaderFoldable( mat_data.m_PresetName, mat_data._EditorExpand );
					InspectorUtil.BeginContentsScope( () =>
					{
						if( mat_data._EditorExpand )
						{
							EditorGUI.BeginChangeCheck();
							mat_data.m_BaseMaterial = EditorGUILayout.ObjectField( "Base Material", mat_data.m_BaseMaterial, typeof( Material ), false ) as Material;
							if( EditorGUI.EndChangeCheck() )
								mIsDirty = true;

							EditorGUI.BeginDisabledGroup( true );
							foreach( I18NTextFontSetting_TMP.PresetMaterialRuntimeData runtime_mat in mat_data.m_RuntimeMaterialList )
							{
								GUILayout.BeginHorizontal();
								EditorGUILayout.ObjectField( "Font", runtime_mat.m_FontAsset, typeof( TMP_FontAsset ), false );
								EditorGUILayout.ObjectField( "Material", runtime_mat.m_Material, typeof( Material ), false );
								GUILayout.EndHorizontal();
							}
							EditorGUI.EndDisabledGroup();
						}
					} );
				}
			} );

			InspectorUtil.DrawLine( Color.cyan );
			mDrawBaseInspector = InspectorUtil.DrawHeaderFoldable( "Base Inspector", mDrawBaseInspector );
			if( mDrawBaseInspector )
			{
				EditorGUI.BeginChangeCheck();
				base.OnInspectorGUI();
				if( EditorGUI.EndChangeCheck() )
					mIsDirty = true;
			}

			if( mIsDirty )
			{
				EditorUtility.SetDirty( mSetting );
				AssetDatabase.SaveAssets();
				mIsDirty = false;
			}
		}

		//------------------------------------------------------------------------
		void Add_Language( string language, bool remove )
		{
			mIsDirty = true;

			if( remove )
			{
				mSetting.m_CategoryFontList.ForEach( a => a.m_FontDataList.RemoveAll( b => b.m_Language == language ) );
				mSetting.m_DefaultFontList.RemoveAll( a => a.m_Language == language );
				mLanguageNameBtnList.RemoveAll( a => a.gui_content.text == language );
				return;
			}

			I18NTextFontSetting_TMP.LanguageFontData data = new I18NTextFontSetting_TMP.LanguageFontData();
			data.m_Language = language;

			mSetting.m_DefaultFontList.Add( data );
			mLanguageNameBtnList.Add( CreateBtn( language ) );
			mLanguageNameBtnList = mLanguageNameBtnList.OrderBy( a => a.gui_content.text ).ToList();

			foreach( I18NTextFontSetting_TMP.FontCategoryData cat_data in mSetting.m_CategoryFontList )
			{
				if( cat_data.m_FontDataList.Exists( a => a.m_Language == language ) == false )
				{
					I18NTextFontSetting_TMP.LanguageFontData cat_font = new I18NTextFontSetting_TMP.LanguageFontData();
					cat_font.m_Language = language;

					cat_data.m_FontDataList.Add( cat_font );
				}
			}
		}

		//------------------------------------------------------------------------
		void Add_Category( string category_name, bool remove )
		{
			mIsDirty = true;

			if( remove )
			{
				mSetting.m_CategoryFontList.RemoveAll( a => a.m_Category == category_name );
				mCategoryNameBtnList.RemoveAll( a => a.gui_content.text == category_name );
				return;
			}

			I18NTextFontSetting_TMP.FontCategoryData data = new I18NTextFontSetting_TMP.FontCategoryData();
			data.m_Category = category_name;
			foreach( I18NTextFontSetting_TMP.LanguageFontData def_font in mSetting.m_DefaultFontList )
			{
				I18NTextFontSetting_TMP.LanguageFontData cat_font = new I18NTextFontSetting_TMP.LanguageFontData();
				cat_font.m_Language = def_font.m_Language;
				data.m_FontDataList.Add( cat_font );
			}

			mSetting.m_CategoryFontList.Add( data );
			mCategoryNameBtnList.Add( CreateBtn( category_name ) );
			mCategoryNameBtnList = mCategoryNameBtnList.OrderBy( a => a.gui_content.text ).ToList();
		}

		//------------------------------------------------------------------------
		void Add_PresetMaterial( string preset_name, bool remove )
		{
			mIsDirty = true;

			if( remove )
			{
				I18NTextFontSetting_TMP.MaterialPresetData remove_data = mSetting.m_PresetMaterialList.Find( a => a.m_PresetName == preset_name );
				if( remove_data != null )
				{
					bool is_dirty = AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( remove_data.m_BaseMaterial ) );

					mSetting.m_PresetMaterialList.Remove( remove_data );
					mPresetNameBtnList.RemoveAll( a => a.gui_content.text == preset_name );

					if( is_dirty )
						AssetDatabase.Refresh();
				}
				return;
			}

			Shader default_Shader = Shader.Find( "TextMeshPro/Distance Field" );
			Material new_mat = new Material( default_Shader );
			new_mat.name = "preset_" + preset_name + " Material";

			string preset_mat_path = "";
			TMP_FontAsset def_tmp_font = TMP_Settings.defaultFontAsset;
			if( def_tmp_font != null )
			{
				string def_font_path = System.IO.Path.GetDirectoryName( AssetDatabase.GetAssetPath( def_tmp_font ) ).NormalizeSlashPath();
				preset_mat_path = def_font_path + "/" + PRESET_MATERIAL_PATH;
			}
			else
			{
				preset_mat_path = "Assets/_I18NTextFont/" + PRESET_MATERIAL_PATH;
			}

			if( System.IO.Directory.Exists( preset_mat_path ) == false )
				System.IO.Directory.CreateDirectory( preset_mat_path );

			preset_mat_path = preset_mat_path + "/" + new_mat.name + ".mat";
			AssetDatabase.CreateAsset( new_mat, AssetDatabase.GenerateUniqueAssetPath( preset_mat_path ) );
			Debug.Log( $"new material for PRESET Font : {preset_mat_path}" );

			I18NTextFontSetting_TMP.MaterialPresetData data = new I18NTextFontSetting_TMP.MaterialPresetData();
			data.m_PresetName = preset_name;
			data.m_BaseMaterial = new_mat;

			mSetting.m_PresetMaterialList.Add( data );
			mPresetNameBtnList.Add( CreateBtn( preset_name ) );
			mPresetNameBtnList = mPresetNameBtnList.OrderBy( a => a.gui_content.text ).ToList();
		}

		//------------------------------------------------------------------------
		void FindPresetUsedObjects( string preset_name )
		{
			List<UMFSelectionWindow.SelectionData> used_list = new List<UMFSelectionWindow.SelectionData>();
			_FindPresetUsedObjects( "Assets/Resources/Prefabs", preset_name, used_list );

			if( used_list.Count > 0 )
				UMFSelectionWindow.Show( used_list, $"I18NText Preset({preset_name}) USED" );
			else
				Debug.Log( $"Not found used preset : {preset_name}" );
		}
		void _FindPresetUsedObjects( string dir, string preset_name, List<UMFSelectionWindow.SelectionData> used_list )
		{
			string[] paths = Directory.GetFiles( dir, "*.prefab" );
			foreach( string full_path in paths )
			{
				string prefab_path = full_path;
				GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>( prefab_path );
				if( go != null )
				{
					UMFSelectionWindow.SelectionData s_data = null;

					I18NText_TMP[] tmp_list = go.GetComponentsInChildren<I18NText_TMP>( true );
					if( tmp_list != null )
					{
						foreach( I18NText_TMP tmp in tmp_list )
						{
							if( tmp.m_PresetName == preset_name )
							{
								if( s_data == null )
								{
									s_data = new UMFSelectionWindow.SelectionData();
									s_data.unity_object = go;
									used_list.Add( s_data );
								}

								s_data.description += $"[{preset_name}] {tmp.name}\n";
							}
						}
					}

					if( s_data != null )
						s_data.description = s_data.description.Trim();						 
				}
			}

			string[] sub_dirs = Directory.GetDirectories( dir );
			if( sub_dirs != null )
			{
				foreach( string s_dir in sub_dirs )
					_FindPresetUsedObjects( s_dir, preset_name, used_list );
			}
		}
	}
}
