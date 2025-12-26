//////////////////////////////////////////////////////////////////////////
//
// ValueFormatterBase
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
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using System.IO;

namespace UMF.Core
{
	//------------------------------------------------------------------------
	public abstract class ValueFormatterBase
	{
		public abstract SerializeAttribute GetAttribute( FieldInfo info );
		public abstract void OnValueInfo( ValueInfo valueInfo );

		public abstract Delegate GetValueSerializerBoolean( bool bMemberValue );
		public abstract Delegate GetValueSerializerByte( bool bMemberValue );
		public abstract Delegate GetValueSerializerChar( bool bMemberValue );
		public abstract Delegate GetValueSerializerDecimal( bool bMemberValue );
		public abstract Delegate GetValueSerializerDouble( bool bMemberValue );
		public abstract Delegate GetValueSerializerInt16( bool bMemberValue );
		public abstract Delegate GetValueSerializerInt32( bool bMemberValue );
		public abstract Delegate GetValueSerializerInt64( bool bMemberValue );
		public abstract Delegate GetValueSerializerSByte( bool bMemberValue );
		public abstract Delegate GetValueSerializerSingle( bool bMemberValue );
		public abstract Delegate GetValueSerializerString( bool bMemberValue );
		public abstract Delegate GetValueSerializerUInt16( bool bMemberValue );
		public abstract Delegate GetValueSerializerUInt32( bool bMemberValue );
		public abstract Delegate GetValueSerializerUInt64( bool bMemberValue );
		public abstract Delegate GetValueSerializerDatetime( bool bMemberValue );
		public abstract Delegate GetValueSerializerVersion( bool bMemberValue );
		public abstract Delegate GetValueSerializerIp( bool bMemberValue );
		public abstract Delegate GetValueSerializerStream( bool bMemberValue );
		virtual public Delegate GetProcedureTableValuedSerializer( bool bMemberValue ) { return null; }
		public abstract Delegate GetIListSerializer();
		public abstract Delegate GetIDictionarySerializer();
		public abstract Delegate GetMemberSerializer();
	}

	//------------------------------------------------------------------------
	public class ValueInfo
	{
		public int HashCode { get; private set; }
		public bool IsChecked { get; set; }

		public ValueInfo()
		{
		}

		public ValueInfo( FieldInfo fieldInfo, SerializeAttribute attr, Delegate serializer, object data )
		{
			this.fieldInfo = fieldInfo;
			this.name = fieldInfo.Name; ;
			valueType = fieldInfo.FieldType;
			this.serializer = serializer;
			this.data = data;
			this.value_attr = attr;
		}

		public ValueInfo( Type type, ValueFormatterBase formatter )
		{
			if( type == typeof( Object ) )
				throw new System.Exception( "type is System.Object" );
			HashCode = type.GetHashCode();
			Make( null, type.ToString(), type, true, false, formatter );
			formatter.OnValueInfo( this );
		}

		public ValueInfo( FieldInfo fieldInfo, SerializeAttribute attr, bool bMemberValue, ValueFormatterBase formatter )
		{
			Make( fieldInfo, fieldInfo.Name, fieldInfo.FieldType, false, bMemberValue, formatter );
			this.fieldInfo = fieldInfo;
			this.value_attr = attr;
			formatter.OnValueInfo( this );
		}

		public ValueInfo( string name, Type type, bool bRoot, bool bMemberValue, ValueFormatterBase formatter )
		{
			Make( null, name, type, bRoot, bMemberValue, formatter );
			formatter.OnValueInfo( this );
		}

		public void Make( FieldInfo fieldInfo, string name, Type type, bool bRoot, bool bMemberValue, ValueFormatterBase formatter )
		{
			this.name = name;
			valueType = type;

			System.TypeCode typecode = System.Type.GetTypeCode( valueType );
			switch( typecode )
			{
				case TypeCode.Object:
					if( valueType.IsArray == true )
						throw new System.Exception( "Array is not supported : " + name );
					else if( valueType.GetInterface( "UMF.Database.IProcedureTableValued" ) != null )
					{
						serializer = formatter.GetProcedureTableValuedSerializer( bMemberValue );
						if( serializer != null )
							return;

						serializer = formatter.GetMemberSerializer();
						builder = new ValueInfoBuilder( valueType, bRoot, bMemberValue, formatter );
						return;
					}
					else if( valueType.GetInterface( "System.Collections.IList" ) != null )
					{
						if( valueType.IsGenericType == false )
							throw new System.Exception( "You can use only Generic Collection : " + name );

						Type[] fieldTypes = valueType.GetGenericArguments();
						if( fieldTypes.Length > 1 )
							throw new System.Exception( "Too many Generic Arguments : " + name );

						serializer = formatter.GetIListSerializer();
						string value_name = name + ".Value";

						if( fieldInfo != null )
						{
							ListAttribute listAttr = fieldInfo.GetCustomAttribute<ListAttribute>();
							if( listAttr != null )
								value_name = listAttr.KeyName;
						}

						builder = new ValueInfoBuilder();
						builder.AddValueInfoDirect( new ValueInfo( value_name, fieldTypes[0], false, true, formatter ) );
						return;
					}
					else if( valueType.GetInterface( "System.Collections.IDictionary" ) != null )
					{
						if( valueType.IsGenericType == false )
							throw new System.Exception( "You can use only Generic Collection : " + name );

						Type[] fieldTypes = valueType.GetGenericArguments();
						if( fieldTypes.Length != 2 )
							throw new System.Exception( "Generic Argument count should be 2 : " + name );

						serializer = formatter.GetIDictionarySerializer();

						builder = new ValueInfoBuilder();
						builder.AddValueInfoDirect( new ValueInfo( name + ".Key", fieldTypes[0], false, true, formatter ) );
						builder.AddValueInfoDirect( new ValueInfo( name + ".Value", fieldTypes[1], false, true, formatter ) );
						return;
					}
					else if( valueType == typeof( Version ) )
					{
						serializer = formatter.GetValueSerializerVersion( bMemberValue );
						return;
					}
					else if( valueType == typeof( IPAddress ) )
					{
						serializer = formatter.GetValueSerializerIp( bMemberValue );
						return;
					}
					else if( valueType == typeof( MemoryStream ) )
					{
						serializer = formatter.GetValueSerializerStream( bMemberValue );
						return;
					}
					else if( valueType.IsPrimitive == false )
					{
						serializer = formatter.GetMemberSerializer();
						builder = new ValueInfoBuilder( valueType, bRoot, bMemberValue, formatter );
						return;
					}
					throw new System.Exception( "Not supported type : " + valueType.FullName );

				case TypeCode.Boolean: serializer = formatter.GetValueSerializerBoolean( bMemberValue ); break;
				case TypeCode.Byte: serializer = formatter.GetValueSerializerByte( bMemberValue ); break;
				case TypeCode.Char: serializer = formatter.GetValueSerializerChar( bMemberValue ); break;
				case TypeCode.Decimal: serializer = formatter.GetValueSerializerDecimal( bMemberValue ); break;
				case TypeCode.Double: serializer = formatter.GetValueSerializerDouble( bMemberValue ); break;
				case TypeCode.Int16: serializer = formatter.GetValueSerializerInt16( bMemberValue ); break;
				case TypeCode.Int32: serializer = formatter.GetValueSerializerInt32( bMemberValue ); break;
				case TypeCode.Int64: serializer = formatter.GetValueSerializerInt64( bMemberValue ); break;
				case TypeCode.SByte: serializer = formatter.GetValueSerializerSByte( bMemberValue ); break;
				case TypeCode.Single: serializer = formatter.GetValueSerializerSingle( bMemberValue ); break;
				case TypeCode.String: serializer = formatter.GetValueSerializerString( bMemberValue ); break;
				case TypeCode.UInt16: serializer = formatter.GetValueSerializerUInt16( bMemberValue ); break;
				case TypeCode.UInt32: serializer = formatter.GetValueSerializerUInt32( bMemberValue ); break;
				case TypeCode.UInt64: serializer = formatter.GetValueSerializerUInt64( bMemberValue ); break;
				case TypeCode.DateTime: serializer = formatter.GetValueSerializerDatetime( bMemberValue ); break;

				default:
					throw new System.Exception( "Not supported type : " + valueType.FullName );
			}
		}

		public string name;
		public Type valueType;
		public Delegate serializer;
		public ValueInfoBuilder builder;        // for object
		public FieldInfo fieldInfo;
		public SerializeAttribute value_attr;
		public System.Attribute value_specific_attr;
		public object data;

		public void CheckNames( List<string> names, string path )
		{
			if( builder != null )
			{
				foreach( ValueInfo info in builder )
				{
					info.CheckNames( names, path + "_" + name );
				}
			}
			else
			{
				string findName = path + "_" + name;
				if( names.Contains( findName.ToUpper() ) == true )
					throw new Exception( string.Format( "Same value name! : {0}", findName ) );
				names.Add( findName.ToUpper() );
			}
		}
	}

	//------------------------------------------------------------------------
	public class ValueInfoBuilder
	{
		public ValueInfoBuilder()
		{
		}

		public ValueInfoBuilder( Type type, bool bRoot, bool bMemberValue, ValueFormatterBase formatter )
		{
			Make( type, bRoot, bMemberValue, formatter );
		}

		static public List<FieldInfo> GetSequentialFields( Type type )
		{
			if( type.BaseType != typeof( Object ) )
			{
				List<FieldInfo> fields = GetSequentialFields( type.BaseType );
				fields.AddRange( type.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance ) );
				return fields;
			}
			else
				return new List<FieldInfo>( type.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance ) );
		}

		void Make( Type type, bool bRoot, bool bMemberValue, ValueFormatterBase formatter )
		{
			FieldInfo[] fields = GetSequentialFields( type ).ToArray();
			if( fields.Length == 0 )
				return;

			foreach( FieldInfo info in fields )
			{
				SerializeAttribute attr = formatter.GetAttribute( info );
				if( attr != null && attr.IsSerializable == false )
					continue;

				valueInfos.Add( new ValueInfo( info, attr, bMemberValue, formatter ) );
			}
		}

		public void AddValueInfoDirect( ValueInfo valueInfo )
		{
			valueInfos.Add( valueInfo );
		}

		List<ValueInfo> valueInfos = new List<ValueInfo>();
		public IEnumerator<ValueInfo> GetEnumerator()
		{
			return valueInfos.GetEnumerator();
		}

		public ValueInfo GetValueInfo( int index )
		{
			return valueInfos[index];
		}
	}
}