using System;
using UMF.Net;
using UMP.CSCommon;
using UMF.Server;
using UMP.Server.Master;
using UMP.Module.AppVerifyModule.Master;
using UMF.Core.Module;
using UMF.Core;

namespace UMP.Test
{
	public class MasterServerTest : TestBase
	{
		public override void TestStart()
		{
			LoadGlobalConfig( "MasterTest" );

			eServiceType service_type = eServiceType.Local;
			string application_config = "MasterApplicationConfig.txt";
			string daemon_listener_config = "DaemonMasterListenetConfig.txt";
			string server_listener_config = "ServerMasterPeerManagerConfig.txt";

			MasterServerApplication app = new MasterServerApplication( "Master", service_type, application_config, null );
			app.AlwaysConsoleWrite = true;

			DaemonMasterPeerManager daemon = new DaemonMasterPeerManager( app, daemon_listener_config );
			ServerMasterPeerManager server = new ServerMasterPeerManager( app, server_listener_config );

			MasterCommand command = new MasterCommand( app, true );

			AppVerifyModuleMasterPeer appverify_module = new AppVerifyModuleMasterPeer( server );

			app.Loop();
		}
	}
}
