//////////////////////////////////////////////////////////////////////////
//
// WorldModuleCommon
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
using System.Collections.Generic;
using UMP.CSCommon.Packet;

namespace UMP.Module.WorldModule
{
	public class WorldModuleCommon
	{
		public const string MODULE_NAME = "UMP.Module.WorldModule";
		public const short ProtocolVersion = 1;

		//------------------------------------------------------------------------
		[PacketVersion( Version = ProtocolVersion )]
		public enum NPID_C2L : short
		{
			__BEGIN = short.MinValue + 1,
			//
			GetWorldList,
		}

		//------------------------------------------------------------------------
		[PacketVersion( Version = ProtocolVersion )]
		public enum NPID_L2C : short
		{
			__BEGIN = short.MinValue + 1,
			//
			GetWorldListAck,
		}
	}

	//------------------------------------------------------------------------
	[System.Flags]
	public enum eWorldDataFlags : int
	{
		None = 0x0000,
		New = 0x0001,
		Recommend = 0x0002,

		Maintenance = 0x0010,

		StatusSmooth = 0x0100,
		StatusNormal = 0x0200,
		StatusBusy = 0x0400,
	}
	public class CS_WorldData
	{
		public int world_idn;
		public string world_name;
		public string status_text;
		public long connectionKey;
		public string host_name;
		public int port;
		public eWorldDataFlags flags;

		public bool HasFlags( eWorldDataFlags flag )
		{
			return ( ( flags & flag ) != 0 );
		}
	}
	public class CS_WorldContainer
	{
		public int auto_refresh_timeout;
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public List<CS_WorldData> world_list;
	}

	//------------------------------------------------------------------------
	[Packet( WorldModuleCommon.NPID_C2L.GetWorldList )]
	public class NC2L_GetWorldList
	{

	}
	[Packet( WorldModuleCommon.NPID_L2C.GetWorldListAck )]
	public class NL2C_GetWorldListAck
	{
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public CS_WorldContainer update_container;
	}
}
