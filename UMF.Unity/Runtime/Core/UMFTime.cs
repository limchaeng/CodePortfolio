//////////////////////////////////////////////////////////////////////////
//
// UMFTime
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UMF.Unity
{
	public static class UMFTime 
	{
		public const string TextKey_Month_Day = "Time_Month_Day";
		public const string TextKey_Day_Hour = "Time_Day_Hour";
		public const string TextKey_Hour_Min = "Time_Hour_Min";
		public const string TextKey_Min_Second = "Time_Min_Second";
		public const string TextKey_Min_SecondMilli = "Time_Min_SecondMilli";
		public const string TextKey_Month = "Time_Month";
		public const string TextKey_Day = "Time_Day";
		public const string TextKey_Hour = "Time_Hour";
		public const string TextKey_Min = "Time_Min";
		public const string TextKey_Second = "Time_Second";
		public const string TextKey_SecondMilli = "Time_SecondMilli";
		public const string TextKey_FullTimeHour = "Time_FullTimeHour";
		public const string TextKey_FullTimeMinute = "Time_FullTimeMinute";
		public const string TextKey_DayOfWeek_Sunday = "Time_DayOfWeek_Sunday";
		public const string TextKey_DayOfWeek_Monday = "Time_DayOfWeek_Monday";
		public const string TextKey_DayOfWeek_Tuesday = "Time_DayOfWeek_Tuesday";
		public const string TextKey_DayOfWeek_Wednesday = "Time_DayOfWeek_Wednesday";
		public const string TextKey_DayOfWeek_Thursday = "Time_DayOfWeek_Thursday";
		public const string TextKey_DayOfWeek_Friday = "Time_DayOfWeek_Friday";
		public const string TextKey_DayOfWeek_Saturday = "Time_DayOfWeek_Saturday";
		public const string TextKey_BeginEndTime = "Time_BeginEndTime";
		public const string TextKey_BeginTimeOnly = "Time_BeginTimeOnly";
		public const string TextKey_EndTimeOnly = "Time_EndTimeOnly";
		public const string TextKey_TimeYear = "Time_TimeYear";
		public const string TextKey_TimeMonth = "Time_TimeMonth";
		public const string TextKey_TimeDay = "Time_TimeDay";
		public const string TextKey_TimeHour = "Time_TimeHour";
		public const string TextKey_TimeMinutes = "Time_TimeMinutes";
		public const string TextKey_Countdown = "Time_Countdown";

		static System.DateTime mSettedServerTime = System.DateTime.Now;
		static float mSettedRealTimeSinceStartup = 0f;

		//------------------------------------------------------------------------	
		public delegate void delOnTimerCallback( TimerCallbackData data );
		public struct TimerCallbackData
		{
			public int index;
			public System.DateTime next_check_time;
			public delOnTimerCallback callback;

			public TimerCallbackData( int idx, System.DateTime next_check_time, delOnTimerCallback callback )
			{
				this.index = idx;
				this.next_check_time = next_check_time;
				this.callback = callback;
			}

			public void Fire()
			{
				if( callback != null )
					callback( this );
			}
		}

		static int mUniqueTimerIdx = 0;
		static List<TimerCallbackData> mTimerCallbackList = new List<TimerCallbackData>();
		public static int AddTimer( System.DateTime next_check_time, delOnTimerCallback callback )
		{
			if( next_check_time <= REAL_TIME )
				return 0;

			mUniqueTimerIdx += 1;
			mTimerCallbackList.Add( new TimerCallbackData( mUniqueTimerIdx, next_check_time, callback ) );
			mTimerCallbackList = mTimerCallbackList.OrderBy( a => a.next_check_time ).ToList();

			return mUniqueTimerIdx;
		}
		public static void RemoveTimer( int idx )
		{
			mTimerCallbackList.RemoveAll( a => a.index == idx );
		}

		static void CheckTimer()
		{
			if( mTimerCallbackList.Count > 0 )
			{
				TimerCallbackData first = mTimerCallbackList[0];
				if( first.next_check_time <= REAL_TIME )
				{
					mTimerCallbackList.RemoveAt( 0 );
					first.Fire();

					CheckTimer();
				}
			}
		}

		//------------------------------------------------------------------------	
		public delegate void delegateOneSecondsTimer();
		static delegateOneSecondsTimer mOnOneSecondsTimer = null;
		public static void AddOneSecondsTimer( delegateOneSecondsTimer callback, bool first_call = true )
		{
			mOnOneSecondsTimer += callback;
			if( first_call )
				callback();
		}
		public static void RemoveOneSecondsTimer( delegateOneSecondsTimer callback )
		{
			mOnOneSecondsTimer -= callback;
		}

		static float _seconds_check_last_time = 0f;
		public static void UpdateTime()
		{
			_seconds_check_last_time += Time.unscaledDeltaTime;
			if( _seconds_check_last_time >= 1f )
			{
				_seconds_check_last_time = 0f;
				if( mOnOneSecondsTimer != null )
					mOnOneSecondsTimer();

				CheckTimer();

				if( _time_sync_enable )
				{
					if( REAL_TIME >= _time_sync_next_time )
					{
						TimeSync_DO();
					}
				}
			}
		}

		//------------------------------------------------------------------------	
		// network time sync
		public delegate bool delegateTimeSyncHandler();
		static delegateTimeSyncHandler mTimeSyncHandler = null;
		public static delegateTimeSyncHandler TimeSyncHandler { set { mTimeSyncHandler = value; } }
		static bool _time_sync_enable = false;

		public static void TimeSync_DO()
		{
			if( mTimeSyncHandler != null )
			{
				if( mTimeSyncHandler() )
					_time_sync_send_time = Time.realtimeSinceStartup;
			}

			TimeSync_NextTimeReset();
		}

		//------------------------------------------------------------------------	
		public static bool time_sync_enable
		{
			set
			{
				_time_sync_enable = value;
				if( value == false )
				{
					_time_sync_network_latency = 0f;
					_time_sync_latency_list.Clear();
				}
			}
		}
		static System.DateTime _time_sync_next_time = System.DateTime.MinValue;
		static int _time_sync_interval = 10;
		static float _time_sync_network_latency = 0f;
		static List<float> _time_sync_latency_list = new List<float>();
		static float _time_sync_send_time = 0f;

		public static void TimeSync_Received( System.DateTime server_time, int interval )
		{
			_time_sync_interval = interval;
			_time_sync_latency_list.Add( Time.realtimeSinceStartup - _time_sync_send_time );
			_time_sync_network_latency = TimeSync_LatencyReCalc();

			SERVER_TIME = server_time;
			TimeSync_NextTimeReset();
		}
		public static void TimeSync_NextTimeReset()
		{
			_time_sync_enable = true;
			_time_sync_next_time = REAL_TIME.AddSeconds( _time_sync_interval );
		}

		static float TimeSync_LatencyReCalc()
		{
			int avg_data_count = 5;
			float avg_half_lat = 0f;
			int count = _time_sync_latency_list.Count;

			if( count > avg_data_count )
			{
				_time_sync_latency_list = _time_sync_latency_list.OrderBy( t => t ).ToList();
				int mid_point = count / 2;
				float median = _time_sync_latency_list[mid_point];
				float stddev = 0f;
				for( int i = 0; i < count; i++ )
				{
					stddev += ( _time_sync_latency_list[i] - median ) * ( _time_sync_latency_list[i] - median );
				}
				stddev /= count;
				stddev = Mathf.Sqrt( stddev );

				float stdmin = median - stddev;
				float stdmax = median + stddev;
				_time_sync_latency_list.RemoveAll( t => t < stdmin || t > stdmax );
			}

			if( count > 0 )
			{
				for( int i = 0; i < _time_sync_latency_list.Count; i++ )
				{
					avg_half_lat += _time_sync_latency_list[i];
				}
				avg_half_lat /= count;
			}

			while( _time_sync_latency_list.Count > avg_data_count )
			{
				_time_sync_latency_list.RemoveAt( 0 );
			}

			Debug.LogFormat( "Network Average latency : {0}ms", Mathf.RoundToInt( avg_half_lat * 1000 ) );

			return avg_half_lat;
		}

		//------------------------------------------------------------------------
		public static System.DateTime SERVER_TIME
		{
			get { return mSettedServerTime; }
			set
			{
				mSettedServerTime = value.AddSeconds( (double)_time_sync_network_latency );
				mSettedRealTimeSinceStartup = Time.realtimeSinceStartup;
			}
		}

		public static System.DateTime REAL_TIME
		{
			get
			{
				return mSettedServerTime.AddSeconds( Time.realtimeSinceStartup - mSettedRealTimeSinceStartup );
			}
		}

		public static System.TimeSpan REAL_TIMESPAN
		{
			get { return System.TimeSpan.FromTicks( REAL_TIME.Ticks ); }
		}

		//------------------------------------------------------------------------	
		public static string ParserSeconds( float seconds, bool show_millisec = false )
		{
			System.TimeSpan time = System.TimeSpan.FromSeconds( (double)seconds );
			return Parser( time, show_millisec );
		}

		//------------------------------------------------------------------------	
		public static string ParserMinutes( int minutes, bool show_millisec = false )
		{
			System.TimeSpan time = System.TimeSpan.FromMinutes( minutes );
			return Parser( time, show_millisec );
		}

		//------------------------------------------------------------------------	
		public static string ParserDuration( int seconds )
		{
			System.TimeSpan time = System.TimeSpan.FromSeconds( seconds );

			string txt = "";
			if( time.Days > 0 )
				txt = string.Format( "{0}", ParserByDay( time.Days ) );

			if( time.Hours > 0 )
				txt = string.Format( "{0} {1}", txt, ParserByHour( time.Hours ) );

			if( time.Minutes > 0 )
				txt = string.Format( "{0} {1}", txt, ParserByMin( time.Minutes ) );

			if( time.Seconds > 0 )
				txt = string.Format( "{0} {1}", txt, ParserBySec( time.Seconds ) );

			return txt;
		}

		//------------------------------------------------------------------------
		public static string ParserAuto( System.TimeSpan time, bool show_millisec = false )
		{
			if( time.Days > 0 )
			{
				if( time.Hours > 0 )
					return ParserByDayHour( time.Days, time.Hours );
				else
					return ParserByDay( time.Days );
			}
			else if( time.Hours > 0 )
			{
				if( time.Minutes > 0 )
					return ParserByHourMin( time.Hours, time.Minutes );
				else
					return ParserByHour( time.Hours );
			}
			else if( time.Minutes > 0 )
			{
				if( time.Seconds > 0 )
					return ParserByMinSec( time.Minutes, time.Seconds, time.Milliseconds, show_millisec );
				else
					return ParserByMin( time.Minutes );
			}
			else if( time.Seconds >= 0 )
			{
				return ParserBySec( time.Seconds, time.Milliseconds, show_millisec );
			}

			return ParserBySec( 0 );
		}

		//------------------------------------------------------------------------
		public static string ParserRemainTime( System.DateTime end_time, bool show_millisec = false )
		{
			return Parser( ( end_time - REAL_TIME ), show_millisec );
		}

		public static float ParserRemainHours( System.DateTime end_time )
		{
			return (float)( end_time - REAL_TIME ).TotalHours;
		}

		public static string Parser( System.TimeSpan time, bool show_millisec = false )
		{
			if( time.Days > 0 )
			{
				return ParserByDayHour( time.Days, time.Hours );
			}
			else if( time.Hours > 0 )
			{
				return ParserByHourMin( time.Hours, time.Minutes );
			}
			else if( time.Minutes > 0 )
			{
				return ParserByMinSec( time.Minutes, time.Seconds, time.Milliseconds, show_millisec );
			}
			else if( time.Seconds >= 0 )
			{
				return ParserBySec( time.Seconds, time.Milliseconds, show_millisec );
			}

			return ParserBySec( 0 );
		}

		//------------------------------------------------------------------------
		public static string ParserShort( System.TimeSpan time, bool show_millisec = false )
		{
			if( time.Days > 0 )
				return ParserByDay( time.Days );
			else if( time.Hours > 0 )
				return ParserByHour( time.Hours );
			else if( time.Minutes > 0 )
				return ParserByMin( time.Minutes );
			else if( time.Seconds >= 0 )
				return ParserBySec( time.Seconds, time.Milliseconds, show_millisec );

			return ParserBySec( 0 );
		}

		//------------------------------------------------------------------------	
		public static string ParserTimeFormat( float floating_time )
		{
			return ParserTimeFormat( System.TimeSpan.FromSeconds( floating_time ) );
		}
		public static string ParserTimeFormat( System.TimeSpan time )
		{
			if( time.Days > 0 )
				return string.Format( "{0}d {1}:{2:00}:{3:00}", time.Days, time.Hours, time.Minutes, time.Seconds );
			else if( time.Hours > 0 )
				return string.Format( "{0}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds );
			else if( time.Minutes > 0 )
				return string.Format( "{0:00}:{1:00}", time.Minutes, time.Seconds );
			else if( time.Seconds > 0 )
				return string.Format( "0:{0:00}", time.Seconds );

			return "0:0";
		}
		
		//------------------------------------------------------------------------
		static string GetTimeText( string key, params object[] parms )
		{
			return I18NTextManagerUnity.GetText( key, parms );
		}

		//------------------------------------------------------------------------	
		public enum eParseDatetimFlag
		{
			None = 0x0000,
			YEAR = 0x0001,
			MONTH = 0x0002,
			DAY = 0x0004,
			HOUR = 0x0008,
			MINUTES = 0x0010,

			YEAR_CHECK = 0x0100,
			HOUR_CHECK = 0x0200,
			MINUTES_CHECK = 0x0400,

			WITH_SPACE = 0x1000,

			ALL = 0xFFFF,
		}
		static bool ContainDatetimeFlag( eParseDatetimFlag flag, eParseDatetimFlag check_flag )
		{
			return ( ( flag & check_flag ) != 0 );
		}
		static System.Text.StringBuilder tmp_time_str = new System.Text.StringBuilder();
		public static string ParseDatetimeGracefull( System.DateTime datetime, eParseDatetimFlag flag )
		{
			int year = datetime.Year;
			int month = datetime.Month;
			int day = datetime.Day;
			int hour = datetime.Hour;
			int minutes = datetime.Minute;

			tmp_time_str.Clear();

			string space_str = ( ContainDatetimeFlag( flag, eParseDatetimFlag.WITH_SPACE ) ? " " : "" );

			if( ContainDatetimeFlag( flag, eParseDatetimFlag.YEAR ) )
			{
				if( ContainDatetimeFlag( flag, eParseDatetimFlag.YEAR_CHECK ) )
				{
					if( year != REAL_TIME.Year )
						tmp_time_str.Append( GetTimeText( TextKey_TimeYear, year ) ).Append( space_str );
				}
				else
				{
					tmp_time_str.Append( GetTimeText( TextKey_TimeYear, year ) ).Append( space_str );
				}
			}

			if( ContainDatetimeFlag( flag, eParseDatetimFlag.MONTH ) )
				tmp_time_str.Append( GetTimeText( TextKey_TimeMonth, month ) ).Append( space_str );

			if( ContainDatetimeFlag( flag, eParseDatetimFlag.DAY ) )
				tmp_time_str.Append( GetTimeText( TextKey_TimeDay, day ) );

			if( ContainDatetimeFlag( flag, eParseDatetimFlag.HOUR ) )
			{
				if( ContainDatetimeFlag( flag, eParseDatetimFlag.HOUR_CHECK ) )
				{
					if( hour > 0 || minutes > 0 )
						tmp_time_str.Append( space_str ).Append( GetTimeText( TextKey_TimeHour, hour ) );
				}
				else
					tmp_time_str.Append( space_str ).Append( GetTimeText( TextKey_TimeHour, hour ) );
			}

			if( ContainDatetimeFlag( flag, eParseDatetimFlag.MINUTES ) )
			{
				if( ContainDatetimeFlag( flag, eParseDatetimFlag.MINUTES_CHECK ) )
				{
					if( minutes > 0 )
						tmp_time_str.Append( space_str ).Append( GetTimeText( TextKey_TimeMinutes, minutes ) );
				}
				else
					tmp_time_str.Append( space_str ).Append( GetTimeText( TextKey_TimeMinutes, minutes ) );
			}

			return tmp_time_str.ToString();
		}

		//------------------------------------------------------------------------
		public static string ParserFullTime( System.DateTime date_time )
		{
			return GetTimeText( TextKey_FullTimeHour, date_time.Year, date_time.Month, date_time.Day, date_time.Hour );
		}
		public static string ParserFullTimeMinute( System.DateTime date_time )
		{
			return GetTimeText( TextKey_FullTimeMinute, date_time.Year, date_time.Month, date_time.Day, date_time.Hour, date_time.Minute );
		}

		//------------------------------------------------------------------------	
		public static string ParseDayOfWeek( System.DayOfWeek day_of_week )
		{
			return GetTimeText( string.Format( "Time_DayOfWeek_{0}", day_of_week.ToString() ) );
		}

		//------------------------------------------------------------------------
		public static string ParserByMonthDay( int month, int day )
		{
			return GetTimeText( TextKey_Month_Day, month, day );
		}

		//------------------------------------------------------------------------
		public static string ParserByDayHour( int day, int hour )
		{
			return GetTimeText( TextKey_Day_Hour, day, hour );
		}

		//------------------------------------------------------------------------
		public static string ParserByHourMin( int hour, int min )
		{
			return GetTimeText( TextKey_Hour_Min, hour, min );
		}

		//------------------------------------------------------------------------
		public static string ParserByMinSec( int seconds )
		{
			return ParserByMinSec( (double)seconds );
		}
		public static string ParserByMinSec( double seconds, bool show_millisec = false )
		{
			System.TimeSpan time = System.TimeSpan.FromSeconds( seconds );
			return ParserByMinSec( (int)time.TotalMinutes, time.Seconds, time.Milliseconds, show_millisec );
		}
		public static string ParserByMinSec( int min, int sec, int millisec = 0, bool show_millisec = false )
		{
			if( show_millisec )
				return GetTimeText( TextKey_Min_SecondMilli, min, (float)( sec + ( (float)millisec / 1000f ) ) );
			else
				return GetTimeText( TextKey_Min_Second, min, sec );
		}

		//------------------------------------------------------------------------
		public static string ParserByMonth( int month )
		{
			return GetTimeText( TextKey_Month, month );
		}

		//------------------------------------------------------------------------
		public static string ParserByDay( int day )
		{
			return GetTimeText( TextKey_Day, day );
		}

		//------------------------------------------------------------------------
		public static string ParserByHour( int hour )
		{
			return GetTimeText( TextKey_Hour, hour );
		}

		//------------------------------------------------------------------------
		public static string ParserByMin( int min )
		{
			return GetTimeText( TextKey_Min, min );
		}

		//------------------------------------------------------------------------
		public static string ParserBySec( int sec, int milli_sec = 0, bool show_millisec = false )
		{
			if( show_millisec )
			{
				return GetTimeText( TextKey_SecondMilli, (float)( sec + ( (float)milli_sec / 1000f ) ) );
			}
			else
				return GetTimeText( TextKey_Second, sec );
		}

		//------------------------------------------------------------------------	
		public static string TimeSubscriptText( float time_value )
		{
			int seconds = (int)time_value;
			float milliseconds = ( time_value - seconds ) * 10f;

			if( milliseconds > 9f )
				return string.Format( "{0}.[sub]{1}[/sub]", seconds, (int)milliseconds );
			else if( milliseconds > 0f )
				return string.Format( "{0}.[sub]{1}[/sub]", seconds, Mathf.CeilToInt( milliseconds ) );
			else
				return seconds.ToString();
		}

		//------------------------------------------------------------------------	
		public static string GetBetweenTimeText( System.DateTime begin_time, System.DateTime end_time )
		{
			if( begin_time != System.DateTime.MinValue && end_time != System.DateTime.MaxValue )
				return GetTimeText( TextKey_BeginEndTime, ParserFullTimeMinute( begin_time ), ParserFullTimeMinute( end_time ) );
			else if( begin_time != System.DateTime.MinValue )
				return GetTimeText( TextKey_BeginTimeOnly, ParserFullTimeMinute( begin_time ) );
			else if( end_time != System.DateTime.MaxValue )
				return GetTimeText( TextKey_EndTimeOnly, ParserFullTimeMinute( end_time ) );

			return "";
		}

		//------------------------------------------------------------------------		
		public static int CalcDaysFromNow( System.DateTime target_time )
		{
			return ( REAL_TIME.Date - target_time.Date ).Days;
		}

		public static int CalcDays( System.DateTime src_time, System.DateTime target_time )
		{
			return ( src_time.Date - target_time.Date ).Days;
		}
		public static System.DateTime GetNextMonthlyBeginTime( System.DateTime curr_time )
		{
			System.DateTime next_month = curr_time.Date.AddMonths( 1 );
			return new System.DateTime( next_month.Year, next_month.Month, 1 );
		}

		//------------------------------------------------------------------------	
		public static float CalcRemainTimeProgress( System.DateTime begin_time, System.DateTime end_time, bool is_fill )
		{
			System.TimeSpan total_time_span = end_time - begin_time;
			System.TimeSpan left_time_span = end_time - REAL_TIME;
			float left_time = (float)( total_time_span - left_time_span ).TotalSeconds;
			float total_time = (float)total_time_span.TotalSeconds;

			if( is_fill )
				return ( left_time / total_time );
			else
				return ( 1f - ( left_time / total_time ) );
		}

		//------------------------------------------------------------------------	
		public static string ParserCountdownNow( System.DateTime time, bool with_parenthesis = false )
		{
			return ParserCountdown( time - REAL_TIME );
		}
		public static string ParserCountdown( System.TimeSpan time, bool with_parenthesis = false )
		{
			if( with_parenthesis )
				return string.Format( "( {0} )", GetTimeText( TextKey_Countdown, (int)time.TotalHours, (int)time.Minutes, (int)time.Seconds ) );
			else
				return GetTimeText( TextKey_Countdown, (int)time.TotalHours, (int)time.Minutes, (int)time.Seconds );
		}

		//------------------------------------------------------------------------	
		public static float SetTimeScale( float value )
		{
			Time.timeScale = value;
			return Time.timeScale;
		}
	}
}