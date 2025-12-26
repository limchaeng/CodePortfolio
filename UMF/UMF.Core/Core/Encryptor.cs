//////////////////////////////////////////////////////////////////////////
//
// Encryptor
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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UMF.Core
{
	//------------------------------------------------------------------------	
	public class Encryptor
	{
		byte[] keys;
		long generator_key;
		int keyIndex = 0;

		//------------------------------------------------------------------------	
		public static long NextValue( long value )
		{
			long _const = 1103515245110351524;
			long _add = 54321;
			long _key = _const * value;
			_key = Math.Abs( _key + _add );

			//Log.Write( "NextValue:{0} -> {1}", value, _key );
			return _key;
		}

		//------------------------------------------------------------------------	
		public Encryptor( long key )
		{
			generator_key = key;
			NextKey();
		}

		//------------------------------------------------------------------------	
		public void NextKey()
		{
			++keyIndex;
			generator_key = NextValue( generator_key );
			keys = BitConverter.GetBytes( generator_key );

			//Log.Write( "----NextKey index:{0} generator_key:{1} Keys:{2}", keyIndex, generator_key, System.BitConverter.ToString( keys ) );
		}

		//------------------------------------------------------------------------	
		public MemoryStream Encrypt( MemoryStream data )
		{
			MemoryStream newStream = new MemoryStream();
			newStream.SetLength( data.Length + 2 );
			Int16 sendKeyIndex = (Int16)keyIndex;
			newStream.Write( BitConverter.GetBytes( sendKeyIndex ), 0, 2 );

			//Log.Write( "-----Encrypt : sendKeyIndex {0}", sendKeyIndex );

			byte[] buffer = newStream.GetBuffer();
			for( int i = 0; i < 2; ++i )
			{
				buffer[i] ^= keys[i % keys.Length];
			}

			byte[] dataBuffer = data.GetBuffer();
			byte plus = (byte)newStream.Length;

			for( int i = 2; i < 4; ++i )
			{
				buffer[i] = (byte)( dataBuffer[i - 2] ^ keys[i % keys.Length] );
			}

			for( int i = 4; i < newStream.Length; ++i )
			{
				buffer[i] = (byte)( (byte)( dataBuffer[i - 2] ^ keys[i % keys.Length] ) + plus );
			}

			NextKey();

			return newStream;
		}

		//------------------------------------------------------------------------	
		public void DecryptHeader( MemoryStream data )
		{
			byte[] buffer = data.GetBuffer();

			for( int i = 0; i < 4; ++i )
			{
				buffer[i] ^= keys[i % keys.Length];
			}

			Int16 recvKeyIndex = BitConverter.ToInt16( buffer, 0 );

			//Log.Write( "-----Encrypt : recvKeyIndex {0}", recvKeyIndex );

			if( recvKeyIndex != (Int16)keyIndex )
				throw new Exception( "Decrypt Error" );
		}

		//------------------------------------------------------------------------	
		public MemoryStream Decrypt( MemoryStream data )
		{
			MemoryStream newStream = new MemoryStream();
			newStream.SetLength( data.Length - 2 );

			byte[] dataBuffer = data.GetBuffer();
			newStream.Write( dataBuffer, 2, 2 );

			byte plus = (byte)data.Length;
			byte[] buffer = newStream.GetBuffer();

			for( int i = 4; i < data.Length; ++i )
			{
				//buffer[i - 4] -= plus;
				buffer[i - 2] = (byte)( ( dataBuffer[i] - plus ) ^ keys[i % keys.Length] );
			}

			data.Dispose();

			NextKey();

			return newStream;
		}
	}

	//------------------------------------------------------------------------
	public class EncryptUtil
	{
		//-----------------------------------------------------------------------------
		// DES
		public static bool DESEncrypt( string input, string key, ref string output )
		{
			output = "";

			if( key.Length != 8 )
			{
				output = "Key length must be 8 character!";
				return false;
			}

			MemoryStream memory = null;
			CryptoStream cs = null;
			try
			{
				DESCryptoServiceProvider des = new DESCryptoServiceProvider();
				des.Key = ASCIIEncoding.ASCII.GetBytes( key );
				des.IV = ASCIIEncoding.ASCII.GetBytes( key );

				memory = new MemoryStream();
				cs = new CryptoStream( memory, des.CreateEncryptor(), CryptoStreamMode.Write );
				byte[] data = System.Convert.FromBase64String( input );
				cs.Write( data, 0, data.Length );
				cs.FlushFinalBlock();

				output = Convert.ToBase64String( memory.ToArray() );
				return true;
			}
			catch( System.Exception ex )
			{
				output = ex.ToString();
			}
			finally
			{
				if( cs != null )
					cs.Close();


				if( memory != null )
					memory.Close();
			}

			return false;
		}

		//-----------------------------------------------------------------------------
		public static bool DESDecrypt( string input, string key, ref string output )
		{
			output = "";

			if( key.Length != 8 )
			{
				output = "Key length must be 8 character!";
				return false;
			}

			MemoryStream memory = null;
			CryptoStream cs = null;
			try
			{
				DESCryptoServiceProvider des = new DESCryptoServiceProvider();
				des.Key = ASCIIEncoding.ASCII.GetBytes( key );
				des.IV = ASCIIEncoding.ASCII.GetBytes( key );

				memory = new MemoryStream();
				cs = new CryptoStream( memory, des.CreateDecryptor(), CryptoStreamMode.Write );

				byte[] data = Convert.FromBase64String( input );
				cs.Write( data, 0, data.Length );
				cs.FlushFinalBlock();

				output = ( new UTF8Encoding() ).GetString( memory.ToArray() );
				return true;
			}
			catch( System.Exception ex )
			{
				output = ex.ToString();
			}
			finally
			{
				if( cs != null )
					cs.Close();

				if( memory != null )
					memory.Close();
			}

			return false;
		}
	}
}