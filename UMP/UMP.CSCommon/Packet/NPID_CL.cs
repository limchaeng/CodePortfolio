//////////////////////////////////////////////////////////////////////////
//
// PID_CL
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
using UMF.Net;

namespace UMP.CSCommon.Packet
{
	//------------------------------------------------------------------------	
	[PacketVersion( Version = NPacketVersion.Version )]
	public enum NPID_C2L : short
	{
		__BEGIN = short.MinValue + 1,
		//
		Login,
	}

	//------------------------------------------------------------------------
	[PacketVersion( Version = NPacketVersion.Version )]
	public enum NPID_L2C : short
	{
		__BEGIN = short.MinValue + 1,
		//
		LoginAck,
	}

	//////////////////////////////////////////////////////////////////////////
	///
	//------------------------------------------------------------------------	
	public class NL_FastConnectionData
	{
		public long connection_key;
		public string host_name;
		public int port;
	}

	[Packet( NPID_C2L.Login )]
	public class NC2L_Login : ExpandPacketBase
	{
		public int world_idn;
		public long gameserver_guid;
		public string curr_localize;
		public string device_package_id;
	}
	public class NP_GameServerConnectionData
	{
		public long connection_key;
		public string gameserver_host_name;
		public int gameserver_port;
	}

	[Packet( NPID_L2C.LoginAck )]
	public class NL2C_LoginAck : ExpandPacketBase
	{
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public NL_FastConnectionData fast_connection_data;
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public NP_GameServerConnectionData gameserver_connection_data;		
	}
}
