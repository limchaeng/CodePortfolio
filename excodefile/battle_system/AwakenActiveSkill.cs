//////////////////////////////////////////////////////////////////////////
//
// AwakenActiveSkill
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 각성석 액티브 스킬 정의
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using U6Common;

namespace DCBattle
{

	public class AwakenActiveSkill : ActiveSkill
	{
		CS_AwakenStoneData mAwakenStoneData = null;
		public CS_AwakenStoneData AwakenStoneData { get { return mAwakenStoneData; } }

		public AwakenActiveSkill( Battle battle, Card owner_card, CS_AwakenStoneData stone_data, SkillInfo info ) 
			: base( battle, owner_card, info.ID, info.Level, eSkillFromType.AwakenSkill, null )
		{
			mAwakenStoneData = stone_data;
			SkillClassType = eSKILL_CLASS_TYPE.AwakenActiveSkill;
		}

		//------------------------------------------------------------------------
		public override bool IsValid()
		{
			if( base.IsValid() == false )
				return false;

			if( m_SkillInfo.SkillType != eSKILL_TYPE.Active )
				return false;

			return true;
		}

	}
}