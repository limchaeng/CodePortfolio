//////////////////////////////////////////////////////////////////////////
//
// FXPlayBase
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 유니티 파티클 플레이 처리 : Cast, Hit, Projectile 등 해당 클래스 상속받아서 확장.
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UMF.Unity;
using UnityEngine;

namespace U6FXV2
{
	[ExecuteInEditMode]
	public abstract class FXPlayBase : MonoBehaviour
	{
		public ParticleSystem m_MainParticle;
		public float m_DelayTime = 0f;
		public bool m_UseDelayAddRandom = false;
		public Vector2 m_DelayTimeAddRandomRange = Vector2.zero;

		public abstract FXMain.eFX_Type FXTYPE { get; }

		// play
		protected bool mIsPlaying = false;
		public bool IsPlaying { get { return mIsPlaying; } }
		protected bool mIsPaused = false;
		public bool IsPaused { get { return mIsPaused; } }

		// time
		public float OverridePlayDelayTime { get; set; } = -1f;
		protected float mPlayTime = 0f;
		public float PlayTime { get { return mPlayTime; } }

		protected float mParticleMaxDuration = 0f;
		public float ParticleMaxDuration { get { return mParticleMaxDuration; } }

		protected float mMaxDuration = 0f;
		public float MaxDuration { get { return mMaxDuration; } }

		protected FXMain mMain = null;
		public FXMain GetFXMain { get { return mMain; } }

		// 
		protected float mPlayDelayTime = 0f;
		public float PlayDelayTime { get { return mPlayDelayTime; } }

		protected IFXPlayTarget mTarget = null;

		protected class ParticleSystemData
		{
			public ParticleSystem particle_system = null;
			public string original_layer_name = "";
			public int original_render_order = 0;
			public ParticleSystemRenderer renderer = null;
		}
		protected List<ParticleSystemData> mAllParticleSystemList = new List<ParticleSystemData>();
		public int ParticleSystemCount { get { return mAllParticleSystemList.Count; } }

		protected class ObjectCacheData
		{
			public GameObject game_object;
			public string original_layer_name;

			public ObjectCacheData( GameObject go )
			{
				game_object = go;
				original_layer_name = LayerMask.LayerToName( go.layer );
			}
		}
		protected List<ObjectCacheData> mAllGameObjectList = new List<ObjectCacheData>();
		protected List<Animator> mAllAnimatorList = new List<Animator>();

		string mSoundID = "";
		public string SoundID { get { return mSoundID; } set { mSoundID = value; } }

		//------------------------------------------------------------------------
		public virtual void Init( FXMain fx_main )
		{
			mMain = fx_main;
			mSoundID = $"{System.IO.Path.GetFileNameWithoutExtension( mMain.FX_ID )}_{FXTYPE}";
			fx_main.AddSoundID( mSoundID );

			mAllParticleSystemList.Clear();
			ParticleSystem[] ps_list = gameObject.GetComponentsInChildren<ParticleSystem>( true );
			if( ps_list.Length > 0 )
			{
				foreach( ParticleSystem ps in ps_list )
				{
					ParticleSystemRenderer renderer = ps.gameObject.GetComponent<ParticleSystemRenderer>();

					ParticleSystemData data = new ParticleSystemData();
					data.particle_system = ps;
					data.original_layer_name = "";
					data.original_render_order = 0;
					data.renderer = renderer;
					if( renderer != null )
					{
						data.original_layer_name = renderer.sortingLayerName;
						data.original_render_order = renderer.sortingOrder;
					}
					mAllParticleSystemList.Add( data );

					if( ps == m_MainParticle )
					{
						ParticleSystem.MainModule main = ps.main;
						main.playOnAwake = false;
					}
				}
			}

			mAllGameObjectList.Clear();
			mAllGameObjectList.Add( new ObjectCacheData( gameObject ) );
			List<Transform> child_obj = gameObject.transform.GetChildrenList( true, true );
			foreach( Transform t in child_obj )
			{
				mAllGameObjectList.Add( new ObjectCacheData( t.gameObject ) );
			}

			mAllAnimatorList.Clear();
			Animator[] anim_list = gameObject.GetComponentsInChildren<Animator>( true );
			if( anim_list.Length > 0 )
			{
				foreach( Animator anim in anim_list )
				{
					mAllAnimatorList.Add( anim );
					anim.gameObject.SetActive( false );
				}
			}
		}

		public virtual void UnInit()
		{
			mMain = null;
			mAllParticleSystemList.Clear();
			mAllGameObjectList.Clear();
			mAllAnimatorList.Clear();
		}

		//------------------------------------------------------------------------
		public virtual void Setup( IFXPlayTarget target )
		{
			OverridePlayDelayTime = -1f;
			mTarget = target;

			if( mTarget.IFXPlayTarget_GetFXParent != null && mMain.transform.parent != mTarget.IFXPlayTarget_GetFXParent.transform )
				transform.SetUniform( mTarget.IFXPlayTarget_GetFXParent );

			// update layer
			if( mTarget.IFXPlayTarget_Layer != FXMain.LAYER_DEFAULT )
			{
				foreach( ObjectCacheData obj in mAllGameObjectList )
				{
					if( obj.original_layer_name == FXMain.LAYER_DEFAULT )
						obj.game_object.layer = FXMain.FindLayerID( mTarget.IFXPlayTarget_Layer );
					else if( obj.original_layer_name == FXMain.LAYER_DEFAULT2D )
						obj.game_object.layer = FXMain.FindLayerID( mTarget.IFXPlayTarget_Layer2D );
				}
			}

			// update particle system orders
			foreach( ParticleSystemData data in mAllParticleSystemList )
			{
				if( data.renderer != null )
				{
					if( mTarget.IFXPlayTarget_Layer != FXMain.LAYER_DEFAULT )
					{
						if( data.original_layer_name == FXMain.LAYER_DEFAULT )
							data.renderer.sortingLayerName = mTarget.IFXPlayTarget_Layer;
						else if( data.original_layer_name == FXMain.LAYER_DEFAULT2D )
							data.renderer.sortingLayerName = mTarget.IFXPlayTarget_Layer2D;
					}

					if( data.original_render_order >= 0 )
					{
						int sorting_order_max = this.mTarget.IFXPlayTarget_GetSortingOrderMax( this );
						data.renderer.sortingOrder = sorting_order_max + data.original_render_order + 1;
					}
					else
						data.renderer.sortingOrder = this.mTarget.IFXPlayTarget_GetSortingOrderMin + data.original_render_order;
				}
			}

			// reset animator
			foreach( Animator anim in mAllAnimatorList )
			{
				anim.Rebind();
				anim.gameObject.SetActive( false );
			}
		}

		//------------------------------------------------------------------------
		public virtual void UnSet()
		{
			if( transform.parent != mMain.gameObject.transform )
				transform.SetUniform( mMain.gameObject );

			mTarget = null;
		}

		//------------------------------------------------------------------------
		[ContextMenu( "Update Time Set" )]
		public virtual void UpdateTimeSet()
		{
			mParticleMaxDuration = 0f;
			if( m_MainParticle != null )
				mParticleMaxDuration = m_MainParticle.CalcParticleMaxDuration( false );

			mMaxDuration = m_DelayTime + mParticleMaxDuration;
			if( OverridePlayDelayTime >= 0f )
				mMaxDuration = OverridePlayDelayTime + mParticleMaxDuration;

			if( m_UseDelayAddRandom )
				mMaxDuration += m_DelayTimeAddRandomRange.y;    //랜덤 최대값

			if( m_MainParticle != null && m_MainParticle.main.loop == true )
				mMaxDuration = -1f;
		}

		//------------------------------------------------------------------------
		protected virtual void PrePlay() { }
		protected virtual void PostPlay() { }
		protected virtual void PlayBegin()
		{
			if( mMain != null )
				mMain.OnPlayBegin( this );
		}

		//------------------------------------------------------------------------
		public void Play( bool ignore_delay = false )
		{
			if( mIsPlaying )
				Stop();

			PrePlay();
			UpdateTimeSet();
			//Debug.Log( $"~~~~~~~~~~ update time set : {mMaxDuration}" );

			mPlayTime = 0f;
			mPlayDelayTime = 0f;
			if( ignore_delay == false )
			{
				mPlayDelayTime = m_DelayTime;
				if( OverridePlayDelayTime >= 0f )
					mPlayDelayTime = OverridePlayDelayTime;

				if( m_UseDelayAddRandom )
					mPlayDelayTime += Random.Range( m_DelayTimeAddRandomRange.x, m_DelayTimeAddRandomRange.y );
			}

			mIsPlaying = true;
			mIsPaused = false;

			if( m_MainParticle != null )
				m_MainParticle.Clear();

			PostPlay();

			if( m_MainParticle != null && mPlayDelayTime <= 0f )
			{
				m_MainParticle.Play();
				mAllAnimatorList.ForEach( a => a.gameObject.SetActive( true ) );
				SoundSetting.Instance.PlayFX( mSoundID );
				PlayBegin();
			}
		}

		//------------------------------------------------------------------------
		public virtual void Pause( bool paused )
		{
			if( mIsPlaying == false )
				return;

			mIsPaused = paused;
			if( mIsPaused )
			{
				if( m_MainParticle != null )
					m_MainParticle.Pause();
			}
			else
			{
				if( m_MainParticle != null )
					m_MainParticle.Play();
			}
		}

		//------------------------------------------------------------------------
		public void Stop()
		{
			if( mIsPlaying )
			{
				if( m_MainParticle != null )
				{
					m_MainParticle.Stop();
					m_MainParticle.Clear();
				}

				mPlayTime = 0f;
				mPlayDelayTime = 0f;
				mIsPlaying = false;
				mIsPaused = false;

				// reset animator
				foreach( Animator anim in mAllAnimatorList )
				{
					anim.Rebind();
					anim.gameObject.SetActive( false );
				}

				PostStop();
			}
		}
		protected virtual void PostStop() { }

		//------------------------------------------------------------------------
		protected virtual void PlayingUpdate( float delta_time ) { }
		public void DoUpdate( float delta_time, bool do_not_stop_when_finished = false )
		{
			if( mIsPlaying && mIsPaused == false )
			{
				if( mPlayDelayTime > 0f )
				{
					mPlayDelayTime -= delta_time;
					if( mPlayDelayTime <= 0f )
					{
						mPlayDelayTime = 0f;

						if( Application.isPlaying )
						{
							if( m_MainParticle != null )
							{
								m_MainParticle.Play();
							}
						}

						mAllAnimatorList.ForEach( a => a.gameObject.SetActive( true ) );
						SoundSetting.Instance.PlayFX( mSoundID );
						PlayBegin();
					}
				}
				else
				{
					mPlayTime += delta_time;
				}

				if( Application.isPlaying == false )
				{
					if( m_MainParticle != null )
					{
						m_MainParticle.Simulate( mPlayTime );
					}
				}

				PlayingUpdate( delta_time );
				//Debug.Log( $"~~~~~~~ {gameObject.name} + delta:{delta_time} / max:{mMaxDuration} / pla:{mPlayTime}" );

				if( mMaxDuration >= 0f && mPlayTime >= mMaxDuration && do_not_stop_when_finished == false )
				{
					Stop();
				}
			}
		}

#if UNITY_EDITOR
		//------------------------------------------------------------------------
		public virtual void Editor_SetPlayTime( float time )
		{
			if( mIsPlaying && mIsPaused )
			{
				mPlayTime = time;
				mIsPaused = false;
				DoUpdate( 0f, true );
				mIsPaused = true;
			}
		}
#endif
	}
}