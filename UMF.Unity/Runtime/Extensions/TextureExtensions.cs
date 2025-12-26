//////////////////////////////////////////////////////////////////////////
//
// TextureExtensions
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
	public static class TextureExtensions 
	{
		//------------------------------------------------------------------------
		public static Texture2D ResizeTexture( this Texture2D tex, int width, int height )
		{
			RenderTexture rt = RenderTexture.GetTemporary( width, height );
			RenderTexture.active = rt;
			Graphics.Blit( tex, rt );
			Texture2D new_tex = new Texture2D( width, height, tex.format, false );
			new_tex.ReadPixels( new Rect( 0, 0, width, height ), 0, 0 );
			new_tex.Apply();
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary( rt );

			return new_tex;
		}
	}
}