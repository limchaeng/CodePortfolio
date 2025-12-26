//////////////////////////////////////////////////////////////////////////
//
// Listener
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
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using UMF.Core;

namespace UMF.Net
{
	//------------------------------------------------------------------------	
	public abstract class Listener
	{
		//------------------------------------------------------------------------	
		public class ListenerConfig : EnvConfig
		{
			// root
			public string VerifyString = "_UMF_";
			public int PeerIndexMin = 1;
			public int PeerIndexMax = 100000000;
			// PacketLog
			public Core.eCoreLogType CoreLogType = Core.eCoreLogType.None;
			public bool LogMulticastId = true;
			public eCheckEmptyHandlerType CheckEmptyHandler = eCheckEmptyHandlerType.Exception;
			// Bind
			public string BindHost = "";
			public bool BindIPAny = true;
			public float PingTime = 5f;
			public TimeSpan PingTimespan { get; private set; }
			public float PingCheckTime = 30f;
			public TimeSpan PingCheckTimespan { get; private set; }
			// BindPort
			public string BindPort = "5000";
			public short BindPortBegin { get; private set; }
			public short BindPortEnd { get; private set; }
			// BufferSize
			public int SendBufferSize = 4096;
			public int RecvBufferSize = 4096;
			// Heartbeat
			public float HeartbeatTime = 60f;
			public TimeSpan HeartbeatTimespan { get; private set; }
			public float HeartbeatFirstTime = 30f;
			public TimeSpan HeartbeatFirstTimespan { get; private set; }
			public bool IsHeartbeatLog = false;
			// Limit
			public int RecvLimit = 5;
			public int AcceptLimit = 300;
			// Packet
			public bool UseEncrypt = true;
			public bool UseConvertDatetime = false;
			public bool IgnoreTimeZone = true;
			public bool UseProtocolVersion = true;

			protected override void ConfigLoad()
			{
				base.ConfigLoad();

				short def_bindport;
				if( short.TryParse( BindPort, out def_bindport ) == false )
					def_bindport = 5000;

				BindPortBegin = def_bindport;
				BindPortEnd = def_bindport;

				if( BindPort.Contains( "-" ) )
				{
					string[] bind_port_range = BindPort.Split( new char[] { '-' } );
					if( bind_port_range != null )
					{
						if( bind_port_range.Length > 0 )
						{
							BindPortBegin = short.Parse( bind_port_range[0] );
							BindPortEnd = (short)( BindPortBegin + 1000 );
						}

						if( bind_port_range.Length > 1 )
						{
							short end = short.Parse( bind_port_range[1] );
							if( end > BindPortBegin )
								BindPortEnd = end;
						}
					}
				}

				PingTimespan = TimeSpan.FromSeconds( PingTime );
				PingCheckTimespan = TimeSpan.FromSeconds( PingCheckTime );

				HeartbeatTimespan = TimeSpan.FromSeconds( HeartbeatTime );
				HeartbeatFirstTimespan = TimeSpan.FromSeconds( HeartbeatFirstTime );
			}
		}


		Stopwatch time = Stopwatch.StartNew();
		TimeSpan span = TimeSpan.FromSeconds( 1 );

		bool m_bResetRecvLeft;

		protected IPAddress mBoundHost = null;
		public virtual IPAddress BoundHost { get { return mBoundHost; } }
		public virtual string NotifyHostName { get { return ( mBoundHost != null ? mBoundHost.ToString() : "" ); } }

		protected short mBoundPort = 0;
		public virtual short BoundPort { get { return mBoundPort; } }

		protected ListenerConfig mListenerConfig = null;
		public ListenerConfig GetListenerConfig { get { return mListenerConfig; } }
		public virtual eCheckEmptyHandlerType CheckEmptyHandler { get { return mListenerConfig.CheckEmptyHandler; } }

		protected Type mSendPacketIdType;		
		protected Type mNSendPacketIdType;
		public Type SendPacketIdType { get { return mSendPacketIdType; } }
		public Type NSendPacketIdType { get { return mNSendPacketIdType; } }
		public Type RecvPacketIdType { get { return mPacketHandlerManager.PacketIdType; } }
		public Type NRecvPacketIdType { get { return mPacketHandlerManager.NPacketIdType; } }

		protected string mServiceTypeString;
		public string ServiceTypeString { get { return mServiceTypeString; } }
		protected string mListenerName = "Listener";
		public virtual string ListenerName { get { return mListenerName; } }

		public bool UseParallel { get; set; }

		public MemoryStream PingPacket = null;

		protected PacketHandlerManagerBase mPacketHandlerManager = null;
		public PacketHandlerManagerBase PacketHandlerManager { get { return mPacketHandlerManager; } }

		public int sendCount = 0, recvCount = 0, sendBytes = 0, recvBytes = 0, sendLeft = 0;

		public int PeerCount
		{
			get
			{
				return m_SelectSockets.Count;
			}
		}

		Socket m_ListenSocket = new Socket( AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp );

		protected ConcurrentQueue<Peer> m_RemovePeers = new ConcurrentQueue<Peer>();
		protected List<Peer> m_SelectSockets = new List<Peer>();
		protected abstract Peer CreateNewPeer( Socket socket );

		Action mOnListenerStarted = null;
		public Action OnListenerStarted { set { mOnListenerStarted = value; } }

		//------------------------------------------------------------------------		
		public Listener( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type N_send_pacekt_id_type )
		{
			mSendPacketIdType = send_packet_id_type;
			mNSendPacketIdType = N_send_pacekt_id_type;

			mServiceTypeString = service_type;
			mListenerName = $"{GetType().Name}:{mServiceTypeString}";
			UseParallel = false;

			LoadConfig( config_file );

			mPacketHandlerManager = packetHandlerManager;
			mPacketHandlerManager.CheckEmptyHandler( mListenerConfig.CheckEmptyHandler );
		}

		//------------------------------------------------------------------------		
		public virtual ListenerConfig ConstructConfig() { return new ListenerConfig(); }

		protected virtual void LoadConfig( string config_file )
		{
			mListenerConfig = ConstructConfig();
			mListenerConfig.ConfigLoad( GlobalConfig.EnvNetPath( config_file ) );
			Log.Write( mListenerConfig.ToString() );
			Log.Write( "" );

			PingPacket = Session.MakePingPacket();
		}

		//------------------------------------------------------------------------		
		public virtual void Close()
		{
			m_ListenSocket.Close();
			foreach( Peer peer in m_SelectSockets )
			{
				peer.Close();
			}
			m_SelectSockets.Clear();

			Peer p;
			while( m_RemovePeers.TryDequeue( out p ) ) { }
		}

		//------------------------------------------------------------------------		
		public void Start()
		{
			string[] hosts = null;
			if( mListenerConfig.BindHost != "localhost" )
				hosts = mListenerConfig.BindHost.Split( '.' );

			IPAddress[] ipAddresses = Dns.GetHostAddresses( Dns.GetHostName() );
			IPAddress availableipAddress = null;
			Log.Write( "---- NETWORK ----" );
			foreach( IPAddress address in ipAddresses )
			{
				Log.Write( "- address : {0}/{1}", address.ToString(), address.AddressFamily.ToString() );
				if( address.AddressFamily == AddressFamily.InterNetwork )
				{
					bool bFind = true;

					if( hosts != null )
					{
						byte[] ad = address.GetAddressBytes();
						for( int i = 0; i < 4; ++i )
						{
							if( hosts[i] != "*" && byte.Parse( hosts[i] ) != ad[i] )
							{
								bFind = false;
								break;
							}
						}
						if( bFind == true )
						{
							availableipAddress = address;
							break;
						}
					}
					else
					{
						availableipAddress = address;
						break;
					}
				}
				else if( address.AddressFamily == AddressFamily.InterNetworkV6 )
				{
					if( address.ToString().Contains( mListenerConfig.BindHost ) )
					{
						availableipAddress = address;
						break;
					}
				}
			}

			// check available port
			IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
			IPEndPoint[] ipListeners = ipGlobalProperties.GetActiveTcpListeners();
			short availablePort = mListenerConfig.BindPortBegin;
			for( short port = mListenerConfig.BindPortBegin; port <= mListenerConfig.BindPortEnd; port++ )
			{
				bool isAvailable = true;
				foreach( IPEndPoint endPoint in ipListeners )
				{
					//Log.Write("-- listener info:" + endPoint.ToString());

					if( endPoint.Port == port )
					{
						isAvailable = false;
						break;
					}
				}

				if( isAvailable == true )
				{
					availablePort = port;
					break;
				}
			}

			m_ListenSocket.SetSocketOption( SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false );

			IPEndPoint localEndPoint = null;
			if( mListenerConfig.BindIPAny )
				localEndPoint = new IPEndPoint( IPAddress.IPv6Any, availablePort );
			else
				localEndPoint = new IPEndPoint( availableipAddress, availablePort );

			m_ListenSocket.Bind( localEndPoint );
			mBoundHost = availableipAddress;
			mBoundPort = availablePort;
			Log.Write( "[{0}] Bound : {1} (ALL:{2})", ListenerName, localEndPoint.ToString(), mListenerConfig.BindIPAny );

			// start listening
			m_ListenSocket.Listen( mListenerConfig.AcceptLimit );
			Log.Write( "[{0}] Listen Started", ListenerName );

			m_ListenSocket.Blocking = false;

			if( mOnListenerStarted != null )
				mOnListenerStarted();

			UpdateAccept();
		}

		//------------------------------------------------------------------------		
		protected virtual Peer handle_accept( Socket socket )
		{
			socket.Blocking = false;

			Peer newPeer = CreateNewPeer( socket );

			Log.Write( "[{0}] Accepted from {1}", ListenerName, socket.RemoteEndPoint.ToString() );

			newPeer.m_RecvLeft = 1;
			m_SelectSockets.Add( newPeer );

			return newPeer;
		}

		//------------------------------------------------------------------------		
		void UpdateAccept()
		{
			try
			{
				Socket acceptedSocket;
				while( ( acceptedSocket = m_ListenSocket.Accept() ) != null )
				{
					handle_accept( acceptedSocket );
					UpdatedPeers();
				}
			}
			catch( SocketException ex )
			{
				if( ex.SocketErrorCode != SocketError.WouldBlock )
					throw ex;
			}
		}

		//------------------------------------------------------------------------		
		void handle_input()
		{
			if( m_SelectSockets.Count == 0 )
				return;

			if( UseParallel == true )
			{
				System.Threading.Tasks.Parallel.ForEach( m_SelectSockets, peer =>
				{
					try
					{
						peer.handle_output();

						if( peer.Connected == true )
						{
							if( peer.Verified == true && m_bResetRecvLeft == true )
								peer.m_RecvLeft = mListenerConfig.RecvLimit;
						}
						else
							peer.m_RecvLeft = -1;

						peer.handle_input();
					}
					catch( PacketException ex )
					{
						peer.Disconnect( ex.ErrorCode, ex.Message, ex.ToString() );
					}
					catch( System.Exception ex )
					{
						peer.Disconnect( (int)eDisconnectErrorCode.SystemError, ex.Message, ex.ToString() );
					}
				} );
			}
			else
			{
				foreach( Peer peer in m_SelectSockets )
				{
					try
					{
						peer.handle_output();

						if( peer.Connected == true )
						{
							if( peer.Verified == true && m_bResetRecvLeft == true )
								peer.m_RecvLeft = mListenerConfig.RecvLimit;
						}
						else
							peer.m_RecvLeft = -1;

						peer.handle_input();
					}
					catch( PacketException ex )
					{
						peer.Disconnect( ex.ErrorCode, ex.Message, ex.ToString() );
					}
					catch( System.Exception ex )
					{
						peer.Disconnect( (int)eDisconnectErrorCode.PacketInputError, ex.Message, ex.ToString() );
					}
				}
			}
		}

		//------------------------------------------------------------------------		
		void handle_packet()
		{
			foreach( Peer peer in m_SelectSockets )
			{
				peer.handle_packets();
			}
		}

		//------------------------------------------------------------------------		
		void CheckDisconnecteds()
		{
			sendLeft = 0;
			foreach( Peer peer in m_SelectSockets )
			{
				peer.CheckDisconnected();
				sendCount += peer.sendCount; peer.sendCount = 0;
				recvCount += peer.recvCount; peer.recvCount = 0;
				sendBytes += peer.sendBytes; peer.sendBytes = 0;
				recvBytes += peer.recvBytes; peer.recvBytes = 0;
				sendLeft += peer.SendPacketCount;
			}
			RemovePeers();
		}

		//------------------------------------------------------------------------		
		public virtual void Update()
		{
			if( time.Elapsed > span )
			{
				m_bResetRecvLeft = true;
				time.Restart();
			}
			else
				m_bResetRecvLeft = false;

			UpdateAccept();

			handle_input();
			handle_packet();
			CheckDisconnecteds();
		}

		//------------------------------------------------------------------------		
		public virtual void RemovePeer( Peer peer )
		{
			m_RemovePeers.Enqueue( peer );
		}

		//------------------------------------------------------------------------		
		void RemovePeers()
		{
			if( m_RemovePeers.Count == 0 )
				return;

			Peer peer;
			while( m_RemovePeers.TryDequeue( out peer ) )
			{
				m_SelectSockets.Remove( peer );
			}

			UpdatedPeers();
		}

		//------------------------------------------------------------------------		
		public void DisconnectAll( int error_code, string error_string )
		{
			DisconnectAll( error_code, error_string, error_string );
		}

		//------------------------------------------------------------------------		
		public virtual void DisconnectAll( int error_code, string error_string, string error_detail_string )
		{
			if( error_code == 0 )
			{
				if( error_string == error_detail_string )
					Log.Write( string.Format( "[{0}] DisconnectAll({1}) : {2}, {3}", ListenerName, m_SelectSockets.Count, error_code, error_string ) );
				else
					Log.Write( string.Format( "[{0}] DisconnectAll({1}) : {2}, {3}, {4}", ListenerName, m_SelectSockets.Count, error_code, error_string, error_detail_string ) );
			}
			else
			{
				if( error_string == error_detail_string )
					Log.WriteWarning( string.Format( "[{0}] DisconnectAll({1}) : {2}, {3}", ListenerName, m_SelectSockets.Count, error_code, error_string ) );
				else
					Log.WriteWarning( string.Format( "[{0}] DisconnectAll({1}) : {2}, {3}, {4}", ListenerName, m_SelectSockets.Count, error_code, error_string, error_detail_string ) );
			}

			foreach( Peer peer in m_SelectSockets )
			{
				peer.Disconnect( error_code, error_string, error_detail_string );
			}
		}

		//------------------------------------------------------------------------
		protected virtual void UpdatedPeers()
		{
		}
	}
}