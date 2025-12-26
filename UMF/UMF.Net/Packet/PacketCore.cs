//////////////////////////////////////////////////////////////////////////
//
// PacketCore
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
using System.IO;
using UMF.Core;
using System.Collections.Generic;
using System.Reflection;

namespace UMF.Net
{
	//------------------------------------------------------------------------	
	public struct PacketCoreID
    {
		// verify 301 ~ 399

		// disconnected 501 ~ 599
		public const short DisconnectPacketId = -501;

		// internal 601 ~ 699
        public const short HeartbeatId = -601;
		public const short HeartbeatAckId = -602;

		// for Module
		public const short ModulePacket = -701;
	}

	//------------------------------------------------------------------------	
	public class PACKET<T>
    {
        //static PacketAttribute m_Attr = null;
        static public PacketAttribute Attr
        {
            get
            {
				return PACKET_CACHE.Attr( typeof( T ) );
				/*
                if (m_Attr == null)
                {
                    MemberInfo info = typeof(T);
                    if (info == null)
                        throw new Exception("packet type is wrong");

					m_Attr = info.GetCustomAttribute<PacketAttribute>();
                    if (m_Attr == null)
                        throw new Exception("packet type is wrong");
                }
                return m_Attr;
				*/
            }
        }
    }
	public class PACKET_CACHE
	{
		static Dictionary<Type, PacketAttribute> mAttributeCache = new Dictionary<Type, PacketAttribute>();
		public static PacketAttribute Attr(Type type)
		{
			if( type == null )
				throw new Exception( "packet type is wrong" );

			PacketAttribute attr;
			if( mAttributeCache.TryGetValue(type, out attr) == false )
			{
				attr = type.GetCustomAttribute<PacketAttribute>();
				if( attr == null )
					throw new Exception( "packet attribute is wrong" );

				mAttributeCache.Add( type, attr );
			}

			return attr;
		}

		public static string ShowInfo()
		{
			return $"# PACKET_CACHE count:{mAttributeCache.Count}";
		}
	}

	//------------------------------------------------------------------------	
	[Packet(PacketCoreID.DisconnectPacketId, eCoreLogType.Always)]
    public class PacketDisconnect
    {
        static public string GetErrorCodeString(int error_code)
        {
            if (Enum.IsDefined(typeof(eDisconnectErrorCode), error_code) == true)
                return Enum.GetName(typeof(eDisconnectErrorCode), error_code);
            else
                return error_code.ToString();
        }

        public string ErrorCodeString   { get { return GetErrorCodeString(error_code); } }
        public int error_code;
        public string error_string;
        public string error_detail_string;
    }

	//------------------------------------------------------------------------	
	[Packet(PacketCoreID.HeartbeatId)]
    public class PacketHeartbeat
    {
        public long heartbeat_key;
    }

	//------------------------------------------------------------------------	
	[Packet(PacketCoreID.HeartbeatAckId)]
    public class PacketHeartbeatAck
    {
        public long heartbeat_key;
    }

	//------------------------------------------------------------------------
	[Packet(PacketCoreID.ModulePacket)]
	public class PacketModule
	{
		public string module_name;
		public short packet_id;
		public MemoryStream packet_stream;
	}

	//------------------------------------------------------------------------	
	public class PacketContainer
	{
		public short packet_id;
		public object packet;
		public PacketAttribute attr;
		public ushort packet_size;

		public PacketContainer(short packet_id, object packet, PacketAttribute attr, ushort size)
		{
			this.packet_id = packet_id;
			this.packet = packet;
			this.attr = attr;
			this.packet_size = size;
		}

		public override string ToString()
		{
			return $"[{packet_id}:{packet.GetType().Name}:{packet_size}]";
		}
	}

	//------------------------------------------------------------------------	
	public class ExpandPacketData
	{
		public string type_name;
		public MemoryStream packet_stream;

		[PacketValue( Type = PacketValueType.None )]
		public object runtime_deserialize_packet = null;
	}
	public class ExpandPacketBase
	{
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public List<ExpandPacketData> expanded_packet_stream_list = null;

		//------------------------------------------------------------------------		
		public void AddExpandPacketData<ExPT>( ExPT added_packet, Session session) where ExPT : class
		{
			string full_name = typeof( ExPT ).FullName;
			if( expanded_packet_stream_list != null && expanded_packet_stream_list.Exists( a => a.type_name == full_name ) )
				throw new Exception( "AddExpandPacketData:Already exist expand type:" + full_name );

			MemoryStream stream = PacketWriteFormatter.Instance.SerializeStream<ExPT>( added_packet, session.GetPacketFormatterConfig );
			if( expanded_packet_stream_list == null )
				expanded_packet_stream_list = new List<ExpandPacketData>();

			ExpandPacketData data = new ExpandPacketData();
			data.type_name = full_name;
			data.packet_stream = stream;

			expanded_packet_stream_list.Add( data );
		}

		//------------------------------------------------------------------------		
		public ExPT GetExpandPacketData<ExPT>(Session session) where ExPT : class
		{
			if( expanded_packet_stream_list != null )
			{
				string type_string = typeof( ExPT ).FullName;
				ExpandPacketData data = expanded_packet_stream_list.Find( a => a.type_name == type_string );
				if( data != null )
				{
					if( data.runtime_deserialize_packet != null )
					{
						return data.runtime_deserialize_packet as ExPT;
					}
					else
					{
						ExPT deserialize_packet = PacketReadFormatter.Instance.Serialize<ExPT>( new BinaryReader( data.packet_stream ), session.GetPacketFormatterConfig );
						data.runtime_deserialize_packet = deserialize_packet;

						return deserialize_packet;
					}
				}
			}

			return null;
		}
	}

}