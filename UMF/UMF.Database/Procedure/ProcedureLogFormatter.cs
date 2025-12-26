//////////////////////////////////////////////////////////////////////////
//
// ProcedureLogFormatter
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
using System.Reflection;
using System.Collections;
using System;
using UMF.Core;

namespace UMF.Database
{
	public class ProcedureLogFormatter : ValueFormatterBase
	{
		public override SerializeAttribute GetAttribute( FieldInfo info ) { return info.GetCustomAttribute<ProcedureValueAttribute>(); }
		override public void OnValueInfo( ValueInfo valueInfo ) { }

		delegate void Serializer( StringWriter stream, object obj, ValueInfo valueInfo );

		class VALUEINFO<LogT>
		{
			static object m_LockObject = new object();
			static ValueInfo info;
			static public ValueInfo Info
			{
				get
				{
					lock( m_LockObject )
					{
						if( info == null || info.HashCode != typeof( LogT ).GetHashCode() )
						{
							info = new ValueInfo( typeof( LogT ), Instance );
						}
					}
					return info;
				}
			}
		}

		//------------------------------------------------------------------------	
		ProcedureLogFormatter()
		{
			ValueSerializerDelegate = new Serializer( ValueSerializer );
			IListSerializerDelegate = new Serializer( IListSerializer );
			IDictionarySerializerDelegate = new Serializer( IDictionarySerializer );
			MemberSerializerDelegate = new Serializer( MemberSerializer );
			StreamSerializerDelegate = new Serializer( StreamSerializer );
		}

		Delegate ValueSerializerDelegate;
		Delegate IListSerializerDelegate;
		Delegate IDictionarySerializerDelegate;
		Delegate MemberSerializerDelegate;
		Delegate StreamSerializerDelegate;

		override public Delegate GetValueSerializerBoolean( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerByte( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerChar( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerDecimal( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerDouble( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerInt16( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerInt32( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerInt64( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerSByte( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerSingle( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerString( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerUInt16( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerUInt32( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerUInt64( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerDatetime( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerVersion( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerIp( bool bMemberValue ) { return ValueSerializerDelegate; }
		override public Delegate GetValueSerializerStream( bool bMemberValue ) { return StreamSerializerDelegate; }
		override public Delegate GetIListSerializer() { return IListSerializerDelegate; }
		override public Delegate GetIDictionarySerializer() { return IDictionarySerializerDelegate; }
		override public Delegate GetMemberSerializer() { return MemberSerializerDelegate; }

		//------------------------------------------------------------------------	
		void ValueSerializer( StringWriter stream, object obj, ValueInfo valueInfo )
		{
			if( obj == null )
				stream.Write( "\"" + valueInfo.name + "\":\"null\"" );
			else
				stream.Write( "\"" + valueInfo.name + "\":\"" + obj.ToString() + "\"" );
		}

		//------------------------------------------------------------------------	
		void StreamSerializer( StringWriter stream, object obj, ValueInfo valueInfo )
		{
			stream.Write( "\"" + valueInfo.name + "\":" + string.Format( "\"stream_{0}\"", ( (MemoryStream)obj ).Length ) );
		}

		//------------------------------------------------------------------------	
		void IDictionarySerializer( StringWriter stream, object obj, ValueInfo valueInfo )
		{
			IDictionary dictionary = obj as IDictionary;
			if( dictionary == null )
				stream.Write( "\"" + valueInfo.name + "\":\"null\"" );
			else
			{
				stream.Write( "\"" + valueInfo.name + "(" + dictionary.Count.ToString() + ")\":" );
				if( dictionary.Count > 0 )
				{
					stream.Write( "{" );

					ValueInfo memberValueInfoKey = valueInfo.builder.GetValueInfo( 0 );
					ValueInfo memberValueInfoValue = valueInfo.builder.GetValueInfo( 1 );

					bool bFirst = true;
					foreach( DictionaryEntry entry in dictionary )
					{
						if( bFirst == false )
							stream.Write( "," );
						else
							bFirst = false;

						( (Serializer)memberValueInfoKey.serializer )( stream, entry.Key, memberValueInfoKey );
						stream.Write( "," );
						( (Serializer)memberValueInfoValue.serializer )( stream, entry.Value, memberValueInfoValue );
					}

					stream.Write( "}" );
				}
			}
		}

		//------------------------------------------------------------------------	
		void IListSerializer( StringWriter stream, object obj, ValueInfo valueInfo )
		{
			IList list = obj as IList;

			if( list == null )
				stream.Write( "\"" + valueInfo.name + "\":\"null\"" );
			else if( list.Count <= 0 )
				stream.Write( "\"" + valueInfo.name + "\":\"0\"" );
			else
			{
				stream.Write( "\"" + valueInfo.name + "(" + list.Count.ToString() + ")\":" );
				stream.Write( "[" );

				ValueInfo memberValueInfo = valueInfo.builder.GetValueInfo( 0 );

				for( int i = 0; i < list.Count; i++ )
				{
					if( i > 0 )
						stream.Write( "," );

					stream.Write( "{" );
					( (Serializer)memberValueInfo.serializer )( stream, list[i], memberValueInfo );
					stream.Write( "}" );
				}

				stream.Write( "]" );
			}
		}

		//------------------------------------------------------------------------	
		void MemberSerializer( StringWriter stream, object obj, ValueInfo valueInfo )
		{
			stream.Write( "\"" + valueInfo.name + "\":" );

			if( obj == null )
			{
				ProcedureValueAttribute attr = (ProcedureValueAttribute)valueInfo.value_attr;
				if( attr != null && attr.IsNullable )
					stream.Write( "\"null\"" );
				else
					stream.Write( "\"null(error)\"" );
			}
			else
			{
				stream.Write( "{" );
				bool bFirstMember = true;
				foreach( ValueInfo info in valueInfo.builder )
				{
					if( bFirstMember == true )
						bFirstMember = false;
					else
						stream.Write( "," );

					object member = info.fieldInfo.GetValue( obj );
					( (Serializer)info.serializer )( stream, member, info );
				}
				stream.Write( "}" );
			}
		}

		//------------------------------------------------------------------------	
		public string Serialize<T>( object procedure, bool bIncludeName = true )
		{
			ValueInfo info = VALUEINFO<T>.Info;

			StringWriter stream = new StringWriter();

			stream.Write( "{" );
			MemberSerializer( stream, procedure, info );
			stream.Write( "}" );

			string str = stream.ToString();
			stream.Close();

			return str.Replace( "\n", "\\n" ).Replace( "\\", "/" );
		}

		//------------------------------------------------------------------------	
		public string SerializeDirect( object procedure, bool bIncludeName = true )
		{
			string str = "";

			if( procedure == null )
				return str;

			try
			{
				ValueInfo info = new ValueInfo( procedure.GetType(), Instance );

				StringWriter stream = new StringWriter();

				stream.Write( "{" );
				MemberSerializer( stream, procedure, info );
				stream.Write( "}" );

				str = stream.ToString();
				stream.Close();
			}
			catch( System.Exception ex )
			{
				Log.WriteError( ex.ToString() );
			}

			return str.Replace( "\n", "\\n" ).Replace( "\\", "/" );
		}

		//------------------------------------------------------------------------	
		static public ProcedureLogFormatter Instance = new ProcedureLogFormatter();
	}
}
