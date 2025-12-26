//////////////////////////////////////////////////////////////////////////
//
// UMNum
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
using System.Linq;
using UnityEngine;

namespace UMF.Unity
{
	public struct EZNumDataInt
	{
		public int prev;
		public int next;
		public int curr;
		public bool start;

		public EZNumDataInt( int _prev, int _next, bool _start )
		{
			prev = _prev;
			next = _next;
			start = _start;
			curr = 0;
		}

		public static EZNumDataInt empty
		{
			get { return new EZNumDataInt( 0, 0, false ); }
		}
	}

	public class EZNumLong
	{
		public float duration;
		float startTime, progressTime;
		long begin, end, value, interValue;

		public long Value { get { return value; } }
		public long DeltaValue { get { long v = value - interValue; interValue = value; return v; } }

		public long ValueBegin { get { return begin; } }
		public long ValueEnd { get { return end; } }
		public float ProgressTime { get { return progressTime; } }
		public bool IsEnd { get { return value == end; } }

		public EZNumLong( long begin, long end, float duration )
		{
			startTime = Time.time;
			this.duration = duration;
			this.begin = begin;
			this.end = end;
			interValue = begin;
			progressTime = 0f;
			value = begin;
		}

		public EZNumLong( long begin, long end, float duration, float startDelay )
			: this( begin, end, duration )
		{
			startTime = startTime + startDelay;
		}

		public bool Update()
		{
			progressTime = Time.time - startTime;
			value = begin + (long)( ( end - begin ) * Mathf.Clamp01( progressTime / duration ) );
			return IsEnd;
		}
	}

	public class EZNumInt
	{
		public float duration;
		float startTime, progressTime;
		int begin, end, value, interValue;

		public int Value { get { return value; } }
		public int DeltaValue { get { int v = value - interValue; interValue = value; return v; } }
		public int ValueBegin { get { return begin; } }
		public int ValueEnd { get { return end; } }
		public bool IsEnd { get { return value == end; } }
		public float ProgressTime { get { return progressTime; } }

		public EZNumInt( int begin, int end, float duration )
		{
			startTime = Time.time;
			this.duration = duration;
			this.begin = begin;
			this.end = end;
			interValue = begin;
			progressTime = 0f;
			value = begin;
		}

		public EZNumInt( int begin, int end, float duration, float startDelay )
			: this( begin, end, duration )
		{
			startTime = startTime + startDelay;
		}

		public bool Update()
		{
			progressTime = Time.time - startTime;
			value = begin + (int)( ( end - begin ) * Mathf.Clamp01( progressTime / duration ) );
			return IsEnd;
		}
	}

	public class EZNumFloat
	{
		public float duration;
		float startTime, progressTime;
		float begin, end, value, interValue;
		int loopcount = 0;
		float loopBegin = 0f;
		float loopEnd = 0f;
		int tmpCurrentLoop = 0;

		public float Value { get { return value; } }
		public float DeltaValue { get { float v = value - interValue; interValue = value; return v; } }

		public float ValueBegin { get { return begin; } }
		public float ValueEnd { get { return end; } }
		public float ProgressTime { get { return progressTime; } }
		public bool IsEnd { get { return value == end; } }

		public EZNumFloat( float begin, float end, float duration )
		{
			startTime = Time.time;
			this.duration = duration;
			this.begin = begin;
			this.end = end;
			interValue = begin;
			progressTime = 0f;
			loopcount = 0;
			loopBegin = begin;
			value = begin;
		}

		public void SetLoopCount( int loopcount, float begin, float end )
		{
			this.loopcount = loopcount;
			loopBegin = begin;
			loopEnd = end;
			tmpCurrentLoop = 0;
		}

		public EZNumFloat( float begin, float end, float duration, float startDelay )
			: this( begin, end, duration )
		{
			startTime = startTime + startDelay;
		}

		public bool Update()
		{
			progressTime = Time.time - startTime;
			value = begin + (float)( ( end - begin ) * Mathf.Clamp01( progressTime / duration ) );

			if( IsEnd )
			{
				if( loopcount > 0 && tmpCurrentLoop != loopcount )
				{
					tmpCurrentLoop++;
					startTime = Time.time - ( progressTime - duration );
					progressTime = 0;
					begin = loopBegin;
					end = loopEnd;
					return false;
				}

				return true;
			}

			return false;
		}
	}

	public enum EZNumMonitoringType
	{
		Reset,
		Start,
		Animating,
		Finish,
	}

	public abstract class EZNumMonitoringBase
	{
		abstract public bool IsAnimating { get; protected set; }
		abstract public void Reset();
		abstract public void Update();

		protected float mBaseTime = 0f;
		public float timeGap
		{
			get
			{
				if( mSpeed != 1f && mSpeed != 0f )
					return mBaseTime / mSpeed;

				return mBaseTime;
			}
		}
		protected float mSpeed = 1f;
		public float Speed { get { return mSpeed; } set { mSpeed = value; } }
	}

	public class EZNumMonitoringManager
	{
		List<EZNumMonitoringBase> m_Monitorings = new List<EZNumMonitoringBase>();

		public int Count { get { return m_Monitorings.Count; } }
		public bool IsAnimating { get { return m_Monitorings.Any( m => m.IsAnimating == true ); } }

		public void Add( EZNumMonitoringBase m )
		{
			m.Reset();
			m_Monitorings.Add( m );
		}

		public void Remove( EZNumMonitoringBase m )
		{
			m_Monitorings.Remove( m );
		}

		public void Reset()
		{
			for( int i = 0; i < m_Monitorings.Count; i++ )
				m_Monitorings[i].Reset();
		}

		public void Clear()
		{
			m_Monitorings.Clear();
		}

		public void Update()
		{
			for( int i = 0; i < m_Monitorings.Count; i++ )
				m_Monitorings[i].Update();
		}
	}

	public class EZNumMonitoringInt : EZNumMonitoringBase
	{
		int _StartValue, _Value, _NewValue;
		public int Value { get { return _Value; } }
		public int GetValue() { return _GetValue(); }

		Func<int> _GetValue;
		Func<bool> _CheckStart;
		Action<int, EZNumMonitoringType> _Action;
		float changedTime = Time.time;
		override public bool IsAnimating { get; protected set; }

		public EZNumMonitoringInt( Func<int> _GetValue, Action<int, EZNumMonitoringType> _Action, float timeGap )
		{
			this._GetValue = _GetValue;
			this._Action = _Action;
			this.mBaseTime = (float)timeGap;

			Reset();
		}

		public EZNumMonitoringInt( Func<int> _GetValue, Action<int, EZNumMonitoringType> _Action, float timeGap, Func<bool> _CheckStart )
		{
			this._GetValue = _GetValue;
			this._Action = _Action;
			this.mBaseTime = (float)timeGap;
			this._CheckStart = _CheckStart;

			Reset();
		}

		override public void Reset()
		{
			_StartValue = _NewValue = _Value = _GetValue();
			_Action( _Value, EZNumMonitoringType.Reset );
			IsAnimating = false;
		}

		override public void Update()
		{
			bool bAnimate = IsAnimating;

			EZNumMonitoringType type = EZNumMonitoringType.Animating;

			int newValue = _GetValue();
			if( newValue != _NewValue && ( _CheckStart == null || _CheckStart() == true ) )
			{
				if( _NewValue == _Value && Math.Abs( newValue - _NewValue ) == 1 )
					changedTime = Time.time - timeGap;
				else
					changedTime = Time.time;
				_NewValue = newValue;
				_StartValue = _Value;
				IsAnimating = true;

				type = EZNumMonitoringType.Start;
			}
			if( IsAnimating == false )
				return;

			bool bAction = true;
			if( bAnimate == true )
			{
				if( Time.time - changedTime > timeGap )
				{
					_Value = _NewValue;
					IsAnimating = false;
					type = EZNumMonitoringType.Finish;
				}
				else
				{
					int oldValue = _Value;
					_Value = _StartValue + (int)( ( _NewValue - _StartValue ) * Mathf.Clamp01( ( Time.time - changedTime ) / timeGap ) );
					if( oldValue == _Value )
						bAction = false;
				}
			}
			if( bAction == true )
				_Action( _Value, type );
		}
	}

	public class EZNumMonitoringLong : EZNumMonitoringBase
	{
		long _StartValue, _Value, _NewValue;
		public long Value { get { return _Value; } }
		public long GetValue() { return _GetValue(); }

		Func<long> _GetValue;
		Func<bool> _CheckStart;
		Action<long, EZNumMonitoringType> _Action;
		float changedTime = Time.time;
		override public bool IsAnimating { get; protected set; }

		public EZNumMonitoringLong( Func<long> _GetValue, Action<long, EZNumMonitoringType> _Action, float timeGap )
		{
			this._GetValue = _GetValue;
			this._Action = _Action;
			this.mBaseTime = timeGap;

			Reset();
		}

		public EZNumMonitoringLong( Func<long> _GetValue, Action<long, EZNumMonitoringType> _Action, float timeGap, Func<bool> _CheckStart )
		{
			this._GetValue = _GetValue;
			this._Action = _Action;
			this.mBaseTime = timeGap;
			this._CheckStart = _CheckStart;

			Reset();
		}

		override public void Reset()
		{
			_StartValue = _NewValue = _Value = _GetValue();
			_Action( _Value, EZNumMonitoringType.Reset );
			IsAnimating = false;
		}

		override public void Update()
		{
			bool bAnimate = IsAnimating;

			EZNumMonitoringType type = EZNumMonitoringType.Animating;

			long newValue = _GetValue();
			if( newValue != _NewValue && ( _CheckStart == null || _CheckStart() == true ) )
			{
				if( _NewValue == _Value && Math.Abs( newValue - _NewValue ) == 1 )
					changedTime = Time.time - timeGap;
				else
					changedTime = Time.time;
				_NewValue = newValue;
				_StartValue = _Value;
				IsAnimating = true;

				type = EZNumMonitoringType.Start;
			}

			bool bAction = true;
			if( bAnimate == true )
			{
				if( Time.time - changedTime > timeGap )
				{
					_Value = _NewValue;
					IsAnimating = false;
					type = EZNumMonitoringType.Finish;
				}
				else
				{
					long oldValue = _Value;
					_Value = _StartValue + (long)( ( _NewValue - _StartValue ) * Mathf.Clamp01( ( Time.time - changedTime ) / timeGap ) );
					if( oldValue == _Value )
						bAction = false;
				}
			}
			if( bAction == true )
				_Action( _Value, type );
		}
	}

	public class EZNumMonitoringFloat : EZNumMonitoringBase
	{
		float _StartValue, _Value, _NewValue;
		public float Value { get { return _Value; } }
		public float GetValue() { return _GetValue(); }

		Func<float> _GetValue;
		Func<bool> _CheckStart;
		Action<float, EZNumMonitoringType> _Action;
		float changedTime = Time.time;
		override public bool IsAnimating { get; protected set; }

		public EZNumMonitoringFloat( Func<float> _GetValue, Action<float, EZNumMonitoringType> _Action, float timeGap )
		{
			this._GetValue = _GetValue;
			this._Action = _Action;
			this.mBaseTime = timeGap;

			Reset();
		}

		public EZNumMonitoringFloat( Func<float> _GetValue, Action<float, EZNumMonitoringType> _Action, float timeGap, Func<bool> _CheckStart )
		{
			this._GetValue = _GetValue;
			this._Action = _Action;
			this.mBaseTime = timeGap;
			this._CheckStart = _CheckStart;

			Reset();
		}

		override public void Reset()
		{
			_StartValue = _NewValue = _Value = _GetValue();
			_Action( _Value, EZNumMonitoringType.Reset );
			IsAnimating = false;
		}

		override public void Update()
		{
			bool bAnimate = IsAnimating;

			EZNumMonitoringType type = EZNumMonitoringType.Animating;

			float newValue = _GetValue();
			if( newValue != _NewValue && ( _CheckStart == null || _CheckStart() == true ) )
			{
				if( _NewValue == _Value && Math.Abs( newValue - _NewValue ) == 1 )
					changedTime = Time.time - timeGap;
				else
					changedTime = Time.time;
				_NewValue = newValue;
				_StartValue = _Value;
				IsAnimating = true;

				type = EZNumMonitoringType.Start;
			}
			if( IsAnimating == false )
				return;

			bool bAction = true;
			if( bAnimate == true )
			{
				if( Time.time - changedTime > timeGap )
				{
					_Value = _NewValue;
					IsAnimating = false;
					type = EZNumMonitoringType.Finish;
				}
				else
				{
					float oldValue = _Value;
					_Value = _StartValue + (float)( ( _NewValue - _StartValue ) * Mathf.Clamp01( ( Time.time - changedTime ) / timeGap ) );
					if( oldValue == _Value )
						bAction = false;
				}
			}
			if( bAction == true )
				_Action( _Value, type );
		}
	}
}
