//////////////////////////////////////////////////////////////////////////
//
// D2M_PacketHandlerManager
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

using System;
using UMF.Net;

namespace UMP.Server.Master
{
	//------------------------------------------------------------------------
	public class D2M_PacketHandlerManagerStandard : D2M_PacketHandlerManager<DaemonMasterPeer>
	{
		public D2M_PacketHandlerManagerStandard( MasterServerApplication application )
			: this( application, typeof( NPID_D2M ) )
		{
		}

		public D2M_PacketHandlerManagerStandard( MasterServerApplication application, Type packet_id_type )
			: base( application, packet_id_type )
		{

		}
	}

	//------------------------------------------------------------------------	
	public class D2M_PacketHandlerManager<ST> : AppPacketHandlerManager<ST> where ST : DaemonMasterPeer
	{
		protected MasterServerApplication mApplication = null;
		public MasterServerApplication Application
		{
			get { return mApplication; }
		}

		public D2M_PacketHandlerManager( MasterServerApplication application, Type packet_id_type )
			: base( application, packet_id_type, typeof( NPID_D2M ) )
		{
			mApplication = application;
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( ND2M_DaemonStatup ) )]
		protected virtual void ND2M_DaemonStatupHandler( ST session, object _packet )
		{
			ND2M_DaemonStatup packet = _packet as ND2M_DaemonStatup;

			session.SetDaemonStartup( packet );
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( ND2M_ProcessCheckAck ) )]
		protected virtual void ND2M_ProcessCheckAckHandler( ST session, object _packet )
		{
			ND2M_ProcessCheckAck packet = _packet as ND2M_ProcessCheckAck;

			mApplication.CommandTool.SendResponseMessage( packet.reuslt );
		}

		//------------------------------------------------------------------------
		[PacketHandler( PacketType = typeof( ND2M_ProcessDumpAck ) )]
		protected virtual void ND2M_ProcessDumpAckHandler( ST session, object _packet )
		{
			ND2M_ProcessDumpAck packet = _packet as ND2M_ProcessDumpAck;

			string dmp_name = packet.result;
			if( packet.dump_file_names != null )
			{
				foreach( string dmp in packet.dump_file_names )
					dmp_name += string.Format( "[{0}]", dmp );
			}

			mApplication.CommandTool.SendResponseMessage( dmp_name );

		}
	}
}
