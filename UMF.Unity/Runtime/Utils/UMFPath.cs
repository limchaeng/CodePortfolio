//////////////////////////////////////////////////////////////////////////
//
// PBX_Path
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
using System.IO;
using UnityEngine;

namespace UMF.Unity
{
	public static class UMFPath
	{
		public static string CUSTOM_DOC_PATH = "";
		public static string CUSTOM_TEMP_PATH = "";

		public const string DOC_DEFAULT_PATH = "_documents_";
		public const string TEMP_DEFAULT_PATH = "_temporaryCachePath_";

		public const char WinSeparator = '\\';
		public const char UnixSeparator = '/';

		//------------------------------------------------------------------------
		public static string NormalizeSlashPath( this string path )
		{
			return path.Replace( WinSeparator, UnixSeparator );
		}

		//-----------------------------------------------------------------------------
		public static string GetDocumentsFilePath()
		{
			return GetDocumentsFilePath( "" );
		}
		public static string GetDocumentsFilePath( string fileName )
		{
			if( string.IsNullOrEmpty( CUSTOM_DOC_PATH ) == false && Directory.Exists( CUSTOM_DOC_PATH ) )
				return Path.Combine( CUSTOM_DOC_PATH, fileName ).NormalizeSlashPath();

			if( Application.isEditor )
			{
				string datapath = "Assets";
				string path = Application.dataPath.Substring( 0, Application.dataPath.Length - datapath.Length );
				return Path.Combine( path, DOC_DEFAULT_PATH, fileName ).NormalizeSlashPath();
			}
			else
			{
				switch( Application.platform )
				{
					case RuntimePlatform.IPhonePlayer:
					case RuntimePlatform.WSAPlayerARM:
					case RuntimePlatform.WSAPlayerX64:
					case RuntimePlatform.WSAPlayerX86:
						return Path.Combine( Application.persistentDataPath, DOC_DEFAULT_PATH , fileName ).NormalizeSlashPath();

					case RuntimePlatform.WindowsPlayer:
						return Path.Combine( GetApplicationPath( DOC_DEFAULT_PATH ), fileName ).NormalizeSlashPath();
				}
			}

			return Path.Combine( Application.persistentDataPath, fileName ).NormalizeSlashPath();
		}

		//-----------------------------------------------------------------------------
		public static string GetTemporaryCachedPath()
		{
			return GetTemporaryCachedPath( "" );
		}
		public static string GetTemporaryCachedPath( string filename )
		{
			if( string.IsNullOrEmpty( CUSTOM_TEMP_PATH ) == false && Directory.Exists( CUSTOM_TEMP_PATH ) )
				return Path.Combine( CUSTOM_TEMP_PATH, filename ).NormalizeSlashPath();

			if( Application.isEditor )
			{
				string datapath = "Assets";
				string path = Application.dataPath.Substring( 0, Application.dataPath.Length - datapath.Length );
				return Path.Combine( path, TEMP_DEFAULT_PATH, filename ).NormalizeSlashPath();
			}
			else
			{
				switch( Application.platform )
				{
					case RuntimePlatform.WSAPlayerARM:
					case RuntimePlatform.WSAPlayerX64:
					case RuntimePlatform.WSAPlayerX86:
						return Path.Combine( Application.persistentDataPath, TEMP_DEFAULT_PATH, filename ).NormalizeSlashPath();

					case RuntimePlatform.WindowsPlayer:
						return Path.Combine( GetApplicationPath( TEMP_DEFAULT_PATH ), filename ).NormalizeSlashPath();
				}
			}

			return Path.Combine( Application.temporaryCachePath, filename ).NormalizeSlashPath();
		}

		//-----------------------------------------------------------------------------
		public static string GetStreamingAssetsPath()
		{
			return Application.streamingAssetsPath;
		}

		//-----------------------------------------------------------------------------
		public static string GetApplicationPath()
		{
			if( Application.platform == RuntimePlatform.OSXPlayer )
				return Application.dataPath + "/../";
			else if( Application.platform == RuntimePlatform.WindowsPlayer )
				return Application.dataPath + "/../";

			return Application.dataPath;
		}

		//-----------------------------------------------------------------------------
		public static string GetApplicationPath( string filename )
		{
			return Path.Combine( GetApplicationPath(), filename ).NormalizeSlashPath();
		}

		//-----------------------------------------------------------------------------
		public static string GetStreamingAssetsPath( string filename )
		{
			return Path.Combine( Application.streamingAssetsPath, filename ).NormalizeSlashPath();
		}
		public static string GetOBBAssetPath( string obb_main_path, string file_name )
		{
			// jar:file:///data/app/[bundle id].apk!/assets  : read only www

			return string.Format( "jar:file:///{0}!/assets/{1}", obb_main_path, file_name );
		}

		//------------------------------------------------------------------------
		// https://docs.unity3d.com/Manual/LogFiles.html
		// %APPDATA% = C:\Users\Username\AppData\Roaming
		// %LOCALAPPDATA% = C:\Users\Username\AppData\Local\Packages\[family id]\AC ??
		public static string GetUnityLogFilePath()
		{
			string env_path = "";
			string logFilePath = "";

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WSA
			env_path = System.Environment.GetEnvironmentVariable( "LOCALAPPDATA" );
			string profile_path = System.Environment.GetEnvironmentVariable( "USERPROFILE" );
			logFilePath = $"{profile_path}/AppData/LocalLow/{Application.companyName}/{Application.productName}/Player.Log";
			#if UNITY_WSA
			logFilePath = $"{env_path}/../TempState/UnityPlayer.log";
			#endif
			#if UNITY_EDITOR
			logFilePath = $"{env_path}/Unity/Editor/Editor.Log";
			#endif
#endif
			return logFilePath;
		}

		//------------------------------------------------------------------------
		public static string ConvertValidFilePath( string input, string replace )
		{
			return string.Join( replace, input.Split( Path.GetInvalidFileNameChars() ) );
		}

		//------------------------------------------------------------------------
		public static void OpenFile( string _filepath, bool with_folder = false )
		{
			string filepath = _filepath.Replace( " ", "%20" );

			switch( Application.platform )
			{
				case RuntimePlatform.LinuxEditor:
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.LinuxPlayer:
				case RuntimePlatform.WSAPlayerX64:
				case RuntimePlatform.WSAPlayerX86:
					{
						string open_url_prefix = "file://";
						if( Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor )
							open_url_prefix = "file:///"; // Windows requires an extra / before the filename

						if( File.Exists( _filepath ) )
							Application.OpenURL( open_url_prefix + filepath );
						else
							Debug.Log( $"- UMFPath:OpenURL not found file:{_filepath}" );

						if( with_folder )
						{
							string dir_path = Path.GetDirectoryName( filepath );
							if( Directory.Exists( dir_path ) )
								Application.OpenURL( open_url_prefix + dir_path );
							else
								Debug.Log( $"- UMFPath:OpenURL not found dir:{dir_path}" );
						}
					}
					break;

				default:
					Debug.LogWarning( $"UMFPath:OpenURL not supported platform : {Application.platform}" );
					break;
			}
		}
	}
}
