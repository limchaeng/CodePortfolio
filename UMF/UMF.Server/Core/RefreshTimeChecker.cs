//////////////////////////////////////////////////////////////////////////
//
// RefreshTimeChecker
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
using UMF.Core;

namespace UMF.Server
{
	public class RefreshTimeChecker
	{
		public delegate void delSetMinutesCallback( bool bNetSet );

		delSetMinutesCallback mCallback = null;
		public delSetMinutesCallback Callback { set { mCallback = value; } }
		List<int> mMinutesList = null;

		//------------------------------------------------------------------------		
		public RefreshTimeChecker( params int[] minutes_list )
		{
			mCallback = null;
			if( minutes_list == null || minutes_list.Length <= 0 )
				mMinutesList = new List<int>() { 0 };
			else
				mMinutesList = minutes_list.OrderBy( n => n ).ToList();
		}

		//------------------------------------------------------------------------		
		// comma separate
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
				if( mCallback != null )
					mCallback( true );
				return;
			}

			if( mCallback != null )
				mCallback( false );
		}

		//------------------------------------------------------------------------		
		public DateTime GetNextTime( DateTime curr_time )
		{
			if( mMinutesList == null || mMinutesList.Count <= 0 )
				return curr_time;

			int curr_minutes = curr_time.Minute;
			foreach( int minutes in mMinutesList )
			{
				if( minutes > curr_minutes )
				{
					return curr_time.Date.AddHours( curr_time.Hour ).AddMinutes( minutes );
				}
			}

			return curr_time.Date.AddHours( curr_time.Hour + 1 ).AddMinutes( mMinutesList[0] );
		}

		//------------------------------------------------------------------------		
		public override string ToString()
		{
			string s = "";
			foreach( int n in mMinutesList )
			{
				s += n.ToString() + ",";
			}

			return s;
		}
	}
}
