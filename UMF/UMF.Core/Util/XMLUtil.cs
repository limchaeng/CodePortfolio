//////////////////////////////////////////////////////////////////////////
//
// XMLUtil
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
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Xml.Serialization;
using System;

namespace UMF.Core
{
	public class XmlBinary
	{
		public static string CurrFilePath = "";

		public const string ROW = "Row";
		public const string HEADER = "DataList";
		public const string ROWCOUNT = "RowCount";

		public class Node
		{
			public string Identity { get; private set; }
			Dictionary<string, string> mKeyValueAttributes = null;
			Dictionary<short, string> mFieldDic = null;
			public Dictionary<short, string> FieldDic { get { return mFieldDic; } }

			public Node( string identity, BinaryReader reader, Dictionary<short, string> row_field_dic, int row_idx = 0 )
			{
				Identity = identity;
				mKeyValueAttributes = new Dictionary<string, string>();
				mFieldDic = row_field_dic;

				string bin_identity = reader.ReadString();

				if( bin_identity == Identity )
				{
					int attr_count = reader.ReadInt32();
					for( int i = 0; i < attr_count; i++ )
					{
						string key = "";
						string value = "";

						if( Identity == ROW )
						{
							short key_idx = reader.ReadInt16();

							if( row_field_dic != null && row_field_dic.ContainsKey( key_idx ) )
								key = row_field_dic[key_idx];

							value = reader.ReadString();

							if( XMLUtil.DEBUG_BINARY_LOG )
							{
								if( XmlBinary.CurrFilePath.Contains( "bin.enc.bytes" ) )
								{
									Log.Write( "attr:{0} key_idx:{1} value:{3} row:{2}", attr_count, key_idx, row_idx, value );
								}
							}
						}
						else if( Identity == HEADER )
						{
							key = reader.ReadString();
							value = reader.ReadString();
						}

						if( string.IsNullOrEmpty( key ) )
						{
							Log.WriteWarning( "-XmlBinary Data Invalid!" );
						}
						else if( mKeyValueAttributes.ContainsKey( key ) )
						{
							Log.WriteWarning( "-XmlBinary Data Already Key:{0}", key );
						}
						else
						{
							mKeyValueAttributes.Add( key, value );
						}
					}

					// row field dic parsing
					if( Identity == HEADER )
					{
						mFieldDic = new Dictionary<short, string>();
						int field_dic_count = reader.ReadInt32();
						for( int i = 0; i < field_dic_count; i++ )
						{
							short _field_key = reader.ReadInt16();
							string _field_value = reader.ReadString();

							if( _field_key > 0 && string.IsNullOrEmpty( _field_value ) == false && mFieldDic.ContainsKey( _field_key ) == false )
								mFieldDic.Add( _field_key, _field_value );
						}
					}
				}
			}

			public string GetAttribute( string key )
			{
				string ret;
				if( mKeyValueAttributes.TryGetValue( key, out ret ) )
					return ret;

				return "";
			}

			public int AttributeCount { get { return mKeyValueAttributes.Count; } }

			public bool HasAttbibute( string key )
			{
				return mKeyValueAttributes.ContainsKey( key );
			}

			public string DebugInfo()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendLine( Identity );
				foreach( KeyValuePair<string, string> kvp in mKeyValueAttributes )
				{
					sb.AppendFormat( "[{0}:{1}]", kvp.Key, kvp.Value );
				}

				return sb.ToString();
			}
		}

		public Node Header { get; private set; }
		public List<Node> RowList { get; private set; }

		public XmlBinary( byte[] bytes, string filepath, bool header_only, bool is_encrypted, string encrypt_key )
		{
			XmlBinary.CurrFilePath = filepath;

			if( is_encrypted && string.IsNullOrEmpty( encrypt_key ) == false )
			{
				byte[] source_bytes = bytes;
				byte[] decrypt_bytes = null;
				if( bytes == null )
					source_bytes = File.ReadAllBytes( filepath );

				using( MemoryStream inputStream = new MemoryStream() )
				{
					DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
					desProvider.Key = ASCIIEncoding.ASCII.GetBytes( encrypt_key );
					desProvider.IV = ASCIIEncoding.ASCII.GetBytes( encrypt_key );
					using( CryptoStream cs = new CryptoStream( inputStream, desProvider.CreateDecryptor(), CryptoStreamMode.Write ) )
					{
						cs.Write( source_bytes, 0, source_bytes.Length );
						cs.FlushFinalBlock();
						decrypt_bytes = inputStream.ToArray();
					}
				}

				using( BinaryReader br = new BinaryReader( new MemoryStream( decrypt_bytes ) ) )
				{
					Load( br, header_only );
				}
			}
			else
			{
				byte[] source_bytes = bytes;
				if( bytes == null )
					source_bytes = File.ReadAllBytes( filepath );

				using( BinaryReader br = new BinaryReader( new MemoryStream( source_bytes ) ) )
				{
					Load( br, header_only );
				}
			}
		}

		//------------------------------------------------------------------------	
		void Load( BinaryReader reader, bool header_only )
		{
			Header = new Node( HEADER, reader, null );
			int row_count = reader.ReadInt32();

			if( header_only == false )
			{
				Dictionary<short, string> row_field_dic = Header.FieldDic;
				RowList = new List<Node>();
				for( int i = 0; i < row_count; i++ )
				{
					RowList.Add( new Node( ROW, reader, row_field_dic, i ) );
				}
			}
		}

		//------------------------------------------------------------------------	
		public string DebugInfo()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine( Header.DebugInfo() );
			foreach( Node node in RowList )
			{
				sb.AppendLine( node.DebugInfo() );
			}

			return sb.ToString();
		}

		//------------------------------------------------------------------------	
		public static void WriteXml( BinaryWriter writer, XmlDocument doc, Dictionary<short, string> row_field_dic )
		{
			XmlNode header_node = doc.SelectSingleNode( HEADER );

			writer.Write( HEADER ); // header
			writer.Write( header_node.Attributes.Count );   // attribute count
			for( int i = 0; i < header_node.Attributes.Count; i++ )
			{
				XmlAttribute attr = header_node.Attributes[i];
				writer.Write( attr.Name );  // key
				writer.Write( attr.Value ); // value
			}

			// row field header
			writer.Write( row_field_dic.Count );
			foreach( KeyValuePair<short, string> kvp in row_field_dic )
			{
				writer.Write( kvp.Key );
				writer.Write( kvp.Value );
			}

			XmlNodeList row_node_list = header_node.SelectNodes( "Row" );
			int row_count = row_node_list.Count;
			writer.Write( row_count );  // row count
			foreach( XmlNode child in row_node_list )
			{
				int attr_count = child.Attributes.Count;
				writer.Write( ROW );        // row header
				writer.Write( attr_count ); // attribute count

				for( int i = 0; i < attr_count; i++ )
				{
					XmlAttribute attr = child.Attributes[i];

					short key_idx = 0;
					string key_value = "";
					foreach( KeyValuePair<short, string> kvp in row_field_dic )
					{
						if( kvp.Value == attr.Name )
						{
							key_idx = kvp.Key;
							key_value = attr.Value;
							break;
						}
					}

					writer.Write( key_idx );    // key
					writer.Write( key_value );  // value
				}
			}
		}
	}

	//------------------------------------------------------------------------	
	public class XmlSelector
	{
		public XmlNode m_XmlNode = null;
		public XmlReader m_XmlReader = null;
		public XmlBinary m_XmlBinary = null;
		public XmlBinary.Node m_XmlBinaryCurrNode = null;

		public XmlNode SelectSingleNode( string node_name )
		{
			if( m_XmlNode != null )
				return m_XmlNode.SelectSingleNode( node_name );

			return null;
		}

		public int AttributeCount
		{
			get
			{
				if( m_XmlNode != null )
					return m_XmlNode.Attributes.Count;

				if( m_XmlReader != null )
					return m_XmlReader.AttributeCount;

				if( m_XmlBinaryCurrNode != null )
					return m_XmlBinaryCurrNode.AttributeCount;

				return 0;
			}
		}

		public bool HasAttribute( string attr_name )
		{
			if( m_XmlNode != null )
				return ( m_XmlNode.Attributes[attr_name] != null );

			if( m_XmlReader != null )
				return ( m_XmlReader.GetAttribute( attr_name ) != null );

			if( m_XmlBinaryCurrNode != null )
				return m_XmlBinaryCurrNode.HasAttbibute( attr_name );

			return false;
		}
	}

	//------------------------------------------------------------------------	
	public class XMLUtil
	{
		public static bool DEBUG_BINARY_LOG = false;

		public interface ISingleNodeClass
		{
			void _Load( XmlNode node );
		}

		//------------------------------------------------------------------------	
		public static ParseType ParseSingleNode<ParseType>( XmlNode node, string name ) where ParseType : class, ISingleNodeClass, new()
		{
			if( node == null )
				return null;

			XmlNode selected_node = node.SelectSingleNode( name );
			if( selected_node == null )
				return null;

			ParseType PT = new ParseType();
			PT._Load( selected_node );

			return PT;
		}

		//------------------------------------------------------------------------	
		public static string ParseInnerText( XmlNode node, string name )
		{
			XmlNode child_node = node.SelectSingleNode( name );
			if( child_node != null )
				return child_node.InnerText;

			return "";
		}

		//------------------------------------------------------------------------
		public static ParseType ParseAttribute<ParseType>( XmlSelector selector, string name, ParseType defaultValue )
		{
			if( selector == null )
				return defaultValue;

			if( selector.m_XmlNode != null )
				return ParseAttribute<ParseType>( selector.m_XmlNode, name, defaultValue );
			else if( selector.m_XmlReader != null )
				return ParseAttribute<ParseType>( selector.m_XmlReader, name, defaultValue );
			else if( selector.m_XmlBinaryCurrNode != null )
				return ParseAttribute<ParseType>( selector.m_XmlBinaryCurrNode, name, defaultValue );

			return defaultValue;
		}

		public static ParseType ParseAttribute<ParseType>( XmlBinary.Node binary, string name, ParseType defaultValue )
		{
			if( binary == null )
				return defaultValue;

			return StringUtil.SafeParse<ParseType>( binary.GetAttribute( name ), defaultValue );
		}
		public static ParseType ParseAttribute<ParseType>( XmlReader reader, string name, ParseType defaultValue )
		{
			if( reader == null )
				return defaultValue;

			return StringUtil.SafeParse<ParseType>( reader.GetAttribute( name ), defaultValue );
		}

		public static ParseType ParseAttribute<ParseType>( XmlNode node, string name, ParseType defaultValue )
		{
			if( node == null )
				return defaultValue;

			XmlAttribute attr = node.Attributes[name];
			if( attr == null )
				return defaultValue;

			return StringUtil.SafeParse<ParseType>( attr.Value, defaultValue );
		}

		//------------------------------------------------------------------------
		public static void AddAttribute( XmlNode node, string name, object value )
		{
			XmlAttribute attr = node.OwnerDocument.CreateAttribute( name );
			attr.Value = value.ToString();
			node.Attributes.Append( attr );
		}

		//------------------------------------------------------------------------
		public static XmlNode AddNode( XmlNode parent_node, string name )
		{
			return parent_node.AppendChild( parent_node.OwnerDocument.CreateElement( name ) );
		}

		//------------------------------------------------------------------------	
		public static bool ContainAttribute( XmlSelector selector, string name )
		{
			if( selector == null )
				return false;

			if( selector.m_XmlNode != null )
				return ContainAttribute( selector.m_XmlNode, name );
			else if( selector.m_XmlReader != null )
				return ContainAttribute( selector.m_XmlReader, name );
			else if( selector.m_XmlBinaryCurrNode != null )
				return ContainAttribute( selector.m_XmlBinaryCurrNode, name );

			return false;
		}
		public static bool ContainAttribute( XmlNode node, string name )
		{
			return ( node.Attributes[name] != null );
		}
		public static bool ContainAttribute( XmlBinary.Node node, string name )
		{
			return node.HasAttbibute( name );
		}
		public static bool ContainAttribute( XmlReader reader, string name )
		{
			return ( reader.GetAttribute( name ) != null );
		}

		//------------------------------------------------------------------------		
		public static List<T> ParseAttributeToList<T>( XmlNode node, string name, params char[] separator )
		{
			string parse_value = ParseAttribute<string>( node, name, "" );
			return StringUtil.SafeParseToList<T>( parse_value, separator );
		}

		public static List<T> ParseAttributeToList<T>( XmlBinary.Node binary, string name, params char[] separator )
		{
			if( binary == null )
				return null;

			string parse_value = ParseAttribute<string>( binary, name, "" );
			return StringUtil.SafeParseToList<T>( parse_value, separator );
		}
		public static List<T> ParseAttributeToList<T>( XmlReader reader, string name, params char[] separator )
		{
			if( reader == null )
				return null;

			string parse_value = ParseAttribute<string>( reader, name, "" );
			return StringUtil.SafeParseToList<T>( parse_value, separator );
		}

		//------------------------------------------------------------------------	
		public static List<ParseType> ParseAttributeToList<ParseType>( XmlSelector selector, string name, params char[] separator )
		{
			if( selector == null )
				return null;

			if( selector.m_XmlNode != null )
				return ParseAttributeToList<ParseType>( selector.m_XmlNode, name, separator );
			else if( selector.m_XmlReader != null )
				return ParseAttributeToList<ParseType>( selector.m_XmlReader, name, separator );
			else if( selector.m_XmlBinaryCurrNode != null )
				return ParseAttributeToList<ParseType>( selector.m_XmlBinaryCurrNode, name, separator );

			return null;
		}

		//------------------------------------------------------------------------
		public static List<ParseType> ParseAttributeMultiple<ParseType>( XmlSelector selector, string name, int begin_idx, ParseType default_value, bool ignore_empty )
		{
			if( selector == null )
				return null;

			List<ParseType> list = null;
			int attr_count = selector.AttributeCount;
			for( int i = begin_idx; i <= attr_count; i++ )
			{
				string attr_name = string.Format( "{0}_{1}", name, i );
				if( selector.HasAttribute( attr_name ) )
				{
					string str_value = ParseAttribute<string>( selector, attr_name, "" );
					if( string.IsNullOrEmpty( str_value ) && ignore_empty )
						continue;

					if( list == null )
						list = new List<ParseType>();

					list.Add( StringUtil.SafeParse<ParseType>( str_value, default_value ) );
				}
			}

			return list;
		}

		//------------------------------------------------------------------------		
		public static string EncodeXml<T>( T data )
		{
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter( sb );

			// Serializer the User object to the stream.
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add( "", "" );

			XmlSerializer encode = new XmlSerializer( typeof( T ) );
			encode.Serialize( sw, data, ns );
			sw.Close();

			return sb.ToString();
		}

		//------------------------------------------------------------------------
		public static T DecodeXml<T>( string data )
		{
			XmlSerializer decode = new XmlSerializer( typeof( T ) );
			MemoryStream read_stream = new MemoryStream( Encoding.UTF8.GetBytes( data ) );
			T _T = (T)decode.Deserialize( read_stream );
			read_stream.Close();
			return _T;
		}

		//------------------------------------------------------------------------
		public static bool SaveXmlDocToFile( string _filepath, XmlDocument doc )
		{
			string dirName = Path.GetDirectoryName( _filepath );
			if( Directory.Exists( dirName ) == false )
				Directory.CreateDirectory( dirName );

			using( FileStream fs = File.Open( _filepath, FileMode.Create, FileAccess.Write, FileShare.Read ) )
			{
				using( StreamWriter sw = new StreamWriter( fs, Encoding.UTF8 ) )
				{
					doc.Save( sw );
				}
			}
			return true;
		}

		//------------------------------------------------------------------------
		public static bool SaveXmlDocToFile( string _filepath, XmlDocument doc, XmlWriterSettings write_settings )
		{
			string dirName = Path.GetDirectoryName( _filepath );
			if( Directory.Exists( dirName ) == false )
				Directory.CreateDirectory( dirName );

			using( XmlWriter writer = XmlWriter.Create( _filepath, write_settings ) )
			{
				doc.Save( writer );
			}
			return true;
		}

		//------------------------------------------------------------------------
		public static bool SaveXmlDocToEncryptFile( string _filepath, XmlDocument doc, string key )
		{
			string dirName = Path.GetDirectoryName( _filepath );
			if( Directory.Exists( dirName ) == false )
				Directory.CreateDirectory( dirName );

			using( FileStream fs = File.Open( _filepath, FileMode.Create, FileAccess.Write, FileShare.Read ) )
			{
				DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
				desProvider.Key = ASCIIEncoding.ASCII.GetBytes( key );
				desProvider.IV = ASCIIEncoding.ASCII.GetBytes( key );

				using( CryptoStream cs = new CryptoStream( fs, desProvider.CreateEncryptor(), CryptoStreamMode.Write ) )
				{
					using( StreamWriter sw = new StreamWriter( cs, Encoding.UTF8 ) )
					{
						doc.Save( sw );
					}
				}
			}

			return true;
		}

		//------------------------------------------------------------------------
		public static bool SaveXmlToFile( string _filepath, string _xml )
		{
			string dirName = Path.GetDirectoryName( _filepath );
			if( Directory.Exists( dirName ) == false )
				Directory.CreateDirectory( dirName );

			using( FileStream fs = File.Open( _filepath, FileMode.Create, FileAccess.Write, FileShare.Read ) )
			{
				using( StreamWriter sw = new StreamWriter( fs, Encoding.UTF8 ) )
				{
					sw.Write( _xml );
				}
			}

			return true;
		}

		//------------------------------------------------------------------------
		public static bool SaveXmlToEncryptFile( string _filepath, string _xml, string key )
		{
			string dirName = Path.GetDirectoryName( _filepath );
			if( Directory.Exists( dirName ) == false )
				Directory.CreateDirectory( dirName );

			using( FileStream fs = File.Open( _filepath, FileMode.Create, FileAccess.Write, FileShare.Read ) )
			{
				DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
				desProvider.Key = ASCIIEncoding.ASCII.GetBytes( key );
				desProvider.IV = ASCIIEncoding.ASCII.GetBytes( key );

				using( CryptoStream cs = new CryptoStream( fs, desProvider.CreateEncryptor(), CryptoStreamMode.Write ) )
				{
					using( StreamWriter sw = new StreamWriter( cs, Encoding.UTF8 ) )
					{
						sw.Write( _xml );
					}
				}
			}

			return true;
		}


		//------------------------------------------------------------------------	
		public static string LoadXmlFromFile( string _filePath )
		{
			if( File.Exists( _filePath ) == false )
				throw new System.Exception( string.Format( "File({0}) does not exists", _filePath ) );

			string xml = "";
			using( StreamReader sr = new StreamReader( _filePath, Encoding.UTF8 ) )
			{
				xml = sr.ReadToEnd();
			}

			return xml;
		}

		//-----------------------------------------------------------------------------
		public static string LoadXmlFromEncryptFile( string _filePath, string key )
		{
			if( key.Length != 8 )
			{
				throw new System.Exception( "invalid key : key length must be 8!" );
			}

			if( File.Exists( _filePath ) == false )
			{
				throw new System.Exception( string.Format( "File({0}) does not exists", _filePath ) );
			}

			string xml = "";
			using( FileStream fs = File.Open( _filePath, FileMode.Open ) )
			{
				DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
				desProvider.Key = ASCIIEncoding.ASCII.GetBytes( key );
				desProvider.IV = ASCIIEncoding.ASCII.GetBytes( key );

				using( CryptoStream cs = new CryptoStream( fs, desProvider.CreateDecryptor(), CryptoStreamMode.Read ) )
				{
					using( StreamReader sr = new StreamReader( cs, Encoding.UTF8 ) )
					{
						xml = sr.ReadToEnd();
					}
				}
			}

			return xml;
		}

		//------------------------------------------------------------------------
		public static string ConvertXmlTextPreDefined( object value, bool also_predefined, bool process_trim )
		{
			string convertText = value.ToString();
			if( also_predefined )
			{
				convertText = convertText.Replace( "\r\n", "\\n" ).Replace( "\n", "\\n" ).Replace( "\r", "\\n" ).Replace( "\t", "\\t" ).Replace( "&", "&amp;" ).Replace( "<", "&lt;" ).Replace( ">", "&gt;" ).Replace( "\'", "&apos;" ).Replace( "\"", "&quot;" );

				// line break
				convertText = convertText.Replace( "_x000D_", "\\n" );
			}
			else
			{
				// XmlDocument.Save : automatic convert
				convertText = convertText.Replace( "\r\n", "\\n" ).Replace( "\n", "\\n" ).Replace( "\r", "\\n" ).Replace( "\t", "\\t" );
			}

			if( process_trim )
				convertText = convertText.Trim();

			int endIdx = convertText.Length;
			for( int i = endIdx - 2; i >= 0; i -= 2 )
			{
				string check_last_linefeed = convertText.Substring( i, 2 );
				if( check_last_linefeed != "\\n" )
					break;

				endIdx = i;
			}

			convertText = convertText.Substring( 0, endIdx );
			return convertText;
		}

		//------------------------------------------------------------------------	
		public static string ReverseConvertXmlPreDefined( string value )
		{
			return value.Replace( "&quot;", "\"" ).Replace( "&apos;", "\'" ).Replace( "&gt;", ">" ).Replace( "&lt;", "<" ).Replace( "&amp;", "&" ).Replace( "&nbsp;", "" );
		}

		//------------------------------------------------------------------------	
		public static XmlNode FindNode( string find_name, string find_value, XmlNodeList node_list )
		{
			foreach( XmlNode node in node_list )
			{
				string v = ParseAttribute<string>( node, find_name, "" );
				if( v == find_value )
					return node;
			}

			return null;
		}

		//------------------------------------------------------------------------	
		public static XmlDocument SafeLoad( byte[] byte_data, System.Action<string> error_callback = null )
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load( new MemoryStream( byte_data ) );

				return doc;
			}
			catch( System.Exception ex )
			{
				if( error_callback != null )
					error_callback( ex.ToString() );
			}

			return null;
		}
		public static XmlDocument SafeLoad( string xml_str, System.Action<string> error_callback = null )
		{
			if( string.IsNullOrEmpty( xml_str ) == false )
			{
				try
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml( xml_str );

					return doc;
				}
				catch( System.Exception ex )
				{
					if( error_callback != null )
						error_callback( ex.ToString() );
				}
			}

			return null;
		}
	}
}