//////////////////////////////////////////////////////////////////////////
//
// ValueLogFormatter
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
using System.Reflection;
using System.Collections.Generic;
using System.Collections;

namespace UMF.Core
{
	public class ValueLogFormatter : ValueFormatterBase
	{
		public override SerializeAttribute GetAttribute( FieldInfo info ) { return info.GetCustomAttribute<SerializeAttribute>(); }
		public override void OnValueInfo( ValueInfo valueInfo ) { }

		protected delegate void Serializer( StringWriter stream, object obj, ValueInfo valueInfo );

		protected class VALUEINFO<LogT>
		{
			public static ValueInfo Info
			{
				get
				{
					return VALUEINFO_CACHE.Info( typeof( LogT ) );
				}
			}
		}
		protected class VALUEINFO_CACHE
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
		protected ValueLogFormatter()
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

		public override Delegate GetValueSerializerBoolean( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerByte( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerChar( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerDecimal( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerDouble( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerInt16( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerInt32( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerInt64( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerSByte( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerSingle( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerString( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerUInt16( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerUInt32( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerUInt64( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerDatetime( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerVersion( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerIp( bool bMemberValue ) { return ValueSerializerDelegate; }
		public override Delegate GetValueSerializerStream( bool bMemberValue ) { return StreamSerializerDelegate; }
		public override Delegate GetIListSerializer() { return IListSerializerDelegate; }
		public override Delegate GetIDictionarySerializer() { return IDictionarySerializerDelegate; }
		public override Delegate GetMemberSerializer() { return MemberSerializerDelegate; }

		//------------------------------------------------------------------------	
		protected virtual void ValueSerializer( StringWriter stream, object obj, ValueInfo valueInfo )
		{
			if( obj == null )
				stream.Write( "\"" + valueInfo.name + "\":\"null\"" );
			else
				stream.Write( "\"" + valueInfo.name + "\":\"" + obj.ToString() + "\"" );
		}

		//------------------------------------------------------------------------	
		protected virtual void StreamSerializer( StringWriter stream, object obj, ValueInfo valueInfo )
		{
			stream.Write( "\"" + valueInfo.name + "\":" + string.Format( "\"stream_{0}\"", ( (MemoryStream)obj ).Length ) );
		}

		//------------------------------------------------------------------------	
		protected virtual void IDictionarySerializer( StringWriter stream, object obj, ValueInfo valueInfo )
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
		protected virtual void IListSerializer( StringWriter stream, object obj, ValueInfo valueInfo )
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
		protected virtual void MemberSerializer( StringWriter stream, object obj, ValueInfo valueInfo )
		{
			stream.Write( "\"" + valueInfo.name + "\":" );

			if( obj == null )
			{
				stream.Write( "\"null\"" );
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
		public virtual string Serialize<T>( object packet, bool bIncludeName = true )
		{
			ValueInfo info = VALUEINFO<T>.Info;

			StringWriter stream = new StringWriter();

			stream.Write( "{" );
			MemberSerializer( stream, packet, info );
			stream.Write( "}" );

			string str = stream.ToString();
			stream.Close();

			return str.Replace( "\n", "\\n" ).Replace( "\\", "/" );
		}

		//------------------------------------------------------------------------	
		public virtual string SerializeDirect( object packet, bool bIncludeName = true )
		{
			string str = "";

			if( packet == null )
				return str;

			try
			{
				ValueInfo info = new ValueInfo( packet.GetType(), Instance );

				StringWriter stream = new StringWriter();

				stream.Write( "{" );
				MemberSerializer( stream, packet, info );
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
		public static ValueLogFormatter Instance = new ValueLogFormatter();
	}
}
