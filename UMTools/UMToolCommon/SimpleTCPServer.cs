using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace UMTools.Common
{
	public class SimpleTCPServer
	{
		TcpListener mServer = null;
		TcpClient mClient = null;
		NetworkStream mClientStream;

		Form mMainForm = null;
		System.Action<string> mMsgHandler = null;
		System.Action<string> mLogHandler = null;

		int mPort = 0;
		public int Port { get { return mPort; } }
		int mBeginPort = 0;
		int mEndPort = 0;

		private byte[] mBuffer = new byte[49152];
		private int mBytesReceived = 0;
		private string mReceivedMessage = "";
		bool mStarted = false;

		public SimpleTCPServer( Form _form, int begin_port, int end_port, System.Action<string> log_handler, System.Action<string> msg_handler)
		{
			mMainForm = _form;
			mLogHandler = log_handler;
			mMsgHandler = msg_handler;

			mBeginPort = begin_port;
			mEndPort = end_port;
		}

		void InvokeMsg(string msg)
		{
			try
			{
				if( mMainForm != null )
				{
					System.Action<string> callback = new System.Action<string>( mMsgHandler );
					mMainForm.Invoke( callback, msg );
				}
			}
			catch (System.Exception ex)
			{
				
			}
		}
		void InvokeLog(string msg)
		{
			try
			{
				if( mMainForm != null )
				{
					System.Action<string> callback = new System.Action<string>( mLogHandler );
					mMainForm.Invoke( callback, string.Format( "[TCP] -{0}", msg ) );
				}
			}
			catch (System.Exception ex)
			{
				
			}
		}

		public int Start()
		{
			if( mStarted )
				return -1;

			for( int i = mBeginPort; i <= mEndPort; i++ )
			{
				try
				{
					mServer = new TcpListener( IPAddress.Any, i );
					mServer.Server.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.DontLinger, true );

					mServer.Start();
					mPort = i;
				}
				catch (System.Exception ex)
				{
					// already use port
					mPort = 0;
					InvokeLog( ex.ToString() );
				}

				if( mPort != 0 )
					break;
			}

			mServer.BeginAcceptTcpClient( new AsyncCallback( OnAcceptClient ), mServer );
			return mPort;
		}

		void BeginAccept()
		{
			InvokeLog( "Begin Accept" );

			try
			{
				StopClient();
			}
			catch (System.Exception ex)
			{
				// DO NOTHING
				InvokeLog( ex.ToString() );
			}

			mServer.BeginAcceptTcpClient( new AsyncCallback( OnAcceptClient ), mServer );
		}

		void OnAcceptClient( IAsyncResult ar )
		{
			if( mServer == null )
				return;

			try
			{
				TcpListener listener = (TcpListener)ar.AsyncState;
				if( listener != null )
				{
					mClient = listener.EndAcceptTcpClient( ar );
					if( mClient != null && mClient.Connected )
					{
						InvokeLog( "client connected" );
						mClientStream = mClient.GetStream();
						mClientStream.BeginRead( mBuffer, 0, mBuffer.Length, OnMessageReceived, null );
					}
				}
			}
			catch (System.Exception ex)
			{
				InvokeLog( ex.ToString() );
				BeginAccept();
			}
		}
		void OnMessageReceived( IAsyncResult result )
		{
			try
			{
				if( mClientStream != null && mClientStream.CanRead )
				{
					mBytesReceived = mClientStream.EndRead( result );
					mReceivedMessage = Encoding.UTF8.GetString( mBuffer, 0, mBytesReceived );
					if( string.IsNullOrEmpty( mReceivedMessage ) == false )
					{
						InvokeLog( mReceivedMessage );
						InvokeMsg( mReceivedMessage );
					}

					mClientStream.BeginRead( mBuffer, 0, mBuffer.Length, OnMessageReceived, null );
				}
			}
			catch (System.Exception ex)
			{
				InvokeLog( ex.ToString() );
				BeginAccept();
			}
		}

		void StopClient()
		{
			if( mClientStream != null )
				mClientStream.Close();
			mClientStream = null;

			if( mClient != null )
				mClient.Close();
			mClient = null;
		}

		public void Close()
		{
			mMainForm = null;

			StopClient();	

			if( mServer != null )
				mServer.Stop();				
			mServer = null;
		}

		public void SendMsg(string send_msg)
		{
			try
			{
				byte[] msg = Encoding.UTF8.GetBytes( send_msg );
				mClientStream.Write( msg, 0, msg.Length );

				InvokeLog( send_msg );
			}
			catch (System.Exception ex)
			{
				InvokeLog( ex.ToString() );
				BeginAccept();
			}
		}
	}
}
