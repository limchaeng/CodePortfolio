//////////////////////////////////////////////////////////////////////////
//
// PeerManager
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
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using UMF.Core;
using UMF.Net.Module;
using System.Reflection;

namespace UMF.Net
{

	//------------------------------------------------------------------------	
    public abstract class PeerManagerBase : Listener, IEnumerable<Peer>, IPacketInterruptor
    {
        protected Dictionary<int, Peer> mPeersDic = new Dictionary<int, Peer>();
		public Dictionary<int, Peer> PeersDic { get { return mPeersDic; } }
		public abstract Type SessionType { get; }
		protected int mMultipleSendCount = 0;
		public int MultipleSendCount { get { return mMultipleSendCount; } }

		protected Dictionary<short, List<PacketSendInterruptHandlerBase>> mPacketSendInterruptors = new Dictionary<short, List<PacketSendInterruptHandlerBase>>();
		protected Dictionary<string, ModuleNetCoreBase> mModuleDic = new Dictionary<string, ModuleNetCoreBase>();
			 

		public PeerManagerBase( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type N_send_packet_id_type )
            : base( service_type, config_file, packetHandlerManager, send_packet_id_type, N_send_packet_id_type)
        {
			if( SessionType.Equals( packetHandlerManager.SessionType ) == false )
				throw new Exception(string.Format("Session Type is diffrent with PacketHandlerManager({0}) {1} != {2}", PacketHandlerManager.ToString(), SessionType.ToString(), packetHandlerManager.SessionType.ToString()));

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
		public abstract object VerifyPeer(Peer peer, PacketVerify verify);

		//------------------------------------------------------------------------	
		public IEnumerator<Peer> GetEnumerator()
        {
            return mPeersDic.Values.GetEnumerator();
        }

		//------------------------------------------------------------------------	
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		//------------------------------------------------------------------------	
		public override void Close()
        {
            base.Close();
            mPeersDic.Clear();
        }

		//------------------------------------------------------------------------	
		public void MulticastPacket<PacketType>(List<int> ids, PacketType packet) where PacketType : class
        {
            if ( ids == null || ids.Count == 0 || mPeersDic.Count == 0 )
                return;

            List<Peer> sends = new List<Peer>();
            foreach (int user_id in ids)
            {
                Peer peer;
                if (mPeersDic.TryGetValue(user_id, out peer) == true)
                {
                    sends.Add(peer);
                }
            }
			MulticastPacket( sends, packet );
        }

		//------------------------------------------------------------------------	
		public void MulticastPacket<PacketType>(List<Peer> peers, PacketType packet) where PacketType : class
        {
			mMultipleSendCount = 0;
            if ( peers == null || peers.Count == 0 || mPeersDic.Count == 0 )
                return;

            PacketAttribute attr = PACKET<PacketType>.Attr;
			short packet_id = attr.GetPacketId( SendPacketIdType, NSendPacketIdType );

			Type send_id_type = SendPacketIdType;
			if( packet_id < 0 )
				send_id_type = NSendPacketIdType;

			if( System.Enum.IsDefined( send_id_type, packet_id) == false)
                throw new Exception(string.Format("[{0}] id({1}) is wrong", ServiceTypeString, packet_id));

			// packet send interrupt
			List<PacketSendInterruptHandlerBase> packet_send_interrupt_list = GetPacketSendInterruptors( packet_id );
			if( packet_send_interrupt_list != null )
				packet_send_interrupt_list.ForEach( a => a.SendInterrupt( packet, null ) );

			bool bLog = ( mListenerConfig.CoreLogType != eCoreLogType.None || attr.LogType != eCoreLogType.None );
			string strLog = string.Format( "[{0}] Multicast to ({1})", ServiceTypeString, peers.Count );

            bool bSendStream = false;

            foreach (IGrouping<short, Peer> group in peers.GroupBy(p => p.protocol_version))
            {
                if (group.Count() > 0 && attr.IsVersion(group.Key) == true)
                {
                    MemoryStream stream = null;
                    foreach (Peer peer in group)
                    {
                        if (stream == null)
                            stream = PacketWriteFormatter.Instance.Serialize<PacketType>(packet, peer.GetPacketFormatterConfig);

                        if (peer.SendStream(stream) == true)
                        {
							mMultipleSendCount++;
                            bSendStream = true;
                            if (bLog == true && mListenerConfig.LogMulticastId == true)
                            {
								strLog += string.Format( "[{0} sendIndex : {1}]", peer.strRemoteEndPoint, peer.SendHandleCount );
                            }
                        }
                    }
                }
            }

            if (bLog == true && bSendStream == true)
            {
                if ( mListenerConfig.LogMulticastId == false)
					strLog += string.Format( "->({0}) : ", peers.Count );
				else
                    strLog += " : ";

                if ( attr.LogType == eCoreLogType.NameOnly || mListenerConfig.CoreLogType == Core.eCoreLogType.NameOnly)
					Log.Write( strLog + packet.ToString());
                else
					Log.Write( strLog + PacketLogFormatter.Instance.Serialize<PacketType>( packet ) );
            }
        }

		//------------------------------------------------------------------------
		public void BroadcastStream(MemoryStream stream)
        {
            foreach (Peer peer in mPeersDic.Values)
            {
                peer.SendStream(stream);
            }
        }

		//------------------------------------------------------------------------
		public void BroadcastPacket<PacketType>(PacketType packet) where PacketType : class
        {
            BroadcastPacket<PacketType>(packet, -1);
        }

		//------------------------------------------------------------------------
		public void BroadcastPacket<PacketType>(PacketType packet, int excludeId) where PacketType : class
        {
			mMultipleSendCount = 0;

            PacketAttribute attr = PACKET<PacketType>.Attr;
			short packet_id = attr.GetPacketId( SendPacketIdType, NSendPacketIdType );

			Type send_id_type = SendPacketIdType;
			if( packet_id < 0 )
				send_id_type = NSendPacketIdType;

			if( System.Enum.IsDefined( send_id_type, packet_id ) == false )
				throw new Exception(string.Format("[{0}] id({1}) is wrong", ServiceTypeString, packet_id));

			// packet send interrupt
			List<PacketSendInterruptHandlerBase> packet_send_interrupt_list = GetPacketSendInterruptors( packet_id );
			if( packet_send_interrupt_list != null )
				packet_send_interrupt_list.ForEach( a => a.SendInterrupt( packet, null ) );

			if( mListenerConfig.CoreLogType != eCoreLogType.None || attr.LogType == eCoreLogType.Important || attr.LogType == eCoreLogType.Always )
			{
				string strLog = string.Format("[{0}] Broadcast to ({1}) : ", ServiceTypeString, mPeersDic.Count);
				if( mListenerConfig.CoreLogType == eCoreLogType.Important || attr.LogType == eCoreLogType.Important )
				{
					Log.WriteImportant( strLog + PacketLogFormatter.Instance.Serialize<PacketType>( packet ) );
				}
				else if( mListenerConfig.CoreLogType == eCoreLogType.NameOnly && attr.LogType != eCoreLogType.Always )
				{
					Log.Write( strLog + packet.ToString() );
				}
				else
				{
					Log.Write( strLog + PacketLogFormatter.Instance.Serialize<PacketType>( packet ) );
				}
			}

            foreach (IGrouping<short, Peer> group in mPeersDic.Values.GroupBy(p => p.protocol_version))
            {
                if (group.Count() > 0 && attr.IsVersion(group.Key) == true)
                {
                    MemoryStream stream = null;
                    foreach (Peer peer in group)
                    {
                        if (peer.PeerIndex != excludeId)
                        {
                            if (stream == null)
                                stream = PacketWriteFormatter.Instance.Serialize<PacketType>(packet, peer.GetPacketFormatterConfig);

							if( peer.SendStream( stream ) == true )
								mMultipleSendCount++;
                        }
                    }
                }
            }
        }

		//------------------------------------------------------------------------		
        public void SendPacket<PacketType>(int userIndex, PacketType packet) where PacketType : class
        {
            Session session = GetPeer(userIndex);

            if (session == null)
            {
                Log.Write("Can't find peer in SendPacket : {0}", userIndex);
            }
            else
                session.SendPacket<PacketType>(packet);
        }

		//------------------------------------------------------------------------		
		public override void RemovePeer(Peer peer)
        {
            base.RemovePeer(peer);
            if (peer.PeerIndex != -1)
                mPeersDic.Remove(peer.PeerIndex);
        }

		//------------------------------------------------------------------------		
		public Peer GetPeer(int index)
        {
            Peer peer;
            if (mPeersDic.TryGetValue(index, out peer) == false)
                return null;

            return peer;
        }

		//------------------------------------------------------------------------		
		public virtual Peer FindServerPeer( int find_server_flag, int to_serverIndex )
		{
			return null;
		}

		//------------------------------------------------------------------------		
		public virtual bool DisconnectPeer(int id, int error_code, string error_string, string error_detail_string)
        {
            Peer peer = GetPeer(id);
            if (peer == null)
                return false;

            peer.Disconnect(error_code, error_string, error_detail_string);
            return true;
        }

		//------------------------------------------------------------------------		
		public bool DisconnectPeer(int id, int error_code, string error_string)
        {
            return DisconnectPeer(id, error_code, error_string, error_string);
        }

		//------------------------------------------------------------------------
		public virtual void AddPacketSendInterruptor<PT>( PacketSendInterruptHandlerBase.DelegatePacketSendInterruptHandler<PT> interruptor ) where PT : class
		{
			PacketAttribute attr = PACKET<PT>.Attr;
			short packet_id = attr.GetPacketId( mSendPacketIdType, mNSendPacketIdType );

			List<PacketSendInterruptHandlerBase> list;
			if( mPacketSendInterruptors.TryGetValue(packet_id, out list) == false )
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
		public virtual List<PacketSendInterruptHandlerBase> GetPacketSendInterruptors( short packet_id )
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
		public virtual void AddModule(ModuleNetCoreBase module)
		{
			if( mModuleDic.ContainsKey( module.ModuleName ) )
				throw new Exception( $"AddModule : Module already exist : {module.ModuleName}" );

			mModuleDic.Add( module.ModuleName, module );
		}

		//------------------------------------------------------------------------
		public virtual void OnModulePacketHandler(Peer peer, PacketModule packet_module)
		{
			ModuleNetCoreBase module;
			if( mModuleDic.TryGetValue( packet_module.module_name, out module ) == false )
				throw new Exception( $"OnModulePacketHandler : module not found : {packet_module.module_name}" );

			module.OnPacketReceived( peer, packet_module.packet_id, packet_module.packet_stream );
		}
	}

	//------------------------------------------------------------------------		
	public abstract class PeerManager : PeerManagerBase
    {
        protected int mNextPeerIndex = 1;
		protected int mPeerIndexMin = 1;
		protected int mPeerIndexMax = int.MaxValue;

		public PeerManager( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type N_send_packet_id_type )
            : base( service_type, config_file, packetHandlerManager, send_packet_id_type, N_send_packet_id_type)
        {
			mPeerIndexMin = mListenerConfig.PeerIndexMin;
			mPeerIndexMax = mListenerConfig.PeerIndexMax;
			mNextPeerIndex = mPeerIndexMin;
		}

		//------------------------------------------------------------------------		
		protected override Peer handle_accept(Socket socket)
        {
            Peer newPeer = base.handle_accept(socket);

            return newPeer;
        }

		//------------------------------------------------------------------------		
		public override object VerifyPeer(Peer peer, PacketVerify verify)
        {
            peer.PeerIndex = mNextPeerIndex;
            mPeersDic.Add(peer.PeerIndex, peer);

			mNextPeerIndex += 1;

			if( mNextPeerIndex >= mPeerIndexMax )
				mNextPeerIndex = mPeerIndexMin;

            return null;
        }
	}

	//------------------------------------------------------------------------	
	public abstract class TPeerManager<T> : PeerManager where T : Peer, new()
    {
        public override Type SessionType { get { return typeof(T); } }

        public TPeerManager( string service_type, string config_file, PacketHandlerManagerBase packetHandlerManager, Type send_packet_id_type, Type N_send_packet_id_type )
            : base(service_type, config_file, packetHandlerManager, send_packet_id_type, N_send_packet_id_type)
        {
        }

		//------------------------------------------------------------------------	
		protected override Peer CreateNewPeer(Socket socket)
        {
            Peer peer = new T();
            peer.Init(this, socket);
            return peer;
        }

		//------------------------------------------------------------------------	
		public new T GetPeer(int id)
        {
            Peer peer;
            if (mPeersDic.TryGetValue(id, out peer) == false)
                return null;

            return peer as T;
        }

		//------------------------------------------------------------------------	
		public T GetPeerByIp(IPAddress ip)
        {
            foreach(Peer peer in mPeersDic.Values)
            {
                if (peer.strRemoteEndPoint.StartsWith(ip.ToString()))
                    return peer as T;
            }

            return null;
        }
    }
}