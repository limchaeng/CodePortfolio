//////////////////////////////////////////////////////////////////////////
//
// PacketAttribute
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
using UMF.Core;
using System.Reflection;

namespace UMF.Net
{
	//------------------------------------------------------------------------	
	[System.AttributeUsage(System.AttributeTargets.Enum)]
    public class PacketVersionAttribute : System.Attribute
    {
        public short Version { get; set; }

        public static short GetVersion(Type type)
        {
            if (type.IsEnum == false)
                throw new Exception("type should be enum");

			PacketVersionAttribute attr = type.GetCustomAttribute<PacketVersionAttribute>();
			if( attr != null )
				return attr.Version;

            return 0;
        }
    }

	//------------------------------------------------------------------------	
	[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class NonCopyValueAttribute : System.Attribute
    {
        string identifier;
        public NonCopyValueAttribute()
        {
            this.identifier = "-ALL-";
        }

        public NonCopyValueAttribute(Type type)
        {
            this.identifier = type.Name;
        }

        public NonCopyValueAttribute(string identifier)
        {
            this.identifier = identifier;
        }

        public static bool isCopyable<T>(FieldInfo info)
        {
			NonCopyValueAttribute attr = info.GetCustomAttribute<NonCopyValueAttribute>();
			if( attr != null && ( attr.identifier == "-ALL-" || attr.identifier == typeof( T ).ToString()) )
				return false;

            return true;
        }
    }

	//------------------------------------------------------------------------	
	[System.AttributeUsage(System.AttributeTargets.Class)]
    public class PacketAttribute : System.Attribute
    {
        readonly object _packetId;
        readonly eCoreLogType _LogType;
        public short Version { get; set; }
        public short MaxVersion { get; set; }

		//------------------------------------------------------------------------		
        public static PacketAttribute GetAttrRaw(System.Type packetType)
        {
            MemberInfo info = packetType;
            if (info == null)
                throw new System.Exception("packet type is wrong");

			PacketAttribute attr = info.GetCustomAttribute<PacketAttribute>();
			if( attr != null )
				return attr;

            throw new System.Exception("packet type is wrong");
        }

		//------------------------------------------------------------------------		
		public short GetPacketId(System.Type packet_id_type, System.Type N_packet_id_type)
        {
			short p_id = GetPacketIdRaw();
			if( p_id < 0 && N_packet_id_type != null )
			{
				if( _packetId.GetType().Equals( N_packet_id_type ) )
					return (short)_packetId;
				throw new System.Exception( "Packet Type is not matched : " + N_packet_id_type.ToString() + " <> " + _packetId.GetType().ToString() + " : " + _packetId.ToString() );
			}
			else
			{
				if( _packetId.GetType().Equals( packet_id_type ) )
					return (short)_packetId;
				throw new System.Exception( "Packet Type is not matched : " + packet_id_type.ToString() + " <> " + _packetId.GetType().ToString() + " : " + _packetId.ToString() );
			}
		}

		//------------------------------------------------------------------------		
		public object GetPacketDirect(System.Type packetIdType)
        {
            if (_packetId.GetType().Equals(packetIdType))
                return _packetId;
            throw new System.Exception("Packet Type is not matched : " + packetIdType.ToString() + "." + _packetId.ToString());
        }

		//------------------------------------------------------------------------		
		public short GetPacketIdRaw()
        {
            return (short)_packetId;
        }

		//------------------------------------------------------------------------		
		public object GetPacketIdObject()
        {
            return _packetId;
        }

		//------------------------------------------------------------------------		
		public System.Type GetPacketIdType()
		{
			return _packetId.GetType();
		}

		//------------------------------------------------------------------------		
		public eCoreLogType LogType
        {
            get
            {
                return _LogType;
            }
        }

		//------------------------------------------------------------------------		
		public PacketAttribute(object packetId)
        {
            this._packetId = packetId;
            this._LogType = eCoreLogType.Detail;
            MaxVersion = short.MaxValue;
        }

		//------------------------------------------------------------------------		
		public PacketAttribute(object packetId, eCoreLogType logType)
        {
            this._packetId = packetId;
            this._LogType = logType;
            MaxVersion = short.MaxValue;
        }

		//------------------------------------------------------------------------		
		public bool IsVersion(short protocol_version)
        {
            return protocol_version == -1 || Version <= protocol_version && protocol_version <= MaxVersion;
        }
    }

	//------------------------------------------------------------------------	
	public enum PacketValueType
    {
        None,
        Serialize,
        SerializeNullable,
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class PacketValueAttribute : SerializeAttribute
    {
        public PacketValueType Type { get; set; }
        public short Version { get; set; }
        public short MaxVersion { get; set; }

        public bool IsNullable { get { return Type == PacketValueType.SerializeNullable; } }


        public PacketValueAttribute()
        {
            Type = PacketValueType.Serialize;
            MaxVersion = short.MaxValue;
        }

        override public bool IsSerializable
        {
            get
            {
                switch (this.Type)
                {
                    case PacketValueType.Serialize:
                    case PacketValueType.SerializeNullable:
                        return true;
                }
                return false;
            }
        }

        public bool IsVersion(short protocol_version)
        {
            if (protocol_version == -1)
                return true;

            return this.Version <= protocol_version && protocol_version <= this.MaxVersion;
        }
    }

}