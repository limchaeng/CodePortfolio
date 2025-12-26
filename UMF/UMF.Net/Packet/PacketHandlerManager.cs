//////////////////////////////////////////////////////////////////////////
//
// IPacketHandlerManager
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

using System.IO;
using System.Collections.Generic;
using System;
using UMF.Core;
using System.Reflection;

namespace UMF.Net
{
	//------------------------------------------------------------------------	
	[AttributeUsage( AttributeTargets.Method )]
	public class PacketHandlerAttribute : Attribute
	{
		public Type PacketType { get; set; }
		
		public PacketHandlerAttribute()
		{
			PacketType = null;
		}
	}

	//------------------------------------------------------------------------	
	[AttributeUsage( AttributeTargets.Method )]
	public class PacketRecvInterruptHandlerAttribute : PacketHandlerAttribute
	{
		public PacketRecvInterruptHandlerAttribute() : base()
		{
		}
	}

	//------------------------------------------------------------------------
	[AttributeUsage( AttributeTargets.Method )]
	public class PacketSendInterruptHandlerAttribute : PacketHandlerAttribute
	{
		public PacketSendInterruptHandlerAttribute() : base()
		{
		}
	}

	//------------------------------------------------------------------------	
	public abstract class PacketHandlerManagerBase
    {
        public delegate void PreHandleDelegate( PacketContainer packet );
        public delegate void PostHandleDelegate( PacketContainer packet );

        PreHandleDelegate _OnPreHandle = null;
        PostHandleDelegate _OnPostHandle = null;
        public PreHandleDelegate OnPreHandle { set { _OnPreHandle = value; } }
        public PostHandleDelegate OnPostHandle { set { _OnPostHandle = value; } }

		protected PacketReceiverManager mPacketReceiverManager = new PacketReceiverManager();
		protected Dictionary<short, PacketHandlerBase> m_Handlers = new Dictionary<short, PacketHandlerBase>();
		protected Dictionary<short, PacketDeserializerBase> m_Deserializer = new Dictionary<short, PacketDeserializerBase>();

		protected bool mHandlerExceptionIgnore = false;

        public Type PacketIdType { get; private set; }
		public Type NPacketIdType { get; private set; }
        public abstract Type SessionType { get; }

		protected PacketHandlerManagerBase( Type packet_id_type, Type n_packet_id_type )
		{
			PacketIdType = packet_id_type;
			NPacketIdType = n_packet_id_type;
		}

		//------------------------------------------------------------------------	
		public virtual void handle_packet(Session session, PacketContainer packet_container)
        {
            PacketHandlerBase handler;
			if( m_Handlers.TryGetValue( packet_container.packet_id, out handler ) == false )
			{
				if( mHandlerExceptionIgnore == false )
					throw new Exception( "Packet handler doesn't exist:" + packet_container.ToString() );
				else
					return;
			}

			if (_OnPreHandle != null) _OnPreHandle( packet_container );

			handler.handle_packet( session, packet_container );

			if (_OnPostHandle != null) _OnPostHandle( packet_container );

			if( mPacketReceiverManager != null )
				mPacketReceiverManager.Received_Packet( packet_container.packet_id, packet_container.packet );
        }

		//------------------------------------------------------------------------	
		public virtual PacketContainer deserialize_packet(Session session, MemoryStream stream, long recvIndex, bool bClose)
        {
            BinaryReader reader = new BinaryReader(stream);

            ushort size = reader.ReadUInt16(); // packet size
            Int16 packetId = reader.ReadInt16();

            PacketDeserializerBase deserializer;
			if( m_Deserializer.TryGetValue( packetId, out deserializer ) == false )
			{
				if( mHandlerExceptionIgnore == false )
				{
					Type p_id_type = PacketIdType;
					if( packetId < 0 )
						p_id_type = NPacketIdType;

					if( Enum.IsDefined( p_id_type, packetId ) == false )
						throw new Exception( string.Format( "Invalid PacketId : {0}, recvIndex:{1}", packetId, recvIndex ) );
					else
						throw new Exception( string.Format( "Packet type doesn't exist({0}) : {1}, recvIndex:{2}", packetId, p_id_type.ToString() + "." + Enum.GetName( p_id_type, packetId ), recvIndex ) );
				}
				else
				{
					return null;
				}
            }

			return deserializer.deserialize_packet( session, reader, recvIndex, bClose, packetId, size );
        }

		//------------------------------------------------------------------------	
		public void CheckEmptyHandler(eCheckEmptyHandlerType type)
        {
            if (type == eCheckEmptyHandlerType.Manual)
                return;

            bool bEmpty = false;
			foreach( object packetId in Enum.GetValues( PacketIdType ) )
			{
				short packet_id = (short)packetId;
				if( packet_id < 0 )
					continue;

                if (m_Handlers.ContainsKey(packet_id) == false)
                {
                    if (type == eCheckEmptyHandlerType.Exception)
                        throw new Exception("PacketHandler Empty : " + PacketIdType.ToString() + "." + Enum.GetName(PacketIdType, packetId));
                    else
                    {
                        bEmpty = true;
                        Log.WriteError("PacketHandler Empty : " + PacketIdType.ToString() + "." + Enum.GetName(PacketIdType, packetId));
                    }
                }
            }

			// negative id check
			foreach( object packetId in Enum.GetValues( NPacketIdType ) )
			{
				short packet_id = (short)packetId;

				// begin id is ignore
				if( packet_id == short.MinValue + 1 ||  packet_id >= 0 )
					continue;

				if( m_Handlers.ContainsKey( packet_id ) == false )
				{
					if( type == eCheckEmptyHandlerType.Exception )
						throw new Exception( "PacketHandlerNegative Empty : " + NPacketIdType.ToString() + "." + Enum.GetName( NPacketIdType, packetId ) );
					else
					{
						bEmpty = true;
						Log.WriteError( "PacketHandlerNegative Empty : " + NPacketIdType.ToString() + "." + Enum.GetName( NPacketIdType, packetId ) );
					}
				}
			}

			if( bEmpty == true)
                Log.Write("");
        }

		//------------------------------------------------------------------------	
		public bool IsExist(short packetId)
        {
            return m_Handlers.ContainsKey(packetId);
        }

		//------------------------------------------------------------------------	
		public void AddNullHandler(object packet_id)
        {
            m_Handlers.Add((short)packet_id, null);
        }

		//------------------------------------------------------------------------
		public virtual void AddPacketRecvInterruptHandler<PT>( PacketInterruptHandlerBase.DelegatePacketInterruptHandler<PT> module_handler ) where PT : class
 		{
			PacketAttribute attr = PACKET<PT>.Attr;
			short packet_id = attr.GetPacketId( PacketIdType, NPacketIdType );

			PacketHandlerBase handler;
			if( m_Handlers.TryGetValue( packet_id, out handler ) )
			{
				Log.WriteImportant( $">> Packet Interrupt regist : {typeof(PT).Name}" );
				handler.AddPacketRecvInterruptHandler( new PacketInterruptHandler<PT>( module_handler ) );
			}
			else
			{
				Log.WriteError( "PacketHandler Interrupt base handler not found : {0}", typeof( PT ).Name );
			}
		}

		//------------------------------------------------------------------------		
		public virtual void AddPacketRecvInterruptHandler( short packet_id, PacketInterruptHandlerBase.DelegatePacketInterruptObjectHandler module_handler )
		{
			PacketHandlerBase handler;
			if( m_Handlers.TryGetValue( packet_id, out handler ) )
			{
				handler.AddPacketRecvInterruptHandler( new PacketInterruptObjectHandler( module_handler ) );
			}
			else
			{
				Log.WriteError( "PacketHandler Interrupt base handler not found : {0}", packet_id );
			}
		}

		//------------------------------------------------------------------------
		public virtual void AddReceiver<PT>( delegatePacketReceiverHandler<PT> handler, bool is_compact = false)
		{
			mPacketReceiverManager.Add_Receiver<PT>( handler, is_compact );
		}
		public virtual void RemoveReceiver<PT>( delegatePacketReceiverHandler<PT> handler )
		{
			mPacketReceiverManager.Remove_Receiver<PT>( handler );
		}
	}

	//------------------------------------------------------------------------	
	public class PacketHandlerManager<ST> : PacketHandlerManagerBase where ST : Session
    {
        public override Type SessionType { get { return typeof(ST); } }
		protected virtual bool DoPacketHandlerAttributeRegist { get; } = true;

		public PacketHandlerManager(Type packet_id_type, Type n_packet_id_type)
            : base(packet_id_type, n_packet_id_type)
        {
			// packet handler automation
			if( DoPacketHandlerAttributeRegist )
			{
				MethodInfo[] handler_methods = GetType().GetAllMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
				if( handler_methods != null )
				{
					foreach( MethodInfo method in handler_methods )
					{
						PacketHandlerAttribute handler_attr = method.GetCustomAttribute<PacketHandlerAttribute>();
						if( handler_attr != null )
						{
							Log.WriteImportant( $"PacketHandler regist : {method.Name} : {method.DeclaringType.Name}" );

							Type packet_type = handler_attr.PacketType;
							if( packet_type == null )
								throw new Exception( "packet type is wrong" );

							PacketAttribute packet_attr = PACKET_CACHE.Attr( packet_type );
							short packetId = packet_attr.GetPacketId( PacketIdType, NPacketIdType );

							if( m_Handlers.ContainsKey( packetId ) == true )
								throw new Exception( "Already exist packetId : " + packet_type.FullName );

							if( m_Deserializer.ContainsKey( packetId ) == false )
								m_Deserializer.Add( packetId, new PacketDeserializer( packet_type ) );

							m_Handlers.Add( packetId, CreatePacketAttributeHandler( method, handler_attr ) );
						}
					}
				}
			}
		}

		//------------------------------------------------------------------------		
		public virtual PacketObjectHandler<ST> CreatePacketAttributeHandler( MethodInfo method, PacketHandlerAttribute handler_attr )
		{
			Type handler_delegate_type = typeof( DelegatePacketObjectHandler<ST> );
			DelegatePacketObjectHandler<ST> handler_delegate = (DelegatePacketObjectHandler<ST>)Delegate.CreateDelegate( handler_delegate_type, this, method );
			PacketObjectHandler<ST> handler = new PacketObjectHandler<ST>( handler_attr.PacketType, handler_delegate );
			return handler;
		}

		//------------------------------------------------------------------------	
		public virtual void AddHandler<PT>(DelegatePacketHandler<ST, PT> handler) where PT : class
        {
			PacketAttribute attr = PACKET<PT>.Attr;
			short packetId = attr.GetPacketId( PacketIdType, NPacketIdType );

			if( m_Handlers.ContainsKey( packetId ) == true )
				throw new Exception( "Already exist packetId : " + typeof( PT ).FullName );

			if( m_Deserializer.ContainsKey( packetId ) == false )
				m_Deserializer.Add( packetId, new PacketDeserializer<PT>() );

			m_Handlers.Add( packetId, new PacketHandler<ST, PT>( handler ) );
		}

		//------------------------------------------------------------------------	
		public void AddDummyHandler<PT>() where PT : class
        {
            AddHandler<PT>(new DelegatePacketHandler<ST, PT>(DummyHandler<PT>));
        }

		//------------------------------------------------------------------------	
		void DummyHandler<PT>(ST session, PT packet) { }
	}
	
	//------------------------------------------------------------------------	
	public class DummyPacketHandlerManager<ST> : PacketHandlerManager<ST> where ST : Session
    {
        public override Type SessionType { get { return typeof(ST); } }

        public DummyPacketHandlerManager(Type packetIdType, Type NPacketIdType)
            : base(packetIdType, NPacketIdType)
        {
        }

        public override PacketContainer deserialize_packet(Session session, MemoryStream stream, long recvIndex, bool bClose)
        {
            BinaryReader reader = new BinaryReader(stream);

            ushort size = reader.ReadUInt16(); // packet size
            Int16 packetId = reader.ReadInt16();

            PacketDeserializerBase deserializer;
            if (m_Deserializer.TryGetValue(packetId, out deserializer) == false)
            {
				Type p_id_type = PacketIdType;
				if( packetId < 0 )
					p_id_type = NPacketIdType;

				if( Enum.IsDefined( p_id_type, packetId ) == false )
					throw new Exception(string.Format("Invalid PacketId : {0}, recvIndex:{1}", packetId, recvIndex));
                else
                    return null;
            }

			return deserializer.deserialize_packet( session, reader, recvIndex, bClose, packetId, size );
        }
    }
}