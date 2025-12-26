//////////////////////////////////////////////////////////////////////////
//
// I18NText_TMPInspector
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
using UnityEditor;
using UnityEngine;
using System.Linq;
using TMPro;
using System.IO;

namespace UMF.Unity.EditorUtil
{
	[CustomEditor( typeof( I18NText_TMP ), true )]
	public class I18NText_TMPInspector : I18NTextBaseInspector
	{
		const string MATERIAL_CUSTOM_PATH = "_custom_mat";

		I18NText_TMP mI18NText = null;

		int mCategorySelectIdx = 0;
		string[] mCategoryNameArray = new string[0];
		int mMatPresetSelectIdx = 0;
		string[] mMatPresetNameArray = new string[0];

		List<InspectorUtil.DynamicBtnListBoxData> mLanguageBtnList = new List<InspectorUtil.DynamicBtnListBoxData>();

		private void OnEnable()
		{
			mI18NText = target as I18NText_TMP;
			Refresh();
		}

		//------------------------------------------------------------------------
		void Refresh()
		{
			I18NTextFontSetting_TMP font_setting = I18NTextFontSetting_TMP.Instance;

			mCategoryNameArray = font_setting.m_CategoryFontList.Select( a => a.m_Category ).ToArray();
			if( mCategoryNameArray == null )
				mCategoryNameArray = new string[0];
			mCategorySelectIdx = System.Array.IndexOf( mCategoryNameArray, mI18NText.m_Category );

			mMatPresetNameArray = font_setting.m_PresetMaterialList.Select( a => a.m_PresetName ).ToArray();
			if( mMatPresetNameArray == null )
				mMatPresetNameArray = new string[0];
			mMatPresetSelectIdx = System.Array.IndexOf( mMatPresetNameArray, mI18NText.m_PresetName );

			mLanguageBtnList.Clear();
			foreach( I18NTextFontSetting_TMP.LanguageFontData font_data in font_setting.m_DefaultFontList )
			{
				InspectorUtil.DynamicBtnListBoxData btn = new InspectorUtil.DynamicBtnListBoxData();
				btn.gui_content = new GUIContent( font_data.m_Language, font_data.m_Language );
				mLanguageBtnList.Add( btn );
			}
		}

		//------------------------------------------------------------------------
		protected override void OnLanguageChanged()
		{
			base.OnLanguageChanged();
			mLanguageBtnList.ForEach( a => a.is_selected = ( a.gui_content.text == mI18NText.EditorForceLanguage ) );
		}

		//------------------------------------------------------------------------
		protected override void DrawChildInspector()
		{
			if( mI18NText == null )
				return;

			bool is_forced_NOBR = GUILayout.Toggle( mI18NText.ForcedNOBR, new GUIContent( "Forced NOBR Update", "Forced <NOBR> tag set first!" ) );
			if( is_forced_NOBR != mI18NText.ForcedNOBR )
			{
				mI18NText.ForcedNOBR = is_forced_NOBR;
			}

			bool is_wordwrapping_size_check = GUILayout.Toggle( mI18NText.WordWrappingSizeCheck, new GUIContent( "WordWrapping Size check", "CJK word wrapping and preferred size issue!" ) );
			if( is_wordwrapping_size_check != mI18NText.WordWrappingSizeCheck )
			{
				mI18NText.WordWrappingSizeCheck = is_wordwrapping_size_check;
			}

			// font
			InspectorUtil.DrawHeader( "I18NText FONT" );
			InspectorUtil.BeginContents( "Font Language" );
			GUILayout.Label( $"Current Editor Language : {mI18NText.EditorForceLanguage}" );
			InspectorUtil.DynamicBtnListBoxData seleted_langauge = InspectorUtil.DrawDynamicButtonListBox( mLanguageBtnList, InspectorUtil.eDynamicBtnListBoxFlag.AlignLeft );
			if( seleted_langauge != null )
			{
				mI18NText._EditorUpdateLanguage( seleted_langauge.gui_content.text );
				OnLanguageChanged();
			}
			InspectorUtil.EndContents();

			InspectorUtil.BeginContents( "Font Category Select" );
			GUILayout.BeginHorizontal();
			int cat_idx = EditorGUILayout.Popup( "Category", mCategorySelectIdx, mCategoryNameArray );
			if( mCategorySelectIdx != cat_idx )
			{
				mCategorySelectIdx = cat_idx;
				mI18NText.m_Category = mCategoryNameArray[mCategorySelectIdx];
				mI18NText._EditorUpdateFont();
				EditorUtility.SetDirty( mI18NText );
			}
			if( GUILayout.Button( "DEL", GUILayout.Width( 50f ) ) )
			{
				mCategorySelectIdx = -1;
				mI18NText.m_Category = "";
				mI18NText._EditorUpdateFont();
				EditorUtility.SetDirty( mI18NText );
			}
			GUILayout.EndHorizontal();
			InspectorUtil.EndContents();

			// material preset
			InspectorUtil.BeginContents( "Material Preset Select" );
			GUILayout.BeginHorizontal();
			int preset_idx = EditorGUILayout.Popup( "Preset", mMatPresetSelectIdx, mMatPresetNameArray );
			if( mMatPresetSelectIdx != preset_idx )
			{
				mMatPresetSelectIdx = preset_idx;
				mI18NText.m_PresetName = mMatPresetNameArray[mMatPresetSelectIdx];
				mI18NText._EditorUpdateFont();
				EditorUtility.SetDirty( mI18NText );
			}
			if( GUILayout.Button( "DEL", GUILayout.Width( 50f ) ) )
			{
				mMatPresetSelectIdx = -1;
				mI18NText.m_PresetName = "";
				mI18NText._EditorUpdateFont();
				EditorUtility.SetDirty( mI18NText );
			}
			GUILayout.EndHorizontal();
			InspectorUtil.EndContents();

			InspectorUtil.BeginContents( "Custom Material" );
			GUILayout.BeginHorizontal();

			EditorGUI.BeginDisabledGroup( mI18NText.m_CustomMaterial != null );
			if( GUILayout.Button( "NEW" ) )
			{
				string mat_name = mI18NText.gameObject.name;
				List<Transform> parent_list = mI18NText.transform.GetParentList( typeof( PrefabRootBehaviour ) );
				if( parent_list != null && parent_list.Count > 0 )
				{
					mat_name = $"{parent_list[0].name}_{mI18NText.gameObject.name}";

					if( parent_list.Count > 1 )
					{
						mat_name = $"{parent_list.Last().name}__{mat_name}";
					}
				}

				mat_name = UMFPath.ConvertValidFilePath( mat_name, "_" );

				TMP_FontAsset font_asset = mI18NText.TMP.font;

				if( Application.isPlaying )
				{
					if( mI18NText.TMP.fontSharedMaterial != null )
					{
						Material src_mat = mI18NText.TMP.fontMaterial;
						Material duplicate = new Material( src_mat );
						duplicate.shaderKeywords = src_mat.shaderKeywords;

						mI18NText.m_CustomMaterial = duplicate;
					}
					else
					{
						Debug.Log( $"fontSharedMaterial is null" );
					}
				}
				else
				{
					string font_asset_path = Path.GetDirectoryName( AssetDatabase.GetAssetPath( font_asset ) ).NormalizeSlashPath();
					if( font_asset_path.IndexOf( "Assets/", System.StringComparison.InvariantCultureIgnoreCase ) == -1 )
					{
						Debug.LogWarning( "Material cannot be created from a material that is located outside the project." );
					}
					else
					{
						if( mI18NText.TMP.fontSharedMaterial != null )
						{
							Material src_mat = mI18NText.TMP.fontMaterial;
							Material duplicate = new Material( src_mat );
							duplicate.shaderKeywords = src_mat.shaderKeywords;

							string new_path = font_asset_path + "/" + MATERIAL_CUSTOM_PATH;
							if( Directory.Exists( new_path ) == false )
								Directory.CreateDirectory( new_path );

							new_path = new_path + "/" + mat_name + ".mat";
							string dup_mat_path = new_path;

							int exist_check_count = 0;
							while( File.Exists( dup_mat_path ) )
							{
								exist_check_count++;
								dup_mat_path = new_path.Replace( ".mat", string.Format( "_{0:00#}.mat", exist_check_count ) );
							}

							mI18NText.m_CustomMaterial = duplicate;

							AssetDatabase.CreateAsset( duplicate, AssetDatabase.GenerateUniqueAssetPath( dup_mat_path ) );
							Debug.Log( $"new material for custom Font : {dup_mat_path}" );
						}
						else
						{
							Debug.Log( $"fontSharedMaterial is null" );
						}
					}

					EditorUtility.SetDirty( mI18NText );
					AssetDatabase.Refresh();
				}
			}
			EditorGUI.EndDisabledGroup();
			if( GUILayout.Button( "DEL" ) )
			{
				if( mI18NText.m_CustomMaterial != null )
				{
					if( Application.isPlaying == false )
					{
						if( AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( mI18NText.m_CustomMaterial ) ) )
							AssetDatabase.Refresh();
					}

					mI18NText.m_CustomMaterial = null;
				}

				mI18NText._EditorUpdateFont();
				EditorUtility.SetDirty( mI18NText );
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.ObjectField( "Material", mI18NText.m_CustomMaterial, typeof( Material ), false );
			InspectorUtil.EndContents();
		}

		//------------------------------------------------------------------------
		string tmpMatMsg = "공통으로 사용하는 Material 의 속성(아웃라인등.)을 변경하면) 모든 텍스트에 적용됩니다.현재 텍스트에만 효과를 사용하려면 상단의 Custom Material 을 사용하세요.";
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.HelpBox( tmpMatMsg, MessageType.Warning );
		}
	}
}