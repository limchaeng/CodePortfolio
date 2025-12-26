//////////////////////////////////////////////////////////////////////////
//
// TypeWriter_TMP
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
#define UM_TMP_PRESENT
#if UM_TMP_PRESENT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UMF.Unity
{
	public class TypeWriterSetting
	{
		public float char_time = 0.1f;
		public float space_time = -1f;
		public float line_time = -1f;
		public float max_time = 0f;
	}

	public class TypeWriter_TMP : MonoBehaviour
	{
		public enum eCallbackType
		{
			Begin,
			Update,
			Update_Line,
			End,
		}

		TMP_Text mTMP = null;
		ScrollRect mAutoScroll = null;
		Vector2 mTMPRendereValues = Vector2.zero;

		float mCharTime = 0f;
		float mSpaceTime = 0f;
		float mLineTime = 0f;
		float mMaxTime = 0f;

		string mParsedText = "";
		int mTextLength = 0;
		public delegate void delegateCallback( eCallbackType callback_type, TMP_Text tmp_text );
		delegateCallback mCallback = null;
		int mTypeWriteCharPosition = 0;

		float mRunTime = 0f;
		float mNextCheckTime = 0f;

		//------------------------------------------------------------------------
		public void Setup( TMP_Text target, TypeWriterSetting setting, delegateCallback finished_callback )
		{
			Setup( target, setting, null, finished_callback );
		}
		public void Setup( TMP_Text target, TypeWriterSetting setting, ScrollRect auto_scroll, delegateCallback finished_callback )
		{
			mTMP = target;
			mAutoScroll = auto_scroll;

			mCharTime = setting.char_time;
			mSpaceTime = setting.space_time;
			mLineTime = setting.line_time;
			mMaxTime = setting.max_time;

			if( mSpaceTime < 0f ) mSpaceTime = mCharTime;			
			if( mLineTime < 0f ) mLineTime = mCharTime;			

			mCallback = finished_callback;

			mTMP.ForceMeshUpdate();
			mParsedText = mTMP.GetParsedText();
			mTextLength = mTMP.textInfo.characterCount;
			mTypeWriteCharPosition = 0;

			float max_duration = 0f;
			for( int i = 0; i < mTextLength; i++ )
			{
				if( mParsedText[i] == ' ' )
					max_duration += mSpaceTime;
				else if( mParsedText[i] == '\n' || mParsedText[i] == '\r' )
					max_duration += mLineTime;
				else
					max_duration += mCharTime;
			}

			if( mMaxTime > 0f && max_duration > mMaxTime )
			{
				float ratio = mMaxTime / max_duration;
				mCharTime *= ratio;
				mSpaceTime *= ratio;
				mLineTime *= ratio;
			}
			else
			{
				mMaxTime = max_duration;
			}

			Restart();
		}

		//------------------------------------------------------------------------
		void UpdateTMP( bool force_mesh_update )
		{
			mTMP.maxVisibleCharacters = mTypeWriteCharPosition;
			if( force_mesh_update )
				mTMP.ForceMeshUpdate();

			if( mAutoScroll != null )
			{
				Vector2 render_values = mTMP.GetRenderedValues();
				Vector2 scroll_normalized_position = mAutoScroll.normalizedPosition;

				if( mAutoScroll.vertical && mTMPRendereValues.y != render_values.y && render_values.y > mAutoScroll.viewport.rect.height )
				{
					mTMPRendereValues.y = render_values.y;
					float view_height = mAutoScroll.viewport.rect.height;

					float unview_size = mTMP.preferredHeight - view_height;
					float scroll_normal = Mathf.Clamp01( 1f - ( ( mTMPRendereValues.y - view_height ) / unview_size ) );

					if( scroll_normal < scroll_normalized_position.y )
						scroll_normalized_position.y = scroll_normal;
				}

				if( mAutoScroll.horizontal && mTMPRendereValues.x != render_values.x && render_values.x > mAutoScroll.viewport.rect.width )
				{
					mTMPRendereValues.x = render_values.x;
					float view_width = mAutoScroll.viewport.rect.width;
					float unview_size = mTMP.preferredWidth - view_width;
					float scroll_normal = Mathf.Clamp01( 1f - ( ( mTMPRendereValues.x - view_width ) / unview_size ) );

					if( scroll_normal < scroll_normalized_position.x )
						scroll_normalized_position.x = scroll_normal;
				}

				mAutoScroll.normalizedPosition = scroll_normalized_position;
			}

			mCallback?.Invoke( eCallbackType.Update, mTMP );
		}

		//------------------------------------------------------------------------
		float GetNextCheckTime( int idx )
		{
			if( idx >= mTextLength )
				return 0f;

			if( mParsedText[idx] == ' ' )
				return mSpaceTime;

			if( mParsedText[idx] == '\n' || mParsedText[idx] == '\r' )
				return mLineTime;

			return mCharTime;
		}

		//------------------------------------------------------------------------
		[ContextMenu( "Restart TypeWriter" )]
		public void Restart()
		{
			if( mTMP == null )
				return;

			enabled = true;

			mTypeWriteCharPosition = 0;
			mNextCheckTime = 0f;
			mRunTime = 0f;
			mTMPRendereValues = Vector2.zero;
			mCallback?.Invoke( eCallbackType.Begin, mTMP );

			UpdateTMP( true );
		}

		//------------------------------------------------------------------------
		[ContextMenu( "maxVisibleCharacters MAX" )]
		void _MaxVisibleMax()
		{
			if( mTMP != null )
				mTMP.maxVisibleCharacters = 99999;
		}			

		//------------------------------------------------------------------------
		public void Skip()
		{
			if( mTMP == null )
				return;

			mTypeWriteCharPosition = mTextLength;
			UpdateTMP( true );
			DoFinish();
		}		

		//------------------------------------------------------------------------
		void DoFinish()
		{
			enabled = false;
			mTextLength = 0;			
			mCallback?.Invoke( eCallbackType.End, mTMP );
		}

		//------------------------------------------------------------------------
		private void LateUpdate()
		{
			if( mTextLength <= 0 )
				return;

			mRunTime += Time.deltaTime;
			if( mRunTime >= mNextCheckTime )
			{
				mRunTime = 0f;
				mTypeWriteCharPosition += 1;
				UpdateTMP( mTypeWriteCharPosition >= mTextLength );

				mNextCheckTime = GetNextCheckTime( mTypeWriteCharPosition );

				if( mTypeWriteCharPosition >= mTextLength )
				{
					DoFinish();
				}
				else
				{
					char cur_char = mParsedText[mTypeWriteCharPosition];
					if( cur_char == '\n' || cur_char == '\r' )
						mCallback?.Invoke( eCallbackType.Update_Line, mTMP );
				}
			}
		}

		//------------------------------------------------------------------------		
		public static TypeWriter_TMP Begin( TMP_Text target, TypeWriterSetting setting, delegateCallback callback )
		{
			return Begin( target, setting, null, callback );			
		}
		public static TypeWriter_TMP Begin( TMP_Text target, TypeWriterSetting setting, ScrollRect auto_scroll, delegateCallback callback )
		{
			TypeWriter_TMP comp = target.GetComponent<TypeWriter_TMP>();
			if( comp == null )
				comp = target.gameObject.AddComponent<TypeWriter_TMP>();

			comp.Setup( target, setting, auto_scroll, callback );
			return comp;
		}
	}
}

#endif