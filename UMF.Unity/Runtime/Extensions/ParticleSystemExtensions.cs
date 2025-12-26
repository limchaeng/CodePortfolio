//////////////////////////////////////////////////////////////////////////
//
// ParticleSystemExtensions
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UMF.Unity
{
	public static class ParticleSystemExtensions
	{
		//------------------------------------------------------------------------
		static float CalcParticleSystemLifeTime( ParticleSystem ps, bool ignore_duration = false )
		{
			float life_time = 0f;
			ParticleSystem.MinMaxCurve minmax_delay = ps.main.startDelay;
			float delay_time = 0f;
			switch( minmax_delay.mode )
			{
				case ParticleSystemCurveMode.Constant:
					delay_time = minmax_delay.constant;
					break;

				case ParticleSystemCurveMode.TwoConstants:
					delay_time = minmax_delay.constantMax;
					break;
			}

			ParticleSystem.MinMaxCurve minmaxcurve = ps.main.startLifetime;

			switch( minmaxcurve.mode )
			{
				case ParticleSystemCurveMode.Constant:
					life_time = minmaxcurve.constant + delay_time;
					break;

				case ParticleSystemCurveMode.TwoConstants:
					life_time = minmaxcurve.constantMax + delay_time;
					break;

				case ParticleSystemCurveMode.Curve:
				case ParticleSystemCurveMode.TwoCurves:
				default:
					{
						float _maxvalue = float.MinValue;
						for( int i = 0; i < minmaxcurve.curve.length; i++ )
							_maxvalue = Mathf.Max( _maxvalue, minmaxcurve.curve.keys[i].value );

						life_time = ( _maxvalue * minmaxcurve.curveMultiplier ) + delay_time;
					}
					break;
			}

			if( ignore_duration )
				return life_time;
			else
				return Mathf.Max( life_time, ps.main.duration );
		}

		public static float CalcParticleMaxLifeTime( this ParticleSystem ps, bool ignore_duration = false )
		{
			float life_time = CalcParticleSystemLifeTime( ps );
			ParticleSystem[] all_ps = ps.GetComponentsInChildren<ParticleSystem>( true );
			foreach( ParticleSystem p_system in all_ps )
			{
				life_time = Mathf.Max( life_time, CalcParticleSystemLifeTime( p_system ) );
			}

			return life_time;
		}

		//------------------------------------------------------------------------
		public static float CalcParticleMaxDuration( this ParticleSystem ps, bool include_loop )
		{
			float duration = ps.main.duration;
			ParticleSystem[] all_ps = ps.GetComponentsInChildren<ParticleSystem>( true );
			foreach( ParticleSystem p_system in all_ps )
			{
				if( include_loop || p_system.main.loop == false  )
					duration = Mathf.Max( duration, p_system.main.duration );
			}

			return duration;
		}
	}
}