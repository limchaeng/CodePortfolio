//////////////////////////////////////////////////////////////////////////
//
// SpriteMaskFillAmount
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
	[ExecuteAlways]
	[RequireComponent(typeof(SpriteMask))]
	public class SpriteMaskFillAmount : MonoBehaviour
	{
		public enum eFillType
		{
			None,
			Horizontal,
		}

		public eFillType m_FillType = eFillType.Horizontal;
		public bool m_Reverse;
		[Range( 0f, 1f )]
		public float m_Value;

		[Tooltip("for size determine")]
		public SpriteRenderer m_MaskTargetSprite;

		private SpriteMask mSpriteMask;
		private Transform mTransaform;
		private bool mIsInit = false;

		private float mAmount = 0f;
		private Vector3 mBaseScale = Vector3.zero;

		private void Awake()
		{
			Init();
		}

		//------------------------------------------------------------------------
		void Init()
		{
			if( m_MaskTargetSprite == null )
				return;

			if( mIsInit )
				return;

			mIsInit = true;

			mTransaform = transform;
			mTransaform.localPosition = Vector3.zero;
			if( mSpriteMask == null )
				mSpriteMask = gameObject.GetComponent<SpriteMask>();

			Rect target_rect = m_MaskTargetSprite.sprite.rect;
			Rect mask_rect = mSpriteMask.sprite.rect;

			Vector3 vscale = Vector3.one;
			vscale.x = ( target_rect.width / m_MaskTargetSprite.sprite.pixelsPerUnit ) / ( mask_rect.width / mSpriteMask.sprite.pixelsPerUnit );
			vscale.y = ( target_rect.height / m_MaskTargetSprite.sprite.pixelsPerUnit ) / ( mask_rect.height / mSpriteMask.sprite.pixelsPerUnit );

			mTransaform.localScale = vscale;
			mBaseScale = vscale;

			switch(m_FillType)
			{
				case eFillType.Horizontal:
					mAmount = target_rect.width / m_MaskTargetSprite.sprite.pixelsPerUnit;
					break;
			}
		}

		//------------------------------------------------------------------------		
		public float fillAmount
		{
			get { return m_Value; }
			set
			{
				m_Value = value;
				UpdateMask();
			}
		}

		//------------------------------------------------------------------------
		public void UpdateMask()
		{
			if( m_MaskTargetSprite == null )
				return;

			Init();

			float fill_amount = m_Value * mAmount;

			Vector3 vpos = mTransaform.localPosition;
			Vector3 vscale = mTransaform.localScale;
			switch(m_FillType)
			{
				case eFillType.Horizontal:
					vpos.x = ( fill_amount * 0.5f ) * ( m_Reverse ? -1f : 1f );
					vscale.x = mBaseScale.x - fill_amount;
					break;
			}

			mTransaform.localPosition = vpos;
			mTransaform.localScale = vscale;
		}

#if UNITY_EDITOR
		void OnValidate()
		{
			mIsInit = false;
			UpdateMask();
		}
#endif
	}
}
