//////////////////////////////////////////////////////////////////////////
//
// ApplicationBase
// 
// Created by LCY.
//
// Copyright 2022 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////
using System;
using System.Net.Sockets;
using UMF.Net;
using UMF.Server;
using UMF.Server.Net;
using System.Xml;
using System.Diagnostics;
using System.IO;
using UMF.Core;
using System.Reflection;

namespace UMP.Server
{
	//------------------------------------------------------------------------	
	public class AppPacketHandlerManager<ST> : PacketHandlerManager<ST> where ST : Session
	{
		protected UMPServerApplication mUMPApplication = null;
		public UMPServerApplication UMP_APPLICATION { get { return mUMPApplication; } }
		protected override bool DoPacketHandlerAttributeRegist => false;

		public AppPacketHandlerManager( UMPServerApplication application, Type packet_id_type, Type n_packet_id_type )
			: base( packet_id_type, n_packet_id_type )
		{
			mUMPApplication = application;

			MethodInfo[] handler_methods = GetType().GetAllMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			if( handler_methods != null )
			{
				foreach( MethodInfo method in handler_methods )
				{
					PacketHandlerAttribute handler_attr = method.GetCustomAttribute<PacketHandlerAttribute>();
					if( handler_attr != null )
					{
						Log.WriteImportant( $">> PacketHandler regist : {method.Name} : {method.DeclaringType.Name}" );

						Type packet_type = handler_attr.PacketType;
						if( packet_type == null )
							throw new Exception( "packet type is wrong" );

						PacketAttribute packet_attr = PACKET_CACHE.Attr( packet_type );
						short packetId = packet_attr.GetPacketId( PacketIdType, NPacketIdType );

						if( m_Handlers.ContainsKey( packetId ) == true )
							throw new Exception( "Already exist packetId : " + packet_type.FullName );

						if( m_Deserializer.ContainsKey( packetId ) == false )
							m_Deserializer.Add( packetId, new PacketDeserializer( packet_type ) );

						m_Handlers.Add( packetId, CreateAutoHandler( application, method, handler_attr ) );
					}
				}
			}
		}

		//------------------------------------------------------------------------
		public virtual PacketObjectHandler<ST> CreateAutoHandler( UMPServerApplication application, MethodInfo method, PacketHandlerAttribute handler_attr )
		{
			Type handler_delegate_type = typeof( DelegatePacketObjectHandler<ST> );
			DelegatePacketObjectHandler<ST> handler_delegate = (DelegatePacketObjectHandler<ST>)Delegate.CreateDelegate( handler_delegate_type, this, method );
			PacketObjectHandler<ST> handler = new PacketObjectHandler<ST>( handler_attr.PacketType, handler_delegate );
			return handler;
		}
	}

	//------------------------------------------------------------------------
	public class AppPeerManager<ST> : TPeerManager<ST> where ST : AppPeer, new()
	{
		protected UMPServerApplication mUMPApplication = null;
		public UMPServerApplication UMP_APPLICATION { get { return mUMPApplication; } }

		// peer index save
		protected virtual string PEER_INDEX_SAVE_FILE_NAME { get; } = "";
		protected XmlDocument mPeerIndexDoc = new XmlDocument();
		protected int mPeerIndexLast = 0;
		public int PeerIndexSaveIntervalSeconds { get; set; } = 60;
		protected Stopwatch mPeerIndexSaveTimer = null;
		protected bool mPeerIndexSaveUse = false;
		protected string mPeerIndexSaveRealFile = "";


		public AppPeerManager( UMPServerApplication application, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type N_send_packet_id_type ) 
			: base( application.ServiceType.ToString(), config_file, packetHandlerManager, send_packet_id_type, N_send_packet_id_type )
		{
			mUMPApplication = application;
			mUMPApplication.AddListener( this );

			UseParallel = application.GetApplicationConfig.UseParallel;

			mPeerIndexSaveUse = false;
			if( string.IsNullOrEmpty( PEER_INDEX_SAVE_FILE_NAME ) == false )
			{
				string ext = Path.GetExtension( PEER_INDEX_SAVE_FILE_NAME );
				mPeerIndexSaveRealFile = string.Format( "{0}_{1}{2}", PEER_INDEX_SAVE_FILE_NAME.Replace( ext, "" ),
					FileUtil.ValidFileNameConvert( mUMPApplication.ServerName, '_' ), ext );
				try
				{
					if( File.Exists( mPeerIndexSaveRealFile ) )
					{
						mPeerIndexDoc.Load( mPeerIndexSaveRealFile );
						mNextPeerIndex = int.Parse( mPeerIndexDoc.DocumentElement.Attributes["index"].Value );
						if( mNextPeerIndex < mPeerIndexMin || mNextPeerIndex > mPeerIndexMax )
							mNextPeerIndex = mPeerIndexMin;
					}
					else
					{
						XmlNode node = mPeerIndexDoc.AppendChild( mPeerIndexDoc.CreateElement( "PeerIndex" ) );
						XmlAttribute index_attr = mPeerIndexDoc.CreateAttribute( "index" );
						index_attr.Value = mNextPeerIndex.ToString();
						mPeerIndexDoc.DocumentElement.Attributes.Append( index_attr );
					}

					mPeerIndexSaveUse = true;
				}
				catch( System.Exception ex )
				{
					Log.WriteWarning( ex.ToString() );
					mPeerIndexSaveUse = false;
				}

				if( mPeerIndexSaveUse )
				{
					mPeerIndexSaveTimer = Stopwatch.StartNew();
					mPeerIndexLast = mNextPeerIndex;
					mPeerIndexDoc.DocumentElement.Attributes["index"].Value = mPeerIndexLast.ToString();
					mPeerIndexDoc.Save( mPeerIndexSaveRealFile );
					Log.WriteImportant( "# Client Peer Index start {0}", mNextPeerIndex );
				}
			}
		}

		//------------------------------------------------------------------------
		protected override Peer CreateNewPeer( Socket socket )
		{
			ST peer = new ST();
			peer.Init( mUMPApplication, this, socket );
			return peer;
		}

		//------------------------------------------------------------------------
		protected override void UpdatedPeers()
		{
			base.UpdatedPeers();
			mUMPApplication.RefreshTitleString = true;
		}

		//------------------------------------------------------------------------
		public override void Update()
		{
			base.Update();

			if( mPeerIndexSaveUse && mPeerIndexSaveTimer != null )
			{
				if( mPeerIndexSaveTimer.Elapsed.TotalSeconds >= PeerIndexSaveIntervalSeconds )
				{
					mPeerIndexSaveTimer.Restart();

					if( mPeerIndexLast != mNextPeerIndex )
					{
						mPeerIndexLast = mNextPeerIndex;
						mPeerIndexDoc.DocumentElement.Attributes["index"].Value = mPeerIndexLast.ToString();
						mPeerIndexDoc.Save( mPeerIndexSaveRealFile );
					}
				}
			}
		}
	}

	//------------------------------------------------------------------------
	public class AppPeer : Peer
	{
		protected UMPServerApplication mUMPApplication = null;
		public UMPServerApplication UMP_APPLICATION { get { return mUMPApplication; } }

		//------------------------------------------------------------------------
		public virtual void Init( UMPServerApplication application, PeerManagerBase peerManager, Socket socket )
		{
			mUMPApplication = application;
			base.Init( peerManager, socket );
		}

		//------------------------------------------------------------------------
		protected override void OnVerified( object userInfo, PacketVerify verify )
		{
			base.OnVerified( userInfo, verify );
			mUMPApplication.RefreshTitleString = true;
		}
	}

	//------------------------------------------------------------------------
	public class AppConnector : Connector
	{
		protected UMPServerApplication mUMPApplication = null;
		public UMPServerApplication UMP_APPLICATION { get { return mUMPApplication; } }

		public AppConnector( UMPServerApplication application, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type n_send_packet_id_type ) 
			: base( application.ServiceType.ToString(), config_file, packetHandlerManager, send_packet_id_type, n_send_packet_id_type )
		{
			mUMPApplication = application;
			mUMPApplication.AddConnector( this );
		}		
	}

	//------------------------------------------------------------------------
	public class AppSSRelayPeerManager<ST> : TSSRelayPeerManager<ST> where ST : AppSSRelayPeer, new()
	{

		protected UMPServerApplication mUMPApplication = null;
		public UMPServerApplication UMP_APPLICATION { get { return mUMPApplication; } }

		public AppSSRelayPeerManager( UMPServerApplication application, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type N_send_packet_id_type, SSRelayPeerManager relay_peer_manager )
			: base( application.ServiceType.ToString(), config_file, packetHandlerManager, send_packet_id_type, N_send_packet_id_type, relay_peer_manager )
		{
			mUMPApplication = application;
			mUMPApplication.AddListener( this );

			UseParallel = application.GetApplicationConfig.UseParallel;
		}

		//------------------------------------------------------------------------
		protected override Peer CreateNewPeer( Socket socket )
		{
			ST peer = new ST();
			peer.Init( mUMPApplication, this, socket );
			return peer;
		}

		//------------------------------------------------------------------------
		protected override void UpdatedPeers()
		{
			base.UpdatedPeers();
			mUMPApplication.RefreshTitleString = true;
		}
	}

	//------------------------------------------------------------------------
	public class AppSSRelayPeer : SSPeer
	{
		protected UMPServerApplication mUMPApplication = null;
		public UMPServerApplication UMP_APPLICATION { get { return mUMPApplication; } }

		public virtual void Init( UMPServerApplication application, PeerManagerBase peerManager, Socket socket )
		{
			mUMPApplication = application;
			base.Init( peerManager, socket );
		}

		//------------------------------------------------------------------------
		protected override void OnVerified( object userInfo, PacketVerify verify )
		{
			base.OnVerified( userInfo, verify );
			mUMPApplication.RefreshTitleString = true;
		}
	}

	//------------------------------------------------------------------------
	public class AppSSRelayConnector : SSRelayConnector 
	{
		protected UMPServerApplication mUMPApplication = null;
		public UMPServerApplication UMP_APPLICATION { get { return mUMPApplication; } }

		public AppSSRelayConnector( UMPServerApplication application, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type n_send_packet_id_type, PeerManagerBase relay_peer_manager )
			: base( application.ServiceType.ToString(), config_file, packetHandlerManager, send_packet_id_type, n_send_packet_id_type, relay_peer_manager )
		{
			mUMPApplication = application;
			mUMPApplication.AddConnector( this );
		}
	}
}
