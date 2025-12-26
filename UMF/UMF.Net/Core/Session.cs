//////////////////////////////////////////////////////////////////////////
//
// Session
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

//#define BINARY_LOG
//#define IGNORE_PACKET_HANDLER_CATCH

using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;
using UMF.Core;

namespace UMF.Net
{
	//------------------------------------------------------------------------	
	public abstract class Session
	{
		protected Encryptor m_SendEncryptor, m_RecvEncryptor;

		protected eDisconnectType m_DisconnectType = eDisconnectType.Disconnected;

		public int recvBytes = 0, sendBytes = 0;
		public double added_recvBytes = 0, added_sendBytes = 0;
		public int recvCount = 0, sendCount = 0;

		protected virtual bool IsUseErrorDetail { get { return true; } }

		long m_SendCount = 0, m_SendHandleCount = 0, m_RecvCount = 0, m_RecvHandleCount = 0;
		public long SendCount { get { return m_SendCount; } }
		public long SendHandleCount { get { return m_SendHandleCount; } }
		public long RecvCount { get { return m_RecvCount; } }
		public long RecvHandleCount { get { return m_RecvHandleCount; } }

		public string strRemoteEndPoint = "";

		protected string mSessionName = "Session";
		public virtual string SessionName { get { return mSessionName; } }
		protected abstract int SendBufferSize { get; }
		protected abstract int RecvBufferSize { get; }

		public abstract Type SendPacketIdType { get; }
		public abstract Type NSendPacketIdType { get; }
		public abstract Type RecvPacketIdType { get; }
		public abstract Type NRecvPacketIdType { get; }

		public abstract eCoreLogType CoreLogType { get; }

		protected DateTime m_LastSendTime = DateTime.Now, m_LastRecvTime = DateTime.Now;
		protected abstract MemoryStream PingPacket { get; }
		protected abstract TimeSpan PingTime { get; }
		protected abstract TimeSpan PingCheckTime { get; }

		protected TimeSpan? m_TempPingCheckTime = null;
		public TimeSpan? GetTempPingCheckTime() { return m_TempPingCheckTime; }
		public void SetTempPingCheckTime( TimeSpan time ) { m_TempPingCheckTime = time; }
		public void ResetTempPingCheckTime() { m_TempPingCheckTime = null; }

		protected bool m_bEnablePingCheck = true;
		protected float m_DisablePingCheckTimeout = 0f;
		protected DateTime m_DisablePingCheckExpireTime = DateTime.Now;
		public bool EnablePingCheck
		{
			get { return m_bEnablePingCheck; }
		}

		public void SetEnablePingCheck( bool bEnable, float timeout = 0f )
		{
			if( bEnable )
			{
				m_bEnablePingCheck = true;
				m_LastRecvTime = DateTime.Now;
			}
			else
			{
				m_bEnablePingCheck = false;
				m_DisablePingCheckTimeout = timeout;
				m_DisablePingCheckExpireTime = DateTime.Now.AddSeconds( timeout );
			}
		}

		protected bool bDisconnecting = false;
		protected bool bConnected = false;

		public abstract bool Verified { get; }
		protected abstract short VerifyPacketId { get; }
		protected abstract string VerifyPacketLog( object packet );
		protected abstract short VerifiedAckPacketId { get; }
		protected virtual bool IsInternalStreamPacketId( short packet_id )
		{
			if( packet_id == VerifyPacketId || packet_id == VerifiedAckPacketId )
				return true;

			switch( packet_id )
			{
				case PacketCoreID.HeartbeatId:
				case PacketCoreID.HeartbeatAckId:
				case PacketCoreID.ModulePacket:
					return true;
			}

			return false;
		}

		public delegate void OnDisconnectedDelegate( Session session, int error_code, string error_string );
		protected OnDisconnectedDelegate _OnDisconnected;
		public OnDisconnectedDelegate OnDisconnectedCallback { set { _OnDisconnected = value; } }
		public void AddDisconnectedCallback( OnDisconnectedDelegate callback ) { _OnDisconnected += callback; }
		public void RemoveDisconnectedCallback( OnDisconnectedDelegate callback ) { _OnDisconnected -= callback; }

		protected string mCurrLanguage = "";
		public virtual string CurrLanguage
		{
			get { return mCurrLanguage; }
			set { mCurrLanguage = value; }
		}

		//------------------------------------------------------------------------	
		public bool Connected
		{
			get { return bConnected; }
		}

		//------------------------------------------------------------------------	
		protected Socket m_Socket = null;
		public virtual Socket Socket
		{
			get
			{
				return m_Socket;
			}
			set { }
		}

		//------------------------------------------------------------------------	
		public class PacketWaitInfo
		{
			public MemoryStream packet;
			public long recv_count;
			public ushort packet_size;

			public PacketWaitInfo( MemoryStream packet, long recv_count, ushort size )
			{
				this.packet = packet;
				this.recv_count = recv_count;
				this.packet_size = size;
			}
		}

		//------------------------------------------------------------------------	
		protected void ProcessWaitVerifyPackets()
		{
			if( m_SendWaitVerifyPackets.Count > 0 )
			{
				foreach( object sendpacket in m_SendWaitVerifyPackets )
				{
					m_SendPackets.Enqueue( sendpacket );
				}
				m_SendWaitVerifyPackets.Clear();
			}

			if( m_RecvWaitVerifyPackets.Count > 0 )
			{
				foreach( PacketWaitInfo info in m_RecvWaitVerifyPackets )
				{
					PacketContainer packet = mPacketHandlerManager.deserialize_packet( this, info.packet, info.recv_count, true );
					if( packet != null )
						m_RecvPackets.Enqueue( packet );
				}
				m_RecvWaitVerifyPackets.Clear();
			}
		}

		//------------------------------------------------------------------------	
		public void ProcessSendWaitPackets()
		{
			if( m_SendWaitPackets.Count > 0 )
			{
				foreach( object sendpacket in m_SendWaitPackets )
				{
					m_SendPackets.Enqueue( sendpacket );
				}
				m_SendWaitPackets.Clear();
			}
		}

		//------------------------------------------------------------------------	
		public void ClearSendWaitPackets()
		{
			m_SendWaitPackets.Clear();
		}

		protected Queue<PacketWaitInfo> m_RecvWaitVerifyPackets = new Queue<PacketWaitInfo>();
		protected Queue<PacketContainer> m_RecvPackets = new Queue<PacketContainer>();
		Queue<object> m_SendPackets = new Queue<object>();
		Queue<object> m_SendWaitVerifyPackets = new Queue<object>();
		Queue<object> m_SendWaitPackets = new Queue<object>();
		public int SendWaitPacketCount { get { return m_SendWaitPackets.Count; } }
		MemoryStream m_RecvPacket = null, m_SendPacket = null;

		public int m_RecvLeft = -1;

		protected int m_PacketPositionCheck = 2;

		protected PacketHandlerManagerBase mPacketHandlerManager = null;
		public PacketHandlerManagerBase PacketHandlerManager { get { return mPacketHandlerManager; } }

		byte[] m_SendBuffer = null, m_SendBufferTemp = null, m_RecvBuffer = null;
		int m_SendBufferOffset = 0, m_SendPacketOffset = 0;
		int m_RecvBufferOffset = 0, m_RecvBufferSize = 0;

		protected PacketDisconnect m_Disconnect = new PacketDisconnect();
		public PacketDisconnect DisconnectedInfo { get { return m_Disconnect; } }
		protected void SetDisconnectInfo( PacketDisconnect disconnect )
		{
			m_Disconnect.error_code = disconnect.error_code;
			m_Disconnect.error_string = disconnect.error_string;
			m_Disconnect.error_detail_string = disconnect.error_detail_string;

			if( m_Disconnect.error_detail_string == "" )
				m_Disconnect.error_detail_string = m_Disconnect.error_string;
		}

		public abstract PacketFormatterConfig GetPacketFormatterConfig { get; }

		//------------------------------------------------------------------------	
		public int SendPacketCount
		{
			get
			{
				return m_SendPackets.Count;
			}
		}

		//------------------------------------------------------------------------	
		public int RecvPacketCount
		{
			get
			{
				return m_RecvPackets.Count;
			}
		}

		protected abstract List<PacketSendInterruptHandlerBase> GetPacketSendInterruptors( short packet_id );

		//------------------------------------------------------------------------	
		protected Session()
		{
			m_Disconnect.error_code = (int)eDisconnectErrorCode.UnknownError;
			m_Disconnect.error_detail_string = m_Disconnect.error_string = "Unknown Error";
		}

		~Session()
		{
		}

		//------------------------------------------------------------------------	
		public virtual void Close()
		{
			if( m_Socket != null )
			{
				try
				{
					m_Socket.Shutdown( SocketShutdown.Both );
				}
				catch( System.Exception ex )
				{
					Log.WriteWarning( "Socket:{0} Shutdown(close) Exception:{1}", strRemoteEndPoint, ex.ToString() );
				}
				finally
				{
					m_Socket.Close();
				}
			}
			m_Socket = null;

			bDisconnecting = false;

			m_SendEncryptor = null;
			m_RecvEncryptor = null;

			m_SendBuffer = null;
			m_SendBufferTemp = null;
			m_SendPacket = null;
			m_RecvBuffer = null;
			m_RecvPacket = null;

			m_SendPackets.Clear();
			m_RecvPackets.Clear();

			m_SendBufferOffset = 0;
			m_SendPacketOffset = 0;
			m_RecvBufferOffset = 0;
			m_RecvBufferSize = 0;

			m_SendCount = 0;
			m_SendHandleCount = 0;
			m_RecvCount = 0;
			m_RecvHandleCount = 0;

			recvBytes = 0;
			sendBytes = 0;
			recvCount = 0;
			sendCount = 0;

			added_recvBytes = 0;
			added_sendBytes = 0;

			m_RecvLeft = -1;
		}

		//------------------------------------------------------------------------	
		public bool IsElapsedFromDisconnected( long msec ) { if( m_DisconnectedWatch == null ) return true; return m_DisconnectedWatch.ElapsedMilliseconds > msec; }
		Stopwatch m_DisconnectedWatch;
		protected virtual void OnDisconnected()
		{
			m_DisconnectedWatch = Stopwatch.StartNew();

			if( CoreLogType != Core.eCoreLogType.None )
				Log.Write( "[{0}][AllPacketSize] OnDisconnected {1} : SendCount:{2}, SendAll:{3} byte, RecvCount:{4} RecvAll:{5} byte", SessionName, strRemoteEndPoint, m_SendCount, added_sendBytes, m_RecvCount, added_recvBytes );

			Close();
			bConnected = false;

			try
			{
				if( _OnDisconnected != null )
					_OnDisconnected( this, m_Disconnect.error_code, m_Disconnect.error_string );
			}
			catch( System.Exception ex )
			{
				Log.WriteWarning( "[{0}][_OnDisconnected] OnDisconnected {1}:{2}", SessionName, strRemoteEndPoint, ex.ToString() );
			}
		}

		//------------------------------------------------------------------------	
		public virtual void handle_input()
		{
			if( bDisconnecting == true || m_Socket == null || m_Socket.Connected == false )
				return;

			SocketError se;
			int copyLength = 0;

			while( true )
			{
				if( m_RecvLeft == 0 )
					break;

				if( m_RecvBufferOffset == m_RecvBufferSize )
				{
					if( m_RecvBuffer == null )
						m_RecvBuffer = new byte[RecvBufferSize];

					m_RecvBufferOffset = 0;
					m_RecvBufferSize = m_Socket.Receive( m_RecvBuffer, 0, m_RecvBuffer.Length, SocketFlags.None, out se );

					if( m_RecvBufferSize == 0 && se != SocketError.WouldBlock /*|| se == SocketError.ConnectionReset || se == SocketError.ConnectionAborted*/)
					{
						try
						{
							m_Socket.Shutdown( SocketShutdown.Both );
						}
						catch( System.Exception ex )
						{
							Log.WriteWarning( "Socket:{0} se:{1} Shutdown(Receive) Exception:{2}", strRemoteEndPoint, se.ToString(), ex.ToString() );
						}
						finally
						{
							m_Socket.Close();
							m_Socket = null;
						}
					}

					if( m_RecvBufferSize == 0 )
						break;

					m_LastRecvTime = DateTime.Now;
#if BINARY_LOG
                    Log.Write("Recv from {0} : {1}", strRemoteEndPoint, BitConverter.ToString(m_RecvBuffer, 0, m_RecvBufferSize));
#endif

					recvBytes += m_RecvBufferSize;
					added_recvBytes += m_RecvBufferSize;
				}

				if( m_RecvPacket == null )
					m_RecvPacket = new MemoryStream();

				if( m_RecvPacket.Position < m_PacketPositionCheck )
				{
					copyLength = (int)Math.Min( m_PacketPositionCheck - m_RecvPacket.Position, m_RecvBufferSize - m_RecvBufferOffset );
					//System.Diagnostics.Debug.Assert(copyLength >= 0);
					m_RecvPacket.Write( m_RecvBuffer, m_RecvBufferOffset, copyLength );
					m_RecvBufferOffset += copyLength;

					if( m_RecvEncryptor != null && m_RecvPacket.Position >= m_PacketPositionCheck )
					{
						try
						{
							m_RecvEncryptor.DecryptHeader( m_RecvPacket );
						}
						catch (System.Exception ex)
						{
							throw new PacketException( (int)eDisconnectErrorCode.DecryptError, ex.Message );
						}						
					}
				}

				if( m_RecvPacket.Position >= m_PacketPositionCheck )
				{
					bool bDummy = false;

					ushort packetSize;
					if( m_RecvEncryptor != null )
					{
						packetSize = (ushort)( BitConverter.ToUInt16( m_RecvPacket.GetBuffer(), 2 ) + 2 );
						if( packetSize == 2 )
							bDummy = true;
					}
					else
					{
						packetSize = BitConverter.ToUInt16( m_RecvPacket.GetBuffer(), 0 );
						if( packetSize == 0 )
							bDummy = true;
					}

					if( bDummy == true )
					{
						if( m_RecvEncryptor != null )
							m_RecvEncryptor.NextKey();
						++m_RecvCount;
						recvCount++;

						m_RecvPacket.Dispose();
						m_RecvPacket = null;
					}
					else
					{
						copyLength = (int)( packetSize - m_RecvPacket.Length );
						//System.Diagnostics.Debug.Assert(copyLength >= 0);
						if( copyLength <= m_RecvBufferSize - m_RecvBufferOffset )
						{
							m_RecvPacket.Write( m_RecvBuffer, m_RecvBufferOffset, copyLength );
							m_RecvBufferOffset += copyLength;

							if( m_RecvEncryptor != null )
								m_RecvPacket = m_RecvEncryptor.Decrypt( m_RecvPacket );

#if BINARY_LOG
                            Log.Write("Recv from {0}, recvIndex{1} : {2}", strRemoteEndPoint, m_RecvCount, BitConverter.ToString(m_RecvPacket.GetBuffer(), 0, (int)m_RecvPacket.Length));
#endif

							m_RecvPacket.Position = 0;

							short packetId = BitConverter.ToInt16( m_RecvPacket.GetBuffer(), 2 );

							if( packetId == PacketCoreID.DisconnectPacketId )
							{
								if( m_Socket != null )
								{
									try
									{
										m_Socket.Shutdown( SocketShutdown.Both );
									}
									catch( SocketException ex )
									{
										Log.WriteWarning( "Socket:{0} Shutdown(Disconnected) Exception:{1}", strRemoteEndPoint, ex.ToString() );
									}
									finally
									{
										m_Socket.Close();
										m_Socket = null;
									}
								}

								m_RecvPackets.Clear();
								m_RecvPackets.Enqueue( new PacketContainer( packetId, m_RecvPacket, null, packetSize ) );
								++m_RecvCount;
								recvCount++;
								return;
							}

							if( m_Socket != null && m_Socket.Connected == true )
							{
								if( IsInternalStreamPacketId( packetId ) )
								{
									m_RecvPackets.Enqueue( new PacketContainer( packetId, m_RecvPacket, null, packetSize ) );
								}
								else
								{
									if( Verified == false )
										m_RecvWaitVerifyPackets.Enqueue( new PacketWaitInfo( m_RecvPacket, m_RecvCount, packetSize ) );
									else
									{
										PacketContainer packet = mPacketHandlerManager.deserialize_packet( this, m_RecvPacket, m_RecvCount, true );
										if( packet != null )
											m_RecvPackets.Enqueue( packet );
									}
								}

								++m_RecvCount;
								recvCount++;
							}

							m_RecvPacket = null;
							if( m_RecvLeft != -1 && m_RecvLeft > 0 )
								--m_RecvLeft;
						}
						else
						{
							copyLength = m_RecvBufferSize - m_RecvBufferOffset;
							//System.Diagnostics.Debug.Assert(copyLength >= 0);
							m_RecvPacket.Write( m_RecvBuffer, m_RecvBufferOffset, copyLength );
							m_RecvBufferOffset += copyLength;
						}
					}
				}
			}
		}

		//------------------------------------------------------------------------	
		public virtual void handle_output()
		{
			if( m_Socket == null || m_Socket.Connected == false )
				return;

			if( m_SendPackets.Count == 0 && m_SendPacket == null && m_SendBufferOffset == 0 )
				return;

			if( m_SendBuffer == null )
			{
				m_SendBuffer = new byte[SendBufferSize];
				m_SendBufferTemp = new byte[SendBufferSize];
			}

			SocketError se;

			while( m_SendPacket != null || m_SendPackets.Count > 0 || m_SendBufferOffset > 0 )
			{
				if( m_SendPacket == null && m_SendPackets.Count > 0 )
				{
					object sendObject = m_SendPackets.Dequeue();
					bool log_enabled = false;
					string packet_name = "";
					if( sendObject is MemoryStream )
					{
						m_SendPacket = sendObject as MemoryStream;
					}
					else
					{
						PacketSerializerBase i_ps = (PacketSerializerBase)sendObject;
						m_SendPacket = i_ps.serialize_packet( GetPacketFormatterConfig );
						log_enabled = i_ps.log_enabled;
						packet_name = i_ps.GetPacketName();
					}

					if( m_SendEncryptor != null )
						m_SendPacket = m_SendEncryptor.Encrypt( m_SendPacket );

					if( m_SendPacket.Length > UInt16.MaxValue )
					{
						MemoryStream sendPacket = m_SendPacket;
						m_SendPacket = null;
						throw new PacketDataException( (int)eDisconnectErrorCode.PacketOutputError, "[" + sendObject.ToString() + "] is Too Big : " + sendPacket.Length.ToString(), sendObject );
					}

					if( log_enabled )
					{
						Log.Write( "[{0}] Send to {1} : {2}[PacketSize]:{3} byte", SessionName, strRemoteEndPoint, packet_name, m_SendPacket.Length );
					}

#if BINARY_LOG
                    Log.Write("Send to [{0}], sendIndex{1} : {2}", strRemoteEndPoint, m_SendCount, BitConverter.ToString(m_SendPacket.GetBuffer(), 0, (int)m_SendPacket.Length));
#endif
					m_SendCount++;
					sendCount++;
				}

				if( m_SendPacket != null )
				{
					int copyLength = (int)Math.Min( m_SendPacket.Length - m_SendPacketOffset, m_SendBuffer.Length - m_SendBufferOffset );
					Array.Copy( m_SendPacket.GetBuffer(), m_SendPacketOffset, m_SendBuffer, m_SendBufferOffset, copyLength );
					m_SendPacketOffset += copyLength;
					m_SendBufferOffset += copyLength;

					if( m_SendPacketOffset == m_SendPacket.Length )
					{
						m_SendPacketOffset = 0;
						//m_SendPacket.Close();

						if( m_SendEncryptor != null )
							m_SendPacket.Dispose();
						m_SendPacket = null;
					}
				}

				if( m_SendBufferOffset == m_SendBuffer.Length || m_SendPacket == null && m_SendPackets.Count == 0 )
				{
					int sendSize = m_Socket.Send( m_SendBuffer, 0, m_SendBufferOffset, SocketFlags.None, out se );
					m_LastSendTime = DateTime.Now;

					if( sendSize == 0 && se != SocketError.WouldBlock )
					{
						try
						{
							m_Socket.Shutdown( SocketShutdown.Both );
						}
						catch( System.Exception ex )
						{
							Log.WriteWarning( "Socket:{0} Shutdown(send) Exception:{1}", strRemoteEndPoint, ex.ToString() );
						}
						finally
						{
							m_Socket.Close();
							m_Socket = null;
						}
					}

					if( sendSize <= 0 )
						break;

#if BINARY_LOG
                    Log.Write("Send to {0} : {1}", strRemoteEndPoint, BitConverter.ToString(m_SendBuffer, 0, sendSize));
#endif

					sendBytes += sendSize;
					added_sendBytes += sendSize;

					if( sendSize != m_SendBufferOffset )
					{
						Array.Copy( m_SendBuffer, sendSize, m_SendBufferTemp, 0, m_SendBufferOffset - sendSize );
						byte[] temp = m_SendBuffer;
						m_SendBuffer = m_SendBufferTemp;
						m_SendBufferTemp = temp;
						m_SendBufferOffset -= sendSize;
					}
					else
						m_SendBufferOffset = 0;

					if( se == SocketError.WouldBlock )
						break;

				}
			};
			if( bDisconnecting == true && m_Socket != null && m_Socket.Connected == true )
			{
				try
				{
					m_Socket.Shutdown( SocketShutdown.Both );
				}
				catch( System.Exception ex )
				{
					Log.WriteWarning( "Socket:{0} Shutdown(handle_output) Exception:{1}", strRemoteEndPoint, ex.ToString() );
				}
				finally
				{
					m_Socket.Close();
					m_Socket = null;
				}
			}
		}

		//------------------------------------------------------------------------	
		public virtual eSendPacketResult SendPacket<PacketType>( PacketType packet ) where PacketType : class
		{
			if( packet == null )
				throw new Exception( "packet is null" );

			eSendPacketResult send_result = eSendPacketResult.Success;
			if( m_Socket == null || m_Socket.Connected == false || bConnected == false )
				send_result = eSendPacketResult.NotConnected;

			PacketAttribute attr = PACKET<PacketType>.Attr;
			short packetid = attr.GetPacketId( SendPacketIdType, NSendPacketIdType );
			bool log_enabled = false;

			Type send_id_type = SendPacketIdType;
			if( packetid < 0 )
				send_id_type = NSendPacketIdType;

			if( System.Enum.IsDefined( send_id_type, packetid ) == false )
				throw new Exception( string.Format( "packet id:{0} is wrong, sendIndex : {1}", packetid, m_SendHandleCount ) );

			// packet send interrupt
			List<PacketSendInterruptHandlerBase> packet_send_interrupt_list = GetPacketSendInterruptors( packetid );
			if( packet_send_interrupt_list != null )
				packet_send_interrupt_list.ForEach( a => a.SendInterrupt( packet, this ) );

			if( CoreLogType != eCoreLogType.None || attr.LogType == eCoreLogType.Important || attr.LogType == eCoreLogType.Always )
			{
				log_enabled = true;
				if( CoreLogType == eCoreLogType.Important || attr.LogType == eCoreLogType.Important )
				{
					Log.WriteImportant( "[{0}] Send to {1} : {2}, sendIndex : {3}", SessionName, strRemoteEndPoint, PacketLogFormatter.Instance.Serialize<PacketType>( packet ), m_SendHandleCount );
				}
				else if( CoreLogType == eCoreLogType.NameOnly && attr.LogType != eCoreLogType.Always )
				{
					Log.Write( "[{0}] Send to {1} : {2}, sendIndex : {3}", SessionName, strRemoteEndPoint, packet.ToString(), m_SendHandleCount );
				}
				else
				{
					Log.Write( "[{0}] Send to {1} : {2}, sendIndex : {3}", SessionName, strRemoteEndPoint, PacketLogFormatter.Instance.Serialize<PacketType>( packet ), m_SendHandleCount );
				}
			}

			++m_SendHandleCount;
			if( Verified == false )
			{
				m_SendWaitVerifyPackets.Enqueue( new PacketSerializer<PacketType>( packet, log_enabled ) );
				Log.Write( "----- Into SendWaitVerify Packets" );
			}
			else if( send_result == eSendPacketResult.NotConnected )
			{
				m_SendWaitPackets.Enqueue( new PacketSerializer<PacketType>( packet, log_enabled ) );
				Log.Write( "----- Into SendWait Packets" );
			}
			else
			{
				m_SendPackets.Enqueue( new PacketSerializer<PacketType>( packet, log_enabled ) );
			}

			return send_result;
		}

		//------------------------------------------------------------------------	
		public bool SendStream( MemoryStream stream )
		{
			if( m_Socket == null || m_Socket.Connected == false )
				return false;

			m_SendPackets.Enqueue( stream );
			++m_SendHandleCount;

			return true;
		}

		//------------------------------------------------------------------------
		public void SendWaitStream(MemoryStream stream)
		{
			m_SendWaitPackets.Enqueue( stream );
		}

		//------------------------------------------------------------------------	
		protected virtual void handle_packet( PacketContainer packet )
		{
			if( packet.packet is MemoryStream )
				throw new Exception( "packet shouldn't be MemoryStream" );
			else
				mPacketHandlerManager.handle_packet( this, packet );
		}

		//------------------------------------------------------------------------	
		public bool PauseHandlePackets { get; set; }

		//------------------------------------------------------------------------	
		protected virtual bool check_delay_handle( object packet ) { return false; }

		//------------------------------------------------------------------------	
		public virtual void handle_packets()
		{
			if( bDisconnecting == true )
				return;

			Queue<PacketContainer> recvPackets = new Queue<PacketContainer>();

			while( m_RecvPackets.Count > 0 )
			{
				PacketContainer recvPacket = m_RecvPackets.Dequeue();

				if( PauseHandlePackets == true && !( recvPacket.packet is MemoryStream ) )
				{
					recvPackets.Enqueue( recvPacket );
					continue;
				}
#if !IGNORE_PACKET_HANDLER_CATCH
				try
				{
#endif
					if( check_delay_handle( recvPacket ) == false )
					{
						handle_packet( recvPacket );

						++m_RecvHandleCount;
					}
					else
						recvPackets.Enqueue( recvPacket );

					++m_RecvHandleCount;
#if !IGNORE_PACKET_HANDLER_CATCH
				}
				catch( PacketException ex )
				{
					string packet_log = "";
					if( Log.g_UserDisconnectWithPacketLog && ( recvPacket.packet is MemoryStream ) == false )
						packet_log = " [P]" + PacketLogFormatter.Instance.SerializeDirect( recvPacket.packet );
					Disconnect( ex.ErrorCode, ex.Message, ex.ToString() + packet_log );
					return;
				}
				catch( System.Exception ex )
				{
					Disconnect( (int)eDisconnectErrorCode.PacketHandleError, ex.Message, ex.ToString() );
					return;
				}
#endif
			}
			m_RecvPackets = recvPackets;
		}

		//------------------------------------------------------------------------	
		public bool Disconnect( int error_code, string error_string )
		{
			return Disconnect( error_code, error_string, error_string );
		}

		//------------------------------------------------------------------------	
		public virtual bool Disconnect( int error_code, string error_string, string error_detail_string )
		{
			if( m_Socket == null || m_Socket.Connected == false )
				return false;

			m_DisconnectType = eDisconnectType.Disconnect;

			m_Disconnect.error_code = error_code;
			m_Disconnect.error_string = error_string;
			m_Disconnect.error_detail_string = error_detail_string;

			m_RecvPackets.Clear();
			m_RecvPacket = null;

			m_SendPackets.Clear();

			bDisconnecting = true;

			if( IsUseErrorDetail == false && m_Disconnect.error_code >= (int)eDisconnectErrorCode.Err_CustomBegin )
			{
				PacketDisconnect disconnect = new PacketDisconnect();
				disconnect.error_code = m_Disconnect.error_code;
				//disconnect.error_string = m_Disconnect.error_string;
				disconnect.error_string = "CODE";
				disconnect.error_detail_string = "";
				SendStream( PacketWriteFormatter.Instance.Serialize( disconnect, GetPacketFormatterConfig ) );
			}
			else
				SendStream( PacketWriteFormatter.Instance.Serialize( m_Disconnect, GetPacketFormatterConfig ) );

			return true;
		}

		//------------------------------------------------------------------------	
		protected void CheckPing()
		{
			if( m_bEnablePingCheck == false && m_DisablePingCheckTimeout > 0f )
			{
				if( m_DisablePingCheckExpireTime < DateTime.Now )
				{
					SetEnablePingCheck( true );
				}
			}

			if( m_Socket != null && m_Socket.Connected == true && bDisconnecting == false && PingPacket != null && m_bEnablePingCheck == true )
			{
				if( DateTime.Now - m_LastRecvTime > PingCheckTime )
				{
					Disconnect( (int)eDisconnectErrorCode.PingCheckError, "Ping Check Failed" );
					return;
				}
				if( DateTime.Now - m_LastSendTime > PingTime )
				{
					SendStream( PingPacket );
				}
			}
		}

		//------------------------------------------------------------------------	
		public static MemoryStream MakePingPacket()
		{
			MemoryStream _PingPacket = new MemoryStream();
			_PingPacket.Write( BitConverter.GetBytes( (Int16)0 ), 0, 2 );
			return _PingPacket;
		}

		//------------------------------------------------------------------------	
		public static bool IsDisconnectedFromNetworkIssue( int disconnected_code )
		{
			if( disconnected_code == (int)eDisconnectErrorCode.PingCheckError ||
				disconnected_code == (int)eDisconnectErrorCode.HeartbeatError ||
				disconnected_code == (int)eDisconnectErrorCode.UnknownError )
				return true;

			return false;
		}
	}
}