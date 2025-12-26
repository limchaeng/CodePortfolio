//////////////////////////////////////////////////////////////////////////
//
// BattleAction
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// - 서버에서 받은 전투 패킷으로 클라이언트에서의 행동처리를 위한 액션핸들러 기본 클래스
//////////////////////////////////////////////////////////////////////////
#if UNITY_2019_1_OR_NEWER
#define CLIENT
#endif

using System.Collections;
using System.Collections.Generic;
using System.IO;
using MNS;

//------------------------------------------------------------------------
// DO NOT CHANGE
public enum eBattleActionType : short
{
	None			= 0,
	BattleBegin,
	TurnChange,
	// ....
	BattleFinish,

	// for DEV
	DEV_AISimulation,
}


public delegate void delegateBattleAction_IN<T>( T action );

//------------------------------------------------------------------------
public interface IBattleActionHandler
{
	void HandleAction( BattleActionBase action );
	string HandleLog( BattleActionBase action );

#if CLIENT
	void HandleActionStream( MemoryStream action_stream );	
	string HandleLogStream( MemoryStream action_stream );
#endif
}

//------------------------------------------------------------------------
public class BattleActionHandler_IN<T> : IBattleActionHandler where T : BattleActionBase
{
	delegateBattleAction_IN<T> handler;	
	public BattleActionHandler_IN( delegateBattleAction_IN<T> handler )
	{
		this.handler = handler;
	}
	public void HandleAction( BattleActionBase action )
	{
		handler( (T)action );
	}
	public string HandleLog( BattleActionBase action )
	{
		if( action.LogEnable == false )
			return string.Format( "[{0}][LOG DISABLE]", action.ActionType );
		return BattleActionBase.LogDeserialize<T>( action );
	}
#if CLIENT
	public void HandleActionStream( MemoryStream action_stream )
	{
		// NOT SUPPORT
	}
	public string HandleLogStream( MemoryStream action_stream )
	{
		return "";
	}
#endif
}

//------------------------------------------------------------------------
#if CLIENT
public delegate IEnumerator delegateBattleActionClientProcess<T>( T action, IBattleActionHandler battle_handler );
public delegate void delegateBattleActionClientQueue( IEnumerator process );
public class BattleActionClientHandler<T> : IBattleActionHandler where T : BattleActionBase
{
	delegateBattleActionClientQueue queue_handler;
	delegateBattleActionClientProcess<T> process_handler;
	PacketFormatterConfig packet_config;
	IBattleActionHandler battle_action_in_handler;

	public BattleActionClientHandler( delegateBattleActionClientQueue queue_handler, delegateBattleActionClientProcess<T> ui_process_handler, delegateBattleAction_IN<T> in_battle_handler, PacketFormatterConfig packet_config )
	{
		this.queue_handler = queue_handler;
		this.process_handler = ui_process_handler;
		this.battle_action_in_handler = new BattleActionHandler_IN<T>( in_battle_handler );

		this.packet_config = packet_config;
	}

	public void HandleAction( BattleActionBase action )
	{
		queue_handler( process_handler( (T)action, battle_action_in_handler ) );
	}

	public void HandleActionStream( MemoryStream action_stream )
	{
		T action = BattleActionBase.Deserialize<T>( action_stream, packet_config );
		HandleAction( action );
	}
	public string HandleLog(BattleActionBase action)
	{
		if( action.LogEnable == false )
			return string.Format( "[{0}][LOG DISABLE]", action.ActionType );
		return BattleActionBase.LogDeserialize<T>( action );
	}
	public string HandleLogStream( MemoryStream action_stream )
	{
		T action = BattleActionBase.Deserialize<T>( action_stream, packet_config );
		return HandleLog( action );
	}
}
#endif

//------------------------------------------------------------------------
public delegate void delegateBattleActionServerProcess<T>( T action, IBattleActionHandler battle_handler );
public class BattleActionServerHandler<T> : IBattleActionHandler where T : BattleActionBase
{
	delegateBattleActionServerProcess<T> process_handler;
	IBattleActionHandler battle_action_in_handler;

	public BattleActionServerHandler(delegateBattleActionServerProcess<T> process_handler, delegateBattleAction_IN<T> in_handler)
	{
		this.process_handler = process_handler;
		
		if( in_handler != null )
			this.battle_action_in_handler = new BattleActionHandler_IN<T>( in_handler );
		else
			this.battle_action_in_handler = null;
	}

	public void HandleAction( BattleActionBase action )
	{
		if( process_handler != null )
			process_handler( (T)action, battle_action_in_handler );
	}
	public string HandleLog( BattleActionBase action )
	{
		if( action.LogEnable == false )
			return string.Format( "[{0}][LOG DISABLE]", action.ActionType );

		return BattleActionBase.LogDeserialize<T>( action );
	}

#if CLIENT
	public void HandleActionStream( MemoryStream action_stream )
	{
	}
	public string HandleLogStream( MemoryStream action_stream )
	{
		return "";
	}
#endif
}

//------------------------------------------------------------------------
public abstract class BattleActionBase
{
	//------------------------------------------------------------------------	
	public static T MakeBattleAction<T>( byte owner_battle_idx ) where T : BattleActionBase, new()
	{
		T action = new T();
		action.owner_battle_idx = owner_battle_idx;

		return action;
	}
	public static MemoryStream Serialize<T>( T action, PacketFormatterConfig config ) where T : BattleActionBase
	{
		return PacketWriteFormatter.Instance.SerializeStream<T>( action, config );
	}
	public static T Deserialize<T>(MemoryStream stream, PacketFormatterConfig config ) where T : BattleActionBase
	{
		return PacketReadFormatter.Instance.Serialize<T>( new BinaryReader( stream ), config );
	}
	public static string LogDeserialize<T>(object packet, bool include_name=true) where T : BattleActionBase
	{
		return PacketLogFormatter.Instance.Serialize<T>( packet, include_name );
	}

	//------------------------------------------------------------------------	
	public byte owner_battle_idx;

	public abstract eBattleActionType ActionType { get; }

	protected virtual string GetDebugString() { return ""; }

	public virtual bool LogEnable { get { return true; } }

	public override string ToString()
	{
		return string.Format( "[o:{0}][t:{1}]{2}", owner_battle_idx, ActionType, GetDebugString() );
	}
}

//------------------------------------------------------------------------
public class BA_BattleBegin : BattleActionBase
{
	public override eBattleActionType ActionType { get { return eBattleActionType.BattleBegin; } }

	public class BlockData
	{
		public byte owner_battle_idx;
		public List<BAD_BlockData> block_list;
	}	

	public List<BlockData> block_list;
}

//------------------------------------------------------------------------
public class BA_TurnChange : BattleActionBase
{
	public override eBattleActionType ActionType { get { return eBattleActionType.TurnChange; } }

	public int turn_count;
	[PacketValue( Version = 6 )]
	public int player_turn_count;
	[PacketValue( Version = 6 )]
	public int player_turn_move_count;
	[PacketValue(Type = PacketValueType.SerializeNullable)]
	public List<BAD_BlockData> checked_auto_rematch_blocks;
	[PacketValue( Type = PacketValueType.SerializeNullable )]
	public BAD_SkillProcessData skill_process_data;
	[PacketValue( Type = PacketValueType.SerializeNullable )]
	public BAD_UpdateBlockContainer update_block_container;
	[PacketValue( Version = 6, Type = PacketValueType.SerializeNullable )]
	public List<BAD_BlockData> turn_change_blocks;
}

