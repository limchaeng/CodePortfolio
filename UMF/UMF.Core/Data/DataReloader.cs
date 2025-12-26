//////////////////////////////////////////////////////////////////////////
//
// DataReloader
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
using System.Linq;
using System.Net;

namespace UMF.Core
{
	//------------------------------------------------------------------------
	public interface IExecuteAfterFirstReloadData
	{
		bool IsExecutedAfterFirstReload { get; set; }
		void ExecuteAfterFirstReload();
	}

	//------------------------------------------------------------------------
	public abstract class DataReloadBase
	{
		public abstract string RELOAD_DATA_ID { get; }
		public abstract string ReloadData();
		public abstract void SyncFromReloadInternal( DataReloadBase prev_data );
		public string STATIC_PATH { get; set; } = "";
		public string FILEEXT { get; set; } = ".xml";

		//------------------------------------------------------------------------		
		public bool LOAD_URL( out string load_url )
		{
			load_url = "";

			// static path check
			if( string.IsNullOrEmpty( STATIC_PATH ) == false )
			{
				bool is_success = CheckFileExist( STATIC_PATH );
				if( is_success )
					load_url = STATIC_PATH;

				return is_success;
			}

			// dynamic name
			string file_path = GlobalConfig.DataPath( RELOAD_DATA_ID + FILEEXT );
			if( CheckFileExist( file_path ) )
			{
				Log.WriteImportant( string.Format( "-> Reload {0} URL:{1}", RELOAD_DATA_ID, load_url ) );
				return true;
			}
			else
			{
				Log.WriteImportant( string.Format( "-> Reload {0} URL:{1} NOT FOUND!!", RELOAD_DATA_ID, load_url ) );
				load_url = "";
				return false;
			}
		}

		//------------------------------------------------------------------------
		protected bool CheckFileExist( string url )
		{
			bool is_exist = false;
			if( url.StartsWith( "http" ) )
			{
				HttpWebResponse response = null;
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create( url );
				request.Timeout = 1000;
				request.Method = "HEAD";

				try
				{
					is_exist = true;
					response = (HttpWebResponse)request.GetResponse();
				}
				catch( System.Exception ex )
				{
					is_exist = false;
				}
				finally
				{
					if( response != null )
						response.Close();
				}
			}
			else if( File.Exists( url ) )
			{
				is_exist = true;
			}

			return is_exist;
		}
	}

	//------------------------------------------------------------------------
	public abstract class DataReloadSingleton<T> : DataReloadBase where T : DataReloadBase, new()
	{
		static T _Instance = null;
		static object _lock = new object();

		public static T Instance
		{
			get
			{
				if( _Instance == null )
				{
					lock( _lock )
					{
						MakeInstance();
					}
				}

				return _Instance;
			}
		}

		public static void MakeInstance()
		{
			if( _Instance == null )
				_Instance = new T();
		}

		public static void ClearInstance()
		{
			if( _Instance != null )
				_Instance = null;
		}

		//------------------------------------------------------------------------
		public virtual void SyncFromReload( T prev_instance ) { }

		//------------------------------------------------------------------------
		public sealed override void SyncFromReloadInternal( DataReloadBase prev_data )
		{
			SyncFromReload( (T)prev_data );
		}

		//------------------------------------------------------------------------		
#if UMSERVER
		public virtual void RegistServer()
		{
			DataReloader.Instance.AddReload( RELOAD_DATA_ID, Reload_Handler );
		}
#endif

		//------------------------------------------------------------------------		
		static string Reload_Handler()
		{
			string static_path = Instance.STATIC_PATH;
			string file_ext = Instance.FILEEXT;

			T prev_instace = Instance;

			ClearInstance();
			MakeInstance();

			Instance.STATIC_PATH = static_path;
			Instance.FILEEXT = file_ext;

			Instance.SyncFromReloadInternal( prev_instace );

			return Instance.ReloadData();
		}
	}

	//------------------------------------------------------------------------	
	public class DataReloader : Singleton<DataReloader>
	{
		public delegate string ReloadDelegate();

		string mReloadSaveFileName = "_reloda_save";
		public string ReloadSaveFileName { set { mReloadSaveFileName = value; } get { return mReloadSaveFileName; } }
		public bool ReloadDataSave { get; set; } = true;
		public string ReloadSaveNameSuffix { get; set; } = "";

		public class ReloadInfo
		{
			public string data_id = "";
			public ReloadDelegate reload_handler = null;

			public string last_loaded_path = "";
			public int reload_count = 0;
		}
		List<ReloadInfo> mReloadInfoList = new List<ReloadInfo>();
		public List<ReloadInfo> ReloadInfoList { get { return mReloadInfoList; } }

		List<IExecuteAfterFirstReloadData> mExecuteAfterFirstReloadList = new List<IExecuteAfterFirstReloadData>();

		//------------------------------------------------------------------------
		public void AddReload( string data_id, ReloadDelegate handler )
		{
			ReloadInfo info = mReloadInfoList.Find( a => a.data_id == data_id );
			if( info != null )
			{
				Log.Write( "# DataReloader [{0}] already exists. update!", data_id );
				info.reload_handler = handler;
			}
			else
			{
				info = new ReloadInfo();
				info.data_id = data_id;
				info.reload_handler = handler;
				info.last_loaded_path = "";
				info.reload_count = 0;

				mReloadInfoList.Add( info );
			}
		}

		//------------------------------------------------------------------------
		public void AddExecuteAfterFirstReload( IExecuteAfterFirstReloadData check )
		{
			if( mExecuteAfterFirstReloadList.Contains( check ) == false )
				mExecuteAfterFirstReloadList.Add( check );
		}
		void ExecuteAfterFirstReload()
		{
			if( mExecuteAfterFirstReloadList.Count > 0 )
			{
				foreach( IExecuteAfterFirstReloadData data in mExecuteAfterFirstReloadList )
				{
					data.IsExecutedAfterFirstReload = true;
					data.ExecuteAfterFirstReload();
				}
				mExecuteAfterFirstReloadList.Clear();
			}
		}

		//------------------------------------------------------------------------		
		public string ReloadData( List<string> _id_list )
		{
			string response = $"ReloadData:";

			List<string> id_list = _id_list;
			if( id_list == null )
				id_list = mReloadInfoList.Select( a => a.data_id ).ToList();

			if( id_list != null )
			{
				foreach( string id in id_list )
				{
					ReloadInfo info = mReloadInfoList.Find( a => a.data_id == id );
					if( info != null && info.reload_handler != null )
					{
						info.reload_count += 1;
						info.last_loaded_path = info.reload_handler();
						if( string.IsNullOrEmpty( info.last_loaded_path ) )
							response += string.Format( "\n[{0}:NOT]", id );
						else
							response += string.Format( "\n[{0}:{1}:OK]", id, info.last_loaded_path );
					}
					else
					{
						response += string.Format( "\n[{0}:IGNORE]", id );
					}
				}
			}

			Log.WriteImportant( $"DataReloader:{response}" );

			if( ReloadDataSave )
				SaveReloadDataInfo();

			ExecuteAfterFirstReload();

			return response;
		}

		//------------------------------------------------------------------------		
		public ReloadInfo GetInfo( string id )
		{
			return mReloadInfoList.Find( a => a.data_id == id );
		}

		//------------------------------------------------------------------------	
		public void SaveReloadDataInfo()
		{
			if( ReloadDataSave == false )
				return;

			XmlDocument doc = new XmlDocument();
			XmlNode rootNode = doc.AppendChild( doc.CreateElement( "ReloadDataInfo" ) );
			foreach( ReloadInfo info in mReloadInfoList )
			{
				XmlNode data_node = rootNode.AppendChild( rootNode.OwnerDocument.CreateElement( "Data" ) );
				XMLUtil.AddAttribute( data_node, "id", info.data_id );
			}

			doc.Save( string.Format( "{0}_{1}.xml", mReloadSaveFileName, ReloadSaveNameSuffix ) );
		}

		//------------------------------------------------------------------------	
		public void LoadFromSavedFile()
		{
			if( ReloadDataSave == false )
				return;

			try
			{
				string filename = string.Format( "{0}_{1}.xml", mReloadSaveFileName, ReloadSaveNameSuffix );

				if( System.IO.File.Exists( filename ) == true )
				{
					XmlDocument doc = new XmlDocument();
					doc.Load( filename );

					XmlNode rootNode = doc.SelectSingleNode( "ReloadDataInfo" );
					if( rootNode != null )
					{
						foreach( XmlNode child in rootNode.ChildNodes )
						{
							string id = XMLUtil.ParseAttribute<string>( child, "id", "" );
							if( string.IsNullOrEmpty( id ) )
								continue;

							ReloadInfo info = mReloadInfoList.Find( a => a.data_id == id );
							if( info != null && info.reload_handler != null )
								info.reload_handler();
						}
					}
				}
			}
			catch( System.Exception ex )
			{
				Log.WriteError( ex.ToString() );
			}
		}
	}
}