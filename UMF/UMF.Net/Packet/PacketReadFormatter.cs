//////////////////////////////////////////////////////////////////////////
//
// PacketReadFormatter
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
using System.Collections;
using System.Reflection;
using System.Net;
using UMF.Core;
using System.Collections.Generic;

namespace UMF.Net
{
	//------------------------------------------------------------------------	
	public class PacketReadFormatter : ValueFormatterBase
    {
        public override SerializeAttribute GetAttribute(FieldInfo info) { return info.GetCustomAttribute<PacketValueAttribute>(); }
        override public void OnValueInfo(ValueInfo valueInfo) { }

        delegate object Serializer(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config);

        PacketReadFormatter()
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
            ValueSerializerVersionDelegate = new Serializer(ValueSerializerVersion);
            ValueSerializerIpDelegate = new Serializer(ValueSerializerIp);
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
        Delegate ValueSerializerVersionDelegate;
        Delegate ValueSerializerIpDelegate;
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
        override public Delegate GetValueSerializerVersion(bool bMemberValue) { return ValueSerializerVersionDelegate; }
        override public Delegate GetValueSerializerIp(bool bMemberValue) { return ValueSerializerIpDelegate; }
        override public Delegate GetValueSerializerStream(bool bMemberValue) { return ValueSerializerStreamDelegate; }
        override public Delegate GetIListSerializer() { return IListSerializerDelegate; }
        override public Delegate GetIDictionarySerializer() { return IDictionarySerializerDelegate; }
        override public Delegate GetMemberSerializer() { return MemberSerializerDelegate; }

        object ValueSerializerBoolean(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadBoolean(); }
        object ValueSerializerByte(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadByte(); }
        object ValueSerializerChar(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadChar(); }
        object ValueSerializerDecimal(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadDecimal(); }
        object ValueSerializerDouble(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadDouble(); }
        object ValueSerializerInt16(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadInt16(); }
        object ValueSerializerInt32(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadInt32(); }
        object ValueSerializerInt64(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadInt64(); }
        object ValueSerializerSByte(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadSByte(); }
        object ValueSerializerSingle(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadSingle(); }
        object ValueSerializerString(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadString(); }
        object ValueSerializerUInt16(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadUInt16(); }
        object ValueSerializerUInt32(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadUInt32(); }
        object ValueSerializerUInt64(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return stream.ReadUInt64(); }
        object ValueSerializerIp(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config) { return new IPAddress(stream.ReadBytes(4)); }

		//------------------------------------------------------------------------	
		object ValueSerializerVersion(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config)
        {
            Version ver = new Version(stream.ReadString());
            if (ver.Revision == -1)
                ver = new Version(ver.Major, ver.Minor, ver.Build==-1?0:ver.Build, 0);
            return ver;
        }

		//------------------------------------------------------------------------	
		object ValueSerializerStream(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config)
        {
            Int32 length = stream.ReadInt32();
            return new MemoryStream(stream.ReadBytes(length), 0, length, false, true);
        }

		//------------------------------------------------------------------------	
		object ValueSerializerDatetime(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config)
        {
            if (config.UseConvertDatetime == true)
            {
                double add_ms = stream.ReadDouble();
                if (add_ms == 0)
                    return DateTime.MinValue;
                return DateTime.Now.AddMilliseconds(-add_ms);
            }

            return DateTime.FromBinary(stream.ReadInt64());
        }

		//------------------------------------------------------------------------	
		object IDictionarySerializer(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config)
        {
            IDictionary dictionary = Activator.CreateInstance(valueInfo.valueType) as IDictionary;
            Int16 count = stream.ReadInt16();

            if (count > 0)
            {
                ValueInfo memberValueInfoKey = valueInfo.builder.GetValueInfo(0);
                ValueInfo memberValueInfoValue = valueInfo.builder.GetValueInfo(1);

                for (int i = 0; i < count; i++)
                {
                    dictionary.Add(((Serializer)memberValueInfoKey.serializer)(stream, memberValueInfoKey, config), ((Serializer)memberValueInfoValue.serializer)(stream, memberValueInfoValue, config));
                }
            }
            return dictionary;
        }

		//------------------------------------------------------------------------	
		object IListSerializer(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config)
        {
            IList list = Activator.CreateInstance(valueInfo.valueType) as IList;
            Int16 count = stream.ReadInt16();

            if (count > 0)
            {
                ValueInfo memberValueInfo = valueInfo.builder.GetValueInfo(0);

                for (int i = 0; i < count; i++)
                {
                    list.Add(((Serializer)memberValueInfo.serializer)(stream, memberValueInfo, config));
                }
            }
            return list;
        }

		//------------------------------------------------------------------------	
		object MemberSerializer(BinaryReader stream, ValueInfo valueInfo, PacketFormatterConfig config)
        {
            object obj = Activator.CreateInstance(valueInfo.valueType);

            foreach (ValueInfo info in valueInfo.builder)
            {
                PacketValueAttribute attr = (PacketValueAttribute)info.value_attr;
                if (attr == null || attr.IsVersion(config.protocol_version) == true)
                {
                    if (attr != null && attr.IsNullable == true)
                    {
                        if (stream.ReadBoolean() == true)
                        {
                            info.fieldInfo.SetValue(obj, null);
                            continue;
                        }
                    }
                    info.fieldInfo.SetValue(obj, ((Serializer)info.serializer)(stream, info, config));
                }
            }
            return obj;
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
			public static ValueInfo Info(Type type)
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
		public PacketType Serialize<PacketType>(BinaryReader stream, PacketFormatterConfig config) where PacketType : class
        {
            ValueInfo info = VALUEINFO<PacketType>.Info;
            return MemberSerializer(stream, info, config) as PacketType;
        }
		public object Serialize( Type packet_type, BinaryReader stream, PacketFormatterConfig config )
		{
			ValueInfo info = VALUEINFO_CACHE.Info( packet_type );
			return MemberSerializer( stream, info, config );
		}

		//------------------------------------------------------------------------	
		public static PacketReadFormatter Instance = new PacketReadFormatter();
    }
}
