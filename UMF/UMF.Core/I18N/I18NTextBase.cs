//////////////////////////////////////////////////////////////////////////
//
// I18NTextBase
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
using System;
using System.Xml;
using System.Linq;

namespace UMF.Core.I18N
{
	public class I18NTextConst
	{
		public const string DEFAULT_PATH_ROOT = "_I18NText";
		public const string DEFAULT_PATH_ENCRYPT = "Encrypt";
		public const string DEFAULT_PATH_LANGUAGES = "Languages";
		public const string DEFAULT_PATH_CODE = "_code";

		public const string EXTENSION_XML = ".xml";
		public const string EXTENSION_XML_ENCRYPT = ".xenc";

		[System.Flags]
		public enum eTranslateFlag
		{
			None = 0x0000,
			IgnoreTranslate = 0x0001,
			EmptyText = 0x0002,
			IgnoreTagCheck = 0x0004,
		}


		//------------------------------------------------------------------------
		public static string EnumToKey( Enum enum_value )
		{
			return string.Format( "{0}_{1}", enum_value.GetType().Name, enum_value );
		}

		//------------------------------------------------------------------------		
		public static string CheckFormatException( string parse_text, object[] parms )
		{
			if( parse_text.Contains( "{" ) == false )
				return parse_text;

			int offset = -1;
			int endoffset = -1;

			List<string> replace_list = null;
			int max_len = parse_text.Length;
			while( true )
			{
				max_len -= 1;
				offset = parse_text.IndexOf( "{", endoffset + 1 );
				if( offset != -1 )
				{
					endoffset = parse_text.IndexOf( "}", offset );
					if( endoffset != -1 )
					{
						if( endoffset < parse_text.Length - 1 && parse_text[endoffset + 1] == '}' )
						{
							endoffset += 1;
						}
						else
						{
							string param_index = parse_text.Substring( offset + 1, endoffset - offset - 1 );
							int idx;
							if( int.TryParse( param_index, out idx ) )
							{
								if( parms == null || idx >= parms.Length )
								{
									// error
									if( replace_list == null )
										replace_list = new List<string>();

									replace_list.Add( parse_text.Substring( offset, endoffset - offset + 1 ) );
								}
							}
							else
							{
								break;
							}
						}
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}

				if( max_len < 0 )
					break;
			}

			if( replace_list != null )
			{
				foreach( string r_text in replace_list )
				{
					parse_text = parse_text.Replace( r_text, "{" + r_text + "}" );
				}
			}

			return parse_text;
		}
	}

	public class I18NTextCategory
	{
		public class TextData
		{
			public TextData( string text ) { this.text = text; }
			public string text { get; private set; }
		}

		List<TextData> mTextList = new List<TextData>();
		public int Count { get { return mTextList.Count; } }
		string mCategoryName = "";
		string mID = "";
		I18NTextConst.eTranslateFlag mTranslateFlags = 0;

		public string Category { get { return mCategoryName; } }
		public string ID { get { return mID; } }
		public I18NTextConst.eTranslateFlag TranslateFlags { get { return mTranslateFlags; } }

		public I18NTextCategory( string category, string id, List<string> text_list, I18NTextConst.eTranslateFlag translate_flags )
		{
			mID = id;
			mCategoryName = category;
			mTranslateFlags = translate_flags;

			for( int i = 0; i < text_list.Count; i++ )
			{
				TextData _TextData = new TextData( text_list[i].Replace( "\\n", "\n" ) );
				mTextList.Add( _TextData );
			}
		}

		//------------------------------------------------------------------------			
		public I18NTextCategory( string category, string id, string text, I18NTextConst.eTranslateFlag translate_flags )
		{
			mID = id;
			mCategoryName = category;
			mTranslateFlags = translate_flags;

			TextData _TextData = new TextData( text.Replace( "\\n", "\n" ) );
			mTextList.Add( _TextData );
		}

		//------------------------------------------------------------------------			
		public I18NTextCategory( string category, string id, XmlNodeList nodes, I18NTextConst.eTranslateFlag translate_flags )
		{
			mID = id;
			mCategoryName = category;
			mTranslateFlags = translate_flags;

			foreach( XmlNode node in nodes )
			{
				if( node.NodeType == XmlNodeType.Comment )
					continue;

				TextData _TextData = new TextData( node.InnerText.Replace( "\\n", "\n" ) );
				mTextList.Add( _TextData );
			}
		}

		//------------------------------------------------------------------------			
		public bool HasTranslateFlag( I18NTextConst.eTranslateFlag flag )
		{
			return ( ( mTranslateFlags & flag ) != 0 );
		}

		//------------------------------------------------------------------------			
		public string Text
		{
			get
			{
				if( mTextList.Count == 0 )
					return "";

				if( mTextList.Count == 1 )
					return mTextList[0].text;

				return mTextList[UMFRandom.Instance.NextRange( 0, mTextList.Count - 1 )].text;
			}
		}

		//------------------------------------------------------------------------			
		public List<string> GetAllList()
		{
			List<string> retList = new List<string>();
			foreach( TextData data in mTextList )
			{
				retList.Add( data.text );
			}

			return retList;
		}

		//------------------------------------------------------------------------			
		public void UpdateTexts( List<string> texts )
		{
			mTextList.Clear();
			foreach( string s in texts )
			{
				mTextList.Add( new TextData( s ) );
			}
		}
	}

	public abstract class I18NTextBase<T> : DataReloadSingleton<T> where T : DataReloadBase, new()
	{
		protected Dictionary<string, Dictionary<string, I18NTextCategory>> mLanguageTexts = new Dictionary<string, Dictionary<string, I18NTextCategory>>();

		//------------------------------------------------------------------------			
		public int CountAll()
		{
			return mLanguageTexts.Values.Sum( a => a.Values.Sum( b => b.Count ) );
		}

		//------------------------------------------------------------------------			
		public virtual void ClearTexts()
		{
			mLanguageTexts.Clear();
		}

		//------------------------------------------------------------------------			
		protected void AddText( string language, string category, string id, string key, XmlNodeList nodes, bool updateExist, I18NTextConst.eTranslateFlag translate_flags )
		{
			if( nodes.Count == 0 )
				return;

			if( ContainLanguage( language, key ) == true )
			{
				Dictionary<string, I18NTextCategory> strings;
				mLanguageTexts.TryGetValue( language, out strings );

				if( updateExist )
				{
					strings[key.ToLower()] = new I18NTextCategory( category, id, nodes, translate_flags );
				}
				else
				{
					throw new Exception( string.Format( "Already has a key : {0}", key ) );
				}
			}
			else
			{
				Dictionary<string, I18NTextCategory> texts;
				if( mLanguageTexts.TryGetValue( language, out texts ) == false )
				{
					texts = new Dictionary<string, I18NTextCategory>();
					mLanguageTexts.Add( language, texts );
				}

				texts.Add( key.ToLower(), new I18NTextCategory( category, id, nodes, translate_flags ) );
			}
		}

		//------------------------------------------------------------------------			
		protected void AddText( string language, string category, string id, string key, string text, bool updateExist, I18NTextConst.eTranslateFlag translate_flags )
		{
			if( ContainLanguage( language, key ) == true )
			{
				if( updateExist )
				{
					Dictionary<string, I18NTextCategory> texts;
					mLanguageTexts.TryGetValue( language, out texts );

					texts[key.ToLower()] = new I18NTextCategory( category, id, text, translate_flags );
				}
				else
				{
					throw new Exception( string.Format( "Already has a key : {0}", key ) );
				}
			}
			else
			{
				Dictionary<string, I18NTextCategory> texts;
				if( mLanguageTexts.TryGetValue( language, out texts ) == false )
				{
					texts = new Dictionary<string, I18NTextCategory>();
					mLanguageTexts.Add( language, texts );
				}

				texts.Add( key.ToLower(), new I18NTextCategory( category, id, text, translate_flags ) );
			}
		}

		//------------------------------------------------------------------------			
		public bool GetLanguageText( string language, string key, out string text )
		{
			Dictionary<string, I18NTextCategory> strings;
			if( mLanguageTexts.TryGetValue( language, out strings ) )
			{
				I18NTextCategory data;
				if( strings.TryGetValue( key.ToLower(), out data ) )
				{
					text = data.Text;
					return true;
				}
			}

			text = null;
			return false;
		}

		//------------------------------------------------------------------------			
		public bool ContainLanguage( string language, string key )
		{
			Dictionary<string, I18NTextCategory> strings;
			if( mLanguageTexts.TryGetValue( language, out strings ) )
				return strings.ContainsKey( key.ToLower() );

			return false;
		}

		//------------------------------------------------------------------------			
		public List<string> GetLanguageTextAllList( string language, string key )
		{
			Dictionary<string, I18NTextCategory> strings;
			if( mLanguageTexts.TryGetValue( language, out strings ) )
			{
				I18NTextCategory data;
				if( strings.TryGetValue( key.ToLower(), out data ) )
				{
					return data.GetAllList();
				}
			}

			return null;
		}

		//------------------------------------------------------------------------
		public Dictionary<string, List<string>> GetTextAllLanguages( string key )
		{
			if( string.IsNullOrEmpty( key ) )
				return null;

			Dictionary<string, List<string>> dic = null;

			foreach( KeyValuePair<string, Dictionary<string, I18NTextCategory>> kvp in mLanguageTexts )
			{
				I18NTextCategory data;
				if( kvp.Value.TryGetValue( key.ToLower(), out data ) )
				{
					if( dic == null )
						dic = new Dictionary<string, List<string>>();

					dic[kvp.Key] = data.GetAllList();
				}
			}

			return dic;
		}

		//------------------------------------------------------------------------			
		public List<I18NTextCategory> GetLanguageTextCategoryAll( string language, string category )
		{
			if( string.IsNullOrEmpty( category ) )
				return null;

			Dictionary<string, I18NTextCategory> strings;
			if( mLanguageTexts.TryGetValue( language, out strings ) )
			{
				List<I18NTextCategory> ret = new List<I18NTextCategory>();
				foreach( I18NTextCategory data in strings.Values )
				{
					if( data.Category == category )
					{
						ret.Add( data );
					}
				}

				return ret;
			}

			return null;
		}

		//------------------------------------------------------------------------			
		public void RemoveCategory( string language, string category )
		{
			Dictionary<string, I18NTextCategory> strings;
			if( mLanguageTexts.TryGetValue( language, out strings ) )
			{
				List<string> removeKey = new List<string>();
				foreach( KeyValuePair<string, I18NTextCategory> kvp in strings )
				{
					if( kvp.Value.Category == category )
						removeKey.Add( kvp.Key );
				}

				for( int i = 0; i < removeKey.Count; i++ )
				{
					strings.Remove( removeKey[i] );
				}
			}
		}

		//------------------------------------------------------------------------		
		public Dictionary<string, I18NTextCategory> GetLanguageTextFull( string language )
		{
			if( mLanguageTexts.ContainsKey( language ) )
				return mLanguageTexts[language];

			return new Dictionary<string, I18NTextCategory>();
		}

		//------------------------------------------------------------------------		
		public void UpdateLanguageText( string language, string id, List<string> text_list )
		{
			Dictionary<string, I18NTextCategory> string_dic = GetLanguageTextFull( language );
			I18NTextCategory data;
			if( string_dic.TryGetValue( id.ToLower(), out data ) )
			{
				data.UpdateTexts( text_list );
			}
			else
			{
				string category = id;
				if( id.IndexOf( "_" ) >= 0 )
					category = id.Substring( 0, id.IndexOf( "_" ) );

				string_dic.Add( id.ToLower(), new I18NTextCategory( category, id, text_list, I18NTextConst.eTranslateFlag.None ) );
			}
		}
	}
}