//////////////////////////////////////////////////////////////////////////
//
// ActiveSkill
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// Normal 액티브 스킬
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCBattle
{
	public class ActiveSkill : ActiveSkillBase
	{
		public Card OwnerCard { get; private set; }

		public ActiveSkill( Battle battle, Card owner_card, string skill_id, int level, eSkillFromType skill_from, SkillAdditionLinkData skill_addition_link )
			: base( battle, skill_id, level, eSKILL_CLASS_TYPE.ActiveSkill, skill_addition_link )
		{
			OwnerCard = owner_card;
			m_SkillFromType = skill_from;
		}

		//------------------------------------------------------------------------	
		public override void Log( string fmt, params object[] parms )
		{
			if( OwnerCard != null )
				OwnerCard.Log( this, fmt, parms );
		}

		//------------------------------------------------------------------------	
		public override void SetDead()
		{
			if( mRemainCastingTime > 0f )
			{
				mRemainCoolTime = SkillCooltime;
				mRemainCastingTime = 0f;
				OwnerCard.ResetCastingSkill( this, false );
			}
		}

		//------------------------------------------------------------------------	
		public override void ResetCasting( bool set_cooltime, bool forced = false )
		{
			if( mRemainCastingTime == 0f && forced == false )
				return;

			// ....
		}

		//------------------------------------------------------------------------	
		public void AddCooltime( float value, eFUNCTION_TYPE func_type )
		{
			if( mRemainCoolTime <= 0f )
				return;

			mRemainCoolTime += value;
			if( mRemainCoolTime <= 0f )
				mRemainCoolTime = 0f;

			// ....
		}

		//------------------------------------------------------------------------	
		public override void Tick( float deltaTime )
		{
			float tick_value = deltaTime * OwnerCard.SkillSpeed;
			_Tick( tick_value, OwnerCard );
		}

		//------------------------------------------------------------------------	
		public void Fire( Card target_card )
		{
			_Fire( OwnerCard, target_card );
		}

#if UMCLIENT
		//------------------------------------------------------------------------	
		public override void Sync_CastingCancel_Action( float cool_time )
		{
			base.Sync_CastingCancel_Action( cool_time );
			OwnerCard.ResetCastingSkill( this, false );
		}

		//------------------------------------------------------------------------	
		public override void Sync_CastingSkill_Action( float casting_time, Card target_card )
		{
			base.Sync_CastingSkill_Action( casting_time, target_card );
			OwnerCard.SetCastingSkill( this, false );
		}

		//------------------------------------------------------------------------	
		public override void Sync_FireSkill_Action( float remain_cooltime )
		{
			base.Sync_FireSkill_Action( remain_cooltime );
			OwnerCard.ResetCastingSkill( this, false );

			if( mBattle.WithServerSide == false )
				mHasCooltimeWithServer = false;
		}
#endif

		//------------------------------------------------------------------------	
		public override void OnDead( Card deadCard )
		{
			// Casting reset ignore
		}

		//------------------------------------------------------------------------	
		public override void OnResurrection( Card resurrection_card )
		{
			// Casting reset ignore
		}

		//------------------------------------------------------------------------	
		public override bool CheckSkillTargetForCasting()
		{
			// ....
			return true;
		}
	}
}