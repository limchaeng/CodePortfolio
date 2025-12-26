//////////////////////////////////////////////////////////////////////////
//
// UISrcPreview
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
using UnityEngine.UI;

namespace UMF.Unity.EditorUtil
{
	[ExecuteAlways]
	public class UISrcPreview : SrcPreview
	{
		public Canvas m_Canvas;
		public Image m_Image;

		private void OnEnable()
		{
			if( m_Canvas != null && m_Canvas.worldCamera == null )
			{
#if UNITY_6000_0_OR_NEWER
				Camera[] cameras = FindObjectsByType<Camera>( FindObjectsInactive.Include, FindObjectsSortMode.None );
#else
				Camera[] cameras = FindObjectsOfType<Camera>();
#endif
				if( cameras != null )
				{
					foreach( Camera cam in cameras )
					{
						if( cam.gameObject.layer == (int)LayerMask.NameToLayer( "UI" ) )
						{
							m_Canvas.worldCamera = cam;
							break;
						}
					}
				}
			}
		}

		public override void SetColor( Color color )
		{
			if( m_Image != null )
				m_Image.color = color;
		}
		public override Sprite GetSprite()
		{
			if( m_Image != null )
				return m_Image.sprite;

			return null;
		}
		public override void SetSprite( Sprite sprite )
		{
			if( m_Image != null )
				m_Image.sprite = sprite;
		}
	}
}