//////////////////////////////////////////////////////////////////////////
//
// StringUtil
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
using System.Linq;
using System.IO;
using System.IO.Compression;
using System;
using System.Text;

namespace UMF.Core
{
	public static class StringUtil
	{
		//------------------------------------------------------------------------	
		public static object SafeParse( string text, Type parse_type, object defaultValue )
		{
			if( string.IsNullOrEmpty( text ) == true )
				return defaultValue;

			try
			{
				if( parse_type.IsEnum )
				{
					string parse_text = text;
					if( text.Contains( "|" ) )
						parse_text = text.Replace( '|', ',' );

					return Enum.Parse( parse_type, parse_text, true );
				}

				if( parse_type == typeof( Version ) )
					return Version.Parse( text );

				return Convert.ChangeType( text, parse_type );
			}
			catch( Exception ex )
			{
				Log.WriteWarning( "SafeParse.Exception:{0}[{1}] def:{2} ex:{3}", text, parse_type, defaultValue?.ToString(), ex.ToString() );
			}

			return defaultValue;
		}

		//------------------------------------------------------------------------	
		public static ParseType SafeParse<ParseType>( string text, ParseType defaultValue )
		{
			if( string.IsNullOrEmpty( text ) == true )
				return defaultValue;

			Type parse_type = typeof( ParseType );
			return (ParseType)SafeParse( text, parse_type, defaultValue );
		}

		//------------------------------------------------------------------------	
		public static List<T> SafeParseToList<T>( string parse_value, params char[] separator )
		{
			if( string.IsNullOrEmpty( parse_value ) )
				return null;

			try
			{
				if( typeof( T ).IsEnum )
				{
					return parse_value.Split( separator ).Select( s => (T)Enum.Parse( typeof( T ), s ) ).ToList();
				}
				else
				{
					return parse_value.Split( separator ).Select( s => (T)Convert.ChangeType( s, typeof( T ) ) ).ToList();
				}
			}
			catch( Exception ex )
			{
				Log.WriteWarning( "StringUtil.Exception:{0}[{1}]", parse_value, typeof( T ).GetType() );
			}

			return null;
		}

		//------------------------------------------------------------------------	
		public static string CompressedBase64String( string xml_str, bool use_clzf )
		{
			string ret_string = "";

			if( use_clzf )
			{
				try
				{
					byte[] text_bytes = Encoding.UTF8.GetBytes( xml_str );
					byte[] compressed = CLZF2.Compress( text_bytes );

					//Log.Write( "CompressedBase64String src:{0} compressed:{1}", text_bytes.Length, compressed.Length );

					ret_string = Convert.ToBase64String( compressed );
				}
				catch( Exception ex )
				{
					Log.WriteWarning( ex.ToString() );
				}
			}
			else
			{
				MemoryStream stream = null;
				try
				{
					byte[] buffer = Encoding.UTF8.GetBytes( xml_str );

					stream = new MemoryStream();
					using( GZipStream gzip = new GZipStream( stream, CompressionMode.Compress, true ) )
					{
						gzip.Write( buffer, 0, buffer.Length );
					}

					stream.Position = 0;

					byte[] compressedData = new byte[stream.Length];
					stream.Read( compressedData, 0, compressedData.Length );

					ret_string = Convert.ToBase64String( compressedData );
				}
				catch( Exception ex )
				{
					Log.WriteWarning( ex.ToString() );
				}
				finally
				{
					if( stream != null )
						stream.Close();
					stream = null;
				}
			}

			return ret_string;
		}

		//------------------------------------------------------------------------
		public static List<string> StringSplit( string in_string, int len )
		{
			List<string> list = new List<string>();

			int size_max = len;
			int loop_count = 1;
			if( in_string.Length >= size_max )
			{
				loop_count = ( in_string.Length / size_max ) + 1;
			}
			int offset = 0;
			int size = Math.Min( in_string.Length, 25000 );
			for( int i = 0; i < loop_count; i++ )
			{
				list.Add( in_string.Substring( offset, size ) );

				offset += size;
				size = Math.Min( in_string.Length - offset, size_max );
			}

			return list;
		}
	}
}
