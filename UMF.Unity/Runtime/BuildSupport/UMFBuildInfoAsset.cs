//////////////////////////////////////////////////////////////////////////
//
// UMFBuildInfoAsset
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
	public class UMFBuildInfoAsset : ScriptableObject
	{
		// Application.CompanyName, Application.ProductName 대신하여 사용. 빌드시 들어가는 name 들은 로그 위치를 위해 path valid 하게 변형되어 들어감.
		// - 플렛폼별로 다르게 들어감
		public string m_CompanyName = "";
		public string m_ProductName = "";
		public int m_BuildNumber = 0;
		public int m_XBuildNumber = 0;
		public int m_XRevision = 0;
		public string m_BuildVersion = "";
		public string m_BundleIdentifier = "";
		//
		public int m_AndroidLastBuildVersionCode = 0;   // for verify

		//------------------------------------------------------------------------	
		public virtual void Clear()
		{
			m_CompanyName = "";
			m_ProductName = "";
			m_BuildNumber = 0;
			m_XBuildNumber = 0;
			m_XRevision = 0;
			m_BuildVersion = "";
			m_BundleIdentifier = "";
			m_AndroidLastBuildVersionCode = 0;
		}

		public void DoSave()
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty( this );
			UnityEditor.AssetDatabase.SaveAssets();
#endif
		}
	}
}