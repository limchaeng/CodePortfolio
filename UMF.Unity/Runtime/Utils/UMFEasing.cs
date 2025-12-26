//////////////////////////////////////////////////////////////////////////
//
// UMFEasing
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

using UnityEngine;

namespace UMF.Unity
{
	public static class UMFEasing
	{
		public enum EasingType
		{
			/// <summary>
			/// Easing equation function for a simple linear tweening, with no easing.
			/// </summary>
			Linear,

			/// <summary>
			/// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: 
			/// decelerating from zero velocity.
			/// </summary>
			Bounce,

			/// <summary>
			/// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in: 
			/// accelerating from zero velocity.
			/// </summary>
			BackEaseIn,
			/// <summary>
			/// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out: 
			/// decelerating from zero velocity.
			/// </summary>
			BackEaseOut,
			/// <summary>
			/// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in/out: 
			/// acceleration until halfway, then deceleration.
			/// </summary>
			BackEaseInOut,

			/// <summary>
			/// Easing equation function for a circular (sqrt(1-t^2)) easing in: 
			/// accelerating from zero velocity.
			/// </summary>
			CircEaseIn,
			/// <summary>
			/// Easing equation function for a circular (sqrt(1-t^2)) easing out: 
			/// decelerating from zero velocity.
			/// </summary>
			CircEaseOut,
			/// <summary>
			/// Easing equation function for a circular (sqrt(1-t^2)) easing in/out: 
			/// acceleration until halfway, then deceleration.
			/// </summary>
			CircEaseInOut,

			/// <summary>
			/// Easing equation function for a cubic (t^3) easing in: 
			/// accelerating from zero velocity.
			/// </summary>
			CubicEaseIn,
			/// <summary>
			/// Easing equation function for a cubic (t^3) easing out: 
			/// decelerating from zero velocity.
			/// </summary>
			CubicEaseOut,
			/// <summary>
			/// Easing equation function for a cubic (t^3) easing in/out: 
			/// acceleration until halfway, then deceleration.
			/// </summary>
			CubicEaseInOut,

			/// <summary>
			/// Easing equation function for an exponential (2^t) easing in: 
			/// accelerating from zero velocity.
			/// </summary>
			ExpoEaseIn,
			/// <summary>
			/// Easing equation function for an exponential (2^t) easing out: 
			/// decelerating from zero velocity.
			/// </summary>
			ExpoEaseOut,
			/// <summary>
			/// Easing equation function for an exponential (2^t) easing in/out: 
			/// acceleration until halfway, then deceleration.
			/// </summary>
			ExpoEaseInOut,

			/// <summary>
			/// Easing equation function for a quadratic (t^2) easing in: 
			/// accelerating from zero velocity.
			/// </summary>
			QuadEaseIn,
			/// <summary>
			/// Easing equation function for a quadratic (t^2) easing out: 
			/// decelerating from zero velocity.
			/// </summary>
			QuadEaseOut,
			/// <summary>
			/// Easing equation function for a quadratic (t^2) easing in/out: 
			/// acceleration until halfway, then deceleration.
			/// </summary>
			QuadEaseInOut,

			/// <summary>
			/// Easing equation function for a quartic (t^4) easing in: 
			/// accelerating from zero velocity.
			/// </summary>
			QuartEaseIn,
			/// <summary>
			/// Easing equation function for a quartic (t^4) easing out: 
			/// decelerating from zero velocity.
			/// </summary>
			QuartEaseOut,
			/// <summary>
			/// Easing equation function for a quartic (t^4) easing in/out: 
			/// acceleration until halfway, then deceleration.
			/// </summary>
			QuartEaseInOut,

			/// <summary>
			/// Easing equation function for a quintic (t^5) easing in: 
			/// accelerating from zero velocity.
			/// </summary>
			QuintEaseIn,
			/// <summary>
			/// Easing equation function for a quintic (t^5) easing out: 
			/// decelerating from zero velocity.
			/// </summary>
			QuintEaseOut,
			/// <summary>
			/// Easing equation function for a quintic (t^5) easing in/out: 
			/// acceleration until halfway, then deceleration.
			/// </summary>
			QuintEaseInOut,

			/// <summary>
			/// Easing equation function for a sinusoidal (sin(t)) easing in: 
			/// accelerating from zero velocity.
			/// </summary>
			SineEaseIn,
			/// <summary>
			/// Easing equation function for a sinusoidal (sin(t)) easing out: 
			/// decelerating from zero velocity.
			/// </summary>
			SineEaseOut,
			/// <summary>
			/// Easing equation function for a sinusoidal (sin(t)) easing in/out: 
			/// acceleration until halfway, then deceleration.
			/// </summary>
			SineEaseInOut,

			Spring,

			ElasticEaseIn,
			ElasticEaseOut,
			ElasticEaseInOut,
		}
		public delegate float EasingFunction( float start, float end, float time );
		public static EasingFunction GetFunction( EasingType easeType )
		{
			switch( easeType )
			{
				case EasingType.BackEaseIn: return easeInBack;
				case EasingType.BackEaseInOut: return easeInOutBack;
				case EasingType.BackEaseOut: return easeOutBack;
				case EasingType.Bounce: return bounce;
				case EasingType.CircEaseIn: return easeInCirc;
				case EasingType.CircEaseInOut: return easeInOutCirc;
				case EasingType.CircEaseOut: return easeOutCirc;
				case EasingType.CubicEaseIn: return easeInCubic;
				case EasingType.CubicEaseInOut: return easeInOutCubic;
				case EasingType.CubicEaseOut: return easeOutCubic;
				case EasingType.ExpoEaseIn: return easeInExpo;
				case EasingType.ExpoEaseInOut: return easeInOutExpo;
				case EasingType.ExpoEaseOut: return easeOutExpo;
				case EasingType.Linear: return linear;
				case EasingType.QuadEaseIn: return easeInQuad;
				case EasingType.QuadEaseInOut: return easeInOutQuad;
				case EasingType.QuadEaseOut: return easeOutQuad;
				case EasingType.QuartEaseIn: return easeInQuart;
				case EasingType.QuartEaseInOut: return easeInOutQuart;
				case EasingType.QuartEaseOut: return easeOutQuart;
				case EasingType.QuintEaseIn: return easeInQuint;
				case EasingType.QuintEaseInOut: return easeInOutQuint;
				case EasingType.QuintEaseOut: return easeOutQuint;
				case EasingType.SineEaseIn: return easeInSine;
				case EasingType.SineEaseInOut: return easeInOutSine;
				case EasingType.SineEaseOut: return easeOutSine;
				case EasingType.Spring: return spring;
				case EasingType.ElasticEaseIn: return easeInElastic;
				case EasingType.ElasticEaseOut: return easeOutElastic;
				case EasingType.ElasticEaseInOut: return easeInOutElastic;
			}

			throw new System.NotImplementedException();
		}

		public static float linear( float start, float end, float time )
		{
			return Mathf.Lerp( start, end, time );
		}

		public static float clerp( float start, float end, float time )
		{
			float min = 0.0f;
			float max = 360.0f;
			float half = Mathf.Abs( ( max - min ) / 2.0f );
			float retval = 0.0f;
			float diff = 0.0f;
			if( ( end - start ) < -half )
			{
				diff = ( ( max - start ) + end ) * time;
				retval = start + diff;
			}
			else if( ( end - start ) > half )
			{
				diff = -( ( max - end ) + start ) * time;
				retval = start + diff;
			}
			else retval = start + ( end - start ) * time;
			return retval;
		}

		public static float spring( float start, float end, float time )
		{
			time = Mathf.Clamp01( time );
			time = ( Mathf.Sin( time * Mathf.PI * ( 0.2f + 2.5f * time * time * time ) ) * Mathf.Pow( 1f - time, 2.2f ) + time ) * ( 1f + ( 1.2f * ( 1f - time ) ) );
			return start + ( end - start ) * time;
		}

		public static float easeInQuad( float start, float end, float time )
		{
			end -= start;
			return end * time * time + start;
		}

		public static float easeOutQuad( float start, float end, float time )
		{
			end -= start;
			return -end * time * ( time - 2 ) + start;
		}

		public static float easeInOutQuad( float start, float end, float time )
		{
			time /= .5f;
			end -= start;
			if( time < 1 ) return end / 2 * time * time + start;
			time--;
			return -end / 2 * ( time * ( time - 2 ) - 1 ) + start;
		}

		public static float easeInCubic( float start, float end, float time )
		{
			end -= start;
			return end * time * time * time + start;
		}

		public static float easeOutCubic( float start, float end, float time )
		{
			time--;
			end -= start;
			return end * ( time * time * time + 1 ) + start;
		}

		public static float easeInOutCubic( float start, float end, float time )
		{
			time /= .5f;
			end -= start;
			if( time < 1 ) return end / 2 * time * time * time + start;
			time -= 2;
			return end / 2 * ( time * time * time + 2 ) + start;
		}

		public static float easeInQuart( float start, float end, float time )
		{
			end -= start;
			return end * time * time * time * time + start;
		}

		public static float easeOutQuart( float start, float end, float time )
		{
			time--;
			end -= start;
			return -end * ( time * time * time * time - 1 ) + start;
		}

		public static float easeInOutQuart( float start, float end, float time )
		{
			time /= .5f;
			end -= start;
			if( time < 1 ) return end / 2 * time * time * time * time + start;
			time -= 2;
			return -end / 2 * ( time * time * time * time - 2 ) + start;
		}

		public static float easeInQuint( float start, float end, float time )
		{
			end -= start;
			return end * time * time * time * time * time + start;
		}

		public static float easeOutQuint( float start, float end, float time )
		{
			time--;
			end -= start;
			return end * ( time * time * time * time * time + 1 ) + start;
		}

		public static float easeInOutQuint( float start, float end, float time )
		{
			time /= .5f;
			end -= start;
			if( time < 1 ) return end / 2 * time * time * time * time * time + start;
			time -= 2;
			return end / 2 * ( time * time * time * time * time + 2 ) + start;
		}

		public static float easeInSine( float start, float end, float time )
		{
			end -= start;
			return -end * Mathf.Cos( time / 1 * ( Mathf.PI / 2 ) ) + end + start;
		}

		public static float easeOutSine( float start, float end, float time )
		{
			end -= start;
			return end * Mathf.Sin( time / 1 * ( Mathf.PI / 2 ) ) + start;
		}

		public static float easeInOutSine( float start, float end, float time )
		{
			end -= start;
			return -end / 2 * ( Mathf.Cos( Mathf.PI * time / 1 ) - 1 ) + start;
		}

		public static float easeInExpo( float start, float end, float time )
		{
			end -= start;
			return end * Mathf.Pow( 2, 10 * ( time / 1 - 1 ) ) + start;
		}

		public static float easeOutExpo( float start, float end, float time )
		{
			end -= start;
			return end * ( -Mathf.Pow( 2, -10 * time / 1 ) + 1 ) + start;
		}

		public static float easeInOutExpo( float start, float end, float time )
		{
			time /= .5f;
			end -= start;
			if( time < 1 ) return end / 2 * Mathf.Pow( 2, 10 * ( time - 1 ) ) + start;
			time--;
			return end / 2 * ( -Mathf.Pow( 2, -10 * time ) + 2 ) + start;
		}

		public static float easeInCirc( float start, float end, float time )
		{
			end -= start;
			return -end * ( Mathf.Sqrt( 1 - time * time ) - 1 ) + start;
		}

		public static float easeOutCirc( float start, float end, float time )
		{
			time--;
			end -= start;
			return end * Mathf.Sqrt( 1 - time * time ) + start;
		}

		public static float easeInOutCirc( float start, float end, float time )
		{
			time /= .5f;
			end -= start;
			if( time < 1 ) return -end / 2 * ( Mathf.Sqrt( 1 - time * time ) - 1 ) + start;
			time -= 2;
			return end / 2 * ( Mathf.Sqrt( 1 - time * time ) + 1 ) + start;
		}

		public static float bounce( float start, float end, float time )
		{
			time /= 1f;
			end -= start;
			if( time < 0.363636f ) // 0.363636 = (1/ 2.75)
			{
				return end * ( 7.5625f * time * time ) + start;
			}
			else if( time < 0.727272f ) // 0.727272 = (2 / 2.75)
			{
				time -= 0.545454f; // 0.545454f = ( 1.5f / 2.75f );
				return end * ( 7.5625f * ( time ) * time + .75f ) + start;
			}
			else if( time < 0.909090f ) // 0.909090 = (2.5 / 2.75) 
			{
				time -= 0.818181f; // 0.818181f = ( 2.25f / 2.75f );
				return end * ( 7.5625f * ( time ) * time + .9375f ) + start;
			}
			else
			{
				time -= 0.9545454f; // 0.9545454f = ( 2.625f / 2.75f );
				return end * ( 7.5625f * ( time ) * time + .984375f ) + start;
			}
		}

		public static float easeInBack( float start, float end, float time )
		{
			end -= start;
			time /= 1;
			float s = 1.70158f;
			return end * ( time ) * time * ( ( s + 1 ) * time - s ) + start;
		}

		public static float easeOutBack( float start, float end, float time )
		{
			float s = 1.70158f;
			end -= start;
			time = ( time / 1 ) - 1;
			return end * ( ( time ) * time * ( ( s + 1 ) * time + s ) + 1 ) + start;
		}

		public static float easeInOutBack( float start, float end, float time )
		{
			float s = 1.70158f;
			end -= start;
			time /= .5f;
			if( ( time ) < 1 )
			{
				s *= ( 1.525f );
				return end / 2 * ( time * time * ( ( ( s ) + 1 ) * time - s ) ) + start;
			}
			time -= 2;
			s *= ( 1.525f );
			return end / 2 * ( ( time ) * time * ( ( ( s ) + 1 ) * time + s ) + 2 ) + start;
		}

		public static float punch( float amplitude, float time )
		{
			float s = 9;
			if( time == 0 )
			{
				return 0;
			}
			if( time == 1 )
			{
				return 0;
			}
			float period = 1 * 0.3f;
			s = period / ( 2 * Mathf.PI ) * Mathf.Asin( 0 );
			return ( amplitude * Mathf.Pow( 2, -10 * time ) * Mathf.Sin( ( time * 1 - s ) * ( 2 * Mathf.PI ) / period ) );
		}

		public static float easeInElastic( float start, float end, float val )
		{
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if( val == 0 ) return start;
			val = val / d;
			if( val == 1 ) return start + end;

			if( a == 0f || a < Mathf.Abs( end ) )
			{
				a = end;
				s = p / 4;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a );
			}
			val = val - 1;
			return -( a * Mathf.Pow( 2, 10 * val ) * Mathf.Sin( ( val * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start;
		}

		public static float easeOutElastic( float start, float end, float val )
		{
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if( val == 0 ) return start;

			val = val / d;
			if( val == 1 ) return start + end;

			if( a == 0f || a < Mathf.Abs( end ) )
			{
				a = end;
				s = p / 4;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a );
			}

			return ( a * Mathf.Pow( 2, -10 * val ) * Mathf.Sin( ( val * d - s ) * ( 2 * Mathf.PI ) / p ) + end + start );
		}

		public static float easeInOutElastic( float start, float end, float val )
		{
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if( val == 0 ) return start;

			val = val / ( d / 2 );
			if( val == 2 ) return start + end;

			if( a == 0f || a < Mathf.Abs( end ) )
			{
				a = end;
				s = p / 4;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a );
			}

			if( val < 1 )
			{
				val = val - 1;
				return -0.5f * ( a * Mathf.Pow( 2, 10 * val ) * Mathf.Sin( ( val * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start;
			}
			val = val - 1;
			return a * Mathf.Pow( 2, -10 * val ) * Mathf.Sin( ( val * d - s ) * ( 2 * Mathf.PI ) / p ) * 0.5f + end + start;
		}
	}
}