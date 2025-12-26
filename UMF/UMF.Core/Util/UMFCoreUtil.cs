//////////////////////////////////////////////////////////////////////////
//
// Util
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
using System.Globalization;
using System.Diagnostics;

namespace UMF.Core
{
	//------------------------------------------------------------------------	
	public class InfinityWhile
	{
		public InfinityWhile( int max_count, System.Func<int, bool> action )
		{
			int idx = 1;
			int _max = max_count;
			while( idx <= _max )
			{
				if( action( idx ) )
					break;

				idx += 1;
			}
		}
	}

	//------------------------------------------------------------------------	
	public class ClassObjectContainer<T> where T : class
	{
		public T obj;
	}

	//------------------------------------------------------------------------
	public class IntValueRange
	{
		public int Value1 { get; set; }
		public int Value2 { get; set; }

		public IntValueRange()
		{
			Value1 = 0;
			Value2 = 0;
		}

		public IntValueRange( int v1, int v2 )
		{
			Value1 = v1;
			Value2 = v2;
		}

		public IntValueRange( string v1, string v2 )
		{
			Value1 = StringUtil.SafeParse<int>( v1, 0 );
			Value2 = StringUtil.SafeParse<int>( v2, 0 );
		}

		public int GetRangeValue( UMFRandom rnd )
		{
			if( Value1 == Value2 )
				return Value1;

			int min = Value1;
			int max = Value2;
			if( Value2 < Value1 )
			{
				min = Value2;
				max = Value1;
			}

			return rnd.NextRange( min, max );
		}

		public bool IsContain( int v )
		{
			return ( v >= Value1 && v <= Value2 );
		}

		/// <summary>
		///   comma(,) separator
		/// </summary>
		public static IntValueRange Parse( string parse_text, int default_value, char sep = ',' )
		{
			if( string.IsNullOrEmpty( parse_text ) )
				return null;

			string[] split = parse_text.Split( sep );
			if( split != null )
			{
				int v1 = 0;
				int v2 = 0;
				if( split.Length == 1 )
				{
					v1 = StringUtil.SafeParse<int>( split[0], default_value );
					v2 = v1;
				}
				else if( split.Length == 2 )
				{
					v1 = StringUtil.SafeParse<int>( split[0], default_value );
					v2 = StringUtil.SafeParse<int>( split[1], default_value );
				}

				return new IntValueRange( v1, v2 );
			}

			return null;
		}

		public static IntValueRange Parse( string parse_text, ref int single_value, int default_value, char sep = ',' )
		{
			single_value = default_value;
			if( string.IsNullOrEmpty( parse_text ) )
				return null;

			string[] split = parse_text.Split( sep );
			if( split != null )
			{
				if( split.Length == 1 )
				{
					single_value = StringUtil.SafeParse<int>( split[0], default_value );
					return null;
				}
				else if( split.Length == 2 )
				{
					int v1 = StringUtil.SafeParse<int>( split[0], default_value );
					int v2 = StringUtil.SafeParse<int>( split[1], default_value );

					return new IntValueRange( v1, v2 );
				}
			}

			return null;
		}

	}

	//------------------------------------------------------------------------	
	public class FloatValueRange
	{
		public float Value1 { get; set; }
		public float Value2 { get; set; }

		public FloatValueRange()
		{
			Value1 = 0f;
			Value2 = 0f;
		}

		public FloatValueRange( float v1, float v2 )
		{
			Value1 = v1;
			Value2 = v2;
		}

		public FloatValueRange( string v1, string v2 )
		{
			Value1 = StringUtil.SafeParse<float>( v1, 0f );
			Value2 = StringUtil.SafeParse<float>( v2, 0f );
		}

		public float GetRangeValue( UMFRandom rnd )
		{
			if( Value1 == Value2 )
				return Value1;

			float min = Value1;
			float max = Value2;
			if( Value2 < Value1 )
			{
				min = Value2;
				max = Value1;
			}

			if( HasPoint() )
			{
				int calc_value = rnd.NextRange( (int)( min * 100f ), (int)( max * 100f ) );
				double _value = (double)( calc_value * 0.01f );

				return (float)System.Math.Round( _value, 2 );
			}
			else
			{
				int calc_value = rnd.NextRange( (int)min, (int)max );
				return (float)calc_value;
			}
		}

		public bool HasPoint()
		{
			return ( System.Math.Abs( Value1 - (int)Value1 ) > 0f || System.Math.Abs( Value2 - (int)Value2 ) > 0f );
		}

		public bool IsContain( float v )
		{
			return ( v >= Value1 && v <= Value2 );
		}

		/// <summary>
		///   comma(,) separator
		/// </summary>
		public static FloatValueRange Parse( string parse_text, float default_value, char sep = ',' )
		{
			if( string.IsNullOrEmpty( parse_text ) )
				return null;

			string[] split = parse_text.Split( sep );
			if( split != null )
			{
				float v1 = 0f;
				float v2 = 0f;
				if( split.Length == 1 )
				{
					v1 = StringUtil.SafeParse<float>( split[0], default_value );
					v2 = v1;
				}
				else if( split.Length == 2 )
				{
					v1 = StringUtil.SafeParse<float>( split[0], default_value );
					v2 = StringUtil.SafeParse<float>( split[1], default_value );
				}

				return new FloatValueRange( v1, v2 );
			}

			return null;
		}

		public static FloatValueRange Parse( string parse_text, ref float single_value, float default_value, char sep = ',' )
		{
			single_value = default_value;
			if( string.IsNullOrEmpty( parse_text ) )
				return null;

			string[] split = parse_text.Split( sep );
			if( split != null )
			{
				if( split.Length == 1 )
				{
					single_value = StringUtil.SafeParse<float>( split[0], default_value );
					return null;
				}
				else if( split.Length == 2 )
				{
					float v1 = StringUtil.SafeParse<float>( split[0], default_value );
					float v2 = StringUtil.SafeParse<float>( split[1], default_value );

					return new FloatValueRange( v1, v2 );
				}
			}

			return null;
		}
	}

	//------------------------------------------------------------------------
	public static class UMFCoreUtil
	{
		//------------------------------------------------------------------------		
		public static T Clamp<T>( T val, T min, T max ) where T : System.IComparable<T>
		{
			if( val.CompareTo( min ) < 0 ) return min;
			else if( val.CompareTo( max ) > 0 ) return max;
			else return val;
		}

		//------------------------------------------------------------------------
		public static uint ARGB2UINT( int a, int r, int g, int b )
		{
			return ARGB2UINT( (byte)a, (byte)r, (byte)g, (byte)b );
		}
		public static uint ARGB2UINT( byte a, byte r, byte g, byte b )
		{
			return ( (uint)( a << 24 ) ) | ( (uint)( r << 16 ) ) | ( (uint)( g << 8 ) ) | ( (uint)b );
		}
		public static uint ColorHEX_RGBA_2_UINT_ARGB( string hex_color )
		{
			try
			{
				string hex = hex_color.Trim();
				if( hex.IndexOf( '#' ) != -1 )
					hex = hex.Replace( "#", "" );

				int r = 255, g = 255, b = 255, a = 255;

				if( hex.Length >= 2 )
					r = int.Parse( hex.Substring( 0, 2 ), NumberStyles.AllowHexSpecifier );

				if( hex.Length >= 4 )
					g = int.Parse( hex.Substring( 2, 2 ), NumberStyles.AllowHexSpecifier );

				if( hex.Length >= 6 )
					b = int.Parse( hex.Substring( 4, 2 ), NumberStyles.AllowHexSpecifier );

				if( hex.Length >= 8 )
					a = int.Parse( hex.Substring( 6, 2 ), NumberStyles.AllowHexSpecifier );

				return ARGB2UINT( (byte)a, (byte)r, (byte)g, (byte)b );
			}
			catch( System.Exception ex )
			{
				Log.WriteError( "!! ColorHEX_RGBA_2_UINT_ARGB:{0}", ex.ToString() );
			}

			return ARGB2UINT( 255, 255, 255, 255 );
		}
		public static string UINT_ARGB_2_ColorHEX_RGBA( uint n_color )
		{
			byte b = (byte)( ( n_color ) & 0xFF );
			byte g = (byte)( ( n_color >> 8 ) & 0xFF );
			byte r = (byte)( ( n_color >> 16 ) & 0xFF );
			byte a = (byte)( ( n_color >> 24 ) & 0xFF );

			return string.Format( "{0}{1}{2}{3}", r.ToString( "X2" ), g.ToString( "X2" ), b.ToString( "X2" ), a.ToString( "X2" ) );
		}

        //------------------------------------------------------------------------		
        public static int CalcDaysNow( DateTime target_time )
		{
			return CalcDays( DateTime.Now, target_time );
		}

        public static int CalcDays( DateTime src_time, DateTime target_time )
		{
			return ( src_time.Date - target_time.Date ).Days;
		}

		//------------------------------------------------------------------------		
		public static double CalcTotalDays( DateTime src_time, DateTime target_time )
		{
			return ( src_time - target_time ).TotalDays;
		}

		//------------------------------------------------------------------------		
		public static bool IsMonthDiff( DateTime src_time, DateTime curr_time )
		{
			if( src_time.Year < curr_time.Year || src_time.Month < curr_time.Month )
				return true;

			return false;
		}

		//------------------------------------------------------------------------		
		public static DateTime GetNextMonthlyBeginTime( DateTime curr_time )
		{
			DateTime next_month = curr_time.Date.AddMonths( 1 );
			return new DateTime( next_month.Year, next_month.Month, 1 );
		}

		//------------------------------------------------------------------------		
		public static string GetCallStack()
		{
			StackTrace stackTrace = new StackTrace( true );           // get call stack
			StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

			string str = "";

			// write call stack method names
			foreach( StackFrame stackFrame in stackFrames )
			{
				System.Reflection.MethodBase method = stackFrame.GetMethod();
				string strFrame = string.Format( "{0}:{1} (at {2}:{3})", method.DeclaringType, method.Name, stackFrame.GetFileName(), stackFrame.GetFileLineNumber() );
				str += strFrame + "\n";
			}

			return str;
		}
	}
}
