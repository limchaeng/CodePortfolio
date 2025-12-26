//////////////////////////////////////////////////////////////////////////
//
// ClientLimit
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

using System.Xml;
using System.IO;
using UMF.Core;

namespace UMP.Server.Master
{
	public class ClientLimit : Singleton<ClientLimit>
	{
		int mDefaultLimit = 5000;
		public int DefaultLimit { get { return mDefaultLimit; } }
		readonly string KEY_FILE_NAME = "_client_limit_master.xml";
		XmlDocument mDoc = null;

		public ClientLimit()
		{
			LoadData();
		}

		//------------------------------------------------------------------------		
		public void LoadData()
		{
			mDoc = new XmlDocument();

			if( File.Exists( KEY_FILE_NAME ) )
			{
				mDoc.Load( KEY_FILE_NAME );
				mDefaultLimit = int.Parse( mDoc.DocumentElement.Attributes["limit"].Value );
			}
			else
			{
				XmlNode userLimitNode = mDoc.AppendChild( mDoc.CreateElement( "ClientLimit" ) );
				XmlAttribute limit_attr = mDoc.CreateAttribute( "limit" );
				limit_attr.Value = mDefaultLimit.ToString();
				mDoc.DocumentElement.Attributes.Append( limit_attr );
				mDoc.Save( KEY_FILE_NAME );
			}
		}

		//------------------------------------------------------------------------		
		void SaveData()
		{
			if( mDoc != null )
			{
				mDoc.DocumentElement.Attributes["limit"].Value = mDefaultLimit.ToString();
				mDoc.Save( KEY_FILE_NAME );
			}
		}

		//------------------------------------------------------------------------		
		public void SetDefaultClientLimit( int limit )
		{
			if( limit == mDefaultLimit )
				return;

			mDefaultLimit = limit;
			SaveData();
		}
	}
}
