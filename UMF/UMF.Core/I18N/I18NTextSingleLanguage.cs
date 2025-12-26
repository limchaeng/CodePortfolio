//////////////////////////////////////////////////////////////////////////
//
// I18NTextSingleLanguage
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

namespace UMF.Core.I18N
{
	public class I18NTextSingleLanguage : I18NTextDataBase<I18NTextSingleLanguage>
	{
		protected string mCurrLanguage = "";
		public string CurrLanguage { get { return mCurrLanguage; } }
		System.Globalization.CultureInfo mCultureInfo = null;
		public System.Globalization.CultureInfo GetCultureInfo { get { return mCultureInfo; } }
		public bool WordWrapEnable = true;

		//------------------------------------------------------------------------
		protected override string LANGUAGE_DEFAULT { get { return mCurrLanguage; } }
		public override string RELOAD_DATA_ID => "I18NText";
		public sealed override string ReloadData() { return ""; }

		//------------------------------------------------------------------------
		public void SetLanguage( string language )
		{
			if( mCurrLanguage != language )
				mCultureInfo = null;

			mCurrLanguage = language;
		}

		//------------------------------------------------------------------------
		public bool Load( byte[] load_data, string language, bool append_language = false )
		{
			SetLanguage( language );

			if( load_data == null )
				return false;

			XmlDocument doc = new XmlDocument();
			doc.Load( new System.IO.MemoryStream( load_data ) );
			return Load( doc, language, append_language );
		}

		public bool Load( XmlDocument doc, string language, bool append_language = false )
		{
			SetLanguage( language );

			XmlNode stringDataNode = doc.SelectSingleNode( "DataList" );
			if( stringDataNode == null )
				return false;

			string data_language = XMLUtil.ParseAttribute<string>( stringDataNode, "language", "" );
			if( string.IsNullOrEmpty( data_language ) )
				data_language = language;

			if( append_language == false )
			{
				int word_wrap_enable = XMLUtil.ParseAttribute<int>( stringDataNode, "wordwrap", -1 );
				if( word_wrap_enable != -1 )
					WordWrapEnable = ( word_wrap_enable == 1 ? true : false );

				if( mCultureInfo == null )
				{
					string culture_code = XMLUtil.ParseAttribute<string>( stringDataNode, "culture", "" );
					if( string.IsNullOrEmpty( culture_code ) == false )
						mCultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture( culture_code );
				}
			}

			AppendTextData( stringDataNode, data_language, true );
			return true;
		}

		//------------------------------------------------------------------------		
		public string GetText( Enum enum_value, params object[] parms )
		{
			return GetText( I18NTextConst.EnumToKey( enum_value ), parms );
		}
		public string GetText( string key, params object[] parms )
		{
			return base.GetTextBase( mCurrLanguage, key, parms );
		}

		//------------------------------------------------------------------------		
		public string GetTextWithCulture( Enum enum_value, params object[] parms )
		{
			return GetTextWithCulture( I18NTextConst.EnumToKey( enum_value ), parms );
		}
		public string GetTextWithCulture( string key, params object[] parms )
		{
			return GetTextBaseWithCultureFormat( mCurrLanguage, key, mCultureInfo, parms );
		}

		//------------------------------------------------------------------------		
		public bool Contains( Enum enum_value )
		{
			return Contains( I18NTextConst.EnumToKey( enum_value ) );
		}
		public bool Contains( string key )
		{
			return Contains( mCurrLanguage, key );
		}

		//------------------------------------------------------------------------		
		public List<string> GetTextAllList( Enum enum_value )
		{
			return GetTextAllList( I18NTextConst.EnumToKey( enum_value ) );
		}
		public List<string> GetTextAllList( string key )
		{
			return GetTextAllList( mCurrLanguage, key );
		}

		//------------------------------------------------------------------------		
		public Dictionary<string, I18NTextCategory> GetTextData()
		{
			return GetLanguageTextFull( mCurrLanguage );
		}

		//------------------------------------------------------------------------		
		public void UpdateLanguageText( Enum enum_value, List<string> str_list )
		{
			UpdateLanguageText( I18NTextConst.EnumToKey( enum_value ), str_list );
		}
		public void UpdateLanguageText( string key, List<string> str_list )
		{
			UpdateLanguageText( mCurrLanguage, key, str_list );
		}
	}
}
