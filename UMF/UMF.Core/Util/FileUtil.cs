//////////////////////////////////////////////////////////////////////////
//
// FileUtil
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

using System.IO;

namespace UMF.Core
{
	public static class FileUtil
	{
		//------------------------------------------------------------------------		
		public static string ValidFileNameConvert( string src, char replace_char )
		{
			char[] symbols = System.IO.Path.GetInvalidFileNameChars();

			string ret_string = src;
			for( int i = 0; i < symbols.Length; i++ )
			{
				ret_string = ret_string.Replace( symbols[i], replace_char );
			}

			return ret_string;
		}

		//------------------------------------------------------------------------		
		public static string GetFullExtension( string filename )
		{
			int idx = filename.IndexOf( '.' );
			if( idx < 0 )
				return "";

			return filename.Substring( idx, filename.Length - idx );
		}

		//------------------------------------------------------------------------
		public static bool SafeDeleteFile( string file_path )
		{
			bool is_success = false;
			try
			{
				if( File.Exists( file_path ) )
				{
					is_success = true;
					File.Delete( file_path );
				}
			}
			catch( System.Exception ex )
			{
				is_success = false;
				Log.WriteWarning( ex.ToString() );
			}

			return is_success;
		}

		//------------------------------------------------------------------------
		static void _SafeDeleteDirectorySub( DirectoryInfo dir_info, bool logging )
		{
			if( dir_info.GetFiles().Length > 0 )
				return;

			DirectoryInfo[] sub_dirs = dir_info.GetDirectories();
			foreach( DirectoryInfo sub in sub_dirs )
			{
				_SafeDeleteDirectorySub( sub, logging );
			}

			if( logging )
				Log.Write( "[DD] {0}", dir_info.FullName );
			dir_info.Delete();
		}
		public static bool SafeDeleteDirectory( string dir, bool include_subdir, bool include_files, bool logging = false )
		{
			bool is_success = false;
			try
			{
				if( Directory.Exists( dir ) )
				{
					DirectoryInfo dir_info = new DirectoryInfo( dir );
					if( include_subdir && include_files )
					{
						if( logging )
							Log.Write( "[DA] {0}", dir_info.FullName );

						dir_info.Delete( true );
					}
					else if( include_subdir )
					{
						_SafeDeleteDirectorySub( dir_info, logging );
					}
					else
					{
						FileInfo[] file_infos = dir_info.GetFiles();
						if( file_infos.Length > 0 )
						{
							if( include_files )
							{
								foreach( FileInfo file in dir_info.GetFiles() )
								{
									if( logging )
										Log.Write( "[DF] {0}", file.FullName );
									file.Delete();
								}
							}
						}
						else
						{
							if( logging )
								Log.Write( "[DD] {0}", dir_info.FullName );

							dir_info.Delete();
						}
					}

					is_success = true;
				}
			}
			catch( System.Exception ex )
			{
				is_success = false;
				Log.Write( ex.ToString() );
			}

			return is_success;
		}
	}
}
