//////////////////////////////////////////////////////////////////////////
//
// Connector
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

using System.Net;
using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;
using UMF.Core;
using System.Collections.Generic;
using UMF.Net.Module;
using System.Reflection;

namespace UMF.Net
{
	//------------------------------------------------------------------------	
	public class Connector : Session, IPacketInterruptor
	{
		protected struct ConnectedCallbackState
		{
			public Socket socket;
			public ManualResetEvent connectedEvent;
		}

		protected override int SendBufferSize { get { return mConfig.SendBufferSize; } }
		protected override int RecvBufferSize { get { return mConfig.RecvBufferSize; } }
		public override eCoreLogType CoreLogType { get { return mConfig.CoreLogType; } }
		public void SetCoreLogType( Core.eCoreLogType logType ) { mConfig.CoreLogType = logType; }

		public override bool Verified
		{
			get
			{
				if( mConfig.UseProtocolVersion == false )
					return true;
				return m_Verified;
			}
		}
		bool m_Verified = false;
		protected override short VerifyPacketId => PacketVerifyID.VerifyPacketId;
		protected override string VerifyPacketLog( object packet )
		{
			return PacketLogFormatter.Instance.Serialize<PacketVerify>( packet, false );
		}
		protected override short VerifiedAckPacketId => PacketVerifyID.VerifiedPacketId;

		PacketFormatterConfig m_PacketFormatterConfig = null;
		public override PacketFormatterConfig GetPacketFormatterConfig
		{
			get
			{
				if( m_PacketFormatterConfig == null )
				{
					m_PacketFormatterConfig = new PacketFormatterConfig();
					m_PacketFormatterConfig.UseConvertDatetime = mConfig.UseConvertDatetime;
					m_PacketFormatterConfig.IgnoreTimeZone = mConfig.IgnoreTimeZone;
					m_PacketFormatterConfig.protocol_version = -1;
				}
				return m_PacketFormatterConfig;
			}
		}

		public override Type RecvPacketIdType { get { return mPacketHandlerManager.PacketIdType; } }
		public override Type NRecvPacketIdType { get { return mPacketHandlerManager.NPacketIdType; } }

		Type mSendPacketIdType;
		Type mNSendPacketIdType;
		public override Type SendPacketIdType { get { return mSendPacketIdType; } }
		public override Type NSendPacketIdType { get { return mNSendPacketIdType; } }


		public delegate void OnConnectedDelegate( bool bConnected );
		public delegate void OnConnectedAdvanceDelegate( bool bConnected, Connector connector );
		public delegate void OnVerifiedDelegate();
		OnConnectedDelegate _OnConnected = null;
		OnConnectedAdvanceDelegate _OnConnectedAdvance = null;
		OnVerifiedDelegate _OnVerified = null;
		public OnConnectedDelegate OnConnectedCallback { set { _OnConnected = value; } }
		public OnConnectedAdvanceDelegate OnConnectedAdvanceCallback { set { _OnConnectedAdvance = value; } }
		public OnVerifiedDelegate OnVerified { set { _OnVerified = value; } }
		public void AddConnectedAdvacneCallback( bool bAdd, OnConnectedAdvanceDelegate callback )
		{
			if( bAdd )
				_OnConnectedAdvance += callback;
			else
				_OnConnectedAdvance -= callback;
		}
		public void AddConnectedCallback( bool bAdd, OnConnectedDelegate callback )
		{
			if( bAdd )
				_OnConnected += callback;
			else
				_OnConnected -= callback;
		}

		//------------------------------------------------------------------------	
		public class ConnectorConfig : EnvConfig
		{
			public string VerifyString = "_UMF_";
			public string Host = "";
			public int Port = 5000;			
			// LOG
			public Core.eCoreLogType CoreLogType = Core.eCoreLogType.None;
			public eCheckEmptyHandlerType CheckEmptyHandler = eCheckEmptyHandlerType.Exception;
			// BufferSize
			public int SendBufferSize = 4096;
			public int RecvBufferSize = 4096;
			// Connector
			public bool UseAsyncConnect = true;
			public int ConnectTimeout = 5;
			public TimeSpan ConnectTimeoutTimespan { get; private set; }

			public float PingTime = 5f;
			public TimeSpan PingTimespan { get; private set; }
			public float PingCheckTime = 30f;
			public TimeSpan PingCheckTimespan { get; private set; }
			// Packet
			public bool UseEncrypt = true;
			public bool UseConvertDatetime = false;
			public bool IgnoreTimeZone = true;
			public bool UseProtocolVersion = true;
			public bool UseIPAddressInsteadHost = true;
			public bool IsHeartbeatLog = false;

			protected override void ConfigLoad()
			{
				base.ConfigLoad();
				ConnectTimeoutTimespan = TimeSpan.FromSeconds( ConnectTimeout );
				PingTimespan = TimeSpan.FromSeconds( PingTime );
				PingCheckTimespan = TimeSpan.FromSeconds( PingCheckTime );
			}
		}
		protected ConnectorConfig mConfig = null;
		public ConnectorConfig Config { get { return mConfig; } }

		public ManualResetEvent ConnectedEvent = null;

		protected string mServiceTypeString;
		protected string mHostname = "";
		protected int mPort = 0;
		protected bool mHandleOutput;

		protected virtual PacketVerify CreateVerify() { return new PacketVerify(); }

		protected PacketVerify mVerifyPacket = null;
		protected void SetVerifyString( string verifyString )
		{
			mVerifyPacket.verify_string = verifyString + "." + SendPacketIdType.ToString();
		}

		protected virtual void Verify()
		{
			SendStream( PacketWriteFormatter.Instance.Serialize<PacketVerify>( mVerifyPacket, GetPacketFormatterConfig ) );
		}

		bool m_bConnecting = false;
		public bool Connecting { get { return m_bConnecting; } }

		Stopwatch m_ConnectingWatch = null;

		public string Hostname
		{
			get { return mHostname; }
		}

		public int Port
		{
			get { return mPort; }
		}

		MemoryStream m_PingPacket = null;
		protected override MemoryStream PingPacket { get { return m_PingPacket; } }
		protected override TimeSpan PingTime { get { return mConfig.PingTimespan; } }
		protected override TimeSpan PingCheckTime { get { if( m_TempPingCheckTime != null ) return m_TempPingCheckTime.Value; return mConfig.PingCheckTimespan; } }

		protected Dictionary<short, List<PacketSendInterruptHandlerBase>> mPacketSendInterruptors = new Dictionary<short, List<PacketSendInterruptHandlerBase>>();
		protected Dictionary<string, ModuleNetCoreBase> mModuleDic = new Dictionary<string, ModuleNetCoreBase>();

		public Connector( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type n_send_packet_id_type )
		{
			if( GetType().Equals( packetHandlerManager.SessionType ) == false && GetType().IsSubclassOf( packetHandlerManager.SessionType ) == false )
				throw new Exception( string.Format( "Session Type is wrong {0} != {1} in {2}", GetType().ToString(), packetHandlerManager.SessionType.ToString(), packetHandlerManager.ToString() ) );

			mSessionName = $"{ GetType().Name}:{service_type}";
			mServiceTypeString = service_type;

			Log.Write( "Load Config : " );
			LoadConfig( config_file );

			mSendPacketIdType = send_packet_id_type;
			mNSendPacketIdType = n_send_packet_id_type;

			mVerifyPacket = CreateVerify();
			SetVerifyString( mConfig.VerifyString );
			mVerifyPacket.protocol_version = PacketVersionAttribute.GetVersion( packetHandlerManager.PacketIdType );

			mPacketHandlerManager = packetHandlerManager;
			mPacketHandlerManager.CheckEmptyHandler( mConfig.CheckEmptyHandler );

			RegistPacketSendInterruptHandlerAttribute();
		}

		//------------------------------------------------------------------------
		protected virtual void RegistPacketSendInterruptHandlerAttribute()
		{
			// packet handler automation
			MethodInfo[] handler_methods = GetType().GetAllMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			if( handler_methods != null )
			{
				foreach( MethodInfo method in handler_methods )
				{
					PacketSendInterruptHandlerAttribute handler_attr = method.GetCustomAttribute<PacketSendInterruptHandlerAttribute>();
					if( handler_attr != null )
					{
						Log.WriteImportant( $">> Packet Send Interrupt regist : {method.Name} : {method.DeclaringType.Name}" );

						Type packet_type = handler_attr.PacketType;
						if( packet_type == null )
							throw new Exception( $"packet type({packet_type.Name}) is wrong" );

						PacketAttribute packet_attr = PACKET_CACHE.Attr( packet_type );
						short packetId = packet_attr.GetPacketId( mSendPacketIdType, mNSendPacketIdType );

						Type handler_delegate_type = typeof( PacketSendInterruptHandlerBase.DelegatePacketSendInterruptObjectHandler );
						PacketSendInterruptHandlerBase.DelegatePacketSendInterruptObjectHandler handler_delegate = (PacketSendInterruptHandlerBase.DelegatePacketSendInterruptObjectHandler)Delegate.CreateDelegate( handler_delegate_type, this, method );
						AddPacketSendInterruptor( packet_type, handler_delegate );
					}
				}
			}
		}

		//------------------------------------------------------------------------		
		public virtual ConnectorConfig ConstructConfig() { return new ConnectorConfig(); }
		protected virtual void LoadConfig( string config_file )
		{
			mConfig = ConstructConfig();
			mConfig.ConfigLoad( GlobalConfig.EnvNetPath( config_file ) );

			m_PingPacket = null;
			if( mConfig.PingTime != 0f )
				m_PingPacket = MakePingPacket();

			Log.Write( mConfig.ToString() );
			mHostname = mConfig.Host;
			mPort = mConfig.Port;

			Log.Write( "[ConnectInfo] host:{0}, port:{1}", mHostname, mPort );
			Log.Write( "" );
		}

		//------------------------------------------------------------------------
		public virtual void Connect()
		{
			_Connect();
		}

		//------------------------------------------------------------------------		
		public virtual void ConnectTo( string hostname )
		{
			ConnectTo( hostname, mPort );
		}

		//------------------------------------------------------------------------		
		public virtual void ConnectTo( string hostname, int port )
		{
			this.mHostname = hostname;
			this.mPort = port;

			Log.Write( "[{0}] ConnectTo : {1}:{2}", SessionName, hostname, port );

			_Connect();
		}

		//------------------------------------------------------------------------		
		public override void Close()
		{
			base.Close();
			m_bConnecting = false;
		}

		//------------------------------------------------------------------------		
		public virtual void OnConnected( bool bSuccessed )
		{
			ConnectedEvent = null;

			if( m_Socket != null )
				m_Socket.Blocking = false;

			m_bConnecting = false;

			mHandleOutput = !mConfig.UseEncrypt;

			if( bSuccessed == false )
			{
				Log.WriteWarning( string.Format( "[{0}] Connect Failed : {1}:{2}", SessionName, mHostname, mPort ) );
				if( _OnConnected != null )
					_OnConnected( bSuccessed );

				if( _OnConnectedAdvance != null )
					_OnConnectedAdvance( bSuccessed, this );

				Close();
				bConnected = false;
				return;
			}

			if( mConfig.UseEncrypt == true )
			{
				m_RecvEncryptor = new Encryptor( 0 );
				m_PacketPositionCheck = 4;
			}

			try
			{
				strRemoteEndPoint = m_Socket.RemoteEndPoint.ToString();
				Log.WriteImportant( string.Format( "[{0}] Connected : {1}->{2}", SessionName, m_Socket.LocalEndPoint.ToString(), strRemoteEndPoint ) );
			}
			catch( System.Exception ex )
			{
				Log.WriteError( ex.ToString() );
			}

			bConnected = true;
			bDisconnecting = false;
			m_Socket.SendBufferSize = SendBufferSize;
			m_Socket.ReceiveBufferSize = RecvBufferSize;

			Verify();

			if( _OnConnected != null )
				_OnConnected( bSuccessed );

			if( _OnConnectedAdvance != null )
				_OnConnectedAdvance( bSuccessed, this );
		}

		//------------------------------------------------------------------------		
		public virtual void OnVerifyFinished()
		{
			if( _OnVerified != null )
				_OnVerified();
		}

		//------------------------------------------------------------------------		
		public void ConnectedCallback( IAsyncResult ar )
		{
			try
			{
				( (ConnectedCallbackState)ar.AsyncState ).socket.EndConnect( ar );
			}
			catch( Exception )
			{
			}

			( (ConnectedCallbackState)ar.AsyncState ).connectedEvent.Set();
		}

		//------------------------------------------------------------------------		
		protected IPAddress GetIPAddress( string hostname )
		{
			try
			{
				if( string.IsNullOrEmpty( hostname ) )
					return null;

				IPAddress[] addresses = Dns.GetHostAddresses( hostname );
				foreach( IPAddress ip_addr in addresses )
				{
					if( ip_addr.Equals( IPAddress.Any ) || ip_addr.Equals( IPAddress.IPv6Any ) )
						continue;

					if( ip_addr.AddressFamily == AddressFamily.InterNetwork || ip_addr.AddressFamily == AddressFamily.InterNetworkV6 )
						return ip_addr;
				}
			}
			catch( System.Exception ex )
			{
				Log.WriteError( ex.ToString() );
			}

			return null;
		}

		//------------------------------------------------------------------------		
		protected virtual void _Connect()
		{
			if( m_bConnecting == true )
			{
				Log.Write( "Already trying connect" );
				//throw new Exception( "Already trying connect" );
				return;
			}

			if( Connected == true )
			{
				Disconnect( 0, string.Format( "Connect to {0}:{1}", Hostname, mPort.ToString() ) );
			}

			m_DisconnectType = eDisconnectType.Disconnected;
			m_Disconnect.error_code = (int)eDisconnectErrorCode.UnknownError;
			m_Disconnect.error_detail_string = m_Disconnect.error_string = "Unknown Error";

			AddressFamily addr_family = AddressFamily.InterNetwork;

			IPAddress ip_addr = null;
			if( mConfig.UseIPAddressInsteadHost )
			{
				ip_addr = GetIPAddress( mHostname );
				if( ip_addr == null )
				{
					OnConnected( false );
					return;
				}

				addr_family = ip_addr.AddressFamily;
				Log.WriteImportant( "ConnectIP({0}) Family:{1} IPAddress:{2}", mHostname, ip_addr.AddressFamily, ip_addr.ToString() );
			}

			m_Socket = new Socket( addr_family, SocketType.Stream, ProtocolType.Tcp );
			m_Socket.NoDelay = true;
			//m_Socket.LingerState = new LingerOption(true, 1);

			if( mConfig.UseAsyncConnect == false )
			{
				try
				{
					if( mConfig.UseIPAddressInsteadHost )
						m_Socket.Connect( ip_addr, mPort );
					else
						m_Socket.Connect( mHostname, mPort );
					OnConnected( true );
				}
				catch( SocketException ex )
				{
					if( ex.SocketErrorCode == SocketError.ConnectionRefused || ex.SocketErrorCode == SocketError.TimedOut )
						OnConnected( false );
					else
						throw ex;
				}
			}
			else
			{
				m_bConnecting = true;
				m_ConnectingWatch = Stopwatch.StartNew();
				ConnectedEvent = new ManualResetEvent( false );

				ConnectedCallbackState state;
				state.socket = m_Socket;
				state.connectedEvent = ConnectedEvent;
				if( mConfig.UseIPAddressInsteadHost )
					m_Socket.BeginConnect( ip_addr, mPort, new AsyncCallback( ConnectedCallback ), state );
				else
					m_Socket.BeginConnect( mHostname, mPort, new AsyncCallback( ConnectedCallback ), state );
			}
		}

		//------------------------------------------------------------------------		
		public void Update()
		{
			if( m_Socket == null )
			{
				if( bConnected == true )
				{
					OnDisconnected();
				}
				return;
			}

			if( m_bConnecting == true )
			{
				if( ConnectedEvent.WaitOne( 0 ) == true )
				{
					OnConnected( m_Socket.Connected );
					return;
				}
				else
				{
					if( m_ConnectingWatch.Elapsed > mConfig.ConnectTimeoutTimespan )
					{
						OnConnected( false );
					}

					return;
				}
			}

			if( m_Socket.Connected == true )
			{
				try
				{
					handle_input();
				}
				catch( System.Exception ex )
				{
					Disconnect( (int)eDisconnectErrorCode.PacketInputError, ex.Message, ex.ToString() );
				}
			}

			handle_packets();

			if( m_Socket == null )
				return;

			if( m_Socket.Connected == true && mHandleOutput == true )
			{
				try
				{
					handle_output();
				}
				catch( System.Exception ex )
				{
					Disconnect( (int)eDisconnectErrorCode.SystemError, ex.Message, ex.ToString() );
				}
			}

			if( bDisconnecting == true || bConnected == true && m_Socket != null && m_Socket.Connected == false )
			{
				bConnected = false;
				OnDisconnected();
			}

			if( mHandleOutput == true )
				CheckPing();
		}

		//------------------------------------------------------------------------		
		protected override void OnDisconnected()
		{
			if( m_Disconnect.error_code >= 0 )
			{
				// ingame throw
				if( m_Disconnect.error_code > (int)eDisconnectErrorCode.Err_CustomBegin )
				{
					if( m_Disconnect.error_string == m_Disconnect.error_detail_string )
						Log.WriteWarning( string.Format( "[{0}] OnDisconnected({1}:{2}) {3}, {4}, {5}", SessionName, mHostname, mPort, m_DisconnectType, m_Disconnect.ErrorCodeString, m_Disconnect.error_string ) );
					else
						Log.WriteWarning( string.Format( "[{0}] OnDisconnected({1}:{2}) {3}, {4}, {5}, {6}", SessionName, mHostname, mPort, m_DisconnectType, m_Disconnect.ErrorCodeString, m_Disconnect.error_string, m_Disconnect.error_detail_string ) );
				}
				else
				{
					if( m_Disconnect.error_string == m_Disconnect.error_detail_string )
						Log.Write( string.Format( "[{0}] OnDisconnected({1}:{2}) {3}, {4}, {5}", SessionName, mHostname, mPort, m_DisconnectType, m_Disconnect.ErrorCodeString, m_Disconnect.error_string ) );
					else
						Log.Write( string.Format( "[{0}] OnDisconnected({1}:{2}) {3}, {4}, {5}, {6}", SessionName, mHostname, mPort, m_DisconnectType, m_Disconnect.ErrorCodeString, m_Disconnect.error_string, m_Disconnect.error_detail_string ) );

				}
			}
			else
			{
				if( m_Disconnect.error_string == m_Disconnect.error_detail_string )
					Log.WriteWarning( string.Format( "[{0}] OnDisconnected({1}:{2}) {3}, {4}, {5}", SessionName, mHostname, mPort, m_DisconnectType, m_Disconnect.ErrorCodeString, m_Disconnect.error_string ) );
				else
					Log.WriteWarning( string.Format( "[{0}] OnDisconnected({1}:{2}) {3}, {4}, {5}, {6}", SessionName, mHostname, mPort, m_DisconnectType, m_Disconnect.ErrorCodeString, m_Disconnect.error_string, m_Disconnect.error_detail_string ) );
			}

			base.OnDisconnected();
		}

		//------------------------------------------------------------------------		
		public override bool Disconnect( int error_code, string error_string, string error_detail_string )
		{
			if( base.Disconnect( error_code, error_string, error_detail_string ) == false )
				return false;

			Update();
			Close();

			return true;
		}

		//------------------------------------------------------------------------		
		protected override void handle_packet( PacketContainer packet )
		{
			if( packet.packet is MemoryStream )
			{
				MemoryStream stream = packet.packet as MemoryStream;
				short packetId = BitConverter.ToInt16( stream.GetBuffer(), 2 );
				stream.Seek( 4, SeekOrigin.Begin );

				if( packetId == PacketCoreID.DisconnectPacketId )
				{
					PacketDisconnect disconnect = PacketReadFormatter.Instance.Serialize<PacketDisconnect>( new BinaryReader( stream ), GetPacketFormatterConfig );
					SetDisconnectInfo( disconnect );
					return;
				}
				else if( packetId == VerifiedAckPacketId )
				{
					PacketVerified _PacketVerified = PacketReadFormatter.Instance.Serialize<PacketVerified>( new BinaryReader( stream ), GetPacketFormatterConfig );
					if( mConfig.UseProtocolVersion == true )
					{
						m_PacketFormatterConfig.protocol_version = Math.Min( PacketVersionAttribute.GetVersion( mPacketHandlerManager.PacketIdType ), _PacketVerified.protocol_version );
						m_Verified = true;
						ProcessWaitVerifyPackets();
						Log.Write( "---------- ProtocolVersion : {0}", m_PacketFormatterConfig.protocol_version );
					}
					OnVerifyFinished();
					return;
				}
				else if( packetId == PacketCoreID.HeartbeatId )
				{
					PacketHeartbeat heartbeat = PacketReadFormatter.Instance.Serialize<PacketHeartbeat>( new BinaryReader( stream ), GetPacketFormatterConfig );

					if( mConfig.IsHeartbeatLog )
						Log.WriteImportant( PacketLogFormatter.Instance.Serialize<PacketHeartbeat>( heartbeat ) );

					if( mConfig.UseProtocolVersion == true && m_PacketFormatterConfig.protocol_version == -1 )
						m_PacketFormatterConfig.protocol_version = 1;

					if( mConfig.UseEncrypt == true && mHandleOutput == false )
					{
						m_SendEncryptor = new Encryptor( heartbeat.heartbeat_key );
						mHandleOutput = true;
					}

					PacketHeartbeatAck heartbeatAck = new PacketHeartbeatAck();
					heartbeatAck.heartbeat_key = Encryptor.NextValue( heartbeat.heartbeat_key );

					SendStream( PacketWriteFormatter.Instance.Serialize( heartbeatAck, GetPacketFormatterConfig ) );

					if( mConfig.IsHeartbeatLog )
						Log.WriteImportant( PacketLogFormatter.Instance.Serialize<PacketHeartbeatAck>( heartbeatAck ) );

					return;
				}
				else if( packetId == PacketCoreID.ModulePacket )
				{
					OnModulePacketHandler( PacketReadFormatter.Instance.Serialize<PacketModule>( new BinaryReader( stream ), GetPacketFormatterConfig ) );
					return;
				}

				throw new Exception( "wrong packet in handle_packet : " + packetId.ToString() );
			}
			else
				base.handle_packet( packet );
		}

		//------------------------------------------------------------------------		
		public eSendPacketResult SendPacket<SendT, RecvT>( SendT packet, delegatePacketReceiverHandler<RecvT> compact_handler ) where SendT : class
		{
			if( compact_handler != null )
				mPacketHandlerManager.AddReceiver<RecvT>( compact_handler, true );

			return SendPacket<SendT>( packet );
		}
		public override eSendPacketResult SendPacket<PacketType>( PacketType packet )
		{
			PacketAttribute attr = PACKET<PacketType>.Attr;
			if( attr.IsVersion( GetPacketFormatterConfig.protocol_version ) == false )
				return eSendPacketResult.VersionLower;

			return base.SendPacket( packet );
		}

		//------------------------------------------------------------------------
		public virtual void AddPacketSendInterruptor<PT>( PacketSendInterruptHandlerBase.DelegatePacketSendInterruptHandler<PT> interruptor ) where PT : class
		{
			PacketAttribute attr = PACKET<PT>.Attr;
			short packet_id = attr.GetPacketId( mSendPacketIdType, mNSendPacketIdType );

			List<PacketSendInterruptHandlerBase> list;
			if( mPacketSendInterruptors.TryGetValue( packet_id, out list ) == false )
			{
				list = new List<PacketSendInterruptHandlerBase>();
				mPacketSendInterruptors.Add( packet_id, list );
			}

			list.Add( new PacketSendInterruptHandler<PT>( interruptor ) );
		}

		//------------------------------------------------------------------------
		public virtual void AddPacketSendInterruptor( Type packet_type, PacketSendInterruptHandlerBase.DelegatePacketSendInterruptObjectHandler interruptor )
		{
			PacketAttribute attr = PACKET_CACHE.Attr( packet_type );
			short packet_id = attr.GetPacketId( mSendPacketIdType, mNSendPacketIdType );

			List<PacketSendInterruptHandlerBase> list;
			if( mPacketSendInterruptors.TryGetValue( packet_id, out list ) == false )
			{
				list = new List<PacketSendInterruptHandlerBase>();
				mPacketSendInterruptors.Add( packet_id, list );
			}

			list.Add( new PacketSendInterruptObjectHandler( interruptor ) );
		}
		
		//------------------------------------------------------------------------
		protected override List<PacketSendInterruptHandlerBase> GetPacketSendInterruptors( short packet_id )
		{
			if( mPacketSendInterruptors.Count > 0 )
			{
				List<PacketSendInterruptHandlerBase> interruptor_list;
				if( mPacketSendInterruptors.TryGetValue( packet_id, out interruptor_list ) )
					return interruptor_list;
			}

			return null;
		}

		//------------------------------------------------------------------------
		public virtual void AddModule( ModuleNetCoreBase module )
		{
			if( mModuleDic.ContainsKey( module.ModuleName ) )
				throw new Exception( $"AddModule : Module already exist : {module.ModuleName}" );

			mModuleDic.Add( module.ModuleName, module );
		}

		//------------------------------------------------------------------------
		public virtual void OnModulePacketHandler( PacketModule packet_module )
		{
			ModuleNetCoreBase module;
			if( mModuleDic.TryGetValue( packet_module.module_name, out module ) == false )
				throw new Exception( $"OnModulePacketHandler : module not found : {packet_module.module_name}" );

			module.OnPacketReceived( this, packet_module.packet_id, packet_module.packet_stream );
		}
	}
}