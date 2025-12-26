using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml;
using System.IO;
using UMF.Core.I18N;
using UMF.Core;
using UMTools.Common;

namespace UMTools.TBLExport
{
	public class TranslateData
	{
		public enum eState
		{
			None,
			ADDED,
			REMOVED,
			CHANGED,
			CONFILICT,
			EMPTY,
			IGNORETRANS,    // dummy
			ADDED_EQUAL,
			CHANGED_EQUAL,
			EMPTY_EQUAL,
			//
			ECHANGED,
			KCHANGED,
		}

		public const string MULTI_TEXT_PREFIX = "[=m=]";
		public const string CATEGORY_TABLE_NAME = "_CATEGORY_";
		public const string EMPTY_TEXT_PARSE = "&nbsp;";

		public class StringData
		{
			public string m_ID;
			public string m_Language;
			public eState m_State = eState.None;
			public bool m_IsFullsepChanged = false;
			public I18NTextConst.eTranslateFlag m_TranslateFlags = I18NTextConst.eTranslateFlag.None;
			public List<string> m_TextList = new List<string>();
			// runtime 
			public CategoryData m_CategoryRef = null;
			public string m_EqualCategoryID = "";
			public string m_EqualID = "";
			public bool m_EqualFromOther = false;
			public List<string> m_SaveTextListForEqualChange = null;
			public bool m_IsDuplicatedAdded = false;
			public string m_ChangedPrevText = "";

			public void SetDuplicated( string org_id )
			{
				if( m_ID == org_id )
				{
					m_IsDuplicatedAdded = true;
				}
			}

			public StringData( CategoryData category_ref )
			{
				m_CategoryRef = category_ref;
			}

			public bool HasTranslateFlag( I18NTextConst.eTranslateFlag flag )
			{
				return ( ( m_TranslateFlags & flag ) != 0 );
			}

			public bool IsChanged
			{
				get
				{
					switch( m_State )
					{
						case eState.ADDED:
						case eState.CHANGED:
						case eState.CONFILICT:
						case eState.EMPTY:
						case eState.KCHANGED:
						case eState.ECHANGED:
							return true;
					}
					return false;
				}
			}
			public bool IsTextEmpty
			{
				get
				{
					foreach( string txt in m_TextList )
					{
						if( string.IsNullOrEmpty( txt ) == false )
							return false;
					}

					return true;
				}
			}

			//------------------------------------------------------------------------	
			public void CheckPrev( StringData prev_data )
			{
				if( m_TextList.Count != prev_data.m_TextList.Count )
				{
					m_State = eState.CHANGED;
					m_ChangedPrevText = prev_data.MakeXMLText();
				}
				else
				{
					foreach( string text in m_TextList )
					{
						if( prev_data.m_TextList.Exists( t => t == text ) == false )
						{
							m_State = eState.CHANGED;
						}
					}

					if( m_State == eState.CHANGED )
						m_ChangedPrevText = prev_data.MakeXMLText();
				}
			}

			//------------------------------------------------------------------------	
			public string MakeXMLText()
			{
				string ret_text = "";
				if( m_TextList.Count > 1 )
				{
					string multi_data = "";
					foreach( string str_text in m_TextList )
					{
						multi_data += string.Format( "{0}{1}", TranslateData.MULTI_TEXT_PREFIX, str_text );
					}

					ret_text = multi_data;
				}
				else if( m_TextList.Count == 1 )
				{
					ret_text = m_TextList[0];
				}

				return ret_text;
			}

			//------------------------------------------------------------------------	
			public string MakeExcelText()
			{
				string ret_text = "";
				if( m_TextList.Count > 1 )
				{
					string multi_data = "";
					foreach( string str_text in m_TextList )
					{
						multi_data += string.Format( "{0}{1}", TranslateData.MULTI_TEXT_PREFIX, str_text.Replace( "\\n", "\n" ) );
					}

					ret_text = multi_data;
				}
				else if( m_TextList.Count == 1 )
				{
					ret_text = m_TextList[0].Replace( "\\n", "\n" );
				}

				if( ret_text.StartsWith( "\n" ) && ret_text.Length > 1 )
				{
					ret_text = "\\n" + ret_text.Substring( 1, ret_text.Length - 1 );
				}

				if( HasTranslateFlag( I18NTextConst.eTranslateFlag.EmptyText ) )
					ret_text = EMPTY_TEXT_PARSE;

				return ret_text;
			}

			//------------------------------------------------------------------------	
			public bool IsTextDifferent( List<string> text_list )
			{
				if( m_TextList.Count != text_list.Count )
					return true;

				for( int i = 0; i < m_TextList.Count; i++ )
				{
					if( m_TextList[i] != text_list[i] )
						return true;
				}

				return false;
			}

			//------------------------------------------------------------------------	
			public bool IsNumericID()
			{
				int _n;
				return int.TryParse( m_ID, out _n );
			}

			//------------------------------------------------------------------------	
			public void CopyText( List<string> text_list )
			{
				m_TextList.Clear();
				foreach( string t in text_list )
				{
					m_TextList.Add( t );
				}
			}
		}

		public class CategoryData
		{
			public string m_Category;
			public List<StringData> m_StringList = new List<StringData>();

			public void UpdateEqualData( StringData kor_data, StringData equal_text )
			{
				StringData s_data = m_StringList.Find( s => s.m_ID == kor_data.m_ID && s.m_Language == equal_text.m_Language );
				if( s_data == null )
				{
					s_data = new StringData( this );
					s_data.m_Language = equal_text.m_Language;
					s_data.m_ID = kor_data.m_ID;
					s_data.m_State = kor_data.m_State;

					m_StringList.Add( s_data );
				}

				s_data.m_TranslateFlags = kor_data.m_TranslateFlags;
				s_data.m_State = kor_data.m_State;

				// for equal reference text changed by equal check
				s_data.m_SaveTextListForEqualChange = s_data.m_TextList;

				List<string> t_list = equal_text.m_TextList;
				if( equal_text.m_SaveTextListForEqualChange != null )
					t_list = equal_text.m_SaveTextListForEqualChange;

				s_data.m_TextList = new List<string>();
				foreach( string str in t_list )
				{
					s_data.m_TextList.Add( str );
				}
			}

			public void AddStringData( string language, I18NTextCategory s_data, FormTBLExport main_exporter )
			{
				StringData str_data = new StringData( this );
				str_data.m_Language = language;
				str_data.m_ID = s_data.ID;
				str_data.m_State = eState.None;
				str_data.m_TranslateFlags = (I18NTextConst.eTranslateFlag)s_data.TranslateFlags;
				str_data.m_TextList.AddRange( s_data.GetAllList() );

				if( language !=  TranslateManager.KOREAN_LANGUAGE )
				{
					StringData korean_exist = m_StringList.Find( s => s.m_Language == TranslateManager.KOREAN_LANGUAGE && s.m_ID == s_data.ID );
					if( korean_exist == null )
					{
						str_data.m_State = eState.REMOVED;
					}
					else if( str_data.IsTextEmpty && str_data.HasTranslateFlag( I18NTextConst.eTranslateFlag.EmptyText ) == false )
					{
						str_data.m_State = eState.EMPTY;
					}

					// 					if( m_StringList.Exists(s => s.m_Language == TranslateManager.KOREAN_LANGUAGE && s.m_ID == s_data.ID) == false )
					// 					{
					// 						str_data.m_State = eState.REMOVED;
					// 					}
				}
				else
				{
					if( str_data.IsTextEmpty )
					{
						str_data.m_TextList = new List<string>();
						str_data.m_TextList.Add( EMPTY_TEXT_PARSE );
					}
				}

				m_StringList.Add( str_data );
			}

			//------------------------------------------------------------------------	
			public void CheckPrevKorean( CategoryData prev_category )
			{
				foreach( StringData s_data in m_StringList )
				{
					if( prev_category != null )
					{
						StringData exist_prev = prev_category.m_StringList.Find( s => s.m_Language == s_data.m_Language && s.m_ID == s_data.m_ID );
						if( exist_prev == null )
						{
							s_data.m_State = eState.ADDED;
						}
						else
						{
							s_data.CheckPrev( exist_prev );
						}
					}
					else
					{
						s_data.m_State = eState.ADDED;
					}
				}
			}

			//------------------------------------------------------------------------	
			public void CheckEqualKoreanTextFromPrev( TranslateData prev_data, bool is_other, ref StringBuilder sb )
			{
				foreach( StringData s_data in m_StringList )
				{
					if( s_data.m_State != eState.ADDED && s_data.m_State != eState.CHANGED && s_data.m_State != eState.EMPTY )
						continue;

					string prefix = s_data.m_State.ToString();
					StringData find_data = prev_data.FindStringDataByText( s_data );

					if( find_data != null )
					{
						switch( s_data.m_State )
						{
							case eState.ADDED:
								s_data.m_State = eState.ADDED_EQUAL;
								break;

							case eState.CHANGED:
								s_data.m_State = eState.CHANGED_EQUAL;
								break;

							case eState.EMPTY:
								s_data.m_State = eState.EMPTY_EQUAL;
								break;
						}

						s_data.m_EqualCategoryID = find_data.m_CategoryRef.m_Category;
						s_data.m_EqualID = find_data.m_ID;
						s_data.m_EqualFromOther = is_other;

						if( sb == null )
							sb = new StringBuilder();

						sb.AppendLine( prefix + string.Format( "_EQUAL({0}) : {1}/{2} <= {3}/{4} : {5}", s_data.m_Language,
							m_Category, s_data.m_ID, find_data.m_CategoryRef.m_Category, find_data.m_ID, find_data.MakeXMLText() ) );
					}
				}
			}

			//------------------------------------------------------------------------	
			public void CheckEmptyOtherLanguage( string[] languages, FormTBLExport main_exporter )
			{
				List<StringData> korean_strings = m_StringList.FindAll( s => s.m_Language == TranslateManager.KOREAN_LANGUAGE );

				foreach( string lan in languages )
				{
					foreach( StringData s_data in korean_strings )
					{
						StringData exist = m_StringList.Find( s => s.m_ID == s_data.m_ID && s.m_Language == lan );
						if( exist == null )
						{
							int idx = m_StringList.FindIndex( s => s.m_ID == s_data.m_ID && s.m_Language == TranslateManager.KOREAN_LANGUAGE );

							exist = new StringData( this );
							exist.m_ID = s_data.m_ID;
							exist.m_Language = lan;
							exist.m_State = eState.EMPTY;
							exist.m_TranslateFlags = s_data.m_TranslateFlags;
							m_StringList.Insert( idx + 1, exist );
						}
					}
				}
			}
		}

		List<CategoryData> mCategoryList = new List<CategoryData>();
		public List<CategoryData> CategoryList { get { return mCategoryList; } }

		Dictionary<string, string> mSheetToCategoryDic = new Dictionary<string, string>();

		FormTBLExport mMainExporter = null;
		//------------------------------------------------------------------------	
		public TranslateData( FormTBLExport main_form )
		{
			mMainExporter = main_form;
		}

		//------------------------------------------------------------------------	
		public void UpdateEqualText( StringData kor_data, StringData equal_text )
		{
			CategoryData c_data = mCategoryList.Find( c => c.m_Category == kor_data.m_CategoryRef.m_Category );
			if( c_data == null )
			{
				c_data = new CategoryData();
				c_data.m_Category = kor_data.m_CategoryRef.m_Category;
				mCategoryList.Add( c_data );
			}

			c_data.UpdateEqualData( kor_data, equal_text );
		}

		//------------------------------------------------------------------------	
		public bool SheetValid( string sheet_name )
		{
			if( sheet_name == CATEGORY_TABLE_NAME )
				return true;

			return mSheetToCategoryDic.ContainsKey( sheet_name );
		}

		//------------------------------------------------------------------------	
		public List<StringData> GetCheckedEqualText()
		{
			List<StringData> list = new List<StringData>();
			foreach( CategoryData c_data in mCategoryList )
			{
				foreach( StringData s_data in c_data.m_StringList )
				{
					if( s_data.m_Language == TranslateManager.KOREAN_LANGUAGE &&
						( s_data.m_State == eState.ADDED_EQUAL ||
						s_data.m_State == eState.CHANGED_EQUAL ||
						s_data.m_State == eState.EMPTY_EQUAL ) )
					{
						list.Add( s_data );
					}
				}
			}

			return list;
		}

		//------------------------------------------------------------------------	
		public StringData FindStringDataByTextLanguage( StringData find_kor_data, string find_language )
		{
			foreach( CategoryData c_data in mCategoryList )
			{
				foreach( StringData s_data in c_data.m_StringList )
				{
					if( s_data.m_Language != find_kor_data.m_Language )
						continue;

					if( s_data.IsTextDifferent( find_kor_data.m_TextList ) )
						continue;

					StringData equal_lan_data = c_data.m_StringList.Find( s => s.m_ID == s_data.m_ID && s.m_Language == find_language );
					if( equal_lan_data == null || equal_lan_data.IsTextEmpty )
						continue;

					return equal_lan_data;
				}
			}

			return null;
		}

		//------------------------------------------------------------------------	
		public StringData FindStringDataByText( StringData find_data, bool without_me = false )
		{
			foreach( CategoryData c_data in mCategoryList )
			{
				foreach( StringData s_data in c_data.m_StringList )
				{
					if( s_data.m_Language != find_data.m_Language )
						continue;

					if( without_me && s_data == find_data )
						continue;

					if( s_data.IsTextDifferent( find_data.m_TextList ) == false )
						return s_data;
				}
			}

			return null;
		}

		//------------------------------------------------------------------------	
		public StringData FindStringData( string category, string id, string language )
		{
			CategoryData c_data = mCategoryList.Find( c => c.m_Category == category );
			if( c_data != null )
			{
				return c_data.m_StringList.Find( c => c.m_ID == id && c.m_Language == language );
			}

			return null;
		}

		//------------------------------------------------------------------------	
		public void LoadFromDataset( DataSet ds, bool is_korean, string language, bool is_fullsep, bool is_changedonly, string filepath )
		{
			// category info table find
			if( is_korean )
			{
				mSheetToCategoryDic.Clear();
				foreach( DataTable table in ds.Tables )
				{
					if( table.TableName == CATEGORY_TABLE_NAME )
					{
						// create column
						List<string> column_list = new List<string>();
						DataRow columnRow = table.Rows[0];
						foreach( object column_item in columnRow.ItemArray )
						{
							string column_name = column_item != null ? column_item.ToString() : "";
							column_list.Add( column_name );
						}

						for( int row = 1; row < table.Rows.Count; row++ )
						{
							DataRow dataRow = table.Rows[row];

							string category_name = "";
							string sheet_name = "";
							for( int i = 0; i < dataRow.ItemArray.Length; i++ )
							{
								if( i >= column_list.Count )
									break;

								if( column_list[i] == "sheet" )
								{
									sheet_name = XMLUtil.ConvertXmlTextPreDefined( dataRow[i], true, true );
								}
								else if( column_list[i] == "category" )
								{
									category_name = XMLUtil.ConvertXmlTextPreDefined( dataRow[i], true, true );
								}
							}

							if( string.IsNullOrEmpty( sheet_name ) == false && string.IsNullOrEmpty( category_name ) == false )
							{
								if( mSheetToCategoryDic.ContainsKey( sheet_name ) == false )
									mSheetToCategoryDic.Add( sheet_name, category_name );
							}
						}
						break;
					}
				}
			}

			bool is_tagissue_begin = false;
			foreach( DataTable table in ds.Tables )
			{
				string sheet_name = table.TableName;
				if( sheet_name == CATEGORY_TABLE_NAME )
					continue;

				string category = sheet_name;
				if( mSheetToCategoryDic.ContainsKey( sheet_name ) )
				{
					category = mSheetToCategoryDic[sheet_name];
				}

				CategoryData c_data = null;
				if( is_korean )
				{
					c_data = new CategoryData();
					c_data.m_Category = category;
				}
				else
				{
					c_data = mCategoryList.Find( c => c.m_Category == category );
				}

				if( c_data == null )
					continue;

				// create column
				List<string> column_list = new List<string>();
				DataRow columnRow = table.Rows[0];
				string all_column = "";
				foreach( object column_item in columnRow.ItemArray )
				{
					string column_name = column_item != null ? column_item.ToString() : "";
					column_list.Add( column_name );

					all_column += column_name + ",";

					if( is_changedonly && is_korean == false )
					{
						if( column_name.StartsWith( "id" ) == false && column_name.StartsWith( TranslateManager.KOREAN_LANGUAGE ) == false )
						{
							if( column_name != "_ENG_" && column_name.StartsWith( language ) == false )
							{
								string error = string.Format( "!! Language Column Invalid:\n{0}\n[{1}][{2}]\n{3}", filepath, language, c_data.m_Category, all_column );
								error += "\nContinue?";
								if( System.Windows.Forms.MessageBox.Show( error, "", System.Windows.Forms.MessageBoxButtons.OKCancel ) == System.Windows.Forms.DialogResult.Cancel )
								{
									throw new Exception( error );
								}
							}
						}
					}
				}

				if( is_korean == false )
				{
					if( column_list.Contains( language ) == false )
						mMainExporter.ExportTranslate.Log( "!! Language Column Invalid:\n{0}\n[{1}][{2}]\n{3}", filepath, language, c_data.m_Category, all_column );

					if( column_list.Contains( language + "_State" ) == false )
						mMainExporter.ExportTranslate.Log( "!! Language State Column Invalid:\n{0}\n[{1}][{2}]\n{3}", filepath, language, c_data.m_Category, all_column );
				}

				for( int row = 1; row < table.Rows.Count; row++ )
				{
					DataRow dataRow = table.Rows[row];

					string id = "";
					for( int i = 0; i < dataRow.ItemArray.Length; i++ )
					{
						if( i >= column_list.Count )
							break;

						if( column_list[i].StartsWith( "##" ) )
							continue;

						if( column_list[i] == "id" )
						{
							// step 1
							id = XMLUtil.ConvertXmlTextPreDefined( dataRow[i], true, true );
							if( string.IsNullOrEmpty( id ) )
								break;
						}
						else if( column_list[i].Contains( "_State" ) )
						{
							// step 3
							string state_language = language;
							if( is_korean )
							{
								state_language = column_list[i].Replace( "_State", "" ).Trim();
							}

							StringData data = c_data.m_StringList.Find( l => l.m_ID == id && l.m_Language == state_language );
							if( data != null )
							{
								eState state;
								if( Enum.TryParse( dataRow[i].ToString(), out state ) == true )
								{
									if( state == eState.IGNORETRANS )
										data.m_TranslateFlags |= I18NTextConst.eTranslateFlag.IgnoreTranslate;
									else
										data.m_State = state;
								}
							}
						}
						else if( column_list[i].Contains( "_TagIssue" ) )
						{
							// do nothing
							// step 4
						}
						else
						{
							// step 2
							string str_language = language;
							if( is_korean )
							{
								str_language = column_list[i];
							}
							else
							{
								if( column_list[i] != language )
									continue;
							}

							// step 2
							StringData data = c_data.m_StringList.Find( l => l.m_ID == id && l.m_Language == str_language );
							if( /*is_korean &&*/ data == null )
							{
								data = new StringData( c_data );
								data.m_ID = id;
								data.m_Language = str_language;
								data.m_State = eState.None;
								c_data.m_StringList.Add( data );
							}

							if( data != null )
							{
								string text = XMLUtil.ConvertXmlTextPreDefined( dataRow[i], true, false );
								string[] multiTexts = text.Split( new string[] { MULTI_TEXT_PREFIX }, StringSplitOptions.RemoveEmptyEntries );
								List<string> text_list = multiTexts.ToList();

								if( is_fullsep && data.IsTextDifferent( text_list ) )
									data.m_IsFullsepChanged = true;

								bool forced_changed_apply = false;
								if( is_changedonly && data.m_IsFullsepChanged )
								{
									mMainExporter.ExportTranslate.Log( "- already changed FULLSEP:[{0}][{1}][{2}]\nFULLSEP:{3}\nCHANGEDONLY:{4}", c_data.m_Category, data.m_ID, str_language, data.MakeExcelText(), dataRow[i].ToString() );
									if( text.Trim().Length > 0 )
										forced_changed_apply = true;
								}

								if( forced_changed_apply || is_changedonly == false || ( is_changedonly && data.m_IsFullsepChanged == false ) )
								{
									string prev_text = data.MakeExcelText();
									data.m_TextList = multiTexts.ToList();

									string new_text = data.MakeExcelText();

									if( is_korean == false )
									{
										// expression check
										StringBuilder sb_dup_log = new StringBuilder();
										StringData kor_data = c_data.m_StringList.Find( l => l.m_ID == id && l.m_Language == TranslateManager.KOREAN_LANGUAGE );
										if( kor_data != null )
										{
											// CHANGEDONLY 에서 중복텍스트가 번역수정이 된경우 다른 텍스트(fullsep 에서 이미 들어간것들)도 함께 변경시켜준다.
											if( is_changedonly )
											{
												List<StringData> dup_kr_list = c_data.m_StringList.FindAll( a => a.m_ID != id && a.m_Language == TranslateManager.KOREAN_LANGUAGE && a.IsTextDifferent( kor_data.m_TextList ) == false );
												if( dup_kr_list != null )
												{
													foreach( StringData dup_kr in dup_kr_list )
													{
														StringData lan_string = c_data.m_StringList.Find( a => a.m_ID == dup_kr.m_ID && a.m_Language == str_language );
														if( lan_string != null )
														{
															string lan_prev_text = lan_string.MakeExcelText();
															lan_string.m_TextList = multiTexts.ToList();
															sb_dup_log.AppendLine( $"- CHANGEDONLY DUP CHANGED:[{lan_string.m_ID}][{str_language}]\n{lan_prev_text}");
														}
														else
														{
															sb_dup_log.AppendLine( $"- CHANGEDONLY DUP NOT FOUND:[{dup_kr.m_ID}][{str_language}]" );
														}
													}
												}												
											}

											// future

											//string kor_text = kor_data.MakeExcelText();
											//string error_msg = "";
											//string fixed_text = "";
											//string reverse_error_msg = "";
											//if( ExportUtil.CheckTAG( Path.GetFileNameWithoutExtension( filepath ), kor_data.m_TranslateFlags, kor_text, new_text, mMain.CheckTagPolicyList, mMain.CheckTagPolicyFlags, ref error_msg, ref reverse_error_msg, ref fixed_text ) == false )
											//{
											//	StringBuilder sb = new StringBuilder();
											//	sb.AppendLine( string.Format( "- TagIssue:[{0}][{1}][{2}] {3}", c_data.m_Category, data.m_ID, str_language, error_msg ) );
											//	if( string.IsNullOrEmpty( reverse_error_msg ) == false )
											//	{
											//		sb.AppendLine( "<==== ReverseTagIssue : " + reverse_error_msg );
											//	}
											//	sb.AppendLine( kor_text );
											//	sb.AppendLine( "====>" );
											//	sb.AppendLine( new_text );
											//	if( string.IsNullOrEmpty( fixed_text ) == false )
											//	{
											//		sb.AppendLine( "- Fixed Suggestion" );
											//		sb.AppendLine( fixed_text );
											//	}

											//	mMain.LogWrite( FormTBLExport.eLogType.Normal, sb.ToString() );

											//	if( is_tagissue_begin == false )
											//	{
											//		mMain.LogTranslateTagIssue( "*************************************************************" );
											//		mMain.LogTranslateTagIssue( Path.GetFileName( filepath ) );
											//		mMain.LogTranslateTagIssue( "*************************************************************" );
											//		is_tagissue_begin = true;
											//	}

											//	mMain.LogTranslateTagIssue( "\n-------------------------------------------------------------" );
											//	mMain.LogTranslateTagIssue( sb.ToString() );
											//}
										}

										if( is_fullsep && data.m_IsFullsepChanged )
										{
											mMainExporter.ExportTranslate.Log( "-----------------------------" );
											mMainExporter.ExportTranslate.Log( "- FULLSEP:[{0}][{1}][{2}]\n{3}\n====>\n{4}", c_data.m_Category, data.m_ID, str_language, prev_text, new_text );
										}

										if( is_changedonly )
										{
											mMainExporter.ExportTranslate.Log( "-----------------------------" );
											if( forced_changed_apply )
												mMainExporter.ExportTranslate.Log( "- CHANGEDONLY(F):[{0}][{1}][{2}]\n{3}\n====>\n{4}", c_data.m_Category, data.m_ID, str_language, prev_text, new_text );
											else
												mMainExporter.ExportTranslate.Log( "- CHANGEDONLY:[{0}][{1}][{2}]\n{3}\n====>\n{4}", c_data.m_Category, data.m_ID, str_language, prev_text, new_text );

											if( sb_dup_log.Length > 0 )
												mMainExporter.ExportTranslate.Log( "\n" + sb_dup_log.ToString() );
										}
									}
								}
							}
						}
					}
				}

				if( is_korean )
					mCategoryList.Add( c_data );
			}
		}

		//------------------------------------------------------------------------			
		public void LoadFromXml( string language, string filepath )
		{
			if( string.IsNullOrEmpty( filepath ) || File.Exists( filepath ) == false )
			{
				return;
			}

			XmlDocument doc = new XmlDocument();
			doc.Load( filepath );

			I18NTextSingleLanguage.Instance.ClearTexts();
			I18NTextSingleLanguage.Instance.SetLanguage( language );
			I18NTextSingleLanguage.Instance.Load( doc, language, false );
			Dictionary<string, I18NTextCategory> allData = I18NTextSingleLanguage.Instance.GetTextData();

			foreach( I18NTextCategory s_data in allData.Values )
			{
				string category = s_data.Category;

				CategoryData cat_data = mCategoryList.Find( c => c.m_Category == category );
				if( cat_data != null )
				{
					cat_data.AddStringData( language, s_data, mMainExporter );
				}
				else
				{
					cat_data = new CategoryData();
					cat_data.m_Category = category;
					cat_data.AddStringData( language, s_data, mMainExporter );

					mCategoryList.Add( cat_data );
				}
			}

			I18NTextSingleLanguage.Instance.ClearTexts();
		}

		//------------------------------------------------------------------------	
		public void CheckPrevKorean( TranslateData prev_data, TranslateData other_data, ref StringBuilder sb )
		{
			foreach( CategoryData category in mCategoryList )
			{
				CategoryData prev_category = prev_data.mCategoryList.Find( c => c.m_Category == category.m_Category );
				category.CheckPrevKorean( prev_category );

				// check equal text
				category.CheckEqualKoreanTextFromPrev( prev_data, false, ref sb );
				if( other_data != null )
					category.CheckEqualKoreanTextFromPrev( other_data, true, ref sb );
			}
		}

		//------------------------------------------------------------------------	
		public void CheckEmptyOtherLanguage( string[] languages )
		{
			foreach( CategoryData category in mCategoryList )
			{
				category.CheckEmptyOtherLanguage( languages, mMainExporter );
			}
		}

		//------------------------------------------------------------------------	
		public void CheckEmptyEqualTexts( string[] languages )
		{
			foreach( string lan in languages )
			{
				foreach( CategoryData category in mCategoryList )
				{
					foreach( StringData s_data in category.m_StringList )
					{
						if( s_data.m_Language == lan && s_data.m_State == eState.EMPTY )
						{
							// find korean equal
							StringData kor_data = category.m_StringList.Find( s => s.m_ID == s_data.m_ID && s.m_Language == TranslateManager.KOREAN_LANGUAGE );
							if( kor_data != null )
							{
								StringData equal_lan_data = FindStringDataByTextLanguage( kor_data, lan );
								if( equal_lan_data != null )
								{
									s_data.m_State = eState.EMPTY_EQUAL;
									s_data.CopyText( equal_lan_data.m_TextList );
								}
							}
						}
					}
				}
			}
		}

		//------------------------------------------------------------------------
		public void CheckDuplicatedText( bool is_merge )
		{
			StringBuilder sb = null;
			foreach( CategoryData category in mCategoryList )
			{
				foreach( StringData s_data in category.m_StringList )
				{
					if( s_data.m_Language != TranslateManager.KOREAN_LANGUAGE )
						continue;

					if( is_merge == false )
					{
						if( s_data.m_State != eState.ADDED && s_data.m_State != eState.None )
							continue;
					}
					else
					{
						if( s_data.m_State != eState.ADDED && s_data.m_State != eState.CHANGED )
							continue;
					}

					// find duplicated added data
					_CheckDuplicatedAdded( category, s_data, is_merge, ref sb );
				}

				// remove all duplication set
				category.m_StringList.RemoveAll( s => s.m_IsDuplicatedAdded );
			}

			if( sb != null )
				mMainExporter.ExportTranslate.LogDuplicated( sb.ToString() );
		}
		void _CheckDuplicatedAdded( CategoryData c_data, StringData s_data, bool is_merge, ref StringBuilder sb )
		{
			foreach( CategoryData category in mCategoryList )
			{
				foreach( StringData chk_data in category.m_StringList )
				{
					if( chk_data == s_data )
						break;

					if( chk_data.m_Language != TranslateManager.KOREAN_LANGUAGE )
						continue;

					if( chk_data.m_IsDuplicatedAdded )
						continue;

					if( is_merge == false )
					{
						if( chk_data.m_State != eState.ADDED && chk_data.m_State != eState.None )
							continue;
					}
					else
					{
						if( chk_data.m_State != eState.ADDED && chk_data.m_State != eState.CHANGED )
							continue;
					}

					if( chk_data.IsTextDifferent( s_data.m_TextList ) == false )
					{
						s_data.m_IsDuplicatedAdded = true;

						if( sb == null )
							sb = new StringBuilder();

						sb.AppendLine( "------------------" );
						sb.AppendLine( $"DUP_TEXT({s_data.m_Language}) : {c_data.m_Category}/{s_data.m_ID} => {category.m_Category}/{chk_data.m_ID}" );
						sb.AppendLine( s_data.MakeXMLText() );

						c_data.m_StringList.ForEach( a => a.SetDuplicated( s_data.m_ID ) );
						return;
					}
				}

				if( category == c_data )
					return;
			}
		}

		//------------------------------------------------------------------------	
		public void DoExportXML( string original_filepath, List<TBLExportTranslate.LanguageCultureData> supported_languages, bool changed_equal_only = false, bool from_merge = false )
		{
			string timeString = DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" );

			List<TBLExportTranslate.LanguageCultureData> export_language_list = new List<TBLExportTranslate.LanguageCultureData>();
			// korean 추가
			export_language_list.Add( new TBLExportTranslate.LanguageCultureData( TranslateManager.KOREAN_LANGUAGE, "ko-KR" ) );
			export_language_list.AddRange( supported_languages );

			foreach( TBLExportTranslate.LanguageCultureData lan_data in export_language_list )
			{
				string _filename = Path.GetFileNameWithoutExtension( original_filepath ).Replace( TranslateManager.KOREAN_LANGUAGE, lan_data.language );

				if( changed_equal_only )
				{
					if( lan_data.language == TranslateManager.KOREAN_LANGUAGE )
						continue;

					if( mCategoryList.Exists( c => c.m_StringList.Exists( s => s.m_Language == lan_data.language && s.m_State == eState.ADDED_EQUAL || s.m_State == eState.CHANGED_EQUAL ) ) == false )
						continue;
				}

				XmlDocument doc = new XmlDocument();
				XmlNode rootNode = doc.AppendChild( doc.CreateElement( "DataList" ) );
				XMLUtil.AddAttribute( rootNode, "version", 0 );
				if( original_filepath.Contains( "ServerText" ) )
					XMLUtil.AddAttribute( rootNode, "data_id", "ServerText" );
				else
					XMLUtil.AddAttribute( rootNode, "data_id", "ClientText" );
				XMLUtil.AddAttribute( rootNode, "language", lan_data.language );
				XMLUtil.AddAttribute( rootNode, "created_time", "" );
				XMLUtil.AddAttribute( rootNode, "Culture", lan_data.culture_code );

				foreach( CategoryData category in mCategoryList )
				{
					XmlNode categoryNode = rootNode.AppendChild( rootNode.OwnerDocument.CreateElement( category.m_Category ) );

					if( changed_equal_only )
					{
						if( _filename.StartsWith( "TBL_" ) )
						{
							if( category.m_StringList.Exists( s => s.IsNumericID() ) )
								category.m_StringList = category.m_StringList.OrderBy( s => int.Parse( s.m_ID ) ).ToList();
							else
								category.m_StringList = category.m_StringList.OrderBy( s => s.m_ID ).ToList();
						}
					}

					foreach( StringData s_data in category.m_StringList )
					{
						if( s_data.m_State == eState.REMOVED )
							continue;

						if( s_data.m_Language == lan_data.language )
						{
							List<string> text_list = new List<string>();

							StringData real_s_data = s_data;
							StringData korean_s_data = category.m_StringList.Find( s => s.m_Language == TranslateManager.KOREAN_LANGUAGE && s.m_ID == s_data.m_ID );
							if( korean_s_data == null )
							{
								mMainExporter.ExportTranslate.Log( $"not found {TranslateManager.KOREAN_LANGUAGE} Data({category.m_Category}_{s_data.m_ID})" );
							}
							else if( korean_s_data.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) )
							{
								real_s_data = korean_s_data;
							}

							if( real_s_data.m_TextList.Count > 1 )
							{
								foreach( string text in real_s_data.m_TextList )
								{
									if( string.IsNullOrEmpty( text ) == false )
										text_list.Add( text );
								}
							}
							else
							{
								if( real_s_data.m_TextList.Count == 1 )
									text_list.Add( real_s_data.m_TextList[0] );
							}

							if( real_s_data.m_Language == TranslateManager.KOREAN_LANGUAGE || text_list.Count > 0 )
							{
								XmlNode dataNode = categoryNode.AppendChild( categoryNode.OwnerDocument.CreateElement( "Data" ) );
								XMLUtil.AddAttribute( dataNode, "id", real_s_data.m_ID );

								if( real_s_data.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) )
									XMLUtil.AddAttribute( dataNode, I18NTextConst.eTranslateFlag.IgnoreTranslate.ToString(), true );

								if( text_list.Count > 1 )
								{
									foreach( string text in text_list )
									{
										XmlNode textNode = dataNode.AppendChild( dataNode.OwnerDocument.CreateElement( "Text" ) );
										if( changed_equal_only )    // from load xlsx
											textNode.InnerText = XMLUtil.ReverseConvertXmlPreDefined( text ).Replace( "\n", "\\n" );
										else
											textNode.InnerText = XMLUtil.ReverseConvertXmlPreDefined( text );
									}
								}
								else if( text_list.Count == 1 )
								{
									if( text_list[0].Replace( "&amp;", "&" ) == EMPTY_TEXT_PARSE )
										XMLUtil.AddAttribute( dataNode, I18NTextConst.eTranslateFlag.EmptyText.ToString(), true );

									if( changed_equal_only )
										dataNode.InnerText = XMLUtil.ReverseConvertXmlPreDefined( text_list[0] ).Replace( "\n", "\\n" );
									else
										dataNode.InnerText = XMLUtil.ReverseConvertXmlPreDefined( text_list[0] );
								}
								else
								{
									dataNode.InnerText = "";
								}
							}
						}
					}
				}

				string o_dir_name = Path.GetDirectoryName( original_filepath );

				string export_dir = o_dir_name + "/" + I18NTextExportConst.TRANSLATION_IMPORT_PATH_PREFIX + System.DateTime.Now.ToString( "yyyyMMdd" );
				if( changed_equal_only )
					export_dir = o_dir_name + "/" + I18NTextExportConst.TRANSLATION_IMPORT_EQUAL_PREFIX + System.DateTime.Now.ToString( "yyyyMMdd" );
				else if( from_merge )
					export_dir = o_dir_name + "/" + I18NTextExportConst.TRANSLATION_MERGED_PATH_PREFIX + System.DateTime.Now.ToString( "yyyyMMdd" );

				if( Directory.Exists( export_dir ) == false )
					Directory.CreateDirectory( export_dir );

				if( _filename.Contains( "ServerText" ) )
				{
					// for diff
					string export_dir2 = Path.Combine( export_dir, "ServerText" );
					if( Directory.Exists( export_dir2 ) == false )
						Directory.CreateDirectory( export_dir2 );

					XMLUtil.SaveXmlDocToFile( Path.Combine( export_dir2, _filename + ".xml" ), doc );
				}
				else
				{
					// for diff
					string export_dir2 = export_dir + "/" + I18NTextConst.DEFAULT_PATH_LANGUAGES + "/" + lan_data.language;
					if( Directory.Exists( export_dir2 ) == false )
						Directory.CreateDirectory( export_dir2 );
					XMLUtil.SaveXmlDocToFile( Path.Combine( export_dir2, _filename + ".xml" ), doc );
				}
			}
		}
	}

	//------------------------------------------------------------------------	
	public class TranslateManager : Singleton<TranslateManager>
	{
		public const string KOREAN_LANGUAGE = "Korean";

		FormTBLExport mMainExporter = null;

		//------------------------------------------------------------------------	
		public void Init( FormTBLExport exporter )
		{
			mMainExporter = exporter;
		}

		//------------------------------------------------------------------------	
		public void Clear()
		{
		}

		//------------------------------------------------------------------------
		TranslateData LoadPrevDataAll( string source_path, string prev_folder )
		{
			TranslateData data = new TranslateData( mMainExporter );

			bool has_prev_data = false;
			string lan_prev_path = Path.Combine( prev_folder, I18NTextConst.DEFAULT_PATH_LANGUAGES );
			string[] lan_prev_dir_list = Directory.GetDirectories( lan_prev_path );
			foreach( string lan_prev_dir in lan_prev_dir_list )
			{
				string lan_name = Path.GetFileName( lan_prev_dir );
				string[] xml_file_list = Directory.GetFiles( lan_prev_dir, "*.xml" );
				if( xml_file_list == null || xml_file_list.Length <= 0 )
					continue;

				foreach( string xml_file in xml_file_list )
				{
					has_prev_data = true;
					mMainExporter.ExportTranslate.Log( "=> ExecuteMergePrevLoad(ALL):{0} <-> {1}", source_path, xml_file );
					data.LoadFromXml( lan_name, xml_file );
				}
			}

			if( has_prev_data == false )
				return null;

			return data;
		}

		//------------------------------------------------------------------------	
		public TranslateData ExecuteMerge( bool is_merge, string source_path, string prev_folder, string other_prev_folder, string[] languages )
		{
			// load
			TranslateData source = new TranslateData( mMainExporter );
			source.LoadFromXml( KOREAN_LANGUAGE, source_path );

			List<TranslateData.StringData> kor_equal_text_list = null;
			TranslateData previous_translate_data = null;
			TranslateData other_previous_translate_data = null;

			if( is_merge )
			{
				if( string.IsNullOrEmpty( prev_folder ) == false )
				{
					bool is_server = false;
					string kor_prev_path = Path.Combine( prev_folder, I18NTextConst.DEFAULT_PATH_LANGUAGES, KOREAN_LANGUAGE );
					if( Directory.Exists( kor_prev_path ) == false )
					{
						kor_prev_path = Path.Combine( prev_folder, "ServerText" );
						if( Directory.Exists( kor_prev_path ) )
							is_server = true;
						else
							return null;
					}

					string prev_path_for_server = "";
					string other_path_for_server = "";

					// for find equal text all TBL, Language texts
					if( is_server == false )
					{
						previous_translate_data = LoadPrevDataAll( source_path, prev_folder );
						if( previous_translate_data == null )
							return null;

						mMainExporter.ExportTranslate.Log( "ExecuteMergePrevLoad(ALL):{0} <-> {1}", source_path, prev_folder );

						if( string.IsNullOrEmpty( other_prev_folder ) == false )
						{
							other_previous_translate_data = LoadPrevDataAll( source_path, other_prev_folder );
							mMainExporter.ExportTranslate.Log( "ExecuteMergePrevLoad(OtherALL):{0} <-> {1}", source_path, other_prev_folder );
						}
					}
					else
					{
						previous_translate_data = new TranslateData( mMainExporter );

						prev_path_for_server = Path.Combine( kor_prev_path, Path.GetFileName( source_path ) );
						mMainExporter.ExportTranslate.Log( "ExecuteMergePrevLoad:{0} <-> {1}", source_path, prev_path_for_server );
						previous_translate_data.LoadFromXml( KOREAN_LANGUAGE, prev_path_for_server );

						if( string.IsNullOrEmpty( other_prev_folder ) == false )
						{
							other_path_for_server = Path.Combine( other_prev_folder, I18NTextConst.DEFAULT_PATH_LANGUAGES, KOREAN_LANGUAGE );
							if( Directory.Exists( other_path_for_server ) == false )
							{
								kor_prev_path = Path.Combine( other_prev_folder, "ServerText" );
								if( Directory.Exists( other_path_for_server ) )
								{
									string other_prev_path = Path.Combine( other_path_for_server, Path.GetFileName( source_path ) ); ;
									other_previous_translate_data = new TranslateData( mMainExporter );
									mMainExporter.ExportTranslate.Log( "ExecuteMergePrevLoad(Other):{0} <-> {1}", source_path, other_prev_path );
									other_previous_translate_data.LoadFromXml( KOREAN_LANGUAGE, other_prev_path );
								}
							}
						}
					}

					StringBuilder check_log = null;
					source.CheckPrevKorean( previous_translate_data, other_previous_translate_data, ref check_log );
					if( check_log != null )
					{
						mMainExporter.ExportTranslate.LogTranslateEqual( check_log.ToString() );
					}

					kor_equal_text_list = source.GetCheckedEqualText();

					// add other language
					if( is_server )
					{
						foreach( string other_lan in languages )
						{
							string other_path = prev_path_for_server.Replace( KOREAN_LANGUAGE, other_lan );
							mMainExporter.ExportTranslate.Log( "ExecuteMerge:{0} <-> {1}", source_path, other_path );
							source.LoadFromXml( other_lan, other_path );
						}
					}
					else
					{
						string prev_lan_path = Path.Combine( prev_folder, I18NTextConst.DEFAULT_PATH_LANGUAGES );
						foreach( string other_lan in languages )
						{
							string lan_path = Path.Combine( prev_lan_path, other_lan );
							string other_path = Path.GetFileName( source_path ).Replace( KOREAN_LANGUAGE, other_lan );

							mMainExporter.ExportTranslate.Log( "ExecuteMerge:{0} <-> {1}", source_path, other_path );
							source.LoadFromXml( other_lan, Path.Combine( lan_path, other_path ) );
						}
					}
				}
			}
			else
			{
				string source_file_name = Path.GetFileName( source_path );
				string source_dir = Path.GetDirectoryName( source_path );
				bool is_server = false;
				if( source_file_name.StartsWith( "ServerText" ) )
					is_server = true;

				// add other language
				if( is_server )
				{
					foreach( string other_lan in languages )
					{
						string other_path = source_path.Replace( KOREAN_LANGUAGE, other_lan );
						mMainExporter.ExportTranslate.Log( "ExecuteExport:{0} <-> {1}", source_path, other_path );
						source.LoadFromXml( other_lan, other_path );
					}
				}
				else
				{
					foreach( string other_lan in languages )
					{
						string lan_path = Path.Combine( source_dir, other_lan );
						if( Directory.Exists( lan_path ) == false )
							lan_path = Path.Combine( source_dir, "..", other_lan );

						string other_path = source_file_name.Replace( KOREAN_LANGUAGE, other_lan );

						mMainExporter.ExportTranslate.Log( "ExecuteExport:{0} <-> {1}", source_path, other_path );
						source.LoadFromXml( other_lan, Path.Combine( lan_path, other_path ) );
					}
				}
			}

			// check empty other language
			source.CheckEmptyOtherLanguage( languages );
			CheckMergeEqualTexts( kor_equal_text_list, source, previous_translate_data, other_previous_translate_data, languages );
			source.CheckEmptyEqualTexts( languages );

			if( is_merge == false && mMainExporter.IsOptionEnabled( FormTBLExport.eOptionEnableType.OPT_I18NText_ExportDuplicateInclude ) == false )
			{
				source.CheckDuplicatedText( false );
			}

			return source;
		}

		//------------------------------------------------------------------------	
		void CheckMergeEqualTexts( List<TranslateData.StringData> kor_equal_list, TranslateData source, TranslateData prev_data, TranslateData other_prev_data, string[] languages )
		{
			if( kor_equal_list != null && kor_equal_list.Count > 0 )
			{
				foreach( string lan in languages )
				{
					foreach( TranslateData.StringData s_data in kor_equal_list )
					{
						string equal_category = s_data.m_EqualCategoryID;
						string equal_id = s_data.m_EqualID;

						TranslateData.StringData equal_data = null;
						if( s_data.m_EqualFromOther == false )
						{
							if( prev_data != null )
								equal_data = prev_data.FindStringData( equal_category, equal_id, lan );
							else
								equal_data = source.FindStringData( equal_category, equal_id, lan );
						}
						else if( other_prev_data != null )
						{
							equal_data = other_prev_data.FindStringData( equal_category, equal_id, lan );
						}

						if( equal_data != null )
						{
							source.UpdateEqualText( s_data, equal_data );
						}
					}
				}
			}
		}
	}
}
