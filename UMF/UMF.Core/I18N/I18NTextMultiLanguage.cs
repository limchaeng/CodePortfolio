//////////////////////////////////////////////////////////////////////////
//
// I18NTextMultiLanguage
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
using System.IO;
using System;
using System.Runtime.Serialization;
using System.Globalization;

namespace UMF.Core.I18N
{
	public class I18NTextMultiLanguage : I18NTextDataBase<I18NTextMultiLanguage>
	{
		public static string DEFAULT_LANGUAGE = "Korean";
		public static string DEFAULT_CULTURE = "ko-KR";
		public static List<string> SUPPORT_LANGUAGES = new List<string>() { DEFAULT_LANGUAGE };

		Dictionary<string, CultureInfo> mCultureInfo = new Dictionary<string, CultureInfo>();
		protected override string LANGUAGE_DEFAULT { get { return DEFAULT_LANGUAGE; } }

		public override string RELOAD_DATA_ID => "I18NText";

		//------------------------------------------------------------------------		
		public override string ReloadData()
		{
			ClearTexts();

			string base_path = GlobalConfig.I18NPath( "" );
			if( string.IsNullOrEmpty( STATIC_PATH ) == false )
				base_path = STATIC_PATH;

			if( Directory.Exists( base_path ) )
			{
				string[] i18n_files = Directory.GetFiles( base_path );
				if( i18n_files != null )
				{
					foreach( string i18n in i18n_files )
					{
						string file_name = Path.GetFileName( i18n );
						if( SUPPORT_LANGUAGES.Exists( a => file_name.Contains( a ) ) == false )
							continue;

						string i18n_file = GlobalConfig.I18NPath( file_name );
						XmlDocument doc = new XmlDocument();
						doc.Load( i18n_file );
						_LoadData( doc );
					}
				}
			}

			return base_path;
		}

		//------------------------------------------------------------------------		
		void _LoadData( XmlDocument doc )
		{
			XmlNode stringDataNode = doc.SelectSingleNode( "DataList" );
			string language = XMLUtil.ParseAttribute<string>( stringDataNode, "language", "" );

			if( mCultureInfo.ContainsKey( language ) == false )
			{
				string culture_code = XMLUtil.ParseAttribute<string>( stringDataNode, "culture", "" );
				if( string.IsNullOrEmpty( culture_code ) == false )
				{
					mCultureInfo.Add( language, CultureInfo.CreateSpecificCulture( culture_code ) );
				}
			}

			AppendTextData( stringDataNode, language, false );
		}

		//------------------------------------------------------------------------
		public CultureInfo GetCultureInfo( string language )
		{
			CultureInfo ret_info;
			if( mCultureInfo.TryGetValue( language, out ret_info ) == false )
				ret_info = CultureInfo.CreateSpecificCulture( DEFAULT_CULTURE );

			return ret_info;
		}

		//------------------------------------------------------------------------		
		public string GetTextWithCulture( string language, Enum enum_value, params object[] parms )
		{
			return GetTextWithCulture( language, I18NTextConst.EnumToKey( enum_value ), parms );
		}
		public string GetTextWithCulture( string language, string key, params object[] parms )
		{
			CultureInfo culture;
			if( mCultureInfo.TryGetValue( language, out culture ) )
				return GetTextBaseWithCultureFormat( language, key, culture, parms );
			else
				return GetTextBaseWithCultureFormat( language, key, null, parms );
		}

		//------------------------------------------------------------------------	
		// TextKeyParams JSON for Global
		public const string TKP_JSON_PREFIX = "{\"T1\":\"";
		[DataContract]
		public class TKP_Data
		{
			[DataMember( Name = "T1" )]
			public string KeyValue = "";                // Text Key
			[DataMember( Name = "T2" )]
			public bool CultureFormat = false;              // With Culture Format
			[DataMember( Name = "V1" )]
			public string TypeName;             // value type name
			[DataMember( Name = "VL" )]
			public List<TKP_Data> SubDatas = null;  // Sub TextData

			public string ToJson()
			{
				return JsonUtil.EncodeJson( this );
			}
		}

		//------------------------------------------------------------------------	
		public string MakeTextKeyParamsCulture( string text_key, params object[] parms )
		{
			return MakeTextKeyParamsData( text_key, true, parms ).ToJson();
		}
		public string MakeTextKeyParams( string text_key, params object[] parms )
		{
			return MakeTextKeyParamsData( text_key, false, parms ).ToJson();
		}
		TKP_Data MakeTextKeyParamsData( string text_key, bool with_culture, object[] parms )
		{
			TKP_Data data = new TKP_Data();
			data.KeyValue = text_key;
			data.CultureFormat = with_culture;
			data.TypeName = "";
			data.SubDatas = null;
			if( parms != null && parms.Length > 0 )
			{
				data.SubDatas = new List<TKP_Data>();

				foreach( object o in parms )
				{
					if( o is TKP_Data )
					{
						data.SubDatas.Add( (TKP_Data)o );
					}
					else
					{
						TKP_Data v = new TKP_Data();
						v.KeyValue = o.ToString();
						v.CultureFormat = with_culture;
						v.TypeName = o.GetType().FullName;
						v.SubDatas = null;
						data.SubDatas.Add( v );
					}
				}
			}

			return data;
		}

		//------------------------------------------------------------------------	
		public string DeseriazeTextKeyParams( string language, string tkp_string )
		{
			try
			{
				TKP_Data data = JsonUtil.DecodeJson<TKP_Data>( tkp_string );
				if( data != null )
					return _ParseTextKeyParams( language, data );
			}
			catch( System.Exception ex )
			{
				Log.WriteError( ex.ToString() );
			}

			return GetTextBase( language, tkp_string );
		}

		//------------------------------------------------------------------------	
		string _ParseTextKeyParams( string language, TKP_Data data )
		{
			if( data.SubDatas != null && data.SubDatas.Count > 0 )
			{
				object[] parms = new object[data.SubDatas.Count];
				for( int i = 0; i < data.SubDatas.Count; i++ )
				{
					TKP_Data sub_data = data.SubDatas[i];
					if( sub_data.SubDatas != null && sub_data.SubDatas.Count > 0 )
					{
						parms[i] = _ParseTextKeyParams( language, sub_data );
					}
					else
					{
						if( sub_data.KeyValue.StartsWith( TKP_JSON_PREFIX ) )
						{
							parms[i] = DeseriazeTextKeyParams( language, sub_data.KeyValue );
						}
						else
						{
							object param_obj = null;
							if( string.IsNullOrEmpty( sub_data.TypeName ) == false )
							{
								try
								{
									System.Type param_type = Type.GetType( sub_data.TypeName, false );
									if( param_type != null )
									{
										param_obj = Convert.ChangeType( sub_data.KeyValue, param_type );
									}
								}
								catch( System.Exception ex )
								{
									param_obj = null;
								}
							}

							if( param_obj == null )
								param_obj = sub_data.KeyValue;

							parms[i] = param_obj;

							// check token language param
							if( param_obj.ToString().Contains( "|" ) )
							{
								string def_text = "";
								string[] loc_splits = param_obj.ToString().Split( new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries );
								if( loc_splits != null && loc_splits.Length > 1 )
								{
									for( int x = 0; x < loc_splits.Length; x += 2 )
									{
										if( loc_splits[x] == DEFAULT_LANGUAGE && loc_splits.Length > x + 1 )
											def_text = loc_splits[x + 1];

										if( loc_splits[x] == language && loc_splits.Length > x + 1 )
										{
											parms[i] = loc_splits[x + 1];
											def_text = "";
											break;
										}
									}

									if( string.IsNullOrEmpty( def_text ) == false )
										parms[i] = def_text;
								}
							}
						}
					}
				}

				if( data.CultureFormat )
					return GetTextWithCulture( language, data.KeyValue, parms );
				else
					return GetTextBase( language, data.KeyValue, parms );
			}
			else
			{
				return GetTextBase( language, data.KeyValue );
			}
		}

		//------------------------------------------------------------------------	
		public string GetText( string language, Enum enum_value, params object[] parms )
		{
			return GetText( language, I18NTextConst.EnumToKey( enum_value ), parms );
		}
		public string GetText( string language, string id, params object[] parms )
		{
			if( string.IsNullOrEmpty( id ) )
				return "";

			if( parms == null || parms.Length <= 0 )
			{
				if( id.StartsWith( TKP_JSON_PREFIX ) )
					return DeseriazeTextKeyParams( language, id );

				if( id.Contains( "|" ) )
				{
					string[] loc_splits = id.Split( new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries );
					if( loc_splits != null && loc_splits.Length > 1 )
					{
						string def_text = "";
						for( int i = 0; i < loc_splits.Length; i += 2 )
						{
							if( loc_splits[i] == DEFAULT_LANGUAGE && loc_splits.Length > i + 1 )
								def_text = loc_splits[i + 1];

							if( loc_splits[i] == language && loc_splits.Length > i + 1 )
								return loc_splits[i + 1];
						}

						if( string.IsNullOrEmpty( def_text ) == false )
							return def_text;
					}
				}

				return GetTextBase( language, id );
			}
			else
			{
				return GetTextBase( language, id, parms );
			}
		}
	}
}
