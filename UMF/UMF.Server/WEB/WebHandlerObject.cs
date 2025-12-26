//////////////////////////////////////////////////////////////////////////
//
// WebHandlerObject
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
using System.IO;
using System.Net;

namespace UMF.Server
{
	public class WebHandlerObject
	{
		public Web web;
		public MemoryStream binary;
		public bool successed = false;
		public string status_description;
		public WebExceptionStatus ExceptionStatus = WebExceptionStatus.Success;
		public string response;
		public long recvIndex;
		public Uri uri;
		public bool keep_alive = true;
		public SortedList<string, string> properties;
		public bool done;
		public IEnumerator web_handler;
		public Web.CallbackWeb callback = null;
		public long unique_request_index;
	}
}
