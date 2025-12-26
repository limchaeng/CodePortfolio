//////////////////////////////////////////////////////////////////////////
//
// RelicSystem
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 특정 조건에 의해 발동되는 유물아이템 시스템
//////////////////////////////////////////////////////////////////////////

using DCBattle;
using System.Collections.Generic;
using UMF.Core;

namespace U6Common
{
	//------------------------------------------------------------------------
	public class RelicSystem
	{
		public class ItemContainer
		{
			// ....
		}

		public interface IRelicTarget
		{
			bool RelicTarget_IsOwnerType( eRELIC_OWNER_TYPE target_type );
			bool RelicTarget_IsConditionMatch( TBL_Relic.OwnerInfo.ConditionData cond_data );
			string RelicTarget_GetInfo();
			bool RelicTarget_CardTypeIgnore();
		}

		public interface IRelicProcessor
		{
			bool RelicProcessor_TriggerFunction( RelicSystem system, TriggerActReservedData reserved_data );
			List<ItemContainer> RelicProcess_GetItems();
			void RelicProcess_ItemUpdate( ItemContainer item, eRelicItemUpdateType update_type, int count, bool item_save );
			void RelicProcess_Log( string log );
		}

		public class TriggerActReservedData
		{
			// ....
			public override string ToString()
			{
				string log = $"idn={relic_info.IDN} func:{relic_info.FunctionType} skill={relic_info.Func_ValueList.Count} owner:{owner} target:{target} fire_skill:{fire_relic_skill_idn} wait:{waiting_action_fire_skill_identity}";

				if( fixed_target_list != null )
					log += $" fixed_target:{fixed_target_list.Count}";

				return log;
			}
		}

		List<TBL_Relic> mInfoList = null;
		public List<TBL_Relic> InfoList { get { return mInfoList; } }

		ItemContainer mContainer = null;
		public ItemContainer Container { get { return mContainer; } }

		bool mTriggerResetUpdateOnce = false;
		List<TriggerActReservedData> mTriggerActReservedList = new List<TriggerActReservedData>();
		List<RelicItemSkill> mBattleSkillCached = new List<RelicItemSkill>();

		public RelicSystem( ItemContainer item, List<TBL_Relic> info_list )
		{
			mInfoList = info_list;
			mContainer = item;
		}

		//------------------------------------------------------------------------
		public void BattleDestroy()
		{
			mBattleSkillCached.Clear();
			mTriggerActReservedList.Clear();
		}

		// ....

		//------------------------------------------------------------------------
		public int FunctionTrigger_ValueSum( eRELIC_FUNCTION_TYPE func_type, IRelicTarget owner, ref int sum_value, ref int sum_percent, ref bool item_save, bool check_only )
		{
			if( owner != null && owner.RelicTarget_CardTypeIgnore() )
				return 0;

			if( IsTriggerReady() == false )
				return 0;

			int trigger_count = 0;

			// ....

			return trigger_count;
		}

		//------------------------------------------------------------------------
		public int FunctionTrigger_ValueCalc( eRELIC_FUNCTION_TYPE func_type, IRelicTarget owner, int base_value, ref int add_value, ref bool item_save )
		{
			if( owner != null && owner.RelicTarget_CardTypeIgnore() )
				return 0;

			if( IsTriggerReady() == false )
				return 0;

			int trigger_count = 0;
			int v = 0;

			// ....
			return trigger_count;
		}

		//------------------------------------------------------------------------
		public bool FunctionTrigger_Has( eRELIC_FUNCTION_TYPE func_type, IRelicTarget owner, ref bool item_save, bool check_only )
		{
			if( owner != null && owner.RelicTarget_CardTypeIgnore() )
				return false;

			if( IsTriggerReady() == false )
				return false;

			// ....
			return false;
		}

		//------------------------------------------------------------------------
		public bool FunctionTrigger_State( eRELIC_FUNCTION_TYPE func_type, eCARD_STATE_TYPE state, IRelicTarget owner, ref bool item_save )
		{
			if( owner != null && owner.RelicTarget_CardTypeIgnore() )
				return false;

			if( IsTriggerReady() == false )
				return false;

			// ....

			return false;
		}

		//------------------------------------------------------------------------
		public bool IsTriggerReady()
		{
			return ( mContainer.DataTriggerCounting >= mContainer.InfoTriggerCounting );
		}

		//------------------------------------------------------------------------
		public int CountingCheck( eRELIC_ACT_TYPE act, IRelicTarget owner, IRelicTarget target, bool from_skill_fire, ref bool has_trigger_reserve_check )
		{
			if( owner != null && owner.RelicTarget_CardTypeIgnore() )
				return 0;

			int matched_counting = 0;

			if( IsTriggerReady() == false )
			{
				// ....
			}
			return matched_counting;
		}

		// ....

		//------------------------------------------------------------------------
		List<TriggerActReservedData> tmp_RemoveReservedList = new List<TriggerActReservedData>();
		public void CheckSkillFireTriggerActReservedAction( IRelicProcessor processor, IRelicTarget skillfire_relic_owner, IRelicTarget skillfire_relic_target, eRELIC_ACT_TYPE trigger_act_type, int waiting_action_fire_skill_identity )
		{
			if( IsTriggerReady() == false )
			{
				mTriggerActReservedList.Clear();
				return;
			}

			if( mTriggerActReservedList.Count <= 0 )
				return;

			// ....
		}

		//------------------------------------------------------------------------
		public int DoTriggerActReservedAction( IRelicProcessor processor, ref bool item_save )
		{
			bool is_trigger_reset_update_once = mTriggerResetUpdateOnce;
			mTriggerResetUpdateOnce = false;

			if( IsTriggerReady() == false )
			{
				mTriggerActReservedList.Clear();
				return 0;
			}

			if( mTriggerActReservedList.Count <= 0 )
				return 0;

			int trigger_count = 0;
			List<TriggerActReservedData> reserved_list = new List<TriggerActReservedData>( mTriggerActReservedList );
			mTriggerActReservedList.Clear();

			// ....

			return trigger_count;
		}

		//////////////////////////////////////////////////////////////////////////
		/// static
		/// 
		//------------------------------------------------------------------------
		public static List<ItemContainer> GetFunctionTrigger_Has_List( IRelicProcessor processor, eRELIC_FUNCTION_TYPE func_type, IRelicTarget owner )
		{
			List<ItemContainer> ret_list = null;
			// ....
			return ret_list;
		}

		public static bool GetFunctionTrigger_Has( IRelicProcessor processor, eRELIC_FUNCTION_TYPE func_type, IRelicTarget owner, bool check_only )
		{
			// ....

			return false;
		}

		public static bool FunctionTrigger_ValueSum( IRelicProcessor processor, eRELIC_FUNCTION_TYPE func_type, IRelicTarget owner, ref int sum_value, ref int sum_percent, bool check_only )
		{
			bool has_action = false;
			// ....

			return has_action;
		}
		public static bool FunctionTrigger_Value( IRelicProcessor processor, eRELIC_FUNCTION_TYPE func_type, IRelicTarget owner, int base_value, ref int add_value )
		{
			bool has_action = false;
			// ....
			return has_action;
		}
		public static bool FunctionTrigger_State( IRelicProcessor processor, eRELIC_FUNCTION_TYPE func_type, eCARD_STATE_TYPE state, IRelicTarget owner )
		{
			// ....
			return false;
		}

		//------------------------------------------------------------------------
		public static bool DoItemCounting( IRelicProcessor processor, eRELIC_ACT_TYPE act_type, IRelicTarget owner = null, IRelicTarget target = null )
		{
			bool has_trigger = false;
			// ....
			return has_trigger;
		}

		//------------------------------------------------------------------------		
		public static bool DoTriggerAct( IRelicProcessor processor )
		{
			bool has_action = false;
			// ....
			return has_action;
		}

		//------------------------------------------------------------------------
		public static int DoTriggerByFunctionPreCount( IRelicProcessor processor, ItemContainer item, eRELIC_FUNCTION_TYPE func_type, ref int pre_trigger_count, ref bool save_item )
		{
			int ret_count = 0;
			// ....
			return ret_count;
		}
	}
}