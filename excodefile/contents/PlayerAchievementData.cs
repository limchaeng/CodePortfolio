//////////////////////////////////////////////////////////////////////////
//
// PlayerAchievementData
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 업적 : 클라이언트 데이터 구조
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerAchievementData : PlayerDataSingleton<PlayerAchievementData>
{
	public class ClientData : CS_AchievementData
	{
		public bool m_IsFinished = false;
		public bool m_IsClosed = false;
		public CS_RewardData m_CloseProgressReward = null;
		public TBL_Achievement m_AchievementInfo = null;
		public TBL_Achievement.Step m_AchievementStepInfo = null;

		public float GetProgressValue()
		{
			if( m_AchievementStepInfo == null )
				return 0f;

			if( progress_value >= m_AchievementStepInfo.AchieveValue )
				return 1f;

			return (float)progress_value / (float)m_AchievementStepInfo.AchieveValue;
		}
	}

	public int Version { get; set; }
	List<ClientData> mCachedAchievelist = new List<ClientData>();
	public int HasUpdatedAchievementCount { get; set; }

	public override void Init()
	{
		
	}

	public override void SetupFromLogin( G2C_AccountLoginAck loginPacket )
	{
		HasUpdatedAchievementCount = 0;
		mCachedAchievelist.Clear();
		Version = loginPacket.game_config.achievement_version;

		UpdateAchievement( loginPacket.achievement_container, true );
	}

	//------------------------------------------------------------------------
	public void UpdateAchievement( CS_AchievementContainer container, bool from_login=false )
	{
		HasUpdatedAchievementCount = 0;

		if( container == null )
			return;

		if( container.updated_list != null )
		{
			HasUpdatedAchievementCount += container.updated_list.Count;
			for( int i = 0; i < container.updated_list.Count; i++ )
			{
				CS_AchievementData data = container.updated_list[i];
				ClientData client_data = mCachedAchievelist.Find( a => a.achievement_idn == data.achievement_idn );
				TBL_Achievement info = TBL_AchievementManager.Instance.GetInfoByIntKey( data.achievement_idn );
				if( info != null )
				{
					TBL_Achievement.Step step_info = info.GetStepInfo( data.achieve_step, Version );
					if( step_info != null )
					{
						if( client_data != null )
							client_data.Update( data );
						else
						{
							client_data = new ClientData();
							client_data.Update( data );
							client_data.m_IsFinished = false;
							client_data.m_IsClosed = false;

							mCachedAchievelist.Add( client_data );
						}

						client_data.m_AchievementInfo = info;
						client_data.m_AchievementStepInfo = step_info;							
					}
				}
			}
		}

		if( container.finished_list != null )
		{
			HasUpdatedAchievementCount += container.finished_list.Count;
			for( int i = 0; i < container.finished_list.Count; i++ )
			{
				CS_AchievementFinishedData data = container.finished_list[i];
				ClientData client_data = mCachedAchievelist.Find( a => a.achievement_idn == data.achievement_idn );
				TBL_Achievement info = TBL_AchievementManager.Instance.GetInfoByIntKey( data.achievement_idn );
				if( info != null )
				{
					TBL_Achievement.Step step_info = info.GetLastStep( Version );
					if( step_info != null )
					{
						if( client_data != null )
							client_data.m_IsFinished = true;
						else
						{
							client_data = new ClientData();
							client_data.achievement_idn = data.achievement_idn;
							client_data.achieve_step = (byte)step_info.AchieveStep;
							client_data.progress_value = step_info.AchieveValue;
							client_data.is_completed = true;
							client_data.m_IsFinished = true;
							client_data.m_IsClosed = false;

							mCachedAchievelist.Add( client_data );
						}

						client_data.m_AchievementInfo = info;
						client_data.m_AchievementStepInfo = step_info;
					}
				}
			}
		}

		if( container.closed_list != null )
		{
			HasUpdatedAchievementCount += container.closed_list.Count;
			for( int i = 0; i < container.closed_list.Count; i++ )
			{
				CS_AchievementCloseData close_data = container.closed_list[i];
				ClientData client_data = mCachedAchievelist.Find( a => a.achievement_idn == close_data.achievement_idn );
				TBL_Achievement info = TBL_AchievementManager.Instance.GetInfoByIntKey( close_data.achievement_idn );
				if( info != null )
				{
					TBL_Achievement.Step step_info = info.GetStepInfo( close_data.achieve_step, Version );
					if( step_info != null )
					{
						if( client_data == null )
						{
							client_data = new ClientData();
							client_data.achievement_idn = close_data.achievement_idn;
							mCachedAchievelist.Add( client_data );
						}

						client_data.achieve_step = close_data.achieve_step;
						client_data.progress_value = close_data.progress_value;
						client_data.is_completed = false;
						client_data.m_IsFinished = false;
						client_data.m_IsClosed = true;
						client_data.m_CloseProgressReward = close_data.progress_reward;
						client_data.m_AchievementInfo = info;
						client_data.m_AchievementStepInfo = step_info;
					}
				}
			}
		}
	}

	//------------------------------------------------------------------------	
	public void GetAchievementList( eAchievementCategory category, ref List<ClientData> list )
	{
		HasUpdatedAchievementCount = 0;

		List<TBL_Achievement> infolist = TBL_AchievementManager.Instance.GetInfoListByCategory( category );
		if( infolist != null && infolist.Count > 0 )
		{
			for( int i = 0; i < infolist.Count; i++ )
			{
				TBL_Achievement info = infolist[i];

				ClientData cached_data = mCachedAchievelist.Find( a => a.achievement_idn == info.IDN );
				if( cached_data != null )
				{
					if( cached_data.m_IsClosed && cached_data.m_CloseProgressReward == null )
						continue;

					list.Add( cached_data );
				}
				else
				{
					TBL_Achievement.Step stepInfo = info.GetStepInfo( 1, Version );
					if( stepInfo != null )
					{
						ClientData clientData = new ClientData();
						clientData.achievement_idn = info.IDN;
						clientData.achieve_step = 1;
						clientData.progress_value = 0;
						clientData.is_completed = false;
						clientData.m_IsFinished = false;
						clientData.m_IsClosed = false;
						clientData.m_AchievementInfo = info;
						clientData.m_AchievementStepInfo = stepInfo;

						list.Add( clientData );
					}
				}
			}
		}

		list = list.OrderBy( a => a.m_IsFinished )
			.ThenByDescending( a => a.is_completed )
			.ThenByDescending( a => a.m_IsClosed )
			.ThenByDescending( a => a.GetProgressValue() )
			.ThenBy( a => a.achievement_idn ).ToList();
	}

	//------------------------------------------------------------------------
	public ClientData GetAchievementData( int idn )
	{
		return mCachedAchievelist.Find( a => a.achievement_idn == idn );
	}

	//------------------------------------------------------------------------
	public void RemoveAchievementData( int idn )
	{
		mCachedAchievelist.RemoveAll( c => c.achievement_idn == idn );
	}
	public void RemoveAllClosedData()
	{
		mCachedAchievelist.RemoveAll( a => a.m_IsClosed );
	}

	//------------------------------------------------------------------------
	public int GetAlarmCount()
	{
		if( mCachedAchievelist.Count <= 0 )
			return 0;

		return mCachedAchievelist.Count( a => a.is_completed && a.m_IsFinished == false );
	}

	//------------------------------------------------------------------------	
	public string GetDescription( int achievement_idn, int achieve_step, bool with_color )
	{
		TBL_Achievement achievement_info = TBL_AchievementManager.Instance.GetInfoByIntKey( achievement_idn );
		if( achievement_info == null )
			return "";

		TBL_Achievement.Step step_info = achievement_info.GetStepInfo( achieve_step, Version );
		if( step_info == null )
			return "";

		string achievement_description = "";
		eAchieveGroup achievement_group_type = achievement_info.AchieveGroup;
		achievement_description = GlobalUtil.GetLocalizeText( achievement_info.DescriptionKey );

		switch( achievement_group_type )
		{
			case eAchieveGroup.LeaguePromotion:
				achievement_description = achievement_description.Replace( "[#v]", PlayerLeagueData.LeagueTierDivisionCommonName( step_info.LeagueTierType, step_info.LeagueDivision ) );
				break;

			default:
				if( with_color )
					achievement_description = achievement_description.Replace( "[#v]", UIColor.GetTextWithColorCode( step_info.AchieveValue.ToString( "N0" ), "QuestValue" ) );
				else
					achievement_description = achievement_description.Replace( "[#v]", step_info.AchieveValue.ToString( "N0" ) );
				break;
		}

		return achievement_description;
	}

	//------------------------------------------------------------------------
	public float GetAchievePercentage()
	{
		int all_steps = TBL_AchievementManager.Instance.Values.Sum( a => a.StepList.Count( b => b.Version <= Version ) );
		int curr_steps = mCachedAchievelist.Sum( a => ( a.is_completed || a.m_IsFinished ) ? a.achieve_step : a.achieve_step - 1 );

		return (float)curr_steps / (float)all_steps;
	}
}


