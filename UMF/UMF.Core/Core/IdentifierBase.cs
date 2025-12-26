//////////////////////////////////////////////////////////////////////////
//
// IdentifierBase
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

namespace UMF.Core
{
	//------------------------------------------------------------------------	
	public class IdentifierInt
	{
		protected Dictionary<string, int> mIdCodeDic = new Dictionary<string, int>();
		protected Dictionary<int, string> mCodeIdDic = new Dictionary<int, string>();

		//------------------------------------------------------------------------
		public void Add( string id, int id_code )
		{
			if( mIdCodeDic.ContainsKey( id ) || mCodeIdDic.ContainsKey( id_code ) )
				throw new System.Exception( $"# AppIdentifier : Already exist id/id_code : {id}/{id_code}" );

			mIdCodeDic.Add( id, id_code );
			mCodeIdDic.Add( id_code, id );
		}

		//------------------------------------------------------------------------
		public int Get( string id )
		{
			int id_code;
			if( mIdCodeDic.TryGetValue( id, out id_code ) )
				return id_code;

			return 0;
		}

		//------------------------------------------------------------------------
		public string Get( int code )
		{
			string id;
			if( mCodeIdDic.TryGetValue( code, out id ) )
				return id;

			return "";
		}
	}

	//------------------------------------------------------------------------	
	public class IdentifierShort<T> : Singleton<T> where T : class, new()
	{
		protected Dictionary<string, short> mIdCodeDic = new Dictionary<string, short>();
		protected Dictionary<short, string> mCodeIdDic = new Dictionary<short, string>();

		//------------------------------------------------------------------------
		public void Add( string id, short id_code )
		{
			if( mIdCodeDic.ContainsKey( id ) || mCodeIdDic.ContainsKey( id_code ) )
				throw new System.Exception( $"# AppIdentifier : Already exist id/id_code : {id}/{id_code}" );

			mIdCodeDic.Add( id, id_code );
			mCodeIdDic.Add( id_code, id );
		}

		//------------------------------------------------------------------------
		public short Get( string id )
		{
			short id_code;
			if( mIdCodeDic.TryGetValue( id, out id_code ) )
				return id_code;

			return 0;
		}

		//------------------------------------------------------------------------
		public string Get( short code )
		{
			string id;
			if( mCodeIdDic.TryGetValue( code, out id ) )
				return id;

			return "";
		}
	}
}
