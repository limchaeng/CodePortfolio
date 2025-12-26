//////////////////////////////////////////////////////////////////////////
//
// UIPopupManager
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
using UnityEngine.Events;

namespace UMF.Unity.UI
{
	public class UIPopupManager : TPrefabRootManagerBase<UIPopupManager>
	{
		//------------------------------------------------------------------------
		public interface ISettingBase
		{
			public float ISettingBase_PopupBlockAlpha { get; }
		}

		public class DefaultSetting : ISettingBase
		{
			public float ISettingBase_PopupBlockAlpha => 0.5f;
		}

		public static eUIPopupTypeFlag GLOBAL_TYPE_FLAGS = eUIPopupTypeFlag.None;

		[Header("[UIPopupManager]")]
		public int m_BaseOrderInLayer = 500;
		public int m_AddOrderInLayer = 100;

		const int TOP_ORDER_IN_LAYER = 2000;	// 대략적인...

		private Queue<UIPopupProperty> mPopupQueue = new Queue<UIPopupProperty>();
		private bool mPopupLoadProgress = false;
		public bool PopupLoadProgress { get { return mPopupLoadProgress; } set { mPopupLoadProgress = value; } }

		int mUniquePopupIndex = 0;
		public int UniquePopupIndex
		{
			get
			{
				mUniquePopupIndex += 1;
				if( mUniquePopupIndex >= int.MaxValue )
					mUniquePopupIndex = 1;

				return mUniquePopupIndex;
			}
		}

		ISettingBase mSetting = new DefaultSetting();
		public ISettingBase Setting { get { return mSetting; } set { mSetting = value; } }

		System.Action<bool> mCloseAllCallback = null;
		public System.Action<bool> CloseAllCallback { set { mCloseAllCallback = value; } }

		UIPopupBase mCurrentPopup = null;
		UIPopupBase mLoadingPopup = null;

		public UIPopupBase CurrentPopup { get { return mCurrentPopup; } }
		public UIPopupBase LoadingPopup { get { return mLoadingPopup; } }
		Stack<UIPopupBase> mPopupStack = new Stack<UIPopupBase>();
		public Stack<UIPopupBase> PopupStack { get { return mPopupStack; } }

		List<UIPopupBase> mLoadingdPopupList = new List<UIPopupBase>();

		UnityEvent<bool> mPopupOpenEvent = new UnityEvent<bool>();
		public void AddPopupOpenEvent(UnityAction<bool> action)
		{
			mPopupOpenEvent.AddListener( action );
		}

		//------------------------------------------------------------------------
		public void OpenPopup(UIPopupProperty property)
		{
			if( property == null || string.IsNullOrEmpty( property.m_PopupName ) )
				return;

			property.UniqueIndex = UniquePopupIndex;
			mPopupQueue.Enqueue( property );
			CheckPopupAndLoad( property );
		}

		//------------------------------------------------------------------------	
		bool CheckPopupAndLoad( UIPopupProperty check_property = null )
		{
			if( mPopupQueue.Count <= 0 )
				return false;

			if( check_property == null || check_property.HasTypeFlag( eUIPopupTypeFlag.ForcedTop ) == false )
			{
				if( mPopupLoadProgress )
					return false;
			}

			UIPopupProperty property = mPopupQueue.Dequeue();
			mPopupLoadProgress = true;
			StartCoroutine( LoadPopupAsync( property ) );

			return true;
		}

		//------------------------------------------------------------------------
		IEnumerator LoadPopupAsync( UIPopupProperty property )
		{
			string popup_name = property.m_PopupName;

			//GameMain.Instance.AddGameActionLog( eGAME_ACTION_LOG_TYPE.PO, popup_name );

			// step : load popup
			UIPopupBase newPopup = Load<UIPopupBase>( popup_name );
			if( newPopup == null )
			{
// 				Debug.LogWarning( GameMain.Instance.AddCriticalBug( string.Format( "!!! new POPUP({0}) invalid!", popup_name ) ) );
				mPopupLoadProgress = false;
				property.ExecuteCallback();
				yield break;
			}

			mLoadingdPopupList.Add( newPopup );
			mPopupOpenEvent.Invoke( true );

			//TooltipManager.Instance.Close();
			//AlarmManager.Instance.EnabledCompleteNotification = false;

			// step : set loading popup
			mLoadingPopup = newPopup;

			// step : current to stack
			if( mCurrentPopup != null )
				mPopupStack.Push( mCurrentPopup );

			WaitForFinish wait = WaitForFinish.POP();

			// step : new popup init
			int add_order = ( mCurrentPopup != null ? mCurrentPopup.AddedOrderInLayer + m_AddOrderInLayer : 0 );
			if( property.HasTypeFlag( eUIPopupTypeFlag.ForcedTop ) )
				add_order = TOP_ORDER_IN_LAYER;

			newPopup.Init( add_order, property, wait );
			while( wait.MoveNext() )
				yield return null;

			// step : show popup 
			newPopup.Open( wait );
			while( wait.MoveNext() )
				yield return null;

			// step : loading popup is null
			mLoadingPopup = null;
			mCurrentPopup = newPopup;

			mLoadingdPopupList.Remove( newPopup );

			WaitForFinish.Release( wait );
			//GameMain.Instance.UnlockInput();

			yield return null;

			mCurrentPopup.Begin();
			mPopupOpenEvent.Invoke( false );

			//GameMain.Instance.AddGameActionLog( eGAME_ACTION_LOG_TYPE.POF );

			//
			mPopupLoadProgress = false;
			bool exist_popup = CheckPopupAndLoad();

			if( exist_popup == false )
			{
// 				AlarmManager.Instance.EnabledCompleteNotification = enableCompleteNotification;
// 				AlarmManager.Instance.EnabledCollectionNotification = enableColletionNotification;

// 				yield return null;
// 
// 				// show package popup
// 				if( ignore_package_popup_check == false )
// 					PlayerPackageData.Instance.CheckPackagePopupShowCondition( eIAPPackagePopupCondition.POPUPOpen, 0, 0, 0, popup_name, null );
// 
// 				// show web view
// 				if( ignore_web_view_check == false )
// 					GameMain.Instance.CheckWebView( eWebViewCondition.PopupOpen, popup_name );
			}
		}

		//------------------------------------------------------------------------
		public void ClosePopup( UIPopupBase popup )
		{
			// close and callback
			mCurrentPopup = null;

			string popup_name = popup.PopupName;
			bool has_interaction = popup.HasInteraction;
			UIPopupProperty property = popup.PopupProperty;
			PrefabManager.Instance.Unload( popup );

// 			GameMain _instance_game_main = GameMain.Instance;
// 			_instance_game_main.AddGameActionLog( eGAME_ACTION_LOG_TYPE.PC, popup_name );

			// prev popup reactive
// 			bool enabled_complete_notification = true;
// 			bool enabled_colletion_notification = true;
// 			bool enabled_uevent_progress_notification = true;
			if( mPopupStack.Count > 0 )
			{
				mCurrentPopup = mPopupStack.Pop();
				mCurrentPopup.OnReActive( popup_name, has_interaction );

// 				enabled_complete_notification = mCurrentPopup.AutoEnableCompleteNotification();
// 				enabled_colletion_notification = mCurrentPopup.AutoEnableCollectionNotificaiton();
// 				enabled_uevent_progress_notification = mCurrentPopup.AutoEnableUEventProgressNotification();
			}
			else
			{
// 				enabled_complete_notification = _instance_game_main.CurrentPage.AutoEnableCompleteNotification();
// 				enabled_uevent_progress_notification = _instance_game_main.CurrentPage.AutoEanbleUEventProgressNotification();
			}

//			enabled_colletion_notification = _instance_game_main.CurrentPage.AutoEnableCollectionNotification();

// 			AlarmManager.Instance.EnabledCompleteNotification = enabled_complete_notification;
// 			AlarmManager.Instance.EnabledCollectionNotification = enabled_colletion_notification;
// 			AlarmManager.Instance.EnabledUEventProgressNotification = enabled_uevent_progress_notification;

			if( property != null )
				property.ExecuteCallback();

			if( mCurrentPopup == null )
			{
				PageBase curr_page = PageManager.Instance.CurrentPage;
				if( curr_page != null )
					curr_page.OnReActive( popup_name, has_interaction );
			}
		}

		//------------------------------------------------------------------------
		public void CloseCurrentPopupIs( string popup_name, bool ignore_callback )
		{
			if( mCurrentPopup != null && mCurrentPopup.PopupName == popup_name )
			{
				if( ignore_callback )
					mCurrentPopup.PopupProperty.m_Callback = null;
				ClosePopup( mCurrentPopup );
			}
		}

		//------------------------------------------------------------------------		
		public bool IsInOpenPopup( string popup_name )
		{
			foreach( UIPopupBase pb in mPopupStack )
			{
				if( pb.PopupName == popup_name )
					return true;
			}

			return false;
		}
		public bool IsInOpenPopup( int unique_index )
		{
			foreach( UIPopupBase pb in mPopupStack )
			{
				if( pb.PopupProperty != null && pb.PopupProperty.UniqueIndex == unique_index )
					return true;
			}

			return false;
		}

		//------------------------------------------------------------------------		
		public void CloseForcedPopup( string popup_name, bool ignore_callback )
		{
			if( IsInOpenPopup( popup_name ) == false )
				return;

			UIPopupBase close_popup = null;
			Stack<UIPopupBase> new_stack = new Stack<UIPopupBase>();
			while( mPopupStack.Count > 0 )
			{
				UIPopupBase popup = mPopupStack.Pop();
				if( popup.PopupName == popup_name )
				{
					close_popup = popup;
				}
				else
				{
					new_stack.Push( popup );
				}
			}

			mPopupStack = new_stack;

			if( close_popup != null )
				close_popup.ForcedClose( ignore_callback );
		}

		//------------------------------------------------------------------------		
		public void CloseForcedPopup( int unique_index, bool ignore_callback )
		{
			if( IsInOpenPopup( unique_index ) == false )
				return;

			UIPopupBase close_popup = null;
			Stack<UIPopupBase> new_stack = new Stack<UIPopupBase>();
			while( mPopupStack.Count > 0 )
			{
				UIPopupBase popup = mPopupStack.Pop();
				if( popup.PopupProperty != null && popup.PopupProperty.UniqueIndex == unique_index )
				{
					close_popup = popup;
				}
				else
				{
					new_stack.Push( popup );
				}
			}

			mPopupStack = new_stack;

			if( close_popup != null )
				close_popup.ForcedClose( ignore_callback );
		}

		//------------------------------------------------------------------------
		public void CloseAll()
		{
			// 로딩중 에러가 나서 남아 있는 팝업 관리용
			if( mLoadingdPopupList.Count > 0 )
			{
				foreach( UIPopupBase popup in mLoadingdPopupList )
				{
					PrefabManager.Instance.Unload( popup );
				}
				mLoadingdPopupList.Clear();
			}

			bool has_popups = false;
			if( mCurrentPopup != null )
			{
				has_popups = true;
				PrefabManager.Instance.Unload( mCurrentPopup );
			}

			mCurrentPopup = null;

			if( mPopupStack.Count > 0 )
				has_popups = true;

			while( mPopupStack.Count > 0 )
			{
				UIPopupBase popup = mPopupStack.Pop();
				PrefabManager.Instance.Unload( popup );
			}

			mPopupStack.Clear();

			mCloseAllCallback?.Invoke( has_popups );
		}
		
		//------------------------------------------------------------------------
		public int GetCurrentPopupCount()
		{
			return mPopupStack.Count;
		}
	}
}