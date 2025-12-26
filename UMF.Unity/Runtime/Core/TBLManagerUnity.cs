//////////////////////////////////////////////////////////////////////////
//
// TBLManagerUnity
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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UMF.Core;
using UnityEngine;

namespace UMF.Unity
{
	public interface ITBLManagerSetting
	{
		string DOWNLOAD_FILE_PATH { get; }
		string SERVER_DOWNLOAD_FILE_PATH { get; }
		string VERSION_STRING { get; }
		string ENCRYPT_KEY { get; }

		void PreRegist();
		void Regist();
	}

	public class TBLManagerUnity : Singleton<TBLManagerUnity>
	{
		private ITBLManagerSetting mSetting = null;
		public ITBLManagerSetting Setting { get { return mSetting; } }

#if UNITY_EDITOR
		public static bool EDITOR_USE_XML = false;
#endif

#if UNITY_EDITOR || UMDEV
		public string CUSTOM_TBL_EXPORT_PATH = "";
#endif

		public void Init( ITBLManagerSetting setting )
		{
			mSetting = setting;
			mSetting.PreRegist();
			Reload();
		}

		public void Regist()
		{
			mSetting.Regist();
			Reload();
		}

		//------------------------------------------------------------------------	
		/*
		public List<CS_TBLClientVersionData> GetTBLVersionList()
		{
			List<CS_TBLClientVersionData> list = null;

			List<string> datalist = DataListManager.Instance.GetServerDataList();
			for( int i = 0; i < datalist.Count; i++ )
			{
				string server_data = "";
				if( DataListManager.Instance.GetServerData( datalist[i], ref server_data ) )
				{
					CS_TBLClientVersionData data = new CS_TBLClientVersionData();
					data.tbl_id = datalist[i];
					data.version = DataListManager.Instance.GetVersion( datalist[i] );

					if( list == null )
						list = new List<CS_TBLClientVersionData>();
					list.Add( data );
				}
			}

			return list;
		}
		*/

		//------------------------------------------------------------------------
		public void Reload()
		{
			// delete old data
			_DeleteOldData();

			List<string> datalist = DataListManager.Instance.GetDataIDList();
			for( int i = 0; i < datalist.Count; i++ )
			{
				_LoadTBL( datalist[i] );
			}
		}

		//------------------------------------------------------------------------	
		public IEnumerator ReloadAsync( int begin, int end, System.Action<int> progress_callback )
		{
			// delete old data
			_DeleteOldData();

			int gap = end - begin;
			List<string> datalist = DataListManager.Instance.GetDataIDList();
			int data_count = datalist.Count;
			for( int i = 0; i < data_count; i++ )
			{
				// #if UNITY_EDITOR
				// 			Profiler.BeginSample( string.Format( "TBLLoad:{0}", datalist[i] ) );
				// #endif
				_LoadTBL( datalist[i] );
				// #if UNITY_EDITOR
				// 			Profiler.EndSample();
				// #endif

				if( progress_callback != null )
				{
					int _p = (int)( ( ( i + 1 ) / (float)data_count ) * ( end - begin ) );
					progress_callback( begin + _p );
				}
				yield return null;
			}
		}

		//------------------------------------------------------------------------	
		void _DeleteOldData()
		{
			if( Directory.Exists( mSetting.SERVER_DOWNLOAD_FILE_PATH ) )
			{
				string[] dirnames = Directory.GetDirectories( mSetting.SERVER_DOWNLOAD_FILE_PATH );
				for( int i = 0; i < dirnames.Length; i++ )
				{
					if( Path.GetFileName( dirnames[i] ) == mSetting.VERSION_STRING )
						continue;

					Directory.Delete( dirnames[i], true );
				}
			}
		}

		//------------------------------------------------------------------------	
		void _LoadTBL( string data_id )
		{
			Debug.Log( "TBL Load:" + data_id );

#if UNITY_EDITOR || UMDEV
			if( string.IsNullOrEmpty( CUSTOM_TBL_EXPORT_PATH ) == false )
			{
				string tb_client_path = CUSTOM_TBL_EXPORT_PATH.NormalizeSlashPath();
				string tb_data_path = Path.Combine( tb_client_path, string.Format( "{0}/{1}{2}", TBLInfoBase.EXPORT_PATH_XML, data_id, TBLInfoBase.EXTENSION_XML ) );
				if( File.Exists( tb_data_path ) )
				{
					string _xml_str = XMLUtil.LoadXmlFromFile( tb_data_path );
					if( string.IsNullOrEmpty( _xml_str ) == false )
					{
						Debug.LogFormat( "- Load:CustomPath:{0}", tb_data_path );
						if( LoadTBL( data_id, _xml_str ) )
						{
							return;
						}
					}
				}
			}
#endif

			// load from resource
			bool use_binary = DataListManager.Instance.UseXmlBinary( data_id );
			string resource_path = GlobalConfig.DataPath( $"{TBLInfoBase.DEFAULT_PATH_ROOT}/{data_id}" );

#if UNITY_EDITOR
			bool is_editor_use_xml = false;
			if( data_id.StartsWith( "TBL_" ) )
			{
				//resource_path += ".bytes";
				if( EDITOR_USE_XML || use_binary == false )
				{
					is_editor_use_xml = true;
					resource_path = GlobalConfig.DataPath( Path.Combine( TBLInfoBase.DEFAULT_PATH_ROOT, TBLInfoBase.EXPORT_PATH_CLIENT, data_id + TBLInfoBase.EXTENSION_XML ) ).NormalizeSlashPath();
				}
				else
				{
					resource_path = GlobalConfig.DataPath( Path.Combine( TBLInfoBase.DEFAULT_PATH_ROOT, TBLInfoBase.EXPORT_PATH_BINARY, data_id + TBLInfoBase.EXTENSION_XML_BINARY + ".bytes" ) ).NormalizeSlashPath();
				}
			}
			else
			{
				resource_path += TBLInfoBase.EXTENSION_XML;
			}

			TextAsset ta = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>( resource_path );
#else
			if( use_binary )
				resource_path += TBLInfoBase.EXTENSION_XML_BINARY;

			TextAsset ta = Resources.Load<TextAsset>( resource_path );;
#endif
			if( ta != null )
			{
				Debug.LogFormat( "- Load:Resource:{0}", resource_path );
#if UNITY_EDITOR
				if( is_editor_use_xml )
					LoadTBL( data_id, ta.bytes, "", false, "" );
				else
					LoadTBL( data_id, ta.bytes, "", use_binary, "" );
#else
				LoadTBL( data_id, ta.bytes, "", use_binary, "" );
#endif
				Resources.UnloadAsset( ta );
			}
			else
			{
				Debug.LogWarning( $"-- TBL Resource Not found : {resource_path}" );
			}

			// check downloaded or server TBL
#if !UNITY_WEBGL
			bool is_encrypted = false;
			bool is_binary = false;
			string load_filepath = "";

			string base_file = string.Format( "{0}/{1}", mSetting.DOWNLOAD_FILE_PATH, data_id ); ;

			string xml_filePath = base_file + TBLInfoBase.EXTENSION_XML;
			string encrypted_filePath = base_file + TBLInfoBase.EXTENSION_XML_ENCRYPT;
			string bin_filepath = base_file + TBLInfoBase.EXTENSION_XML_BINARY;
			string bin_encrypt_filepath = base_file + TBLInfoBase.EXTENSION_XML_BINARY_ENCRYPT;

			string serverFilePath = string.Format( "{0}/{1}{2}", mSetting.SERVER_DOWNLOAD_FILE_PATH, data_id, TBLInfoBase.EXTENSION_XML_ENCRYPT );

			// select order
			if( File.Exists( xml_filePath ) )
				load_filepath = xml_filePath;

			if( File.Exists( encrypted_filePath ) )
			{
				load_filepath = encrypted_filePath;
				is_encrypted = true;
			}

			if( File.Exists( bin_filepath ) )
			{
				load_filepath = bin_filepath;
				is_binary = true;
			}

			if( File.Exists( bin_encrypt_filepath ) )
			{
				load_filepath = bin_encrypt_filepath;
				is_encrypted = true;
				is_binary = true;
			}

			if( File.Exists( serverFilePath ) )
			{
				load_filepath = serverFilePath;
				is_encrypted = true;
				is_binary = false;
			}

			if( File.Exists( load_filepath ) )
			{
				Debug.LogWarningFormat( "- Load:Downloaded:{0}:encrypted:{1}", load_filepath, is_encrypted );

				try
				{
					if( is_binary == false )
					{
						string xmlStr = "";
						if( is_encrypted )
							xmlStr = XMLUtil.LoadXmlFromEncryptFile( load_filepath, mSetting.ENCRYPT_KEY );
						else
							xmlStr = XMLUtil.LoadXmlFromFile( load_filepath );

						if( string.IsNullOrEmpty( xmlStr ) == false )
						{
							if( CheckFileVersion( data_id, xmlStr, load_filepath ) == true )
								LoadTBL( data_id, xmlStr );
						}
					}
					else
					{
						if( CheckFileVersion( data_id, "", load_filepath, true, is_encrypted ) == true )
							LoadTBL( data_id, null, load_filepath, true, ( is_encrypted ? mSetting.ENCRYPT_KEY : "" ) );
					}
				}
				catch( System.Exception ex )
				{
					Debug.LogWarning( ex.ToString() );
					File.Delete( load_filepath );
				}
			}
#endif
		}

		//------------------------------------------------------------------------	
		bool CheckFileVersion( string data_id, string xml_str, string filepath, bool is_binary = false, bool is_bin_encrypt = false )
		{
			int res_version = DataListManager.Instance.GetVersion( data_id );
			int file_version = 0;
			if( is_binary )
			{
				XmlBinary xml_bin = new XmlBinary( null, filepath, true, is_bin_encrypt, mSetting.ENCRYPT_KEY );
				file_version = StringUtil.SafeParse( xml_bin.Header.GetAttribute( "version" ), 0 );
			}
			else
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml( xml_str );

				XmlNode datalistNode = doc.SelectSingleNode( "DataList" );
				if( datalistNode == null )
					return false;

				file_version = XMLUtil.ParseAttribute<int>( datalistNode, "version", 0 );
			}

			if( res_version >= file_version )
			{
				File.Delete( filepath );
				return false;
			}

			return true;
		}

		//------------------------------------------------------------------------
		public bool LoadTBL( string data_id, byte[] bytes, string filepath, bool is_binary, string encrypt_key )
		{
			return DataListManager.Instance.Load( data_id, bytes, filepath, is_binary, encrypt_key );
		}
		public bool LoadTBL( string data_id, string xml_str )
		{
			return DataListManager.Instance.Load( data_id, xml_str );
		}
		public string LoadTBLFromServer( string data_id, string xml_str )
		{
			string error = "";
			try
			{
				if( LoadTBL( data_id, xml_str ) )
				{
					// save to file(always encrypted)
					string dirPath = Path.Combine( mSetting.SERVER_DOWNLOAD_FILE_PATH, mSetting.VERSION_STRING ).NormalizeSlashPath();
					string filePath = string.Format( "{0}/{1}{2}", dirPath, data_id, TBLInfoBase.EXTENSION_XML_ENCRYPT );
					XMLUtil.SaveXmlToEncryptFile( filePath, xml_str, mSetting.ENCRYPT_KEY );
				}
			}
			catch( System.Exception ex )
			{
				Debug.LogWarning( ex.ToString() );
				error = ex.ToString();
			}

			return error;
		}

		//------------------------------------------------------------------------	
		/*
		List<CS_TBLClientData> mTemporaryTBLData = new List<CS_TBLClientData>();
		public void ReloadTBLData( CS_TBLClientData tbl_data, int total_count, bool is_endoflist )
		{
			if( tbl_data == null )
				return;

			if( string.IsNullOrEmpty( tbl_data.tbl_id ) || string.IsNullOrEmpty( tbl_data.tbl_base64 ) )
				return;

			CS_TBLClientData exist = mTemporaryTBLData.Find( t => t.tbl_id == tbl_data.tbl_id );
			if( exist != null )
				exist.tbl_base64 += tbl_data.tbl_base64;
			else
				mTemporaryTBLData.Add( tbl_data );

			if( tbl_data.is_eof )
			{
				CS_TBLClientData apply_data = mTemporaryTBLData.Find( t => t.tbl_id == tbl_data.tbl_id );
				if( apply_data == null )
					return;

				mTemporaryTBLData.RemoveAll( t => t.tbl_id == tbl_data.tbl_id );

				byte[] source = System.Convert.FromBase64String( apply_data.tbl_base64 );
				byte[] decompressed = MNS.CLZF2.Decompress( source );
				if( decompressed != null )
				{
					string xml_str = System.Text.Encoding.UTF8.GetString( decompressed );
					if( string.IsNullOrEmpty( xml_str ) == false )
					{
						LoadTBLFromServer( apply_data.tbl_id, xml_str );
					}
				}
			}

			if( is_endoflist )
				mTemporaryTBLData.Clear();
		}
		*/
	}
}