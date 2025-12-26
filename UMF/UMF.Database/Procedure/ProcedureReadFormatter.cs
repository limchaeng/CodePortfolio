//////////////////////////////////////////////////////////////////////////
//
// ProcedureReadFormatter_MSSql
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
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Net;
using UMF.Core;
using System.Data.Common;

namespace UMF.Database
{
	//------------------------------------------------------------------------	
	class SqlDataIterator
	{
		public DbDataReader reader; 
		public DbCommand command;

		public object[] values = null;
		int index = 0;
		bool bFirstResult = true;
		bool bCheck;

		public SqlDataIterator( DbDataReader reader, DbCommand command, bool bCheck )
		{
			this.reader = reader;
			this.command = command;
			this.bCheck = bCheck;
		}

		//------------------------------------------------------------------------	
		public bool StartCollection()
		{
			if( values != null && index != values.Length - 1 )
				throw new Exception( string.Format( "Left values({0}) : {1}", values.Length - index, reader.GetName( index ) ) );

			if( bFirstResult == true )
			{
				bFirstResult = false;
				return reader.HasRows;
			}

			if( reader.NextResult() == false )
				throw new Exception( "No more results" );

			return reader.HasRows;
		}

		//------------------------------------------------------------------------	
		public bool ReadRecord()
		{
			if( reader.Read() == false )
				return false;

			values = new object[reader.FieldCount];
			if( reader.GetValues( values ) != reader.FieldCount )
				throw new Exception( "GetValues failed" );

			index = 0;

			return true;
		}

		//------------------------------------------------------------------------	
		public void EndRecord()
		{
			if( values != null && index != values.Length - 1 )
				throw new Exception( string.Format( "Left values({0}) : {1}", values.Length - index, reader.GetName( index ) ) );
		}
		
		//------------------------------------------------------------------------	
		public object ReadValue( string name, Type type )
		{
			if( bCheck == true )
			{
				if( string.Compare( reader.GetName( index ), name, true ) != 0 )
					throw new Exception( string.Format( "Name doesn't match {0}(db) != {1}", reader.GetName( index ), name ) );

				Type valueType = values[index].GetType();
				if( type.IsEnum == true )
				{
					if( valueType != typeof( DBNull ) && valueType != type.GetEnumUnderlyingType() )
						throw new Exception( string.Format( "[{0}] Type doesn't match {1} != {2}", name, valueType, type.GetEnumUnderlyingType() ) );
				}
				else if( valueType != typeof( DBNull ) && valueType != type )
					throw new Exception( string.Format( "[{0}] Type doesn't match {1} != {2}", name, valueType, type ) );
			}

			object value = values[index++];
			if( type.IsEnum == true && !( value is DBNull ) )
				value = Enum.ToObject( type, value );

			if( index == values.Length )
				values = null;

			return value;
		}

		//------------------------------------------------------------------------	
		public object NextValue( string name, Type type )
		{
			if( values == null )
			{
				if( bFirstResult == true )
				{
					bFirstResult = false;
					if( ReadRecord() == false )
						throw new Exception( "First Read failed:" + name );
				}
				else
				{
					if( reader.Read() == true )
						throw new Exception( "Left records:" + name );

					if( reader.NextResult() == false )
						throw new Exception( "No more results:" + name );

					if( reader.HasRows == false )
						throw new Exception( "No records:" + name );

					if( ReadRecord() == false )
						throw new Exception( "Read failed:" + name );
				}
			}
			return ReadValue( name, type );
		}
	}

	//------------------------------------------------------------------------	
	public class ProcedureReadFormatter : ValueFormatterBase
	{
		public override SerializeAttribute GetAttribute( FieldInfo info ) { return info.GetCustomAttribute<ProcedureValueAttribute>(); }
		delegate object Serializer( SqlDataIterator reader, ValueInfo valueInfo, string add_value );
		override public void OnValueInfo( ValueInfo valueInfo )
		{
			if( valueInfo.valueType.GetInterface( "System.Collections.IList" ) != null )
			{
				ProcedureParamListAttribute listAttr = valueInfo.fieldInfo.GetCustomAttribute<ProcedureParamListAttribute>();
				valueInfo.value_specific_attr = listAttr;
				if( listAttr != null )
				{
					ValueInfo memberValueInfo = valueInfo.builder.GetValueInfo( 0 );

					if( memberValueInfo.builder != null )
					{
						bool bFindKey = false;
						foreach( ValueInfo collectionValueInfo in memberValueInfo.builder )
						{
							if( collectionValueInfo.name == listAttr.KeyName )
							{
								bFindKey = true;
								memberValueInfo.data = collectionValueInfo;
								break;
							}
						}
						if( bFindKey == false )
							throw new System.Exception( string.Format( "Can't find {0} in {1}", listAttr.KeyName, memberValueInfo.valueType.ToString() ) );
					}
					else
					{
						if( memberValueInfo.valueType != listAttr.InvalidKey.GetType() )
							throw new System.Exception( string.Format( "invalid type {0} : {1} != {2}", listAttr.KeyName, memberValueInfo.valueType.ToString(), listAttr.InvalidKey.GetType().ToString() ) );
					}
				}
			}
		}

		class VALUEINFO<RecvT>
			where RecvT : PROCEDURE_READ_BASE
		{
			static object m_LockObject = new object();
			static ValueInfo info;
			static public ValueInfo Info
			{
				get
				{
					lock( m_LockObject )
					{
						if( info == null || info.HashCode != typeof( RecvT ).GetHashCode() )
						{
							info = new ValueInfo( typeof( RecvT ), Instance );

							List<ValueInfo> return_infos = new List<ValueInfo>();

							FieldInfo[] fields = typeof( RecvT ).GetFields();
							foreach( FieldInfo fieldInfo in fields )
							{
								ProcedureValueAttribute value_attr = fieldInfo.GetCustomAttribute<ProcedureValueAttribute>();
								if( value_attr != null )
								{
									switch( value_attr.Type )
									{
										case eProcedureValueType.OutputValue:
											return_infos.Add( new ValueInfo( fieldInfo, value_attr, new Serializer( Instance.OutputValueSerializer ), null ) );
											break;

										case eProcedureValueType.ReturnValue:
											return_infos.Add( new ValueInfo( fieldInfo, value_attr, new Serializer( Instance.ReturnValueSerializer ), null ) );
											break;
									}
								}
							}
							info.data = return_infos;
						}
					}
					return info;
				}
			}
		}

		//------------------------------------------------------------------------	
		ProcedureReadFormatter()
		{
			CollectionValueSerializerDelegate = new Serializer( CollectionValueSerializer );
			CollectionValueSerializerVersionDelegate = new Serializer( CollectionValueSerializerVersion );
			CollectionValueSerializerIpDelegate = new Serializer( CollectionValueSerializerIp );
			CollectionValueSerializerStreamDelegate = new Serializer( CollectionValueSerializerStream );
			ValueSerializerDelegate = new Serializer( ValueSerializer );
			ValueSerializerVersionDelegate = new Serializer( ValueSerializerVersion );
			ValueSerializerIpDelegate = new Serializer( ValueSerializerIp );
			ValueSerializerStreamDelegate = new Serializer( ValueSerializerStream );
			IListSerializerDelegate = new Serializer( IListSerializer );
			MemberSerializerDelegate = new Serializer( MemberSerializer );
		}

		Delegate CollectionValueSerializerDelegate;
		Delegate CollectionValueSerializerVersionDelegate;
		Delegate CollectionValueSerializerIpDelegate;
		Delegate CollectionValueSerializerStreamDelegate;
		Delegate ValueSerializerDelegate;
		Delegate ValueSerializerVersionDelegate;
		Delegate ValueSerializerIpDelegate;
		Delegate ValueSerializerStreamDelegate;
		Delegate IListSerializerDelegate;
		Delegate MemberSerializerDelegate;

		override public Delegate GetValueSerializerBoolean( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerByte( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerChar( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerDecimal( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerDouble( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerInt16( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerInt32( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerInt64( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerSByte( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerSingle( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerString( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerUInt16( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerUInt32( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerUInt64( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerDatetime( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDelegate : ValueSerializerDelegate; }
		override public Delegate GetValueSerializerVersion( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerVersionDelegate : ValueSerializerVersionDelegate; }
		override public Delegate GetValueSerializerIp( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerIpDelegate : ValueSerializerIpDelegate; }
		override public Delegate GetValueSerializerStream( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerStreamDelegate : ValueSerializerStreamDelegate; }
		override public Delegate GetIListSerializer() { return IListSerializerDelegate; }
		override public Delegate GetIDictionarySerializer() { throw new Exception( "Not supported IDictionary" ); }
		override public Delegate GetMemberSerializer() { return MemberSerializerDelegate; }

		//------------------------------------------------------------------------	
		object ValueSerializer( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			return reader.NextValue( valueInfo.name, valueInfo.valueType );
		}

		//------------------------------------------------------------------------	
		object ValueSerializerVersion( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			return Version.Parse( (string)reader.NextValue( valueInfo.name, valueInfo.valueType ) );
		}

		//------------------------------------------------------------------------	
		object ValueSerializerIp( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			return IPAddress.Parse( (string)reader.NextValue( valueInfo.name, valueInfo.valueType ) );
		}

		//------------------------------------------------------------------------	
		object ValueSerializerStream( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			byte[] binary = (byte[])reader.NextValue( valueInfo.name, typeof( byte[] ) );
			return new MemoryStream( binary, 0, binary.Length, false, true );
		}

		//------------------------------------------------------------------------	
		object CollectionValueSerializer( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			return reader.ReadValue( valueInfo.name + add_value, valueInfo.valueType );
		}

		//------------------------------------------------------------------------	
		object CollectionValueSerializerVersion( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			return Version.Parse( (string)reader.ReadValue( valueInfo.name + add_value, valueInfo.valueType ) );
		}

		//------------------------------------------------------------------------	
		object CollectionValueSerializerIp( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			return IPAddress.Parse( (string)reader.ReadValue( valueInfo.name + add_value, valueInfo.valueType ) );
		}

		//------------------------------------------------------------------------	
		object CollectionValueSerializerStream( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			byte[] binary = (byte[])reader.ReadValue( valueInfo.name + add_value, typeof( byte[] ) );
			return new MemoryStream( binary, 0, binary.Length, false, true );
		}

		//------------------------------------------------------------------------	
		object IListSerializer( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			IList list = Activator.CreateInstance( valueInfo.valueType ) as IList;

			ProcedureParamListAttribute attr = (ProcedureParamListAttribute)valueInfo.value_specific_attr;

			if( attr == null )
			{
				if( reader.StartCollection() == true )
				{
					ValueInfo memberValueInfo = valueInfo.builder.GetValueInfo( 0 );

					while( reader.ReadRecord() )
					{
						list.Add( ( (Serializer)memberValueInfo.serializer )( reader, memberValueInfo, add_value ) );
						reader.EndRecord();
					}
				}
			}
			else
			{
				ValueInfo memberValueInfo = valueInfo.builder.GetValueInfo( 0 );

				for( int i = 0; i < attr.Count; i++ )
				{
					object obj = ( (Serializer)memberValueInfo.serializer )( reader, memberValueInfo, attr != null ? "_" + ( ( i + attr.StartIndex ).ToString() + add_value ) : "" );

					object keyValue;
					if( memberValueInfo.data != null )
						keyValue = ( (ValueInfo)memberValueInfo.data ).fieldInfo.GetValue( obj );
					else
						keyValue = obj;

					if( keyValue.GetType() != attr.InvalidKey.GetType() )
						throw new Exception( string.Format( "{0}({1}) is not matched with keyType({2})", ( (ValueInfo)memberValueInfo.data ).name, keyValue.GetType().ToString(), attr.InvalidKey.GetType().ToString() ) );

					if( Object.Equals( keyValue, attr.InvalidKey ) == false )
						list.Add( obj );
				}
			}
			return list;
		}

		//------------------------------------------------------------------------	
		object MemberSerializer( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			object obj = Activator.CreateInstance( valueInfo.valueType );

			foreach( ValueInfo info in valueInfo.builder )
			{
				ProcedureValueAttribute attr = (ProcedureValueAttribute)info.value_attr;
				bool is_emptiable = info.fieldInfo != null && attr != null && attr.Type == eProcedureValueType.SerializeEmptiable;
				if( is_emptiable )
				{
					if( reader.StartCollection() == false )
						continue;

					reader.ReadRecord();
				}

				object value = ( (Serializer)info.serializer )( reader, info, add_value );
				if( value != null )
				{
					if( value.GetType() != typeof( DBNull ) )
						info.fieldInfo.SetValue( obj, value );
					else if( attr == null || attr.Type != eProcedureValueType.SerializeNullable )
						throw new Exception( string.Format( "{0} is null", info.name ) );
				}
				if( is_emptiable )
					reader.EndRecord();
			}

			return obj;
		}

		//------------------------------------------------------------------------	
		object ReturnValueSerializer( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			object value = reader.command.Parameters["@" + valueInfo.name].Value;
			if( value == null )
				throw new Exception( string.Format( "{0} is null", valueInfo.name ) );
			if( (int)value != 0 )
				value = (int)value - 100000;
			return value;
		}

		//------------------------------------------------------------------------	
		object OutputValueSerializer( SqlDataIterator reader, ValueInfo valueInfo, string add_value )
		{
			object value = reader.command.Parameters["@" + valueInfo.name].Value;
			if( value == null )
				throw new Exception( string.Format( "{0} is null", valueInfo.name ) );
			return value;
		}

		//------------------------------------------------------------------------	
		public T Serialize<T>( DbDataReader reader, DbCommand command ) where T : PROCEDURE_READ_BASE
		{
			ValueInfo info = VALUEINFO<T>.Info;

			int error_code = 0;

			Exception localEx = null;

			SqlDataIterator itr = new SqlDataIterator( reader, command, !info.IsChecked );
			info.IsChecked = true;
			T obj;
			try
			{
				obj = (T)MemberSerializer( itr, info, "" );
			}
			catch( System.Exception ex )
			{
				localEx = ex;
				obj = (T)Activator.CreateInstance( typeof( T ) );
				error_code = -2;
			}

			if( reader != null )
				reader.Close();

			if( localEx == null )
				itr.EndRecord();

			try
			{
				List<ValueInfo> infos = (List<ValueInfo>)info.data;
				foreach( ValueInfo return_info in infos )
				{
					return_info.fieldInfo.SetValue( obj, ( (Serializer)return_info.serializer )( itr, return_info, "" ) );
				}
			}
			catch( System.Exception )
			{
				error_code = -3;
			}

			if( obj.return_code == 0 && error_code != 0 )
			{
				obj.return_code = error_code;
				if( localEx != null )
					Log.WriteError( localEx.ToString() );
			}

			return obj;
		}

		//------------------------------------------------------------------------	
		static public ProcedureReadFormatter Instance = new ProcedureReadFormatter();
	}
}
