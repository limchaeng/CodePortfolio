using System;
using UMF.Core;
using System.Collections.Generic;
using UMP.CSCommon;
using UMP.Server.Game;
using UMP.Server;
using UMP.Server.Contents;

namespace UMP.Test
{
	public class ContentsServerTest : TestBase
	{
		public override void TestStart()
		{
			LoadGlobalConfig( "ContentsTest" );

			//
			eServiceType service_type = eServiceType.Local;
			string application_config = "ContentsApplicationConfig.txt";
			string client_listener_config = "ClientContentsPeerManagerConfig.txt";
			string master_config = "ContentsMasterConnectorConfig.txt";
			string relay_config = "ContentsRelayConnectorConfig.txt";

			ContentsServerApplication app = new ContentsServerApplication( "ContentsTest", service_type, application_config, null );
			app.AlwaysConsoleWrite = true;

			ClientContentsPeerManagerStandard user_manager = new ClientContentsPeerManagerStandard( app, client_listener_config );
			ContentsRelayConnector relay_connector = new ContentsRelayConnector( app, relay_config, user_manager );

			ContentsMasterConnector master_connector = new ContentsMasterConnector( app, master_config );

			app.Loop();
		}
	}
}
