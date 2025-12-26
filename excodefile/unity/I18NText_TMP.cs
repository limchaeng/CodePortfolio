//////////////////////////////////////////////////////////////////////////
//
// I18NText_TMP
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// I18N text 처리 TextMeshPro 지원
//////////////////////////////////////////////////////////////////////////
#define UM_TMP_PRESENT
#if UM_TMP_PRESENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

namespace UMF.Unity
{
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[AddComponentMenu( "UMF/Component/I18N Text - TMP" )]
	[RequireComponent( typeof( TMP_Text ) )]
	public class I18NText_TMP : I18NTextBase
	{
		[SerializeField]
		[HideInInspector]
		bool mForcedNOBR = false;

		[SerializeField]
		[HideInInspector]
		bool mWordWrappingSizeCheck = false;

		[HideInInspector]
		public string m_Category = "";
		[HideInInspector]
		public string m_PresetName = "";
		[HideInInspector]
		public Material m_CustomMaterial;

		// for anim
		[HideInInspector]
		public float m_Alpha = 1f;

		bool mPrevForcedSetNOBR = false;
		string mTMPNOBRSetText = "";

		// for UGUI
		RectTransform mRectTransform = null;
		Vector2 mRectSizeDefault = Vector2.zero;

		TMP_Text mTMP;
		public TMP_Text TMP
		{
			get
			{
				if( mInit == false )
					Init();

				if( mTMP == null )
					mTMP = gameObject.GetComponent<TMP_Text>();

				return mTMP;
			}
		}

		protected override void Init()
		{
			base.Init();

			if( mTMP == null )
				mTMP = gameObject.GetComponent<TMP_Text>();

			mRectTransform = gameObject.GetComponent<RectTransform>();
			if( mRectTransform != null )
				mRectSizeDefault = mRectTransform.sizeDelta;

			if( Application.isPlaying )
				UpdateFont();
		}

		public override Color color
		{
			get { return TMP.color; }
			set { TMP.color = value; }
		}

		public override float alpha
		{
			get 
			{
				m_Alpha = TMP.alpha;
				return TMP.alpha; 
			}
			set 
			{
				m_Alpha = value;
				TMP.alpha = value; 
			}
		}

		public override string text
		{
			get
			{
				if( mPrevForcedSetNOBR && string.IsNullOrEmpty( mTMPNOBRSetText ) == false )
					return mTMPNOBRSetText;

				return TMP.text;
			}
			set
			{
				if( mForcedNOBR )
				{
					mPrevForcedSetNOBR = true;
					mTMPNOBRSetText = value;
					TMP.text = $"<NOBR>{value}</NOBR>";
				}
				else
				{
					mPrevForcedSetNOBR = false;
					mTMPNOBRSetText = "";
					TMP.text = value;

					CheckWordWrappingSize();
				}
			}
		}

		public bool ForcedNOBR
		{
			get { return mForcedNOBR; }
			set
			{
				mForcedNOBR = value;
				text = text;
			}
		}

		public bool WordWrappingSizeCheck
		{
			get { return mWordWrappingSizeCheck; }
			set { mWordWrappingSizeCheck = value; }
		}

		//------------------------------------------------------------------------
		public override void UpdateFont()
		{
			if( mTMP != null )
			{
				I18NTextFontSetting_TMP font_setting = I18NTextFontSetting_TMP.Instance;
				if( font_setting != null )
				{
					I18NTextFontSetting_TMP.LanguageFontData font_data = font_setting.CurrentFontData;
					if( font_data == null )
						return;

					mEditorForceLanguage = I18NTextManagerUnity.CurrentLanguage();
					_UpdateFont( font_data );
				}
			}
		}

		//------------------------------------------------------------------------
		void _UpdateFont( I18NTextFontSetting_TMP.LanguageFontData def_font_data )
		{
			bool is_updated = false;
			TMP_FontAsset font_asset = I18NTextFontSetting_TMP.Instance.FindFontCategory( m_Category, def_font_data.m_Language );
			if( font_asset == null )
				font_asset = def_font_data.m_FontAsset;

			if( mTMP.font != font_asset )
			{
				is_updated = true;
				mTMP.font = font_asset;
			}

			if( m_CustomMaterial != null )
			{
				ShaderUtilities.GetShaderPropertyIDs();
				m_CustomMaterial.SetTexture( ShaderUtilities.ID_MainTex, font_asset.material.GetTexture( ShaderUtilities.ID_MainTex ) );
				m_CustomMaterial.SetFloat( ShaderUtilities.ID_GradientScale, font_asset.material.GetFloat( ShaderUtilities.ID_GradientScale ) );
				m_CustomMaterial.SetFloat( ShaderUtilities.ID_TextureWidth, font_asset.material.GetFloat( ShaderUtilities.ID_TextureWidth ) );
				m_CustomMaterial.SetFloat( ShaderUtilities.ID_TextureHeight, font_asset.material.GetFloat( ShaderUtilities.ID_TextureHeight ) );
				m_CustomMaterial.SetFloat( ShaderUtilities.ID_WeightNormal, font_asset.material.GetFloat( ShaderUtilities.ID_WeightNormal ) );
				m_CustomMaterial.SetFloat( ShaderUtilities.ID_WeightBold, font_asset.material.GetFloat( ShaderUtilities.ID_WeightBold ) );

				if( mTMP.fontSharedMaterial == null || mTMP.fontMaterial != m_CustomMaterial )
				{
					is_updated = true;
					mTMP.fontMaterial = m_CustomMaterial;
				}
			}
			else
			{
				// check preset material
				bool has_preset = false;
				if( string.IsNullOrEmpty( m_PresetName ) == false )
				{
					Material preset_material = I18NTextFontSetting_TMP.Instance.GetPresetMaterial( m_PresetName, font_asset );
					if( preset_material != null )
					{
						has_preset = true;
						if( mTMP.fontSharedMaterial == null || mTMP.fontMaterial != preset_material )
						{
							is_updated = true;
							mTMP.fontMaterial = preset_material;
						}
					}
				}

				if( has_preset == false )
				{
					if( mTMP.fontSharedMaterial == null || mTMP.fontMaterial != mTMP.font.material )
					{
						is_updated = true;
						mTMP.fontMaterial = mTMP.font.material;
					}
				}
			}

			if( is_updated && mTMP.enabled )
			{
				mTMP.enabled = false;
				mTMP.enabled = true;
			}
		}

		//------------------------------------------------------------------------
		[ContextMenu( "TMP Check Layout Size" )]
		void CheckWordWrappingSize()
		{
#if UNITY_EDITOR
			if( mRectTransform == null )
				mRectTransform = gameObject.GetComponent<RectTransform>();
#endif
			if( mRectTransform == null || mTMP == null )
				return;

			if( mWordWrappingSizeCheck == false || mTMP.enableWordWrapping == false )
				return;

			if( string.IsNullOrEmpty( mTMP.text ) )
				return;

			if( Application.isEditor || I18NTextManagerUnity.Instance.IsWordWrappingSizeCheckCurrentLanguage )
			{
				mTMP.ForceMeshUpdate();
				mTMP.GetPreferredValues();
				float p_height = mTMP.preferredHeight;
				if( mTMP.textInfo != null && mTMP.textInfo.lineInfo.Length > 0 )
				{
					Vector2 v2 = mRectTransform.sizeDelta;
					v2.x = mRectSizeDefault.x;

					//Debug.Log( $"-- {mTMP.text} - {p_height} - {v2}" );

					float line_height = mTMP.textInfo.lineInfo[0].lineHeight;
					float total_line_height = ( mTMP.textInfo.lineCount * line_height ) + mTMP.margin.y + mTMP.margin.w;
					if( Mathf.Abs( p_height - total_line_height ) > line_height * 0.5f )
					{
						v2.x -= ( mTMP.textInfo.characterInfo[0].topRight.x - mTMP.textInfo.characterInfo[0].topLeft.x ) * 0.5f;
					}

					mRectTransform.sizeDelta = v2;
				}
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			OnDidApplyAnimationProperties();
		}
#endif

		void OnDidApplyAnimationProperties()
		{
			alpha = m_Alpha;
		}

#if UNITY_EDITOR
		public override void _EditorUpdateFont()
		{
			if( mTMP != null )
			{
				I18NTextFontSetting_TMP font_setting = I18NTextFontSetting_TMP.Instance;
				if( font_setting != null )
				{
					if( string.IsNullOrEmpty( mEditorForceLanguage ) )
						mEditorForceLanguage = I18NTextManagerUnity.CurrentLanguage();

					I18NTextFontSetting_TMP.LanguageFontData font_data = font_setting.m_DefaultFontList.Find( a => a.m_Language == mEditorForceLanguage );
					if( font_data == null )
						return;

					_UpdateFont( font_data );
				}
			}
		}

		[ContextMenu("TMP info")]
		void _EditorTMP_Info()
		{
			if( mTMP == null )
				return;

			Debug.Log( $"ph={mTMP.preferredHeight} rh={mTMP.renderedHeight}" );
			Debug.Log( $"bound={mTMP.bounds}.{mTMP.bounds.size.y} tbound={mTMP.textBounds}" );
			Debug.Log( $"scale={mTMP.transform.localScale} / {mTMP.transform.lossyScale}" );
			Debug.Log( $"line={mTMP.textInfo.lineCount} height={mTMP.textInfo.lineInfo[0].lineHeight}" );
		}
#endif
	}
}

#endif
