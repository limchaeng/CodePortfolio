//////////////////////////////////////////////////////////////////////////
//
// TBLInfoManager
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

using System.Xml;
using System.Collections.Generic;

namespace UMF.Core
{
	public abstract class TBLInfoBase
	{
		// for Client, Tool
		public const string DEFAULT_PATH_ROOT = "_TBL";
		public const string EXPORT_PATH = "Export";
		public const string EXPORT_PATH_CLIENT = "ClientXML";
		public const string EXPORT_PATH_XML = "XML";
		public const string EXPORT_PATH_ENCRYPT = "Encrypt";
		public const string EXPORT_PATH_BINARY = "Binary";
		public const string EXPORT_PATH_BINARY_ENCRYPT = "BinaryEncrypt";

		public const string EXTENSION_XML = ".xml";
		public const string EXTENSION_XML_ENCRYPT = ".xenc";
		public const string EXTENSION_XML_BINARY = ".xbin";
		public const string EXTENSION_XML_BINARY_ENCRYPT = ".xbinenc";

		public int IDN { get; private set; }
		public string ID { get; private set; }

		virtual public bool IsValid { get { return true; } }
		public abstract void Load( XmlSelector node );
		public abstract void LoadAppend( XmlSelector node );
		public virtual string DebugInfo() { return ""; }

		public virtual void Init( int idn, string id )
		{
			this.IDN = idn;
			this.ID = id;
		}
	}

	//------------------------------------------------------------------------	
	/// <summary>
	/// TBLVerifyColumn = null;
	/// UseClient = true;
	/// CheckSameIds = false;
	/// ExportIgnore = false;
	/// I18NTextCategorySort = true;
	/// I18NTextDataSort = true;
	/// IgnoreTranslateCategorys = null;
	/// I18NTextOnly = false;
	/// FullSheetExport = false;
	/// ValidCheckHandler = null;
	/// </summary>
	public class TBLToolParameters
	{
		public List<string> TBLVerifyColumn = null;
		public bool UseClient = true;
		public bool CheckSameIds = false;
		public bool ExportIgnore = false;
		public bool I18NTextCategorySort = true;
		public bool I18NTextDataSort = true;
		public List<string> IgnoreTranslateCategorys = null;
		public bool I18NTextOnly = false;
		public bool FullSheetExport = false;
		public System.Action<string> ValidCheckHandler = null;
	}

	//------------------------------------------------------------------------	
	public abstract class TBLInfoManager<ValueType, ManagerType> : DataReloadSingleton<ManagerType>
		where ValueType : TBLInfoBase, new()
		where ManagerType : DataReloadBase, new()
	{
		public override string RELOAD_DATA_ID { get { return DATA_ID; } }
		public abstract string DATA_ID { get; }
		public string DATA_XML_ID = "";
		public abstract bool CheckSameIDN { get; }
		public abstract bool CheckSameID { get; }

		protected List<ValueType> m_Infos = new List<ValueType>();
		protected Dictionary<int, TBLInfoBase> m_InfosByIntKey = new Dictionary<int, TBLInfoBase>();
		protected Dictionary<string, TBLInfoBase> m_InfosByStrKey = new Dictionary<string, TBLInfoBase>();
		public List<ValueType> Values { get { return m_Infos; } }
		abstract protected void ParsingRow( XmlSelector rowNode );
		public int Count { get { return m_Infos.Count; } }
		public int Version { get; private set; }
		public System.DateTime CreatedTime { get; private set; }
		public System.DateTime LastLoadTime { get; private set; }

		public bool UseXMLReader { get; private set; }
		bool mUseXMLBinary = false;
		public bool EnableServerPatch { get; private set; }
		public bool IsServerRegist { get; private set; }

		protected string mBase64CompressedString = "";

		//------------------------------------------------------------------------		
		public TBLInfoManager()
		{
			EnableServerPatch = false;
			UseXMLReader = false;
			mUseXMLBinary = false;
			IsServerRegist = false;

			DataListManager.Instance.AddHandler( RELOAD_DATA_ID, LoadDataInternal, GetVersion, GetServerPatchData, UseXmlBinary );
#if UMSERVER
			RegistServer();
#endif
		}

#if UMCLIENT
		//------------------------------------------------------------------------		
		public void RegistClient( bool enable_server_patch, bool use_xml_reader, bool use_xml_bin )
		{
			EnableServerPatch = enable_server_patch;
			UseXMLReader = use_xml_reader;
			mUseXMLBinary = use_xml_bin;
			IsServerRegist = false;
		}
#endif

#if UMSERVER
		//------------------------------------------------------------------------
		public override void RegistServer()
		{
			base.RegistServer();
			IsServerRegist = true;
		}

		//------------------------------------------------------------------------		
		public override string ReloadData()
		{
			string load_url;
			if( LOAD_URL( out load_url ) )
			{
				LoadDataInternal( load_url, true, null, "", false, "" );
				return load_url;
			}

			return "";
		}
#endif

		protected virtual void PreLoadData( XmlSelector node ) { }
		protected virtual void PostLoadData( XmlSelector node ) { }
		public virtual string DebugInfo() { return ""; }

		protected ValueType CreateIDNBaseParse( XmlSelector rowNode )
		{
			int idn = XMLUtil.ParseAttribute( rowNode, "IDN", 0 );
			string id = XMLUtil.ParseAttribute( rowNode, "ID", "" );

			if( idn == 0 || ( CheckSameID && string.IsNullOrEmpty( id ) ) )
			{
				string error = string.Format( "{0} invalid IDN:{1} ID:{2}", DATA_ID, idn, id );
				throw new System.Exception( error );
			}

			if( CheckSameID )
			{
				ValueType exist_id_value = GetInfoByStrKey( id );
				if( exist_id_value != null )
				{
					string error = string.Format( "{0} already exist ID (IDN:{1} ID:{2})", DATA_ID, idn, id );
					throw new System.Exception( error );
				}
			}

			ValueType return_value = GetInfoByIntKey( idn );
			if( return_value != null )
			{
				if( CheckSameIDN )
				{
					string error = string.Format( "{0} already exist IDN (IDN:{1} ID:{2})", DATA_ID, idn, id );
					throw new System.Exception( error );
				}
				else
				{
					return_value.LoadAppend( rowNode );
				}
			}
			else
			{
				return_value = new ValueType();
				return_value.Init( idn, id );
				return_value.Load( rowNode );
				AddInfo( return_value, idn, id );
			}

			return return_value;
		}

		//------------------------------------------------------------------------
		int GetVersion()
		{
			return Version;
		}

		//------------------------------------------------------------------------	
		bool GetServerPatchData( ref string server_data )
		{
			server_data = mBase64CompressedString;
			return EnableServerPatch;
		}

		//------------------------------------------------------------------------	
		bool UseXmlBinary()
		{
			return mUseXMLBinary;
		}

		public void ImmediateLoad( string xml )
		{
			if( string.IsNullOrEmpty( xml ) == false )
				LoadDataInternal( xml, false, null, "", false, "" );
		}

		//------------------------------------------------------------------------	
		void LoadDataInternal( string xml_str_or_url, bool is_url, byte[] bytes, string filepath, bool is_binary, string bin_encrypt_key )
		{
			m_Infos.Clear();
			m_InfosByIntKey.Clear();
			m_InfosByStrKey.Clear();

			System.IO.Stream inputStream = null;
			XmlDocument doc = null;
			XmlSelector selector = new XmlSelector();
			if( is_binary && mUseXMLBinary )
			{
				Log.Write( "InfoManager:" + typeof( ManagerType ).ToString() + " Load Binary" );

				XmlBinary xml_bin = new XmlBinary( bytes, filepath, false, ( string.IsNullOrEmpty( bin_encrypt_key ) == false ), bin_encrypt_key );
				selector.m_XmlBinary = xml_bin;
				selector.m_XmlBinaryCurrNode = xml_bin.Header;
			}
			else
			{
				if( UseXMLReader )
				{
					Log.Write( "InfoManager:" + typeof( ManagerType ).ToString() + " Load Reader" );

					try
					{
						XmlReader reader = null;
						if( bytes != null )
						{
							inputStream = new System.IO.MemoryStream( bytes );
							reader = XmlReader.Create( inputStream );
						}
						else
						{
							if( is_url )
							{
								reader = XmlReader.Create( xml_str_or_url );
							}
							else
							{
								reader = XmlReader.Create( new System.IO.StringReader( xml_str_or_url ) );
							}
						}

						selector.m_XmlReader = reader;
						while( reader.Read() )
						{
							if( reader.NodeType == XmlNodeType.Element && reader.Name.Equals( "DataList" ) )
							{
								break;
							}
						}
					}
					catch( System.Exception ex )
					{
						throw ex;
					}
				}
				else
				{
					Log.Write( "InfoManager:" + typeof( ManagerType ).ToString() + " Load" );

					try
					{
						doc = new XmlDocument();
						if( bytes != null )
						{
							inputStream = new System.IO.MemoryStream( bytes );
							doc.Load( inputStream );
						}
						else
						{
							if( is_url )
								doc.Load( xml_str_or_url );
							else
								doc.LoadXml( xml_str_or_url );
						}

						XmlNode infoListNode = doc.SelectSingleNode( "DataList" );
						selector.m_XmlNode = infoListNode;
					}
					catch( System.Exception ex )
					{
						throw ex;
					}
				}
			}

			Version = XMLUtil.ParseAttribute<int>( selector, "version", 0 );
			DATA_XML_ID = XMLUtil.ParseAttribute<string>( selector, "data_id", "" );
			CreatedTime = XMLUtil.ParseAttribute<System.DateTime>( selector, "created_time", System.DateTime.Now );
			LastLoadTime = System.DateTime.Now;

			if( DATA_XML_ID != DATA_ID )
			{
				throw new System.Exception( string.Format( "Data id({0} != {1}) Not matched!", DATA_ID, DATA_XML_ID ) );
			}

			PreLoadData( selector );

			if( is_binary && mUseXMLBinary )
			{
				for( int i = 0; i < selector.m_XmlBinary.RowList.Count; i++ )
				{
					selector.m_XmlBinaryCurrNode = selector.m_XmlBinary.RowList[i];
					ParsingRow( selector );
				}
			}
			else
			{
				if( UseXMLReader )
				{
					while( selector.m_XmlReader.Read() )
					{
						if( selector.m_XmlReader.NodeType == XmlNodeType.Element && selector.m_XmlReader.Name.Equals( "Row" ) )
						{
							ParsingRow( selector );
						}
					}
				}
				else
				{
					XmlNode root_node = selector.m_XmlNode;
					foreach( XmlNode infoNode in root_node.ChildNodes )
					{
						if( infoNode.NodeType == XmlNodeType.Comment )
							continue;

						selector.m_XmlNode = infoNode;
						ParsingRow( selector );
					}
				}
			}

			PostLoadData( selector );

			if( inputStream != null )
			{
				inputStream.Close();
				inputStream.Dispose();
				inputStream = null;
			}

			if( doc != null && IsServerRegist )
			{
				string xml_text = "";
				using( System.IO.StringWriter sw = new System.IO.StringWriter() )
				{
					using( XmlWriter xmlw = XmlWriter.Create( sw ) )
					{
						doc.WriteTo( xmlw );
						xmlw.Flush();
						xml_text = sw.GetStringBuilder().ToString();
					}
				}

				if( string.IsNullOrEmpty( xml_text ) == false )
					mBase64CompressedString = StringUtil.CompressedBase64String( xml_text, true );
			}
		}

		//------------------------------------------------------------------------
		protected void AddInfo( TBLInfoBase info, int intKey, string strKey )
		{
			try
			{
				m_InfosByIntKey.Add( intKey, info );
				if( string.IsNullOrEmpty( strKey ) == false )
					m_InfosByStrKey.Add( strKey, info );

				m_Infos.Add( (ValueType)info );

			}
			catch( System.Exception ex )
			{
				Log.WriteError( "InfoManager AddInfo Exception : {0}-{1}", intKey, strKey );
				throw ex;
			}
		}

		//------------------------------------------------------------------------
		public ValueType GetInfoByIntKey( int intKey )
		{
			TBLInfoBase value;
			if( m_InfosByIntKey.TryGetValue( intKey, out value ) == false )
				return null;

			return (ValueType)value;
		}

		//------------------------------------------------------------------------
		public ValueType GetInfoByStrKey( string strKey )
		{
			TBLInfoBase value;
			if( m_InfosByStrKey.TryGetValue( strKey, out value ) == false )
				return null;

			return (ValueType)value;
		}

		//------------------------------------------------------------------------
		public ValueType GetInfoBy_Index( int index )
		{
			if( index >= 0 && index < m_Infos.Count )
				return (ValueType)m_Infos[index];

			return null;
		}

		//------------------------------------------------------------------------
		public bool Contains( int index ) { return index < m_Infos.Count; }
		public bool ContainsIntKey( int intKey ) { return m_InfosByIntKey.ContainsKey( intKey ); }
		public bool ContainsStrKey( string strKey ) { return m_InfosByStrKey.ContainsKey( strKey ); }

		//------------------------------------------------------------------------	
		public string IDN_2_ID( int idn )
		{
			ValueType info = GetInfoByIntKey( idn );
			if( info != null )
				return info.ID;

			return "";
		}
		public int ID_2_IDN( string id )
		{
			ValueType info = GetInfoByStrKey( id );
			if( info != null )
				return info.IDN;

			return 0;
		}
	}
}