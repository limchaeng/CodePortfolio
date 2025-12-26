//////////////////////////////////////////////////////////////////////////
//
// EnvironmentProperty
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

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UMF.Core
{
	public class EnvironmentProperty
	{
		protected Dictionary<string, string> mEnvironementDic = new Dictionary<string, string>();
		protected string mEnvName = "";

		/*
			# ENVIRONMENT VARIABLE
			KEY=VALUE
		*/
		//------------------------------------------------------------------------
		public void LoadProperty( string env_name, string config_text )
		{
			mEnvName = env_name;
			mEnvironementDic.Clear();

			using( StringReader sr = new StringReader( config_text ) )
			{
				while( true )
				{
					string line = sr.ReadLine();
					if( line == null )
						break;

					if( string.IsNullOrEmpty( line ) || line.StartsWith( "#" ) )
						continue;

					line = line.Trim();
					int equal_idx = line.IndexOf( '=' );
					if( equal_idx < 0 )
						continue;

					string key = line.Substring( 0, equal_idx ).Trim();
					string value = line.Substring( equal_idx + 1, line.Length - equal_idx - 1 ).Trim();

					if( mEnvironementDic.ContainsKey( key ) )
						throw new Exception( $"{env_name} has duplicated key {key}" );

					mEnvironementDic.Add( key, value );
				}
			}
		}

		//------------------------------------------------------------------------
		public void LoadPropertyFile( string env_file, bool ignore_throw = false )
		{
			mEnvName = env_file;
			string filepath = env_file;
			if( System.IO.File.Exists( filepath ) == true )
			{
				using( StreamReader sr = new StreamReader( filepath ) )
				{
					LoadProperty( mEnvName, sr.ReadToEnd() );
				}
			}
			else
			{
				if( ignore_throw )
					Log.WriteWarning( string.Format( "!! environment file not found:{0}", filepath ) );
				else
					throw new Exception( string.Format( "!! environmentfile not found:{0}", filepath ) );
			}
		}

		//------------------------------------------------------------------------		
		public static bool IsEnvironmentPropertyKey( string value )
		{
			return value.StartsWith( "{" ) && value.EndsWith( "}" );
		}

		//------------------------------------------------------------------------		
		/// <summary>
		///   get_key = {KEY_NAME}
		/// </summary>
		public string GetEnvironmentProperty( string get_key )
		{
			if( mEnvironementDic.Count == 0 )
				return "";

			string key = get_key.Replace( "{", "" ).Replace( "}", "" );

			return GetEnvironmentValue( key );
		}

		//------------------------------------------------------------------------		
		public string GetEnvironmentValue( string key )
		{
			if( mEnvironementDic.Count == 0 )
				return "";

			string value;
			if( mEnvironementDic.TryGetValue( key, out value ) )
				return value;

			return "";
		}

		//------------------------------------------------------------------------
		public void UpdateProperty( string key, string value )
		{
			if( mEnvironementDic.ContainsKey( key ) )
				mEnvironementDic[key] = value;
			else
				mEnvironementDic.Add( key, value );
		}

		//------------------------------------------------------------------------
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( string.Format( "Env:{0}", mEnvName ) );

			foreach( var kvp in mEnvironementDic )
			{
				sb.AppendLine( string.Format( "{0} = {1}", kvp.Key, kvp.Value ) );
			}

			return sb.ToString();
		}
	}
}
