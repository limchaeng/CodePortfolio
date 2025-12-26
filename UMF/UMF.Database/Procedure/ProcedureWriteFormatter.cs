//////////////////////////////////////////////////////////////////////////
//
// ProcedureWriteFormatter
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
using System.Data.Common;
using System.Collections.Generic;
using UMF.Core;

namespace UMF.Database
{
	public class ProcedureWriteFormatter : ValueFormatterBase
	{
		public override SerializeAttribute GetAttribute( FieldInfo info ) { return info.GetCustomAttribute<ProcedureValueAttribute>(); }
		delegate void Serializer( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value );

		//------------------------------------------------------------------------		
		override public void OnValueInfo( ValueInfo valueInfo )
		{
			if( valueInfo.valueType.GetInterface( "System.Collections.IList" ) != null )
			{
				ProcedureParamListAttribute listAttr = valueInfo.fieldInfo.GetCustomAttribute<ProcedureParamListAttribute>();
				if( listAttr == null )
					throw new System.Exception( string.Format( "List({0}) should have ProcedureParamListAttribute", valueInfo.name ) );

				valueInfo.value_specific_attr = listAttr;

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

		//------------------------------------------------------------------------		
		class VALUEINFO<RecvT, SendT>
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
						if( info == null || info.HashCode != typeof( SendT ).GetHashCode() )
						{
							info = new ValueInfo( typeof( SendT ), Instance );

							FieldInfo[] fields = typeof( RecvT ).GetFields();
							foreach( FieldInfo fieldInfo in fields )
							{
								ProcedureValueAttribute value_attr = fieldInfo.GetCustomAttribute<ProcedureValueAttribute>();
								if( value_attr != null )
								{
									switch( value_attr.Type )
									{
										case eProcedureValueType.OutputValue:
											info.builder.AddValueInfoDirect( new ValueInfo( fieldInfo, value_attr, new Serializer( Instance.OutputValueSerializer ), null ) );
											break;

										case eProcedureValueType.ReturnValue:
											info.builder.AddValueInfoDirect( new ValueInfo( fieldInfo, value_attr, new Serializer( Instance.ReturnValueSerializer ), null ) );
											break;
									}
								}
							}
							info.CheckNames( new List<string>(), "" );
						}
					}
					return info;
				}
			}
		}

		//------------------------------------------------------------------------		
		ProcedureWriteFormatter()
		{
			CollectionValueSerializerDelegate = new Serializer( CollectionValueSerializer );
			CollectionValueSerializerVersionDelegate = new Serializer( CollectionValueSerializerVersion );
			CollectionValueSerializerIpDelegate = new Serializer( CollectionValueSerializerIp );
			CollectionValueSerializerStreamDelegate = new Serializer( CollectionValueSerializerStream );
			CollectionValueSerializerDateTimeDelegate = new Serializer( CollectionValueSerializerDateTime );
			ValueSerializerDelegate = new Serializer( ValueSerializer );
			ValueSerializerVersionDelegate = new Serializer( ValueSerializerVersion );
			ValueSerializerIpDelegate = new Serializer( ValueSerializerIp );
			ValueSerializerStreamDelegate = new Serializer( ValueSerializerStream );
			ValueSerializerDateTimeDelegate = new Serializer( ValueSerializerDateTime );
			IListSerializerDelegate = new Serializer( IListSerializer );
			ProcedureTableValuedSerializerDelegate = new Serializer( ProcedureTableValuedSerializer );
			MemberSerializerDelegate = new Serializer( MemberSerializer );
		}

		Delegate CollectionValueSerializerDelegate;
		Delegate CollectionValueSerializerVersionDelegate;
		Delegate CollectionValueSerializerIpDelegate;
		Delegate CollectionValueSerializerStreamDelegate;
		Delegate CollectionValueSerializerDateTimeDelegate;
		Delegate ValueSerializerDelegate;
		Delegate ValueSerializerVersionDelegate;
		Delegate ValueSerializerIpDelegate;
		Delegate ValueSerializerStreamDelegate;
		Delegate ValueSerializerDateTimeDelegate;
		Delegate IListSerializerDelegate;
		Delegate ProcedureTableValuedSerializerDelegate;
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
		override public Delegate GetValueSerializerDatetime( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerDateTimeDelegate : ValueSerializerDateTimeDelegate; }
		override public Delegate GetValueSerializerVersion( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerVersionDelegate : ValueSerializerVersionDelegate; }
		override public Delegate GetValueSerializerIp( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerIpDelegate : ValueSerializerIpDelegate; }
		override public Delegate GetValueSerializerStream( bool bMemberValue ) { return bMemberValue == true ? CollectionValueSerializerStreamDelegate : ValueSerializerStreamDelegate; }
		override public Delegate GetProcedureTableValuedSerializer( bool bMemberValue ) { return ProcedureTableValuedSerializerDelegate; }
		override public Delegate GetIListSerializer() { return IListSerializerDelegate; }
		override public Delegate GetIDictionarySerializer() { throw new Exception( "Not supported IDictionary" ); }
		override public Delegate GetMemberSerializer() { return MemberSerializerDelegate; }

		//------------------------------------------------------------------------		
		void IListSerializer( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			IList list = valueInfo.fieldInfo.GetValue( obj ) as IList;

			ProcedureParamListAttribute attr = (ProcedureParamListAttribute)valueInfo.value_specific_attr;
			if( attr == null )
				throw new System.Exception( string.Format( "List({0}) should have ProcedureParamListAttribute", valueInfo.name ) );

			if( list.Count > attr.Count )
				throw new System.Exception( string.Format( "List({0}) Count({1}) should <= {2}", valueInfo.name, list.Count, attr.Count ) );

			ValueInfo memberValueInfo = valueInfo.builder.GetValueInfo( 0 );

			if( memberValueInfo.valueType.IsClass == false )
			{
				if( memberValueInfo.valueType != attr.InvalidKey.GetType() )
					throw new System.Exception( string.Format( "List Member Type({0}) should be class", memberValueInfo.name ) );
			}

			int i = 0;
			for( ; i < list.Count; i++ )
			{
				( (Serializer)memberValueInfo.serializer )( cmd, list[i], memberValueInfo, attr != null ? "_" + ( ( i + attr.StartIndex ).ToString() + add_value ) : "" );
			}
			if( i < attr.Count )
			{
				object member;
				if( memberValueInfo.valueType.IsClass == true )
				{
					member = Activator.CreateInstance( memberValueInfo.valueType );
					( (ValueInfo)memberValueInfo.data ).fieldInfo.SetValue( member, attr.InvalidKey );
				}
				else
					member = attr.InvalidKey;

				for( ; i < attr.Count; i++ )
				{
					( (Serializer)memberValueInfo.serializer )( cmd, member, memberValueInfo, attr != null ? "_" + ( ( i + attr.StartIndex ).ToString() + add_value ) : "" );
				}
			}
		}

		//------------------------------------------------------------------------		
		void ProcedureTableValuedSerializer( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo != null ? valueInfo.fieldInfo.GetValue( obj ) : obj;
			if( ( (IProcedureTableValued)member ).Count > 0 )
				cmd.AddTableValued( valueInfo.name + add_value, valueInfo.valueType.Name, member );
			else
				cmd.AddTableValued( valueInfo.name + add_value, valueInfo.valueType.Name, null );
		}

		//------------------------------------------------------------------------		
		void CollectionValueSerializer( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo != null ? valueInfo.fieldInfo.GetValue( obj ) : obj;
			cmd.AddWithValue( valueInfo.name + add_value, member );
		}

		//------------------------------------------------------------------------		
		void CollectionValueSerializerVersion( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo.GetValue( obj );
			cmd.AddWithValue( valueInfo.name + add_value, member.ToString() );
		}

		//------------------------------------------------------------------------		
		void CollectionValueSerializerIp( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo.GetValue( obj );
			cmd.AddWithValue( valueInfo.name + add_value, member.ToString() );
		}

		//------------------------------------------------------------------------		
		void CollectionValueSerializerStream( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo.GetValue( obj );
			cmd.AddWithValue( valueInfo.name + add_value, ( (MemoryStream)member ).ToArray() );
		}

		//------------------------------------------------------------------------		
		void CollectionValueSerializerDateTime( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo != null ? valueInfo.fieldInfo.GetValue( obj ) : obj;

			int none_min_max = 0;
			if( member == null || (DateTime)member == DateTime.MinValue )
				none_min_max = 1;
			else if( (DateTime)member == DateTime.MaxValue )
				none_min_max = 2;

			cmd.AddDateTime( valueInfo.name + add_value, none_min_max, member );
		}

		//------------------------------------------------------------------------		
		void ValueSerializer( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo.GetValue( obj );
			if( valueInfo.fieldInfo.FieldType.IsEnum == true )
				member = Convert.ChangeType( member, valueInfo.fieldInfo.FieldType.GetEnumUnderlyingType() );

			cmd.AddWithValue( valueInfo.name, member );
		}

		//------------------------------------------------------------------------		
		void ValueSerializerVersion( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo.GetValue( obj );
			cmd.AddWithValue( valueInfo.name, member.ToString() );
		}

		//------------------------------------------------------------------------		
		void ValueSerializerIp( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo.GetValue( obj );
			cmd.AddWithValue( valueInfo.name, member.ToString() );
		}

		//------------------------------------------------------------------------		
		void ValueSerializerStream( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo.GetValue( obj );
			cmd.AddWithValue( valueInfo.name, ( (MemoryStream)member ).ToArray() );
		}

		//------------------------------------------------------------------------		
		void ValueSerializerDateTime( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = valueInfo.fieldInfo.GetValue( obj );

			int none_min_max = 0;
			if( member == null || (DateTime)member == DateTime.MinValue )
				none_min_max = 1;
			else if( (DateTime)member == DateTime.MaxValue )
				none_min_max = 2;

			cmd.AddDateTime( valueInfo.name, none_min_max, member );
		}

		//------------------------------------------------------------------------		
		void MemberSerializer( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			Object member = obj;
			if( valueInfo.fieldInfo != null )
				member = valueInfo.fieldInfo.GetValue( obj );

			foreach( ValueInfo info in valueInfo.builder )
			{
				( (Serializer)info.serializer )( cmd, member, info, add_value );
			}
		}

		//------------------------------------------------------------------------		
		void ReturnValueSerializer( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			cmd.AddReturnValue( valueInfo.name, valueInfo.fieldInfo.FieldType );
		}

		//-------------------------------------------------------------s-----------		
		void OutputValueSerializer( ISqlCommand cmd, object obj, ValueInfo valueInfo, string add_value )
		{
			cmd.AddOutputValue( valueInfo.name, valueInfo.fieldInfo.FieldType );
		}

		//------------------------------------------------------------------------		
		public void Serialize<ReadType, ProcedureType>( ProcedureType procedure, DatabaseMain database, ISqlCommand cmd )
			where ReadType : PROCEDURE_READ_BASE
		{
			ValueInfo info = VALUEINFO<ReadType, ProcedureType>.Info;

			MemberSerializer( cmd, procedure, info, "" );
		}

		//------------------------------------------------------------------------		
		static public ProcedureWriteFormatter Instance = new ProcedureWriteFormatter();
	}
}
