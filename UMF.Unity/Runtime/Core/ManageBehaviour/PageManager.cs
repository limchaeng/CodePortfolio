//////////////////////////////////////////////////////////////////////////
//
// PageManager
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
using UnityEngine.Events;

namespace UMF.Unity
{
	public class PageManager : TPrefabRootManagerBase<PageManager>
	{
		public enum ePageLoadStep
		{
			Start,
			Load,
			Checked,
			Init,		// for new
			PostInit,	
			Switch,		// show / hide
			Unload,		// destroy
			Begin,      // new begin
			Finished
		}

		public delegate void delPageLoadProgress(ePageLoadStep step, bool is_completed);
		private delPageLoadProgress mProgressCallback = null;

		public bool IsLoading { get; private set; } = false;
		public string LoadingPageName { get; private set; } = "";
		PageBase mCurrentPage = null;
		public PageBase CurrentPage { get { return mCurrentPage; } }
		public string CurrentPageName
		{
			get
			{
				if( mCurrentPage != null )
					return mCurrentPage.PrefabName;

				return "";
			}
		}

		public delegate bool delegateLoadingEvent( PageBase page, bool is_out, WaitForFinish wait );
		delegateLoadingEvent mLoadingEventHandler = null;
		public delegateLoadingEvent LoadingEventHandler { set { mLoadingEventHandler = value; } }

		UnityEvent<bool> mPageLoadEvent = new UnityEvent<bool>();
		public void AddPageLoadEvent(UnityAction<bool> action)
		{
			mPageLoadEvent.AddListener( action );
		}

		//------------------------------------------------------------------------
		void NotifyProgress( ePageLoadStep step, bool is_completed = true )
		{
			mProgressCallback?.Invoke( step, is_completed );
		}

		//------------------------------------------------------------------------
		/// <summary>
		///   shortcut_param = key,value|key,value|....
		/// </summary>
		public bool PageLoadFromShortcut( string page_name, string shortcut_param, delPageLoadProgress progress_callback )
		{
			PageParam page_param = null;
			if( string.IsNullOrEmpty(shortcut_param) == false )
			{
				string[] kvp_splits = shortcut_param.Split( '|' );
				if( kvp_splits != null )
				{
					foreach( string kvp in kvp_splits )
					{
						string[] kv = kvp.Split( ',' );
						if( kv == null || kv.Length != 2 )
							continue;

						string key = kv[0];
						object value = kv[1];

						if( page_param == null )
							page_param = new PageParam();

						page_param.AddParam( key, value );
					}
				}
			}

			return PageLoad( page_name, page_param, progress_callback );
		}
		public bool PageLoad( string page_name )
		{
			return PageLoad( page_name, null, null );
		}
		public bool PageLoad( string page_name, PageParam page_param )
		{
			return PageLoad( page_name, page_param, null );
		}
		public bool PageLoad(string page_name, PageParam page_param, delPageLoadProgress progress_callback )
		{
			if( mCurrentPage != null && mCurrentPage.PrefabName == page_name )
				return false;

			mProgressCallback = progress_callback;
			IsLoading = true;
			LoadingPageName = page_name;

			NotifyProgress( ePageLoadStep.Start, true );
			StartCoroutine( _PageLoadRoutine( page_name, page_param ) );

			return true;
		}

		//------------------------------------------------------------------------
		IEnumerator DoLoadingEvent( PageBase page, bool is_out )
		{
			WaitForFinish wait = WaitForFinish.POP();

			if( mLoadingEventHandler != null )
			{
				if( mLoadingEventHandler( page, is_out, wait) )
				{
					while( wait.MoveNext() )
						yield return null;
				}
			}

			WaitForFinish.Release( wait );
		}

		//------------------------------------------------------------------------
		IEnumerator _PageLoadRoutine( string page_name, PageParam page_param )
		{
			WaitForFinish wait = WaitForFinish.POP();

			wait.AddEnumerator( DoLoadingEvent( mCurrentPage, true ) );
			while( wait.MoveNext() )
				yield return null;

			PageBase new_page = Load<PageBase>( page_name );
			if( new_page == null )
			{
				IsLoading = false;
				NotifyProgress( ePageLoadStep.Load, false );
				NotifyProgress( ePageLoadStep.Finished );

				wait.AddEnumerator( DoLoadingEvent( mCurrentPage, false ) );
				while( wait.MoveNext() )
					yield return null;

				yield break;
			}

			mPageLoadEvent.Invoke( true );

			NotifyProgress( ePageLoadStep.Load, true );
			yield return null;

			// check
			if( new_page.CheckParams( page_param ) == false )
			{
				Unload( new_page );
				IsLoading = false;
				NotifyProgress( ePageLoadStep.Checked, false );
				NotifyProgress( ePageLoadStep.Finished );

				wait.AddEnumerator( DoLoadingEvent( mCurrentPage, false ) );
				while( wait.MoveNext() )
					yield return null;

				yield break;					
			}

			new_page.PostLoad();
			NotifyProgress( ePageLoadStep.Checked );

			if( mCurrentPage != null )
			{
				mCurrentPage.StopPageMuisic( new_page.PageMusicID );
				mCurrentPage.PlayHide( wait, new_page );
				while( wait.MoveNext() )
					yield return null;
			}

			yield return null;

			// initialize
			new_page.P_Init();
			new_page.Init_2();
			NotifyProgress( ePageLoadStep.Init );
			yield return null;

			new_page.PostInit();			
			NotifyProgress( ePageLoadStep.PostInit );
			yield return null;

			// switch
			NotifyProgress( ePageLoadStep.Switch, false );

			// loading
			wait.AddEnumerator( DoLoadingEvent( new_page, false ) );
			while( wait.MoveNext() )
				yield return null;

			new_page.PlayPageMusic();
			new_page.PlayShow( wait );
			while( wait.MoveNext() )
				yield return null;

			NotifyProgress( ePageLoadStep.Switch );

			// unload
			if( mCurrentPage != null )
				Unload( mCurrentPage );

			mCurrentPage = new_page;

			if( mCurrentPage.IgnoreGCCollect == false )
				System.GC.Collect();

			AsyncOperation op = Resources.UnloadUnusedAssets();
			while( op.isDone == false )
				yield return null;

			NotifyProgress( ePageLoadStep.Unload );
			yield return null;

			NotifyProgress( ePageLoadStep.Begin );
			mCurrentPage.Begin();			
			NotifyProgress( ePageLoadStep.Finished );

			WaitForFinish.Release( wait );

			mPageLoadEvent.Invoke( false );
		}

		//------------------------------------------------------------------------
		public void OnUnHandledException()
		{
			if( mCurrentPage != null )
				mCurrentPage.OnUnHandledException();
		}
	}
}