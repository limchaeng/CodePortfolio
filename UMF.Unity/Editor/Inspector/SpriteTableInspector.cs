//////////////////////////////////////////////////////////////////////////
//
// SpriteTableInspector
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
	[CustomEditor(typeof(SpriteTable), true)]
	public class SpriteTableInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			if( GUILayout.Button( "Execute" ) )
			{
				SpriteTable st = target as SpriteTable;
				if( st != null )
					st.Reposition();
			}

			base.OnInspectorGUI();
		}
	}
}
