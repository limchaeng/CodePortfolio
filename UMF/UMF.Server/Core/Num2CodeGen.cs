//////////////////////////////////////////////////////////////////////////
//
// Num2CodeGen
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

using System.Linq;

namespace UMF.Server
{
	public class Num2CodeGen
	{
		const string Codes = "EH4DKB7LQGT6UJ5V9NXR2MPA8ZSC3YFW";    // removes I, 1, O, 0
		public const long MaxValue = 1048576;
		const int UnitCount = 4;

		static int[] DecryptTable;

		static public bool IsCodeLetter( char c ) { return Codes.Contains( c ); }

		//------------------------------------------------------------------------		
		static public long Repeat( long value, long max )
		{
			if( value >= max )
				value %= max;

			while( value < 0 )
				value += max;

			return value;
		}

		//------------------------------------------------------------------------		
		static public string Encrypt( long value, int unit_count )
		{
			string code = "";
			long base_index = 0;
			for( int unit_index = 0; unit_index < unit_count; ++unit_index )
			{
				int find_value = (int)Repeat( value + base_index, Codes.Length );
				code += Codes[find_value];

				value /= Codes.Length;

				base_index = unit_index * 5 + find_value;
			}
			return code;
		}

		//------------------------------------------------------------------------		
		static public string Encrypt( long value )
		{
			return Encrypt( value % MaxValue, UnitCount );
		}

		//------------------------------------------------------------------------		
		/// <summary>
		///  CAUTION : string code upper / lower different long value
		/// </summary>
		static public long Decrypt( string code )
		{
			if( DecryptTable == null )
			{
				if( Codes.Distinct().Count() != Codes.Length )
					throw new System.Exception( "Codes broken!!" );

				DecryptTable = new int[128];
				for( int i = 0; i < Codes.Length; ++i )
				{
					DecryptTable[Codes[i]] = i;
				}
			}
			long value = 0;
			long base_index = 0;

			long pow = 1;
			for( int unit_index = 0; unit_index < code.Length; ++unit_index )
			{
				char code_value = code[unit_index];
				long find_value = DecryptTable[code_value];//Codes.IndexOf(code_value);
				long set_value = (int)Repeat( find_value - base_index + Codes.Length, Codes.Length );

				value += set_value * pow;
				pow *= Codes.Length;

				base_index = unit_index * 5 + find_value;
			}
			return value;
		}

		//------------------------------------------------------------------------		
		static public long CreateNextValue( long value )
		{
			return UMF.Core.UMFRandom.NextValue( value ) % MaxValue;
		}
	}
}
