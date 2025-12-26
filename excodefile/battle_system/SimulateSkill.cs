//////////////////////////////////////////////////////////////////////////
//
// SimulateSkill
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 액티브 스킬의 시뮬레이션 클래스
//////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using U6Common;
using UMF.Core;

namespace DCBattle
{
	public class SimulateSkill
	{
		public enum eApplyType
		{
			None,
			Value,
			Percent,
			Count,
			SubName,
		}

		public class FuncFireData : IUMFObjectPoolData
		{
			// ....
		}

		public class Container : IUMFObjectPoolData
		{
			// ....
			Battle mBattle = null;
			public Battle SetBattle { set { mBattle = value; } }

			public List<FuncFireData> fire_data_list = null;

			public Dictionary<eCARD_STATE_TYPE, float> state_dic = null;
			public Dictionary<eFUNCTION_TYPE, float> rate_dic = null;
			public Dictionary<eFUNCTION_TYPE, float> value_dic = null;
			public Dictionary<eFUNCTION_TYPE, float> immune_dic = null;
			public Dictionary<eFUNCTION_TYPE, float> chance_dic = null;

			public float calc_add_hp = 0;
			public float calc_add_shield = 0;
			public float calc_add_corruption = 0;
			public float calc_solidarity_value = 0;

			// ....
			public void Init()
			{
				DataReset();
			}

			public void UnInit()
			{
				DataReset();
			}

			void DataReset()
			{
				// ....
				fire_data_list = null;

				state_dic = null;
				rate_dic = null;
				value_dic = null;
				immune_dic = null;
				chance_dic = null;

				calc_add_hp = 0;
				calc_add_shield = 0;
				calc_add_corruption = 0;
				calc_solidarity_value = 0;

				other_card_container_list = null;
			}

			public void SetState( eCARD_STATE_TYPE state_type, float value )
			{
				if( state_dic == null )
					state_dic = new Dictionary<eCARD_STATE_TYPE, float>();

				if( state_dic.ContainsKey( state_type ) )
				{
					if( value > state_dic[state_type] )
						state_dic[state_type] = value;
				}
				else
				{
					state_dic.Add( state_type, value );
				}
			}

			public void SetImmune( eFUNCTION_TYPE immune_func, float value )
			{
				if( immune_dic == null )
					immune_dic = new Dictionary<eFUNCTION_TYPE, float>();

				if( immune_dic.ContainsKey( immune_func ) )
				{
					if( value > immune_dic[immune_func] )
						immune_dic[immune_func] = value;
				}
				else
					immune_dic.Add( immune_func, value );
			}

			public void SetRate( eFUNCTION_TYPE func_type, float value )
			{
				if( rate_dic == null )
					rate_dic = new Dictionary<eFUNCTION_TYPE, float>();

				if( rate_dic.ContainsKey( func_type ) )
				{
					if( value > rate_dic[func_type] )
						rate_dic[func_type] = value;
				}
				else
				{
					rate_dic.Add( func_type, value );
				}
			}

			public void AddValue( eFUNCTION_TYPE func_type, float value )
			{
				if( value_dic == null )
					value_dic = new Dictionary<eFUNCTION_TYPE, float>();

				if( value_dic.ContainsKey( func_type ) )
				{
					value_dic[func_type] += value;
				}
				else
				{
					value_dic.Add( func_type, value );
				}
			}

			public void SetChance( eFUNCTION_TYPE func_type, float value )
			{
				if( chance_dic == null )
					chance_dic = new Dictionary<eFUNCTION_TYPE, float>();

				if( chance_dic.ContainsKey( func_type ) )
				{
					if( value > chance_dic[func_type] )
						chance_dic[func_type] = value;
				}
				else
				{
					chance_dic.Add( func_type, value );
				}
			}

			public void CalculateData()
			{
				if( target_card == null || fire_data_list == null || fire_data_list.Count <= 0 )
					return;

				foreach( FuncFireData f_data in fire_data_list )
				{
					eApplyType apply_type = f_data.value_type;
					eFUNCTION_TYPE func_type = f_data.sim_functype;

					// ....
				}
			}
		}

		Battle mBattle = null;
		UMFObjectPool<FuncFireData> mFuncFireDataPool = null;
		UMFObjectPool<Container> mContainerPool = null;

		public SimulateSkill( Battle battle )
		{
			mBattle = battle;
		}

		public void Destroy()
		{
			mBattle = null;

			if( mContainerPool != null )
				mContainerPool.Destroy();
			mContainerPool = null;

			if( mFuncFireDataPool != null )
				mFuncFireDataPool.Destroy();
			mFuncFireDataPool = null;
		}

		public FuncFireData GetFuncFireData()
		{
			if( mFuncFireDataPool == null )
				mFuncFireDataPool = new UMFObjectPool<FuncFireData>( 5 );

			return mFuncFireDataPool.Get();
		}

		public void ReturnFuncFireData( FuncFireData data )
		{
			if( mFuncFireDataPool != null )
				mFuncFireDataPool.Return( data );
		}

		public Container GetContainer( Card _target )
		{
			if( mContainerPool == null )
			{
				mContainerPool = new UMFObjectPool<Container>( 10, ( Container data ) =>
				{
					data.SetBattle = mBattle;
				} );
			}

			Container container = mContainerPool.Get();
			container.target_card = _target;

			return container;
		}

		public void ReturnContainer( Container container )
		{
			if( mContainerPool != null )
				mContainerPool.Return( container );
		}

		//------------------------------------------------------------------------
		public static eApplyType FuncApplyType( eFUNCTION_TYPE func_type )
		{
			switch( func_type )
			{
				// ....
				default:
					return eApplyType.Value;
			}
		}
	}
}