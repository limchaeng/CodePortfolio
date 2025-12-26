//////////////////////////////////////////////////////////////////////////
//
// I18NTextManagerUnity
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
using UMF.Core;
using UnityEngine;
using UMF.Core.I18N;
using System.IO;
using System.Xml;

namespace UMF.Unity
{
	public interface II18NTextManagerSetting
	{
		string DOWNLOAD_FILE_PATH { get; }
		string ENCRYPT_KEY { get; }
		List<string> CODE_DATA_FILES { get; }
		List<string> CODE_CATEGORYS { get; }
		string GET_FALLBACK_FONT_LANGUAGE( string curr_language );
		bool IsWordWrappingSizeCheckLanguage( string language );
	}

	public interface II18NTextFontSetting
	{
		bool II18NTextFontSetting_ChangeLanguage( string language );
	}

	public class I18NTextManagerUnity : Singleton<I18NTextManagerUnity>
	{
		private II18NTextManagerSetting mSetting = null;

		private II18NTextFontSetting mFontSetting = null;
		public II18NTextFontSetting FontSetting { get { return mFontSetting; } }

		System.Action<string, bool> mRefreshBroadcastingCallback = null;

		private bool mInitialized = false;
		public bool Initialized { get { return mInitialized; } }

		// cache
		bool mWordWrappingSizeCheckCurrentLanguage = false;
		public bool IsWordWrappingSizeCheckCurrentLanguage { get { return mWordWrappingSizeCheckCurrentLanguage; } }

		public void Init( string init_language, II18NTextManagerSetting setting, II18NTextFontSetting font_setting, System.Action<string, bool> refresh_broadacasting_callback )
		{
			if( Application.isEditor && Application.isPlaying == false && mInitialized )
				return;

			mInitialized = true;

			mSetting = setting;
			mFontSetting = font_setting;
			mRefreshBroadcastingCallback = refresh_broadacasting_callback;

			Reload( init_language, false, true );
		}

#if UNITY_EDITOR || UMDEV
		public bool DEV_LANGUAGE_ALL_LOAD = false;
		public bool DEV_TEXT_LONG_TEST = false;

		public string GetLanguageTextUILongTest( System.Enum enum_id, params object[] parms )
		{
			return GetLanguageTextUILongTest( I18NTextConst.EnumToKey( enum_id ), parms );
		}
		public string GetLanguageTextUILongTest( string id, params object[] parms )
		{
			I18NTextSingleLanguage _instance = I18NTextSingleLanguage.Instance;

			if( DEV_TEXT_LONG_TEST )
			{
				string long_string = "";
				if( parms != null )
					long_string = _instance.GetText( id, parms );
				else
					long_string = _instance.GetText( id );

				if( IsCodeID( id ) )
					return long_string;

				List<I18NTextLanguageInfo> language_list = _instance.LanguageInfoList;

				foreach( I18NTextLanguageInfo l_info in language_list )
				{
					if( l_info.language == _instance.CurrLanguage )
						continue;

					string other_string = _instance.GetTextBase( l_info.language, id, parms );
					if( other_string != id )
					{
						if( other_string.Length > long_string.Length )
							long_string = other_string;
					}
				}

				return long_string;
			}
			else
			{
				if( parms != null )
					return _instance.GetText( id, parms );
				else
					return _instance.GetText( id );
			}
		}
#endif

		public bool HasFastUpdate { get; set; } = false;

		//------------------------------------------------------------------------
		public bool IsCodeID( System.Enum enum_value )
		{
			return IsCodeID( I18NTextConst.EnumToKey( enum_value ) );
		}
		public bool IsCodeID( string id )
		{
			if( mSetting == null )
				return false;

			return mSetting.CODE_CATEGORYS.Exists( a => id.Contains( a ) );
		}

		//------------------------------------------------------------------------
		public void ReloadText()
		{
			Reload( CurrentLanguage(), true );
		}
		public void Reload( string language, bool text_only = false, bool from_init = false )
		{
			Debug.Log( $"======= I18NTextManager Reload - {language}" );

			I18NTextSingleLanguage _instance = I18NTextSingleLanguage.Instance;

			HasFastUpdate = false;
			_instance.ClearTexts();
			_instance.SetLanguage( language );

			mWordWrappingSizeCheckCurrentLanguage = mSetting.IsWordWrappingSizeCheckLanguage( language );

//#if UNITY_EDITOR || UMDEV
//			DEV_LANGUAGE_ALL_LOAD = Application.isEditor;
//#endif

			// font load
			if( text_only == false )
			{
				if( Application.isEditor == false || Application.isPlaying )
				{
					if( mFontSetting != null )
					{
						if( mFontSetting.II18NTextFontSetting_ChangeLanguage( language ) == false )
						{
							// check fallback font
							mFontSetting.II18NTextFontSetting_ChangeLanguage( mSetting.GET_FALLBACK_FONT_LANGUAGE( language ) );
						}
					}
				}
			}

			List<TextAsset> text_asset_list = new List<TextAsset>();

#if UNITY_EDITOR || UMDEV
			Load_DEV( language );
#else
			Load_Resource( language );
#endif

			// download data check
#if !UNITY_WEBGL
			if( mSetting != null && Directory.Exists( mSetting.DOWNLOAD_FILE_PATH ) )
			{
				string[] existFiles = Directory.GetFiles( mSetting.DOWNLOAD_FILE_PATH, "*.bytes" );
				foreach( string filepath in existFiles )
				{
					string data_id = Path.GetFileNameWithoutExtension( filepath ).Replace( I18NTextConst.EXTENSION_XML_ENCRYPT, "" ).Replace( ".bytes", "" );

					bool is_code = mSetting.CODE_DATA_FILES.Exists( a => data_id.Contains( a ) );
					if( data_id.EndsWith( language ) || is_code )
					{
						bool is_valid = false;

						string xmlStr = XMLUtil.LoadXmlFromEncryptFile( filepath, mSetting.ENCRYPT_KEY );
						if( string.IsNullOrEmpty( xmlStr ) == false )
						{
							XmlDocument doc = XMLUtil.SafeLoad( xmlStr );
							if( doc != null )
							{
								string _data_id = "";
								int _version = -1;
								if( ParseDataInfo( doc, ref _data_id, ref _version ) )
								{
									I18NTextDataInfo curr_info = _instance.FindDataInfo( language, _data_id );
									if( curr_info == null || curr_info.version < _version )
									{
										Debug.Log( "I18NText Load:Downloaded:" + Path.GetFileName( filepath ) );
										is_valid = _instance.Load( doc, language );
									}
								}
							}
						}

						if( is_valid == false )
						{
							FileUtil.SafeDeleteFile( filepath );
							Debug.LogFormat( "I18NText delete downloaded invalid:{0}", filepath );
						}
					}
				}
			}
#endif
			OnRefresh( from_init );
		}

		//------------------------------------------------------------------------	
		bool ParseDataInfo( XmlDocument doc, ref string data_id, ref int version )
		{
			XmlNode datalistNode = doc.SelectSingleNode( "DataList" );
			if( datalistNode == null )
				return false;

			data_id = XMLUtil.ParseAttribute<string>( datalistNode, "data_id", "" );
			version = XMLUtil.ParseAttribute<int>( datalistNode, "version", -1 );

			return true;
		}

		//------------------------------------------------------------------------
#if UNITY_EDITOR || UMDEV
		protected void Load_DEV( string language )
		{
			I18NTextSingleLanguage _instance = I18NTextSingleLanguage.Instance;

			string data_path = GlobalConfig.I18NPath( "" );
			string code_path = Path.Combine( data_path, I18NTextConst.DEFAULT_PATH_CODE ).NormalizeSlashPath();

			bool use_custom_export_path = false;
			if( string.IsNullOrEmpty( TBLManagerUnity.Instance.CUSTOM_TBL_EXPORT_PATH ) == false )
			{
				string i18ntext_path = $"{TBLManagerUnity.Instance.CUSTOM_TBL_EXPORT_PATH}/{I18NTextConst.DEFAULT_PATH_ROOT}";
				if( Directory.Exists( i18ntext_path ) )
				{
					use_custom_export_path = true;
					data_path = i18ntext_path;
					code_path = Path.Combine( data_path, I18NTextConst.DEFAULT_PATH_CODE ).NormalizeSlashPath();
				}
			}

			List<string> ed_data_path_list = new List<string>();
			if( Directory.Exists( code_path ) )
			{
				string[] code_files = Directory.GetFiles( code_path );
				foreach( string code in code_files )
				{
					if( Path.GetExtension( code ) != I18NTextConst.EXTENSION_XML )
						continue;

					ed_data_path_list.Add( code );
				}
			}

			string language_path = Path.Combine( data_path, I18NTextConst.DEFAULT_PATH_LANGUAGES, language ).NormalizeSlashPath();
			if( Directory.Exists( language_path ) )
			{
				string[] language_filess = Directory.GetFiles( language_path );
				foreach( string file in language_filess )
				{
					if( Path.GetExtension( file ) != I18NTextConst.EXTENSION_XML )
						continue;

					ed_data_path_list.Add( file );
				}
			}

			if( DEV_LANGUAGE_ALL_LOAD )
			{
				string lan_path = Path.Combine( data_path, I18NTextConst.DEFAULT_PATH_LANGUAGES );
				if( Directory.Exists( lan_path ) )
				{
					string[] language_dirs = Directory.GetDirectories( lan_path );
					foreach( string dir_language in language_dirs )
					{
						string dir_name = Path.GetFileName( dir_language );
						if( dir_name == language || dir_name == I18NTextConst.DEFAULT_PATH_CODE )
							continue;

						string[] language_filess = Directory.GetFiles( dir_language );
						foreach( string file in language_filess )
						{
							if( Path.GetExtension( file ) != I18NTextConst.EXTENSION_XML )
								continue;

							ed_data_path_list.Add( file );
						}
					}
				}
			}

			foreach( string ed_path in ed_data_path_list )
			{
				int load_success = 0;

				if( use_custom_export_path == false )
				{
#if UNITY_EDITOR
					TextAsset ta = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>( ed_path );
					if( ta != null )
					{
						Debug.LogFormat( "I18NText Load:Assets:{0}", ed_path );

						if( ta.name.Contains( language ) == false )
						{
							if( _instance.Load( ta.bytes, language, true ) == false )
								load_success = 1;
						}
						else
						{
							if( _instance.Load( ta.bytes, language ) == false )
								load_success = 2;
						}

						Resources.UnloadAsset( ta );
					}
#else
					load_success = 0;
#endif
				}
				else
				{
					string _xml = XMLUtil.LoadXmlFromFile( ed_path );
					if( string.IsNullOrEmpty( _xml ) == false )
					{
						XmlDocument doc = XMLUtil.SafeLoad( _xml );
						if( doc != null )
						{
							Debug.LogFormat( "I18NText Load:CustomPath:{0}", ed_path );

							if( Path.GetFileName( ed_path ).Contains( language ) == false )
							{
								if( _instance.Load( doc, language, true ) == false )
									load_success = 3;
							}
							else
							{
								if( _instance.Load( doc, language ) == false )
									load_success = 4;
							}
						}
					}
				}

				if( load_success != 0 )
					Debug.LogWarning( $"I18NTextManagerUnity cannot load(code:{load_success}) : {ed_path}" );
			}

			if( use_custom_export_path == false )
				Load_Resource( language );
		}
#endif

		//------------------------------------------------------------------------
		void Load_Resource( string language )
		{
			string data_path = GlobalConfig.I18NPath( "" );
			string code_path = Path.Combine( data_path, I18NTextConst.DEFAULT_PATH_CODE ).NormalizeSlashPath();

			I18NTextSingleLanguage _instance = I18NTextSingleLanguage.Instance;

			TextAsset[] code_resources = Resources.LoadAll<TextAsset>( code_path );
			foreach( TextAsset ta in code_resources )
			{
				Debug.LogFormat( "I18NText Load:Resource:{0}", ta.name );

				_instance.Load( ta.bytes, language );
				Resources.UnloadAsset( ta );
			}

			string lan_path = Path.Combine( data_path, I18NTextConst.DEFAULT_PATH_LANGUAGES, language ).NormalizeSlashPath();
			TextAsset[] data_resources = Resources.LoadAll<TextAsset>( lan_path );
			foreach( TextAsset ta in data_resources )
			{
				Debug.LogFormat( "I18NText Load:Resource:{0}", ta.name );

				_instance.Load( ta.bytes, language );
				Resources.UnloadAsset( ta );
			}
		}

		//------------------------------------------------------------------------
		void OnRefresh( bool from_init )
		{
			if( mRefreshBroadcastingCallback != null )
				mRefreshBroadcastingCallback( "OnI18NTextUpdateBroadcast", from_init );
		}

		//------------------------------------------------------------------------	
		public static string FindLanguageRegionCode( string default_code )
		{
			if( default_code == null || default_code.Length != 5 )
				return default_code;

			string first_code_2 = default_code.Substring( 0, 2 );
			System.Globalization.CultureInfo curr_culture = System.Globalization.CultureInfo.CurrentCulture;
			if( curr_culture == null )
				return default_code;

			string culture_name = curr_culture.Name;
			if( culture_name.Length > 2 && culture_name.Substring( 0, 2 ).ToLower() == default_code.Substring( 0, 2 ).ToLower() )
				return culture_name;

			return default_code;
		}

		//------------------------------------------------------------------------	
		public static string GetText( System.Enum enum_value, params object[] parms )
		{
			return GetText( I18NTextConst.EnumToKey( enum_value ), parms );
		}
		public static string GetText( string id, params object[] parms )
		{
			return GetTextCulture( id, false, parms );
		}
		public static string GetTextCulture( string id, bool with_culture_format, params object[] parms )
		{
			string txt = "";
#if UNITY_EDITOR || UMDEV
			if( I18NTextManagerUnity.Instance.DEV_TEXT_LONG_TEST )
			{
				txt = I18NTextManagerUnity.Instance.GetLanguageTextUILongTest( id, parms );
				return TextReplaceUtil.GetTextReplace( txt );
			}
#endif
			if( Application.isEditor == false )
			{
				// exception 이 나서 깨지지 않게 param 체크 로직 추가
				// editor 에서는 에러가 나야 함.
			}

			if( with_culture_format )
				txt = I18NTextSingleLanguage.Instance.GetTextWithCulture( id, parms );
			else
				txt = I18NTextSingleLanguage.Instance.GetText( id, parms );

			return TextReplaceUtil.GetTextReplace( txt );
		}

		//------------------------------------------------------------------------		
		public static string CurrentLanguage()
		{
			return I18NTextSingleLanguage.Instance.CurrLanguage;
		}

		//------------------------------------------------------------------------
		public static bool ContainText( System.Enum enum_value )
		{
			return I18NTextSingleLanguage.Instance.Contains( enum_value );
		}
		public static bool ContainText( string id )
		{
			return I18NTextSingleLanguage.Instance.Contains( id );
		}
	}
}
