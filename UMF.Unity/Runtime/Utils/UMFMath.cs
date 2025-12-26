//////////////////////////////////////////////////////////////////////////
//
// UMFMath
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// ref code : NGUIMath
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UMF.Unity
{
	public static class UMFMath
	{
		/// <summary>
		/// Lerp function that doesn't clamp the 'factor' in 0-1 range.
		/// </summary>

		[System.Diagnostics.DebuggerHidden]
		[System.Diagnostics.DebuggerStepThrough]
		static public float Lerp( float from, float to, float factor ) { return from * ( 1f - factor ) + to * factor; }

		/// <summary>
		/// Clamp the specified integer to be between 0 and below 'max'.
		/// </summary>

		[System.Diagnostics.DebuggerHidden]
		[System.Diagnostics.DebuggerStepThrough]
		static public int ClampIndex( int val, int max ) { return ( val < 0 ) ? 0 : ( val < max ? val : max - 1 ); }

		/// <summary>
		/// Wrap the index using repeating logic, so that for example +1 past the end means index of '1'.
		/// </summary>

		[System.Diagnostics.DebuggerHidden]
		[System.Diagnostics.DebuggerStepThrough]
		static public int RepeatIndex( int val, int max )
		{
			if( max < 1 ) return 0;
			while( val < 0 ) val += max;
			while( val >= max ) val -= max;
			return val;
		}

		/// <summary>
		/// Ensure that the angle is within -180 to 180 range.
		/// </summary>

		[System.Diagnostics.DebuggerHidden]
		[System.Diagnostics.DebuggerStepThrough]
		static public float WrapAngle( float angle )
		{
			while( angle > 180f ) angle -= 360f;
			while( angle < -180f ) angle += 360f;
			return angle;
		}

		/// <summary>
		/// In the shader, equivalent function would be 'fract'
		/// </summary>

		[System.Diagnostics.DebuggerHidden]
		[System.Diagnostics.DebuggerStepThrough]
		static public float Wrap01( float val ) { return val - Mathf.FloorToInt( val ); }

		/// <summary>
		/// This code is not framerate-independent:
		/// 
		/// target.position += velocity;
		/// velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 9f);
		/// 
		/// But this code is:
		/// 
		/// target.position += NGUIMath.SpringDampen(ref velocity, 9f, Time.deltaTime);
		/// </summary>

		static public Vector3 SpringDampen( ref Vector3 velocity, float strength, float deltaTime )
		{
			if( deltaTime > 1f ) deltaTime = 1f;
			float dampeningFactor = 1f - strength * 0.001f;
			int ms = Mathf.RoundToInt( deltaTime * 1000f );
			float totalDampening = Mathf.Pow( dampeningFactor, ms );
			Vector3 vTotal = velocity * ( ( totalDampening - 1f ) / Mathf.Log( dampeningFactor ) );
			velocity = velocity * totalDampening;
			return vTotal * 0.06f;
		}

		/// <summary>
		/// Same as the Vector3 version, it's a framerate-independent Lerp.
		/// </summary>

		static public Vector2 SpringDampen( ref Vector2 velocity, float strength, float deltaTime )
		{
			if( deltaTime > 1f ) deltaTime = 1f;
			float dampeningFactor = 1f - strength * 0.001f;
			int ms = Mathf.RoundToInt( deltaTime * 1000f );
			float totalDampening = Mathf.Pow( dampeningFactor, ms );
			Vector2 vTotal = velocity * ( ( totalDampening - 1f ) / Mathf.Log( dampeningFactor ) );
			velocity = velocity * totalDampening;
			return vTotal * 0.06f;
		}

		/// <summary>
		/// Calculate how much to interpolate by.
		/// </summary>

		static public float SpringLerp( float strength, float deltaTime )
		{
			if( deltaTime > 1f ) deltaTime = 1f;
			int ms = Mathf.RoundToInt( deltaTime * 1000f );
			deltaTime = 0.001f * strength;
			float cumulative = 0f;
			for( int i = 0; i < ms; ++i ) cumulative = Mathf.Lerp( cumulative, 1f, deltaTime );
			return cumulative;
		}

		/// <summary>
		/// Mathf.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
		/// </summary>

		static public float SpringLerp( float from, float to, float strength, float deltaTime )
		{
			if( deltaTime > 1f ) deltaTime = 1f;
			int ms = Mathf.RoundToInt( deltaTime * 1000f );
			deltaTime = 0.001f * strength;
			for( int i = 0; i < ms; ++i ) from = Mathf.Lerp( from, to, deltaTime );
			return from;
		}

		/// <summary>
		/// Vector2.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
		/// </summary>

		static public Vector2 SpringLerp( Vector2 from, Vector2 to, float strength, float deltaTime )
		{
			return Vector2.Lerp( from, to, SpringLerp( strength, deltaTime ) );
		}

		/// <summary>
		/// Vector3.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
		/// </summary>

		static public Vector3 SpringLerp( Vector3 from, Vector3 to, float strength, float deltaTime )
		{
			return Vector3.Lerp( from, to, SpringLerp( strength, deltaTime ) );
		}

		/// <summary>
		/// Quaternion.Slerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
		/// </summary>

		static public Quaternion SpringLerp( Quaternion from, Quaternion to, float strength, float deltaTime )
		{
			return Quaternion.Slerp( from, to, SpringLerp( strength, deltaTime ) );
		}

		/// <summary>
		/// Since there is no Mathf.RotateTowards...
		/// </summary>

		static public float RotateTowards( float from, float to, float maxAngle )
		{
			float diff = WrapAngle( to - from );
			if( Mathf.Abs( diff ) > maxAngle ) diff = maxAngle * Mathf.Sign( diff );
			return from + diff;
		}

		/// <summary>
		/// Determine the distance from the specified point to the line segment.
		/// </summary>

		static float DistancePointToLineSegment( Vector2 point, Vector2 a, Vector2 b )
		{
			float l2 = ( b - a ).sqrMagnitude;
			if( l2 == 0f ) return ( point - a ).magnitude;
			float t = Vector2.Dot( point - a, b - a ) / l2;
			if( t < 0f ) return ( point - a ).magnitude;
			else if( t > 1f ) return ( point - b ).magnitude;
			Vector2 projection = a + t * ( b - a );
			return ( point - projection ).magnitude;
		}

		//------------------------------------------------------------------------
		/// <summary>
		/// 소숫점 버림
		/// </summary>
		public static float TruncateDigit( float f_value, double digit )
		{
			double digit_value = System.Math.Pow( 10, digit );
			return (float)( System.Math.Truncate( f_value * digit_value ) / digit_value );
		}

        public static class SATCollision
        {
            public static bool PolygonsIntersect( Vector2[] polyA, Vector2[] polyB )
            {
                return !( HasSeparatingAxis( polyA, polyB ) || HasSeparatingAxis( polyB, polyA ) );
            }

            private static bool HasSeparatingAxis( Vector2[] polyA, Vector2[] polyB )
            {
                int count = polyA.Length;
                for( int i = 0; i < count; i++ )
                {
                    Vector2 p1 = polyA[i];
                    Vector2 p2 = polyA[( i + 1 ) % count];

                    Vector2 edge = p2 - p1;
                    Vector2 axis = new Vector2( -edge.y, edge.x ).normalized;

                    (float minA, float maxA) = ProjectPolygon( axis, polyA );
                    (float minB, float maxB) = ProjectPolygon( axis, polyB );

                    if( maxA < minB || maxB < minA )
                        return true; // 분리축 발견 → 겹치지 않음
                }
                return false;
            }

            private static (float, float) ProjectPolygon( Vector2 axis, Vector2[] poly )
            {
                float min = Vector2.Dot( axis, poly[0] );
                float max = min;

                for( int i = 1; i < poly.Length; i++ )
                {
                    float proj = Vector2.Dot( axis, poly[i] );
                    if( proj < min ) min = proj;
                    if( proj > max ) max = proj;
                }
                return (min, max);
            }
        }

    }
}
