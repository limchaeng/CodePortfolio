//////////////////////////////////////////////////////////////////////////
//
// ErrorLogTracer
// 
// Created by LCY.
//
// Copyright 2022 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using System.IO;
using UMF.Core;
using UMF.Server;

namespace UMP.Server.Daemon
{
	//------------------------------------------------------------------------	
	public class ErrorLogTracer
	{
		StreamReader reader;

		DaemonServerApplication mApplication = null;

		public ErrorLogTracer(DaemonServerApplication application)
		{
			mApplication = application;				 

			string filename = string.Format( "{0}/Errors.txt", Log.LOG_PATH );
			FileStream fs = new FileStream( filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite );
			fs.Seek( 0, SeekOrigin.End );
			reader = new StreamReader( fs, System.Text.Encoding.UTF8 );

			mApplication.AddUpdater( Update );
		}

		//------------------------------------------------------------------------		
		public void Update()
		{
			string str = reader.ReadLine();
			if( string.IsNullOrEmpty( str ) == false )
				mApplication.WriteConsole( str, ServerApplication.LogType.Error );
		}
	}
}
