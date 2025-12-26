//////////////////////////////////////////////////////////////////////////
//
// SkillBase
// 
// Created by LCY.
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 스킬 시스템 베이스 
//////////////////////////////////////////////////////////////////////////

#if ANTICHEAT
using CodeStage.AntiCheat.ObscuredTypes;
#endif
using System.Collections.Generic;
using System.Linq;
using U6Common;
using UMF.Core;

namespace DCBattle
{
	//------------------------------------------------------------------------
	public class SkillFireData_OUT
	{
		// 스킬발동후의 결과 및 추가 저장 데이터 클래스

		public SkillFireData_OUT( SkillBase skill )
		{
			fire_skill_base = skill;
			Clear();
		}

		public void Clear()
		{
		}
	}

	//------------------------------------------------------------------------
	public class SkillBase
	{
		// active skill INDEX 
		public const int DYNAMIC_ACTIVE_SKILL_INDEX_PREFIX = 200;
		public const int DYNAMIC_THEME_SKILL_INDEX = 50;

		public static bool IsDynamicSkillIndex( int skill_idx )
		{
			return skill_idx >= DYNAMIC_ACTIVE_SKILL_INDEX_PREFIX;
		}

		// cmd sorting order
		public enum eSkillFromType : byte
		{
			None,
			// sorting order
			// ....
		}

		public eSkillFromType m_SkillFromType { get; protected set; }
		public eSKILL_CLASS_TYPE SkillClassType { get; protected set; }

		public SkillInfo m_SkillInfo { get; protected set; }

		protected int mLevel = 0;
		public int m_Level { get { return mLevel; } }
		protected Battle mBattle = null;
		protected string mID = "";

		protected List<SkillFunctionBase> mSkillFunctionList = new List<SkillFunctionBase>();
		public float CastingTime { get { return m_SkillInfo.CastingTime; } }

		protected int mIndex = -1;
		public void SetIndex( int idx ) { mIndex = idx; }
		public int Index { get { return mIndex; } }
		public bool IsDynamicActiveSkill { get; set; } = false;

		protected List<SkillTargetPreFireData> mPreFireDataList = new List<SkillTargetPreFireData>();
		protected List<SkillTargetFiredData> mFiredTargetDataList = new List<SkillTargetFiredData>();
		public List<SkillTargetFiredData> FiredTargetDataList { get { return mFiredTargetDataList; } }

		protected SkillAdditionLinkData mSkillAdditionLink = null;

		protected float mActionBaseValuePlus_Value = 0f;
		protected float mActionBaseValuePlus_Percent = 0f;

		protected SkillFireData_OUT mSKillFireOutData = null;
		public SkillFireData_OUT SKillFireOutData { get { return mSKillFireOutData; } }
		public void ClearSkillFireoutData() { mSKillFireOutData.Clear(); }

		//------------------------------------------------------------------------	
		public SkillBase( Battle battle, string id, int level, eSKILL_CLASS_TYPE class_type, SkillAdditionLinkData skill_addition_link )
		{
			mBattle = battle;
			mID = id;
			mLevel = level;
			SkillClassType = class_type;

			mSkillAdditionLink = skill_addition_link;
			mSKillFireOutData = new SkillFireData_OUT( this );

			m_SkillInfo = SkillInfoManager.Instance.GetSkillLevelInfo( id, level );
			if( m_SkillInfo != null )
			{
				mLevel = m_SkillInfo.Level;
				int insert_index = 0;
				foreach( SkillActionInfo action_info in m_SkillInfo.SkillActionList )
				{
					if( action_info.IsValid() )
					{
						SkillFunctionBase functionBase = SkillFunctionFactoryManager.Instance.Create( battle, this, action_info, class_type, skill_addition_link );
						if( functionBase != null )
						{
							// ....
							mSkillFunctionList.Add( functionBase );
						}
						else
							Log( "? SkillActionInfo FunctionInvalid :{0}", action_info.FunctionType );
					}
					else
					{
						Log( "? SkillActionInfo Invalid:{0}/{1}", id, m_Level );
					}
				}
			}
			else
			{
				Log( "? SkillInfo Invalid:{0}", id );
			}
		}

		//------------------------------------------------------------------------
		public virtual bool IsValid()
		{
			bool is_valid = ( m_SkillInfo != null && m_SkillInfo.IsValid );
			if( is_valid == false )
			{
				Log( $"?? SkillBase Invalid : idx={mIndex} ID={mID} lv={mLevel} class={SkillClassType} from={m_SkillFromType}" );
			}

			return is_valid;
		}

		public override string ToString()
		{
			if( m_SkillInfo != null )
				return $"[{m_SkillInfo.IDN}/{m_SkillInfo.ID}-{mLevel} class:{SkillClassType}]";
			else
				return $"[{mID}-{mLevel} class:{SkillClassType}]";
		}

		//------------------------------------------------------------------------
		public virtual void BattleBegin() { }

		//------------------------------------------------------------------------
		public virtual float CalcActionBaseValuePlus( float value )
		{
			float plus_value = mActionBaseValuePlus_Value;
			plus_value += value * mActionBaseValuePlus_Percent * 0.01f;

			float result = value + plus_value;
			if( mActionBaseValuePlus_Value != 0f || mActionBaseValuePlus_Percent != 0f )
			{
				Log( $"- CalcActionBaseValuePlus v={value} plus={mActionBaseValuePlus_Value} percent={mActionBaseValuePlus_Percent} result={result}" );
			}

			return result;
		}

		public virtual void SetActionBaseValuePlus( float value, float percent )
		{
			mActionBaseValuePlus_Value = value;
			mActionBaseValuePlus_Percent = percent;
		}
		public void ClearActionBaseValuePlus()
		{
			mActionBaseValuePlus_Value = 0f;
			mActionBaseValuePlus_Percent = 0f;
		}

		//------------------------------------------------------------------------	
		public bool CanSkillPrefire( Card target, eFUNCTION_TYPE func_type )
		{
			if( target.IsDeadOrDyingPending )
			{
				switch( func_type )
				{
					case eFUNCTION_TYPE.Resurrection:
						return true;

					default:
						return false;
				}
			}
			else
			{
				return true;
			}
		}

		//------------------------------------------------------------------------	
		public bool CanTargeting( Card target )
		{
			for( int i = 0; i < mSkillFunctionList.Count; i++ )
			{
				if( mSkillFunctionList[i].CanTargeting( target ) == true )
					return true;
			}

			return false;
		}

		//------------------------------------------------------------------------	
		public bool HasConditionState( Card target )
		{
			for( int i = 0; i < mSkillFunctionList.Count; i++ )
			{
				SkillFunctionBase skill_func = mSkillFunctionList[i];
				if( skill_func.m_ActionInfo.ActionCondition != null &&
					skill_func.m_ActionInfo.ActionCondition.TargetCondtion != null &&
					skill_func.m_ActionInfo.ActionCondition.TargetCondtion.ConditionType == eSKILL_CONDITION_TYPE.State )
				{
					if( target.HasBuffState( skill_func.m_ActionInfo.ActionCondition.TargetCondtion.StateTypeList ) )
						return true;
				}
			}

			return false;
		}

		//------------------------------------------------------------------------	
		protected virtual void PriorityFire()
		{
			Log( " [{0}] PriorityFire fireCount:{1}", m_SkillInfo.NameKey, mPreFireDataList.Count );

			for( int i = 0; i < mPreFireDataList.Count; i++ )
			{
				SkillTargetPreFireData fireData = mPreFireDataList[i];
				Card target_card = fireData.m_TargetCard;

				if( target_card != null )
				{
					SkillTargetFiredData fired_data = mBattle.SkillTargetFiredData_GET();
					fired_data.m_TargetCard = target_card;
					fired_data.m_SkillTargetFlags = fireData.m_SkillTargetFlags;

					mFiredTargetDataList.Add( fired_data );
				}

				fireData.m_Function.Fire( fireData );
			}

			mPreFireDataList.Clear();
		}

		//------------------------------------------------------------------------	
		protected void PreFunctionFire( SkillFunctionBase func, Card owner_card, Card func_target_card, Card skill_target_card, float f_value, bool for_passive_remove, eSkillTargetFlag skill_target_flags )
		{
			if( mBattle.Config.HasFlags( BattleConfig.eFlag.ExSkill_Func_TargetLegacy ) )
			{
				eSkillTargetFlag real_skill_target_flags = skill_target_flags;

				List<Card> func_target_list = new List<Card>();
				switch( func.m_ActionInfo.ActionTarget )
				{
					case eSKILL_TARGET_TYPE.Self:
						func_target_list.Add( owner_card );
						real_skill_target_flags |= eSkillTargetFlag.FuncTarget;
						break;

					case eSKILL_TARGET_TYPE.Enemy_All:
						func_target_list.AddRange( owner_card.m_CardDeck.m_EnemyDeck.CardList );
						real_skill_target_flags |= eSkillTargetFlag.FuncTarget;
						break;

					case eSKILL_TARGET_TYPE.Friend_All:
						func_target_list.AddRange( owner_card.m_CardDeck.CardList );
						real_skill_target_flags |= eSkillTargetFlag.FuncTarget;
						break;

					case eSKILL_TARGET_TYPE.NotSelf_Friend_All:
						func_target_list.AddRange( owner_card.m_CardDeck.CardList.FindAll( a => a != owner_card ) );
						real_skill_target_flags |= eSkillTargetFlag.FuncTarget;
						break;

					case eSKILL_TARGET_TYPE.All:
						func_target_list.AddRange( owner_card.m_CardDeck.m_EnemyDeck.CardList );
						func_target_list.AddRange( owner_card.m_CardDeck.CardList );
						real_skill_target_flags |= eSkillTargetFlag.FuncTarget;
						break;

					case eSKILL_TARGET_TYPE.NotSelf_All:
						func_target_list.AddRange( owner_card.m_CardDeck.m_EnemyDeck.CardList );
						func_target_list.AddRange( owner_card.m_CardDeck.CardList.FindAll( a => a != owner_card ) );
						real_skill_target_flags |= eSkillTargetFlag.FuncTarget;
						break;

					default:
						func_target_list.Add( skill_target_card );
						break;
				}

				foreach( Card _func_target in func_target_list )
				{
					if( CanSkillPrefire( _func_target, func.m_ActionInfo.FunctionType ) == false )
						continue;

					int func_fire_count = func.m_ActionInfo.ActionFireCount;
					for( int fc = 0; fc < func_fire_count; fc++ )
					{
						if( CanSkillPrefire( _func_target, func.m_ActionInfo.FunctionType ) == false )
							continue;

						SkillTargetPreFireData fireData = func.PreFire( owner_card, _func_target, skill_target_card, f_value, for_passive_remove, real_skill_target_flags );
						if( fireData != null )
							mPreFireDataList.Add( fireData );
					}
				}
			}
			else
			{

				int func_fire_count = func.m_ActionInfo.ActionFireCount;
				eSKILL_TARGET_TYPE func_target_type = func.GetSkillBase.m_SkillInfo.TargetType;
				for( int fc = 0; fc < func_fire_count; fc++ )
				{
					if( func_target_type != eSKILL_TARGET_TYPE.Self )
					{
						if( CanSkillPrefire( func_target_card, func.m_ActionInfo.FunctionType ) == false )
							continue;
					}

					SkillTargetPreFireData fireData = func.PreFire( owner_card, func_target_card, skill_target_card, f_value, for_passive_remove, skill_target_flags );
					if( fireData != null )
						mPreFireDataList.Add( fireData );
				}
			}
		}

		//------------------------------------------------------------------------	
		List<Card> GetFixedSkillTargetList( eSKILL_TARGET_TYPE target_type, Card owner_card, Card target_card, SkillFunctionBase func )
		{
			List<Card> list = null;

			switch( target_type )
			{
				case eSKILL_TARGET_TYPE.Enemy:
				case eSKILL_TARGET_TYPE.Friend:
				case eSKILL_TARGET_TYPE.NotSelf_Friend:
					{
						if( target_type == eSKILL_TARGET_TYPE.NotSelf_Friend && target_card == owner_card )
							return null;

						if( target_card == null )
							return null;

						if( list == null )
							list = new List<Card>();

						list.Add( target_card );
					}
					break;

				case eSKILL_TARGET_TYPE.Enemy_All:
					{
						if( list == null )
							list = new List<Card>();

						foreach( Card _target in owner_card.m_CardDeck.m_EnemyDeck.CardList )
						{
							if( func is SkillFuncAttackBase )
							{
								if( _target.HasPriorityDamageActWideSkill() && func is SkillFuncAttackBase )
									list.Insert( 0, _target );
								else
									list.Add( _target );
							}
							else
							{
								list.Add( _target );
							}
						}
					}
					break;

				case eSKILL_TARGET_TYPE.Friend_All:
				case eSKILL_TARGET_TYPE.NotSelf_Friend_All:
					{
						foreach( Card _target in owner_card.m_CardDeck.CardList )
						{
							if( target_type == eSKILL_TARGET_TYPE.NotSelf_Friend_All && _target == owner_card )
								continue;

							if( list == null )
								list = new List<Card>();

							list.Add( _target );
						}
					}
					break;

				case eSKILL_TARGET_TYPE.All:
				case eSKILL_TARGET_TYPE.NotSelf_All:
					{
						List<Card> all_cards = new List<Card>();
						all_cards.AddRange( owner_card.m_CardDeck.m_EnemyDeck.CardList );
						all_cards.AddRange( owner_card.m_CardDeck.CardList );

						foreach( Card _target in owner_card.m_CardDeck.m_EnemyDeck.CardList )
						{
							if( list == null )
								list = new List<Card>();

							list.Add( _target );
						}

						foreach( Card _target in owner_card.m_CardDeck.CardList )
						{
							if( target_type == eSKILL_TARGET_TYPE.NotSelf_All && _target == owner_card )
								continue;

							if( list == null )
								list = new List<Card>();

							list.Add( _target );
						}
					}

					break;

				case eSKILL_TARGET_TYPE.Self:
					if( list == null )
						list = new List<Card>();
					list.Add( owner_card );
					break;
			}

			return list;
		}

		//------------------------------------------------------------------------	
		protected virtual void PriorityPreFire( List<SkillFunctionBase> fire_list, Card owner_card, Card target_card, int priority, float f_value, bool for_passive_remove = false )
		{
			mPreFireDataList.Clear();

			if( fire_list.Count <= 0 )
				return;

			Log( " [{0}] PriorityPreFire priority:{1}", m_SkillInfo.NameKey, priority );

			for( int i = 0; i < fire_list.Count; i++ )
			{
				SkillFunctionBase func = fire_list[i];

				// new target system
				if( mBattle.Config.HasFlags( BattleConfig.eFlag.ExSkill_Func_TargetLegacy ) == false )
				{
					eSKILL_TARGET_TYPE real_target_type = m_SkillInfo.TargetType;
					List<Card> fixed_skill_target_list = null;
					List<Card> skill_target_list = GetFixedSkillTargetList( m_SkillInfo.TargetType, owner_card, target_card, func );
					bool is_func_target = false;

					if( func.m_ActionInfo.ActionTarget != eSKILL_TARGET_TYPE.None )
					{
						is_func_target = true;
						real_target_type = func.m_ActionInfo.ActionTarget;
						fixed_skill_target_list = GetFixedSkillTargetList( func.m_ActionInfo.ActionTarget, owner_card, target_card, func );
					}
					else
					{
						fixed_skill_target_list = skill_target_list;
					}

					if( fixed_skill_target_list == null || fixed_skill_target_list.Count <= 0 )
						continue;

					foreach( Card fixed_target in fixed_skill_target_list )
					{
						if( real_target_type != eSKILL_TARGET_TYPE.Self )
						{
							if( CanSkillPrefire( fixed_target, func.m_ActionInfo.FunctionType ) == false )
								continue;
						}

						eSkillTargetFlag skill_target_flags = eSkillTargetFlag.None;
						if( is_func_target )
							skill_target_flags |= eSkillTargetFlag.FuncTarget;

						if( skill_target_list != null && skill_target_list.Contains( fixed_target ) )
							skill_target_flags |= eSkillTargetFlag.SkillTarget;

						PreFunctionFire( func, owner_card, fixed_target, target_card, f_value, for_passive_remove, skill_target_flags );
					}
				}
				else
				{
					eSkillTargetFlag skill_target_flags = eSkillTargetFlag.SkillTarget;
					// old system
					switch( m_SkillInfo.TargetType )
					{
						case eSKILL_TARGET_TYPE.Enemy:
						case eSKILL_TARGET_TYPE.Friend:
						case eSKILL_TARGET_TYPE.NotSelf_Friend:
							{
								if( m_SkillInfo.TargetType == eSKILL_TARGET_TYPE.NotSelf_Friend && target_card == owner_card )
									continue;

								if( target_card == null )
									continue;

								if( CanSkillPrefire( target_card, func.m_ActionInfo.FunctionType ) == false )
									continue;

								PreFunctionFire( func, owner_card, null, target_card, f_value, for_passive_remove, skill_target_flags );
							}
							break;

						case eSKILL_TARGET_TYPE.Enemy_All:
							{
								List<Card> _target_cards = owner_card.m_CardDeck.m_EnemyDeck.CardList;
								List<Card> tmp_sorted_card_list = new List<Card>();
								foreach( Card _target in _target_cards )
								{
									if( CanSkillPrefire( _target, func.m_ActionInfo.FunctionType ) == false )
										continue;

									if( func is SkillFuncAttackBase )
									{
										if( _target.HasPriorityDamageActWideSkill() && func is SkillFuncAttackBase )
											tmp_sorted_card_list.Insert( 0, _target );
										else
											tmp_sorted_card_list.Add( _target );
									}
									else
									{
										tmp_sorted_card_list.Add( _target );
									}
								}

								for( int t = 0; t < tmp_sorted_card_list.Count; t++ )
								{
									Card _target = tmp_sorted_card_list[t];

									PreFunctionFire( func, owner_card, null, _target, f_value, for_passive_remove, skill_target_flags );
								}
							}
							break;

						case eSKILL_TARGET_TYPE.Friend_All:
						case eSKILL_TARGET_TYPE.NotSelf_Friend_All:
							{
								foreach( Card _target in owner_card.m_CardDeck.CardList )
								{
									if( m_SkillInfo.TargetType == eSKILL_TARGET_TYPE.NotSelf_Friend_All && _target == owner_card )
										continue;

									if( CanSkillPrefire( _target, func.m_ActionInfo.FunctionType ) == false )
										continue;

									PreFunctionFire( func, owner_card, null, _target, f_value, for_passive_remove, skill_target_flags );
								}
							}
							break;

						case eSKILL_TARGET_TYPE.All:
						case eSKILL_TARGET_TYPE.NotSelf_All:
							{
								List<Card> all_cards = new List<Card>();
								all_cards.AddRange( owner_card.m_CardDeck.m_EnemyDeck.CardList );
								all_cards.AddRange( owner_card.m_CardDeck.CardList );

								foreach( Card _target in all_cards )
								{
									if( m_SkillInfo.TargetType == eSKILL_TARGET_TYPE.NotSelf_All && _target == owner_card )
										continue;

									if( CanSkillPrefire( _target, func.m_ActionInfo.FunctionType ) == false )
										continue;

									PreFunctionFire( func, owner_card, null, _target, f_value, for_passive_remove, skill_target_flags );
								}
							}
							break;

						case eSKILL_TARGET_TYPE.Self:
							PreFunctionFire( func, owner_card, null, owner_card, f_value, for_passive_remove, skill_target_flags );
							break;
					}
				}
			}

			owner_card.m_CardDeck.ResetRemoveReservedFunctionValue();
			if( target_card != null )
				target_card.m_CardDeck.ResetRemoveReservedFunctionValue();

			PriorityFire();
		}

		//------------------------------------------------------------------------		
		protected bool mIsSimulateFire = false;
		protected void InternalFire( Card owner_card, Card target_card, float f_value )
		{
			if( mFiredTargetDataList.Count > 0 )
				mFiredTargetDataList.ForEach( a => mBattle.SkillTargetFired_RETURN( a ) );
			mFiredTargetDataList.Clear();

			if( mSkillFunctionList.Exists( f => f.m_ActionInfo.Priority != 0 ) )
			{
				List<SkillFunctionBase> p_list = new List<SkillFunctionBase>();
				List<SkillFunctionBase> sorted_list = mSkillFunctionList.OrderBy( f => f.m_ActionInfo.Priority ).ToList();
				int priority = sorted_list[0].m_ActionInfo.Priority;
				for( int i = 0; i < sorted_list.Count; i++ )
				{
					if( sorted_list[i].m_ActionInfo.Priority != priority )
					{
						PriorityPreFire( p_list, owner_card, target_card, priority, f_value );
						p_list.Clear();

						priority = sorted_list[i].m_ActionInfo.Priority;
					}

					p_list.Add( sorted_list[i] );
				}

				if( p_list.Count > 0 )
				{
					PriorityPreFire( p_list, owner_card, target_card, priority, f_value );
				}
			}
			else
			{
				PriorityPreFire( mSkillFunctionList, owner_card, target_card, 0, f_value );
			}
		}

		//------------------------------------------------------------------------
		public virtual void Log( string fmt, params object[] parms ) { }

		//------------------------------------------------------------------------	
		public bool HasState( eCARD_STATE_TYPE state )
		{
			return mSkillFunctionList.Exists( f => SkillFunctionBase.GetStateType( f.m_ActionInfo.FunctionType ) == state );
		}
	}
}