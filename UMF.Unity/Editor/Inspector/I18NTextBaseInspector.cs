//////////////////////////////////////////////////////////////////////////
//
// I18NTextBaseInspector
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
using UMF.Core.I18N;
using UMF.Unity.EditorUtil;

namespace UMF.Unity.EditorUtil
{
	[CustomEditor( typeof( I18NTextBase ), true )]
	public class I18NTextBaseInspector : Editor
	{
		bool _info_dirty = false;

		Dictionary<string, string> mLanguageTextDic = null;

		protected virtual void DrawChildInspector() { }
		protected virtual void OnLanguageChanged() { }

		public override void OnInspectorGUI()
		{
			I18NTextBase i18n_text = target as I18NTextBase;

			base.OnInspectorGUI();

			Color gui_color = GUI.color;

			DrawChildInspector();

			InspectorUtil.DrawHeader( "I18NTextBase" );
			InspectorUtil.BeginContents();
			GUILayout.BeginHorizontal();
			if( GUILayout.Button( "Refresh" ) )
			{
				i18n_text._EditorUpdate();
				_info_dirty = true;
			}
			if( GUILayout.Button( "Reload Table" ) )
			{
				i18n_text._EditorUpdate( true );

#if UNITY_6000_0_OR_NEWER
				I18NTextBase[] ltexts = GameObject.FindObjectsByType<I18NTextBase>( FindObjectsSortMode.None );
#else
				I18NTextBase[] ltexts = GameObject.FindObjectsOfType<I18NTextBase>();
#endif
				foreach( I18NTextBase lt in ltexts )
					i18n_text._EditorUpdate();

				_info_dirty = true;
			}
			if( GUILayout.Button( "Clear Key" ) )
			{
				i18n_text.SetText( "" );

				_info_dirty = true;
			}
			GUILayout.EndHorizontal();

			i18n_text.m_EditorInfoExpand = InspectorUtil.DrawHeaderFoldable( "I18NText information", i18n_text.m_EditorInfoExpand );
			if( i18n_text.m_EditorInfoExpand )
			{
				InspectorUtil.BeginContents();

				if( _info_dirty || mLanguageTextDic == null )
				{
					if( mLanguageTextDic == null )
						mLanguageTextDic = new Dictionary<string, string>();
					else
						mLanguageTextDic.Clear();

					_info_dirty = false;
					
					Dictionary<string, List<string>> txt_dic = I18NTextSingleLanguage.Instance.GetTextAllLanguages( i18n_text.GetKey() );
					if( txt_dic != null )
					{
						foreach( var kvp in txt_dic )
						{
							string language = kvp.Key;
							string txts = "";
							for(int i=0; i<kvp.Value.Count; i++ )
							{
								if( i == 0 )
									txts = kvp.Value[i].Replace( "\n", "\\n" );
								else
									txts += "," + kvp.Value[i].Replace( "\n", "\\n" );
							}

							mLanguageTextDic[language] = txts;
						}
					}
				}				

				if( mLanguageTextDic != null )
				{
					bool is_app_playing = Application.isPlaying;
					foreach(var kvp in mLanguageTextDic)
					{
						GUILayout.BeginHorizontal();
						{
							if( kvp.Key == i18n_text.EditorForceLanguage )
								GUI.color = Color.green;
							if( GUILayout.Button( new GUIContent( kvp.Key, "Set Current Language" ), GUILayout.Width( 100f ) ) )
							{
								i18n_text._EditorUpdateLanguage( kvp.Key );
								OnLanguageChanged();
							}
							GUILayout.Label( $"{kvp.Value}" );
							GUI.color = gui_color;
						}
						GUILayout.EndHorizontal();
					}
				}

				InspectorUtil.EndContents();
			}

			InspectorUtil.EndContents();
		}
	}
}
