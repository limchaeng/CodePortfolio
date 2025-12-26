//////////////////////////////////////////////////////////////////////////
//
// UMFAudioManager
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

using System.Collections.Generic;
using UnityEngine;

namespace UMF.Unity
{
	public class UMFAudioManager : SingletonBehaviour<UMFAudioManager>
	{
		public enum AudioFade
		{
			None = 0x00,
			In = 0x01,
			Out = 0x02,
			Both = In | Out,
		}

		public float m_FadeInDuration = 2.0f;
		public float m_FadeOutDuration = 2.0f;
		public int m_AUDIOSourceCount = 2;
		public int m_OneShotSoundSourceCount = 2;
		public bool m_OneShotSoundPlayOnceAtSameTime = true;
		public float m_OneShotSoundPlayOnceAtSameTimeInterval = 0f;

		private Queue<UMFAudioComponent> mAudioObjectQueue = new Queue<UMFAudioComponent>();

		private List<UMFAudioComponent> mSoundPoolObjectList = new List<UMFAudioComponent>();  // for all
		private List<UMFAudioComponent> mMusicObjectList = new List<UMFAudioComponent>();
		private List<UMFAudioComponent> mOneShotSoundList = new List<UMFAudioComponent>();
		private int mOneShotSoundIndex = 0;

		public bool MusicMute { get; private set; } = false;
		public bool SoundMute { get; private set; } = false;
		public float MusicVolume { get; private set; } = 1f;
		public float SoundVolume { get; private set; } = 1f;


		public bool AudioSessionEnabled = true;
		private bool mIsAudioLoaded = false;

		protected override void Awake()
		{
			base.Awake();

			// Sound source is always live...
			for( int i = 0; i < m_OneShotSoundSourceCount; i++ )
			{
				GameObject go = new GameObject( string.Format( "OneShotSound-{0}", i ) );
				go.transform.SetUniform( gameObject );

				UMFAudioComponent audio_comp = go.AddComponent<UMFAudioComponent>();
				audio_comp.GetAudioSource.volume = AudioListener.volume;
				audio_comp.GetAudioSource.playOnAwake = false;

				mOneShotSoundList.Add( audio_comp );
			}

			AudioLoad();
		}

		void OnDestroy()
		{
			UnInit();
		}

		public void UnInit()
		{
			foreach( UMFAudioComponent audio_comp in mMusicObjectList )
			{
				if( audio_comp != null )
				{
					Destroy( audio_comp.gameObject );
				}
			}
			mMusicObjectList.Clear();

			AudioUnload();

			mOneShotSoundList.Clear();
			mSoundPoolObjectList.Clear();
			mAudioObjectQueue.Clear();
		}

		//-----------------------------------------------------------------------------
		public void AudioLoad()
		{
			if( mIsAudioLoaded )
				return;

			for( int i = 0; i < m_AUDIOSourceCount; i++ )
			{
				UMFAudioComponent audioObj = CreateAudioComponent( false );
				if( audioObj != null )
					mAudioObjectQueue.Enqueue( audioObj );
			}

			mIsAudioLoaded = true;

			SoundVolume = 1f;
			MusicVolume = 1f;
		}

		//-----------------------------------------------------------------------------
		private void ReleaseAudioObjectHandler( UMFAudioComponent audio, bool interrupted )
		{
			ReleaseAudioObject( audio, interrupted );
		}

		//-----------------------------------------------------------------------------
		public void ReleaseAudioObject( UMFAudioComponent audio_comp, bool interrupted )
		{
			if( audio_comp == null )
				return;

			//Debug.Log(string.Format("--- ReleaseAudioObject count = {0}, {1}", mMusicObjectList.Count, mAudioObjectQueue.Count));
			mMusicObjectList.Remove( audio_comp );
			mSoundPoolObjectList.Remove( audio_comp );

			audio_comp.TimeoutStop();
			if( audio_comp.AudioFinishNotify != null )
			{
				audio_comp.AudioFinishNotify( audio_comp, interrupted );
				audio_comp.AudioFinishNotify = null;
			}

			if( audio_comp.IsDestroyAfterStop )
			{
				audio_comp.GetAudioSource.Stop();
				audio_comp.GetAudioSource.enabled = false;
				audio_comp.GetAudioSource.clip = null;
				GameObject.Destroy( audio_comp.gameObject );
				return;
			}

			audio_comp.GetAudioSource.Stop();
			audio_comp.GetAudioSource.clip = null;
			audio_comp.GetAudioSource.enabled = false;

			audio_comp.gameObject.name = "AudioSource";

			mAudioObjectQueue.Enqueue( audio_comp );
			//Debug.Log(string.Format("ReleaseAudioObject count = {0}, {1}---", mMusicObjectList.Count, mAudioObjectQueue.Count));
		}

		//-----------------------------------------------------------------------------
		public void AllMusicStop( bool immediate )
		{
			// fadeout music
			foreach( UMFAudioComponent audio_comp in mMusicObjectList )
				StopAudio( audio_comp, immediate, 0f );

			mMusicObjectList.Clear();
		}

		//-----------------------------------------------------------------------------
		public void AudioUnload()
		{
			// release queue
			while( mAudioObjectQueue.Count > 0 )
			{
				UMFAudioComponent audio_comp = mAudioObjectQueue.Dequeue();
				audio_comp.GetAudioSource.Stop();
				audio_comp.GetAudioSource.enabled = false;
				audio_comp.GetAudioSource.clip = null;
				Destroy( audio_comp.gameObject );
			}

			// release loop sound
			foreach( UMFAudioComponent audio_comp in mSoundPoolObjectList )
			{
				audio_comp.GetAudioSource.Stop();
				audio_comp.GetAudioSource.enabled = false;
				audio_comp.GetAudioSource.clip = null;
				Destroy( audio_comp.gameObject );
			}
			mSoundPoolObjectList.Clear();

			mIsAudioLoaded = false;
		}

		//-----------------------------------------------------------------------------
		public void StopAudio( UMFAudioComponent audio, bool immediate, float fadeout_duration )
		{
			if( immediate )
			{
				ReleaseAudioObject( audio, true );
			}
			else
			{
				if( fadeout_duration > 0f )
					FadeOutAudio( audio, fadeout_duration, true );
				else
					FadeOutAudio( audio, m_FadeOutDuration, true );
			}
		}

		//-----------------------------------------------------------------------------
		// for Music Play
		//-----------------------------------------------------------------------------
		public UMFAudioComponent PlayMusic( AudioClip musicClip )
		{
			return PlayMusic( musicClip, true, false, AudioFade.None, 0f, null );
		}

		//-----------------------------------------------------------------------------
		public UMFAudioComponent PlayMusic( AudioClip musicClip, UMFAudioComponent.delegateOnAudioFinishNotify audioFinishNotify )
		{
			return PlayMusic( musicClip, true, false, AudioFade.None, 0f, audioFinishNotify );
		}

		//-----------------------------------------------------------------------------
		public UMFAudioComponent PlayMusic( string musicClipName, bool autoPlay, bool isLoop, AudioFade fadeFlag, float fade_duration, UMFAudioComponent.delegateOnAudioFinishNotify audioFinishNotify )
		{
			AudioClip ac = Resources.Load( musicClipName, typeof( AudioClip ) ) as AudioClip;
			if( ac != null )
			{
				return PlayMusic( ac, autoPlay, isLoop, fadeFlag, fade_duration, audioFinishNotify );
			}

			return null;
		}

		//-----------------------------------------------------------------------------
		public UMFAudioComponent PlayMusic( AudioClip musicClip, bool autoPlay, bool isLoop, AudioFade fadeFlag, float fade_duration, UMFAudioComponent.delegateOnAudioFinishNotify audioFinishNotify )
		{
			if( musicClip == null )
			{
				return null;
			}

			return PlayMusic( musicClip, autoPlay, isLoop, fadeFlag, fade_duration, 0.0f, audioFinishNotify );
		}

		//-----------------------------------------------------------------------------
		public UMFAudioComponent PlayMusic( AudioClip musicClip, bool autoPlay, bool isLoop, AudioFade fadeFlag, float fade_duration, float duration, UMFAudioComponent.delegateOnAudioFinishNotify audioFinishNotify )
		{
			if( AudioSessionEnabled == false )
				return null;

			UMFAudioComponent audio_comp = CreateAudioObject( true, audioFinishNotify );  // music is always create and destroy
			if( audio_comp != null && audio_comp.GetAudioSource != null )
			{
				audio_comp.name = string.Format( "Music-{0}", musicClip.name );

				audio_comp.GetAudioSource.mute = MusicMute;
				audio_comp.GetAudioSource.enabled = true;
				audio_comp.GetAudioSource.loop = isLoop;
				audio_comp.GetAudioSource.clip = musicClip;
				audio_comp.GetAudioSource.volume = MusicVolume;
				if( autoPlay )
					audio_comp.GetAudioSource.Play();

				if( ( fadeFlag & AudioFade.In ) != 0 )
				{
					if( fade_duration > 0f )
						FadeInAudio( audio_comp, fade_duration );
					else
						FadeInAudio( audio_comp, m_FadeInDuration );
				}

				// for all stop
				mMusicObjectList.Add( audio_comp );

				if( isLoop == false || duration > 0.0f )
				{
					float playTime = ( duration > 0.0f ? duration : musicClip.length );
					if( ( fadeFlag & AudioFade.Out ) != 0 )
					{
						float fadeout_time = ( fade_duration > 0f ? fade_duration : m_FadeOutDuration );

						if( playTime > fadeout_time )
							audio_comp.SetTimeout( ( playTime - fadeout_time ), fadeFlag, OnPlayTimeoutHandler );
						else
							audio_comp.SetTimeout( playTime, fadeFlag, OnPlayTimeoutHandler );
					}
					else
					{
						audio_comp.SetTimeout( playTime, fadeFlag, OnPlayTimeoutHandler );
					}
				}

				return audio_comp;
			}

			return null;

		}

		//-----------------------------------------------------------------------------
		private void OnPlayTimeoutHandler( UMFAudioComponent audio_comp, AudioFade fade_flags )
		{
			//Debug.Log(string.Format("=== OnPlayTimeoutHandler : {0}", sender.gameObject.audio.clip.name));

			if( audio_comp.GetAudioSource.enabled == true )
			{
				if( ( fade_flags & AudioFade.Out ) != 0 )
				{
					FadeOutAudio( audio_comp, m_FadeOutDuration, false );
				}
				else
				{
					StopAudio( audio_comp, true, 0f );
				}
			}
		}

		//-----------------------------------------------------------------------------
		public void PlayOneShotSound( AudioClip SoundClip )
		{
			PlayOneShotSound( SoundClip, 1.0f );
		}

		//-----------------------------------------------------------------------------
		public void PlayOneShotSound( AudioClip SoundClip, float pitch )
		{
			if( AudioSessionEnabled == false || SoundVolume == 0f || SoundMute == true )
				return;

			if( SoundClip != null && mOneShotSoundList.Count > 0 )
			{
				AudioSource _audioSource = mOneShotSoundList[mOneShotSoundIndex].GetAudioSource;
				_audioSource.pitch = pitch;
				_audioSource.PlayOneShot( SoundClip );

				mOneShotSoundIndex++;
				if( mOneShotSoundIndex >= mOneShotSoundList.Count )
					mOneShotSoundIndex = 0;
			}
		}

		//-----------------------------------------------------------------------------
		public UMFAudioComponent PlaySoundLoop( AudioClip SoundClip, float duration, float fade_duration = 0f )
		{
			if( AudioSessionEnabled == false )
				return null;

			UMFAudioComponent audio_comp = PrepareAudioObject();      // Sound loop is always getting in pool
			if( audio_comp != null && audio_comp.GetAudioSource != null )
			{
				audio_comp.gameObject.name = string.Format( "SoundLoop-{0}", SoundClip.name );

				AudioSource _audioSource = audio_comp.GetAudioSource;
				_audioSource.mute = SoundMute;
				_audioSource.volume = SoundVolume;
				_audioSource.enabled = true;
				_audioSource.loop = true;
				_audioSource.clip = SoundClip;
				_audioSource.Play();

				// for allstop
				mSoundPoolObjectList.Add( audio_comp );

				if( fade_duration > 0f )
				{
					FadeInAudio( audio_comp, fade_duration );
				}

				if( duration > 0f )
				{
					float playTime = duration;
					audio_comp.SetTimeout( playTime, AudioFade.None, OnPlayTimeoutHandler );
				}
			}

			return audio_comp;
		}

		//-----------------------------------------------------------------------------
		public UMFAudioComponent PlaySoundPool( AudioClip clip )
		{
			if( AudioSessionEnabled == false )
				return null;

			UMFAudioComponent audio_comp = PrepareAudioObject();      // Sound loop is always getting in pool
			if( audio_comp != null && audio_comp.GetAudioSource != null )
			{
				audio_comp.name = string.Format( "Audio-{0}", clip.name );

				AudioSource _audioSource = audio_comp.GetAudioSource;
				_audioSource.mute = SoundMute;
				_audioSource.volume = SoundVolume;
				_audioSource.enabled = true;
				_audioSource.loop = false;
				_audioSource.clip = clip;
				_audioSource.Play();

				// for all stop
				mSoundPoolObjectList.Add( audio_comp );

				audio_comp.SetTimeout( clip.length, AudioFade.None, OnPlayTimeoutHandler );
			}

			return audio_comp;
		}

		//-----------------------------------------------------------------------------
		public void FadeInAudio( UMFAudioComponent audio_comp, float duration )
		{
			if( audio_comp != null && audio_comp.GetAudioSource != null && duration > 0.0f )
			{
				audio_comp.FadeAudio( true, duration, false );
			}
		}

		//-----------------------------------------------------------------------------
		public void FadeOutAudio( UMFAudioComponent audio_comp, float duration, bool interrupted )
		{
			if( audio_comp != null && audio_comp.GetAudioSource != null && duration > 0.0f )
			{
				audio_comp.FadeAudio( false, duration, interrupted );
			}
		}

		//-----------------------------------------------------------------------------
		private UMFAudioComponent PrepareAudioObject()
		{
			if( mAudioObjectQueue.Count > 0 )
				return mAudioObjectQueue.Dequeue();

			return CreateAudioComponent( false );
		}

		//-----------------------------------------------------------------------------
		private UMFAudioComponent CreateAudioComponent( bool destroyAfterStop )
		{
			return CreateAudioObject( destroyAfterStop, null );
		}
		//-----------------------------------------------------------------------------
		private UMFAudioComponent CreateAudioObject( bool destroyAfterStop, UMFAudioComponent.delegateOnAudioFinishNotify audioFinishNotify )
		{
			GameObject obj = new GameObject( "AudioSource" );
			obj.transform.SetUniform( gameObject );

			UMFAudioComponent audio_comp = obj.AddComponent<UMFAudioComponent>();
			if( audio_comp != null )
			{
				audio_comp.IsDestroyAfterStop = destroyAfterStop;
				audio_comp.ReleaseHandler = ReleaseAudioObjectHandler;
				audio_comp.AudioFinishNotify = audioFinishNotify;
			}

			AudioSource source = audio_comp.GetAudioSource;
			source.enabled = false;
			source.playOnAwake = false;
			return audio_comp;
		}

		//-----------------------------------------------------------------------------
		public void SetMusicVolume( float volume )
		{
			float vol = Mathf.Clamp( volume, 0.0f, 1.0f );
			MusicVolume = vol;

			mMusicObjectList.ForEach( a => a.GetAudioSource.volume = vol );
		}

		//-----------------------------------------------------------------------------
		public void SetSoundVolume( float volume )
		{
			float vol = Mathf.Clamp( volume, 0.0f, 1.0f );
			SoundVolume = vol;

			mOneShotSoundList.ForEach( a => a.GetAudioSource.volume = vol );
			mSoundPoolObjectList.ForEach( a => a.GetAudioSource.volume = vol );
		}

		//-----------------------------------------------------------------------------
		public void SetMusicMute( bool bMute )
		{
			MusicMute = bMute;
			mMusicObjectList.ForEach( a => a.GetAudioSource.mute = bMute );
		}

		//-----------------------------------------------------------------------------
		public void StopAllOneShotSound()
		{
			mOneShotSoundList.ForEach( a => a.GetAudioSource.Stop() );
		}

		public bool IsPlayingMusic()
		{
			Debug.Log( string.Format( "mMusicObjectList.Count = {0}", mMusicObjectList.Count ) );
			return ( mMusicObjectList.Count > 0 );
		}
		//-----------------------------------------------------------------------------
		public void SetSoundMute( bool bMute )
		{
			SoundMute = bMute;
			mOneShotSoundList.ForEach( a => a.GetAudioSource.mute = bMute );
			mSoundPoolObjectList.ForEach( a => a.GetAudioSource.mute = bMute );
		}
	}
}
