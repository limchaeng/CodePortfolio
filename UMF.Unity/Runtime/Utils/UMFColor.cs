//////////////////////////////////////////////////////////////////////////
//
// UMFColor
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
using System.Globalization;
using UnityEngine;

namespace UMF.Unity
{
	public static class UMFColor
	{
		//------------------------------------------------------------------------	
		public static Color ColorFromHEX( string hex_color )
		{
			try
			{
				string hex = hex_color.Trim();
				if( hex.IndexOf( '#' ) != -1 )
					hex = hex.Replace( "#", "" );

				int r = 255, g = 255, b = 255, a = 255;

				if( hex.Length >= 2 )
					r = int.Parse( hex.Substring( 0, 2 ), NumberStyles.AllowHexSpecifier );

				if( hex.Length >= 4 )
					g = int.Parse( hex.Substring( 2, 2 ), NumberStyles.AllowHexSpecifier );

				if( hex.Length >= 6 )
					b = int.Parse( hex.Substring( 4, 2 ), NumberStyles.AllowHexSpecifier );

				if( hex.Length >= 8 )
					a = int.Parse( hex.Substring( 6, 2 ), NumberStyles.AllowHexSpecifier );

				return new Color32( (byte)r, (byte)g, (byte)b, (byte)a );
			}
			catch( System.Exception ex )
			{
				Debug.LogWarning( ex.ToString() );
			}

			return Color.white;
		}

		//------------------------------------------------------------------------
		// 보색
		public static Color ComplementaryColor( Color color )
		{
            Color.RGBToHSV( color, out float H, out float S, out float V );

            // 보색 계산 (색조 180도 이동)
            float complementaryH = ( H + 0.5f ) % 1f; // 0.5는 180도를 의미 (0~1 사이 값)

            // HSV를 다시 RGB로 변환
            return Color.HSVToRGB( complementaryH, S, V );
        }
    }
}
