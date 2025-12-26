//////////////////////////////////////////////////////////////////////////
//
// ServerApplication
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

using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using Newtonsoft.Json;
using UMF.Core;
using UMF.Net;

namespace UMF.Server
{
	//------------------------------------------------------------------------	
	public class ServerApplication
	{
		public class ApplicationConfig : EnvConfig
		{
			public int WorldIDN = 0;
			public bool FileLog = true;
			public string UserLogName = "UserLog";
			public int LogfileSize = 10000000;
			public bool UseNLog = false;
			public bool UnuseTLS12 = false;			
			public bool UseParallel = true;
		}

		protected ApplicationConfig mApplicationConfig = null;
		public ApplicationConfig GetApplicationConfig { get { return mApplicationConfig; } }
		protected string mServerName = "";
		protected string mServerCurrDir = "";
		protected Version mServerVersion = new Version( 0, 0, 0, 1 );
		protected string mConsolTitlePrefix = "";
		protected bool mIsMaintenance = false;
		protected int mLogIndex = 0;
		protected string mLogFilename, mCurrentLogFilename, mCurrentLogFilepath;
		protected DateTime mServerStartTime;
		protected string mServerStartTimeString;

		public string CurrentLogFilename { get { return mCurrentLogFilename; } }
		public string CurrentLogFilepath { get { return mCurrentLogFilepath; } }

		public delegate void ChangeCurrentLogFilenameDelegate();
		public ChangeCurrentLogFilenameDelegate _ChangeCurrentLogFilename = null;

		public bool IsShutdown { get; protected set; } = false;

		public string ServerName { get { return mServerName; } }
		public Version ServerVersion { get { return mServerVersion; } set { mServerVersion = value; bRefreshTitleString = true; } }
		public bool IsMaintenance
		{
			set { mIsMaintenance = value; bRefreshTitleString = true; }
			get { return mIsMaintenance; }
		}
		protected bool bStarted = false;

		[DllImport( "winmm.dll" )]
		internal static extern uint timeBeginPeriod( uint period );

		StreamWriter writer, writerError;

		public delegate void UpdateDelegate();
		public List<Listener> m_Listeners = new List<Listener>();
		public List<Connector> m_Connectors = new List<Connector>();
		public List<UpdateDelegate> m_Updaters = new List<UpdateDelegate>();

		protected bool bLoop = true;
		public void StopLoop() { bLoop = false; }

		protected string mGlobalTypeString = "";
		protected string mEnvTypeString = "";
 		protected string mServerTypeString = "";
 		protected string mServiceTypeString = "";

		public bool AlwaysConsoleWrite { get; set; }
		protected bool mTitlePrint = true;

		long guid_long = 0;
		public long GUID_LONG { get { return guid_long; } }
		string guid_str = "";
		public string GUID_STR { get { return guid_str; } }

		protected Logger mNLOGDEBUG = null;
		protected Logger mNLOG = null;
		protected Logger mUSERLOG = null;
		public bool IS_USERLOG { get { return mUSERLOG != null; } }

		protected UMFRandom mRandom = null;
		protected INotification mNotificationModule = null;
		public INotification NotificationModule { get => mNotificationModule; set => mNotificationModule = value; }

		// server
		public int ServerNetIndex { get; set; }
		public string ServerIP { get; set; }

		protected Dictionary<string, string> mArgsDic = new Dictionary<string, string>();

		//------------------------------------------------------------------------		
		public ServerApplication( string server_name, string server_type, string service_type, string config_file, string[] args )
		{
			mRandom = UMFRandom.Instance;

			mArgsDic.Clear();
			if( args != null )
			{
				foreach (string arg in args)
				{
					string[] splits = arg.Split( '=' );
					if( splits != null && splits.Length > 0 )
					{
						string key = splits[0].Trim();
						string value = "";
						if( splits.Length > 1 )
							value = splits[1].Trim();
						if( mArgsDic.ContainsKey( key ) == false )
							mArgsDic.Add( key, value );
					}
				}
			}

			ServerNetIndex = 0;
			ServerIP = "";

			mGlobalTypeString = GlobalConfig.GlobalType;
			mEnvTypeString = GlobalConfig.EnvironmentType;
			mServerTypeString = server_type;
			mServiceTypeString = service_type;

			AlwaysConsoleWrite = true;

			IsShutdown = false;

			System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo( "en-US" );

			string curr_dir = Directory.GetCurrentDirectory();

			try
			{
				int last_idx = curr_dir.LastIndexOf( "\\" );
				last_idx = curr_dir.LastIndexOf( "\\", last_idx - 1 );
				curr_dir = curr_dir.Substring( last_idx, curr_dir.Length - last_idx );
			}
			catch( System.Exception ex )
			{
				LogWarning( ex.ToString() );
			}

			mServerName = server_name;
			mServerCurrDir = curr_dir;
			mServerVersion = new Version( 0, 0, 0, 1 );
			mIsMaintenance = true;
			mServerStartTime = DateTime.Now;
			mServerStartTimeString = mServerStartTime.ToString( "MM-dd HH:mm:ss" );

			Directory.CreateDirectory( UMF.Core.Log.LOG_PATH );
			mLogFilename = string.Format( "{0}({1})_{2}", mServerName, Process.GetCurrentProcess().Id, DateTime.Now.ToString( "yyyy_MM_dd_HH_mm_ss" ) );

			UMF.Core.Log._Log = Log;
			UMF.Core.Log._LogWarning = LogWarning;
			UMF.Core.Log._LogError = LogError;
			UMF.Core.Log._LogImportant = LogImportant;
			UMF.Core.Log._LogDB = LogDB;
			UMF.Core.Log._LogUserLog = LogUserLog;
			UMF.Core.Log._Notification = SendServerNotification;

			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.WindowWidth = 100;

			DataReloader.Instance.ReloadSaveNameSuffix = FileUtil.ValidFileNameConvert( mServerName, '_' );

			LoadConfig( config_file );

			Log( string.Format( "Current SecurityProtocol:{0}", System.Net.ServicePointManager.SecurityProtocol ) );
			if( mApplicationConfig.UnuseTLS12 == false )
			{
				System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
				Log( string.Format( "NEW SecurityProtocol:{0}", System.Net.ServicePointManager.SecurityProtocol ) );
			}

			if( mApplicationConfig.UseNLog )
			{
				mApplicationConfig.FileLog = false;

				// Log
				mNLOG = LogManager.GetLogger( mServerName );
				mUSERLOG = LogManager.GetLogger( string.Format( "{0}.{1}", mApplicationConfig.UserLogName, mServerName ) );
				mNLOGDEBUG = LogManager.GetLogger( "Debug" );
			}

			if( mApplicationConfig.FileLog == true )
				CheckLogFile();

			timeBeginPeriod( 1 );

			// GUID
			Guid guid = Guid.NewGuid();
			guid_str = guid.ToString();
			byte[] guid_buffer = guid.ToByteArray();
			guid_long = BitConverter.ToInt64( guid_buffer, 0 );

			UpdateApplicationTitle( "" );
		}

		//------------------------------------------------------------------------
		public string GetArgsValue(string key)
		{
			string value;
			if( mArgsDic.TryGetValue( key, out value ) )
				return value;

			return null;
		}
		public bool HasArgs(string key)
		{
			return mArgsDic.ContainsKey( key );
		}

		//------------------------------------------------------------------------	
		// console title : check for process on daemon 
		int tmp_live_random = 0;
		int last_live_random = 0;
		void UpdateApplicationTitle( string message )
		{
			tmp_live_random = mRandom.Next();
			if( last_live_random != 0 && last_live_random == tmp_live_random )
				tmp_live_random = mRandom.Next();

			last_live_random = tmp_live_random;

			string maintenance_text = mIsMaintenance ? "[M]" : "";
			string base_title = string.Format( $"[{mServerTypeString}]{maintenance_text}{mConsolTitlePrefix}[{last_live_random}][{mServerName}/{mServiceTypeString}][{mGlobalTypeString}/{mEnvTypeString}][W{mApplicationConfig.WorldIDN}][{mServerVersion.ToString()}:{mServerCurrDir}]{mServerStartTimeString}-{message}" );
			Console.Title = base_title;
			PostUpdateApplicationTitle( base_title );
		}
		protected virtual void PostUpdateApplicationTitle( string title ) { }

		//------------------------------------------------------------------------		
		public virtual ApplicationConfig ConstructConfig() { return new ApplicationConfig(); }
		protected virtual void LoadConfig(string config_file )
		{
			mApplicationConfig = ConstructConfig();
			mApplicationConfig.ConfigLoad( GlobalConfig.EnvConfigPath( config_file ) );

			Log( "Load Application Config" );
			Log( mApplicationConfig.ToString() );
			Log( "" );
		}

		//------------------------------------------------------------------------		
		public void AddListener( Listener listener )
		{
			if( m_Listeners.Contains( listener ) == true )
				throw new Exception( "Already exist listener : " + listener.ToString() );

			m_Listeners.Add( listener );
		}

		//------------------------------------------------------------------------		
		public void AddConnector( Connector connector )
		{
			if( m_Connectors.Contains( connector ) == true )
				throw new Exception( "Already exist connector : " + connector.ToString() );

			m_Connectors.Add( connector );
		}

		//------------------------------------------------------------------------		
		public void AddUpdater( UpdateDelegate updater )
		{
			if( m_Updaters.Contains( updater ) == true )
				throw new Exception( "Already exist updater : " + updater.ToString() );

			m_Updaters.Add( updater );
		}

		Stopwatch time = Stopwatch.StartNew();
		TimeSpan span = TimeSpan.FromSeconds( 10 );
		int tick = 0;
		bool bRefreshTitleString = true;
		public bool RefreshTitleString { set { bRefreshTitleString = value; } }

		//------------------------------------------------------------------------		
		public void Loop()
		{
			Start();

			while( bLoop )
			{
				try
				{
					Update();
					System.Threading.Thread.Sleep( 1 );
				}
				catch (System.Exception ex)
				{
					LogError( ex.ToString() );
				}
			}
			Log( "---- shutdown ----" );

			foreach( Connector connector in m_Connectors )
			{
				if( connector.Connected == true )
					connector.Disconnect( (int)eDisconnectErrorCode.ServerMaintenance, "shutdown" );
			}

			bool bFinish = false;
			while( bFinish == false )
			{
				bFinish = true;
				foreach( Connector connector in m_Connectors )
				{
					if( connector.Connected == true )
						bFinish = false;
				}
				System.Threading.Thread.Sleep( 1 );
			}
		}

		//------------------------------------------------------------------------		
		public virtual void Start()
		{
			if( bStarted == true )
				throw new Exception( "Already Started" );

			string start_log = string.Format( $"[{mServerName}/{mServiceTypeString}/{mServerTypeString}][{mGlobalTypeString}/{mEnvTypeString}][W{mApplicationConfig.WorldIDN}][{mServerVersion.ToString()}:{mServerCurrDir}]{mServerStartTimeString}" );
			LogImportant( start_log );

			bStarted = true;
			foreach( Listener listener in m_Listeners )
			{
				listener.Start();
			}
		}

		protected int lastTick = 0;
		protected int peerCount = 0, recvCount = 0, sendCount = 0, recvBytes = 0, sendBytes = 0, sendLeft = 0;

		//------------------------------------------------------------------------		
		string DoRefreshTitleString()
		{
			string logString = MakeTitleMessage();
			UpdateApplicationTitle( logString );
			bRefreshTitleString = false;

			return logString;
		}

		//------------------------------------------------------------------------		
		protected virtual string MakeTitleMessage()
		{
			peerCount = 0;
			foreach( Listener listener in m_Listeners )
			{
				peerCount += listener.PeerCount;
			}

			return string.Format( "tick:{0:0.0}, conn:{1}, rb:{2:0,}k, sb:{3:0,}k, rc:{4:0,0}, sc:{5:0,0}, sl:{6:0,0}", lastTick, peerCount, recvBytes, sendBytes, recvCount, sendCount, sendLeft );
		}

		//------------------------------------------------------------------------		
		virtual public void Update()
		{
			++tick;

			if( mApplicationConfig.FileLog == true )
				CheckLogFile();

			foreach( UpdateDelegate updater in m_Updaters )
			{
				updater();
			}

			foreach( Listener listener in m_Listeners )
			{
				listener.Update();
			}

			foreach( Connector connector in m_Connectors )
			{
				if( connector.Connected == false && connector.Connecting == false && connector.IsElapsedFromDisconnected( 1000 ) )
				{
					if( IsShutdown == false )
						connector.Connect();
				}
				else
					connector.Update();
			}

			if( time.Elapsed > span )
			{
				peerCount = 0;
				recvCount = 0;
				sendCount = 0;
				recvBytes = 0;
				sendBytes = 0;
				sendLeft = 0;

				foreach( Listener listener in m_Listeners )
				{
					peerCount += listener.PeerCount;
					recvCount += listener.recvCount;
					sendCount += listener.sendCount;
					recvBytes += listener.recvBytes;
					sendBytes += listener.sendBytes;
					sendLeft += listener.sendLeft;

					listener.recvBytes = 0;
					listener.sendBytes = 0;
					listener.recvCount = 0;
					listener.sendCount = 0;
				}

				foreach( Connector connector in m_Connectors )
				{
					recvCount += connector.recvCount;
					sendCount += connector.sendCount;
					recvBytes += connector.recvBytes;
					sendBytes += connector.sendBytes;
					sendLeft += connector.SendPacketCount;

					connector.recvBytes = 0;
					connector.sendBytes = 0;
					connector.recvCount = 0;
					connector.sendCount = 0;
				}

				lastTick = tick;
				tick = 0;

				string refresh_title = DoRefreshTitleString();
				if( mTitlePrint )
					_Log( refresh_title, LogType.ConsoleOnly );

				time.Restart();
				//GC.Collect();
			}

			if( bRefreshTitleString == true )
				DoRefreshTitleString();
		}

		void CheckLogFile()
		{
			if( writer != null && writer.BaseStream.Length > mApplicationConfig.LogfileSize )
			{
				try
				{
					writer.Flush();
				}
				catch( System.Exception )
				{

				}
				writer.Close();
				writer = null;
			}
			if( writer == null )
			{
				mCurrentLogFilename = string.Format( "{0}_{1}.txt", mLogFilename, mLogIndex++ );
				mCurrentLogFilepath = Directory.GetCurrentDirectory() + "/Log/" + mCurrentLogFilename;
				FileStream fs = new FileStream( mCurrentLogFilepath, FileMode.CreateNew, FileAccess.Write );
				writer = new StreamWriter( fs, System.Text.Encoding.UTF8 );

				if( _ChangeCurrentLogFilename != null )
					_ChangeCurrentLogFilename();
			}
		}

		void CheckLogErrorFile()
		{
			if( writerError == null )
			{
				string filename = string.Format( "{0}/Errors.txt", UMF.Core.Log.LOG_PATH );
				FileStream fs = new FileStream( filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite );
				writerError = new StreamWriter( fs, System.Text.Encoding.UTF8 );
				try
				{
					writerError.WriteLine( "[" + mServerName + "] " + "[" + DateTime.Now.ToString() + "] START" );
					writerError.Flush();
				}
				catch( System.Exception )
				{
				}
			}
		}

		public enum LogType
		{
			ConsoleOnly,
			Debug,
			Normal,
			Warning,
			Error,
			Important,
			DB,
			UserLog,
		}

		public class UserLogBase
		{
			public string log_type;
			public string log;
		}
		UserLogBase m_userlog_base = new UserLogBase();
		JsonSerializerSettings m_json_ops = null;
		public JsonSerializerSettings LogJsonOption
		{
			get
			{
				if( m_json_ops == null )
				{
					m_json_ops = new Newtonsoft.Json.JsonSerializerSettings();
					m_json_ops.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
					m_json_ops.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
					m_json_ops.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
				}

				return m_json_ops;
			}
		}

		public void SerializeUserLog( object log_type, object log_object )
		{
			if( mUSERLOG != null )
			{
				string log = "";
				if( log_object is string )
				{
					log = log_object.ToString();
				}
				else
				{
					log = JsonConvert.SerializeObject( log_object, Newtonsoft.Json.Formatting.None, m_json_ops );
				}

				mUSERLOG.Trace( "{\"log_type\":\"" + log_type.ToString() + "\",\"time\":\"" + DateTime.Now.ToString() + "\",\"log\":" + log + "}" );
			}
		}

		void _Log( string strLog, LogType log_type )
		{
			if( mApplicationConfig.FileLog == true )
			{
				try
				{
					writer.WriteLine( "[" + DateTime.Now.ToString() + "] " + strLog );
					writer.Flush();
				}
				catch( System.Exception )
				{
				}
			}

			if( AlwaysConsoleWrite || log_type == LogType.ConsoleOnly || log_type == LogType.Error || log_type == LogType.Important )
				WriteConsole( strLog, log_type );

			// use nLog
			if( mApplicationConfig.UseNLog )
			{
				switch( log_type )
				{
					case ServerApplication.LogType.Debug:
						if( mNLOGDEBUG != null )
							mNLOGDEBUG.Trace( strLog );
						break;

					case ServerApplication.LogType.Normal:
					case ServerApplication.LogType.Important:
					case ServerApplication.LogType.DB:
						if( mNLOG != null )
							mNLOG.Info( strLog );
						break;

					case ServerApplication.LogType.Warning:
						if( mNLOG != null )
							mNLOG.Warn( strLog );
						break;

					case ServerApplication.LogType.Error:
						if( mNLOG != null )
							mNLOG.Error( strLog );
						break;

					case LogType.UserLog:
						if( mUSERLOG != null )
							mUSERLOG.Trace( strLog );
						break;
				}
			}
		}
		System.ConsoleColor m_ConsoleColor = ConsoleColor.White;

		public void WriteConsole( string strLog, LogType type )
		{
			System.ConsoleColor color = ConsoleColor.Gray;
			switch( type )
			{
				case LogType.Error:
					color = ConsoleColor.Red;
					break;

				case LogType.Warning:
					color = ConsoleColor.Cyan;
					break;

				case LogType.Important:
					color = ConsoleColor.Green;
					break;

				case LogType.DB:
					color = ConsoleColor.Yellow;
					break;

				case LogType.UserLog:
					color = ConsoleColor.Magenta;
					break;
			}
			if( m_ConsoleColor != color )
			{
				m_ConsoleColor = color;
				Console.ForegroundColor = color;
			}
			Console.WriteLine( strLog );
		}

		void Log( string strLog )
		{
			_Log( strLog, LogType.Normal );
		}

		void LogWarning( string strLog )
		{
			_Log( strLog, LogType.Warning );
		}

		void LogError( string strLog )
		{
			CheckLogErrorFile();

			try
			{
				writerError.WriteLine( "[" + mServerName + "] " + "[" + DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss.fff" ) + "] " + strLog );
				writerError.Flush();
			}
			catch( System.Exception )
			{
			}

			_Log( strLog, LogType.Error );
		}

		void LogImportant( string strLog )
		{
			_Log( strLog, LogType.Important );
		}

		void LogDB( string strLog )
		{
			_Log( strLog, LogType.DB );
		}

		void LogUserLog( string strLog )
		{
			_Log( strLog, LogType.UserLog );
		}

		public void Close()
		{
			if( mApplicationConfig.FileLog == true && writer != null )
			{
				try
				{
					writer.Flush();
				}
				catch( System.Exception )
				{

				}
				writer.Close();
			}
			if( writerError != null )
			{
				try
				{
					writerError.Flush();
				}
				catch( System.Exception )
				{

				}
				writerError.Close();
			}

			foreach( Connector connector in m_Connectors )
			{
				connector.Close();
			}
			foreach( Listener listener in m_Listeners )
			{
				listener.Close();
			}
		}

		//------------------------------------------------------------------------	
		public virtual void SendServerNotification( string message )
		{
			string maintenance = mIsMaintenance ? "[M]" : "";
			string type_text = $"[G:{mGlobalTypeString}:{mEnvTypeString}][W:{mApplicationConfig.WorldIDN}]";
			string server_text = $"[S:{mServerTypeString},{mServiceTypeString},{mServerVersion.ToString()},{mServerCurrDir},{mServerStartTimeString}]";
			string now = DateTime.Now.ToString( "MM-dd HH:mm:ss" );

			string full_message = $"{maintenance}{type_text}{server_text}\n{message}\n{now}";
			string sender = string.Format( "{0}[{1}][{2}]", mServerName, mEnvTypeString, ServerIP );

			if( mNotificationModule != null )
			{
				mNotificationModule.Send( sender, message, full_message );
			}
			else
			{
				full_message = full_message.Replace( "\n", " " );
				LogImportant( $"[Notification][{sender}]{full_message}" );
			}
		}
		
		//------------------------------------------------------------------------		
		public virtual void Shutdown(bool immeidate_quit)
		{
			if( IsShutdown == false )
				AddUpdater( CheckShutdownProcess );

			IsShutdown = true;
			SendServerNotification( "shutdown received" );
		}

		//------------------------------------------------------------------------
		protected virtual bool IsFinishedForShutdown()
		{
			if( mNotificationModule != null && mNotificationModule.IsFinished() == false )
				return false;

			return true;
		}

		//------------------------------------------------------------------------		
		protected virtual void CheckShutdownProcess()
		{
			if( IsFinishedForShutdown() )
				StopLoop();
		}
	}
}
