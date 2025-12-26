//////////////////////////////////////////////////////////////////////////
//
// I18NTextBase
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// I18N text 처리 기본 : 키를 기준으로 런타임시 언어별 텍스트 처리
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMF.Core.I18N;

namespace UMF.Unity
{
	public abstract class I18NTextBase : MonoBehaviour
	{
		public string m_Key;
		[ReadOnly]
		public string m_InternalKey = "";
		[HideInInspector]
		public bool m_EditorInfoExpand = false;

		protected bool mInit = false;
		protected bool mInternalKeySetted = false;
		protected object[] mParams = null;

		public class ReplaceData
		{
			public string m_Source;
			public string m_Replace;
			public ReplaceData( string src, string replace )
			{
				m_Source = src;
				m_Replace = replace;
			}
		}
		protected ReplaceData[] mReplaceData = null;

		protected string[] mFrontAddedText = null;
		protected string[] mBackAddedText = null;

		public System.Action TextUpdatedCallback { get; set; } = null;

		protected string mEditorForceLanguage = "";
		public string EditorForceLanguage { get { return mEditorForceLanguage; } }

		protected virtual void Awake()
		{
			Init();
		}
		protected virtual void Init()
		{
			if( mInit )
				return;

			mInit = true;

			if( mInternalKeySetted == false )
				m_InternalKey = m_Key;
		}

		void OnEnable()
		{
			if( mInit == false )
				Init();

			UpdateFont();
			UpdateText();
		}

		//------------------------------------------------------------------------
		public abstract Color color { get; set; }
		public abstract float alpha { get; set; }
		public abstract string text { get; set; }


		//------------------------------------------------------------------------		
		string _GetText( string key, params object[] parms )
		{
#if UNITY_EDITOR
			if( string.IsNullOrEmpty( mEditorForceLanguage ) == false )
			{
				string lan_text;
				if( I18NTextSingleLanguage.Instance.GetLanguageText( mEditorForceLanguage, key, out lan_text ) )
				{
					if( parms != null )
					{
						return string.Format( I18NTextConst.CheckFormatException( lan_text, parms ), parms );
					}
				}
				else
				{
					lan_text = key;
				}

				return lan_text;
			}
#endif
			return I18NTextManagerUnity.GetText( key, parms );
		}

		//------------------------------------------------------------------------
		protected void UpdateText()
		{
			if( string.IsNullOrEmpty( m_InternalKey ) )
				return;

			string _text = "";
			if( mParams == null )
			{
				_text = _GetText( m_InternalKey );
			}
			else
			{
				object[] _parms = new object[mParams.Length];
				for( int i = 0; i < _parms.Length; i++ )
				{
					if( I18NTextSingleLanguage.Instance.Contains( mParams[i].ToString() ) )
						_parms[i] = _GetText( mParams[i].ToString() );
					else
						_parms[i] = mParams[i];
				}
				_text = _GetText( m_InternalKey, _parms );
			}

			text = PostUpdate( _text );
		}

		//------------------------------------------------------------------------	
		protected string PostUpdate( string _text )
		{
			string text = _text;
			// replace data
			if( mReplaceData != null )
			{
				for( int i = 0; i < mReplaceData.Length; i++ )
				{
					ReplaceData replace = mReplaceData[i];
					if( string.IsNullOrEmpty( replace.m_Source ) == false && replace.m_Replace != null )
					{
						if( I18NTextSingleLanguage.Instance.Contains( replace.m_Replace ) )
							text = text.Replace( replace.m_Source, _GetText( replace.m_Replace ) );
						else
							text = text.Replace( replace.m_Source, replace.m_Replace );
					}
				}
			}

			// added text
			if( mFrontAddedText != null )
			{
				for( int i = 0; i < mFrontAddedText.Length; i++ )
				{
					string frontText = _GetText( mFrontAddedText[i] );
					text = string.Format( "{0}{1}", frontText, text );
				}
			}

			if( mBackAddedText != null )
			{
				for( int i = 0; i < mBackAddedText.Length; i++ )
				{
					string backText = _GetText( mBackAddedText[i] );
					text = string.Format( "{0}{1}", text, backText );
				}
			}

			return text;
		}

		//------------------------------------------------------------------------
		public void SetText( string key, params object[] parms )
		{
			mInternalKeySetted = true;
			m_InternalKey = key;
			mParams = parms;
			mReplaceData = null;

			text = "";
			UpdateText();
		}

		//------------------------------------------------------------------------
		public void SetTextWithReplace( string key, ReplaceData[] replace_datas, params object[] parms )
		{
			m_InternalKey = key;
			mParams = parms;
			mReplaceData = replace_datas;
			text = "";
			UpdateText();
		}

		//------------------------------------------------------------------------	
		public void SetAddedText( bool front, params string[] textOrKey )
		{
			if( front )
				mFrontAddedText = textOrKey;
			else
				mBackAddedText = textOrKey;

			UpdateText();
		}

		//------------------------------------------------------------------------
		public virtual void UpdateFont() { }

		//------------------------------------------------------------------------	
		public string GetKey()
		{
			if( Application.isPlaying )
				return m_InternalKey;
			else
				return m_Key;
		}

		//------------------------------------------------------------------------
		// from broadcast
		void OnI18NTextUpdateBroadcast()
		{
			UpdateFont();
			UpdateText();

			if( TextUpdatedCallback != null )
				TextUpdatedCallback();
		}

#if UNITY_EDITOR
		//------------------------------------------------------------------------	
		public void _EditorUpdate( bool reload = false )
		{
			if( reload )
				I18NTextManagerUnity.Instance.ReloadText();

			if( string.IsNullOrEmpty( m_InternalKey ) || m_InternalKey != m_Key )
				m_InternalKey = m_Key;

			if( string.IsNullOrEmpty( m_InternalKey ) == false )
				UpdateText();
			else
				text = text;
		}

		//------------------------------------------------------------------------
		public virtual void _EditorUpdateFont() { }

		public void _EditorUpdateLanguage( string language )
		{
			mEditorForceLanguage = language;
			_EditorUpdateFont();
			_EditorUpdate();
		}
#endif
	}
}
