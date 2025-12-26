//////////////////////////////////////////////////////////////////////////
//
// DoTweenExtensions
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
//#define UM_DOTWEEN_PRESENT
#if UM_DOTWEEN_PRESENT

using DG.Tweening.Core;
using DG.Tweening.Core.Enums;
using DG.Tweening.Plugins.Options;
using UMF.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace DG.Tweening
{
	public static class DoTweenExtensions
	{
		public static Tweener DOPunchPositionWithRewind( this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1, bool snapping = false )
		{
			DOTween.Rewind( target );
			return DOTween.Punch( () => target.localPosition, x => target.localPosition = x, punch, duration, vibrato, elasticity )
				.SetTarget( target ).SetOptions( snapping );
		}

		public static Tweener DOPunchScaleWithRewind( this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1 )
		{
			DOTween.Rewind( target );
			return DOTween.Punch( () => target.localScale, x => target.localScale = x, punch, duration, vibrato, elasticity )
				.SetTarget( target );
		}

		public static Tweener DOPunchRotationWithRewind( this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1 )
		{
			DOTween.Rewind( target );
			return DOTween.Punch( () => target.localEulerAngles, x => target.localRotation = Quaternion.Euler( x ), punch, duration, vibrato, elasticity )
				.SetTarget( target );
		}

		public static Tweener DOShakePositionWithRewind( this Transform target, float duration, float strength = 1, int vibrato = 10, float randomness = 90, bool snapping = false )
		{
			DOTween.Rewind( target );
			return DOTween.Shake( () => target.localPosition, x => target.localPosition = x, duration, strength, vibrato, randomness, false )
				.SetTarget( target ).SetSpecialStartupMode( SpecialStartupMode.SetShake ).SetOptions( snapping );
		}

		public static Tweener DOShakePositionWithRewind( this Transform target, float duration, Vector3 strength, int vibrato = 10, float randomness = 90, bool snapping = false )
		{
			DOTween.Rewind( target );
			return DOTween.Shake( () => target.localPosition, x => target.localPosition = x, duration, strength, vibrato, randomness )
				.SetTarget( target ).SetSpecialStartupMode( SpecialStartupMode.SetShake ).SetOptions( snapping );
		}

        public static Tweener DOShakeRotationWithRewind( this Transform target, float duration, float strength = 90, int vibrato = 10, float randomness = 90 )
		{
			DOTween.Rewind( target );
			return DOTween.Shake( () => target.localEulerAngles, x => target.localRotation = Quaternion.Euler( x ), duration, strength, vibrato, randomness, false )
				.SetTarget( target ).SetSpecialStartupMode( SpecialStartupMode.SetShake );
		}
		public static Tweener DOShakeRotationWithRewind( this Transform target, float duration, Vector3 strength, int vibrato = 10, float randomness = 90 )
		{
			DOTween.Rewind( target );
			return DOTween.Shake( () => target.localEulerAngles, x => target.localRotation = Quaternion.Euler( x ), duration, strength, vibrato, randomness )
				.SetTarget( target ).SetSpecialStartupMode( SpecialStartupMode.SetShake );
		}

		public static Tweener DOShakeScaleWithRewind( this Transform target, float duration, float strength = 1, int vibrato = 10, float randomness = 90 )
		{
			DOTween.Rewind( target );
			return DOTween.Shake( () => target.localScale, x => target.localScale = x, duration, strength, vibrato, randomness, false )
				.SetTarget( target ).SetSpecialStartupMode( SpecialStartupMode.SetShake );
		}

		public static Tweener DOShakeScaleWithRewind( this Transform target, float duration, Vector3 strength, int vibrato = 10, float randomness = 90 )
		{
			DOTween.Rewind( target );
			return DOTween.Shake( () => target.localScale, x => target.localScale = x, duration, strength, vibrato, randomness )
				.SetTarget( target ).SetSpecialStartupMode( SpecialStartupMode.SetShake );
		}

		public static Tweener DOFade( this SpriteRendererGroup target, float endValue, float duration )
		{
			DOTween.Rewind( target );
			TweenerCore<float, float, FloatOptions> t = DOTween.To( () => target.alpha, x => target.alpha = x, endValue, duration );
			t.SetTarget( target );
			return t;
		}
	}
}

#endif