using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml;
using System.IO;
using UMTools.Common;
using UMF.Core;
using UMF.Core.I18N;

namespace UMTools.TBLExport
{
	public class I18NTextIgnoreTranslateData
	{
		public string m_FieldName;
		public List<string> m_StartWithList;
	}

	public class I18NTextAddData
	{
		public string m_SheetName;
		public I18NTextData m_data;
		public List<string> m_fieldNameList;
	}

	public class I18NTextData
	{
		public const string I18NText_SHEET = "I18N";
		public const string I18NText_FIELD = "I18N";
		public const string I18NText_TEXT = "TEXT";
		public const string I18NText_SINGLE_KEY = "KEY";
		public const string I18NText_MULTI_KEY = "KEY_";		
		public const string I18NText_ADD_FIELD = "I18NText";
		public const string I18NText_IGNORE_START = "$$";

		public class TextFieldData
		{
			public string fieldName;
			public string id;
			public string text;
			public I18NTextConst.eTranslateFlag translate_flags;

			public bool IsNumericID()
			{
				int _n;
				return int.TryParse( id, out _n );
			}

			public bool HasTranslateFlag( I18NTextConst.eTranslateFlag flag)
			{
				return ( ( translate_flags & flag ) != 0 );
			}
		}

		FormTBLExport mForm;
		string mCategory;
		List<string> mKeyFieldList = new List<string>();
		List<string> mI18NTextFieldList = new List<string>();
		List<string> mI18NTextAddFieldList = new List<string>();
		public List<string> I18NTextFieldList { get { return mI18NTextFieldList; } }
		List<TextFieldData> mTextList = new List<TextFieldData>();
		public List<TextFieldData> TextList { get { return mTextList; } }
		public bool CategorySort { get; set; }
		public bool DataSort { get; set; }

		public I18NTextData( FormTBLExport form, string category )
		{
			mForm = form;
			mCategory = category;
			CategorySort = true;
			DataSort = true;
		}

		public override string ToString()
		{
			string str = string.Format( "I18NText Field Data:{0}\n", mCategory );
			for( int i = 0; i < mKeyFieldList.Count; i++ )
				str += string.Format( "[KEY_{0}:{1}],", i, mKeyFieldList[i] );

			str += "\n";
			for( int i = 0; i < mI18NTextFieldList.Count; i++ )
				str += string.Format( "[FIELD {0}:{1}", i, mI18NTextFieldList[i] );

			return str;
		}

		string MakeKey()
		{
			return "";
		}

		//------------------------------------------------------------------------
		public void SetKeyField(string field, int insertIdx=0)
		{
			if( insertIdx > 0 )
			{ 
				if( mKeyFieldList.Count < insertIdx )
					mKeyFieldList.Capacity = insertIdx;
				mKeyFieldList[insertIdx - 1] = field;
			}
			else
			{ 
				mKeyFieldList.Add( field );
			}
		}
		public void AddI18NTextdField(string field)
		{
			mI18NTextFieldList.Add( field );
		}
		public void MakeI18NTextText(List<string> field_name_list, DataRow data_row)
		{
			bool trim_ignore = ProjectConfig.Instance.CurrentProjectGlobaltypeProperty.export_text_trim_ignore;

			string id = "";
			for(int i=0; i<mKeyFieldList.Count; i++)
			{
				for( int j = 0; j < field_name_list.Count; j++ )
				{
					if( mKeyFieldList[i] == field_name_list[j] )
					{
						if( string.IsNullOrEmpty( id ) )
							id = data_row[j].ToString().Trim();
						else
							id += "_" + data_row[j].ToString().Trim();
					}
				}
			}

			if( string.IsNullOrEmpty(id) == false )
			{
				for(int i=0; i<mI18NTextFieldList.Count; i++)
				{
					for(int j=0; j<field_name_list.Count; j++)
					{
						if( mI18NTextFieldList[i] == field_name_list[j] )
						{
							if( IsExists(mI18NTextFieldList[i], id) == false )
							{
								TextFieldData textData = new TextFieldData();
								textData.fieldName = mI18NTextFieldList[i];
								textData.id = id;
								textData.text = XMLUtil.ConvertXmlTextPreDefined( data_row[j], false, ( trim_ignore == false ) );
								textData.translate_flags = I18NTextConst.eTranslateFlag.None;

								if( textData.text.StartsWith( I18NText_IGNORE_START ) )
									textData.translate_flags |= I18NTextConst.eTranslateFlag.IgnoreTranslate;

								mTextList.Add( textData );
							}
						}
					}
				}
			}
		}

		bool IsExists(string fieldname, string id)
		{
			foreach(TextFieldData data in mTextList)
			{
				if( data.fieldName == fieldname && data.id == id )
					return true;
			}

			return false;
		}

		public bool IsI18NTextField(string field)
		{
			foreach(string fieldName in mI18NTextFieldList)
			{
				if( fieldName == field )
					return true;
			}
			return false;
		}

		public void Export(string export_path, string filename, string time, int version, string encrypt_key, List<string> ignore_translate_categorys)
		{
			string default_language_text_path = Path.Combine( export_path, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_LANGUAGES, FormTBLExport.KOREAN_LANGUAGE );
			if( Directory.Exists( default_language_text_path ) == false )
				Directory.CreateDirectory( default_language_text_path );

			string exportFile = Path.Combine( default_language_text_path, filename + "_Text_" + FormTBLExport.KOREAN_LANGUAGE + I18NTextConst.EXTENSION_XML );

			XmlDocument doc = new XmlDocument();
			XmlNode rootNode = doc.AppendChild( doc.CreateElement( "DataList" ) );
			XMLUtil.AddAttribute( rootNode, "version", 0 );
			XMLUtil.AddAttribute( rootNode, "tbl_version", version );
			XMLUtil.AddAttribute( rootNode, "data_id", "ClientText" );
			XMLUtil.AddAttribute( rootNode, "language", FormTBLExport.KOREAN_LANGUAGE );
			XMLUtil.AddAttribute( rootNode, "created_time", time );

			Dictionary<string, XmlNode> nodeDic = new Dictionary<string, XmlNode>();

			if( CategorySort )
				mI18NTextFieldList = mI18NTextFieldList.OrderBy( a => a ).ToList();

			if( mI18NTextAddFieldList.Count > 0 )
			{
				foreach (string add_field in mI18NTextAddFieldList)
				{
					mI18NTextFieldList.Add( add_field );
				}
			}

			for(int i=0; i<mI18NTextFieldList.Count; i++)
			{
				XmlNode cat_node = rootNode.AppendChild( rootNode.OwnerDocument.CreateElement( mCategory + "_" + mI18NTextFieldList[i] ) );
				nodeDic.Add( mI18NTextFieldList[i], cat_node );

				if( ignore_translate_categorys != null && ignore_translate_categorys.Contains(mI18NTextFieldList[i]) )
				{
					XMLUtil.AddAttribute( cat_node, I18NTextConst.eTranslateFlag.IgnoreTranslate.ToString(), true );
				}
			}

			if( DataSort )
			{
				if( mTextList.Exists( t => t.IsNumericID() == false ) )
					mTextList = mTextList.OrderBy( t => t.fieldName ).ThenBy( t => t.id ).ToList();
				else
					mTextList = mTextList.OrderBy( t => t.fieldName ).ThenBy( t => int.Parse( t.id ) ).ToList();
			}

			foreach(TextFieldData textData in mTextList)
			{
				if( string.IsNullOrEmpty( textData.text ) == false )
				{
					XmlNode parentNode = nodeDic[textData.fieldName];
					XmlNode textNode = parentNode.AppendChild( parentNode.OwnerDocument.CreateElement( "Data" ) );
					XMLUtil.AddAttribute( textNode, "id", textData.id );

					if( textData.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) )
						XMLUtil.AddAttribute( textNode, I18NTextConst.eTranslateFlag.IgnoreTranslate.ToString(), true );

					textNode.InnerText = textData.text;
				}
			}

			XMLUtil.SaveXmlDocToFile( exportFile, doc );

// 			string i18ntextEncryptFolder = Path.Combine( export_path, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_ENCRYPT );
// 			if( Directory.Exists( i18ntextEncryptFolder ) == false )
// 				Directory.CreateDirectory( i18ntextEncryptFolder );
//
// 			string exportEncryptFile = Path.Combine( i18ntextEncryptFolder, filename + "_Text_Korean" + I18NTextConst.EXTENSION_XML_ENCRYPT );
// 			XMLUtil.SaveXmlDocToEncryptFile( exportEncryptFile, doc, encrypt_key );
		}

		//------------------------------------------------------------------------	
		public void I18NTextAdd(I18NTextAddData add_data)
		{
			string field_name = add_data.m_SheetName.Replace( I18NText_ADD_FIELD + "_", "" );

			foreach( string col_field_name in add_data.m_data.I18NTextFieldList)
			{
				string real_field_name = field_name + "_" + col_field_name;
				if( mI18NTextFieldList.Exists( f => f == real_field_name ) )
					throw new System.Exception( string.Format( "I18NTextAdd same field found:{0} - {1}", add_data.m_SheetName, real_field_name ) );

				if( mI18NTextAddFieldList.Exists( f => f == real_field_name ) )
					throw new System.Exception( string.Format( "I18NTextAdd already added field found:{0} - {1}", add_data.m_SheetName, real_field_name ) );

				mI18NTextAddFieldList.Add( real_field_name );
			}


			foreach(TextFieldData text_data in add_data.m_data.TextList )
			{
				text_data.fieldName = field_name + "_" + text_data.fieldName;
				mTextList.Add( text_data );
			}
		}
	}
}
