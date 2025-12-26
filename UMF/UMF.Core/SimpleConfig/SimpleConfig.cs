//////////////////////////////////////////////////////////////////////////
//
// SimpleConfig
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

namespace UMF.Core.SimpleConfig
{
	//------------------------------------------------------------------------
	public static class SimpleConfig
	{
		public static T LOAD<T>( SimpleConfigLoaderBase loader ) where T : class, new()
		{
			Type config_type = typeof( T );

			return loader.Load<T>();
		}
	}

	//------------------------------------------------------------------------
	public abstract class SimpleConfigCustomParserBase
	{
		public abstract bool Parse( System.Reflection.FieldInfo fieldinfo, string value );
	}


	//------------------------------------------------------------------------
	public abstract class SimpleConfigLoaderBase
	{
		protected Func<string, string> mGetPathHandler;
		public Func<string, string> GetPathHandler { set { mGetPathHandler = value; } }
		public abstract T Load<T>() where T : class, new();
		public abstract T Load<T>( string file_path ) where T : class, new();
		public virtual T LoadFromText<T>( string txt ) where T : class, new()
		{
			return null;
		}

		protected string mLastLoadLog = "";
		public string LastLoadLog { get { return mLastLoadLog; } }
	}

}