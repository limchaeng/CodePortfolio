#define NO_UNITY_EDITOR_COROUTINE
#if !NO_UNITY_EDITOR_COROUTINE
//////////////////////////////////////////////////////////////////////////
//
// ParticleSystemPlayExtensions
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// https://gist.github.com/ababilinski/1da122e073fb72cdac9c7249ab29bab0
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

namespace UMF.Unity
{
	public static class ParticleSystemPlayExtensions
	{
		//------------------------------------------------------------------------
		private static Dictionary<ParticleSystem, EditorCoroutine> _simulationCoroutinesByParticleSystem;
		private static Dictionary<ParticleSystem, bool> _autoRandomSeedByParticleSystem;
		private static EditorCoroutine _CallbackCoroutine = null;

		/// <summary>
		/// Plays the particle system inside the editor without having to select it. (if called during Play Mode, particle system will play as usual)
		/// </summary>
		/// <param name="particleSystem">this</param>
		/// <param name="withChildren">Play all children particles as well</param>
		/// <param name="effectFinishedEditorEvent">action call back when system is finished [Does Not Work Outside Of Unity Editor] </param>
		public static void PlayInEditor( this ParticleSystem particleSystem, bool withChildren, Action effectFinishedEditorEvent )
		{

			if( Application.isPlaying )
			{
				//play system as normal when game is playing
				particleSystem.Play( withChildren );
				//wait for particleSystem.IsPlaying to equal false
				if( _CallbackCoroutine != null )
					EditorCoroutineUtility.StopCoroutine( _CallbackCoroutine );
				_CallbackCoroutine = EditorCoroutineUtility.StartCoroutine( DoCallbackWhenFinished( particleSystem ), particleSystem );
				return;
			}

			//When playing the system in the editor, we have to control each child individually
			var particleSystems = withChildren ? particleSystem.GetComponentsInChildren<ParticleSystem>( true ) : new[] { particleSystem.GetComponentInChildren<ParticleSystem>( true ) };

			var particleCount = particleSystems.Length;
			var finishedParticleCount = 0;

			if( _simulationCoroutinesByParticleSystem == null )
			{
				//Store a dictionary of systems so that we do not start multiple coroutines
				_simulationCoroutinesByParticleSystem = new Dictionary<ParticleSystem, EditorCoroutine>();
			}

			if( _autoRandomSeedByParticleSystem == null )
			{
				//Store a dictionary of systems so that we can reset the random seed value after playing
				_autoRandomSeedByParticleSystem = new Dictionary<ParticleSystem, bool>();
			}

			foreach( var childParticleSystem in particleSystems )
			{
				if( !childParticleSystem.gameObject.activeInHierarchy )
				{
					continue;
				}

				if( _simulationCoroutinesByParticleSystem.TryGetValue( childParticleSystem, out EditorCoroutine editorCoroutine ) )
				{
					//if the system is currently playing or the coroutine was interrupted previously, make sure to stop the system and remove the dictionary item
					childParticleSystem.Stop( false, ParticleSystemStopBehavior.StopEmittingAndClear );
					EditorCoroutineUtility.StopCoroutine( editorCoroutine );
					_simulationCoroutinesByParticleSystem.Remove( childParticleSystem );

					if( _autoRandomSeedByParticleSystem.TryGetValue( childParticleSystem, out bool isOn ) )
					{
						//if the system is currently playing or the coroutine was interrupted previously, make sure to set the autoRandomSeed value to the original value
						childParticleSystem.useAutoRandomSeed = isOn;
						_autoRandomSeedByParticleSystem.Remove( childParticleSystem );
					}
				}


				var currentEditorCoroutine = EditorCoroutineUtility.StartCoroutine( PlaySingleParticleSystemInEditor( childParticleSystem, () =>
				{
					finishedParticleCount++;
					// if we ar playing children, only do master call back once all systems finish
					if( finishedParticleCount == particleCount )
					{
						effectFinishedEditorEvent?.Invoke();
					}
				} ), childParticleSystem );
				_simulationCoroutinesByParticleSystem.Add( childParticleSystem, currentEditorCoroutine );
			}
		}

		/* ---------------------------------------------------------------------------------------- */
		/* ------------------------ Shorthand Methods --------------------------------------------- */
		/* ---------------------------------------------------------------------------------------- */
		#region Shorthand
		/// <summary>
		/// Plays the particle system inside the editor without having to select it. (if called during Play Mode, particle system will play as usual)
		/// </summary>
		/// <param name="particleSystem">this</param>
		/// <param name="withChildren">Play all children particles as well</param>
		public static void PlayInEditor( this ParticleSystem particleSystem, bool withChildren )
		{
			particleSystem.PlayInEditor( withChildren, null );
		}

		/// <summary>
		/// Plays the particle system and it's children inside the editor without having to select it. (if called during Play Mode, particle system will play as usual)
		/// </summary>
		/// <param name="particleSystem">this</param>
		public static void PlayInEditor( this ParticleSystem particleSystem )
		{
			particleSystem.PlayInEditor( true, null );
		}

		/// <summary>
		/// Plays the particle system and it's children inside the editor without having to select it. (if called during Play Mode, particle system will play as usual)
		/// </summary>
		/// <param name="particleSystem">this</param>
		/// <param name="effectFinishedEditorEvent">action call back when system is finished [Does Not Work Outside Of Unity Editor] </param>
		public static void PlayInEditor( this ParticleSystem particleSystem, Action effectFinishedEditorEvent )
		{
			particleSystem.PlayInEditor( true, effectFinishedEditorEvent );
		}

		public static void StopInEditor( this ParticleSystem particleSystem )
		{
			if( Application.isPlaying )
				particleSystem.Stop();


			if( _simulationCoroutinesByParticleSystem != null )
			{
				foreach( var kvp in _simulationCoroutinesByParticleSystem )
				{
					kvp.Key.Stop();
					EditorCoroutineUtility.StopCoroutine( kvp.Value );
				}

				_simulationCoroutinesByParticleSystem.Clear();
			}
		}
		#endregion

		/* ---------------------------------------------------------------------------------------- */
		/* --------------------------------- Worker Coroutines ------------------------------------ */
		/* ---------------------------------------------------------------------------------------- */
		#region Editor Coroutines
		static IEnumerator DoCallbackWhenFinished( ParticleSystem particleSystem, Action effectFinished = null )
		{

			_CallbackCoroutine = null;

			//[always check if the state of the system changes or if we exit play mode]
			float curTime = 0;
			while( curTime < particleSystem.time && particleSystem != null && particleSystem.isPlaying )
			{
				curTime = particleSystem.time;

				yield return null;
			}

			if( particleSystem != null && particleSystem.isPlaying )
			{
				effectFinished?.Invoke();
			}
		}
		static IEnumerator PlaySingleParticleSystemInEditor( ParticleSystem particleSystem, Action effectFinished = null )
		{
			bool isAutoRandomSeed = particleSystem.useAutoRandomSeed;

			if( _autoRandomSeedByParticleSystem.ContainsKey( particleSystem ) == false )
				_autoRandomSeedByParticleSystem.Add( particleSystem, isAutoRandomSeed );
			particleSystem.Stop( false, ParticleSystemStopBehavior.StopEmittingAndClear );

			yield return null;
			var mainSystem = particleSystem.main;
			var totalTime = mainSystem.startLifetimeMultiplier * mainSystem.duration;
			particleSystem.randomSeed = (uint)UnityEngine.Random.Range( 0, int.MaxValue );
			var curTime = 0.0f;

			yield return null;
			//update system to play state
			particleSystem.Play( false );
			//Time.deltaTime does not work in editor so we use the Unity Time Since Startup
			var initialTime = (float)EditorApplication.timeSinceStartup;

			while( curTime < totalTime && particleSystem != null && particleSystem.gameObject.activeInHierarchy )
			{

				// in order to play in edit mode, both Simulate and time have to be changed. 
				particleSystem.Simulate( curTime, false, true );
				particleSystem.time = curTime;
				//track delta between start and current Unity Time Since Startup
				curTime = (float)EditorApplication.timeSinceStartup - initialTime;


				yield return null;
			}

			if( particleSystem != null )
			{
				//update system to stop state
				particleSystem.Stop( false, ParticleSystemStopBehavior.StopEmittingAndClear );

				yield return null;

				if( isAutoRandomSeed )
				{
					//restore random seed value back to original value
					particleSystem.randomSeed = 0;
					particleSystem.useAutoRandomSeed = isAutoRandomSeed;
				}
				//remove item from dictionary when system finishes

				_autoRandomSeedByParticleSystem.Remove( particleSystem );
				_simulationCoroutinesByParticleSystem.Remove( particleSystem );
				yield return null;

				effectFinished?.Invoke();
			}
		}
		#endregion
		//------------------------------------------------------------------------
		//------------------------------------------------------------------------
	}
}
#endif