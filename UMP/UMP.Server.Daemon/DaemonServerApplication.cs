//////////////////////////////////////////////////////////////////////////
//
// DaemonServerApplication
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

using UMP.CSCommon;
using System.Collections.Generic;
using System.Diagnostics;
using UMF.Core;
using UMF.Server;
using System;
using UMF.Server.Core;

namespace UMP.Server.Daemon
{
	//------------------------------------------------------------------------	
	public class DaemonServerApplication : UMPServerApplication
	{
		public class DaemonApplicationConfig : ApplicationConfig
		{
			public string LoginServerExec = "";
			public string GameServerExec = "";
			public string RelayServerExec = "";
			public string ContentsServerExec = "";

			public bool UsePerformanceCounter = true;

			Dictionary<eServerType, string> mExecNameDic = new Dictionary<eServerType, string>();

			protected override void ConfigLoad()
			{
				base.ConfigLoad();

				mExecNameDic.Clear();
				mExecNameDic.Add( eServerType.Login, LoginServerExec );
				mExecNameDic.Add( eServerType.Game, GameServerExec );
				mExecNameDic.Add( eServerType.Relay, RelayServerExec );
				mExecNameDic.Add( eServerType.Contents, ContentsServerExec );
			}

			/// <summary>
			///   return string[0]=exec file , string[1]=args
			/// </summary>
			public string[] GetExecFile(eServerType server_type)
			{
				string exec;
				if( mExecNameDic.TryGetValue( server_type, out exec ) )
				{
					if( string.IsNullOrEmpty( exec ) )
						return null;

					string[] info = new string[2] { "", "" };

					string[] splits = exec.Split( ' ' );
					if( splits.Length > 0 )
						info[0] = splits[0];

					if( splits.Length > 1 )
						info[1] = splits[1];

					return info;
				}

				return null;
			}
		}

		protected List<ProcessData> mExecutedProcessList = new List<ProcessData>();

		protected ErrorLogTracer mErrorLogTracer = null;

		// counter
		UMPerformanceCounter mPerformanceCounter = null;
		public int m_CheckProcessTimeoutSeconds = 30;
		public int m_CheckProcessTimeoutNotRespondingCountingTimout = 5;
		public int m_CheckProcessTimeoutCount = 5;
		public eDaemonDumpSetType m_ProcessKillDumpType = eDaemonDumpSetType.full;

		DaemonApplicationConfig mConfig = null;

		//------------------------------------------------------------------------		
		public DaemonServerApplication( string server_name, eServiceType service_type, string config_file, string[] args ) 
			: base( server_name, eServerType.Daemon, service_type, config_file, args )
		{
			if( mConfig.UsePerformanceCounter )
			{
				mPerformanceCounter = new UMPerformanceCounter();
			}

			mExecutedProcessList.Add( new ProcessData( Process.GetCurrentProcess(), true, mConfig.UsePerformanceCounter ) );

			foreach( string arg in args )
			{
				eServerType type;
				if( Enum.TryParse<eServerType>( arg, out type ) == true )
					ExecuteProcess( type );
			}

			AddUpdater( CheckProcessesUpdate );
		}

		//------------------------------------------------------------------------
		public override ApplicationConfig ConstructConfig()
		{
			mConfig = new DaemonApplicationConfig();
			return mConfig;
		}

		//------------------------------------------------------------------------
		protected override string MakeTitleMessage()
		{
			return string.Format( "tick:{0:0.0}, deadTimeout:{1}/{2} process:{3}", lastTick, m_CheckProcessTimeoutSeconds, m_CheckProcessTimeoutCount, mExecutedProcessList.Count );
		}

		//------------------------------------------------------------------------		
		public void ExecuteProcesses( List<eServerType> processNames )
		{
			foreach( eServerType type in processNames )
			{
				ExecuteProcess( type );
			}
		}

		//------------------------------------------------------------------------
		public void ExecuteProcess( eServerType type )
		{
			if( type == eServerType.Daemon )
				return;

			string[] exec_file = mConfig.GetExecFile( type );

			if( exec_file == null )
			{
				Log.WriteWarning( $"ExecuteProcess : {type} type not found!"  );
				return;
			}

			if( System.IO.File.Exists(exec_file[0]) == false )
			{
				Log.WriteWarning( $"ExecuteProcess : {exec_file} file not found!" );
				return;
			}

			try
			{
				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.FileName = exec_file[0];
				startInfo.WindowStyle = ProcessWindowStyle.Normal;
				startInfo.UseShellExecute = true;
				startInfo.ErrorDialog = false;
				startInfo.Arguments = exec_file[1];

				Process process = Process.Start( startInfo );
				mExecutedProcessList.Add( new ProcessData( process, false, mConfig.UsePerformanceCounter ) );
				Log.WriteImportant( $"{type} ({exec_file[0]}) has been started." );
			}
			catch( System.Exception ex )
			{
				Log.WriteError( ex.ToString() );
			}
		}

		//------------------------------------------------------------------------
		public List<string> ExecuteProcessDumpAll( eDaemonDumpSetType set_type )
		{
			List<string> dmp_name_list = new List<string>();
			foreach( ProcessData processData in mExecutedProcessList )
			{
				string dmp_file = string.Format( "{0}/{1}_{2:yyyyMMdd_HHmmss}.dmp", Log.LOG_PATH, processData.process_name, DateTime.Now );
				MinidumpWriter.MiniDumpType dump_type = MinidumpWriter.MiniDumpType.Normal;
				if( set_type == eDaemonDumpSetType.full )
				{
					dump_type |= MinidumpWriter.MiniDumpType.WithFullMemory |
						MinidumpWriter.MiniDumpType.WithHandleData |
						MinidumpWriter.MiniDumpType.WithProcessThreadData |
						MinidumpWriter.MiniDumpType.WithThreadInfo;
				}

				MinidumpWriter.MakeDump( dmp_file, processData.process.Id, dump_type );
				Log.Write( "Dump :{0}", dmp_file );

				dmp_name_list.Add( dmp_file );
			}

			return dmp_name_list;
		}


		//------------------------------------------------------------------------		
		protected void CheckProcessesUpdate()
		{
			if( IsShutdown )
				return;

			List<ProcessData> removes = new List<ProcessData>();

			foreach( ProcessData processData in mExecutedProcessList )
			{
				if( processData.bDaemon == false )
				{
					if( processData.process.HasExited == true )
					{
						if( IsMaintenance == false )
						{
							string exited_message = processData.process_name + " was exited : " + processData.process.ExitCode.ToString();
							Log.WriteError( exited_message );

							SendServerNotification( exited_message );
							processData.process.Start();
						}
						else
						{
							Log.Write( processData.process_name + " was exited(maintenance) : " + processData.process.ExitCode.ToString() );
							removes.Add( processData );
						}
					}
					else
					{
						if( IsMaintenance == false )
						{
							if( processData.title_check_time < DateTime.Now )
							{
								processData.process.Refresh();
								if( processData.process_title == processData.process.MainWindowTitle )
								{
									processData.not_reponding_count++;
									if( processData.not_reponding_count > m_CheckProcessTimeoutCount )
									{
										if( m_ProcessKillDumpType != eDaemonDumpSetType.none )
										{
											string dmp_file = string.Format( "{0}/kill_{1}_{2:yyyyMMdd_HHmmss}.dmp", Log.LOG_PATH, processData.process_name, DateTime.Now );
											MinidumpWriter.MiniDumpType dump_type = MinidumpWriter.MiniDumpType.Normal;
											if( m_ProcessKillDumpType == eDaemonDumpSetType.full )
											{
												dump_type |= MinidumpWriter.MiniDumpType.WithFullMemory |
													MinidumpWriter.MiniDumpType.WithHandleData |
													MinidumpWriter.MiniDumpType.WithProcessThreadData |
													MinidumpWriter.MiniDumpType.WithThreadInfo;
											}
											MinidumpWriter.MakeDump( dmp_file, processData.process.Id, dump_type );
											Log.Write( "Dump :{0}", dmp_file );
										}

										string kill_msg = "Kill (not responding): " + processData.process_name + ":" + processData.process_title;
										kill_msg += "\n-->" + processData.GetState();
										Log.WriteError( kill_msg );
										processData.process.Kill();

										SendServerNotification( kill_msg );
									}
									else
									{
										string log_msg = "NotResponding Checked:" + processData.not_reponding_count.ToString() + ":" + processData.process_title;
										log_msg += "\n-->" + processData.GetState();
										Log.WriteError( log_msg );
										processData.title_check_time = DateTime.Now.AddSeconds( m_CheckProcessTimeoutNotRespondingCountingTimout );
									}
								}
								else
								{
									processData.title_check_time = DateTime.Now.AddSeconds( m_CheckProcessTimeoutSeconds );
									processData.process_title = processData.process.MainWindowTitle;
									processData.not_reponding_count = 0;
								}
							}
						}
					}
				}
			}

			foreach( ProcessData processData in removes )
			{
				mExecutedProcessList.Remove( processData );
			}
		}

		//------------------------------------------------------------------------
		public string GetProcessStatus()
		{
			if( IsShutdown )
				return "shutdown";

			System.Text.StringBuilder result = new System.Text.StringBuilder();
			foreach( ProcessData processData in mExecutedProcessList )
			{
				result.AppendLine( processData.GetState() );
			}

			Log.Write( result.ToString() );

			return result.ToString();
		}
	}
}
