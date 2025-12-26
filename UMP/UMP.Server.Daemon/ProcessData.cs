//////////////////////////////////////////////////////////////////////////
//
// ProcessData
// 
// Created by LCY.
//
// Copyright 2022 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using UMF.Server.Core;

namespace UMP.Server.Daemon
{
	//------------------------------------------------------------------------	
	public class ProcessData
	{
		public ProcessData( Process process, bool bDaemon, bool use_performnace_counter )
		{
			this.bDaemon = bDaemon;
			this.process = process;
			process_name = process.ProcessName;

			if( use_performnace_counter )
				mPerformanceCount = new UMPerformanceCounter( process_name );
		}

		public bool bDaemon = false;
		public Process process { get; private set; }
		public string process_name, process_title;
		public int not_reponding_count;
		public DateTime title_check_time = DateTime.Now.AddSeconds( 30 );

		public UMPerformanceCounter mPerformanceCount = null;

		//------------------------------------------------------------------------		
		public string GetState()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine( string.Format( "== {0}", process_name ) );
			try
			{
				if( mPerformanceCount != null )
					sb.AppendLine( mPerformanceCount.ToString() );

				if( process != null )
				{
					if( process.Responding )
						sb.AppendLine( "# Responding" );
					else
						sb.AppendLine( "# NOT Responding" );
				}
			}
			catch( System.Exception ex )
			{
				sb.AppendLine( ex.ToString() );
			}

			return sb.ToString();
		}
	}
}
