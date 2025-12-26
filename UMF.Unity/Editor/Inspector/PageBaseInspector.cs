//////////////////////////////////////////////////////////////////////////
//
// PageBaseInspector
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
	[CustomEditor( typeof( PageBase ), true )]
	public class PageBaseInspector : PrefabRootBehaviourInspector
	{
		public override void PreDraw()
		{
			PageBase pb = target as PageBase;

			base.PreDraw();

			//InspectorUtil.DrawHeader( "PageBase" );
		}
	}
}