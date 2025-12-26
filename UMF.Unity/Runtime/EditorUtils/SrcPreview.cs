//////////////////////////////////////////////////////////////////////////
//
// SrcPreview
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
	public class SrcPreview : MonoBehaviour
	{
		[HideInInspector]
		[Range( 0f, 1f )]
		public float m_PreviewScale = 1f;

		[HideInInspector]
		[Range( 0f, 1f )]
		public float m_PreviewEditorWindowScale = 1f;

		[HideInInspector]
		[Range( 0f, 1f )]
		public float m_PreviewAlpha = 1f;

		[HideInInspector]
		public Color m_PreviewColor = Color.white;

		protected virtual void Awake()
		{
			if( Application.isPlaying )
			{
				gameObject.SetActive( false );
			}
		}

		public virtual void SetColor( Color color ) { }
		public virtual Sprite GetSprite() { return null; }
		public virtual void SetSprite( Sprite sprite ) { }
	}
}
