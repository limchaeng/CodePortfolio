//////////////////////////////////////////////////////////////////////////
//
// UMFEditorUtil
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
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Reflection;

namespace UMF.Unity.EditorUtil
{
	public static class UMFEditorUtil
	{
		//------------------------------------------------------------------------
		static GUIContent mTmpGUIContents = new GUIContent();
		public static GUIContent GetTempGUIContents( string text )
		{
			mTmpGUIContents.text = text;
			mTmpGUIContents.tooltip = text;
			mTmpGUIContents.image = null;

			return mTmpGUIContents;
		}


        //------------------------------------------------------------------------
        public static void LogToFile( string log, string file_path, string file_name, bool is_append )
		{
			try
			{
				if( Directory.Exists( file_path ) == false )
					Directory.CreateDirectory( file_path );

				string logfilename = file_path + "/" + file_name;
				using( FileStream fs = new FileStream( logfilename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read ) )
				{
					if( is_append == false )
					{
						fs.SetLength( 0 );
						fs.Flush();
					}

					using( StreamWriter sw = new StreamWriter( fs, System.Text.Encoding.UTF8 ) )
					{
						sw.WriteLine( log );
						sw.Flush();
						sw.Close();
					}
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning( ex.ToString() );
			}
		}

		//------------------------------------------------------------------------
		public static List<T> LoadAllAssetAtFolder<T>( string[] asset_folders, string custom_find_type = "" ) where T : UnityEngine.Object
		{
			if( asset_folders == null )
				return null;

			List<T> list = new List<T>();

			string find_type = typeof( T ).Name;
			if( string.IsNullOrEmpty( custom_find_type ) == false )
				find_type = custom_find_type;

			string[] guids = AssetDatabase.FindAssets( $"t:{find_type}", asset_folders );

			foreach( string guid in guids )
			{
				string asset_path = AssetDatabase.GUIDToAssetPath( guid );

				List<T> asset_list = AssetDatabase.LoadAllAssetsAtPath( asset_path ).OfType<T>().ToList();
				if( asset_list != null )
					list.AddRange( asset_list );
			}

			return list;
		}
        public static List<T> LoadAssetAtFolder<T>( string[] asset_folders, string custom_find_type = "" ) where T : UnityEngine.Object
        {
            if( asset_folders == null )
                return null;

            List<T> list = new List<T>();

            string find_type = typeof( T ).Name;
            if( string.IsNullOrEmpty( custom_find_type ) == false )
                find_type = custom_find_type;

            string[] guids = AssetDatabase.FindAssets( $"t:{find_type}", asset_folders );

            foreach( string guid in guids )
            {
                string asset_path = AssetDatabase.GUIDToAssetPath( guid );

				T asset = AssetDatabase.LoadAssetAtPath<T>( asset_path );
				if( asset != null )
					list.Add( asset );
            }

            return list;
        }


        //------------------------------------------------------------------------
        public class LoadAllAssetInfoData<T> where T : UnityEngine.Object
		{
			public string guid;
			public string asset_path;
			public string asset_name;
			// runtime
			public T asset = null;

			bool is_loaded = false;

			public void LoadAsset()
			{
				if( is_loaded )
					return;

				is_loaded = true;
				asset = AssetDatabase.LoadAllAssetsAtPath( asset_path ).OfType<T>().FirstOrDefault();
			}
		}
		public static List<LoadAllAssetInfoData<T>> LoadAllAssetInfoAtFolder<T>( string asset_folder, string custom_find_type = "" ) where T : UnityEngine.Object
		{
			if( string.IsNullOrEmpty( asset_folder ) )
				return null;

			List<LoadAllAssetInfoData<T>> list = new List<LoadAllAssetInfoData<T>>();

			string find_type = typeof( T ).Name;
			if( string.IsNullOrEmpty( custom_find_type ) == false )
				find_type = custom_find_type;

			string[] guids = AssetDatabase.FindAssets( $"t:{find_type}", new string[] { asset_folder } );
			foreach( string guid in guids )
			{
				LoadAllAssetInfoData<T> data = new LoadAllAssetInfoData<T>();
				data.guid = guid;
				data.asset_path = AssetDatabase.GUIDToAssetPath( guid );
				data.asset_name = Path.GetFileNameWithoutExtension( data.asset_path );
				data.asset = null;

				list.Add( data );
			}

			return list;
		}


		//------------------------------------------------------------------------		
		public static void CreateFolder( string folder )
		{
			if( Directory.Exists( folder ) == false )
			{
				Directory.CreateDirectory( folder );
				Debug.Log( $"Create Folder : {folder}" );
			}
			else
			{
				Debug.Log( $"Create Folder : exist {folder}" );
			}
		}

		//------------------------------------------------------------------------
		public static void OpenComponentSelection<T>( GameObject active_object ) where T : Component
		{
			List<Object> finded = new List<Object>();
			T[] type_select_list = active_object.GetComponentsInChildren<T>( true );

			if( type_select_list.Length > 0 )
			{
				foreach( T sel in type_select_list )
					finded.Add( sel.gameObject );
			}
			Debug.Log( string.Format( "Selected {0}:{1}", typeof( T ).ToString(), finded.Count ) );

			UMFSelectionWindow.Show( finded );
		}

		//------------------------------------------------------------------------	
		public static bool DeleteAllFiles( string src_dir, bool with_subfolder )
		{
			Debug.Log( $"# DeleteAllFiles(withsub={with_subfolder}) : {src_dir}" );

			if( Directory.Exists( src_dir ) == false )
				return false;

			int delete_file = 0;
			int delete_subfolder = 0;

			string[] files = Directory.GetFiles( src_dir );
			foreach( string file in files )
			{
				File.Delete( file );
				delete_file++;
			}

			if( with_subfolder )
			{
				string[] sub_dirs = Directory.GetDirectories( src_dir );

				foreach( string dir in sub_dirs )
				{
					Directory.Delete( dir, true );
					delete_subfolder++;
				}
			}

			Debug.Log( $"- delete files={delete_file} subfolder={delete_subfolder}" );

			return true;
		}

		//------------------------------------------------------------------------	
		public static bool CopyAllFiles( string src_dir, string dest_dir, bool meta_ignore, bool dest_folder_create, bool src_exception_throw = true, bool include_subfolders = false )
		{
			try
			{
				if( Directory.Exists( src_dir ) == false )
				{
					if( src_exception_throw )
					{
						throw new System.Exception( string.Format( "SrcDir({0}) Not Found", src_dir ) );
					}
					else
					{
						Debug.LogWarning( string.Format( "SrcDir({0}) Not Found", src_dir ) );
						return false;
					}

				}

				string[] files = Directory.GetFiles( src_dir );
				if( include_subfolders )
				{
					files = Directory.GetFiles( src_dir, "*.*", SearchOption.AllDirectories );
					dest_folder_create = true;
				}

				foreach( string file in files )
				{
					if( meta_ignore && Path.GetExtension( file ) == ".meta" )
						continue;

					if( include_subfolders == false )
					{
						FileAttributes fa = File.GetAttributes( file );
						if( ( fa & FileAttributes.Directory ) == FileAttributes.Directory )
							continue;
					}

					CopyFile( file, string.Format( "{0}/{1}", dest_dir, Path.GetFileName( file ) ), dest_folder_create, src_exception_throw );
				}

				return true;
			}
			catch( System.Exception ex )
			{
				string error = string.Format( "CopyAll Exception\n{0}\nCheck and Retry!", ex.ToString() );
				Debug.LogWarning( error );
				EditorUtility.DisplayDialog( "ERROR", error, "OK" );
				return false;

			}
		}

		//------------------------------------------------------------------------
		public static bool CopyFile( string src, string dest, bool dest_folder_create, bool copy_exception_throw = true )
		{
			try
			{
				if( File.Exists( src ) == false )
				{
					if( copy_exception_throw )
					{
						throw new System.Exception( string.Format( "Src({0}) Not Found", src ) );
					}
					else
					{
						Debug.LogWarning( string.Format( "Src({0}) Not Found", src ) );
						return false;
					}
				}

				string dest_folder = Path.GetDirectoryName( dest );
				if( Directory.Exists( dest_folder ) == false )
				{
					if( dest_folder_create )
					{
						Directory.CreateDirectory( dest_folder );
					}
					else
					{
						Debug.LogWarningFormat( "Dest folder:{0} not found!", dest );
						return false;
					}
				}

				File.Copy( src, dest, true );
				Debug.Log( string.Format( "** ({0} -> {1}) Copy Success!", src, dest ) );
				return true;
			}
			catch( System.Exception ex )
			{
				string error = string.Format( "Copy Exception\n{0}\nCheck and Retry!", ex.ToString() );
				Debug.LogWarning( error );
				EditorUtility.DisplayDialog( "ERROR", error, "OK" );

				if( copy_exception_throw )
					throw new System.Exception( error );
				else
					return false;
			}
		}

		//------------------------------------------------------------------------
		// https://forum.unity.com/threads/way-to-play-audio-in-editor-using-an-editor-script.132042/#post-4767824
		public static void PlayAudioClip( AudioClip clip, int startSample = 0, bool loop = false )
		{
			Assembly editor_assem = typeof( AudioImporter ).Assembly;
			System.Type audio_util_type = editor_assem.GetType( "UnityEditor.AudioUtil" );
			MethodInfo method = audio_util_type.GetMethod( "PlayPreviewClip", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof( AudioClip ), typeof( int ), typeof( bool ) }, null );
			if( method != null )
				method.Invoke( null, new object[] { clip, startSample, loop} );
		}
	}
}