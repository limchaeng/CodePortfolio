//////////////////////////////////////////////////////////////////////////
//
// UMFSaveModule_XML
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
using System.Xml;
using System.Xml.Serialization;
using UMF.Core;
using UnityEngine;

namespace UMF.Unity
{
	//------------------------------------------------------------------------
	public class UMFSaveModule_XML : IUMFSaveModule
	{
		public const string FILE_EXTENSION = "xsave";
		public const string ENC_FILE_EXTENSION = "exsave";

		public string RootPath => UMFPath.GetDocumentsFilePath();
		public string FileExtension => FILE_EXTENSION;
		public string EncFileExtension => ENC_FILE_EXTENSION;

		public object Load( UMFSaveSettingBase setting, string _base_path, string encrypt_key, bool use_encrypt )
		{
			string base_path = _base_path.TrimEnd( '/' );
			string xml_path = UMFPath.GetDocumentsFilePath( $"{base_path}/{setting.SAVE_FILE}.{FILE_EXTENSION}" );
			string enc_xml_path = UMFPath.GetDocumentsFilePath( $"{base_path}/{setting.SAVE_FILE}.{ENC_FILE_EXTENSION}" );

			if( File.Exists( xml_path ) == false && File.Exists( enc_xml_path ) == false )
				return null;

			try
			{
				string xmlStr = "";
				bool is_migration = false;

				if( use_encrypt )
				{
					// for migration
					if( File.Exists( xml_path ) )
					{
						xmlStr = XMLUtil.LoadXmlFromFile( xml_path );
						File.Delete( xml_path );
						is_migration = true;
					}
					else if( File.Exists( enc_xml_path ) )
					{
						xmlStr = XMLUtil.LoadXmlFromEncryptFile( enc_xml_path, encrypt_key );
					}
				}
				else
				{
					// for migration
					if( File.Exists( enc_xml_path ) )
					{
						xmlStr = XMLUtil.LoadXmlFromEncryptFile( enc_xml_path, encrypt_key );
						File.Delete( enc_xml_path );
						is_migration = true;
					}
					else if( File.Exists( xml_path ) )
					{
						xmlStr = XMLUtil.LoadXmlFromFile( xml_path );
					}
				}

				object data = null;

				if( string.IsNullOrEmpty( xmlStr ) == false )
				{
					using StringReader tr = new StringReader( xmlStr );
					XmlSerializer xs = new XmlSerializer( setting.DATA_TYPE );
					data = xs.Deserialize( tr );
				}

				if( data != null && is_migration )
					Save( data, setting, _base_path, encrypt_key, use_encrypt );

				return data;
			}
			catch( System.Exception ex )
			{
				Debug.LogWarning( ex.ToString() );

				if( File.Exists( xml_path ) )
					File.Move( xml_path, $"{xml_path}_{System.DateTime.Now.ToString( "yyyyMMdd_HH_mm_ss" )}" );

				if( File.Exists( enc_xml_path ) )
					File.Move( enc_xml_path, $"{enc_xml_path}_{System.DateTime.Now.ToString( "yyyyMMdd_HH_mm_ss" )}" );
			}

			return null;
		}

		//------------------------------------------------------------------------		
		public void Save( object data, UMFSaveSettingBase setting, string _base_path, string encrypt_key, bool use_encrypt )
		{
			string base_path = _base_path.TrimEnd( '/' );
			string xml_path = UMFPath.GetDocumentsFilePath( $"{base_path}/{setting.SAVE_FILE}.{FILE_EXTENSION}" );
			if( use_encrypt )
				xml_path = UMFPath.GetDocumentsFilePath( $"{base_path}/{setting.SAVE_FILE}.{ENC_FILE_EXTENSION}" );

			try
			{
				XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
				ns.Add( string.Empty, string.Empty );
				XmlSerializer xs = new XmlSerializer( setting.DATA_TYPE );

				XmlDocument doc = new XmlDocument();

				using StringWriter sw = new StringWriter();
				xs.Serialize( sw, data, ns );

				// -- save
				string dir = Path.GetDirectoryName( xml_path );

				if( Directory.Exists( dir ) == false )
					Directory.CreateDirectory( dir );

				if( use_encrypt )
				{
					XMLUtil.SaveXmlToEncryptFile( xml_path, sw.ToString(), encrypt_key );
				}
				else
				{
					XMLUtil.SaveXmlToFile( xml_path, sw.ToString() );
				}
			}
			catch( System.Exception ex )
			{
				Debug.LogWarning( ex.ToString() );
			}
		}
	}
}
