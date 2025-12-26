//////////////////////////////////////////////////////////////////////////
//
// PointerTypeTrigger
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

using UMF.Unity.EXTERNAL;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UMF.Unity
{
	[System.Serializable]
	public class PointerTypeEvent : UnityEvent<PointerTypeTrigger.ePointerType, PointerEventData> { }

	public class PointerTypeTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
	{
		public enum ePointerType
		{
			Enter,
			Exit,
			Move,
			Clicked,
			Down,
			Up,
		}

		public PointerTypeEvent m_Event;

		public void OnPointerEnter( PointerEventData eventData )
		{
			if( Win32Manager.IsApplicationFocus == false )
				return;

			m_Event?.Invoke( ePointerType.Enter, eventData );
		}

		public void OnPointerExit( PointerEventData eventData )
		{
			if( Win32Manager.IsApplicationFocus == false )
				return;

			m_Event?.Invoke( ePointerType.Exit, eventData );
		}

		public void OnPointerMove( PointerEventData eventData )
		{
			if( Win32Manager.IsApplicationFocus == false )
				return;

			m_Event?.Invoke( ePointerType.Move, eventData );
		}

		public void OnPointerClick( PointerEventData eventData )
		{
			if( Win32Manager.IsApplicationFocus == false )
				return;

			m_Event?.Invoke( ePointerType.Clicked, eventData );
		}

		public void OnPointerDown( PointerEventData eventData )
		{
			if( Win32Manager.IsApplicationFocus == false )
				return;

			m_Event?.Invoke( ePointerType.Down, eventData );
		}

		public void OnPointerUp( PointerEventData eventData )
		{
			if( Win32Manager.IsApplicationFocus == false )
				return;

			m_Event?.Invoke( ePointerType.Up, eventData );
		}

		//public void OnPointerTypeTrigger( PointerTypeTrigger.ePointerType pointer_type, UnityEngine.EventSystems.PointerEventData event_data )
		//{
		//	switch( pointer_type )
		//	{
		//		case PointerTypeTrigger.ePointerType.Enter:
		//			break;

		//		case PointerTypeTrigger.ePointerType.Exit:
		//			break;

		//		case PointerTypeTrigger.ePointerType.Move:
		//			break;

		//		case PointerTypeTrigger.ePointerType.Clicked:
		//			if(event_data.button != PointerEventData.InputButton.Left )
		//				return;
		//			break;
		//	}
		//}


	}
}