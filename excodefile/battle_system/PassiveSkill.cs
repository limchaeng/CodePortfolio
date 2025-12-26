//////////////////////////////////////////////////////////////////////////
//
// PassiveSkill
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 패시브 스킬 
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using U6Common;

namespace DCBattle
{
	//------------------------------------------------------------------------
	public class PassiveSkill : SkillBase
	{
		public Card OwnerCard { get; protected set; }

		protected bool mIsProcessRemoved = true;

		protected Card mTargetOnlyCard = null;

		public PassiveSkill( Battle battle, Card owner_card, string skill_id, int level, eSkillFromType skill_from, SkillAdditionLinkData skill_addition_link )
			: base( battle, skill_id, level, eSKILL_CLASS_TYPE.PassiveSkill, skill_addition_link )
		{
			OwnerCard = owner_card;
			m_SkillFromType = skill_from;
		}

		//------------------------------------------------------------------------
		public override bool IsValid()
		{
			if( base.IsValid() == false )
				return false;

			if( m_SkillInfo.SkillType != eSKILL_TYPE.Passive )
			{
				Log( $"?? PassiveSkill Invalid : idx={mIndex} ID={mID} lv={mLevel} class={SkillClassType} from={m_SkillFromType} type={m_SkillInfo.SkillType}" );
				return false;
			}

			return true;
		}

		//------------------------------------------------------------------------
		public override void Log( string fmt, params object[] parms )
		{
			if( OwnerCard != null )
				OwnerCard.Log( this, fmt, parms );
		}

		//------------------------------------------------------------------------	
		protected override void PriorityFire()
		{
			Log( " [{0}] PriorityFire fireCount:{1}", m_SkillInfo.NameKey, mPreFireDataList.Count );

			// ....

			mTargetOnlyCard = null;
			mPreFireDataList.Clear();
		}

		//------------------------------------------------------------------------
		public virtual void PassiveProcess( bool bRemove, bool actSkillOnly, bool is_resurrection = false )
		{
			// ....
			PriorityPreFire( p_list, OwnerCard, null, 0, 0f, bRemove );
		}
	}
}