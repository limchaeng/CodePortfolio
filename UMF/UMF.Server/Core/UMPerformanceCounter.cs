//////////////////////////////////////////////////////////////////////////
//
// UMPerformanceCounter
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

using System.Diagnostics;
using System;
using System.Text;

namespace UMF.Server.Core
{
	public class UMPerformanceCounter
	{
		protected PerformanceCounter cpuCounter;
		protected PerformanceCounter workingSetCounter;
		protected PerformanceCounter handleCounter;
		protected PerformanceCounter threadCounter;
		protected PerformanceCounter freeMemCounter;

		protected PerformanceCounter cpuTotalCounter;
		protected PerformanceCounter memoryTotalCounter;

		protected string mProcessName;

		public UMPerformanceCounter(string process_name)
		{
			mProcessName = process_name;

			cpuCounter = new PerformanceCounter( "Process", "% Processor Time", process_name, true );
			workingSetCounter = new PerformanceCounter( "Process", "Working Set - Private", process_name, true );
			handleCounter = new PerformanceCounter( "Process", "Handle Count", process_name, true );
			threadCounter = new PerformanceCounter( "Process", "Thread Count", process_name, true );
			freeMemCounter = new PerformanceCounter( "Memory", "Available MBytes", true );
		}

		public UMPerformanceCounter()
		{
			mProcessName = "";

			cpuTotalCounter = new PerformanceCounter( "Processor", "% Processor Time", "_Total", true );
			memoryTotalCounter = new PerformanceCounter( "Memory", "committed bytes", true );
		}

		//------------------------------------------------------------------------
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( "#" );
			if( cpuTotalCounter != null )
				sb.Append( string.Format( " cpuTotal:{0}%", cpuTotalCounter.NextValue() ) );

			if( memoryTotalCounter != null )
				sb.Append( string.Format( " memTotal:{0}KB", memoryTotalCounter.NextValue() / 1024f ) );

			if( string.IsNullOrEmpty( mProcessName ) == false )
			{
				sb.Append( string.Format( " [{0}]", mProcessName ) );
				sb.Append( string.Format( " cpu:{0}% WS:{1}KB FREE:{2}MB HC:{3} TC:{4}",
					cpuCounter.NextValue(),
					( workingSetCounter.NextValue() / 1024f ),
					freeMemCounter.NextValue(),				
					handleCounter.NextValue(),
					threadCounter.NextValue()
				) );
			}

			return sb.ToString();
		}
	}
}
