//////////////////////////////////////////////////////////////////////////
//
// MD5String
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

using System.Security.Cryptography;
using System.Text;

namespace UMF.Core
{
	public class MD5String
	{
		static MD5 md5Hasher = new MD5CryptoServiceProvider();

		static public string GetHashString( byte[] data )
		{
			byte[] md5data;

			lock( md5Hasher )
			{
				md5data = md5Hasher.ComputeHash( data );
			}

			StringBuilder sBuilder = new StringBuilder();

			// Loop through each byte of the hashed data // and format each one as a hexadecimal string.for (
			foreach( byte value in md5data )
			{
				sBuilder.Append( value.ToString( "X2" ) );
			}

			return sBuilder.ToString();
		}

		static public string GetHashString( string str )
		{
			return GetHashString( Encoding.Default.GetBytes( str ) );
		}
	}
}
