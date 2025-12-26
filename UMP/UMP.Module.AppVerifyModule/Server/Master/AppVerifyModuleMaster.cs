//////////////////////////////////////////////////////////////////////////
//
// AppVerifyModuleMasterModule
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

using System;
using UMF.Net;
using UMF.Net.Module;
using System.Collections.Generic;
using UMP.Server;
using UMP.Server.Master;
using UMP.CSCommon;

namespace UMP.Module.AppVerifyModule.Master
{
	//------------------------------------------------------------------------
	public class AppVerifyModuleMasterCommon
	{
		public const string MODULE_NAME = "UMP.Module.AppVerifyModule.Master";
		public const short ProtocolVersion = 1;

		//------------------------------------------------------------------------
		[PacketVersion( Version = ProtocolVersion )]
		public enum NPID_S2M : short
		{
			__BEGIN = short.MinValue + 1,
			//
			NotifyAppVerifyData,
		}

		//------------------------------------------------------------------------
		[PacketVersion( Version = ProtocolVersion )]
		public enum NPID_M2S : short
		{
			__BEGIN = short.MinValue + 1,
			//
			NotifyAppVerifyData
		}
	}

	//------------------------------------------------------------------------		
	public class P_NotifyAppVerifyData
	{
		public int type_flag;
		public List<string> verify_list;
	}

	//------------------------------------------------------------------------		
	[Packet( AppVerifyModuleMasterCommon.NPID_S2M.NotifyAppVerifyData )]
	public class S2M_NotifyAppVerifyData
	{
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public List<P_NotifyAppVerifyData> verify_data_list;
	}

	//------------------------------------------------------------------------		
	[Packet( AppVerifyModuleMasterCommon.NPID_M2S.NotifyAppVerifyData )]
	public class M2S_NotifyAppVerifyData : MasterCommandBase
	{
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public List<P_NotifyAppVerifyData> verify_data_list;
	}

	//------------------------------------------------------------------------	
	public class AppVerifyModuleMasterPeer : ModuleNetPeer
	{
		public override string ModuleName => AppVerifyModuleMasterCommon.MODULE_NAME;
		protected override short ProtocolVersion => AppVerifyModuleMasterCommon.ProtocolVersion;

		public override Type SendPacketIdType => typeof( AppVerifyModuleMasterCommon.NPID_M2S );

		public override Type NSendPacketIdType => typeof( AppVerifyModuleMasterCommon.NPID_M2S );

		public override Type RecvPacketIdType => typeof( AppVerifyModuleMasterCommon.NPID_S2M );

		public override Type NRecvPacketIdType => typeof( AppVerifyModuleMasterCommon.NPID_S2M );

		ServerMasterPeerManager mServerMasterPeerManager = null;
		public AppVerifyModuleMasterPeer( ServerMasterPeerManager peer_manager )
			: base( peer_manager )
		{
			mServerMasterPeerManager = peer_manager;
		}

		//------------------------------------------------------------------------		
		[PacketHandler(PacketType = typeof( S2M_NotifyAppVerifyData ) )]
		protected virtual void S2M_NotifyAppVerifyDataHandler( Peer session, object _packet )
		{
			S2M_NotifyAppVerifyData packet = _packet as S2M_NotifyAppVerifyData;

			List<Peer> to_peers = mServerMasterPeerManager.FindPeers( 0, eServerType.Login );
			if( to_peers != null )
			{
				M2S_NotifyAppVerifyData _M2S_NotifyAppVerifyData = new M2S_NotifyAppVerifyData();
				_M2S_NotifyAppVerifyData.verify_data_list = packet.verify_data_list;

				MulticastPacket( to_peers, _M2S_NotifyAppVerifyData );
			}
		}
	}

	//------------------------------------------------------------------------
	public class AppVerifyModuleMasterConnector : ModuleNetConnector
	{

		public override string ModuleName => AppVerifyModuleMasterCommon.MODULE_NAME;

		protected override short ProtocolVersion => AppVerifyModuleMasterCommon.ProtocolVersion;

		public override Type SendPacketIdType => typeof( AppVerifyModuleMasterCommon.NPID_S2M );

		public override Type NSendPacketIdType => typeof( AppVerifyModuleMasterCommon.NPID_S2M );

		public override Type RecvPacketIdType => typeof( AppVerifyModuleMasterCommon.NPID_M2S );

		public override Type NRecvPacketIdType => typeof( AppVerifyModuleMasterCommon.NPID_M2S );

		public class AppVerifyNotifyDataContainer
		{
			public int app_code;
			public List<CS_AppVerifyNotifyData> app_v_data_list;
		}
		List<AppVerifyNotifyDataContainer> mNotifyContainerList = null;

		public AppVerifyModuleMasterConnector( Connector connector ) 
			: base( connector )
		{
		}

		//------------------------------------------------------------------------		
		[PacketHandler(PacketType = typeof( M2S_NotifyAppVerifyData ) )]
		protected virtual void M2S_NotifyAppVerifyDataHandler( Connector session, object _packet )
		{
			M2S_NotifyAppVerifyData packet = _packet as M2S_NotifyAppVerifyData;

			mNotifyContainerList = null;
			if( packet.verify_data_list != null )
			{
				mNotifyContainerList = new List<AppVerifyNotifyDataContainer>();
				foreach( P_NotifyAppVerifyData p_data in packet.verify_data_list )
				{
					CS_AppVerifyNotifyData cs_data = new CS_AppVerifyNotifyData();
					cs_data.app_verify_types = (eAppVerifyRequestTypeFlag)p_data.type_flag;
					cs_data.app_verify_notify_list = p_data.verify_list;

					List<string> used_apps = AppVerifyModuleConfig.Instance.FindAppFromVerifyType( cs_data.app_verify_types );
					if( used_apps != null )
					{
						foreach( string app_id in used_apps )
						{
							int app_code = AppIdentifier.Instance.Get( app_id );
							if( app_code > 0 )
							{
								AppVerifyNotifyDataContainer container = mNotifyContainerList.Find( a => a.app_code == app_code );
								if( container == null )
								{
									container = new AppVerifyNotifyDataContainer();
									container.app_code = app_code;
									container.app_v_data_list = new List<CS_AppVerifyNotifyData>();

									mNotifyContainerList.Add( container );
								}

								container.app_v_data_list.Add( cs_data );
							}
						}
					}
				}
			}
		}

		//------------------------------------------------------------------------		
		public List<CS_AppVerifyNotifyData> GetAppVerifyNotifyList( int app_code )
		{
			if( mNotifyContainerList == null )
				return null;

			AppVerifyNotifyDataContainer container = mNotifyContainerList.Find( a => a.app_code == app_code );
			if( container == null )
				return null;

			return container.app_v_data_list;
		}
	}
}

#endif