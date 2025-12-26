//////////////////////////////////////////////////////////////////////////
//
// ModulePacket
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
/////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using UMF.Core;

namespace UMF.Net.Module
{
	//------------------------------------------------------------------------	
	public class ModulePacketDeserializer<PT> : PacketDeserializerBase where PT : class
	{
		protected PacketFormatterConfig mPacketFormatterConfig = null;
		public ModulePacketDeserializer(PacketFormatterConfig formatter_config)
		{
			mPacketFormatterConfig = formatter_config;
		}

		public override PacketContainer deserialize_packet( Session session, BinaryReader reader, long recvIndex, bool bClose, short packet_id, ushort p_size )
		{
			try
			{
				object packet = PacketReadFormatter.Instance.Serialize<PT>( reader, mPacketFormatterConfig );
				if( reader.BaseStream.Position < reader.BaseStream.Length )
					throw new Exception( string.Format( "[{0}] Stream Left : {1}, recvIndex : {2}", typeof( PT ).ToString(), reader.BaseStream.Length - reader.BaseStream.Position, recvIndex ) );

				return new PacketContainer( packet_id, packet, PACKET<PT>.Attr, p_size );
			}
			catch( System.IO.EndOfStreamException ex )
			{
				throw new Exception( string.Format( "[{0}] {1}, length : {2}, recvIndex : {3}", typeof( PT ).ToString(), ex.Message, reader.BaseStream.Length, recvIndex ) );
			}
			finally
			{
				if( bClose == true )
					reader.Close();
			}
		}
	}
	public class ModulePacketDeserializer : PacketDeserializer 
	{
		protected PacketFormatterConfig mPacketFormatterConfig = null;
		public ModulePacketDeserializer( Type packet_type, PacketFormatterConfig formatter_config )
			: base( packet_type )
		{
			mPacketFormatterConfig = formatter_config;
		}

		public override PacketContainer deserialize_packet( Session session, BinaryReader reader, long recvIndex, bool bClose, short packet_id, ushort p_size )
		{
			try
			{
				object packet = PacketReadFormatter.Instance.Serialize( mPacketType, reader, mPacketFormatterConfig );
				if( reader.BaseStream.Position < reader.BaseStream.Length )
					throw new Exception( string.Format( "[{0}] Stream Left : {1}, recvIndex : {2}", mPacketType.Name, reader.BaseStream.Length - reader.BaseStream.Position, recvIndex ) );

				return new PacketContainer( packet_id, packet, PACKET_CACHE.Attr( mPacketType ), p_size );
			}
			catch( System.IO.EndOfStreamException ex )
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
	public class ModulePacketHandlerManager<ST> : PacketHandlerManager<ST> where ST : Session
	{
		public ModulePacketHandlerManager( Type packet_id_type, Type n_packet_id_type )
			: base( packet_id_type, n_packet_id_type )
		{
		}

		//------------------------------------------------------------------------		
		public void AddHandler<PT>( DelegatePacketHandler<ST, PT> handler, PacketFormatterConfig formatter_confg ) where PT : class
		{
			PacketAttribute attr = PACKET<PT>.Attr;
			short packetId = attr.GetPacketId( PacketIdType, NPacketIdType );

			if( m_Handlers.ContainsKey( packetId ) == true )
				throw new Exception( "Already exist packetId : " + typeof( PT ).FullName );

			if( m_Deserializer.ContainsKey( packetId ) == false )
				m_Deserializer.Add( packetId, new ModulePacketDeserializer<PT>( formatter_confg ) );

			m_Handlers.Add( packetId, new PacketHandler<ST, PT>( handler ) );
		}

		//------------------------------------------------------------------------
		public void AddHandler( short packetId, PacketHandlerAttribute handler_attr, PacketHandlerBase handler, PacketFormatterConfig formatter_confg )
		{
			if( m_Deserializer.ContainsKey( packetId ) == false )
				m_Deserializer.Add( packetId, new ModulePacketDeserializer( handler_attr.PacketType, formatter_confg ) );

			m_Handlers.Add( packetId, handler );
		}
	}
}
