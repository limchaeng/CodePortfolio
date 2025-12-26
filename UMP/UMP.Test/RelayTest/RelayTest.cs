using UMF.Core;
using UMP.CSCommon;
using UMP.Server.Relay;
using UMP.Server;

namespace UMP.Test
{
	public class RelayServerTest : TestBase
	{
		public override void TestStart()
		{
			LoadGlobalConfig( "RelayTest" );

			eServiceType service_type = eServiceType.Local;
			string application_config = "RelayApplicationConfig.txt";
			string game_listener_config = "GameRelayPeerManagerConfig.txt";
			string contents_listener_config = "ContentsRelayPeerManagerConfig.txt";
			string master_config = "RelayMasterConnectorConfig.txt";

			RelayServerApplication relay_app = new RelayServerApplication( "RelayTest", service_type, null, null, application_config, null );
			relay_app.AlwaysConsoleWrite = true;

			GameRelayPeerManagerStandard game_manager = new GameRelayPeerManagerStandard( relay_app, game_listener_config, null );
			ContentsRelayPeerManagerStandard contents_manager = new ContentsRelayPeerManagerStandard( relay_app, contents_listener_config, game_manager );

			RelayMasterConnector master_connector = new RelayMasterConnector( relay_app, master_config );

			DataReloader.Instance.LoadFromSavedFile();

			relay_app.Loop();
		}

	}
}
