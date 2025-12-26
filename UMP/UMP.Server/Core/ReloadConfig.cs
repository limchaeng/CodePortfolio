//////////////////////////////////////////////////////////////////////////
//
// ReloadConfig
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
using UMF.Core;
using System;
using System.Xml;
using System.Collections.Generic;

namespace UMP.Server
{
	public abstract class ReloadConfig<T> : DataReloadSingleton<T> where T : DataReloadBase, new()
	{
		public abstract string ROOT_NODE { get; }
		public DateTime LastReloadTime { get; protected set; }

		//------------------------------------------------------------------------
		public ReloadConfig()
		{
			RegistServer();
		}

		//------------------------------------------------------------------------
		public override string ReloadData()
		{
			string load_url;
			if( LOAD_URL( out load_url ) )
			{
				LastReloadTime = DateTime.Now;

				XmlDocument doc = new XmlDocument();
				doc.Load( load_url );

				LoadConfigData( doc.SelectSingleNode( ROOT_NODE ) );
			}

			return load_url;
		}

		protected abstract void LoadConfigData( XmlNode root_node );
	}
}
