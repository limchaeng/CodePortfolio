//////////////////////////////////////////////////////////////////////////
//
// I18NTextDataBase
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

using System;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;

namespace UMF.Core.I18N
{
	//------------------------------------------------------------------------	
	public class I18NTextDataInfo
	{
		public string data_id = "";
		public int version = 0;
		/// <summary>
		///   -1=keep current 0=disable 1=enable
		/// </summary>
		public int word_wrap = -1;
		public DateTime created_time = DateTime.MinValue;

		public I18NTextDataInfo( string _data_id )
		{
			data_id = _data_id;
		}
	}

	//------------------------------------------------------------------------	
	public class I18NTextLanguageInfo
	{
		public string language = "";
		public List<I18NTextDataInfo> data_info_list = new List<I18NTextDataInfo>();

		public I18NTextLanguageInfo( string language )
		{
			this.language = language;
		}

		public int GetVersion( string data_id )
		{
			I18NTextDataInfo data = data_info_list.Find( a => a.data_id == data_id );
			if( data != null )
				return data.version;

			return 0;
		}
	}

	//------------------------------------------------------------------------	
	public abstract class I18NTextDataBase<T> : I18NTextBase<T> where T : DataReloadBase, new()
	{
		protected List<I18NTextLanguageInfo> mLanguageInfoList = new List<I18NTextLanguageInfo>();
		public List<I18NTextLanguageInfo> LanguageInfoList { get { return mLanguageInfoList; } }

		protected abstract string LANGUAGE_DEFAULT { get; }

		//------------------------------------------------------------------------
		public override void ClearTexts()
		{
			base.ClearTexts();
			mLanguageInfoList.Clear();
		}

		//------------------------------------------------------------------------
		public I18NTextDataInfo FindDataInfo( string language, string data_id )
		{
			I18NTextLanguageInfo v_data = mLanguageInfoList.Find( a => a.language == language );
			if( v_data != null )
				return v_data.data_info_list.Find( a => a.data_id == data_id );

			return null;
		}

		//------------------------------------------------------------------------
		protected void AppendTextData( XmlNode data_node, string language, bool update_exist )
		{
			string data_id = XMLUtil.ParseAttribute<string>( data_node, "data_id", "" );
			int version = XMLUtil.ParseAttribute<int>( data_node, "version", 0 );
			int word_wrap_enable = XMLUtil.ParseAttribute<int>( data_node, "wordwrap", -1 );
			DateTime created_time = XMLUtil.ParseAttribute<DateTime>( data_node, "created_time", DateTime.Now );

			I18NTextLanguageInfo v_data = mLanguageInfoList.Find( a => a.language == language );
			if( v_data == null )
			{
				v_data = new I18NTextLanguageInfo( language );
				mLanguageInfoList.Add( v_data );
			}

			I18NTextDataInfo f_data = v_data.data_info_list.Find( a => a.data_id == data_id );
			if( f_data == null )
			{
				f_data = new I18NTextDataInfo( data_id );
				v_data.data_info_list.Add( f_data );
			}

			f_data.version = version;
			f_data.word_wrap = word_wrap_enable;
			f_data.created_time = created_time;

			foreach( XmlNode categoryNode in data_node.ChildNodes )
			{
				if( categoryNode.NodeType == XmlNodeType.Comment )
					continue;

				bool ignore_translate_cat = XMLUtil.ParseAttribute<bool>( categoryNode, I18NTextConst.eTranslateFlag.IgnoreTranslate.ToString(), false );

				foreach( XmlNode dataNode in categoryNode.ChildNodes )
				{
					if( dataNode.NodeType == XmlNodeType.Comment )
						continue;

					string id = dataNode.Attributes["id"].Value;

					// for tool translate options
					I18NTextConst.eTranslateFlag translate_flag = I18NTextConst.eTranslateFlag.None;
					bool ignore_translate = ignore_translate_cat;
					if( ignore_translate == false )
						ignore_translate = XMLUtil.ParseAttribute<bool>( dataNode, I18NTextConst.eTranslateFlag.IgnoreTranslate.ToString(), false );

					if( ignore_translate )
						translate_flag |= I18NTextConst.eTranslateFlag.IgnoreTranslate;

					if( XMLUtil.ParseAttribute<bool>( dataNode, I18NTextConst.eTranslateFlag.EmptyText.ToString(), false ) )
						translate_flag |= I18NTextConst.eTranslateFlag.EmptyText;

					if( XMLUtil.ParseAttribute<int>( dataNode, I18NTextConst.eTranslateFlag.IgnoreTagCheck.ToString(), 0 ) == 1 )
						translate_flag |= I18NTextConst.eTranslateFlag.IgnoreTagCheck;

					string key = categoryNode.Name + "_" + id;
					int childCount = dataNode.ChildNodes.Count;
					if( childCount == 0 || ( childCount == 1 && dataNode.ChildNodes[0].NodeType == XmlNodeType.Text ) )
					{
						AddText( language, categoryNode.Name, id, key, dataNode.InnerText, update_exist, translate_flag );
					}
					else
					{
						AddText( language, categoryNode.Name, id, key, dataNode.ChildNodes, update_exist, translate_flag );
					}
				}
			}
		}

		//------------------------------------------------------------------------		
		public string GetTextBase( string language, Enum enum_value, params object[] parms )
		{
			return GetTextBase( language, I18NTextConst.EnumToKey( enum_value ), parms );
		}
		public string GetTextBase( string language, string key, params object[] parms )
		{
			return GetTextBaseWithCultureFormat( language, key, null, parms );
		}

		//------------------------------------------------------------------------		
		public string GetTextBaseWithCultureFormat( string language, Enum enum_value, CultureInfo culture_info, params object[] parms )
		{
			return GetTextBaseWithCultureFormat( language, I18NTextConst.EnumToKey( enum_value ), culture_info, parms );
		}
		public string GetTextBaseWithCultureFormat( string language, string key, CultureInfo culture_info, params object[] parms )
		{
			if( string.IsNullOrEmpty( key ) )
				return "";

			string text;
			if( GetLanguageText( language, key, out text ) == false )
			{
				if( string.IsNullOrEmpty( LANGUAGE_DEFAULT ) == false && language != LANGUAGE_DEFAULT )
				{
					GetLanguageText( LANGUAGE_DEFAULT, key, out text );
				}
			}

			if( string.IsNullOrEmpty( text ) == false )
			{
				try
				{
					string real_text = I18NTextConst.CheckFormatException( text, parms );

					if( culture_info != null )
					{
						if( parms == null )
							return real_text;
						else
							return string.Format( culture_info, real_text, parms );
					}
					else
					{
						if( parms == null )
							return real_text;
						else
							return string.Format( real_text, parms );
					}
				}
				catch( System.Exception ex )
				{
					Log.WriteWarning( ex.ToString() );
				}
			}

			return key;
		}

		//------------------------------------------------------------------------		
		public bool Contains( string language, Enum enum_value )
		{
			return Contains( language, I18NTextConst.EnumToKey( enum_value ) );
		}
		public bool Contains( string language, string key )
		{
			if( string.IsNullOrEmpty( key ) )
				return false;

			return ContainLanguage( language, key );
		}

		//------------------------------------------------------------------------		
		public List<string> GetTextAllList( string language, Enum enum_value )
		{
			return GetTextAllList( language, I18NTextConst.EnumToKey( enum_value ) );
		}
		public List<string> GetTextAllList( string language, string key )
		{
			if( string.IsNullOrEmpty( key ) )
				return null;

			return GetLanguageTextAllList( language, key );
		}
	}
}
