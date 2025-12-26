using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UMTools.Common
{
	public class SimpleTCPClient
	{
		public int Port { get; private set; }
		public string IP { get; private set; }

		private System.Net.Sockets.TcpClient mTCPClient;
		private System.Net.Sockets.NetworkStream mTCPClientStream = null;
		private byte[] mBuffer = new byte[49152];
		private int mBytesReceived = 0;
		private string mReceivedMessage = "";

		bool mReconnect = false;
		bool mConnecting = false;

		System.Action<string> mMsgHanlder = null;
		System.Action<string> mLogHandler = null;
		Form mMainForm = null;

		public SimpleTCPClient( Form main_form, string ip_addr, int port, System.Action<string> log_handler, System.Action<string> msg_handler )
		{
			mMainForm = main_form;

			IP = ip_addr;
			Port = port;

			mLogHandler = log_handler;
			mMsgHanlder = msg_handler;

			mTCPClient = new System.Net.Sockets.TcpClient();
		}

		//------------------------------------------------------------------------
		public void Update()
		{
			if( mReconnect )
			{
				Connect();
			}
		}

		//------------------------------------------------------------------------	
		public void Connect()
		{
			if( mTCPClient.Connected )
				return;

			mReconnect = false;
			mConnecting = true;

			try
			{
				mTCPClient.BeginConnect( IP, Port, OnConnected, null );
				InvokeLog( "Client Starting..." );
			}
			catch( System.Exception ex )
			{
				CloseClient();
			}
		}

		//------------------------------------------------------------------------	
		void OnConnected( System.IAsyncResult result )
		{
			mConnecting = false;

			try
			{
				InvokeLog( "OnConnected : " + mTCPClient.Connected );

				if( mTCPClient.Connected )
				{
					mTCPClientStream = mTCPClient.GetStream();
					mTCPClientStream.BeginRead( mBuffer, 0, mBuffer.Length, MessageReceived, null );
				}
			}
			catch( System.Exception ex )
			{
				CloseClient();
			}
		}

		//------------------------------------------------------------------------
		private void MessageReceived( System.IAsyncResult result )
		{
			try
			{
				if( result.IsCompleted && mTCPClient.Connected )
				{
					mBytesReceived = mTCPClientStream.EndRead( result );
					mReceivedMessage = System.Text.Encoding.UTF8.GetString( mBuffer, 0, mBytesReceived );

					if( string.IsNullOrEmpty( mReceivedMessage ) == false )
					{
						InvokeMsg( mReceivedMessage );
						mTCPClientStream.BeginRead( mBuffer, 0, mBuffer.Length, MessageReceived, null );
						InvokeLog( "Msg received on Client: " + mReceivedMessage );
					}
				}
			}
			catch( System.Exception ex )
			{
				CloseClient();
			}
		}

		public void SendMsg( string send_msg )
		{
			try
			{
				byte[] msg = System.Text.Encoding.UTF8.GetBytes( send_msg );
				mTCPClientStream.Write( msg, 0, msg.Length );
				InvokeLog( "Msg send to Server: " + send_msg );
			}
			catch( System.Exception ex )
			{
				CloseClient();
			}
		}

		//------------------------------------------------------------------------	
		private void CloseClient()
		{
			try
			{
				if( mTCPClient.Connected )
					mTCPClient.Close();
			}
			catch( System.Exception ex )
			{
				
			}

			mReconnect = true;
		}

		void InvokeMsg( string msg )
		{
			try
			{
				if( mMainForm != null )
				{
					System.Action<string> callback = new System.Action<string>( mMsgHanlder );
					mMainForm.Invoke( callback, msg );
				}
			}
			catch( System.Exception ex )
			{

			}
		}
		void InvokeLog( string msg )
		{
			try
			{
				if( mMainForm != null )
				{
					System.Action<string> callback = new System.Action<string>( mLogHandler );
					mMainForm.Invoke( callback, string.Format( "[TCP] -{0}", msg ) );
				}
			}
			catch( System.Exception ex )
			{

			}
		}
	}
}