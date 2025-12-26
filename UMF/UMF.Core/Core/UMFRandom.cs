//////////////////////////////////////////////////////////////////////////
//
// Random
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
using System.Linq;

namespace UMF.Core
{
	//------------------------------------------------------------------------
	public interface IUMFChance
	{
		int IUMFChance_GetChance { get; }
	}

	//------------------------------------------------------------------------	
	public class UMFRandom
	{
		long value;
		public long Value { get { return value; } }
		public long Seed { set { this.value = value; } }

		public UMFRandom()
		{
			value = System.DateTime.Now.ToBinary();
		}

		public UMFRandom( long seed )
		{
			this.value = seed;
		}

		public static long NextValue( long value )
		{
			value ^= ( value << 21 );
			value ^= ( value >> 35 );
			value ^= ( value << 4 );
			return value;
		}

		public long _Next()
		{
			value = NextValue( value );
			return Math.Abs( value );
		}

		public int Next()
		{
			long result;
			System.Math.DivRem( _Next() / 10, (long)int.MaxValue + 1, out result );
			return (int)result;
		}

		/// <summary>
		///   max Include
		/// </summary>
		public int NextRange( int min, int max )
		{
			return (int)_NextRange( min, max );
		}

		public long _NextRange( long min, long max )
		{
			System.Diagnostics.Debug.Assert( min < 0 || max < 0 );

			if( min == max || max == 0 )
				return min;

			long result;
			System.Math.DivRem( _Next() / 10, max - min + 1, out result );
			System.Diagnostics.Debug.Assert( result + min <= max );

			return result + min;
		}

		public bool Check_10KChance( int chance )
		{
			return ( NextRange( 1, 10000 ) <= chance );
		}

		public T Get_10KChanceSelect<T>( List<T> list, bool use_chance_sum ) where T : class, IUMFChance
		{
			if( list == null || list.Count <= 0 )
				return null;

			List<T> chance_list = list.OrderByDescending( a => a.IUMFChance_GetChance ).ToList();

			int max_chance = 10000;
			if( use_chance_sum )
			{
				if( chance_list.Count > 1 )
					max_chance = chance_list.Sum( a => a.IUMFChance_GetChance );
				else
					max_chance = chance_list[0].IUMFChance_GetChance;
			}

			int selected_chance = NextRange( 1, max_chance );
			foreach( T data in chance_list )
			{
				selected_chance -= data.IUMFChance_GetChance;
				if( selected_chance <= 0 )
					return data;
			}

			return null;
		}

		public static UMFRandom Instance = new UMFRandom();
	}
}