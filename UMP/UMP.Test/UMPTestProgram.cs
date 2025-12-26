using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UMP.CSCommon;
using UMF.Server;

namespace UMP.Test
{
	public class PacketVersion
	{
		public const short Version = 1;
	}

	class UMPTestProgram
	{
		class ProcessData
		{
			public ProcessData( Process process, bool bDaemon )
			{
				this.bDaemon = bDaemon;
				this.process = process;
				strName = process.ProcessName;
// 				cpuCounter = new PerformanceCounter( "Process", "% Processor Time", strName, true );
// 				workingSetCounter = new PerformanceCounter( "Process", "Working Set - Private", strName, true );
// 				handleCounter = new PerformanceCounter( "Process", "Handle Count", strName, true );
// 				threadCounter = new PerformanceCounter( "Process", "Thread Count", strName, true );
// 				freeMemCounter = new PerformanceCounter( "Memory", "Available MBytes", true );
			}

			public bool bDaemon = false;
			public Process process { get; private set; }
			public string strName, strTitle;
			public int not_reponding_count;
			public DateTime title_check_time = DateTime.Now.AddSeconds( 30 );
//			public PerformanceCounter cpuCounter, workingSetCounter, handleCounter, threadCounter, freeMemCounter;

			public string GetState()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendLine( string.Format( "== {0}", strName ) );
				try
				{
					//sb.AppendLine( string.Format( "# cpu:{0}% WS:{1}KB FREE:{2}MB", cpuCounter.NextValue(), ( workingSetCounter.NextValue() / 1024f ), freeMemCounter.NextValue() ) );
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

		static List<ProcessData> mProcessDataList = new List<ProcessData>();
		static CommandManager mConsoleManager = null;

		class ExecuteTestType
		{
			public string type_name;
			public System.Type activate_type;

			public ExecuteTestType( string _name, System.Type _activat_type )
			{
				type_name = _name;
				activate_type = _activat_type;
			}
		}

		static List<ExecuteTestType> mExecuteTypes = new List<ExecuteTestType>()
		{
			new ExecuteTestType( eServerType.Contents.ToString(), typeof(ContentsServerTest) ),
			new ExecuteTestType( eServerType.Relay.ToString(), typeof(RelayServerTest) ),
			new ExecuteTestType( eServerType.Game.ToString(), typeof(GameServerTest) ),
			new ExecuteTestType( eServerType.Login.ToString(), typeof(LoginServerTest) ),
			new ExecuteTestType( eServerType.Master.ToString(), typeof(MasterServerTest) ),
			new ExecuteTestType( eServerType.Daemon.ToString(), typeof(DaemonServerTest) ),
			new ExecuteTestType( "Client", typeof(ClientTest) ),
			new ExecuteTestType( "ModuleTestClient", typeof(ModuleClientTest) ),
			new ExecuteTestType( "ModuleTestServer", typeof(ModuleServerTest) ),
		};

		static void Main( string[] args )
		{
			if( args != null && args.Length > 0 )
			{
				Start( args );
				return;
			}

			ExecuteProcess( eServerType.Master.ToString() );
			ExecuteProcess( eServerType.Daemon.ToString() );
			//ExecuteProcess( eServerType.Login.ToString() );
			//ExecuteProcess( eServerType.Relay.ToString() );
			//ExecuteProcess( eServerType.Game.ToString() );
			//ExecuteProcess( eServerType.Contents.ToString() );
			ExecuteProcess( "Client" );
			//ExecuteProcess( "ModuleTestClient" );
			//ExecuteProcess( "ModuleTestServer" );

			mConsoleManager = new CommandManager();
			mConsoleManager.AddRootCommand( "q", ( string command ) =>
			{
				foreach( ProcessData data in mProcessDataList )
				{
					if( data.process != null && data.process.HasExited == false )
						data.process.Kill();
				}
				mProcessDataList.Clear();
			}, eCommandAuthority.Server );
			mConsoleManager.AddRootCommand( "info", ( string command ) =>
			 {
				 foreach(ProcessData data in mProcessDataList)
				 {
					 Console.WriteLine( data.GetState() );
				 }
			 }, eCommandAuthority.Server );

			while( true)
			{
				mConsoleManager.Update();
				Thread.Sleep( 1 );

				mProcessDataList.RemoveAll( a => a.process == null || a.process.HasExited );
				if( mProcessDataList.Count <= 0 )
					break;
			}

			Console.WriteLine( "Test Finished!" );
		}

		static void ExecuteProcess(string arg)
		{
			Console.WriteLine( $" ExecuteProcess {arg}" );

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = "UMP.Test.exe";
			startInfo.WindowStyle = ProcessWindowStyle.Normal;
			startInfo.UseShellExecute = true;
			startInfo.ErrorDialog = false;
			startInfo.Arguments = arg;

			Process process = Process.Start( startInfo );
			ProcessData data = new ProcessData( process, false );
			data.strName = arg;
			mProcessDataList.Add( data );
		}

		//------------------------------------------------------------------------		
		static void Start(string[] args)
		{
			string start_type = args[0];

			Console.WriteLine( $"=== {start_type} start" );

			//Console.WriteLine( $"- wait attach to process and press any key " );
			//Console.ReadKey();

			ExecuteTestType data = mExecuteTypes.Find( a => a.type_name == start_type );
			if( data != null )
			{
				TestBase tb = Activator.CreateInstance( data.activate_type ) as TestBase;
				tb.Start();
			}
			else
			{
				Console.WriteLine( $"!!! Start failed {start_type}" );
			}
		}
	}
}
