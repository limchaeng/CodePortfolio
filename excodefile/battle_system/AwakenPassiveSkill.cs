//////////////////////////////////////////////////////////////////////////
//
// AwakenPassiveSkill
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 각성석 패시브 스킬
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using U6Common;

namespace DCBattle
{
	public class AwakenPassiveSkill : PassiveSkill
	{
		CS_AwakenStoneData mAwakenStoneData = null;
		public CS_AwakenStoneData AwakenStoneData { get { return mAwakenStoneData; } }

		public AwakenPassiveSkill( Battle battle, Card owner_card, CS_AwakenStoneData stone_data, SkillInfo info ) 
			: base( battle, owner_card, info.ID, info.Level, eSkillFromType.AwakenSkill, null )
		{
			mAwakenStoneData = stone_data;
			SkillClassType = eSKILL_CLASS_TYPE.AwakenPassiveSkill;
		}

		//------------------------------------------------------------------------
		public override bool IsValid()
		{
			if( base.IsValid() == false )
				return false;

			if( m_SkillInfo.SkillType != eSKILL_TYPE.Passive )
				return false;

			return true;
		}

	}
}