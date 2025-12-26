//////////////////////////////////////////////////////////////////////////
//
// UIPopupBase
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

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UMF.Unity.UI
{
	public delegate void delegatePopupCallback( int btnId, object[] parms );
	public delegate void delegatePopupOpenCallback( UIPopupBase popup );

	[System.Flags]
	public enum eUIPopupTypeFlag
	{
		None                 = 0x0000,
		Modeless             = 0x0001,
		NoPlayCloseSound     = 0x0002,
		NoCloseOutsideFrame  = 0x0004,
		NoPopupAnimation     = 0x0008,
		Draggable            = 0x0010,
		NoCloseButton        = 0x0020,
		NoCloseESC           = 0x0040,
		NoInputAction        = 0x0080,
		ForcedTop            = 0x0100,
		IgnoreGlobalFlags    = 0x8000,
	}

	//------------------------------------------------------------------------	
	public class UIPopupProperty
	{
		public string m_PopupName = "";
		public float m_PopupBlockAlpha = 0.5f;
		public eUIPopupTypeFlag m_PopupTypeFlags = eUIPopupTypeFlag.None;
		public delegatePopupCallback m_Callback = null;
		public object[] m_CallbackParams = null;
		public string m_CustomParams = "";
		public delegatePopupOpenCallback m_OpenCallback = null;

		// 특정 팝업을 구분하기 위한 값 : UIPopupManager 에서만 셋팅한다.
		protected int mUniqueIndex = 0;
		public int UniqueIndex { get { return mUniqueIndex; } set { mUniqueIndex = value; } }

		public void SetTypeFlag( eUIPopupTypeFlag flag, bool is_set )
		{
			if( is_set )
				m_PopupTypeFlags |= flag;
			else
				m_PopupTypeFlags &= ~flag;
		}
		public bool HasTypeFlag( eUIPopupTypeFlag flag )
		{
			return ( ( m_PopupTypeFlags & flag ) != 0 );
		}

		private int mCallbackBtnID = -1;
		public int CallbackBtnID { get { return mCallbackBtnID; } }

		public UIPopupProperty( string popup_name )
		{
			m_PopupName = popup_name;
			m_PopupBlockAlpha = UIPopupManager.Instance.Setting.ISettingBase_PopupBlockAlpha;
		}

		public void SetCallbackBtnID( int btn_id )
		{
			mCallbackBtnID = btn_id;
		}

		public void SetCallbackParams( params object[] parms )
		{
			m_CallbackParams = parms;
		}

		public virtual void ExecuteCallback()
		{
			if( m_Callback != null )
			{
				m_Callback( mCallbackBtnID, m_CallbackParams );

				mCallbackBtnID = -1;
			}
		}
	}

	//------------------------------------------------------------------------	
	[RequireComponent( typeof( Canvas ) )]
	[RequireComponent( typeof( GraphicRaycaster ) )]
	public abstract class UIPopupBase : PrefabRootBehaviour
	{
		[Header( "[UIPopupBase]" )]
		public RectTransform m_ContentsRoot;
		[Header( "[UIPopupBase] - nullable" )]
		public Image m_BackgroundBlock;
		public Button m_CloseButton;
		[Tooltip( "for Modeless POPUP" )]
		public RectTransform m_Frame;

		protected Canvas mRootCanvas;
		public Canvas RootCanvas { get { return mRootCanvas; } }
		protected int mBaseRootOrderInLayer = 0;
		protected int mAddedOrderInLayer = 0;
		public int AddedOrderInLayer { get { return mAddedOrderInLayer; } }

		protected UIPopupProperty mPopupProperty = null;
		public UIPopupProperty PopupProperty { get { return mPopupProperty; } }
		public string PopupName { get { return ( mPopupProperty != null ? mPopupProperty.m_PopupName : "" ); } }
		public bool HasInteraction { get; set; } = false;
		public bool IsClosed { get; set; } = false;

		private void Awake()
		{
			mRootCanvas = gameObject.GetComponent<Canvas>();
			mBaseRootOrderInLayer = mRootCanvas.sortingOrder;
		}

		//------------------------------------------------------------------------
		public sealed override void P_Init() { }

		//------------------------------------------------------------------------
		public virtual void Init( int add_order_in_layer, UIPopupProperty property, WaitForFinish wait )
		{
			mAddedOrderInLayer = add_order_in_layer;
			mPopupProperty = property;
			if( mPopupProperty.HasTypeFlag( eUIPopupTypeFlag.IgnoreGlobalFlags ) == false )
				mPopupProperty.SetTypeFlag( UIPopupManager.GLOBAL_TYPE_FLAGS, true );

			IsClosed = false;

			if( mRootCanvas == null )
			{
				mRootCanvas = gameObject.GetComponent<Canvas>();
				mBaseRootOrderInLayer = mRootCanvas.sortingOrder;
			}

			int base_order = UIPopupManager.Instance.m_BaseOrderInLayer;

			if( mRootCanvas.sortingOrder >= base_order )
				mRootCanvas.sortingOrder += AddedOrderInLayer;
			else
				mRootCanvas.sortingOrder = base_order + AddedOrderInLayer;

			Canvas[] all_canvas = gameObject.GetComponentsInChildren<Canvas>( true );
			foreach( Canvas canvas in all_canvas )
			{
				if( canvas == mRootCanvas )
					continue;

				if( canvas.overrideSorting )
				{
					if( canvas.sortingOrder >= base_order )
						canvas.sortingOrder += AddedOrderInLayer;
					else
						canvas.sortingOrder += base_order + AddedOrderInLayer;
				}
			}

			if( mPopupProperty.HasTypeFlag( eUIPopupTypeFlag.NoCloseButton ) )
			{
				if( m_CloseButton != null )
					m_CloseButton.gameObject.SetActive( false );
			}

			if( mPopupProperty.HasTypeFlag( eUIPopupTypeFlag.Modeless ) )
			{
				// for Modeless
				if( m_BackgroundBlock != null )
					m_BackgroundBlock.gameObject.SetActive( false );
			}
			else
			{
				// for Modal
				if( m_BackgroundBlock != null )
				{
					m_BackgroundBlock.SetAlphaExt( mPopupProperty.m_PopupBlockAlpha );
					m_BackgroundBlock.raycastTarget = true;

					if( mPopupProperty.HasTypeFlag( eUIPopupTypeFlag.NoCloseOutsideFrame ) == false )
					{
						EventTrigger evt_trigger = m_BackgroundBlock.gameObject.GetComponent<EventTrigger>();
						if( evt_trigger == null )
							evt_trigger = m_BackgroundBlock.gameObject.AddComponent<EventTrigger>();

						if( evt_trigger.triggers.Exists( a => a.eventID == EventTriggerType.PointerClick ) == false )
						{
							EventTrigger.Entry pointer_click_event = new EventTrigger.Entry();
							pointer_click_event.eventID = EventTriggerType.PointerClick;
							pointer_click_event.callback.AddListener( OnBackgroundClicked );
							evt_trigger.triggers.Add( pointer_click_event );
						}
					}
				}
			}

			if( mPopupProperty.HasTypeFlag( eUIPopupTypeFlag.Draggable ) )
			{
				if( m_Frame != null )
				{
					// TODO : add draggable component
				}
			}

			if( m_CloseButton != null && m_CloseButton.onClick.GetPersistentEventCount() == 0 )
			{
				m_CloseButton.onClick.AddListener( OnCloseClicked );
			}
		}

		//------------------------------------------------------------------------
		public virtual void Open( WaitForFinish wait )
		{
			if( mPopupProperty.m_OpenCallback != null )
				mPopupProperty.m_OpenCallback( this );

			wait.IncrementWaitCount();
			PlayOpenAnimation( () =>
			{
				wait.DecrementWaitCount();
			} );
		}

		//------------------------------------------------------------------------
		public virtual void OnBackgroundClicked( BaseEventData data )
		{
			if( mPopupProperty.HasTypeFlag( eUIPopupTypeFlag.NoCloseOutsideFrame ) == false )
				Close();
		}

		//------------------------------------------------------------------------
		public virtual void OnCloseClicked()
		{
			Close();
		}

		//------------------------------------------------------------------------
		public void CloseSilent()
		{
			mPopupProperty.SetTypeFlag( eUIPopupTypeFlag.NoPlayCloseSound | eUIPopupTypeFlag.NoPopupAnimation, true );
			Close();
		}

		//------------------------------------------------------------------------
		public virtual void Close()
		{
			if( IsClosed )
				return;

			IsClosed = true;
			CloseInternal();
		}

		//------------------------------------------------------------------------
		public virtual void ForcedClose( bool ignore_callback )
		{
			if( ignore_callback )
				mPopupProperty.m_Callback = null;

			CloseInternal();
		}

		//------------------------------------------------------------------------
		protected virtual void CloseInternal()
		{
			if( mPopupProperty.HasTypeFlag( eUIPopupTypeFlag.NoPlayCloseSound ) == false )
			{
				// TODO : sound
			}

			PlayCloseAnimation( () =>
			{
				UIPopupManager.Instance.ClosePopup( this );
			} );
		}

		//------------------------------------------------------------------------	
		public virtual void PlayOpenAnimation( System.Action callback )
		{
			if( mPopupProperty.HasTypeFlag( eUIPopupTypeFlag.NoPopupAnimation ) )
			{
				callback?.Invoke();
			}
			else
			{
				callback?.Invoke();
			}
		}
		public virtual void PlayCloseAnimation( System.Action callback )
		{
			if( mPopupProperty.HasTypeFlag( eUIPopupTypeFlag.NoPopupAnimation ) )
			{
				callback?.Invoke();
			}
			else
			{
				callback?.Invoke();
			}
		}

		//------------------------------------------------------------------------
		public virtual void Begin() { }
		public virtual void OnReActive( string closed_popup_name, bool closed_popup_has_interaction ) { }

		//------------------------------------------------------------------------
		public static bool IsValidParam<T>( object[] parms, int idx )
		{
			if( parms == null )
				return false;

			if( idx < 0 || idx >= parms.Length )
				return false;

			System.Type t_type = typeof( T );

			if( t_type == parms[idx].GetType() )
				return true;

			return false;
		}

#if UNITY_EDITOR || UMDEV
		public virtual void TestHide( bool bHide ) { }
#endif
	}
}