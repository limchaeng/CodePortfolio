//////////////////////////////////////////////////////////////////////////
//
// RectTransformExtensions
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
	//------------------------------------------------------------------------	
	public enum AnchorPresets
	{
		None,

		TopLeft,
		TopCenter,
		TopRight,

		MiddleLeft,
		MiddleCenter,
		MiddleRight,

		BottomLeft,
		BottonCenter,
		BottomRight,
		BottomStretch,

		VertStretchLeft,
		VertStretchRight,
		VertStretchCenter,

		HorStretchTop,
		HorStretchMiddle,
		HorStretchBottom,

		StretchAll
	}

	//------------------------------------------------------------------------	
	public enum ePivotFlag
	{
		None	= 0x0000,

		Top		= 0x0001,
		Bottom	= 0x0002,
		Middle	= 0x0004,

		Left	= 0x0010,
		Right	= 0x0020,
		Center	= 0x0040,
	}

	//------------------------------------------------------------------------	
	public static class RectTransformExtensions
	{
		//------------------------------------------------------------------------	
		public static void SetAnchor( this RectTransform source, AnchorPresets allign, float offsetX = 0f, float offsetY = 0f )
		{
			source.anchoredPosition = new Vector3( offsetX, offsetY, 0f );

			switch( allign )
			{
				case AnchorPresets.TopLeft:
					source.anchorMin = new Vector2( 0, 1 );
					source.anchorMax = new Vector2( 0, 1 );
					break;

				case AnchorPresets.TopCenter:
					source.anchorMin = new Vector2( 0.5f, 1 );
					source.anchorMax = new Vector2( 0.5f, 1 );
					break;
				case AnchorPresets.TopRight:
					source.anchorMin = new Vector2( 1, 1 );
					source.anchorMax = new Vector2( 1, 1 );
					break;

				case AnchorPresets.MiddleLeft:
					source.anchorMin = new Vector2( 0, 0.5f );
					source.anchorMax = new Vector2( 0, 0.5f );
					break;

				case AnchorPresets.MiddleCenter:
					source.anchorMin = new Vector2( 0.5f, 0.5f );
					source.anchorMax = new Vector2( 0.5f, 0.5f );
					break;

				case AnchorPresets.MiddleRight:
					source.anchorMin = new Vector2( 1, 0.5f );
					source.anchorMax = new Vector2( 1, 0.5f );
					break;

				case AnchorPresets.BottomLeft:
					source.anchorMin = new Vector2( 0, 0 );
					source.anchorMax = new Vector2( 0, 0 );
					break;

				case AnchorPresets.BottonCenter:
					source.anchorMin = new Vector2( 0.5f, 0 );
					source.anchorMax = new Vector2( 0.5f, 0 );
					break;

				case AnchorPresets.BottomRight:
					source.anchorMin = new Vector2( 1, 0 );
					source.anchorMax = new Vector2( 1, 0 );
					break;

				case AnchorPresets.HorStretchTop:
					source.anchorMin = new Vector2( 0, 1 );
					source.anchorMax = new Vector2( 1, 1 );
					break;

				case AnchorPresets.HorStretchMiddle:
					source.anchorMin = new Vector2( 0, 0.5f );
					source.anchorMax = new Vector2( 1, 0.5f );
					break;

				case AnchorPresets.HorStretchBottom:
					source.anchorMin = new Vector2( 0, 0 );
					source.anchorMax = new Vector2( 1, 0 );
					break;

				case AnchorPresets.VertStretchLeft:
					source.anchorMin = new Vector2( 0, 0 );
					source.anchorMax = new Vector2( 0, 1 );
					break;

				case AnchorPresets.VertStretchCenter:
					source.anchorMin = new Vector2( 0.5f, 0 );
					source.anchorMax = new Vector2( 0.5f, 1 );
					break;

				case AnchorPresets.VertStretchRight:
					source.anchorMin = new Vector2( 1, 0 );
					source.anchorMax = new Vector2( 1, 1 );
					break;

				case AnchorPresets.StretchAll:
					source.anchorMin = new Vector2( 0, 0 );
					source.anchorMax = new Vector2( 1, 1 );
					break;
			}
		}

		//------------------------------------------------------------------------	
		public static void SetPivot( this RectTransform source, ePivotFlag pivot )
		{
			Vector2 s_pivot = source.pivot;
			if( ( pivot & ePivotFlag.Top ) != 0 )
				s_pivot.y = 1f;
			else if( ( pivot & ePivotFlag.Middle ) != 0 )
				s_pivot.y = 0.5f;
			else if( ( pivot & ePivotFlag.Bottom ) != 0 )
				s_pivot.y = 0f;

			if( ( pivot & ePivotFlag.Left ) != 0 )
				s_pivot.x = 0f;
			else if( ( pivot & ePivotFlag.Center ) != 0 )
				s_pivot.x = 0.5f;
			else if( ( pivot & ePivotFlag.Right ) != 0 )
				s_pivot.x = 1f;

			source.pivot = s_pivot;
		}

		//------------------------------------------------------------------------	
        public static bool IsOverlapping( RectTransform a, RectTransform b, Canvas canvas )
        {
            if( a == null || b == null || canvas == null )
                return false;

            Vector3[] aCorners = new Vector3[4];
            Vector3[] bCorners = new Vector3[4];

            a.GetWorldCorners( aCorners );
            b.GetWorldCorners( bCorners );

            // 화면공간 Rect 계산
            Rect aRect = GetScreenRect( aCorners, canvas );
            Rect bRect = GetScreenRect( bCorners, canvas );

            return aRect.Overlaps( bRect );
        }

        private static Rect GetScreenRect( Vector3[] corners, Canvas canvas )
        {
            Vector2 min = RectTransformUtility.WorldToScreenPoint( canvas.worldCamera, corners[0] );
            Vector2 max = min;

            for( int i = 1; i < 4; i++ )
            {
                Vector2 screenCorner = RectTransformUtility.WorldToScreenPoint( canvas.worldCamera, corners[i] );
                min = Vector2.Min( min, screenCorner );
                max = Vector2.Max( max, screenCorner );
            }

            return new Rect( min, max - min );
        }

		//------------------------------------------------------------------------
		/// anchor,pivot 상관없이 가운데 월드좌표 반환
		public static Vector3 GetViewWorldCenter( this RectTransform rt )
		{
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners( corners );

            // 4개의 코너 평균
            Vector3 center = ( corners[0] + corners[1] + corners[2] + corners[3] ) / 4f;
            return center;
        }

        //------------------------------------------------------------------------
        public static void RectTransformScaling( this RectTransform rt, RectTransform bounds_rect )
        {
            rt.RectTransformScaling( bounds_rect, Vector2.one );
        }

        public static void RectTransformScaling( this RectTransform rt, RectTransform bounds_rect, Vector2 def_scale )
        {
            Vector2 _scale = def_scale;
            if( rt.rect.height > bounds_rect.rect.height )
                _scale.y = bounds_rect.rect.height / rt.rect.height;

            if( rt.rect.width > bounds_rect.rect.width )
                _scale.x = bounds_rect.rect.width / rt.rect.width;

            Vector3 vscale = rt.localScale;
            vscale.x = vscale.y = Mathf.Min( _scale.x, _scale.y );
            rt.localScale = vscale;
        }
    }

    //------------------------------------------------------------------------	
    public static class ScrollRectExtensions
	{
		//------------------------------------------------------------------------	
		public static void SnapTo( this ScrollRect instance, Transform target, bool to_center )
		{
			Canvas.ForceUpdateCanvases();

			Vector2 snap_pos = instance.content.anchoredPosition;
			Vector2 calc_pos = (Vector2)instance.transform.InverseTransformPoint( instance.content.position ) - (Vector2)instance.transform.InverseTransformPoint( target.position );

			if( instance.horizontal )
				snap_pos.x = calc_pos.x;
			if( instance.vertical )
				snap_pos.y = calc_pos.y;

			if( to_center )
				snap_pos.y += (instance.transform as RectTransform).sizeDelta.y * 0.5f;

			instance.content.anchoredPosition = snap_pos;					
		}

		//------------------------------------------------------------------------	
		public static Vector2 GetSnapToPositionToBringChildIntoView( this ScrollRect instance, Transform child )
		{
			Canvas.ForceUpdateCanvases();
			Vector2 viewportLocalPosition = instance.viewport.localPosition;
			Vector2 childLocalPosition = child.localPosition;

			Vector2 result = instance.content.localPosition;

			if( instance.horizontal )
				result.x = 0 - ( viewportLocalPosition.x + childLocalPosition.x );

			if( instance.vertical )
			{
				result.y = 0 + ( viewportLocalPosition.y - childLocalPosition.y );
			}

			return result;
		}
	}

}