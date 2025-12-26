//////////////////////////////////////////////////////////////////////////
//
// SpriteRendererTransformAnchor
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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UMF.Unity
{
	[ExecuteAlways]
	public class SpriteRendererTransformAnchor : MonoBehaviour
	{
		public enum eAnchorType
		{
			TopLeft,
			TopCenter,
			TopRight,

			MiddleLeft,
			MiddleCenter,
			MiddleRight,

			BottomLeft,
			BottonCenter,
			BottomRight,
		}

		public Transform m_Target;
		public SpriteRenderer m_RelativeSprite;
		public eAnchorType m_AnchorType = eAnchorType.MiddleCenter;

		//------------------------------------------------------------------------
		private void OnEnable()
		{
			UpdateAnchor();
		}

		//------------------------------------------------------------------------
		public void UpdateAnchor()
		{
			if( m_RelativeSprite == null || m_Target == null )
				return;

			Vector2 size = m_RelativeSprite.size;
			m_Target.position = m_RelativeSprite.transform.position;

			Vector3 vpos = m_Target.localPosition;
			switch( m_AnchorType )
			{
				case eAnchorType.TopLeft:
				case eAnchorType.MiddleLeft:
				case eAnchorType.BottomLeft:
					vpos.x -= size.x * 0.5f;
					break;

				case eAnchorType.TopRight:
				case eAnchorType.MiddleRight:
				case eAnchorType.BottomRight:
					vpos.x += size.x * 0.5f;
					break;
			}

			switch( m_AnchorType )
			{
				case eAnchorType.TopLeft:
				case eAnchorType.TopCenter:
				case eAnchorType.TopRight:
					vpos.y += size.y * 0.5f;
					break;

				case eAnchorType.BottomLeft:
				case eAnchorType.BottonCenter:
				case eAnchorType.BottomRight:
					vpos.y -= size.y * 0.5f;
					break;
			}

			m_Target.localPosition = vpos;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(SpriteRendererTransformAnchor))]
	public class SpriteRendererTransformAnchorInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();

			if( EditorGUI.EndChangeCheck() )
			{
				SpriteRendererTransformAnchor comp = target as SpriteRendererTransformAnchor;
				comp.UpdateAnchor();
			}
		}
	}
#endif

}