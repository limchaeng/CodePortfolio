//////////////////////////////////////////////////////////////////////////
//
// PointerClickTrigger
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
using UMF.Unity.EXTERNAL;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UMF.Unity
{

	[System.Serializable]
	public class ClickEvent : UnityEvent<PointerClickTrigger.ePointerType, PointerEventData> { }

	public class PointerClickTrigger : MonoBehaviour, IPointerClickHandler
	{
		public enum ePointerType
		{
			Left,
			Middle,
			Right,
		}

		public ClickEvent m_ClickEvent;

		public void OnPointerClick( PointerEventData eventData )
		{
			if( Win32Manager.IsApplicationFocus == false )
				return;

			switch( eventData.button )
			{
				case PointerEventData.InputButton.Left: m_ClickEvent?.Invoke( ePointerType.Left, eventData ); break;
				case PointerEventData.InputButton.Middle: m_ClickEvent?.Invoke( ePointerType.Middle, eventData ); break;
				case PointerEventData.InputButton.Right: m_ClickEvent?.Invoke( ePointerType.Right, eventData ); break;
			}
		}

		//public void OnPointerClickTrigger( PointerClickTrigger.ePointerType pointer_type, UnityEngine.EventSystems.PointerEventData event_data )
		//{
		//	switch( pointer_type )
		//	{
		//		case PointerClickTrigger.ePointerType.Left:
		//			break;

		//		case PointerClickTrigger.ePointerType.Middle:
		//			break;

		//		case PointerClickTrigger.ePointerType.Right:
		//			break;

		//	}
		//}
	}
}
