//////////////////////////////////////////////////////////////////////////
//
// UISrcPreviewInspector
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
using UnityEditor;

namespace UMF.Unity.EditorUtil
{
	[CustomEditor( typeof( SrcPreview ), true )]
	public class SrcPreviewInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			SrcPreview comp = target as SrcPreview;
			if( comp == null )
				return;

			if( GUILayout.Button( "Open window" ) )
			{
				SrcPreviewEditor.Show( comp );
			}

			SrcPreviewEditor.DrawSrcPreviewControl( comp, true );
		}
	}
}
