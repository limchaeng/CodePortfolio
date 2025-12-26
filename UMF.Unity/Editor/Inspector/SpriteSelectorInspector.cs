//////////////////////////////////////////////////////////////////////////
//
// SpriteSelectorInspector
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
using UnityEditorInternal;
using System.Linq;

namespace UMF.Unity.EditorUtil
{
	[CustomEditor( typeof( SpriteSelectorBase ), true )]
	public class SpriteSelectorBaseInspector : Editor
	{
		readonly GUIContent _clear_btn = new GUIContent( "Clear", "Clear all sprite" );
		readonly GUIContent _refresh = new GUIContent( "Refresh", "Refresh" );
		readonly GUIContent _auto_collect_btn = new GUIContent( "From Sheet", "Collect from SpriteSheet" );
		

		SpriteSelectorBase mInstance = null;

        bool mShowListBtns = true;
		List<InspectorUtil.DynamicBtnListBoxData> mShowIndexBtnList = new List<InspectorUtil.DynamicBtnListBoxData>();
        List<InspectorUtil.DynamicBtnListBoxData> mTempList = new List<InspectorUtil.DynamicBtnListBoxData>();

        private void OnEnable()
		{
			mInstance = target as SpriteSelectorBase;
			mShowListBtns = true;
			RefreshIndexBtns();
		}

		void RefreshIndexBtns()
		{
			if( mInstance == null || mInstance.m_SpriteDataList == null )
				return;				

			mTempList.Clear();
			mTempList.AddRange( mShowIndexBtnList );
			mShowIndexBtnList.Clear();
			int idx = 0;
			foreach( SpriteSelectorBase.SpriteData sp_data in mInstance.m_SpriteDataList )
			{
				InspectorUtil.DynamicBtnListBoxData btn_data = mTempList.Find( a => a.custom_data == sp_data );
				if( btn_data == null )
				{
					btn_data = new InspectorUtil.DynamicBtnListBoxData();

					btn_data.gui_content = new GUIContent( idx.ToString(), sp_data.m_Sprite != null ? sp_data.m_Sprite.name : "null" );
					if( sp_data.m_Sprite != null )
					{
						btn_data.gui_content.image = sp_data.m_Sprite.texture;
						btn_data.size_width = 50f;
						btn_data.size_height = 50f;
					}
					btn_data.is_selected = false;
					btn_data.custom_data = sp_data.m_Sprite;
				}

				mShowIndexBtnList.Add( btn_data );
				idx++;
			}
			mTempList.Clear();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			bool is_dirty = false;
			EditorGUI.BeginChangeCheck();

			InspectorUtil.DrawHeader( "Sprite Selector Tools" );
			InspectorUtil.BeginContents();
			{
				GUILayout.BeginHorizontal();
				{
					if( GUILayout.Button( _clear_btn ) )
					{
						if( mInstance != null )
						{
							mInstance.m_SpriteDataList.Clear();
							is_dirty = true;
						}
					}

					if( GUILayout.Button( _refresh ) )
					{
						is_dirty = true;
					}

					if( GUILayout.Button( _auto_collect_btn ) )
					{
						UpdateSpriteList();
                        is_dirty = true;
                    }

					//if( GUILayout.Button( mShowListBtns ? "IDX Hide" : "IDX Show" ) )
					//{
					//	mShowListBtns = !mShowListBtns;
					//}
				}
				GUILayout.EndHorizontal();
			}

			if( mShowListBtns )
			{
				InspectorUtil.DynamicBtnListBoxData clicked = InspectorUtil.DrawDynamicButtonListBox( mShowIndexBtnList, InspectorUtil.eDynamicBtnListBoxFlag.None, Color.green );
				if( clicked != null )
				{
					Sprite sp = clicked.custom_data as Sprite;
					if( sp != null )
					{
						EditorGUIUtility.PingObject( sp );
						mInstance.UpdateSprite( sp );
					}
				}
			}

			string[] drop_sprite_paths = InspectorUtil.DrawDragAndDropArea( "Sprite drop here!", 0f, 40f, Color.green, DragAndDropVisualMode.Copy );
			if( drop_sprite_paths != null )
			{
				foreach( string path in drop_sprite_paths )
				{
					Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>( path );
					if( sp != null )
					{
						mInstance.m_SpriteDataList.Add( new SpriteSelectorBase.SpriteData() { m_Sprite = sp } );
                        is_dirty = true;
                    }
				}
			}

			InspectorUtil.EndContents();

			base.OnInspectorGUI();

			if( EditorGUI.EndChangeCheck() || is_dirty )
				RefreshIndexBtns();

			serializedObject.ApplyModifiedProperties();

		}

		//------------------------------------------------------------------------
		void UpdateSpriteList()
		{
			Sprite curr_sprite = mInstance.GetSprite();
			if( curr_sprite == null || curr_sprite.texture == null )
				return;

			List<Sprite> sprite_list = AssetDatabase.LoadAllAssetsAtPath( AssetDatabase.GetAssetPath( curr_sprite.texture ) ).OfType<Sprite>().ToList();
			foreach( Sprite sp in sprite_list )
			{
				SpriteSelectorBase.SpriteData exist_data = mInstance.m_SpriteDataList.Find( a => a.m_Sprite == sp );
				if( exist_data == null )
				{
					exist_data = new SpriteSelectorBase.SpriteData();
					exist_data.m_Sprite = sp;

					mInstance.m_SpriteDataList.Add( exist_data );
				}
			}
		}
	}
}
