//////////////////////////////////////////////////////////////////////////
//
// Web
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
using System.Net;
using System.Collections.Generic;
using UMF.Core;

namespace UMF.Server
{
	public class Web
	{
		//------------------------------------------------------------------------		
		public enum RequestMethodType
		{
			GET,
			POST,
			STOR
		}

		//------------------------------------------------------------------------		
		public enum WebHandleState
		{
			WaitingConnection,
			Connected,
			WaitingCompleted,
			Completed,
		}

		//------------------------------------------------------------------------		
		public class WebConfig : EnvConfig
		{
			public bool WebEnabled = true;
			public eCoreLogType WebLogType = eCoreLogType.None;
			public string WebURL = "";
			public Uri WebURI { get; private set; }
			public int WebPoolSize = 40;
			public RequestMethodType MethodType = RequestMethodType.GET;
			public string PostContentsType = "";
			public string PostKey = "";
			public string StorID = "";
			public string StorPW = "";

			protected override void ConfigLoad()
			{
				base.ConfigLoad();

				WebURI = null; 
				if( string.IsNullOrEmpty( WebURL ) == false )
					WebURI = new Uri( WebURL );				
			}
		}

		protected WebConfig mWebConfig = new WebConfig();
		public WebConfig GetWebConfig { get { return mWebConfig; } }
		public void SetLogType( eCoreLogType log_type )
		{
			mWebConfig.WebLogType = log_type;
		}

		string mWebName = "";
		public string WebName { get { return mWebName; } }

		public Uri URI { get { return mWebConfig.WebURI; } }

		Queue<WebHandlerObject> m_WaitWebHandlers = new Queue<WebHandlerObject>();
		Queue<WebHandlerObject> m_WebHandlers = new Queue<WebHandlerObject>();

		public int WaitCount { get { return m_WaitWebHandlers.Count; } }
		public int WebCount { get { return m_WebHandlers.Count; } }

		public delegate void CallbackWeb( WebHandlerObject data );

		//------------------------------------------------------------------------		
		public Web( string web_name, string config_file )
		{
			mWebName = web_name;
			LoadConfig( config_file );
		}

		//------------------------------------------------------------------------		

		void LoadConfig( string config_file )
		{
			mWebConfig.ConfigLoad( GlobalConfig.EnvConfigPath( config_file ) );
			Log.Write( mWebConfig.ToString() );
		}

		//------------------------------------------------------------------------		
		public WebRequest GetWebRequest( string strRequestStr, Uri uri, bool keep_alive, SortedList<string, string> properties )
		{
			string strRequest = "";

			if( mWebConfig.MethodType != RequestMethodType.POST )
				strRequest = strRequestStr;

			WebRequest request = null;

			if( mWebConfig.MethodType == RequestMethodType.STOR )
			{
				FtpWebRequest ftp_request = (FtpWebRequest)WebRequest.Create( uri + strRequest );
				request = ftp_request;

				request.Credentials = new NetworkCredential( mWebConfig.StorID, mWebConfig.StorPW );
			}
			else
			{
				HttpWebRequest http_request = (HttpWebRequest)WebRequest.Create( uri + strRequest );
				request = http_request;

				http_request.KeepAlive = keep_alive;
				if( mWebConfig.MethodType == RequestMethodType.POST )
					request.ContentType = mWebConfig.PostContentsType;
			}
			request.CachePolicy = new System.Net.Cache.RequestCachePolicy( System.Net.Cache.RequestCacheLevel.NoCacheNoStore );
			request.Method = mWebConfig.MethodType.ToString();

			if( properties != null && properties.Count > 0 )
			{
				foreach( KeyValuePair<string, string> pair in properties )
				{
					request.Headers.Add( pair.Key, pair.Value );
				}
			}

			return request;
		}

		//------------------------------------------------------------------------		
		public void AddWebHandler( WebHandlerObject obj )
		{
			if( mWebConfig.WebEnabled == false )
				return;

			m_WaitWebHandlers.Enqueue( obj );
		}

		//------------------------------------------------------------------------		
		public void Update()
		{
			if( m_WebHandlers.Count > 0 )
			{
				Queue<WebHandlerObject> web_handlers = new Queue<WebHandlerObject>();
				foreach( WebHandlerObject obj in m_WebHandlers )
				{
					if( obj.web_handler.MoveNext() == true )
						web_handlers.Enqueue( obj );
					else
						obj.done = true;
				}
				m_WebHandlers = web_handlers;
			}

			while( m_WaitWebHandlers.Count > 0 && m_WebHandlers.Count < mWebConfig.WebPoolSize )
			{
				WebHandlerObject obj = m_WaitWebHandlers.Dequeue();
				if( obj.web_handler.MoveNext() == true )
					m_WebHandlers.Enqueue( obj );
				else
					obj.done = true;
			}
		}
	}
}