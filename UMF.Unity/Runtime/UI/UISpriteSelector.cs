//////////////////////////////////////////////////////////////////////////
//
// UISpriteSelector
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

namespace UMF.Unity.UI
{
	[RequireComponent( typeof( Image ) )]
	public class UISpriteSelector : SpriteSelectorBase
	{
		public bool m_SetNativeSize = false;

		Image mImage = null;
		public Image GetImage
		{
			get
			{
				if( mImage == null )
					mImage = gameObject.GetComponent<Image>();

				return mImage;
			}
		}

		public override float alpha
		{
			get	{ return color.a; }
			set
			{
				Color col = color;
				if( col.a != value )
				{
					col.a = value;
					color = col;
				}
			}
		}

		public override Color color
		{
			get
			{
				return GetImage.color;
			}
			set
			{
				GetImage.color = value;
			}
		}

		//------------------------------------------------------------------------
		private void Awake()
		{
			mImage = gameObject.GetComponent<Image>();
		}

		//------------------------------------------------------------------------		
		public override void UpdateSprite( Sprite sprite )
		{
			GetImage.SetSafeSprite( sprite );

			if( m_SetNativeSize )
			{
				GetImage.SetNativeSize();
                Vector2 size = GetImage.rectTransform.sizeDelta;
                size *= GetImage.canvas.referencePixelsPerUnit;
                GetImage.rectTransform.sizeDelta = size;
            }
        }

		//------------------------------------------------------------------------
		public override Sprite GetSprite()
		{
			return GetImage.sprite;
		}
	}
}