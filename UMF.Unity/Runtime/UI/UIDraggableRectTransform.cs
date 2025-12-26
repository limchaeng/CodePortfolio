//////////////////////////////////////////////////////////////////////////
//
// UIDraggableRectTransform
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
using UnityEngine.EventSystems;

namespace UMF.Unity.UI
{
	[RequireComponent( typeof( RectTransform ) )]
	public class UIDraggableRectTransform : MonoBehaviour, IDragHandler
	{
		RectTransform mRT = null;

		void Start()
		{
			mRT = GetComponent<RectTransform>();
		}

		public void OnDrag( PointerEventData eventData )
		{
			if( mRT != null )
			{
				mRT.anchoredPosition += eventData.delta;
			}
		}
	}
}