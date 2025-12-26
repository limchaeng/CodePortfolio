//////////////////////////////////////////////////////////////////////////
//
// I18NTextFontSetting_TMP
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
#define UM_TMP_PRESENT
#if UM_TMP_PRESENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UMF.Core;

namespace UMF.Unity
{
	[CreateAssetMenu( fileName = "I18NTextFontSetting_TMP", menuName = "UMF/I18NTextFontSetting_TMP" )]
	public class I18NTextFontSetting_TMP : ScriptableObject, II18NTextFontSetting
	{
		//------------------------------------------------------------------------	
		private static I18NTextFontSetting_TMP instance;
		public static I18NTextFontSetting_TMP Instance
		{
			get
			{
				if( instance == null )
				{
					instance = Resources.Load( "I18NTextFontSetting_TMP" ) as I18NTextFontSetting_TMP;
					if( instance == null )
					{
						instance = CreateInstance<I18NTextFontSetting_TMP>();
#if UNITY_EDITOR
						UnityEditor.AssetDatabase.CreateAsset( instance, "Assets/Resources/I18NTextFontSetting_TMP.asset" );
#endif
					}

					instance.Init();
				}

				return instance;
			}
		}

		[System.Serializable]
		public class LanguageFontData
		{
			public string m_Language = "";
			public TMP_FontAsset m_FontAsset;
		}

		public class PresetMaterialRuntimeData
		{
			public TMP_FontAsset m_FontAsset;
			public Material m_Material;
		}

		[System.Serializable]
		public class MaterialPresetData
		{
			public string m_PresetName = "";
			public Material m_BaseMaterial;

			[HideInInspector]
			public bool _EditorExpand = false;

			[System.NonSerialized]
			public List<PresetMaterialRuntimeData> m_RuntimeMaterialList = new List<PresetMaterialRuntimeData>();
		}

		[System.Serializable]
		public class FontCategoryData
		{
			public string m_Category = "";
			public List<LanguageFontData> m_FontDataList = new List<LanguageFontData>();

			[HideInInspector]
			public bool _EditorExpand = false;
		}

		public List<LanguageFontData> m_DefaultFontList = new List<LanguageFontData>();
		public List<FontCategoryData> m_CategoryFontList = new List<FontCategoryData>();
		public List<MaterialPresetData> m_PresetMaterialList = new List<MaterialPresetData>();

		LanguageFontData mCurrentFontData = null;
		public LanguageFontData CurrentFontData { get { return mCurrentFontData; } }

		//------------------------------------------------------------------------
		void _CollectAllFontAsset( TMP_FontAsset font_asset, ref List<TMP_FontAsset> list )
		{
			if( font_asset == null )
				return;

			list.AddIfNotContain( font_asset );

			List<TMP_FontAsset> fallback_fonts = font_asset.fallbackFontAssetTable;
			if( fallback_fonts != null )
			{
				foreach( TMP_FontAsset font in fallback_fonts )
					list.AddIfNotContain( font );
			}

			TMP_FontWeightPair[] weight_fonts = font_asset.fontWeightTable;
			if( weight_fonts != null )
			{
				foreach( TMP_FontWeightPair pair in weight_fonts )
				{
					if( pair.italicTypeface != null )
						list.AddIfNotContain( pair.italicTypeface );

					if( pair.regularTypeface != null )
						list.AddIfNotContain( pair.regularTypeface );
				}
			}
		}

		//------------------------------------------------------------------------
		public void Init()
		{
			ClearFontAssetData();
		}

		//------------------------------------------------------------------------
		public void ClearFontAssetData()
		{
			List<TMP_FontAsset> font_asset_validations = new List<TMP_FontAsset>();
			_CollectAllFontAsset( TMP_Settings.defaultFontAsset, ref font_asset_validations );

			foreach( LanguageFontData font in m_DefaultFontList )
				_CollectAllFontAsset( font.m_FontAsset, ref font_asset_validations );

			foreach( FontCategoryData cat_data in m_CategoryFontList )
			{
				foreach( LanguageFontData font in cat_data.m_FontDataList )
					_CollectAllFontAsset( font.m_FontAsset, ref font_asset_validations );
			}

			// check
			foreach( TMP_FontAsset font_asset in font_asset_validations )
			{
				font_asset.ClearFontAssetData( true );
			}
		}

		//------------------------------------------------------------------------			
		public TMP_FontAsset FindFontCategory( string category, string langauge )
		{
			FontCategoryData cat_data = m_CategoryFontList.Find( a => a.m_Category == category );
			if( cat_data != null )
			{
				LanguageFontData font = cat_data.m_FontDataList.Find( a => a.m_Language == langauge );
				if( font != null )
					return font.m_FontAsset;
			}

			return null;
		}

		//------------------------------------------------------------------------			
		public Material GetPresetMaterial( string _name, TMP_FontAsset font_asset )
		{
			if( font_asset == null )
				return null;

			MaterialPresetData mat_data = m_PresetMaterialList.Find( a => a.m_PresetName == _name );
			if( mat_data != null && mat_data.m_BaseMaterial != null && font_asset.material != null )
			{
				Material target_mat = null;

				if( Application.isEditor && Application.isPlaying == false )
				{
					target_mat = mat_data.m_BaseMaterial;
				}
				else
				{
					PresetMaterialRuntimeData runtime_mat = mat_data.m_RuntimeMaterialList.Find( a => a.m_FontAsset == font_asset );
					if( runtime_mat != null )
						return runtime_mat.m_Material;

					target_mat = new Material( mat_data.m_BaseMaterial );
					target_mat.shaderKeywords = mat_data.m_BaseMaterial.shaderKeywords;

					runtime_mat = new PresetMaterialRuntimeData();
					runtime_mat.m_Material = target_mat;
					runtime_mat.m_FontAsset = font_asset;

					mat_data.m_RuntimeMaterialList.Add( runtime_mat );
				}

				Material src_mat = font_asset.material;

				// copy current font asset's material
				ShaderUtilities.GetShaderPropertyIDs();
				target_mat.SetTexture( ShaderUtilities.ID_MainTex, src_mat.GetTexture( ShaderUtilities.ID_MainTex ) );
				target_mat.SetFloat( ShaderUtilities.ID_GradientScale, src_mat.GetFloat( ShaderUtilities.ID_GradientScale ) );
				target_mat.SetFloat( ShaderUtilities.ID_TextureWidth, src_mat.GetFloat( ShaderUtilities.ID_TextureWidth ) );
				target_mat.SetFloat( ShaderUtilities.ID_TextureHeight, src_mat.GetFloat( ShaderUtilities.ID_TextureHeight ) );
				target_mat.SetFloat( ShaderUtilities.ID_WeightNormal, src_mat.GetFloat( ShaderUtilities.ID_WeightNormal ) );
				target_mat.SetFloat( ShaderUtilities.ID_WeightBold, src_mat.GetFloat( ShaderUtilities.ID_WeightBold ) );

				return target_mat;
			}

			return null;
		}

		//------------------------------------------------------------------------		
		// interface
		public bool II18NTextFontSetting_ChangeLanguage( string language )
		{
			LanguageFontData font_data = m_DefaultFontList.Find( a => a.m_Language == language );
			if( font_data == null )
			{
				Debug.LogWarning( $"I18NTextFontSetting not found language({language}) font data!" );
				return false;
			}

			mCurrentFontData = font_data;
			return true;
		}
	}
}

#endif