using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UMF.Net;
using UMF.Server;
using UMM.Module.ModuleTest;
using UMP.Client;
using UMP.Client.Net;
using UMP.CSCommon;
using UMP.CSCommon.Packet;
using UMP.Module.WorldModule;

namespace UMP.Test
{
#if UMCLIENT
	public class ClientTest : TestBase
	{
		bool bLoop = false;

		public class TestLogin : LoginConnector
		{
			public TestLogin( string config_file ) 
				: base( "TestLogin", config_file, new L2C_PacketHandlerManagerStandard(), 
					  typeof(NPID_C2L), typeof(NPID_C2L), "0.0.0.1", 1, 0, "lan", 0, "Korean" )
			{
			}
		}

		public class TestGame : GameConnector
		{
			public TestGame( string config_file )
				: base( "TestGame", config_file, new G2C_PacketHandlerManagerStandard()
					  , typeof(NPID_C2G), typeof(NPID_C2G), "0.0.0.1", 1, 0, "lan", 0, "Korean" )
			{
			}
		}

		public override void TestStart()
		{
			LoadGlobalConfig( "ClientTest" );

			UMF.Core.Log.SetAll( Log );
			AppIdentifier.Instance.Add( "test", 1 );
			Console.Title = "[Client]";

			CommandManager mConsoleManager = null;
			/**/
			string login_connector_config = "ClientLoginConnectorConfig.txt";
			string game_connector_config = "ClientGameConnectorConfig.txt";

			TestLogin mLogin = new TestLogin( login_connector_config );
			TestGame mGame = new TestGame( game_connector_config );

			WorldModuleClient world_module = new WorldModuleClient( mLogin );
			ModuleTestClient test_module = new ModuleTestClient( mLogin );

			mLogin.OnConnectedCallback = ( bool bConnected ) =>
			{
				if( bConnected )
					NetworkHandler.Instance.ServerConnected( eServerType.Login );
				else
					NetworkHandler.Instance.ServerDisconnected( eServerType.Login, (int)eDisconnectErrorCode.ConnectFail, "connect failed", "" );
			};
			mLogin.OnVerified = () =>
			{
				NetworkHandler.Instance.ServerVerified( eServerType.Login );
			};
			mLogin.OnDisconnectedCallback = ( Session session, int error_code, string error_string ) =>
			{
				NetworkHandler.Instance.ServerDisconnected( eServerType.Login, error_code, error_string, "" );
			};

			mGame.OnConnectedCallback = ( bool bConnected ) =>
			{
				if( bConnected )
					NetworkHandler.Instance.ServerConnected( eServerType.Game );
				else
					NetworkHandler.Instance.ServerDisconnected( eServerType.Game, (int)eDisconnectErrorCode.ConnectFail, "connect failed", "" );
			};
			mGame.OnVerified = () =>
			{
				NetworkHandler.Instance.ServerVerified( eServerType.Game );
			};
			mGame.OnDisconnectedCallback = ( Session session, int error_code, string error_string ) =>
			{
				NetworkHandler.Instance.ServerDisconnected( eServerType.Game, error_code, error_string, "" );
			};

			mConsoleManager = new CommandManager();
			mConsoleManager.AddRootCommand( "quit", ( string command ) =>
			{
				bLoop = false;
			}, eCommandAuthority.Server );
			mConsoleManager.AddRootCommand( "connect", ( string command ) =>
			{
				mLogin.Connect();
			}, eCommandAuthority.Server );
			mConsoleManager.AddRootCommand( "module", ( string command ) =>
			{
				test_module.SendTest();
			}, eCommandAuthority.Server );

			NetworkHandler.Instance.OnServerConnectedHandler = ( eServerType server ) =>
			{
				Console.WriteLine( $"OnServerConnectedHandler : {server}" );

				if( server == eServerType.Login )
				{
					NC2L_Login _NC2L_Login = new NC2L_Login();
					_NC2L_Login.world_idn = 1;
					_NC2L_Login.gameserver_guid = 0;
					_NC2L_Login.curr_localize = "Korean";
					_NC2L_Login.device_package_id = "";
					mLogin.SendPacket( _NC2L_Login, ( NL2C_LoginAck packet ) =>
					{
						Console.WriteLine( $"NL2C_LoginAck Received packet : " );

						if( packet.gameserver_connection_data != null )
						{
							mLogin.Disconnect( (int)eDisconnectErrorCode.Normal, "" );
							mGame.ConnectTo( packet.gameserver_connection_data.gameserver_host_name, packet.gameserver_connection_data.gameserver_port,
								packet.gameserver_connection_data.connection_key, "lan", "Korean" );
						}
					} );
				}
				else if( server == eServerType.Game )
				{
					NC2G_AccountLogin _NC2G_AccountLogin = new NC2G_AccountLogin();
					_NC2G_AccountLogin.localize = "Korean";
					_NC2G_AccountLogin.app_store_type = 1;
					_NC2G_AccountLogin.device_data = new CS_AccountDeviceData();
					_NC2G_AccountLogin.device_data.device_type = "";
					_NC2G_AccountLogin.device_data.device_id = "";
					_NC2G_AccountLogin.device_data.device_location = "";
					_NC2G_AccountLogin.device_data.device_os_version = "";
					_NC2G_AccountLogin.device_data.device_language = "";
					_NC2G_AccountLogin.device_data.device_advertising_id = "";
					_NC2G_AccountLogin.device_data.device_package_id = "";

					_NC2G_AccountLogin.auth_data = new CS_AuthPlatformData();
					_NC2G_AccountLogin.auth_data.auth_platform_type = (short)eLoginPlatformInternal.Guest;
					_NC2G_AccountLogin.auth_data.auth_platform_id = "guest_guest_guest_123";
					_NC2G_AccountLogin.auth_data.auth_platform_key = "guest_guest_guest_999";
					_NC2G_AccountLogin.auth_data.auth_platform_name = "guest!";

					mGame.SendPacket( _NC2G_AccountLogin, ( NG2C_AccountLoginAck packet ) =>
					{
						Console.WriteLine( $"NG2C_AccountLoginAck Received packet : " );
					} );
				}
			};
			NetworkHandler.Instance.OnServerDisconnectedHandler = ( eServerType server, int error, string err_msg ) =>
			{
				Console.WriteLine( $"OnServerDisconnectedHandler : {server}, {error}, {err_msg}" );
			};
			NetworkHandler.Instance.OnServerVerifiedHandler = ( eServerType server ) =>
			{
				Console.WriteLine( $"OnServerVerifiedHandler : {server}" );
			};

			bLoop = true;
			while( bLoop )
			{
				mLogin.Update();
				mGame.Update();
				mConsoleManager.Update();
				Thread.Sleep( 1 );
			}
			/**/
		}

		void Log( string log )
		{
			Console.WriteLine( log );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	///
	public class ModuleClientTest : TestBase
	{
		public class TestLogin : LoginConnector
		{
			public TestLogin( string config_file )
				: base( "ModuleTest", config_file, new L2C_PacketHandlerManagerStandard(),
					  typeof( NPID_C2L ), typeof( NPID_C2L ), "0.0.0.2", 1, 0, "lan", 0, "Korean" )
			{
			}
		}

		void Log( string log )
		{
			Console.WriteLine( log );
		}

		bool bLoop = false;

		public override void TestStart()
		{
			LoadGlobalConfig( "ClientTest" );

			UMF.Core.Log.SetAll( Log );
			AppIdentifier.Instance.Add( "test", 1 );
			Console.Title = "[Client]";

			CommandManager mConsoleManager = null;
			/**/
			string login_connector_config = "ClientLoginConnectorConfig.txt";

			TestLogin mLogin = new TestLogin( login_connector_config );

			WorldModuleClient world_module = new WorldModuleClient( mLogin );
			ModuleTestClient test_module = new ModuleTestClient( mLogin );

			mLogin.OnConnectedCallback = ( bool bConnected ) =>
			{
				if( bConnected )
					NetworkHandler.Instance.ServerConnected( eServerType.Login );
				else
					NetworkHandler.Instance.ServerDisconnected( eServerType.Login, (int)eDisconnectErrorCode.ConnectFail, "connect failed", "" );
			};
			mLogin.OnVerified = () =>
			{
				NetworkHandler.Instance.ServerVerified( eServerType.Login );
			};
			mLogin.OnDisconnectedCallback = ( Session session, int error_code, string error_string ) =>
			{
				NetworkHandler.Instance.ServerDisconnected( eServerType.Login, error_code, error_string, "" );
			};

			mConsoleManager = new CommandManager();
			mConsoleManager.AddRootCommand( "quit", ( string command ) =>
			{
				bLoop = false;
			}, eCommandAuthority.Server );
			mConsoleManager.AddRootCommand( "connect", ( string command ) =>
			{
				mLogin.Connect();
			}, eCommandAuthority.Server );
			mConsoleManager.AddRootCommand( "module", ( string command ) =>
			{
				test_module.SendTest();
			}, eCommandAuthority.Server );

			NetworkHandler.Instance.OnServerConnectedHandler = ( eServerType server ) =>
			{
				Console.WriteLine( $"OnServerConnectedHandler : {server}" );

				if( server == eServerType.Login )
				{
					NC2L_Login _NC2L_Login = new NC2L_Login();
					_NC2L_Login.world_idn = 1;
					_NC2L_Login.gameserver_guid = 0;
					_NC2L_Login.curr_localize = "Korean";
					_NC2L_Login.device_package_id = "";
					mLogin.SendPacket<NC2L_Login, NL2C_LoginAck>( _NC2L_Login, ( NL2C_LoginAck packet ) =>
					{
						Console.WriteLine( $"NL2C_LoginAck Received packet : " );
					} );
				}
			};
			NetworkHandler.Instance.OnServerDisconnectedHandler = ( eServerType server, int error, string err_msg ) =>
			{
				Console.WriteLine( $"OnServerDisconnectedHandler : {server}, {error}, {err_msg}" );
			};
			NetworkHandler.Instance.OnServerVerifiedHandler = ( eServerType server ) =>
			{
				Console.WriteLine( $"OnServerVerifiedHandler : {server}" );
			};

			bLoop = true;
			while( bLoop )
			{
				mLogin.Update();
				mConsoleManager.Update();
				Thread.Sleep( 1 );
			}
		}
	}

#endif
}
