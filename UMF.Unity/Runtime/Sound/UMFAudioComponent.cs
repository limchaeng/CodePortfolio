//////////////////////////////////////////////////////////////////////////
//
// UMFAudioComponent
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

namespace UMF.Unity
{
	[RequireComponent( typeof( AudioSource ) )]
	public class UMFAudioComponent : MonoBehaviour
	{
		public delegate void delegateOnReleaseObject( UMFAudioComponent audio, bool interrupted );
		private delegateOnReleaseObject mReleaseHandler;
		public delegateOnReleaseObject ReleaseHandler { set { mReleaseHandler = value; } }

		public delegate void delegateOnAudioFinishNotify( UMFAudioComponent audio, bool interrupted );
		public delegateOnAudioFinishNotify AudioFinishNotify { get; set; } = null;

		public delegate void delegateOnTimeout( UMFAudioComponent audio, UMFAudioManager.AudioFade fade_flags );
		private delegateOnTimeout mTimeoutCallback = null;


		private bool mDestroyAfterStop = false;
		public bool IsDestroyAfterStop { set { mDestroyAfterStop = value; } get { return mDestroyAfterStop; } }

		private AudioSource mAudioSource = null;
		public AudioSource GetAudioSource
		{
			get
			{
				if( mAudioSource == null )
					mAudioSource = gameObject.GetComponent<AudioSource>();
				if( mAudioSource == null )
					mAudioSource = gameObject.AddComponent<AudioSource>();

				return mAudioSource;
			}
		}

		//------------------------------------------------------------------------
		private void Awake()
		{
			mAudioSource = gameObject.GetComponent<AudioSource>();
			if( mAudioSource == null )
				mAudioSource = gameObject.AddComponent<AudioSource>();
		}

		//-----------------------------------------------------------------------------
		public void FadeAudio( bool isIn, float duration, bool interrupted )
		{
			if( isIn )
				StartCoroutine( _FadeInAudio( duration ) );
			else
				StartCoroutine( _FadeOutAudio( duration, interrupted ) );
		}

		//-----------------------------------------------------------------------------
		private IEnumerator _FadeInAudio( float duration )
		{
			AudioSource source = gameObject.GetComponent<AudioSource>();
			if( source == null )
				yield break;

			float _endVol = source.volume;
			float beginTime = Time.unscaledTime;
			float endTime = beginTime + duration;
			float elapsedTime = 0.0f;

			source.volume = 0f;

			while( Time.unscaledTime < endTime )
			{
				elapsedTime = Time.unscaledTime - beginTime;
				source.volume = Mathf.Lerp( 0f, _endVol, ( elapsedTime / duration ) );
				yield return null;
			}

			source.volume = _endVol;
		}

		//-----------------------------------------------------------------------------
		private IEnumerator _FadeOutAudio( float duration, bool interrupted )
		{
			AudioSource source = gameObject.GetComponent<AudioSource>();
			if( source == null )
				yield break;

			float _startVol = source.volume;
			float beginTime = Time.unscaledTime;
			float endTime = beginTime + duration;
			float elapsedTime = 0.0f;

			while( Time.unscaledTime < endTime )
			{
				elapsedTime = Time.unscaledTime - beginTime;
				source.volume = Mathf.Lerp( _startVol, 0f, ( elapsedTime / duration ) );
				yield return null;
			}

			source.volume = 0f;

			if( mReleaseHandler != null )
				mReleaseHandler( this, interrupted );
			else
				Destroy( gameObject );
		}

		//------------------------------------------------------------------------
		Coroutine mTimeoutRoutine = null;
		public void SetTimeout( float timeout, UMFAudioManager.AudioFade fade_flags, delegateOnTimeout callback )
		{
			TimeoutStop();

			mTimeoutCallback = callback;
			mTimeoutRoutine = StartCoroutine( _TimeoutRoutine( timeout, fade_flags ) );
		}

		//-----------------------------------------------------------------------------
		private IEnumerator _TimeoutRoutine( float timeout, UMFAudioManager.AudioFade fade_flags )
		{
			yield return new WaitForSeconds( timeout );

			Debug.Log("** _TimeoutRoutine:" + this.gameObject.name);

			if( mTimeoutCallback != null )
				mTimeoutCallback( this, fade_flags );
		}

		//-----------------------------------------------------------------------------
		public void TimeoutStop()
		{
			mTimeoutCallback = null;

			if( mTimeoutRoutine != null )
				StopCoroutine( mTimeoutRoutine );

			mTimeoutRoutine = null;
		}
	}
}
