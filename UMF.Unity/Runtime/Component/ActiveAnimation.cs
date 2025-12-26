//////////////////////////////////////////////////////////////////////////
//
// ActiveAnimation
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
using System.Linq;
using UnityEngine;

namespace UMF.Unity
{
    // legacy animation mode only
    public class ActiveAnimation : MonoBehaviour
    {
        public enum Direction
        {
            Reverse = -1,
            Toggle = 0,
            Forward = 1,
        }

        public Animation m_Animation;
        public float m_Delay = 0f;
        public float m_Speed = 1f;

        public System.Action OnPlayCallback { get; set; } = null;
        public System.Action<string, int, float> OnAnimationEventCallback { get; set; } = null;

        float mDelayRuntime = 0f;
        float mPlayTime = 0f;
        System.Action mOnFinishedCallback = null;
        Direction mLastDirection = Direction.Toggle;

        class AEventData
        {
            public AnimationEvent a_event = null;
            public bool is_fired = false;
        }

        List<AEventData> mAnimEvents = new List<AEventData>();

        //------------------------------------------------------------------------
        private void Awake()
        {
            if( m_Animation == null )
                m_Animation = gameObject.GetComponent<Animation>();
        }

        //------------------------------------------------------------------------
        public float GetMaxTime()
        {
            if( m_Animation == null )
                return 0f;

            float max_time = 0f;
            foreach( AnimationState state in m_Animation )
            {
                if( state.length > max_time )
                    max_time = state.length;
            }

            return ( max_time / m_Speed ) + m_Delay;
        }

        //------------------------------------------------------------------------		
        public bool isPlaying
        {
            get
            {
                if( mDelayRuntime > 0f )
                    return true;

                if( m_Animation == null )
                    return false;

                foreach( AnimationState state in m_Animation )
                {
                    if( !m_Animation.IsPlaying( state.name ) )
                        continue;

                    if( mLastDirection == Direction.Forward )
                    {
                        if( state.time < state.length )
                            return true;
                    }
                    else if( mLastDirection == Direction.Reverse )
                    {
                        if( state.time > 0f )
                            return true;
                    }
                    else
                        return true;
                }
                return false;
            }
        }

        //------------------------------------------------------------------------
        public void Finish()
        {
            mDelayRuntime = 0f;

            if( m_Animation != null )
            {
                foreach( AnimationState state in m_Animation )
                {
                    if( mLastDirection == Direction.Forward )
                        state.time = state.length;
                    else if( mLastDirection == Direction.Reverse )
                        state.time = 0f;
                }
                m_Animation.Sample();
            }
        }

        //------------------------------------------------------------------------
        void OnAnimationEvent( string p_str, int p_int, float p_float )
        {
            if( OnAnimationEventCallback != null )
                OnAnimationEventCallback( p_str, p_int, p_float );
        }

        //------------------------------------------------------------------------		
        public void Reset()
        {
            mDelayRuntime = m_Delay;
            mPlayTime = 0f;
            mAnimEvents.Clear();

            if( m_Animation != null )
            {
                foreach( AnimationState state in m_Animation )
                {
                    if( mLastDirection == Direction.Reverse )
                        state.time = state.length;
                    else if( mLastDirection == Direction.Forward )
                        state.time = 0f;
                }
            }
        }

        //------------------------------------------------------------------------		
        void Update()
        {
            float delta = Time.deltaTime * m_Speed;
            if( delta == 0f ) return;

            if( mDelayRuntime > 0f )
            {
                mDelayRuntime -= delta;
                return;
            }

            if( m_Animation != null )
            {
                bool playing = false;
                mPlayTime += delta;

                foreach( AnimationState state in m_Animation )
                {
                    if( !m_Animation.IsPlaying( state.name ) ) continue;
                    float movement = state.speed * delta;
                    state.time += movement;

                    if( movement < 0f )
                    {
                        if( state.time > 0f ) playing = true;
                        else state.time = 0f;
                    }
                    else
                    {
                        if( state.time < state.length ) playing = true;
                        else state.time = state.length;
                    }

                    if( state.wrapMode == WrapMode.Loop )
                    {
                        if( state.time == 0f )
                            state.time = state.length;
                        else if( state.time == state.length )
                            state.time = 0f;

                        if( playing == false )
                            playing = true;

                        if( mAnimEvents.Count > 0 )
                            mAnimEvents.ForEach( a => a.is_fired = false );
                    }
                }

                if( OnPlayCallback != null && playing == true )
                {
                    OnPlayCallback.Invoke();
                    OnPlayCallback = null;
                }

                m_Animation.Sample();

                if( mAnimEvents.Count > 0 )
                {
                    foreach( AEventData evt in mAnimEvents )
                    {
                        if( evt.a_event.time <= mPlayTime && evt.is_fired == false )
                        {
                            evt.is_fired = true;
                            OnAnimationEvent( evt.a_event.stringParameter, evt.a_event.intParameter, evt.a_event.floatParameter );
                        }
                    }
                }

                if( playing ) return;
                enabled = false;
            }
            else
            {
                enabled = false;
                return;
            }

            mOnFinishedCallback?.Invoke();
        }

        //------------------------------------------------------------------------		
        public void Play()
        {
            Play( null, Direction.Forward, null );
        }
        public void Play( System.Action finished )
        {
            Play( null, Direction.Forward, finished );
        }
        public void Play( Direction playDirection, System.Action finished )
        {
            Play( null, playDirection, finished );
        }
        public void Play( string clipName )
        {
            Play( clipName, Direction.Forward, null );
        }
        public void Play( string clipName, Direction playDirection )
        {
            Play( clipName, playDirection, null );
        }

        public void Play( string clipName, Direction playDirection, System.Action finished )
        {
            Reset();
            mOnFinishedCallback = finished;

            // Determine the play direction
            if( playDirection == Direction.Toggle )
                playDirection = ( mLastDirection != Direction.Forward ) ? Direction.Forward : Direction.Reverse;

            if( m_Animation != null )
            {
                // We will sample the animation manually so that it works when the time is paused
                enabled = true;
                m_Animation.enabled = false;

                bool noName = string.IsNullOrEmpty( clipName );

                // Play the animation if it's not playing already
                AnimationClip clip = null;
                if( noName )
                {
                    clip = m_Animation.clip;
                    if( !m_Animation.isPlaying ) m_Animation.Play();
                }
                else if( !m_Animation.IsPlaying( clipName ) )
                {
                    clip = m_Animation.GetClip( clipName );
                    m_Animation.Play( clipName );
                }

                if( clip != null && clip.events != null && clip.events.Length > 0 )
                {
                    foreach( AnimationEvent ae in clip.events )
                    {
                        mAnimEvents.Add( new AEventData() { a_event = ae, is_fired = false } );
                    }
                }

                // Update the animation speed based on direction -- forward or back
                foreach( AnimationState state in m_Animation )
                {
                    if( string.IsNullOrEmpty( clipName ) || state.name == clipName )
                    {
                        float speed = Mathf.Abs( state.speed );
                        state.speed = speed * (int)playDirection;

                        // Automatically start the animation from the end if it's playing in reverse
                        if( playDirection == Direction.Reverse && state.time == 0f ) state.time = state.length;
                        else if( playDirection == Direction.Forward && state.time == state.length ) state.time = 0f;
                    }
                }

                // Remember the direction for disable checks in Update()
                mLastDirection = playDirection;
                m_Animation.Sample();
            }
            else
            {
                mOnFinishedCallback?.Invoke();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        ///
        //------------------------------------------------------------------------		
        public static ActiveAnimation PLAY( Animation anim )
        {
            return PLAY( anim, null );
        }
        public static ActiveAnimation PLAY( Animation anim, System.Action finished_callback )
        {
            return PLAY( anim, Direction.Forward, finished_callback );
        }
        public static ActiveAnimation PLAY( Animation anim, Direction playDirection, System.Action finished_callback )
        {
            return PLAY( anim, null, playDirection, finished_callback );
        }
        public static ActiveAnimation PLAY( Animation anim, string clipName, Direction playDirection, System.Action finished_callback )
        {
            ActiveAnimation aa = anim.GetComponent<ActiveAnimation>();
            if( aa == null )
                aa = anim.gameObject.AddComponent<ActiveAnimation>();

            aa.m_Animation = anim;
            aa.Play( clipName, playDirection, finished_callback );
            return aa;
        }
    }
}