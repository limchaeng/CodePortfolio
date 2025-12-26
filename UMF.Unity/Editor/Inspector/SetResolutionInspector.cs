//////////////////////////////////////////////////////////////////////////
//
// SetResolutionInspector
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
	[CustomEditor( typeof( SetResolution ) )]
	public class SetResolutionInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			if( GUILayout.Button( "Set Resolution" ) )
			{
				SetResolution sr = target as SetResolution;
				sr.UpdateResolution();
			}

			base.OnInspectorGUI();
		}
	}
}
