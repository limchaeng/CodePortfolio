//////////////////////////////////////////////////////////////////////////
//
// ActiveSkillBase
// 
// Created by LCY.
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 액티브 스킬의 기본형
//////////////////////////////////////////////////////////////////////////
#if ANTICHEAT
using CodeStage.AntiCheat.ObscuredTypes;
#endif

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using U6Common;

namespace DCBattle
{
	//------------------------------------------------------------------------
	public class ActiveSkillBase : SkillBase
	{
#if ANTICHEAT
		protected ObscuredFloat mRemainCastingTime = 0f;
		protected ObscuredFloat mRemainCoolTime = 0f;
#else
		protected float mRemainCastingTime = 0f;
		protected float mRemainCoolTime = 0f;
#endif
		public float RemainCastingTime { get { return mRemainCastingTime; } }
		public float RemainCastingTimePercent { get { return mRemainCastingTime / m_SkillInfo.CastingTime; } }

		protected bool mHasCooltimeWithServer = false;
		public bool HasCooltimeWithServer { get { return mHasCooltimeWithServer; } }

		protected float mNegativeCastingOvertimeForCooltime = 0f;

		public float RemainCoolTime
		{
			get
			{
				return mRemainCoolTime;
			}
			
			#if DCONSOLE
			set
			{
				mRemainCoolTime = value;
			}
			#endif
		}
		
		public float RemainCoolTimePercent { get { return mRemainCoolTime / SkillCooltime; } }

		protected Card mSkillTargetForCasting = null;
		public Card SkillTargetForCasting { get { return mSkillTargetForCasting; } }

		public virtual float SkillCooltime
		{
			get
			{
				return m_SkillInfo.CoolTime;
			}
		}

		protected virtual void UpdateBattleActionWhenFire( BattleAction fire_action ) { }

		// for active skill simulate

		Dictionary<Card, SimulateSkill.Container> mSimulateCahced = new Dictionary<Card, SimulateSkill.Container>();

		//------------------------------------------------------------------------	
		public ActiveSkillBase( Battle battle, string id, int level, eSKILL_CLASS_TYPE class_type, SkillAdditionLinkData skill_addition_link )
			: base( battle, id, level, class_type, skill_addition_link )
		{

		}

		//------------------------------------------------------------------------
		public override bool IsValid()
		{
			if( base.IsValid() == false )
				return false;

			if( m_SkillInfo.SkillType != U6Common.eSKILL_TYPE.Active )
			{
				Log( $"?? ActiveSkillBase Invalid : idx={mIndex} ID={mID} lv={mLevel} class={SkillClassType} from={m_SkillFromType} type={m_SkillInfo.SkillType}" );
				return false;
			}

			return true;
		}

		//------------------------------------------------------------------------	
		public virtual int GetSkillClassIDN()
		{
			return 0;
		}

		//------------------------------------------------------------------------	
		public override void BattleBegin()
		{
			base.BattleBegin();

			if( m_SkillInfo.CoolTimePre > 0f )
			{
				mRemainCoolTime = m_SkillInfo.CoolTimePre;
			}
		}

		//------------------------------------------------------------------------	
		public virtual void SetDead()
		{
			if( mRemainCastingTime > 0f )
			{
				mRemainCoolTime = SkillCooltime;
				mRemainCastingTime = 0f;
			}
		}

		//------------------------------------------------------------------------	
		public virtual void ResetCasting( bool set_cooltime, bool forced = false )
		{
			if( mRemainCastingTime == 0f && forced == false )
				return;

			mRemainCastingTime = 0f;

			if( set_cooltime )
				mRemainCoolTime = SkillCooltime;
		}

		//------------------------------------------------------------------------	
		public virtual void Tick( float deltaTime )
		{
			_Tick( deltaTime, null );
		}

		//------------------------------------------------------------------------
		protected void _Tick( float tick_value, Card owner_card )
		{
			if( HasWaitTimeForFire() )
			{
				if( mRemainCastingTime > 0f )
				{
					mRemainCastingTime -= tick_value;
					if( mRemainCastingTime <= 0f )
					{
						mNegativeCastingOvertimeForCooltime = mRemainCastingTime;
						mRemainCastingTime = 0f;

						// ....
					}
				}
				else
				{
					if( mRemainCoolTime > 0f )
					{
						mRemainCoolTime -= tick_value;
						if( mRemainCoolTime <= 0f )
						{
							mRemainCoolTime = 0f;

							if( owner_card != null )
							{
								// ....
							}

							CoolTimeFinished();
						}
					}
				}
			}
		}

		//------------------------------------------------------------------------	
		protected virtual void CoolTimeFinished() { }

#if UMCLIENT
		//------------------------------------------------------------------------	
		public virtual void Sync_CastingCancel_Action( float cool_time )
		{
			mSkillTargetForCasting = null;
			mRemainCastingTime = 0f;

			mRemainCoolTime = cool_time;
		}

		//------------------------------------------------------------------------	
		public virtual void Sync_CastingSkill_Action( float casting_time, Card target_card )
		{
			mSkillTargetForCasting = target_card;
			mRemainCastingTime = casting_time;
		}

		//------------------------------------------------------------------------	
		public virtual void Sync_FireSkill_Action( float remain_cooltime )
		{
			mNegativeCastingOvertimeForCooltime = 0f;
			mRemainCoolTime = remain_cooltime;
			mHasCooltimeWithServer = true;
		}

		//------------------------------------------------------------------------	
		public virtual void Sync_SkillCooltimeUpdate_Action( float value )
		{
			mRemainCoolTime = value;
			mHasCooltimeWithServer = false;
		}
#endif

		//------------------------------------------------------------------------
		public bool HasWaitTimeForFire()
		{
			return ( mRemainCastingTime > 0f || mRemainCoolTime > 0f || mRemainCoolTime < 0f ); // only one process when cooltime is negative
		}

		//------------------------------------------------------------------------	
		protected void _Fire( Card owner_card, Card target_card )
		{
			mSkillTargetForCasting = target_card;
			mRemainCastingTime = m_SkillInfo.CastingTime;

			if( mRemainCastingTime <= 0f )
			{
				_RealFire( owner_card, target_card );
			}
			else
			{
				// ....
			}
		}

		//------------------------------------------------------------------------	
		protected void _RealFire( Card owner_card, Card target_card )
		{
			if( owner_card != null )
				owner_card.m_CardDeck.ClearResurrectionFlag();

			if( target_card != null )
				target_card.m_CardDeck.ClearResurrectionFlag();

			if( mBattle.WithServerSide && mBattle.IsServer == false )
				return;

			mBattle.OnActiveSkillFire_Begin( this );

			int fire_skill_identity = mBattle.FireSkillIdentity_Create();

			// ....
			// SkillFireData_OUT 처리

			ClearSkillFireoutData();

			mBattle.OnActiveSkillFire_End( this );
		}

		//------------------------------------------------------------------------	
		public SimulateSkill.Container SimulateFire( Card owner, Card target, System.Func<eSKILL_TARGET_TYPE, List<Card>> func_get_current_target_cards )
		{
			if( owner == null || target == null )
				return null;

			SimulateSkill.Container container;
			if( mSimulateCahced.TryGetValue( target, out container ) )
				return container;

			container = mBattle.GetSimulateSkill.GetContainer( target );
			mSimulateCahced.Add( target, container );

			owner.Log( this, $"[{m_SkillInfo.NameKey}] Simulate Process Begin" );

			DoSimulateFire( container, owner, target );

			// ....

			owner.Log( this, $"[{m_SkillInfo.NameKey}] Simulate Process End" );

			return container;
		}

		//------------------------------------------------------------------------
		void DoSimulateFire( SimulateSkill.Container container, Card owner, Card target )
		{
			foreach( SkillFunctionBase func in mSkillFunctionList )
			{
				switch( func.m_ActionInfo.ActionTarget )
				{
					case eSKILL_TARGET_TYPE.Self:
						if( owner != target )
							continue;
						break;

					case eSKILL_TARGET_TYPE.Enemy_All:
						if( owner.BattleIDX == target.BattleIDX )
							return;
						break;

					case eSKILL_TARGET_TYPE.Friend_All:
						if( owner.BattleIDX != target.BattleIDX )
							continue;
						break;

					case eSKILL_TARGET_TYPE.NotSelf_Friend_All:
					case eSKILL_TARGET_TYPE.NotSelf_All:
						if( owner == target )
							continue;
						break;
				}

				// ....

				eSkillTargetFlag skill_target_flags = eSkillTargetFlag.SkillTarget;
				if( func.m_ActionInfo.ActionTarget != eSKILL_TARGET_TYPE.None )
					skill_target_flags |= eSkillTargetFlag.FuncTarget;

				func.BeginSimulate( container );

				// ....

				func.EndSimulate();
			}

			// 게이지 시뮬을 위한 합산 데이터 여기에서 계산 ( ShowPreFuncData )
			container.CalculateData();
		}


		//------------------------------------------------------------------------	
		// 턴종료시 새로운 값을 위해 모든 데이터 리셋
		public void SimulateReset()
		{
			if( mSimulateCahced.Count > 0 )
			{
				foreach( SimulateSkill.Container container in mSimulateCahced.Values )
				{
					mBattle.GetSimulateSkill.ReturnContainer( container );
				}
			}

			mSimulateCahced.Clear();
		}

		//------------------------------------------------------------------------	
		public virtual void OnDead( Card deadCard )
		{
			// ....
		}

		//------------------------------------------------------------------------	
		public virtual void OnResurrection( Card resurrection_card )
		{
			// ....
		}

		//------------------------------------------------------------------------	
		public virtual bool CheckSkillTargetForCasting() { return true; }
	}
}