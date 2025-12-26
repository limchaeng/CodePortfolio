//////////////////////////////////////////////////////////////////////////
//
// WebExecute
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using UMF.Core;

namespace UMF.Server
{
	public static class WebExecute
	{
		static long mRequestUniqueIndex = 0;
		public static long RequestUniqueIndex
		{
			get
			{
				mRequestUniqueIndex += 1;
				if( mRequestUniqueIndex >= long.MaxValue )
					mRequestUniqueIndex = 1;

				return mRequestUniqueIndex;
			}
		}

		//------------------------------------------------------------------------		
		public static IEnumerator WebRequestHandler( string strRequestData, WebHandlerObject data )
		{
			Stopwatch connectTime, queryTime, totalTime = Stopwatch.StartNew();

			WebRequest request = null;
			Uri uri = data.uri;
			if( uri == null )
				uri = data.web.URI;

			Log.Write( "[{0}:{1}] Begin {2}:{3}", data.web.WebName, data.unique_request_index, uri.ToString(), strRequestData );

			byte[] requestData = null;

			if( data.web.GetWebConfig.MethodType == Web.RequestMethodType.POST )
			{
				requestData = System.Text.Encoding.UTF8.GetBytes( data.web.GetWebConfig.PostKey + strRequestData );//.request_data.ToArray();
			}
			else if( data.web.GetWebConfig.MethodType == Web.RequestMethodType.STOR )
			{
				requestData = data.binary.GetBuffer();
			}

			yield return null;

			connectTime = Stopwatch.StartNew();

			while( ( request = data.web.GetWebRequest( strRequestData, uri, data.keep_alive, data.properties ) ) == null )
				yield return null;
			connectTime.Stop();

			yield return null;

			queryTime = Stopwatch.StartNew();

			string error = "";
			switch( data.web.GetWebConfig.MethodType )
			{
				case Web.RequestMethodType.POST:
				case Web.RequestMethodType.STOR:
					{
						request.ContentLength = requestData.Length;
						IAsyncResult request_result = request.BeginGetRequestStream( null, null );

						yield return null;
						while( request_result.IsCompleted == false )
							yield return null;

						Stream request_stream = null;
						try
						{
							request_stream = request.EndGetRequestStream( request_result );

							request_stream.Write( requestData, 0, requestData.Length );
							request_stream.Close();
						}
						catch( IOException ex )
						{
							request_stream = null;
							data.successed = false;
							data.status_description = ex.Message;

							if( string.IsNullOrEmpty( data.status_description ) )
								data.status_description = ex.ToString();

							error = string.Format( "[REQ:{0}:{1}] IOEx:{2}", data.web.WebName, data.unique_request_index, ex.ToString() );
						}
						catch( WebException ex )
						{
							request_stream = null;
							data.successed = false;
							if( ex.Response is HttpWebResponse )
								data.status_description = ( (HttpWebResponse)ex.Response ).StatusDescription;
							else if( ex.Response is FtpWebResponse )
								data.status_description = ( (FtpWebResponse)ex.Response ).StatusDescription;
							data.ExceptionStatus = ex.Status;

							if( string.IsNullOrEmpty( data.status_description ) )
								data.status_description = ex.ToString();

							error = string.Format( "[REQ:{0}:{1}] WebEx:{2}", data.web.WebName, data.unique_request_index, ex.ToString() );
						}

						if( string.IsNullOrEmpty( error ) == false )
							Log.WriteError( error );

						if( request_stream == null )
						{
							if( data.callback != null )
								data.callback( data );
							data.callback = null;

							yield break;
						}
					}
					break;
			}

			IAsyncResult response_result = request.BeginGetResponse( null, null );
			yield return null;
			while( response_result.IsCompleted == false )
				yield return null;

			WebResponse response = null;
			try
			{
				response = (WebResponse)request.EndGetResponse( response_result );
			}
			catch( WebException ex )
			{
				data.ExceptionStatus = ex.Status;
				data.successed = false;

				if( ex.Response is HttpWebResponse )
					data.status_description = ( (HttpWebResponse)ex.Response ).StatusDescription;
				else if( ex.Response is FtpWebResponse )
					data.status_description = ( (FtpWebResponse)ex.Response ).StatusDescription;

				error = string.Format( "[RES:{0}:{1}] WebEx:{2}", data.web.WebName, data.unique_request_index, ex.Message );
			}

			if( string.IsNullOrEmpty( error ) == false )
				Log.WriteError( error );

			if( response == null )
			{
				if( data.callback != null )
					data.callback( data );
				data.callback = null;

				yield break;
			}

			if( response is HttpWebResponse )
			{
				data.successed = ( (HttpWebResponse)response ).StatusCode == HttpStatusCode.OK;
				data.status_description = ( (HttpWebResponse)response ).StatusDescription;
			}
			else if( response is FtpWebResponse )
			{
				data.successed = ( (FtpWebResponse)response ).StatusCode == FtpStatusCode.ClosingData;
				data.status_description = ( (FtpWebResponse)response ).StatusDescription;
			}

			if( data.successed == false )
			{
				if( data.callback != null )
					data.callback( data );
				data.callback = null;

				yield break;
			}

			Stream response_stream = response.GetResponseStream();

			yield return null;

			string response_string = "";
			try
			{
				StreamReader sr = new StreamReader( response_stream );
				response_string = sr.ReadToEnd();
				data.response = response_string;
			}
			catch( System.Exception ex )
			{
				data.successed = false;
				data.status_description = "unknown error";
				Log.WriteError( "[RES:{0}:{1}] SysEx:{2}", data.web.WebName, data.unique_request_index, ex.ToString() );
			}

			if( response != null )
				response.Close();

			queryTime.Stop();

			totalTime.Stop();
			yield return null;

			Log.Write( "[{0}:{1}] time:{2}, connect:{3}, query:{4} response:{5}", data.web.WebName, data.unique_request_index, totalTime.ElapsedMilliseconds.ToString(), connectTime.ElapsedMilliseconds.ToString(), queryTime.ElapsedMilliseconds.ToString(), response_string );

			if( data.callback != null )
				data.callback( data );
			data.callback = null;
		}

		//------------------------------------------------------------------------		
		public static IEnumerator Execute( string strRequestData, WebHandlerObject data )
		{
			IEnumerator handler = WebRequestHandler( strRequestData, data );
			data.unique_request_index = RequestUniqueIndex;
			data.done = false;
			data.web_handler = handler;
			data.web.AddWebHandler( data );

			while( data.done == false )
				yield return null;
		}

		//------------------------------------------------------------------------		
		public static void ExecuteNonWaiting( string request_data, WebHandlerObject data )
		{
			IEnumerator handler = WebRequestHandler( request_data, data );
			data.unique_request_index = RequestUniqueIndex;
			data.done = false;
			data.web_handler = handler;
			data.web.AddWebHandler( data );
		}

		//------------------------------------------------------------------------		
		public static void ExecuteCallback( string request_data, WebHandlerObject data, Web.CallbackWeb callback )
		{
			IEnumerator handler = WebRequestHandler( request_data, data );
			data.unique_request_index = RequestUniqueIndex;
			data.done = false;
			data.web_handler = handler;
			data.callback = callback;
			data.web.AddWebHandler( data );
		}

		//------------------------------------------------------------------------		
		public static string ExecuteSync( Web web, string strRequestData, Uri uri, SortedList<string, string> properties, bool is_keep_alive )
		{
			WebHandlerObject _WebHandlerObject = new WebHandlerObject();
			_WebHandlerObject.uri = uri;
			_WebHandlerObject.web = web;
			_WebHandlerObject.keep_alive = is_keep_alive;
			_WebHandlerObject.properties = properties;
			_WebHandlerObject.unique_request_index = WebExecute.RequestUniqueIndex;

			IEnumerator handler = WebExecute.WebRequestHandler( strRequestData, _WebHandlerObject );
			while( handler.MoveNext() == true )
				System.Threading.Thread.Sleep( 1 );

			return _WebHandlerObject.response;
		}
	}
}
