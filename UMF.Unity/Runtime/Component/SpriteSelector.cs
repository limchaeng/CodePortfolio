//////////////////////////////////////////////////////////////////////////
//
// SpriteSelector
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

namespace UMF.Unity
{
	[RequireComponent( typeof( SpriteRenderer ) )]
	public class SpriteSelector : SpriteSelectorBase
	{
		SpriteRenderer mSpriteRenderer = null;
		public SpriteRenderer GetSpriteRenderer
		{
			get
			{
				if( mSpriteRenderer == null )
					mSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();

				return mSpriteRenderer;
			}
		}

		//------------------------------------------------------------------------
		private void Awake()
		{
			mSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		}

		//------------------------------------------------------------------------
		public override void UpdateSprite( Sprite sprite )
		{
			GetSpriteRenderer.SetSafeSprite( sprite );
		}

		//------------------------------------------------------------------------
		public override Sprite GetSprite()
		{
			return GetSpriteRenderer.sprite;
		}

		//------------------------------------------------------------------------
		public override float alpha
		{
			get { return color.a; }
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

		//------------------------------------------------------------------------
		public override Color color
		{
			get => GetSpriteRenderer.color;
			set => GetSpriteRenderer.color = value;
		}
	}
}
