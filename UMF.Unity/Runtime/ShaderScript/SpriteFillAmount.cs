//////////////////////////////////////////////////////////////////////////
//
// SpriteFillAmount
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
	[RequireComponent( typeof( SpriteRenderer ) )]
	public class SpriteFillAmount : MonoBehaviour
	{
		public enum eFillType
		{
			Vertical,
			Horizontal,
		}

		public eFillType m_FillType = eFillType.Vertical;
		[Range( 0f, 1f )]
		public float m_Amount = 1f;
		public bool m_Reverse = false;
		public float m_ScrollSpeed = 0f;

		readonly string ShaderName_H = "UMF/Sprites/SpriteFill_H";
		readonly string ShaderName_V = "UMF/Sprites/SpriteFill_V";

		readonly int ShaderPropertyID_Amount = Shader.PropertyToID( "_FillAmount" );
		readonly int ShaderProprtyID_Reverse = Shader.PropertyToID( "_Reverse" );
		readonly int ShaderProprtyID_ScrollSpeed = Shader.PropertyToID( "_ScrollSpeed" );

		private SpriteRenderer mSpriteRenderer;
		public SpriteRenderer SpriteRenderer { get { return mSpriteRenderer; } }
		private Material mMaterial;

		public float Amount
		{
			get { return m_Amount; }
			set
			{
				if( m_Amount != value )
				{
					m_Amount = value;
					UpdateAmount();
				}
			}
		}

		private void Awake()
		{
			mMaterial = null;
			mSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
			Init();
		}

		//------------------------------------------------------------------------
		public void Init()
		{
			if( mSpriteRenderer == null )
			{
				mSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
				return;
			}

			if( mSpriteRenderer == null )
				return;

			string shader_name = "";
			switch( m_FillType )
			{
				case eFillType.Horizontal:
					shader_name = ShaderName_H;
					break;

				case eFillType.Vertical:
					shader_name = ShaderName_V;
					break;
			}

			if( mMaterial == null || mMaterial.shader == null || mMaterial.shader.name.Equals( shader_name ) == false )
			{
				Shader shader = Shader.Find( shader_name );
				if( shader != null )
				{
					if( mMaterial == null )
						mMaterial = new Material( shader );
					else
						mMaterial.shader = shader;

					mSpriteRenderer.sharedMaterial = mMaterial;
				}
			}

			if( mMaterial != null )
			{
				mMaterial.SetFloat( ShaderProprtyID_Reverse, m_Reverse ? 1f : 0f );
				mMaterial.SetFloat( ShaderProprtyID_ScrollSpeed, m_ScrollSpeed );
			}

			UpdateAmount();
		}

		//------------------------------------------------------------------------
		void UpdateAmount()
		{
			if( mMaterial != null )
			{
				mMaterial.SetFloat( ShaderPropertyID_Amount, m_Amount );
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			Init();
			UpdateAmount();
		}
#endif
	}
}
	