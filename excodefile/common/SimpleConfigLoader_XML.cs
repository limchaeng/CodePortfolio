//////////////////////////////////////////////////////////////////////////
//
// SimpleConfigLoader_XML
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// XML 형식의 간단한 config 파일 로더
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace UMF.Core.SimpleConfig
{
	//------------------------------------------------------------------------	
	public class SimpleConfigLoader_XML : SimpleConfigLoaderBase
	{
		public override T Load<T>()
		{
			Type load_type = typeof( T );

			SimpleConfigAttribute attr = load_type.GetCustomAttribute<SimpleConfigAttribute>();
			if( attr == null )
				throw new System.Exception( "Load() SimpleConfigAttribute not found!" );

			string file_path = attr.ResourceName;
			if( mGetPathHandler != null )
				file_path = mGetPathHandler( attr.ResourceName );

			return Load<T>( file_path );
		}

		//------------------------------------------------------------------------		
		public override T Load<T>( string filepath )
		{
			Type config_type = typeof( T );

			XmlDocument doc = new XmlDocument();
			doc.Load( filepath );

			return LoadXMLDoc<T>( doc, config_type );
		}

		//------------------------------------------------------------------------		
		public override T LoadFromText<T>( string txt )
		{
			Type config_type = typeof( T );

			XmlDocument doc = new XmlDocument();
			doc.LoadXml( txt );

			return LoadXMLDoc<T>( doc, config_type ) as T;
		}

		//------------------------------------------------------------------------
		public T LoadXMLDoc<T>( XmlDocument doc, Type config_type ) where T : class
		{
			string root_name = config_type.Name;
			SimpleConfigAttribute attr = config_type.GetCustomAttribute<SimpleConfigAttribute>();
			if( attr != null && attr.IgnoreFieldName == false && string.IsNullOrEmpty( attr.FieldName ) == false )
				root_name = attr.FieldName;

			XmlNode root_node = doc.SelectSingleNode( root_name );
			return LoadFieldNode( root_node, config_type, attr ) as T;
		}

		//------------------------------------------------------------------------		
		public object LoadFieldNode( XmlNode node, Type _type, SimpleConfigAttribute config_attr )
		{
			if( node == null )
				return null;

			object ret_obj = Activator.CreateInstance( _type );
			bool ignore_field_name = false;
			if( config_attr != null )
				ignore_field_name = config_attr.IgnoreFieldName;

			FieldInfo[] field_infos = _type.GetFields();
			foreach( FieldInfo field in field_infos )
			{
				mLastLoadLog = $"{_type.FullName}:{field.Name}";
				SimpleConfigFieldAttribute attr = field.GetCustomAttribute<SimpleConfigFieldAttribute>();

				string field_name = "";
				bool use_field_name = false;
				if( ignore_field_name == false && attr != null && string.IsNullOrEmpty( attr.FieldName ) == false )
				{
					use_field_name = true;
					field_name = attr.FieldName;
				}

				if( attr != null && attr.UseCustomParse && _type.IsSubclassOf( typeof( SimpleConfigCustomParserBase ) ) )
				{
					if( use_field_name == false )
						field_name = field.Name;

					if( ( (SimpleConfigCustomParserBase)ret_obj ).Parse( field, XMLUtil.ParseAttribute<string>( node, field_name, "" ) ) )
						continue;
				}

				TypeCode typecode = Type.GetTypeCode( field.FieldType );
				if( typecode == TypeCode.Object )
				{
					if( use_field_name == false )
						field_name = field.FieldType.Name;

					if( field.FieldType.GetInterface( "System.Collections.IList" ) != null )
					{
						if( field.FieldType.IsGenericType == false )
							throw new Exception( $"You can use only Generic Collection : {field.Name}" );

						Type[] generic_types = field.FieldType.GetGenericArguments();
						if( generic_types.Length > 1 )
							throw new Exception( $"Too many Generic Arguments : {field.Name}" );

						TypeCode generic_type_code = Type.GetTypeCode( generic_types[0] );
						if( generic_type_code == TypeCode.Object )
						{
							IList list = Activator.CreateInstance( field.FieldType ) as IList;

							if( use_field_name == false )
								field_name = generic_types[0].Name;

							foreach( XmlNode child in node.SelectNodes( field_name ) )
							{
								if( child.NodeType == XmlNodeType.Comment )
									continue;

								object list_obj = LoadFieldNode( child, generic_types[0], config_attr );
								list.Add( list_obj );
							}

							field.SetValue( ret_obj, list );
						}
						else
						{
							SimpleConfigSepListFieldAttribute list_attr = attr as SimpleConfigSepListFieldAttribute;
							if( list_attr != null )
							{
								if( use_field_name == false )
									field_name = field.Name;

								List<string> sep_list = XMLUtil.ParseAttributeToList<string>( node, field_name, list_attr.ListSeparator );
								if( sep_list != null )
								{
									IList list = Activator.CreateInstance( field.FieldType ) as IList;
									foreach( string str in sep_list )
									{
										list.Add( StringUtil.SafeParse( str, generic_types[0], Activator.CreateInstance( generic_types[0] ) ) );
									}

									field.SetValue( ret_obj, list );
								}
							}
						}
					}
					else if( field.FieldType.IsPrimitive == false )
					{
						XmlNode sub_node = node.SelectSingleNode( field_name );
						field.SetValue( ret_obj, LoadFieldNode( sub_node, field.FieldType, config_attr ) );
					}
					else
					{
						throw new Exception( $"Not supported type : {field.FieldType.FullName}" );
					}
				}
				else
				{
					if( use_field_name == false )
						field_name = field.Name;

					object def_value = field.GetValue( ret_obj );
					string str_val = XMLUtil.ParseAttribute<string>( node, field_name, "" );
					if( string.IsNullOrEmpty( str_val ) == false )
						field.SetValue( ret_obj, StringUtil.SafeParse( str_val, field.FieldType, def_value ) );
				}
			}

			return ret_obj;
		}
	}
}
