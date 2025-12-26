//////////////////////////////////////////////////////////////////////////
//
// JsonUtil
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

using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;

namespace UMF.Core
{
	public class JsonUtil
	{
		//------------------------------------------------------------------------		
		public static string EncodeJsonFile<T>( T data, string _filepath )
		{
			string json = EncodeJson<T>( data );

			string dirName = Path.GetDirectoryName( _filepath );
			if( Directory.Exists( dirName ) == false )
				Directory.CreateDirectory( dirName );

			using( FileStream fs = File.Open( _filepath, FileMode.Create, FileAccess.Write, FileShare.Read ) )
			{
				using( StreamWriter sw = new StreamWriter( fs, Encoding.UTF8 ) )
				{
					sw.Write( json );
				}
			}

			return json;
		}
		public static string EncodeJson<T>( T data )
		{
			MemoryStream ms = new MemoryStream();

			// Serializer the User object to the stream.
			DataContractJsonSerializer encode = new DataContractJsonSerializer( typeof( T ) );
			encode.WriteObject( ms, data );

			ms.Position = 0;
			StreamReader sr = new StreamReader( ms );

			string str = sr.ReadToEnd();
			sr.Close();

			return str;
		}

		//------------------------------------------------------------------------
		public static T DecodeJsonFile<T>( string _filePath )
		{
			if( File.Exists( _filePath ) == false )
				throw new System.Exception( string.Format( "File({0}) does not exists", _filePath ) );

			string json = "";
			using( StreamReader sr = new StreamReader( _filePath, Encoding.UTF8 ) )
			{
				json = sr.ReadToEnd();
			}

			return DecodeJson<T>( json );
		}
		public static T DecodeJson<T>( string data )
		{
			DataContractJsonSerializer decode = new DataContractJsonSerializer( typeof( T ) );
			MemoryStream read_stream = new MemoryStream( Encoding.UTF8.GetBytes( data ) );
			T _T = (T)decode.ReadObject( read_stream );
			read_stream.Close();
			return _T;
		}

	}
}
