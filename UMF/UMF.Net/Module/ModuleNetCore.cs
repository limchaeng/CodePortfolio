//////////////////////////////////////////////////////////////////////////
//
// ModuleNetCore
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

using System;
using UMF.Core.Module;
using System.Collections.Generic;
using System.IO;
using UMF.Core;
using System.Linq;
using System.Reflection;

namespace UMF.Net.Module
{
	//------------------------------------------------------------------------	
	public abstract class ModuleNetCoreBase : ModuleCoreBase
	{
		public delegate void delegatePacketReceivedCallback( short packet_id, object packet );
		public abstract void OnPacketReceived( Session session, short packet_id, MemoryStream packet_stream );

		public ModuleNetCoreBase() : base() { }
	}

	//------------------------------------------------------------------------
	public abstract class TModuleNetBase<ST> : ModuleNetCoreBase, IPacketInterruptor where ST : Session
	{
		protected abstract short ProtocolVersion { get; }
		public abstract Type SendPacketIdType { get; }
		public abstract Type NSendPacketIdType { get; }
		public abstract Type RecvPacketIdType { get; }
		public abstract Type NRecvPacketIdType { get; }

		public PacketHandlerManagerBase PacketHandlerManager { get { return mModulePacketHandlerManager; } }

		protected ModulePacketHandlerManager<ST> mModulePacketHandlerManager = null;
		protected PacketFormatterConfig mModulePacketFormatterConfig = null;
		protected int mSendCount = 0;
		protected int mRecvIndex = 0;

		protected Dictionary<short, List<PacketSendInterruptHandlerBase>> mPacketSendInterruptors = new Dictionary<short, List<PacketSendInterruptHandlerBase>>();

		public TModuleNetBase() : base()
		{
			mModulePacketHandlerManager = new ModulePacketHandlerManager<ST>( RecvPacketIdType, NRecvPacketIdType );
		}

		//------------------------------------------------------------------------		
		protected eSendPacketResult SendPacketToSession<PacketType>( PacketType packet, Session session ) where PacketType : class
		{
			PacketAttribute attr = PACKET<PacketType>.Attr;
			if( attr.IsVersion( mModulePacketFormatterConfig.protocol_version ) == false )
				return eSendPacketResult.VersionLower;

			short packetid = attr.GetPacketId( SendPacketIdType, NSendPacketIdType );

			Type send_id_type = SendPacketIdType;
			if( packetid < 0 )
				send_id_type = NSendPacketIdType;

			if( System.Enum.IsDefined( send_id_type, packetid ) == false )
				throw new Exception( string.Format( "packet id:{0} is wrong, sendIndex : {1}", packetid, mSendCount ) );

			// packet send interrupt
			List<PacketSendInterruptHandlerBase> packet_send_interrupt_list = GetPacketSendInterruptors( packetid );
			if( packet_send_interrupt_list != null )
				packet_send_interrupt_list.ForEach( a => a.SendInterrupt( packet, session ) );

			mSendCount++;
			if( mSendCount >= int.MaxValue )
				mSendCount = 1;

			if( session.CoreLogType != eCoreLogType.None || attr.LogType == eCoreLogType.Important || attr.LogType == eCoreLogType.Always )
			{
				if( session.CoreLogType == eCoreLogType.Important || attr.LogType == eCoreLogType.Important )
				{
					Log.WriteImportant( "[{0}] Send to {1} : {2}, sendIndex : {3}", ModuleName, session.strRemoteEndPoint, PacketLogFormatter.Instance.Serialize<PacketType>( packet ), mSendCount );
				}
				else if( session.CoreLogType == eCoreLogType.NameOnly && attr.LogType != eCoreLogType.Always )
				{
					Log.Write( "[{0}] Send to {1} : {2}, sendIndex : {3}", ModuleName, session.strRemoteEndPoint, packet.ToString(), mSendCount );
				}
				else
				{
					Log.Write( "[{0}] Send to {1} : {2}, sendIndex : {3}", ModuleName, session.strRemoteEndPoint, PacketLogFormatter.Instance.Serialize<PacketType>( packet ), mSendCount );
				}
			}

			PacketModule module_packet = new PacketModule();
			module_packet.module_name = ModuleName;
			module_packet.packet_id = packetid;
			module_packet.packet_stream = PacketWriteFormatter.Instance.Serialize( packet, mModulePacketFormatterConfig );

			MemoryStream stream = PacketWriteFormatter.Instance.Serialize( module_packet, session.GetPacketFormatterConfig );

			if( session.SendStream( stream ) == false )
			{
				session.SendWaitStream( stream );
				return eSendPacketResult.NotConnected;
			}

			return eSendPacketResult.Success;
		}

		//------------------------------------------------------------------------
		protected virtual void RegistPacketHandlerAttribute( IPacketInterruptor interrupt_target, PacketFormatterConfig formatter )
		{
			// packet handler automation
			MethodInfo[] handler_methods = GetType().GetAllMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			if( handler_methods != null )
			{
				foreach( MethodInfo method in handler_methods )
				{
					PacketHandlerAttribute handler_attr = method.GetCustomAttribute<PacketHandlerAttribute>();
					if( handler_attr != null )
					{
						Type attr_type = handler_attr.GetType();
						Log.WriteImportant( $">> ModulePacketHandler regist : {method.Name} : {method.DeclaringType.Name} : {attr_type.Name}" );

						Type packet_type = handler_attr.PacketType;
						if( packet_type == null )
							throw new Exception( "packet type is wrong" );

						PacketAttribute packet_attr = PACKET_CACHE.Attr( packet_type );
						
						if( attr_type.Equals( typeof( PacketRecvInterruptHandlerAttribute ) ) )
						{
							short packetId = packet_attr.GetPacketId( interrupt_target.RecvPacketIdType, interrupt_target.NRecvPacketIdType );

							if( interrupt_target.PacketHandlerManager.IsExist( packetId ) )
							{
								Type handler_delegate_type = typeof( PacketInterruptHandlerBase.DelegatePacketInterruptObjectHandler );
								PacketInterruptHandlerBase.DelegatePacketInterruptObjectHandler interruptor_delegate = (PacketInterruptHandlerBase.DelegatePacketInterruptObjectHandler)Delegate.CreateDelegate( handler_delegate_type, this, method );
								interrupt_target.PacketHandlerManager.AddPacketRecvInterruptHandler( packetId, interruptor_delegate );
							}
							else
							{
								Log.WriteError( "RegistPacketInterruptHandlerAttribute Interrupt base handler not found : {0}", packet_type.Name );
							}
						}
						else if( attr_type.Equals( typeof( PacketSendInterruptHandlerAttribute ) ) )
						{
							short packet_id = packet_attr.GetPacketId( interrupt_target.SendPacketIdType, interrupt_target.NSendPacketIdType );

							Type handler_delegate_type = typeof( PacketSendInterruptHandlerBase.DelegatePacketSendInterruptObjectHandler );
							PacketSendInterruptHandlerBase.DelegatePacketSendInterruptObjectHandler interruptor_delegate = (PacketSendInterruptHandlerBase.DelegatePacketSendInterruptObjectHandler)Delegate.CreateDelegate( handler_delegate_type, this, method );

							interrupt_target.AddPacketSendInterruptor( packet_type, interruptor_delegate );
						}
						else
						{							
							short packetId = packet_attr.GetPacketId( RecvPacketIdType, NRecvPacketIdType );

							if( mModulePacketHandlerManager.IsExist( packetId ) == true )
								throw new Exception( "Already exist packetId : " + packet_type.FullName );

							mModulePacketHandlerManager.AddHandler( packetId, handler_attr, CreatePacketHandlerByAttribute( method, handler_attr ), formatter );
						}
					}
				}
			}
		}

		//------------------------------------------------------------------------		
		public virtual PacketObjectHandler<ST> CreatePacketHandlerByAttribute( MethodInfo method, PacketHandlerAttribute handler_attr )
		{
			Type handler_delegate_type = typeof( DelegatePacketObjectHandler<ST> );
			DelegatePacketObjectHandler<ST> handler_delegate = (DelegatePacketObjectHandler<ST>)Delegate.CreateDelegate( handler_delegate_type, this, method );
			PacketObjectHandler<ST> handler = new PacketObjectHandler<ST>( handler_attr.PacketType, handler_delegate );
			return handler;
		}

		//------------------------------------------------------------------------	
		public virtual void AddPacketHandler<PT>( DelegatePacketHandler<ST, PT> handler ) where PT : class
		{
			if( mModulePacketHandlerManager == null )
				throw new Exception( $"PacketHandlerManager not found!" );

			mModulePacketHandlerManager.AddHandler<PT>( handler, mModulePacketFormatterConfig );
		}

		//------------------------------------------------------------------------
		public virtual void AddPacketReceiver<PT>( delegatePacketReceiverHandler<PT> handler, bool is_compact )
		{
			if( mModulePacketHandlerManager != null )
				mModulePacketHandlerManager.AddReceiver<PT>( handler, is_compact );
		}
		public virtual void RemovePacketReceiver<PT>( delegatePacketReceiverHandler<PT> handler )
		{
			if( mModulePacketHandlerManager != null )
				mModulePacketHandlerManager.RemoveReceiver<PT>( handler );
		}

		//------------------------------------------------------------------------
		public sealed override void OnPacketReceived( Session session, short packet_id, MemoryStream packet_stream )
		{
			if( mModulePacketHandlerManager == null )
				return;

			mRecvIndex++;
			if( mRecvIndex >= int.MaxValue )
				mRecvIndex = 1;

			PacketContainer packet = mModulePacketHandlerManager.deserialize_packet( session, packet_stream, mRecvIndex, true );
			mModulePacketHandlerManager.handle_packet( (ST)session, packet );
		}

		//------------------------------------------------------------------------
		public virtual void AddPacketSendInterruptor<PT>( PacketSendInterruptHandlerBase.DelegatePacketSendInterruptHandler<PT> interruptor ) where PT : class
		{
			PacketAttribute attr = PACKET<PT>.Attr;
			short packet_id = attr.GetPacketId( SendPacketIdType, NSendPacketIdType );

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
			short packet_id = attr.GetPacketId( SendPacketIdType, NSendPacketIdType );

			List<PacketSendInterruptHandlerBase> list;
			if( mPacketSendInterruptors.TryGetValue( packet_id, out list ) == false )
			{
				list = new List<PacketSendInterruptHandlerBase>();
				mPacketSendInterruptors.Add( packet_id, list );
			}

			list.Add( new PacketSendInterruptObjectHandler( interruptor ) );
		}

		//------------------------------------------------------------------------
		protected List<PacketSendInterruptHandlerBase> GetPacketSendInterruptors( short packet_id )
		{
			List<PacketSendInterruptHandlerBase> interruptor_list;
			if( mPacketSendInterruptors.TryGetValue( packet_id, out interruptor_list ) )
				return interruptor_list;

			return null;
		}
	}

	//------------------------------------------------------------------------
	public abstract class ModuleNetConnector : TModuleNetBase<Connector>
	{
		protected Connector mConnector;

		//------------------------------------------------------------------------
		public ModuleNetConnector( Connector connector )
			: base()
		{
			mConnector = connector;
			mConnector.AddModule( this );

			mModulePacketFormatterConfig = new PacketFormatterConfig();
			mModulePacketFormatterConfig.UseConvertDatetime = mConnector.GetPacketFormatterConfig.UseConvertDatetime;
			mModulePacketFormatterConfig.IgnoreTimeZone = mConnector.GetPacketFormatterConfig.IgnoreTimeZone;
			mModulePacketFormatterConfig.protocol_version = ProtocolVersion;

			RegistPacketHandlerAttribute( mConnector, mModulePacketFormatterConfig );
		}

		//------------------------------------------------------------------------
		public eSendPacketResult SendPacket<SendT, RecvT>( SendT packet, delegatePacketReceiverHandler<RecvT> compact_handler ) where SendT : class
		{
			if( compact_handler != null )
				AddPacketReceiver<RecvT>( compact_handler, true );

			return SendPacket<SendT>( packet );
		}
		public eSendPacketResult SendPacket<PacketType>( PacketType packet ) where PacketType : class
		{
			if( mConnector == null )
				return eSendPacketResult.SessionInvalid;

			return SendPacketToSession( packet, mConnector );
		}
	}

	//------------------------------------------------------------------------	
	public abstract class ModuleNetPeer : TModuleNetBase<Peer>
	{
		protected bool mIsNetInit = false;
		protected Connector mConnector;
		protected PeerManagerBase mPeerManager = null;

		//------------------------------------------------------------------------
		public ModuleNetPeer( PeerManagerBase peer_manager )
			: base()
		{
			mPeerManager = peer_manager;
			mPeerManager.AddModule( this );

			mModulePacketFormatterConfig = new PacketFormatterConfig();
			mModulePacketFormatterConfig.UseConvertDatetime = mPeerManager.GetListenerConfig.UseConvertDatetime;
			mModulePacketFormatterConfig.IgnoreTimeZone = mPeerManager.GetListenerConfig.IgnoreTimeZone;
			mModulePacketFormatterConfig.protocol_version = ProtocolVersion;

			RegistPacketHandlerAttribute( mPeerManager, mModulePacketFormatterConfig );
		}

		//------------------------------------------------------------------------
		public eSendPacketResult SendPacket<PacketType>( PacketType packet, Peer peer ) where PacketType : class
		{
			if( mConnector == null )
				return eSendPacketResult.SessionInvalid;

			return SendPacketToSession( packet, peer );
		}

		//------------------------------------------------------------------------	
		public void MulticastPacket<PacketType>( List<int> ids, PacketType packet ) where PacketType : class
		{
			if( ids == null || ids.Count == 0 || mPeerManager.PeersDic.Count == 0 )
				return;

			List<Peer> sends = new List<Peer>();
			foreach( int user_id in ids )
			{
				Peer peer;
				if( mPeerManager.PeersDic.TryGetValue( user_id, out peer ) == true )
				{
					sends.Add( peer );
				}
			}
			MulticastPacket( sends, packet );
		}

		//------------------------------------------------------------------------	
		public void MulticastPacket<PacketType>( List<Peer> peers, PacketType packet ) where PacketType : class
		{
			if( peers == null || peers.Count == 0 || mPeerManager.PeersDic.Count == 0 )
				return;

			PacketAttribute attr = PACKET<PacketType>.Attr;
			if( attr.IsVersion( mModulePacketFormatterConfig.protocol_version ) == false )
				return;

			short packet_id = attr.GetPacketId( SendPacketIdType, NSendPacketIdType );

			Type send_id_type = SendPacketIdType;
			if( packet_id < 0 )
				send_id_type = NSendPacketIdType;

			if( System.Enum.IsDefined( send_id_type, packet_id ) == false )
				throw new Exception( string.Format( "[{0}] id({1}) is wrong", mPeerManager.ServiceTypeString, packet_id ) );

			// packet send interrupt
			List<PacketSendInterruptHandlerBase> packet_send_interrupt_list = GetPacketSendInterruptors( packet_id );
			if( packet_send_interrupt_list != null )
				packet_send_interrupt_list.ForEach( a => a.SendInterrupt( packet, null ) );

			bool bLog = ( mPeerManager.GetListenerConfig.CoreLogType != eCoreLogType.None || attr.LogType != eCoreLogType.None );
			string strLog = "[" + mPeerManager.ServiceTypeString + "] Multicast to(" + peers.Count.ToString() + ") ";

			bool bSendStream = false;
			bool bFirst = true;

			foreach( IGrouping<short, Peer> group in peers.GroupBy( p => p.protocol_version ) )
			{
				if( group.Count() > 0 )
				{
					MemoryStream stream = null;
					foreach( Peer peer in group )
					{
						if( stream == null )
						{
							PacketModule module_packet = new PacketModule();
							module_packet.module_name = ModuleName;
							module_packet.packet_id = packet_id;
							module_packet.packet_stream = PacketWriteFormatter.Instance.Serialize( packet, mModulePacketFormatterConfig );

							stream = PacketWriteFormatter.Instance.Serialize( module_packet, peer.GetPacketFormatterConfig );
						}

						if( peer.SendStream( stream ) == true )
						{
							bSendStream = true;
							if( bLog == true && mPeerManager.GetListenerConfig.LogMulticastId == true )
							{
								if( bFirst == false )
									strLog += ", ";
								strLog += peer.strRemoteEndPoint + " sendIndex : " + peer.SendHandleCount.ToString();
								bFirst = false;
							}
						}
					}
				}
			}

			if( bLog == true && bSendStream == true )
			{
				if( mPeerManager.GetListenerConfig.LogMulticastId == false )
					strLog += string.Format( "({0}) : ", peers.Count );
				else
					strLog += " : ";

				if( attr.LogType == eCoreLogType.NameOnly || mPeerManager.GetListenerConfig.CoreLogType == Core.eCoreLogType.NameOnly )
					Log.Write( strLog + packet.ToString() );
				else
					Log.Write( strLog + PacketLogFormatter.Instance.Serialize<PacketType>( packet ) );
			}
		}

		//------------------------------------------------------------------------
		public void BroadcastPacket<PacketType>( PacketType packet ) where PacketType : class
		{
			BroadcastPacket<PacketType>( packet, -1 );
		}

		//------------------------------------------------------------------------
		public void BroadcastPacket<PacketType>( PacketType packet, int excludeId ) where PacketType : class
		{
			PacketAttribute attr = PACKET<PacketType>.Attr;
			short packet_id = attr.GetPacketId( SendPacketIdType, NSendPacketIdType );

			Type send_id_type = SendPacketIdType;
			if( packet_id < 0 )
				send_id_type = NSendPacketIdType;

			if( System.Enum.IsDefined( send_id_type, packet_id ) == false )
				throw new Exception( string.Format( "[{0}] id({1}) is wrong", mPeerManager.ServiceTypeString, packet_id ) );

			// packet send interrupt
			List<PacketSendInterruptHandlerBase> packet_send_interrupt_list = GetPacketSendInterruptors( packet_id );
			if( packet_send_interrupt_list != null )
				packet_send_interrupt_list.ForEach( a => a.SendInterrupt( packet, null ) );

			if( mPeerManager.GetListenerConfig.CoreLogType != eCoreLogType.None || attr.LogType == eCoreLogType.Important || attr.LogType == eCoreLogType.Always )
			{
				string strLog = string.Format( "[{0}] Broadcast to ({1}) : ", mPeerManager.ServiceTypeString, mPeerManager.PeersDic.Count );
				if( mPeerManager.GetListenerConfig.CoreLogType == eCoreLogType.Important || attr.LogType == eCoreLogType.Important )
				{
					Log.WriteImportant( strLog + PacketLogFormatter.Instance.Serialize<PacketType>( packet ) );
				}
				else if( mPeerManager.GetListenerConfig.CoreLogType == eCoreLogType.NameOnly && attr.LogType != eCoreLogType.Always )
				{
					Log.Write( strLog + packet.ToString() );
				}
				else
				{
					Log.Write( strLog + PacketLogFormatter.Instance.Serialize<PacketType>( packet ) );
				}
			}

			foreach( IGrouping<short, Peer> group in mPeerManager.PeersDic.Values.GroupBy( p => p.protocol_version ) )
			{
				if( group.Count() > 0 )
				{
					MemoryStream stream = null;
					foreach( Peer peer in group )
					{
						if( peer.PeerIndex != excludeId )
						{
							if( stream == null )
							{
								PacketModule module_packet = new PacketModule();
								module_packet.module_name = ModuleName;
								module_packet.packet_id = packet_id;
								module_packet.packet_stream = PacketWriteFormatter.Instance.Serialize( packet, mModulePacketFormatterConfig );

								stream = PacketWriteFormatter.Instance.Serialize( module_packet, peer.GetPacketFormatterConfig );
							}

							peer.SendStream( stream );
						}
					}
				}
			}
		}
	}
}
