//////////////////////////////////////////////////////////////////////////
//
// PrefabRoot
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
	public abstract class PrefabRootBehaviour : MonoBehaviour
	{
		[HideInInspector]
		public bool m_EditorBaseInspectorExpand = true;

		public abstract void P_Init();
		public abstract void P_UnInit();
		public virtual void P_PreUnInit() { }

		public string PrefabName
		{
			get { return gameObject.name; }
		}
	}
}
