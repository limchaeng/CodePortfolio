//////////////////////////////////////////////////////////////////////////
//
// UMFSoundManager
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMF.Core;

namespace UMF.Unity
{
	//------------------------------------------------------------------------
	public interface IUMFSoundSetting
	{
		string RESOURCE_PATH { get; }

		List<string> IUMFSoundSetting_PreloadPathList();
	}

	//------------------------------------------------------------------------	
	public class UMFSoundManager : Singleton<UMFSoundManager>
	{
		public enum eSoundType
		{
			Sound,
		}

		public class SoundData
		{
			eSoundType mSoundType = eSoundType.Sound;
			public eSoundType SoundType { get { return mSoundType; } }
			string mSoundID = "";
			public string SoundID { get { return mSoundID; } }
			AudioClip mClip = null;
			UMFAudioComponent mSoundComponent = null;

			Dictionary<string, UMFAudioComponent> mLoopSounds = new Dictionary<string, UMFAudioComponent>();
			public Dictionary<string, UMFAudioComponent> LoopSounds { get { return mLoopSounds; } }
			bool mPendingDestroy = false;
			public bool PendingDestroy { get { return mPendingDestroy; } }

			UMFSoundManager mManager = null;
			UMFAudioManager mAudioManager = null;

			float mOneShotSoundLastPlayTime = 0f;

			public SoundData( UMFSoundManager manager, UMFAudioManager audio_manager, string sound_id, AudioClip clip, eSoundType sound_type )
			{
				mManager = manager;
				mAudioManager = audio_manager;

				mSoundType = sound_type;
				mSoundID = sound_id;
				mClip = clip;
				mPendingDestroy = false;
			}

			//------------------------------------------------------------------------
			public float PlayOneShot()
			{
				if( mClip != null )
				{
					float curr_time = Time.unscaledTime;
					if( mAudioManager.m_OneShotSoundPlayOnceAtSameTime == false )
					{
						mAudioManager.PlayOneShotSound( mClip );
						mOneShotSoundLastPlayTime = curr_time;
					}
					else
					{
						if( mAudioManager.m_OneShotSoundPlayOnceAtSameTimeInterval > 0f )
						{
							if( curr_time - mOneShotSoundLastPlayTime > mAudioManager.m_OneShotSoundPlayOnceAtSameTimeInterval )
							{
								mAudioManager.PlayOneShotSound( mClip );
								mOneShotSoundLastPlayTime = curr_time;
							}
						}
						else if( curr_time != mOneShotSoundLastPlayTime )
						{
							mAudioManager.PlayOneShotSound( mClip );
							mOneShotSoundLastPlayTime = curr_time;
						}
					}

					return mClip.length;
				}

				return 0f;
			}

			//------------------------------------------------------------------------
			public void PlayLoop( object loop_id, float duration, float fade_duration = 0f )
			{
				if( mClip != null )
				{
					string loopid = loop_id.ToString();
					if( mLoopSounds.ContainsKey( loopid ) )
					{
						StopLoop( loop_id, true );
					}

					UMFAudioComponent audio_comp = mAudioManager.PlaySoundLoop( mClip, duration, fade_duration );
					mLoopSounds.Add( loopid, audio_comp );
				}
			}

			public void StopLoop( object loop_id, bool immediate, float fadetime = 0.5f )
			{
				string loopid = loop_id.ToString();
				if( mLoopSounds.ContainsKey( loopid ) )
				{
					UMFAudioComponent audio_comp = mLoopSounds[loopid];
					mLoopSounds.Remove( loopid );

					mAudioManager.StopAudio( audio_comp, immediate, fadetime );
				}
			}

			public void StopAllLoop()
			{
				foreach( UMFAudioComponent audio_comp in mLoopSounds.Values )
				{
					mAudioManager.StopAudio( audio_comp, true, 0f );
				}
				mLoopSounds.Clear();
			}

			//------------------------------------------------------------------------			
			public float PlayVoice()
			{
				StopVoice();
				if( mClip != null )
				{
					mSoundComponent = mAudioManager.PlaySoundPool( mClip );
					return mClip.length;
				}

				return 0f;
			}

			public void StopVoice()
			{
				if( mSoundComponent != null )
				{
					mAudioManager.StopAudio( mSoundComponent, true, 0f );
					mSoundComponent = null;
				}
			}

			//------------------------------------------------------------------------			
			public void PlayMusic( UMFAudioManager.AudioFade fadeflag = UMFAudioManager.AudioFade.Both, float fadetime = 1f )
			{
				if( mSoundComponent != null && mSoundComponent.GetComponent<AudioSource>() != null && mSoundComponent.GetComponent<AudioSource>().isPlaying )
					return;

				StopMusic( true );
				if( mClip != null )
				{
					mSoundComponent = mAudioManager.PlayMusic( mClip, true, true, fadeflag, fadetime, null );
				}
			}

			public void StopMusic( bool immediate = true, float fade_time = 0.5f, bool destroy_finished = false )
			{
				if( mSoundComponent != null )
				{
					if( immediate == false && destroy_finished == true )
					{
						UMFAudioComponent audioComp = mSoundComponent.GetComponent<UMFAudioComponent>();
						if( audioComp != null )
						{
							mPendingDestroy = true;
							audioComp.AudioFinishNotify = OnAudioFinished;
						}
					}

					mAudioManager.StopAudio( mSoundComponent, immediate, fade_time );
					mSoundComponent = null;
				}

				if( destroy_finished )
					mManager.RemoveSound( this, immediate );
			}

			void OnAudioFinished( UMFAudioComponent audio_comp, bool interrupted )
			{
				mManager.DestroyPending( this );
			}

			public void Destroy()
			{
				StopAllLoop();
				if( mSoundComponent != null )
				{
					mAudioManager.StopAudio( mSoundComponent, true, 0f );
					mSoundComponent = null;
				}

				if( mClip != null )
				{
					// TODO
				}

				mClip = null;
			}
		}

		IUMFSoundSetting mSetting = null;

		Dictionary<string, SoundData> mPreloadedSounds = new Dictionary<string, SoundData>();
		Dictionary<string, SoundData> mLoadedSounds = new Dictionary<string, SoundData>();
		List<SoundData> mDestoryPendingList = new List<SoundData>();

		//------------------------------------------------------------------------	
		public void Init( IUMFSoundSetting setting )
		{
			mSetting = setting;
			Preload();
		}

		//------------------------------------------------------------------------	
		public void Clear()
		{
			foreach( SoundData data in mLoadedSounds.Values )
			{
				data.Destroy();
			}
			mLoadedSounds.Clear();

			foreach( SoundData data in mPreloadedSounds.Values )
			{
				data.Destroy();
			}
			mPreloadedSounds.Clear();
		}

		//------------------------------------------------------------------------	
		public void Preload()
		{
			List<string> sound_preload_paths = mSetting.IUMFSoundSetting_PreloadPathList();
			if( sound_preload_paths == null )
				return;

			foreach( string path in sound_preload_paths )
			{
				AudioClip clip = Resources.Load<AudioClip>( path );
				if( clip != null )
				{
					//Debug.LogFormat( "## SOUND Pre Loaded from Resource:{0}", path );
					if( mPreloadedSounds.ContainsKey( clip.name ) == false )
					{
						mPreloadedSounds.Add( path, new SoundData( this, UMFAudioManager.Instance, clip.name, clip, eSoundType.Sound ) );
					}
					else
					{
						clip = null;
					}
				}
				else
				{
					Debug.LogWarningFormat( "## SOUND Cannot Pre Load from Resource:{0}", path );
				}
			}
		}

		//------------------------------------------------------------------------	
		public void Load( List<string> sound_paths )
		{
			if( sound_paths == null )
				return;

			for( int i = 0; i < sound_paths.Count; i++ )
			{
				string id_path = sound_paths[i];

				if( mPreloadedSounds.ContainsKey( id_path ) )
					continue;

				if( mLoadedSounds.ContainsKey( id_path ) )
					continue;

				AudioClip clip = Resources.Load<AudioClip>( id_path );
				if( clip != null )
				{
					//Debug.LogFormat( "## SOUND Loaded from Resource:{0}", id_path );
					mLoadedSounds.Add( id_path, new SoundData( this, UMFAudioManager.Instance, id_path, clip, eSoundType.Sound ) );
				}
				else
				{
					Debug.LogWarningFormat( "## SOUND Cannot Load from Resource:{0}", id_path );
				}
			}
		}

		//------------------------------------------------------------------------	
		public void Unload( List<string> sound_ids )
		{
			if( sound_ids == null )
				return;

			for( int i = 0; i < sound_ids.Count; i++ )
			{
				string sound_id = sound_ids[i].ToString();
				if( mLoadedSounds.ContainsKey( sound_id ) )
				{
					SoundData s_data = mLoadedSounds[sound_id];
					if( s_data.LoopSounds.Count > 0 )
					{
						Debug.Log( $"Sound [{sound_id}] Unload : has loop sound. unload ignored." );
						continue;
					}

					if( mLoadedSounds[sound_id].PendingDestroy == false )
					{
						mLoadedSounds[sound_id].Destroy();
						mLoadedSounds.Remove( sound_id );
					}
				}
			}
		}
		public void RemoveSound( SoundData data, bool immediate )
		{
			Debug.Log( string.Format( "-- SoundManager : auto destroyed:{0} / {1}", data.SoundID, immediate ) );

			if( data.SoundType == eSoundType.Sound )
				mLoadedSounds.Remove( data.SoundID );

			if( immediate )
			{
				data.Destroy();
			}
			else
			{
				mDestoryPendingList.Add( data );
			}
		}
		public void DestroyPending( SoundData data )
		{
			data.Destroy();
			mDestoryPendingList.Remove( data );
		}

		public SoundData FindSoundData( string sound_id )
		{
			SoundData s_data;
			if( mPreloadedSounds.TryGetValue( sound_id, out s_data ) )
			{
				return s_data;
			}

			if( mLoadedSounds.TryGetValue( sound_id, out s_data ) )
			{
				return s_data;
			}

			return null;
		}

		//------------------------------------------------------------------------	
		public float PlayVoice( object voice )
		{
			if( voice == null )
				return 0f;

			string sound_id = voice.ToString();

			SoundData s_data = FindSoundData( sound_id );
			if( s_data != null )
				return s_data.PlayVoice();

			return 0f;
		}

		//------------------------------------------------------------------------	
		public void StopVoice( object voice )
		{
			if( voice == null )
				return;

			string id = voice.ToString();

			SoundData s_data = FindSoundData( id );
			if( s_data != null )
				s_data.StopVoice();
		}

		//------------------------------------------------------------------------	
		public float PlayOneShot( object sound )
		{
			if( sound == null )
				return 0f;

			string sound_id = sound.ToString();

			SoundData s_data = FindSoundData( sound_id );
			if( s_data != null )
				return s_data.PlayOneShot();

			return 0f;
		}

		//------------------------------------------------------------------------	
		public void PlayLoop( object sound, object loop_id, float fade_duration = 0f,  float duration = 0f )
		{
			if( sound == null )
				return;

			string sound_id = sound.ToString();

			SoundData s_data = FindSoundData( sound_id );
			if( s_data != null )
				s_data.PlayLoop( loop_id, duration, fade_duration );
		}
		public void StopLoop( object sound, object loop_id, bool immediate = true, float fadetime = 0.5f )
		{
			if( sound == null )
				return;

			string sound_id = sound.ToString();
			SoundData s_data = FindSoundData( sound_id );
			if( s_data != null )
				s_data.StopLoop( loop_id, immediate, fadetime );
		}

		//------------------------------------------------------------------------	
		public void PlayMusic( object sound, UMFAudioManager.AudioFade fadeflag = UMFAudioManager.AudioFade.Both, float fadetime = 1f )
		{
			if( sound == null )
				return;

			string sound_id = sound.ToString();

			SoundData s_data = FindSoundData( sound_id );
			if( s_data != null )
				s_data.PlayMusic( fadeflag, fadetime );
		}
		public void StopMusic( object sound, bool immediate, bool destroy_finished, float fadetime = 0.5f )
		{
			if( sound == null )
				return;

			string sound_id = sound.ToString();
			SoundData s_data = FindSoundData( sound_id );
			if( s_data != null )
				s_data.StopMusic( immediate, fadetime, destroy_finished );
		}
	}
}
