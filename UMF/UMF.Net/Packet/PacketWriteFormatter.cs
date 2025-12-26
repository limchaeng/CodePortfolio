//////////////////////////////////////////////////////////////////////////
//
// PacketWriteFormatter
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
using System.Collections;
using System;
using System.Reflection;
using System.Net;
using UMF.Core;
using System.Collections.Generic;

namespace UMF.Net
{
	//------------------------------------------------------------------------	
	public class PacketFormatterConfig
    {
        public bool UseConvertDatetime;
		public bool IgnoreTimeZone;
        public short protocol_version;
    }

	//------------------------------------------------------------------------	
	public class PacketWriteFormatter : ValueFormatterBase
    {
        public override SerializeAttribute GetAttribute(FieldInfo info) { return info.GetCustomAttribute<PacketValueAttribute>(); }
        override public void OnValueInfo(ValueInfo valueInfo) { }

        delegate void Serializer(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config);

        PacketWriteFormatter()
        {
            ValueSerializerBooleanDelegate = new Serializer(ValueSerializerBoolean);
            ValueSerializerByteDelegate = new Serializer(ValueSerializerByte);
            ValueSerializerCharDelegate = new Serializer(ValueSerializerChar);
            ValueSerializerDecimalDelegate = new Serializer(ValueSerializerDecimal);
            ValueSerializerDoubleDelegate = new Serializer(ValueSerializerDouble);
            ValueSerializerInt16Delegate = new Serializer(ValueSerializerInt16);
            ValueSerializerInt32Delegate = new Serializer(ValueSerializerInt32);
            ValueSerializerInt64Delegate = new Serializer(ValueSerializerInt64);
            ValueSerializerSByteDelegate = new Serializer(ValueSerializerSByte);
            ValueSerializerSingleDelegate = new Serializer(ValueSerializerSingle);
            ValueSerializerStringDelegate = new Serializer(ValueSerializerString);
            ValueSerializerUInt16Delegate = new Serializer(ValueSerializerUInt16);
            ValueSerializerUInt32Delegate = new Serializer(ValueSerializerUInt32);
            ValueSerializerUInt64Delegate = new Serializer(ValueSerializerUInt64);
            ValueSerializerDatetimeDelegate = new Serializer(ValueSerializerDatetime);
            ValueSerializerIpDelegate = new Serializer(ValueSerializerIp);
            ValueSerializerVersionDelegate = new Serializer(ValueSerializerVersion);
            ValueSerializerStreamDelegate = new Serializer(ValueSerializerStream);
            IListSerializerDelegate = new Serializer(IListSerializer);
            IDictionarySerializerDelegate = new Serializer(IDictionarySerializer);
            MemberSerializerDelegate = new Serializer(MemberSerializer);
        }

        Delegate ValueSerializerBooleanDelegate;
        Delegate ValueSerializerByteDelegate;
        Delegate ValueSerializerCharDelegate;
        Delegate ValueSerializerDecimalDelegate;
        Delegate ValueSerializerDoubleDelegate;
        Delegate ValueSerializerInt16Delegate;
        Delegate ValueSerializerInt32Delegate;
        Delegate ValueSerializerInt64Delegate;
        Delegate ValueSerializerSByteDelegate;
        Delegate ValueSerializerSingleDelegate;
        Delegate ValueSerializerStringDelegate;
        Delegate ValueSerializerUInt16Delegate;
        Delegate ValueSerializerUInt32Delegate;
        Delegate ValueSerializerUInt64Delegate;
        Delegate ValueSerializerDatetimeDelegate;
        Delegate ValueSerializerIpDelegate;
        Delegate ValueSerializerVersionDelegate;
        Delegate ValueSerializerStreamDelegate;
        Delegate IListSerializerDelegate;
        Delegate IDictionarySerializerDelegate;
        Delegate MemberSerializerDelegate;

        override public Delegate GetValueSerializerBoolean(bool bMemberValue) { return ValueSerializerBooleanDelegate; }
        override public Delegate GetValueSerializerByte(bool bMemberValue) { return ValueSerializerByteDelegate; }
        override public Delegate GetValueSerializerChar(bool bMemberValue) { return ValueSerializerCharDelegate; }
        override public Delegate GetValueSerializerDecimal(bool bMemberValue) { return ValueSerializerDecimalDelegate; }
        override public Delegate GetValueSerializerDouble(bool bMemberValue) { return ValueSerializerDoubleDelegate; }
        override public Delegate GetValueSerializerInt16(bool bMemberValue) { return ValueSerializerInt16Delegate; }
        override public Delegate GetValueSerializerInt32(bool bMemberValue) { return ValueSerializerInt32Delegate; }
        override public Delegate GetValueSerializerInt64(bool bMemberValue) { return ValueSerializerInt64Delegate; }
        override public Delegate GetValueSerializerSByte(bool bMemberValue) { return ValueSerializerSByteDelegate; }
        override public Delegate GetValueSerializerSingle(bool bMemberValue) { return ValueSerializerSingleDelegate; }
        override public Delegate GetValueSerializerString(bool bMemberValue) { return ValueSerializerStringDelegate; }
        override public Delegate GetValueSerializerUInt16(bool bMemberValue) { return ValueSerializerUInt16Delegate; }
        override public Delegate GetValueSerializerUInt32(bool bMemberValue) { return ValueSerializerUInt32Delegate; }
        override public Delegate GetValueSerializerUInt64(bool bMemberValue) { return ValueSerializerUInt64Delegate; }
        override public Delegate GetValueSerializerDatetime(bool bMemberValue) { return ValueSerializerDatetimeDelegate; }
        override public Delegate GetValueSerializerIp(bool bMemberValue) { return ValueSerializerIpDelegate; }
        override public Delegate GetValueSerializerVersion(bool bMemberValue) { return ValueSerializerVersionDelegate; }
        override public Delegate GetValueSerializerStream(bool bMemberValue) { return ValueSerializerStreamDelegate; }
        override public Delegate GetIListSerializer() { return IListSerializerDelegate; }
        override public Delegate GetIDictionarySerializer() { return IDictionarySerializerDelegate; }
        override public Delegate GetMemberSerializer() { return MemberSerializerDelegate; }

        void ValueSerializerBoolean(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((bool)obj); }
        void ValueSerializerByte(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((byte)obj); }
        void ValueSerializerChar(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((char)obj); }
        void ValueSerializerDecimal(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((Decimal)obj); }
        void ValueSerializerDouble(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((Double)obj); }
        void ValueSerializerInt16(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((Int16)obj); }
        void ValueSerializerInt32(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((Int32)obj); }
        void ValueSerializerInt64(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((Int64)obj); }
        void ValueSerializerSByte(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((SByte)obj); }
        void ValueSerializerSingle(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((Single)obj); }
        void ValueSerializerString(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write(obj == null ? "" : (string)obj); }
        void ValueSerializerUInt16(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((UInt16)obj); }
        void ValueSerializerUInt32(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((UInt32)obj); }
        void ValueSerializerUInt64(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write((UInt64)obj); }
        void ValueSerializerIp(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write(((IPAddress)obj).GetAddressBytes()); }
        void ValueSerializerVersion(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config) { stream.Write(((Version)obj).ToString()); }

		//------------------------------------------------------------------------	
		void ValueSerializerStream(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config)
        {
            MemoryStream binary = (MemoryStream)obj;
            stream.Write((Int32)binary.Length);
            stream.Write(binary.ToArray());
        }

		//------------------------------------------------------------------------	
		void ValueSerializerDatetime(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config)
        {
			if( config.UseConvertDatetime == true )
			{
				DateTime time = (DateTime)obj;
				if( time == DateTime.MinValue )
					stream.Write( (double)0 );
				else
					stream.Write( ( DateTime.Now - time ).TotalMilliseconds );
			}
			else
			{
				if( config.IgnoreTimeZone )
				{
					stream.Write( DateTime.SpecifyKind( (DateTime)obj, DateTimeKind.Unspecified ).ToBinary() );
				}
				else
				{
					stream.Write( ( (DateTime)obj ).ToBinary() );
				}
			}
        }

		//------------------------------------------------------------------------	
		void IDictionarySerializer(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config)
        {
            IDictionary dictionary = obj as IDictionary;
            if (dictionary == null)
                stream.Write((Int16)0);
            else
            {
                stream.Write((Int16)dictionary.Count);

                if (dictionary.Count > 0)
                {
                    ValueInfo memberValueInfoKey = valueInfo.builder.GetValueInfo(0);
                    ValueInfo memberValueInfoValue = valueInfo.builder.GetValueInfo(1);

                    foreach(DictionaryEntry entry in dictionary)
                    {
                        ((Serializer)memberValueInfoKey.serializer)(stream, entry.Key, memberValueInfoKey, config);
                        ((Serializer)memberValueInfoValue.serializer)(stream, entry.Value, memberValueInfoValue, config);
                    }
                }
            }
        }

		//------------------------------------------------------------------------	
		void IListSerializer(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config)
        {
            IList list = obj as IList;
            if (list == null)
                stream.Write((Int16)0);
            else
            {
                stream.Write((Int16)list.Count);

                if (list.Count > 0)
                {
                    ValueInfo memberValueInfo = valueInfo.builder.GetValueInfo(0);

                    for (int i = 0; i < list.Count; i++)
                    {
                        ((Serializer)memberValueInfo.serializer)(stream, list[i], memberValueInfo, config);
                    }
                }
            }
        }

		//------------------------------------------------------------------------	
		void MemberSerializer(BinaryWriter stream, object obj, ValueInfo valueInfo, PacketFormatterConfig config)
        {
            foreach (ValueInfo info in valueInfo.builder)
            {
                PacketValueAttribute attr = (PacketValueAttribute)info.value_attr;
                if (attr == null || attr.IsVersion(config.protocol_version) == true)
                {
					// ??? WTF
					if( obj == null )
						Log.WriteError( "MemberSerializer:obj is null fieldname:{0} infoname:{1}", info.fieldInfo.Name, info.name );

                    Object member = info.fieldInfo.GetValue(obj);
                    if (attr != null && attr.IsNullable == true)
                    {
                        if (member == null)
                        {
                            stream.Write(true);
                            continue;
                        }
                        else
                            stream.Write(false);
                    }
                    else if (System.Type.GetTypeCode(info.valueType) == TypeCode.Object && info.valueType.IsPrimitive == false && info.valueType.IsClass && member == null)
                    {
                        member = Activator.CreateInstance(info.valueType);
                    }
                    ((Serializer)info.serializer)(stream, member, info, config);
                }
            }
        }

		//------------------------------------------------------------------------	
		class VALUEINFO<PacketT>
		{
			//static object m_LockObject = new object();
			//static ValueInfo info;
			static public ValueInfo Info
			{
				get
				{
					return VALUEINFO_CACHE.Info( typeof( PacketT ) );
					/*
					lock (m_LockObject)
					{
						if (info == null || info.HashCode != typeof(PacketT).GetHashCode())
						{
							info = new ValueInfo(typeof(PacketT), Instance);
						}
					}
					return info;
					*/
				}
			}
		}
		class VALUEINFO_CACHE
		{
			static object mLockObject = new object();
			static Dictionary<Type, ValueInfo> mInfoCache = new Dictionary<Type, ValueInfo>();
			public static ValueInfo Info( Type type )
			{
				ValueInfo info;
				lock( mLockObject )
				{
					if( mInfoCache.TryGetValue( type, out info ) == false )
					{
						info = new ValueInfo( type, Instance );
						mInfoCache.Add( type, info );
					}
					else if( info.HashCode != type.GetHashCode() )
					{
						info = new ValueInfo( type, Instance );
						mInfoCache[type] = info;
					}
				}
				return info;
			}

			public static string ShowInfo()
			{
				return $"# VALUEINFO_CACHE count:{mInfoCache.Count}";
			}
		}


		//------------------------------------------------------------------------	
		public MemoryStream Serialize<PacketType>(PacketType packet, PacketFormatterConfig config) where PacketType : class
        {
            ValueInfo info = VALUEINFO<PacketType>.Info;

            PacketAttribute attr = PACKET<PacketType>.Attr;

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((UInt16)0);
            writer.Write(attr.GetPacketIdRaw());
            MemberSerializer(writer, packet, info, config);
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((UInt16)stream.Length);

//             string hex = "";
//             for (int i = 0; i < stream.Length; ++i)
//                 hex += string.Format("{0:X2} ", stream.GetBuffer()[i]);
//             Log.Write(string.Format("{0} : {1}", packet.GetType().ToString(), hex));
            return stream;
        }

		//------------------------------------------------------------------------	
		public MemoryStream SerializeStream<StreamType>( StreamType packet, PacketFormatterConfig config ) where StreamType : class
		{
			ValueInfo info = VALUEINFO<StreamType>.Info;

			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter( stream );
			MemberSerializer( writer, packet, info, config );
			return stream;
		}

		//------------------------------------------------------------------------	
		static public PacketWriteFormatter Instance = new PacketWriteFormatter();
    }
}
