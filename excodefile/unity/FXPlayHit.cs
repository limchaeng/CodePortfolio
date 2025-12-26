//////////////////////////////////////////////////////////////////////////
//
// FXPlayHit
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 유니티 파티클 플레이 처리 : Hit 
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UMF.Unity;
using UnityEngine;

namespace U6FXV2
{
	public class FXPlayHit : FXPlayBase
	{
		[System.Serializable]
		public class HitData
		{
			public float m_HitCallbackTime;
			public AnimationClip m_HitAnimation;
		}

		public List<float> m_HitCallbackTimeList = new List<float>();


		public override FXMain.eFX_Type FXTYPE
		{
			get
			{
				if( mCustomFXType != FXMain.eFX_Type.None )
					return mCustomFXType;

				if( gameObject.name == FXMain.FHIT_OBJ_NAME )
					return FXMain.eFX_Type.FuncHit;

				return FXMain.eFX_Type.Hit;
			}
		}

		protected int mLastCallbackIdx = 0;
		protected float mNextCallbackTime = 0f;
		protected List<float> mSortedCallbackTimeList = null;

		protected FXMain.eFX_Type mCustomFXType = FXMain.eFX_Type.None;

		//------------------------------------------------------------------------
		public void Init( FXMain fx_main, FXMain.eFX_Type custom_fx_type )
		{
			mCustomFXType = custom_fx_type;
			mCallbackTimeInfoCached = null;
			base.Init( fx_main );
		}

		//------------------------------------------------------------------------
		string mCallbackTimeInfoCached = null;
		public string GetCallbackTimeInfo()
		{
			if( mCallbackTimeInfoCached != null )
				return mCallbackTimeInfoCached;

			if( m_HitCallbackTimeList.Count <= 0 )
			{
				mCallbackTimeInfoCached = "empty";
			}
			else
			{
				foreach( float time in m_HitCallbackTimeList )
				{
					mCallbackTimeInfoCached += $"[{time}]->";
				}
			}

			return mCallbackTimeInfoCached;
		}

		//------------------------------------------------------------------------
		protected override void PrePlay()
		{
			base.PrePlay();

			mSortedCallbackTimeList = m_HitCallbackTimeList.OrderBy( a => a ).ToList();
			if( mSortedCallbackTimeList == null )
				mSortedCallbackTimeList = new List<float>();

			mLastCallbackIdx = 0;
			mNextCallbackTime = 0f;

			if( mSortedCallbackTimeList.Count > 0 )
				mNextCallbackTime = mSortedCallbackTimeList[0];
		}

		//------------------------------------------------------------------------
		protected override void PlayBegin()
		{
			base.PlayBegin();
			if( mNextCallbackTime <= 0f && ( mSortedCallbackTimeList == null || mSortedCallbackTimeList.Count <= 0 ) )
			{
				if( mTarget != null )
					mTarget.IFXPlayTarget_OnHit( this );

				if( mMain != null )
					mMain.OnHit( this );
			}
		}

		//------------------------------------------------------------------------
		protected override void PlayingUpdate( float delta_time )
		{
			if( mNextCallbackTime > 0f )
			{
				if( mPlayTime >= mNextCallbackTime )
				{
					if( Application.isPlaying == false )
						Debug.Log( $"HIT callback time = {mPlayTime} - {mNextCallbackTime}" );

					if( mTarget != null )
						mTarget.IFXPlayTarget_OnHit( this );

					if( mMain != null )
						mMain.OnHit( this );

					mLastCallbackIdx += 1;
					if( mLastCallbackIdx < mSortedCallbackTimeList.Count )
					{
						mNextCallbackTime = mSortedCallbackTimeList[mLastCallbackIdx];
					}
					else
					{
						mNextCallbackTime = 0f;
					}
				}
			}
		}

		//------------------------------------------------------------------------
		public float MinHitCallbackTime()
		{
			if( mSortedCallbackTimeList != null && mSortedCallbackTimeList.Count > 0 )
				return mSortedCallbackTimeList[0];

			if( m_HitCallbackTimeList != null && m_HitCallbackTimeList.Count > 0 )
				return m_HitCallbackTimeList.Min( a => a );

			return 0f;
		}
	}
}