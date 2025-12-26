//////////////////////////////////////////////////////////////////////////
//
// SrcPreviewEditor
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

namespace UMF.Unity.EditorUtil
{
	public class SrcPreviewEditor : EditorWindow
	{
		static SrcPreviewEditor instance;

		SrcPreview mPreview = null;

		//------------------------------------------------------------------------		
		public static void Show( SrcPreview comp )
		{
			if( instance != null )
			{
				instance.ShowUtility();
				instance.Focus();
				instance.Init( comp );
				return;
			}

			instance = (SrcPreviewEditor)EditorWindow.GetWindow( typeof( SrcPreviewEditor ), false, "Source Preview" );
			instance.ShowUtility();
			instance.Focus();
			instance.Init( comp );
		}

		//------------------------------------------------------------------------	
		public void Init( SrcPreview comp )
		{
			mPreview = comp;
		}

		//------------------------------------------------------------------------
		public static void DrawSrcPreviewControl( SrcPreview comp, bool is_inspector )
		{
			EditorGUI.BeginChangeCheck();

			comp.m_PreviewColor = EditorGUILayout.ColorField( "Color", comp.m_PreviewColor );
			if( is_inspector )
				comp.m_PreviewScale = EditorGUILayout.Slider( "Scale", comp.m_PreviewScale, 0f, 1f );
			else
				comp.m_PreviewEditorWindowScale = EditorGUILayout.Slider( "Scale", comp.m_PreviewEditorWindowScale, 0f, 1f );
			comp.m_PreviewAlpha = EditorGUILayout.Slider( "Alpha", comp.m_PreviewAlpha, 0f, 1f );

			comp.m_PreviewColor.a = comp.m_PreviewAlpha;
			comp.SetColor( comp.m_PreviewColor );

			Sprite sprite = comp.GetSprite();
			if( sprite != null )
			{
				float inspector_width = EditorGUIUtility.currentViewWidth;
				float width = inspector_width - 30f;
				float height = 100f;
				//Debug.Log( $"{size} - {sprite.rect}" );

				float ratio = inspector_width / sprite.rect.width;
				height = sprite.rect.height * ratio;

				if( is_inspector )
				{
					width *= comp.m_PreviewScale;
					height *= comp.m_PreviewScale;
				}
				else
				{
					width *= comp.m_PreviewEditorWindowScale;
					height *= comp.m_PreviewEditorWindowScale;
				}

				Sprite new_sprite = EditorGUILayout.ObjectField( sprite, typeof( Sprite ), true, GUILayout.Width( width ), GUILayout.Height( height ) ) as Sprite;
				if( sprite != new_sprite )
					comp.SetSprite( new_sprite );
			}

			if( EditorGUI.EndChangeCheck() )
				EditorUtility.SetDirty( comp );
		}

		//------------------------------------------------------------------------	
		void OnGUI()
		{
			if( mPreview == null )
				return;

			GUILayout.Label( "UI Source Preview" );

			DrawSrcPreviewControl( mPreview, false );

			if( GUILayout.Button( "CLOSE" ) )
			{
				Close();
			}
		}
	}
}