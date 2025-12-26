//////////////////////////////////////////////////////////////////////////
//
// IPacketHandler
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
using System;
using UMF.Core;
using System.Collections.Generic;

namespace UMF.Net
{
    public delegate void DelegatePacketHandler<ST, PT>(ST session, PT packet);
	public delegate void DelegatePacketObjectHandler<ST>( ST session, object _packet );

	//------------------------------------------------------------------------	
	public abstract class PacketDeserializerBase
	{
		public abstract PacketContainer deserialize_packet(Session session, BinaryReader reader, long recvIndex, bool bClose, short packet_id, ushort p_size);
	}

	//------------------------------------------------------------------------	
	public class PacketDeserializer<PT> : PacketDeserializerBase where PT : class
    {
		public override PacketContainer deserialize_packet(Session session, BinaryReader reader, long recvIndex, bool bClose, short packet_id, ushort p_size)        
		{
			try
			{
				object packet = PacketReadFormatter.Instance.Serialize<PT>(reader, session.GetPacketFormatterConfig);
				if (reader.BaseStream.Position < reader.BaseStream.Length)
					throw new Exception(string.Format("[{0}] Stream Left : {1}, recvIndex : {2}", typeof(PT).ToString(), reader.BaseStream.Length - reader.BaseStream.Position, recvIndex));

				return new PacketContainer( packet_id, packet, PACKET<PT>.Attr, p_size);
			}
			catch (System.IO.EndOfStreamException ex)
			{
				throw new Exception(string.Format("[{0}] {1}, length : {2}, recvIndex : {3}", typeof(PT).ToString(), ex.Message, reader.BaseStream.Length, recvIndex));
			}
			finally
			{
				if (bClose == true)
					reader.Close();
			}
		}
    }
	public class PacketDeserializer : PacketDeserializerBase
	{
		protected Type mPacketType = null;
		public PacketDeserializer(Type packet_type)
		{
			mPacketType = packet_type;
		}

		public override PacketContainer deserialize_packet( Session session, BinaryReader reader, long recvIndex, bool bClose, short packet_id, ushort p_size )
		{
			try
			{
				object packet = PacketReadFormatter.Instance.Serialize( mPacketType, reader, session.GetPacketFormatterConfig );
				if( reader.BaseStream.Position < reader.BaseStream.Length )
					throw new Exception( string.Format( "[{0}] Stream Left : {1}, recvIndex : {2}", mPacketType.Name, reader.BaseStream.Length - reader.BaseStream.Position, recvIndex ) );

				return new PacketContainer( packet_id, packet, PACKET_CACHE.Attr( mPacketType ), p_size );
			}
			catch( EndOfStreamException ex )
			{
				throw new Exception( string.Format( "[{0}] {1}, length : {2}, recvIndex : {3}", mPacketType.Name, ex.Message, reader.BaseStream.Length, recvIndex ) );
			}
			finally
			{
				if( bClose == true )
					reader.Close();
			}
		}
	}

	//------------------------------------------------------------------------	
	public abstract class PacketSerializerBase
	{
		public abstract MemoryStream serialize_packet(PacketFormatterConfig config);
		public virtual string GetPacketName() { return ToString(); }
		public bool log_enabled = false;
		
	}

	//------------------------------------------------------------------------	
	public class PacketSerializer<PT> : PacketSerializerBase where PT : class
	{
		PT packet;

		public PacketSerializer(PT packet, bool log_enabled)
		{
			this.packet = packet;
			this.log_enabled = log_enabled;
		}

		public override MemoryStream serialize_packet(PacketFormatterConfig config)
		{
			try
			{
				return PacketWriteFormatter.Instance.Serialize( packet, config );
			}
			catch (System.Exception ex)
			{
				throw new PacketDataException((int)eDisconnectErrorCode.SystemError, "[" + packet.ToString() + "] " + ex.ToString(), packet);
			}
		}

		public override string GetPacketName()
		{
			return packet.ToString();
		}
	}

	//------------------------------------------------------------------------	
	public abstract class PacketHandlerBase
	{
		protected bool mHasPacketInterrupt = false;
		protected List<PacketInterruptHandlerBase> mPacketInterruptHandlers = new List<PacketInterruptHandlerBase>();
		public abstract void handle_packet( Session session, PacketContainer packet_container );

		//------------------------------------------------------------------------
		public virtual void AddPacketRecvInterruptHandler( PacketInterruptHandlerBase handler )
		{
			mPacketInterruptHandlers.Add( handler );
			mHasPacketInterrupt = true;
		}
	}

	//------------------------------------------------------------------------	
	public class PacketHandler<ST, PT> : PacketHandlerBase where ST : Session where PT : class
    {
        DelegatePacketHandler<ST, PT> handler;

        public PacketHandler( DelegatePacketHandler<ST, PT> handler )
        {
            this.handler = handler;
        }

		//------------------------------------------------------------------------		
        public override void handle_packet(Session session, PacketContainer packet_container )
        {
			if( !( typeof( PT ).Equals( packet_container.packet.GetType() ) ) )
				throw new Exception(string.Format("[{0}] packet doesn't matched with {1}", session.SessionName, typeof(PT).ToString()));

			PacketAttribute attr = packet_container.attr;
            eCoreLogType log_type = attr.LogType;
			if( log_type == eCoreLogType.None )
				log_type = session.CoreLogType;

			if( log_type != eCoreLogType.None )
			{
				if( log_type == eCoreLogType.Important )
				{
					Log.WriteImportant( "[{0}] Receive from {1} : {2}, recvIndex : {3}", session.SessionName, session.Socket.RemoteEndPoint.ToString(), PacketLogFormatter.Instance.Serialize<PT>( packet_container.packet ), session.RecvHandleCount );
				}
				else if( log_type == eCoreLogType.NameOnly )
				{
					Log.Write( "[{0}] Receive from {1} : {2}, recvIndex : {3}", session.SessionName, session.Socket.RemoteEndPoint.ToString(), packet_container.ToString(), session.RecvHandleCount );
				}
				else
				{
					Log.Write( "[{0}] Receive from {1} : {2}, recvIndex : {3}", session.SessionName, session.Socket.RemoteEndPoint.ToString(), PacketLogFormatter.Instance.Serialize<PT>( packet_container.packet ), session.RecvHandleCount );
				}
			}

			if( !( session is ST ) )
				throw new Exception("SessionType is wrong");

			if( mHasPacketInterrupt )
				mPacketInterruptHandlers.ForEach( a => a.handle_packet_interrupt( session, packet_container ) );

			handler( (ST)session, (PT)packet_container.packet );
        }
    }

	//------------------------------------------------------------------------	
	public class PacketObjectHandler<ST> : PacketHandlerBase where ST : Session
	{
		Type mPacketType = null;
		DelegatePacketObjectHandler<ST> mHandler;

		public PacketObjectHandler( Type packet_type, DelegatePacketObjectHandler<ST> handler )
		{
			mPacketType = packet_type;
			mHandler = handler;
		}

		//------------------------------------------------------------------------		
		public override void handle_packet( Session session, PacketContainer packet_container )
		{
 			if( mPacketType.Equals( packet_container.packet.GetType() ) == false )
 				throw new Exception( string.Format( "[{0}] packet doesn't matched with {1}", session.SessionName, mPacketType.Name ) );

			PacketAttribute attr = packet_container.attr;
			eCoreLogType log_type = attr.LogType;
			if( log_type == eCoreLogType.None )
				log_type = session.CoreLogType;

			if( log_type != eCoreLogType.None )
			{
				if( log_type == eCoreLogType.Important )
				{
					Log.WriteImportant( "[{0}] Receive from {1} : {2}, recvIndex : {3}", session.SessionName, session.Socket.RemoteEndPoint.ToString(), PacketLogFormatter.Instance.SerializeDirect( packet_container.packet ), session.RecvHandleCount );
				}
				else if( log_type == eCoreLogType.NameOnly )
				{
					Log.Write( "[{0}] Receive from {1} : {2}, recvIndex : {3}", session.SessionName, session.Socket.RemoteEndPoint.ToString(), packet_container.ToString(), session.RecvHandleCount );
				}
				else
				{
					Log.Write( "[{0}] Receive from {1} : {2}, recvIndex : {3}", session.SessionName, session.Socket.RemoteEndPoint.ToString(), PacketLogFormatter.Instance.SerializeDirect( packet_container.packet ), session.RecvHandleCount );
				}
			}

			if( !( session is ST ) )
				throw new Exception( "SessionType is wrong" );

			if( mHasPacketInterrupt )
				mPacketInterruptHandlers.ForEach( a => a.handle_packet_interrupt( session, packet_container ) );

			mHandler( (ST)session, packet_container.packet );
		}
	}
}