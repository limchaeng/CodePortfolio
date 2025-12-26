//////////////////////////////////////////////////////////////////////////
//
// InterruptPacket
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
using UMF.Core;

namespace UMF.Net
{
	//------------------------------------------------------------------------
	public interface IPacketInterruptor
	{
		Type SendPacketIdType { get; }
		Type NSendPacketIdType { get; }
		Type RecvPacketIdType { get; }
		Type NRecvPacketIdType { get; }
		PacketHandlerManagerBase PacketHandlerManager { get; }
		void AddPacketSendInterruptor<PT>( PacketSendInterruptHandlerBase.DelegatePacketSendInterruptHandler<PT> interruptor ) where PT : class;
		void AddPacketSendInterruptor( Type packet_type, PacketSendInterruptHandlerBase.DelegatePacketSendInterruptObjectHandler interruptor );
	}

	//------------------------------------------------------------------------
	public abstract class PacketInterruptHandlerBase
	{
		public delegate void DelegatePacketInterruptHandler<PT>( Session session, PT packet );
		public delegate void DelegatePacketInterruptObjectHandler( Session session, object packet );
		public abstract void handle_packet_interrupt( Session session, PacketContainer packet_container );
	}

	//------------------------------------------------------------------------
	public class PacketInterruptHandler<PT> : PacketInterruptHandlerBase where PT : class
	{
		DelegatePacketInterruptHandler<PT> interrupt_handler;

		public PacketInterruptHandler( DelegatePacketInterruptHandler<PT> handler )
		{
			this.interrupt_handler = handler;
		}

		//------------------------------------------------------------------------		
		public override void handle_packet_interrupt( Session session, PacketContainer packet_container )
		{
			interrupt_handler( session, (PT)packet_container.packet );
		}
	}

	//------------------------------------------------------------------------
	public class PacketInterruptObjectHandler : PacketInterruptHandlerBase 
	{
		DelegatePacketInterruptObjectHandler interrupt_handler;

		public PacketInterruptObjectHandler( DelegatePacketInterruptObjectHandler handler )
		{
			this.interrupt_handler = handler;
		}

		//------------------------------------------------------------------------		
		public override void handle_packet_interrupt( Session session, PacketContainer packet_container )
		{
			interrupt_handler( session, packet_container.packet );
		}
	}
	//------------------------------------------------------------------------
	public abstract class PacketSendInterruptHandlerBase
	{
		public delegate void DelegatePacketSendInterruptHandler<PT>( PT packet, Session session );
		public delegate void DelegatePacketSendInterruptObjectHandler( object packet, Session session );

		/// <summary>
		///   session is nullable (when broadcast or multicast)
		/// </summary>
		public abstract void SendInterrupt( object packet, Session session );
	}

	//------------------------------------------------------------------------
	public class PacketSendInterruptHandler<PT> : PacketSendInterruptHandlerBase where PT : class
	{
		DelegatePacketSendInterruptHandler<PT> send_interrupt;

		public PacketSendInterruptHandler(DelegatePacketSendInterruptHandler<PT> interruptor)
		{
			this.send_interrupt = interruptor;
		}

		//------------------------------------------------------------------------
		public override void SendInterrupt( object packet, Session session )
		{
			send_interrupt( (PT)packet, session );
		}
	}

	//------------------------------------------------------------------------
	public class PacketSendInterruptObjectHandler : PacketSendInterruptHandlerBase
	{
		DelegatePacketSendInterruptObjectHandler send_interrupt;

		public PacketSendInterruptObjectHandler( DelegatePacketSendInterruptObjectHandler interruptor )
		{
			this.send_interrupt = interruptor;
		}

		//------------------------------------------------------------------------
		public override void SendInterrupt( object packet, Session session )
		{
			send_interrupt( packet, session );
		}
	}

}
