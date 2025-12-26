//////////////////////////////////////////////////////////////////////////
//
// UMFAppBuildInfo
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
using System.Xml;
using UMF.Core;
using UnityEngine;

namespace UMF.Unity.EditorUtil
{
	public class UMFAppBuildInfo
	{
		//------------------------------------------------------------------------
		public class DoNotShipInfo
		{
			public bool IsFolder { get; private set; }
			public string Path { get; private set; }

			public DoNotShipInfo( XmlNode node )
			{
				IsFolder = XMLUtil.ParseAttribute<bool>( node, "IsFolder", true );
				Path = node.InnerText;
			}
		}

		//------------------------------------------------------------------------	
		public class LocalizeInfo
		{
			public string UsedLocalize { get; protected set; }
			public string FontPath { get; protected set; }

			public LocalizeInfo( XmlNode node )
			{
				UsedLocalize = XMLUtil.ParseAttribute<string>( node, "UsedLocalize", "" );
				FontPath = XMLUtil.ParseAttribute<string>( node, "FontPath", "" );
			}
		}

		public string BundleID { get; protected set; }
		public bool LogDebugMode { get; protected set; }
		public string ProductName { get; protected set; }
		public string CompanyName { get; protected set; }
		public string AppFileName { get; protected set; }
		public string GlobalTypeName { get; protected set; }
		public string DefaultFont { get; protected set; }

		public List<LocalizeInfo> UsedLocalizeList { get; protected set; }
		public List<DoNotShipInfo> DoNotShipList { get; private set; }

		// runtime add
		List<string> mDeletePostFileList = new List<string>();
		public List<string> DeletePostFileList { get { return mDeletePostFileList; } }
		public string UWP_DisplayNameAdded { get; set; } = "";

		public UMFAppBuildInfo()
		{
			BundleID = "Default";
			LogDebugMode = false;
			ProductName = "UMFProduct";
			CompanyName = "UMFCompany";
			AppFileName = "umfapp";
			GlobalTypeName = "NE";
			DefaultFont = "";

			UsedLocalizeList = null;
			DoNotShipList = null;
		}

		public UMFAppBuildInfo( XmlNode node, UMFAppBuildInfo def_info )
		{
			BundleID = node.Name;
			LogDebugMode = XMLUtil.ParseAttribute<bool>( node, "LogDebugMode", def_info.LogDebugMode );
			ProductName = XMLUtil.ParseAttribute<string>( node, "ProductName", def_info.ProductName );
			CompanyName = XMLUtil.ParseAttribute<string>( node, "CompanyName", def_info.CompanyName );
			AppFileName = XMLUtil.ParseAttribute<string>( node, "AppFileName", def_info.AppFileName );
			GlobalTypeName = XMLUtil.ParseAttribute<string>( node, "GlobalType", def_info.GlobalTypeName );
			DefaultFont = XMLUtil.ParseAttribute<string>( node, "DefaultFont", def_info.DefaultFont );

			XmlNode localize_node = node.SelectSingleNode( "Localize" );
			if( localize_node != null )
			{
				UsedLocalizeList = new List<LocalizeInfo>();
				foreach( XmlNode child in localize_node.SelectNodes( "Info" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					UsedLocalizeList.Add( new LocalizeInfo( child ) );
				}
			}
			else
			{
				UsedLocalizeList = def_info.UsedLocalizeList;
			}

			XmlNode donotship_node = node.SelectSingleNode( "DoNotShip" );
			if( donotship_node != null )
			{
				DoNotShipList = new List<DoNotShipInfo>();
				foreach( XmlNode child in donotship_node.SelectNodes( "Info" ) )
				{
					if( child.NodeType == XmlNodeType.Comment )
						continue;

					DoNotShipList.Add( new DoNotShipInfo( child ) );
				}
			}
			else
			{
				DoNotShipList = def_info.DoNotShipList;
			}
		}
	}
}