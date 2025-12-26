//////////////////////////////////////////////////////////////////////////
//
// CustomTextData
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

using System.Collections.Generic;
using System.Xml;

namespace UMF.Core.I18N
{
	public class CustomTextData
	{
		public string Language { get; private set; }
		Dictionary<string, string> mTextDic = new Dictionary<string, string>();

		public CustomTextData( XmlNode node )
		{
			Language = XMLUtil.ParseAttribute<string>( node, "Language", "" );
			mTextDic.Clear();
			foreach( XmlNode child in node.ChildNodes )
			{
				if( child.NodeType == XmlNodeType.Comment )
					continue;

				string id = XMLUtil.ParseAttribute<string>( child, "id", "" );
				if( string.IsNullOrEmpty( id ) )
					continue;

				if( mTextDic.ContainsKey( id ) )
					mTextDic[id] = child.InnerText.Replace( "\\n", "\n" );
				else
					mTextDic.Add( id, child.InnerText.Replace( "\\n", "\n" ) );
			}
		}

		//------------------------------------------------------------------------
		public string GetText( string key )
		{
			string txt;
			if( mTextDic.TryGetValue( key, out txt ) )
				return txt;

			return "";
		}

		//------------------------------------------------------------------------
		/// <summary>
		///   return Language1|Text1|Language2|Text2|...
		/// </summary>
		public static string MakeTokenText( List<CustomTextData> list, string key, params object[] parms )
		{
			string ret = "";
			foreach( CustomTextData t_data in list )
			{
				string find_text = t_data.GetText( key );
				if( string.IsNullOrEmpty( find_text ) == false )
				{
					if( parms != null && parms.Length > 0 )
						find_text = string.Format( find_text, parms );

					ret += string.Format( "{0}|{1}|", t_data.Language, find_text );
				}
			}

			return ret;
		}

		//------------------------------------------------------------------------
		public static string FindText( List<CustomTextData> list, string key, string localize )
		{
			if( list == null )
				return "";

			CustomTextData t_data = list.Find( t => t.Language == localize );
			if( t_data != null )
			{
				return t_data.GetText( key );
			}

			return "";
		}
	}
}
