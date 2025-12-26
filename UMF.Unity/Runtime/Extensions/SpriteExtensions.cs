//////////////////////////////////////////////////////////////////////////
//
// SpriteExtensions
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

namespace UMF.Unity
{
	public static class SpriteExtensions
	{
		//------------------------------------------------------------------------
		public static void SetSafeSprite( this Image ui_image, Sprite sprite )
		{
			ui_image.sprite = sprite;
			if( ui_image.sprite == null )
				ui_image.enabled = false;
			else
				ui_image.enabled = true;
		}

		//------------------------------------------------------------------------
		public static void SetSafeSprite( this SpriteRenderer sr, Sprite sprite )
		{
			sr.sprite = sprite;
			if( sr.sprite == null )
				sr.enabled = false;
			else
				sr.enabled = true;
		}

		//------------------------------------------------------------------------
		public static void CheckSpriteInvalid( this Image ui_image )
		{
			if( ui_image.sprite == null )
				ui_image.enabled = false;
			else
				ui_image.enabled = true;
		}

		//------------------------------------------------------------------------
		public static void SetAlpha( this SpriteRenderer sr, float alpha )
		{
			Color color = sr.color;
			color.a = alpha;
			sr.color = color;
		}

        public static Vector2 GetSpritePolygonSize( Sprite sprite )
		{
            List<Vector2> shape = new List<Vector2>();
            sprite.GetPhysicsShape( 0, shape );
			if( shape.Count == 0 )
				return sprite.rect.size;
            
			Vector2 min = shape[0];
            Vector2 max = shape[0];

            foreach( var p in shape )
            {
                min = Vector2.Min( min, p );
                max = Vector2.Max( max, p );
            }

			return ( max - min ) * sprite.pixelsPerUnit;
        }

        public static Vector2[] GetSpritePolygons( Sprite sprite, RectTransform rt, bool is_local = false )
        {
            List<Vector2> shape = new List<Vector2>();
            sprite.GetPhysicsShape( 0, shape );

            Vector2[] points = new Vector2[shape.Count];

            //// 스프라이트 픽셀 단위 pivot → 유닛 단위
            //Vector2 spritePivot = sprite.pivot / sprite.pixelsPerUnit;

            //// RectTransform pivot (0~1)
            //Vector2 rtPivot = rt.pivot;
            //Vector2 rtSize = rt.rect.size;

            for( int i = 0; i < shape.Count; i++ )
            {
				//// 1. 픽셀 → 유닛
				//Vector2 local = shape[i] / sprite.pixelsPerUnit;

				//            // 2. 스프라이트 pivot 보정
				//            local -= spritePivot;

				//            // 3. RectTransform 크기(scale) 반영
				//            local = Vector2.Scale( local, rtSize / sprite.rect.size );

				//            // 4. RectTransform pivot 보정 (0~1 → -0.5~0.5)
				//            local += ( new Vector2( 0.5f, 0.5f ) - rtPivot ) * rtSize;


				//// 5. 최종 RectTransform local position
				//points[i] = local;

				// 왜인지 나중에 체크
				if( is_local )
					points[i] = ( shape[i] * sprite.pixelsPerUnit );
				else
                    points[i] = ( shape[i] * sprite.pixelsPerUnit ) + rt.anchoredPosition;
            }

            return points;
        }

    }
}
