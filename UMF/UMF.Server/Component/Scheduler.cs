//////////////////////////////////////////////////////////////////////////
//
// Scheduler
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
using UMF.Core;

namespace UMF.Server
{
	public class Scheduler : Singleton<Scheduler>
	{
		//------------------------------------------------------------------------		
		public class ScheduleData : IComparable<ScheduleData>
		{
			public ScheduleData( DateTime time, Delegate OnSchedule, object[] parms )
			{
				this.m_Time = time;
				this.m_OnSchedule = OnSchedule;
				m_ScheduleIndex = ++s_ScheduleIndex;
				this.m_Params = parms;
			}

			//------------------------------------------------------------------------		
			public int CompareTo( ScheduleData obj )
			{
				return m_Time.CompareTo( obj.m_Time );
			}

			//------------------------------------------------------------------------		
			string GetParamString()
			{
				if( m_Params == null )
					return "()";

				string str = "(";
				bool bFirst = true;

				foreach( object param in m_Params )
				{
					if( bFirst == true )
						bFirst = false;
					else
						str += ",";
					str += param.ToString();
				}
				str += ")";
				return str;
			}

			//------------------------------------------------------------------------		
			public void Fire()
			{
				m_bFired = true;
				if( m_OnSchedule.Target != null )
					Log.Write( "Fire Schedule : {0}", m_OnSchedule.Target.GetType().Name + "." + m_OnSchedule.Method.Name + GetParamString() );
				else
					Log.Write( "Fire Schedule : {0}", m_OnSchedule.Method.Name + GetParamString() );
				m_OnSchedule.DynamicInvoke( m_Params );
			}

			DateTime m_Time;
			public DateTime Time { get { return m_Time; } }

			long m_ScheduleIndex;
			public long ScheduleIndex { get { return m_ScheduleIndex; } }

			object[] m_Params;
			Delegate m_OnSchedule;
			bool m_bFired = false;
			public bool Fired { get { return m_bFired; } }

			static long s_ScheduleIndex = 0;
		}

		List<ScheduleData> m_Scheduler = new List<ScheduleData>();

		//------------------------------------------------------------------------		
		public Scheduler()
		{
		}		

		//------------------------------------------------------------------------		
		public ScheduleData AddSchedule( DateTime time, Delegate OnSchedule, params object[] parms )
		{
			ScheduleData schedule = new ScheduleData( time, OnSchedule, parms );
			m_Scheduler.Add( schedule );
			m_Scheduler.Sort();
			return schedule;
		}

		//------------------------------------------------------------------------		
		public ScheduleData AddSchedule( DateTime time, Delegate OnSchedule )
		{
			return AddSchedule( time, OnSchedule, new object[0] );
		}

		//------------------------------------------------------------------------		
		public ScheduleData AddSchedule( TimeSpan span, Delegate OnSchedule, params object[] parms )
		{
			return AddSchedule( DateTime.Now + span, OnSchedule, parms );
		}

		//------------------------------------------------------------------------		
		public ScheduleData AddSchedule( TimeSpan span, Delegate OnSchedule )
		{
			return AddSchedule( DateTime.Now + span, OnSchedule );
		}

		//------------------------------------------------------------------------		
		public void RemoveSchedule( ScheduleData data )
		{
			if( data.Fired == true )
				return;

			if( m_Scheduler.Remove( data ) == false )
				Log.WriteError( "Can't find schedule({0}) in RemoveSchedule", data.ScheduleIndex );
		}

		//------------------------------------------------------------------------		
		public void CheckSchedule( bool use_parallel )
		{
			while( m_Scheduler.Count > 0 )
			{
				if( m_Scheduler[0].Time > DateTime.Now )
					return;

				ScheduleData data = m_Scheduler[0];
				m_Scheduler.RemoveAt( 0 );
				data.Fire();
			}
		}
	}
}