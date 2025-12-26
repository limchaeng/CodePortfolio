//////////////////////////////////////////////////////////////////////////
//
// Peer
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
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using UMF.Core;
using UMF.Net.Module;
using System.Collections.Generic;

namespace UMF.Net
{
	//------------------------------------------------------------------------	
    public class Peer : Session
    {
        public short protocol_version = -1;

        protected PeerManagerBase mPeerManager = null;
		public PeerManagerBase GetPeerManager { get { return mPeerManager; } }
		protected bool bVerified = false;
        public override bool Verified
        {
            get
            {
                return bVerified;
            }
        }
		protected override short VerifyPacketId => PacketVerifyID.VerifyPacketId;
		protected override string VerifyPacketLog( object packet )
		{
			return PacketLogFormatter.Instance.Serialize<PacketVerify>( packet, false );
		}

		protected override short VerifiedAckPacketId => PacketVerifyID.VerifiedPacketId;

		protected override int SendBufferSize { get { return mPeerManager.GetListenerConfig.SendBufferSize; } }
        protected override int RecvBufferSize { get { return mPeerManager.GetListenerConfig.RecvBufferSize; } }

		public override Type SendPacketIdType { get { return mPeerManager.SendPacketIdType; } }
		public override Type NSendPacketIdType { get { return mPeerManager.NSendPacketIdType; } }
		public override Type RecvPacketIdType { get { return mPeerManager.RecvPacketIdType; } }
		public override Type NRecvPacketIdType { get { return mPeerManager.NRecvPacketIdType; } }

		protected string VerifyString { get { return mPeerManager.GetListenerConfig.VerifyString; } }
        public override eCoreLogType CoreLogType { get { return mPeerManager.GetListenerConfig.CoreLogType; } }

		protected int mPeerIndex = 0;
        public int PeerIndex { get { return mPeerIndex; } set { mPeerIndex = value; } }

        private long m_HeartbeatRandomKey = DateTime.Now.ToBinary();
        private long m_HeartbeatKey = 0;
        private bool m_bHeartbeatFirst = true;
        private Stopwatch m_HeartbeatWatch;
        private TimeSpan m_HeartbeatTime;

        protected virtual PacketVerify VerifyPacket(short packetId, MemoryStream stream)
        {
            if (packetId != PacketVerifyID.VerifyPacketId)
                throw new Exception("Not correct verify packet : " + packetId.ToString());

            return PacketReadFormatter.Instance.Serialize<PacketVerify>(new BinaryReader(stream), GetPacketFormatterConfig);
        }

        protected override MemoryStream PingPacket { get { return mPeerManager.PingPacket; } }
        protected override TimeSpan PingTime { get { return mPeerManager.GetListenerConfig.PingTimespan; } }
        protected override TimeSpan PingCheckTime { get { if (m_TempPingCheckTime != null) return m_TempPingCheckTime.Value; return mPeerManager.GetListenerConfig.PingCheckTimespan; } }

        PacketFormatterConfig m_PacketFormatterConfig = null;
        public override PacketFormatterConfig GetPacketFormatterConfig
        {
            get
            {
                if (m_PacketFormatterConfig == null)
                {
                    m_PacketFormatterConfig = new PacketFormatterConfig();
                    m_PacketFormatterConfig.UseConvertDatetime = mPeerManager.GetListenerConfig.UseConvertDatetime;
					m_PacketFormatterConfig.IgnoreTimeZone = mPeerManager.GetListenerConfig.IgnoreTimeZone;
                    m_PacketFormatterConfig.protocol_version = protocol_version;
                }
                return m_PacketFormatterConfig;
            }
        }

        public Peer()
        {
        }

		//------------------------------------------------------------------------	
		public virtual void Init(PeerManagerBase peerManager, Socket socket)
        {
            bConnected = true;
            mPeerManager = peerManager;
            mPacketHandlerManager = mPeerManager.PacketHandlerManager;
            m_HeartbeatWatch = new Stopwatch();

			mSessionName = $"{ GetType().Name}:{mPeerManager.ServiceTypeString}";

			m_Socket = socket;
            m_Socket.NoDelay = true;
            m_Socket.SendBufferSize = SendBufferSize;
            m_Socket.ReceiveBufferSize = RecvBufferSize;
            //m_Socket.LingerState = new LingerOption(true, 1);
            strRemoteEndPoint = m_Socket.RemoteEndPoint.ToString();

            if ( mPeerManager.GetListenerConfig.HeartbeatFirstTimespan != TimeSpan.FromSeconds(0))
            {
                m_HeartbeatWatch.Start();
                m_HeartbeatTime = mPeerManager.GetListenerConfig.HeartbeatFirstTimespan;
                while (m_HeartbeatKey == 0)
                    m_HeartbeatKey = m_HeartbeatRandomKey = Encryptor.NextValue(m_HeartbeatRandomKey);
                if ( mPeerManager.GetListenerConfig.UseEncrypt == true)
                {
                    m_RecvEncryptor = new Encryptor(m_HeartbeatKey);
                    m_SendEncryptor = new Encryptor(0);
                    m_PacketPositionCheck = 4;
                }
                SendHeartbeat();
            }
        }

		//------------------------------------------------------------------------	
		public override void handle_output()
        {
            if (m_Socket != null && m_Socket.Connected == true && bDisconnecting == false)
            {
                if (m_HeartbeatWatch.IsRunning == true && m_HeartbeatWatch.Elapsed > m_HeartbeatTime)
                {
                    if (m_HeartbeatKey != 0)
                    {
                        if (PingPacket == null)
                        {
                            if (m_bHeartbeatFirst == true)
                                Disconnect((int)eDisconnectErrorCode.HeartbeatFirstError, "HeartbeatFirst error");
                            else
                                Disconnect((int)eDisconnectErrorCode.HeartbeatError, "Heartbeat error");
                        }
                        return;
                    }
                    m_HeartbeatWatch.Restart();
                    while (m_HeartbeatKey == 0)
                        m_HeartbeatKey = m_HeartbeatRandomKey = Encryptor.NextValue(m_HeartbeatRandomKey);
                    SendHeartbeat();
                }
            }

            base.handle_output();
        }

		//------------------------------------------------------------------------	
		void SendHeartbeat()
        {
            PacketHeartbeat _PacketHeartbeat = new PacketHeartbeat();
            _PacketHeartbeat.heartbeat_key = m_HeartbeatKey;
            SendStream(PacketWriteFormatter.Instance.Serialize(_PacketHeartbeat, GetPacketFormatterConfig));

			if( mPeerManager.GetListenerConfig.IsHeartbeatLog )
				Log.WriteImportant( PacketLogFormatter.Instance.Serialize<PacketHeartbeat>( _PacketHeartbeat ) );
        }

		//------------------------------------------------------------------------	
		protected override void OnDisconnected()
        {
			if( (m_Disconnect.error_code != (int)eDisconnectErrorCode.ClientQuit && m_Disconnect.error_code != (int)eDisconnectErrorCode.Normal) ||
				string.IsNullOrEmpty(m_Disconnect.error_string) == false || string.IsNullOrEmpty(m_Disconnect.error_detail_string) )
			{
				if( m_Disconnect.error_string == m_Disconnect.error_detail_string )
					Log.WriteWarning( string.Format( "[{0}] OnDisconnected({1},{2}) {3}, {4}, {5}", SessionName, strRemoteEndPoint, mPeerIndex, m_DisconnectType, m_Disconnect.ErrorCodeString, m_Disconnect.error_string ) );
				else
					Log.WriteWarning( string.Format( "[{0}] OnDisconnected({1},{2}) {3}, {4}, {5}, {6}", SessionName, strRemoteEndPoint, mPeerIndex, m_DisconnectType, m_Disconnect.ErrorCodeString, m_Disconnect.error_string, m_Disconnect.error_detail_string ) );
			}
			else
			{
				if( m_Disconnect.error_string == m_Disconnect.error_detail_string )
					Log.Write( string.Format( "[{0}] OnDisconnected({1},{2}) {3}, {4}, {5}", SessionName, strRemoteEndPoint, mPeerIndex, m_DisconnectType, m_Disconnect.ErrorCodeString, m_Disconnect.error_string ) );
				else
					Log.Write( string.Format( "[{0}] OnDisconnected({1},{2}) {3}, {4}, {5}, {6}", SessionName, strRemoteEndPoint, mPeerIndex, m_DisconnectType, m_Disconnect.ErrorCodeString, m_Disconnect.error_string, m_Disconnect.error_detail_string ) );
			}

            mPeerManager.RemovePeer(this);
            base.OnDisconnected();
        }

		//------------------------------------------------------------------------	
		public void CheckDisconnected()
        {
            if (bConnected == true && (m_Socket == null || m_Socket.Connected == false))
            {
                bConnected = false;
                OnDisconnected();
            }

            if (Verified == true)
            {
                CheckPing();
            }
        }

		//------------------------------------------------------------------------	
		public override bool Disconnect(int error_code, string error_string, string error_detail_string)
        {
            if (base.Disconnect(error_code, error_string, error_detail_string) == false)
                return false;

			handle_output();
            Close();

            return true;
        }

		//------------------------------------------------------------------------	
		public override void handle_input()
        {
            base.handle_input();
        }

		//------------------------------------------------------------------------	
		protected override void handle_packet(PacketContainer packet)
        {
            if (packet.packet is MemoryStream)
            {
                MemoryStream stream = packet.packet as MemoryStream;
                Int16 packetId = BitConverter.ToInt16(stream.GetBuffer(), 2);

                if (packetId == PacketCoreID.DisconnectPacketId)
                {
                    stream.Seek(4, SeekOrigin.Begin);
                    PacketDisconnect disconnect = PacketReadFormatter.Instance.Serialize<PacketDisconnect>(new BinaryReader(stream), GetPacketFormatterConfig);
                    SetDisconnectInfo(disconnect);

					return;
                }
                else if (bVerified == false)
                {
                    stream.Seek(4, SeekOrigin.Begin);
                    PacketVerify verify = VerifyPacket(packetId, stream);

                    OnVerified(Verify(verify), verify);

                    return;
                }
                else if (packetId == PacketCoreID.HeartbeatAckId)
                {
                    stream.Seek(4, SeekOrigin.Begin);

                    PacketHeartbeatAck heartbeatAck = PacketReadFormatter.Instance.Serialize<PacketHeartbeatAck>(new BinaryReader(stream), GetPacketFormatterConfig);

					if( mPeerManager.GetListenerConfig.IsHeartbeatLog )
						Log.WriteImportant( PacketLogFormatter.Instance.Serialize<PacketHeartbeatAck>( heartbeatAck ) );

					long next_heartbeat = Encryptor.NextValue( m_HeartbeatKey );
					if( heartbeatAck.heartbeat_key == next_heartbeat )
					{
						m_HeartbeatKey = 0;

						if( m_bHeartbeatFirst == true )
						{
							m_HeartbeatTime = mPeerManager.GetListenerConfig.HeartbeatTimespan;
							if( m_HeartbeatTime == TimeSpan.FromSeconds( 0 ) )
								m_HeartbeatWatch.Stop();
							m_bHeartbeatFirst = false;
						}
						else if( mPeerManager.GetListenerConfig.IsHeartbeatLog == true )
							Log.Write( "[{0}] Heartbeat : {1}", strRemoteEndPoint, m_HeartbeatWatch.ElapsedMilliseconds );
					}
					else
					{
						throw new PacketException( m_bHeartbeatFirst == true ? (int)eDisconnectErrorCode.HeartbeatFirstError : (int)eDisconnectErrorCode.HeartbeatError,
							string.Format( "heartbeat key was wrong ack:{0} <> next:{1}", heartbeatAck.heartbeat_key, next_heartbeat ) );
					}
                    return;
                }
				else if( packetId == PacketCoreID.ModulePacket )
				{
					stream.Seek( 4, SeekOrigin.Begin );
					mPeerManager.OnModulePacketHandler( this, PacketReadFormatter.Instance.Serialize<PacketModule>( new BinaryReader( stream ), GetPacketFormatterConfig ) );
					return;
				}
                throw new PacketException((int)eDisconnectErrorCode.PacketHandleError, "wrong packet in handle_packet : " + packetId.ToString());
            }
            else
            {
                if (bVerified == false)
                {
					throw new PacketException( (int)eDisconnectErrorCode.VerifyError, "Not verify packet : " + packet.attr.GetPacketId( RecvPacketIdType, RecvPacketIdType ) );
                }

                base.handle_packet(packet);
            }
        }

		//------------------------------------------------------------------------		
        protected virtual void OnVerified(object userInfo, PacketVerify verify)
        {
            bVerified = true;
            m_RecvLeft = mPeerManager.GetListenerConfig.RecvLimit;

			Log.Write( "[{0}] Verified Peer({1}) : peerIndex({2}), {3}", SessionName, strRemoteEndPoint, mPeerIndex, VerifyPacketLog( verify ) );

            PacketVerified _PacketVerified = new PacketVerified();
            _PacketVerified.protocol_version = Math.Min(protocol_version, PacketVersionAttribute.GetVersion(mPeerManager.PacketHandlerManager.PacketIdType));
			SendStream(PacketWriteFormatter.Instance.Serialize(_PacketVerified, GetPacketFormatterConfig));
        }

		//------------------------------------------------------------------------		
		protected virtual object Verify(PacketVerify verify)
        {
            if (VerifyString + "." + RecvPacketIdType.ToString() != verify.verify_string)
                throw new PacketException((int)eDisconnectErrorCode.VerifyError, string.Format( "[{0}] Verified failed({1}) : C:{2} != P:{3}", SessionName, 
					strRemoteEndPoint, verify.verify_string, 
					VerifyString + "." + RecvPacketIdType.ToString()));

            if (mPeerManager.GetListenerConfig.UseProtocolVersion == true)
            {
                protocol_version = Math.Min(verify.protocol_version, PacketVersionAttribute.GetVersion(mPeerManager.PacketHandlerManager.PacketIdType));
                if (m_PacketFormatterConfig != null)
                    m_PacketFormatterConfig.protocol_version = protocol_version;
				ProcessWaitVerifyPackets();
            }
            else
            {
                protocol_version = -1;
                if (m_PacketFormatterConfig != null)
                    m_PacketFormatterConfig.protocol_version = -1;
            }

            return mPeerManager.VerifyPeer(this, verify);
        }

		//------------------------------------------------------------------------		
		public override eSendPacketResult SendPacket<PacketType>( PacketType packet )
        {
            PacketAttribute attr = PACKET<PacketType>.Attr;
			if( attr.IsVersion( m_PacketFormatterConfig.protocol_version ) == false )
				return eSendPacketResult.VersionLower;

			return base.SendPacket( packet );
        }

		//------------------------------------------------------------------------
		protected override List<PacketSendInterruptHandlerBase> GetPacketSendInterruptors( short packet_id )
		{
			return mPeerManager.GetPacketSendInterruptors( packet_id );
		}
	}
}