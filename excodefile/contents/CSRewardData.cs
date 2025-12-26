//////////////////////////////////////////////////////////////////////////
//
// CSRewardData
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 보상 데이터 구조 및 유틸
// - 전체 시스템의 보상의 구조를 통일하여 관리 및 획득,사용시 편리하게 사용되게 한다.
//////////////////////////////////////////////////////////////////////////

using DCBattle;
using System.Collections.Generic;
using UMF.Core;

namespace U6Common
{
	//------------------------------------------------------------------------
	public interface IRewardDataChecker
	{
		bool IRewardDataChecker_InvalidCheck( eRewardType r_type, int r_value, bool from_gacha );
	}

	//------------------------------------------------------------------------
	public class RewardDataManager : Singleton<RewardDataManager>
	{
		Dictionary<eRewardType, List<IRewardDataChecker>> mTypeChecker = new Dictionary<eRewardType, List<IRewardDataChecker>>();

		//------------------------------------------------------------------------
		public void AddChecker( eRewardType r_type, IRewardDataChecker checker )
		{
			// ....
		}

		//------------------------------------------------------------------------
		public bool InvalidCheck( CS_RewardData r_data, bool from_gacha )
		{
			return InvalidCheck( r_data.reward_type, r_data.reward_value, null, from_gacha );
		}
		public bool InvalidCheck( eRewardType r_type, int r_value, List<CS_ShopProductData> shop_list, bool from_gacha )
		{
			// ....

			return false;
		}

		//------------------------------------------------------------------------
		public bool IsInvalidCheckType( eRewardType r_type, int r_value )
		{
			bool need_check = false;
			switch( r_type )
			{
				case eRewardType.Item:
					{
						// ....
					}
					break;

				case eRewardType.CardIDN:
					need_check = true;
					break;
			}

			return need_check;
		}

		//------------------------------------------------------------------------
		public CS_RewardData ToConvertReward( CS_RewardData r_data )
		{
			return ToConvertReward( r_data.reward_type, r_data.reward_value, r_data.reward_count, r_data.reward_index );
		}
		public CS_RewardData ToConvertReward( eRewardType r_type, int r_value, int r_count, int r_index = 0 )
		{
			// ....
			return null;
		}
	}

	//------------------------------------------------------------------------
	public class CS_RewardData
	{
		public eRewardType reward_type;
		public int reward_value;
		public int reward_count;

		// for specific index
		public int reward_index = 0;

		// ....

		public CS_RewardData()
		{
			reward_type = eRewardType.None;
			reward_value = 0;
			reward_count = 0;
		}

		public CS_RewardData( eRewardType r_type, int r_value, int r_count, int r_index = 0 )
		{
			reward_type = r_type;
			reward_value = r_value;
			reward_count = r_count;

			reward_index = r_index;
		}

		public void Set( CS_RewardData cs_data )
		{
			this.reward_type = cs_data.reward_type;
			this.reward_value = cs_data.reward_value;
			this.reward_count = cs_data.reward_count;
			this.reward_index = cs_data.reward_index;
		}

		public CS_RewardData Copy()
		{
			CS_RewardData copy = new CS_RewardData();
			copy.Set( this );
			return copy;
		}

		public bool IsEqualType( CS_RewardData r_data )
		{
			if( this.reward_type == r_data.reward_type && this.reward_value == r_data.reward_value )
				return true;

			return false;
		}

		public override string ToString()
		{
			return string.Format( "{0}:{1}:{2}", reward_type, reward_value, reward_count );
		}

		public string GetCountText()
		{
			return reward_count.ToString( "N0" );
		}

		//////////////////////////////////////////////////////////////////////////
		// static utility
		static char[] _ParseRewardList = new char[] { '|' };
		static char[] _ParseRewardData = new char[] { ',' };

		/// <summary>
		///   reward_string = type[0],value[1],count[2],index[3]
		/// </summary>
		public static CS_RewardData ParseString( string r_parse )
		{
			string[] reward_parse = r_parse.Split( _ParseRewardData, System.StringSplitOptions.RemoveEmptyEntries );
			if( reward_parse == null || reward_parse.Length < 3 )
				return null;

			eRewardType r_type = StringUtil.SafeParse<eRewardType>( reward_parse[0], eRewardType.None );
			if( r_type == eRewardType.None )
				return null;

			int r_count = StringUtil.SafeParse<int>( reward_parse[2], 0 );
			if( r_count <= 0 )
				return null;

			int r_value = StringUtil.SafeParse<int>( reward_parse[1], 0 );

			int r_index = 0;
			if( reward_parse.Length > 3 )
				r_index = StringUtil.SafeParse<int>( reward_parse[3], 0 );

			return new CS_RewardData( r_type, r_value, r_count, r_index );
		}

		/// <summary>
		///   reward_string = type,value,count|type,value,count|type,value,count,index|...
		/// </summary>
		public static List<CS_RewardData> ParseMultipleString( string reward_string )
		{
			if( string.IsNullOrEmpty( reward_string ) )
				return null;

			List<CS_RewardData> list = null;

			string[] r_parse_array = reward_string.Split( _ParseRewardList, System.StringSplitOptions.RemoveEmptyEntries );
			if( r_parse_array == null || r_parse_array.Length <= 0 )
				return null;

			foreach( string r_parse in r_parse_array )
			{
				CS_RewardData cs_data = ParseString( r_parse );
				if( cs_data != null )
				{
					if( list == null )
						list = new List<CS_RewardData>();

					list.Add( cs_data );
				}
			}

			return list;
		}

		//------------------------------------------------------------------------
		public static bool IsPointType( eRewardType r_type )
		{
			// ....
		}

		//------------------------------------------------------------------------
		public static CS_RewardData ToRealRewardData( CS_RewardData r_data, eGameLevel game_level, bool to_convert = true )
		{
			return _ToRealRewardData( to_convert, r_data.reward_type, r_data.reward_value, r_data.reward_count, r_data.reward_index, game_level, null );
		}

		public static CS_RewardData ToRealRewardData( CS_RewardData r_data, eGameLevel game_level, List<CS_ShopProductData> shop_list, bool to_convert = true )
		{
			return _ToRealRewardData( to_convert, r_data.reward_type, r_data.reward_value, r_data.reward_count, r_data.reward_index, game_level, shop_list );
		}

		public static CS_RewardData ToRealRewardData( eRewardType r_type, int r_value, int r_count, eGameLevel game_level, bool to_convert = true, int r_index = 0 )
		{
			return _ToRealRewardData( to_convert, r_type, r_value, r_count, r_index, game_level, null );
		}
		static CS_RewardData _ToRealRewardData( bool to_convert, eRewardType r_type, int r_value, int r_count, int r_index, eGameLevel game_level, List<CS_ShopProductData> shop_list )
		{
			// ....
		}

		//------------------------------------------------------------------------
		public static List<CS_RewardData> ProcessRealRewards( List<CS_RewardData> org_list, eGameLevel game_level, ref List<CS_RewardData> save_list, ref bool need_save, bool to_convert = true )
		{
			// ....
			return real_list;
		}
	}
}
