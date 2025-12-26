//////////////////////////////////////////////////////////////////////////
//
// PageBase
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
	public class PageParam
	{
		Dictionary<string, object> mParamDic = new Dictionary<string, object>();

		public int Count { get { return mParamDic.Count; } }

		public PageParam AddParam( System.Enum enum_key, object value )
		{
			return AddParam( enum_key.ToString(), value );
		}
		public PageParam AddParam( string key, object value )
		{
			if( mParamDic.ContainsKey( key ) )
				mParamDic[key] = value;
			else
				mParamDic.Add( key, value );

			return this;
		}

		public T GetParam<T>( System.Enum enum_key, T default_value )
		{
			return GetParam<T>( enum_key.ToString(), default_value );
		}
		public T GetParam<T>( string key, T default_value )
		{
			object v;
			if( mParamDic.TryGetValue( key, out v ) )
			{
				System.Type t_type = typeof( T );
				if( t_type.IsClass )
					return (T)v;

				if( t_type.IsValueType && t_type.IsPrimitive == false && t_type.IsEnum == false )
					return (T)v;

				return StringUtil.SafeParse<T>( v.ToString(), default_value );
			}

			return default_value;
		}
	}

	public abstract class PageBase : PrefabRootBehaviour
	{
		protected PageParam mPageParam = null;
		public PageParam GetPageParam { get { return mPageParam; } }

		// 		protected object[] mPageParams = null;
		// 		public object[] PageParams { get { return mPageParams; } set { mPageParams = value; } }
		// 		protected object[] mShortcutParams = null;

		protected bool mPageBegin = false;

		//------------------------------------------------------------------------
		public virtual void Begin()
		{
			mPageBegin = true;
		}

		//------------------------------------------------------------------------
		public virtual bool CheckParams( PageParam page_param )
		{
			mPageParam = page_param;
// 			mPageParams = page_params;
// 			mShortcutParams = shortcut_params;
			return true;
		}

// 		public T GetPageParam<T>( int index, T default_value )
// 		{
// 			if( mPageParams == null || mPageParams.Length <= index )
// 				return default_value;
// 
// 			System.Type t_type = typeof( T );
// 			if( t_type.IsClass )
// 				return (T)mPageParams[index];
// 
// 			return StringUtil.SafeParse<T>( mPageParams[index].ToString(), default_value );
// 		}
// 
		//------------------------------------------------------------------------
		public T FindPageParam<T>( System.Enum enum_key, T default_value )
		{
			return FindPageParam<T>( enum_key.ToString(), default_value );
		}
		public T FindPageParam<T>( string key, T default_value )
		{
			if( mPageParam != null )
				return mPageParam.GetParam<T>( key, default_value );

			return default_value;
		}

		// virtual 
		public virtual bool IgnoreLoading { get; } = false;
		public virtual bool IgnoreLoadingFX { get; } = false;
		public virtual bool IgnoreGCCollect { get; } = false;

		public virtual void Init_2() { }
		public virtual void PostInit() { }		
		public virtual void PlayShow( WaitForFinish wait ) { }
		public virtual void PlayHide( WaitForFinish wait, PageBase new_page ) { }

		protected string mPageMusicID = "";
		public string PageMusicID { get { return mPageMusicID; } }
		protected bool mStopMainBGLoopSound = false;
		public bool StopMainBGLoopSound { get { return mStopMainBGLoopSound; } }
		public virtual void PostLoad() { }
		public virtual void StopPageMuisic( string change_music_id ) { }
		public virtual void PlayPageMusic() { }		
		public virtual string GetUIName() { return ""; }

		public virtual void OnUnHandledException() { }

		//------------------------------------------------------------------------		
		public virtual void OnReActive(string closed_popup_name, bool has_popup_interaction ) { }

#if UNITY_EDITOR || UMDEV
		public virtual void TestHide( bool bHide ) { }
#endif
	}
}
