//////////////////////////////////////////////////////////////////////////
//
// ActSkill
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 조건에 의해 자동 발동되는 스킬 : 액티브스킬기본형 상속
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System;

namespace DCBattle
{
	public class ActSkill : ActiveSkillBase
	{
		public Card SkillOwnerCard { get; private set; }
		public Card ActOwnerCard { get; private set; }

		SkillFunctionBase mSourceFunctionBase = null;
		public SkillFunctionBase SourceFunctionBase { get { return mSourceFunctionBase; } }
		public eFUNCTION_TYPE ActType { get; private set; }
		List<eFUNCTION_TYPE> mSubActTypeList = null;
		bool mFiredForNegativeDuration = false;
		bool mRemoveProcessWhenFired = false;

		public int MainSkillIDN { get; private set; }

		eFUNCTION_TYPE mFireActType = eFUNCTION_TYPE.None;

		//------------------------------------------------------------------------		
		public ActSkill( Battle battle, SkillFunctionBase func_base, eFUNCTION_TYPE act_type, Card skill_owner_card, Card act_owner_card, string skill_id, int level, SkillAdditionLinkData skill_addition_link, int main_skill_idn )
			: base( battle, skill_id, level, eSKILL_CLASS_TYPE.ActSkill, skill_addition_link )
		{
			mSourceFunctionBase = func_base;
			ActType = act_type;
			mSubActTypeList = null;

			SkillOwnerCard = skill_owner_card;
			ActOwnerCard = act_owner_card; // 테마스킬의 ActOwnerCard 는 등록시 살아있는 카드중 1개라 이값으로 뭔가를 사용하면 안됨.	

			for( int i = 0; i < mSkillFunctionList.Count; i++ )
				mSkillFunctionList[i].m_ActSkillFunctionType = act_type;

			MainSkillIDN = main_skill_idn;
		}

		//------------------------------------------------------------------------	
		public void AddSubActType( eFUNCTION_TYPE act_func_type )
		{
			if( mSubActTypeList == null )
				mSubActTypeList = new List<eFUNCTION_TYPE>();

			if( mSubActTypeList.Contains( act_func_type ) == false )
				mSubActTypeList.Add( act_func_type );
		}

		//------------------------------------------------------------------------	
		public override void Tick( float deltaTime )
		{
			if( mRemainCoolTime > 0f )
			{
				float value = deltaTime * System.Math.Max( 1f + SkillOwnerCard.GetFunctionValue( eFUNCTION_TYPE.RatioSkillTime ), 0.5f );
				mRemainCoolTime -= value;
				if( mRemainCoolTime <= 0f )
				{
					mRemainCoolTime = 0f;
				}
			}
		}

		//------------------------------------------------------------------------	
		public bool CanAct( eFUNCTION_TYPE actType, Card act_target )
		{
			if( IsValid() == false )
				return false;

			if( ActType == actType || ( mSubActTypeList != null && mSubActTypeList.Contains( actType ) ) )
			{
				// ....

				if( mRemainCoolTime != 0f )
					return false;

				return true;
			}

			return false;
		}

		//------------------------------------------------------------------------	
		public void RealFire( Card target_card )
		{
			// ....
			base.PriorityFire();
		}

		//------------------------------------------------------------------------	
		protected override void PriorityPreFire( List<SkillFunctionBase> fire_list, Card owner_card, Card target_card, int priority, float f_value, bool for_passive_remove = false )
		{
			// ....
			base.PriorityPreFire( fire_list, owner_card, target_card, priority, f_value, for_passive_remove );
			if( priority != 0 )
			{
				for( int i = 0; i < mPreFireDataList.Count; i++ )
					mPreFireDataList[i].m_FireActType = mFireActType;

				mRemainCoolTime = SkillCooltime;
				base.PriorityFire();
			}
		}

		//------------------------------------------------------------------------	
		protected override void PriorityFire()
		{
			// wait for all act skill pre fire
		}

		//------------------------------------------------------------------------	
		public void RemoveAct()
		{
			mSourceFunctionBase = null;
			MainSkillIDN = 0;
		}

		//------------------------------------------------------------------------	
		public void ProcessReset()
		{
			mFiredForNegativeDuration = false;

			for( int i = 0; i < mSkillFunctionList.Count; i++ )
				mSkillFunctionList[i].ProcessReset( SkillOwnerCard, SkillOwnerCard );
		}

		//------------------------------------------------------------------------	
		public eSKILL_CLASS_TYPE GetMainSkillClassType()
		{
			if( mSourceFunctionBase != null && mSourceFunctionBase.GetSkillBase != null )
				return mSourceFunctionBase.GetSkillBase.SkillClassType;

			return eSKILL_CLASS_TYPE.None;
		}

		//------------------------------------------------------------------------	
		public override void Log( string fmt, params object[] parms )
		{
			if( SkillOwnerCard != null )
			{
				SkillOwnerCard.Log( this, fmt, parms );
			}
		}
	}
}