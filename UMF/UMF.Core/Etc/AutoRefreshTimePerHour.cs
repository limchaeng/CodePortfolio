//////////////////////////////////////////////////////////////////////////
//
// AutoRefreshTimePerHour
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
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace UMF.Core
{
	public class AutoRefreshTimePerHour
	{
		public delegate void delSetMinutesCallback( bool bNetSet );

		delSetMinutesCallback mResetTimeCallback = null;
		public delSetMinutesCallback ResetTimeCallback { set { mResetTimeCallback = value; } }
		List<int> mMinutesList = null;

		public DateTime LastTime { get; set; } = DateTime.MinValue;
		public DateTime NextTime { get; set; } = DateTime.MinValue;

		public AutoRefreshTimePerHour( params int[] minutes_list )
		{
			mResetTimeCallback = null;
			if( minutes_list == null || minutes_list.Length <= 0 )
				mMinutesList = new List<int>() { 0 };
			else
				mMinutesList = minutes_list.OrderBy( n => n ).ToList();
		}

		//------------------------------------------------------------------------		
		/// <summary>
		///   parse_time = comma separate
		/// </summary>
		public void SetRefresTimePerHour( string parse_time )
		{
			List<int> new_times = null;
			string[] minutes_txt = parse_time.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
			if( minutes_txt != null && minutes_txt.Length > 0 )
			{
				foreach( string min_text in minutes_txt )
				{
					int min = StringUtil.SafeParse<int>( min_text, -1 );
					if( min != -1 )
					{
						if( new_times == null )
							new_times = new List<int>();

						if( new_times.Contains( min ) == false )
							new_times.Add( min );
					}
				}
			}

			if( new_times != null )
			{
				mMinutesList = new_times.OrderBy( n => n ).ToList();
				if( mResetTimeCallback != null )
					mResetTimeCallback( true );
				return;
			}

			if( mResetTimeCallback != null )
				mResetTimeCallback( false );
		}

		//------------------------------------------------------------------------		
		public DateTime ResetNextTime( DateTime curr_time )
		{
			LastTime = curr_time;
			NextTime = DateTime.MaxValue;
			if( mMinutesList == null || mMinutesList.Count <= 0 )
				return NextTime;

			int curr_minutes = curr_time.Minute;
			foreach( int minutes in mMinutesList )
			{
				if( minutes > curr_minutes )
				{
					NextTime = curr_time.Date.AddHours( curr_time.Hour ).AddMinutes( minutes );
					return NextTime;
				}
			}

			NextTime = curr_time.Date.AddHours( curr_time.Hour + 1 ).AddMinutes( mMinutesList[0] );
			return NextTime;
		}

		//------------------------------------------------------------------------		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( $"Last:{LastTime} Next:{NextTime} Minutes:" );
			foreach( int n in mMinutesList )
			{
				sb.Append( $"{n}," );
			}

			return sb.ToString();
		}
	}
}
