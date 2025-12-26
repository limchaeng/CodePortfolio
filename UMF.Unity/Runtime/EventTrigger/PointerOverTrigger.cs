using UMF.Unity.EXTERNAL;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UMF.Unity
{
	[System.Serializable]
	public class PointerOverEvent : UnityEvent<PointerOverTrigger.ePointerType, PointerEventData> { }
	public class PointerOverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
	{
		public enum ePointerType
		{
			Enter,
			Exit,
			Move,
		}

		public PointerOverEvent m_Event;



		public void Call( ePointerType pointer_type, PointerEventData event_data )
		{
			switch( pointer_type )
			{
				case ePointerType.Enter: OnPointerEnter( event_data ); break;
				case ePointerType.Exit: OnPointerExit( event_data ); break;
				case ePointerType.Move: OnPointerMove( event_data ); break;
			}
		}

		public virtual void OnPointerEnter( PointerEventData eventData )
		{
			if( Win32Manager.IsApplicationFocus == false )
				return;

			m_Event?.Invoke( ePointerType.Enter, eventData );
		}

		public virtual void OnPointerExit( PointerEventData eventData )
		{
			if( Win32Manager.IsApplicationFocus == false )
				return;

			m_Event?.Invoke( ePointerType.Exit, eventData );
		}

		public virtual void OnPointerMove( PointerEventData eventData )
		{
			if( Win32Manager.IsApplicationFocus == false )
				return;

			m_Event?.Invoke( ePointerType.Move, eventData );
		}

		//------------------------------------------------------------------------
		public virtual void OnApplicationFocus( bool focus )
		{
			if( focus == false )
			{
				m_Event?.Invoke( ePointerType.Exit, null );
			}

		}

		//public void OnPointerOverTrigger( PointerOverTrigger.ePointerType pointer_type, UnityEngine.EventSystems.PointerEventData event_data )
		//{
		//	switch( pointer_type )
		//	{
		//		case PointerOverTrigger.ePointerType.Enter:
		//			break;

		//		// event_data nullable;
		//		case PointerOverTrigger.ePointerType.Exit:
		//			break;

		//		case PointerOverTrigger.ePointerType.Move:
		//			break;

		//	}
		//}
	}
}

