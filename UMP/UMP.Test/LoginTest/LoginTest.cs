using UMF.Core;
using System.Collections.Generic;
using UMP.CSCommon;
using UMP.Server.Login;
using UMP.Server;
using UMP.Module.WorldModule;
using UMM.Module.ModuleTest;
using UMP.Module.AppVerifyModule;
using UMF.Net;
using UMP.CSCommon.Packet;

namespace UMP.Test
{
#if UMSERVER
	//------------------------------------------------------------------------	
	public class LoginServerTest : TestBase
	{
		public override void TestStart()
		{
			LoadGlobalConfig( "LoginTest" );

			AppIdentifier.Instance.Add( "editor", 1 );

			WorldModuleConfig.Instance.STATIC_PATH = "_server_config/template_WorldModuleConfig.xml";
			AppVerifyModuleConfig.Instance.STATIC_PATH = "_server_config/template_AppVerifyModuleConfig.xml";

			//
			eServiceType service_type = eServiceType.Local;
			string application_config = "LoginApplicationConfig.txt";
			string listener_config = "ClientLoginPeerManagerConfig.txt";
			string master_config = "LoginMasterConnectorConfig.txt";

			LoginServerApplication login_app = new LoginServerApplication( "LoginTest", service_type, application_config, null );
			login_app.AlwaysConsoleWrite = true;

			//C2L_PacketHandlerManagerStandard packet_handler_manager = new C2L_PacketHandlerManagerStandard( login_app );
			//ClientLoginPeerManager user_manager = new ClientLoginPeerManager( login_app, listener_config, typeof( NPID_L2C ), packet_handler_manager );
			ClientLoginPeerManager user_manager = new ClientLoginPeerManager( login_app, listener_config );
			LoginMasterConnector master_connector = new LoginMasterConnector( login_app, master_config );

			WorldModuleServer world_module = new WorldModuleServer( login_app, user_manager );
			AppVerifyModuleLoginServer appverify_module = new AppVerifyModuleLoginServer( login_app );

			DataReloader.Instance.LoadFromSavedFile();
			//
			login_app.Loop();
		}
	}
#endif

	//------------------------------------------------------------------------
	public class ModuleServerTest : TestBase
	{
		public class C2L_PacketHandlerManagerTest : C2L_PacketHandlerManagerStandard
		{
			public C2L_PacketHandlerManagerTest( LoginServerApplication application ) : base( application )
			{
				AddPacketRecvInterruptHandler<NC2L_Login>( NC2L_LoginInterrupt );
			}

			//[PacketRecvInterruptHandler(PacketType = typeof(NC2L_Login))]
			void NC2L_LoginInterrupt(Session session, NC2L_Login packet)
			{
				Log.WriteImportant( "NC2L_Login : interrupt" );
				//NC2L_Login packet = _packet as NC2L_Login;
			}

			protected override void NC2L_LoginHandler( ClientLoginPeer session, object _packet )
			{
				base.NC2L_LoginHandler( session, _packet );

				Log.WriteImportant( "NC2L_Login : override" );
			}
		}


		public override void TestStart()
		{
			LoadGlobalConfig( "LoginTest" );

			AppIdentifier.Instance.Add( "editor", 1 );

			WorldModuleConfig.Instance.STATIC_PATH = "_server_config/template_WorldModuleConfig.xml";
			AppVerifyModuleConfig.Instance.STATIC_PATH = "_server_config/template_AppVerifyModuleConfig.xml";

			//
			eServiceType service_type = eServiceType.Local;
			string application_config = "LoginApplicationConfig.txt";
			string listener_config = "ClientLoginPeerManagerConfig.txt";

			LoginServerApplication login_app = new LoginServerApplication( "LoginTest", service_type, application_config, null );
			login_app.AlwaysConsoleWrite = true;

			C2L_PacketHandlerManagerTest packet_handler_manager = new C2L_PacketHandlerManagerTest( login_app );
			ClientLoginPeerManager user_manager = new ClientLoginPeerManager( login_app, listener_config, typeof( NPID_C2L ), packet_handler_manager );
			//ClientLoginPeerManager user_manager = new ClientLoginPeerManager( login_app, listener_config, typeof( NPID_L2C ), packet_handler_manager );
			//ClientLoginPeerManager user_manager = new ClientLoginPeerManager( login_app, listener_config );

			ModuleTestServer module = new ModuleTestServer( user_manager );

			DataReloader.Instance.ReloadData( null );

			//DataReloader.Instance.LoadFromSavedFile();
			//
			login_app.Loop();

		}
	}

}
