using System;
using UMF.Core;
using System.Collections.Generic;
using UMP.CSCommon;
using UMP.Server.Game;
using UMP.Server;
using UMP.Module.AppVerifyModule;
using UMF.Core.Module;
using UMF.Database;

namespace UMP.Test
{
	//------------------------------------------------------------------------	
	public class GameServerTest : TestBase
	{
		GameServerApplication mGameApp = null;
		GameMasterConnector mMaster = null;

		public override void TestStart()
		{
			LoadGlobalConfig( "GameTest" );

			AppIdentifier.Instance.Add( "editor", 1 );

			AccountConfig.MakeInstance();
			AccountConfig.Instance.STATIC_PATH = "_server_config/template_AppVerifyModuleConfig.xml";

			//
			eServiceType service_type = eServiceType.Local;
			string application_config = "GameApplicationConfig.txt";
			string client_listener_config = "ClientGamePeerManagerConfig.txt";
			string master_config = "GameMasterConnectorConfig.txt";
			string relay_config = "GameRelayConnectorConfig.txt";

			string world_db_config = "WorldDBConfig.txt";
			string auth_db_config = "AuthDBConfig.txt";
			string common_db_config = "CommonDBConfig.txt";
			string game_db_config = "GameDBConfig.txt";
			DatabaseMain world_db = new DatabaseMain( world_db_config );
			DatabaseMain auth_db = new DatabaseMain( auth_db_config );
			DatabaseMain common_db = new DatabaseMain( common_db_config );

			mGameApp = new GameServerApplication( "GameTest", service_type, application_config, world_db, auth_db, common_db, game_db_config, null );
			mGameApp.AlwaysConsoleWrite = true;

			ClientGamePeerManagerStandard user_manager = new ClientGamePeerManagerStandard( mGameApp, client_listener_config );
			mMaster = new GameMasterConnector( mGameApp, master_config );

			GameRelayConnector relay_connector = new GameRelayConnector( mGameApp, relay_config, user_manager );
			AppVerifyModuleGameServer appverify_module = new AppVerifyModuleGameServer( mGameApp, world_db );
			
			DataReloader.Instance.LoadFromSavedFile();
			//
			mGameApp.Loop();
		}
	}
}
