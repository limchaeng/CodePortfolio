//////////////////////////////////////////////////////////////////////////
//
// DataListManager
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
using System.Linq;

namespace UMF.Core
{
	public class DataListManager : Singleton<DataListManager>
	{
		public delegate void delDataLoadHandler( string xml_str_or_url, bool is_url, byte[] bytes, string filepath, bool is_binary, string bin_encrypt_key );
		Dictionary<string, delDataLoadHandler> mDataLoadHandlers = new Dictionary<string, delDataLoadHandler>();

		public delegate int delGetVersionHandler();
		Dictionary<string, delGetVersionHandler> mGetVersionHandlers = new Dictionary<string, delGetVersionHandler>();

		public delegate bool delGetServerData( ref string server_data );
		Dictionary<string, delGetServerData> mServerDataHandlers = new Dictionary<string, delGetServerData>();

		public delegate bool delUseXmlBinary();
		Dictionary<string, delUseXmlBinary> mUseXmlBinaryHandlers = new Dictionary<string, delUseXmlBinary>();


		//------------------------------------------------------------------------	
		public List<string> GetDataIDList()
		{
			return mDataLoadHandlers.Keys.ToList();
		}

		//------------------------------------------------------------------------	
		public List<string> GetServerDataIDList()
		{
			return mServerDataHandlers.Keys.ToList();
		}

		//------------------------------------------------------------------------
		public void AddHandler( string data_id, delDataLoadHandler loadHandler, delGetVersionHandler versionHandler, delGetServerData serverDatahandler, delUseXmlBinary usexmlHandler )
		{
			if( mDataLoadHandlers.ContainsKey( data_id ) )
				mDataLoadHandlers.Remove( data_id );

			if( loadHandler != null )
				mDataLoadHandlers.Add( data_id, loadHandler );

			if( mGetVersionHandlers.ContainsKey( data_id ) )
				mGetVersionHandlers.Remove( data_id );

			if( versionHandler != null )
				mGetVersionHandlers.Add( data_id, versionHandler );

			if( mServerDataHandlers.ContainsKey( data_id ) )
				mServerDataHandlers.Remove( data_id );

			if( serverDatahandler != null )
				mServerDataHandlers.Add( data_id, serverDatahandler );

			if( mUseXmlBinaryHandlers.ContainsKey( data_id ) )
				mUseXmlBinaryHandlers.Remove( data_id );

			if( usexmlHandler != null )
				mUseXmlBinaryHandlers.Add( data_id, usexmlHandler );
		}

		//------------------------------------------------------------------------
		public bool Load( string data_id, byte[] bytes, string filepath, bool is_binary, string bin_encrypt_key )
		{
			Log.Write( "[DataList Load:" + data_id + "]" );
			if( mDataLoadHandlers.ContainsKey( data_id ) )
			{
				mDataLoadHandlers[data_id]( "", false, bytes, filepath, is_binary, bin_encrypt_key );
				return true;
			}

			return false;

		}
		public bool Load( string data_id, string xml_str )
		{
			Log.Write( "[DataList Load:" + data_id + "]" );
			if( mDataLoadHandlers.ContainsKey( data_id ) )
			{
				mDataLoadHandlers[data_id]( xml_str, false, null, "", false, "" );
				return true;
			}

			return false;
		}

		//------------------------------------------------------------------------
		public int GetVersion( string data_id )
		{
			if( mGetVersionHandlers.ContainsKey( data_id ) )
				return mGetVersionHandlers[data_id]();

			return -1;
		}

		//------------------------------------------------------------------------	
		public bool GetServerData( string data_id, ref string server_data )
		{
			if( mServerDataHandlers.ContainsKey( data_id ) )
			{
				return mServerDataHandlers[data_id]( ref server_data );
			}

			return false;
		}

		//------------------------------------------------------------------------	
		public bool UseXmlBinary( string data_id )
		{
			if( mUseXmlBinaryHandlers.ContainsKey( data_id ) )
				return mUseXmlBinaryHandlers[data_id]();

			return false;
		}
	}
}
