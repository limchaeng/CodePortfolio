//////////////////////////////////////////////////////////////////////////
//
// SpriteGridInspector
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
	[CustomEditor(typeof(SpriteGrid))]
	public class SpriteGridInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			if( GUILayout.Button("Execute" ) )
			{
				SpriteGrid sg = target as SpriteGrid;
				if( sg != null )
					sg.Reposition();
			}
			base.OnInspectorGUI();
		}
	}
}
