using UMP.Server.Daemon;
using UMP.CSCommon;
using UMF.Module.SlackModule;
using UMF.Core;

namespace UMP.Test
{
	public class DaemonServerTest : TestBase
	{
		public override void TestStart()
		{
			LoadGlobalConfig( "DaemonTest" );

			string app_config = "DaemonTestApplicationConfig.txt";
			string master_connector_config = "DaemonMasterConnectorConfig.txt";

			DaemonServerApplication app = new DaemonServerApplication( "Daemon", eServiceType.Local, app_config, new string[] { "Login", "Relay", "Game", "Contents" } );
			app.AlwaysConsoleWrite = true;

			DaemonMasterConnector master = new DaemonMasterConnector( app, master_connector_config );

			SlackModuleServer slack = new SlackModuleServer( app );

			app.Loop();
		}

	}
}
