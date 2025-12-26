//////////////////////////////////////////////////////////////////////////
//
// SlackModuleServer
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
#if UMSERVER

using System.Runtime.Serialization;
using UMF.Core.Module;
using UMF.Server;
using UMF.Core;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace UMF.Module.SlackModule
{
	public class SlackModuleServer : ModuleCoreBase, INotification
	{
		public override string ModuleName => SlackModuleCommon.MODULE_NAME;

		//------------------------------------------------------------------------		
		[DataContract( Name = "payload" )]
		public class MessageData
		{
			[DataMember( Name = "channel" )]
			public string Channel;

			[DataMember( Name = "username" )]
			public string UserName;

			[DataMember( Name = "text" )]
			public string Text;

			[DataMember( Name = "icon_emoji" )]
			public string Icon = ":computer:";
		}

		//------------------------------------------------------------------------		
		public class SlackAPIConfig : EnvConfig
		{
			public string Channel = "";
			public int DuplicatedMsgTimeoutSeconds = 5;
			public bool SlackEnabled = true;
		}

		protected Web mWeb = null;
		public Web GetWeb { get { return mWeb; } }
		protected SlackAPIConfig mConfig = new SlackAPIConfig();
		protected IEnumerator mCurrent = null;
		protected Queue<IEnumerator> mHandlerQueue = new Queue<IEnumerator>();

		public class DuplicatedMessageData
		{
			public DateTime m_DupCheckExpireTime;
			public string m_Message;
			public int m_DuplicatedCount;
		}
		List<DuplicatedMessageData> mDuplicatedMsgCheckList = new List<DuplicatedMessageData>();
		ServerApplication mApplication = null;

		public SlackModuleServer(ServerApplication application)
		{
			mApplication = application;
			mApplication.AddUpdater( Update );
			Reload();
		}

		//------------------------------------------------------------------------
		public bool IsFinished()
		{
			return ( mCurrent == null && mHandlerQueue.Count <= 0 );
		}

		//------------------------------------------------------------------------		
		public void Send( string sender, string check_message, string full_message )
		{
			if( mWeb == null || string.IsNullOrEmpty( mConfig.Channel ) || mConfig.SlackEnabled == false )
				return;

			DateTime server_time = DateTime.Now;

			DuplicatedMessageData exist_data = mDuplicatedMsgCheckList.Find( m => m.m_Message == check_message );
			if( exist_data != null )
			{
				if( exist_data.m_DupCheckExpireTime > server_time )
				{
					exist_data.m_DuplicatedCount++;
					return;
				}
			}

			if( exist_data == null )
			{
				exist_data = new DuplicatedMessageData();
				exist_data.m_DuplicatedCount = 0;
				mDuplicatedMsgCheckList.Add( exist_data );
			}

			exist_data.m_DuplicatedCount += 1;

			exist_data.m_DupCheckExpireTime = DateTime.Now.AddSeconds( mConfig.DuplicatedMsgTimeoutSeconds );
			exist_data.m_Message = check_message;

			MessageData msg = new MessageData();
			msg.Channel = mConfig.Channel;
			msg.UserName = sender;
			msg.Text = string.Format( "{0}[{1}]", full_message, exist_data.m_DuplicatedCount );

			Enqueue( msg );
		}

		//------------------------------------------------------------------------	
		public void Reload()
		{
			string config_file = GlobalConfig.EnvConfigPath( SlackModuleCommon.CONFIG_FILE );
			if( File.Exists( config_file ) == false )
			{
				Log.WriteWarning( "SlackModule config file not found:{0}", config_file );
				mWeb = null;
				mHandlerQueue.Clear();
			}
			else
			{
				mWeb = new Web( SlackModuleCommon.MODULE_NAME, config_file );
				mConfig.ConfigLoad( config_file );
				Log.Write( mConfig.ToString() );
			}
		}

		//------------------------------------------------------------------------	
		void Update()
		{
			if( mWeb != null )
				mWeb.Update();

			if( mCurrent == null || mCurrent.MoveNext() == false )
			{
				if( mHandlerQueue.Count == 0 )
				{
					mCurrent = null;
				}
				else
				{
					mCurrent = mHandlerQueue.Dequeue();
					if( mCurrent != null && mCurrent.MoveNext() == false )
						mCurrent = null;
				}
			}
		}

		//------------------------------------------------------------------------	
		protected void Enqueue( MessageData message )
		{
			IEnumerator handler = SendMessage( message );
			mHandlerQueue.Enqueue( handler );
		}

		//------------------------------------------------------------------------			
		IEnumerator SendMessage( MessageData message )
		{
			WebHandlerObject _WebHandlerObject = new WebHandlerObject();
			_WebHandlerObject.web = mWeb;

			IEnumerator handler = WebExecute.Execute( JsonUtil.EncodeJson( message ), _WebHandlerObject );
			while( handler.MoveNext() == true )
				yield return null;

			if( _WebHandlerObject.successed == false )
			{
				Log.WriteWarning( "Slack:Send failed:{0}", _WebHandlerObject.status_description );
			}
		}
	}
}

#endif