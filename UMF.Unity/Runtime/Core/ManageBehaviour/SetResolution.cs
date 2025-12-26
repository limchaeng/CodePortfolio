//////////////////////////////////////////////////////////////////////////
//
// SetResolution
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
using UMF.Unity.EXTERNAL;
using UnityEngine.Animations;

namespace UMF.Unity
{
	public class SetResolution : SingletonBehaviour<SetResolution>
	{
		public enum ePerfectType
		{
			Width,
			Height,
			Auto,
		}

		public Vector2 m_BaseResolution = new Vector2( 1920f, 1080f );
		public ePerfectType m_PerfectType = ePerfectType.Auto;
		public float m_BaseFOV = 60f;
		public float m_PixelPerUnit = 1f;
		public Camera m_MainCamera;
		public List<Camera> m_ChildMainCameras = new List<Camera>();
		public List<Transform> m_RepositionZeroBaseZChilds = new List<Transform>();
		public Light m_MainDirectionalLight;
		public Camera m_Main2DCamera;		
		public Camera m_UICamera;
		public Camera m_LBCamera;
		public Color m_CameraBackgroundSolidColor = Color.black;
		public Color m_MainCameraGizmoColor = Color.green;
		public Color m_UICameraGizmoColor = Color.green;

		public static Vector2 BASE_RESOLUTION = Vector2.zero;
		public static float BASE_FOV = 0f;
		public static float BASE_ORTHO_SIZE = 0f;
		public static Vector2 CURRENT_ASPECT_RATIO = Vector2.zero;

		public delegate void delegateUpdateResolution();
		public static event delegateUpdateResolution UPDATE_RESOLUTION_EVENT;

		bool mHoldBaseResolutionAspect = false;

		System.Text.StringBuilder mDebugInfo = new System.Text.StringBuilder();
		public string DebugInfo
		{
			get { return mDebugInfo.ToString(); }
		}
		
		//------------------------------------------------------------------------
		protected override void Awake()
		{
			base.Awake();
			
			if( m_MainCamera == null )
				m_MainCamera = Camera.main;
		}

		public void Init()
		{
			mHoldBaseResolutionAspect = false;
#if UNITY_WSA
			mHoldBaseResolutionAspect = true;	// UWP : 해상도에 따른 카메라 update 하지 않음(고정비율)
#endif
			CheckHoldAspect();
			UpdateResolution();
		}

		//------------------------------------------------------------------------
		public void CheckHoldAspect()
		{
			float deviceAspect = (float)Screen.width / (float)Screen.height;

			if( deviceAspect > Win32Manager.LockedAspect() )
			{
				HoldAspect();
				
				return;
			}
			else
			{
				if( Screen.height > Screen.width )
				{
					HoldAspect();
					
					return;
				}
			}

			if( m_LBCamera != null )
				m_LBCamera.gameObject.SetActive( false );
		}

		//------------------------------------------------------------------------
		public void HoldAspect()
		{
			Vector2 resTarget = new Vector2( m_BaseResolution.x, m_BaseResolution.y );
			Vector2 resViewport = new Vector2( Screen.width, Screen.height );
			Vector2 resNormalized = resTarget / resViewport;
			Vector2 size = resNormalized / Mathf.Max( resNormalized.x, resNormalized.y );
				
			m_MainCamera.rect = new Rect( default, size ) { center = new Vector2( 0.5f, 0.5f ) };
		
			if( m_UICamera != null )
				m_UICamera.rect = new Rect( default, size ) { center = new Vector2( 0.5f, 0.5f ) };

			if( m_Main2DCamera != null )
				m_Main2DCamera.rect = new Rect( default, size ) { center = new Vector2( 0.5f, 0.5f ) };

			if( m_LBCamera != null )
				m_LBCamera.gameObject.SetActive( true );
		}


		public void UpdateResolutionFromWin32( int width, int height, bool is_fullscreen )
		{
			// 윈도우 창 크기 변경시에는 업데이트 해주지 않는다.(테스트) : 고정해상도 비율 초기에 init 됨.
			//UpdateResolution( true );
			InvokeResultionEvent();
		}

		//------------------------------------------------------------------------
		[ContextMenu( "UpdateResolution" )]
		public void UpdateResolution()
		{
			UpdateResolution( false );
		}
		public void UpdateResolution( bool is_window_changed )
		{
			mDebugInfo.Clear();

			BASE_RESOLUTION = m_BaseResolution;
			BASE_FOV = m_MainCamera.fieldOfView;
			if( m_UICamera != null )
				BASE_ORTHO_SIZE = m_UICamera.orthographicSize;

			// calculation perspective camera
			float s_width = m_MainCamera.pixelWidth;
			float s_height = m_MainCamera.pixelHeight;

			if( mHoldBaseResolutionAspect )
			{
				s_width = m_BaseResolution.x;
				s_height = m_BaseResolution.y;
			}

			float adjust_height = m_BaseResolution.y / s_height;
			float adjust_width = m_BaseResolution.x / s_width;

			Vector2 screen_size = new Vector2( Screen.width, Screen.height );

			mDebugInfo.AppendLine( $"Device:{SystemInfo.deviceType}" );
			mDebugInfo.AppendLine( $"Resolution MainCameraPixel:{s_width}x{s_height} Base:{m_BaseResolution}(fov:{BASE_FOV})" );
			mDebugInfo.AppendLine( $"- Screen:{screen_size.x}x{screen_size.y} full:{Screen.fullScreen}/{Screen.fullScreenMode} adjust:{adjust_width}x{adjust_height}" );
			mDebugInfo.AppendLine( $"- Camera pixelRect:{m_MainCamera.pixelRect} aspect:{m_MainCamera.aspect}" );
			mDebugInfo.AppendLine( $"- SafeArea:{Screen.safeArea}" );

			Rect[] cutouts = Screen.cutouts;
			if( cutouts != null )
			{
				for( int i = 0; i < cutouts.Length; i++ )
				{
					mDebugInfo.AppendLine( string.Format( "- Cutouts({0}) : {1}", i, cutouts[i] ) );
				}
			}

			// base distance
			if( m_MainCamera.orthographic == false )
			{
				float adjust_perfect = 0f;
				switch( m_PerfectType )
				{
					case ePerfectType.Width: adjust_perfect = adjust_width; break;
					case ePerfectType.Height: adjust_perfect = adjust_height; break;
					default: adjust_perfect = Mathf.Max( adjust_width, adjust_height ); break;
				}
				float distance = s_height * 0.5f / Mathf.Tan( m_BaseFOV * 0.5f * Mathf.Deg2Rad ) / m_PixelPerUnit * adjust_perfect * -1f;
				mDebugInfo.AppendLine( $"- Base Distance : {distance}" );

				// camera setting
				m_MainCamera.fieldOfView = m_BaseFOV;
				m_MainCamera.transform.position = new Vector3( 0f, 0f, distance );
				if( m_MainDirectionalLight != null )
					m_MainDirectionalLight.transform.position = new Vector3( 0f, 0f, distance );

				foreach( Transform t in m_RepositionZeroBaseZChilds )
				{
					Vector3 vpos = t.localPosition;
					vpos.z = distance * -1f;
					t.localPosition = vpos;

					PositionConstraint pos_constraint = t.GetComponent<PositionConstraint>();
					if( pos_constraint != null )
					{
						pos_constraint.locked = false;
						pos_constraint.translationAtRest = vpos;
						pos_constraint.translationOffset = Vector3.zero;
						pos_constraint.locked = true;
					}
				}
			}
			else
			{
				m_MainCamera.orthographicSize = m_BaseResolution.y / 2f / m_PixelPerUnit;
			}

			m_MainCamera.clearFlags = CameraClearFlags.SolidColor;

			if( m_Main2DCamera != null )
			{
				m_Main2DCamera.transform.position = m_MainCamera.transform.position;
				m_Main2DCamera.clearFlags = CameraClearFlags.Depth;
			}

			if( Application.isEditor )
			{
#if UNITY_EDITOR
				Vector2 gameview_size = UnityEditor.Handles.GetMainGameViewSize();
				mDebugInfo.AppendLine( $"- Editor GameView:{gameview_size}" );
				screen_size = gameview_size;
				tmp_last_gameview_size = gameview_size;
#endif

				if( Application.isPlaying == false )
				{
					m_MainCamera.backgroundColor = m_CameraBackgroundSolidColor;
					if( m_Main2DCamera != null )
						m_Main2DCamera.backgroundColor = m_CameraBackgroundSolidColor;

					if( m_UICamera != null )
						m_UICamera.backgroundColor = m_CameraBackgroundSolidColor;
				}
				else
				{
					m_MainCamera.backgroundColor = Color.black;
					if( m_Main2DCamera != null )
						m_Main2DCamera.backgroundColor = Color.black;

					if( m_UICamera != null )
						m_UICamera.backgroundColor = Color.black;
				}
			}

			m_MainCamera.depth = 0;

			if( m_UICamera != null )
			{
				m_UICamera.orthographicSize = m_BaseResolution.y / 2f / m_PixelPerUnit;
				m_UICamera.clearFlags = CameraClearFlags.Depth;
				m_UICamera.depth = 100;
			}

			if( m_LBCamera != null )
			{
				m_LBCamera.orthographicSize = m_BaseResolution.y / 2f / m_PixelPerUnit;
				m_LBCamera.depth = -100;
			}

			if( m_Main2DCamera != null )
			{
				m_Main2DCamera.orthographic = true;
				m_Main2DCamera.orthographicSize = m_BaseResolution.y / 2f / m_PixelPerUnit;
				m_Main2DCamera.clearFlags = CameraClearFlags.Depth;
				m_Main2DCamera.depth = 50;
			}

			// for window box : HoldAspect 사용으로 사용안함
			//if( m_BaseResolution.x / m_BaseResolution.y < screen_size.x / screen_size.y )
			//{
			//	float newWidth = ( m_BaseResolution.x / m_BaseResolution.y ) / ( screen_size.x / screen_size.y );
			//	m_MainCamera.rect = new Rect( ( 1f - newWidth ) / 2f, 0f, newWidth, 1f );
			//	if( m_UICamera != null )
			//		m_UICamera.rect = new Rect( ( 1f - newWidth ) / 2f, 0f, newWidth, 1f );

			//	if( m_Main2DCamera != null )
			//		m_Main2DCamera.rect = new Rect( ( 1f - newWidth ) / 2f, 0f, newWidth, 1f );

			//	mDebugInfo.AppendLine( $"Rect A : {m_MainCamera.rect}" );
			//}
			//else
			//{
			//	float newHeight = ( (float)screen_size.x / screen_size.y ) / ( m_BaseResolution.x / m_BaseResolution.y );
			//	m_MainCamera.rect = new Rect( 0f, ( 1f - newHeight ) / 2f, 1f, newHeight );
			//	if( m_UICamera != null )
			//		m_UICamera.rect = new Rect( 0f, ( 1f - newHeight ) / 2f, 1f, newHeight );

			//	if( m_Main2DCamera != null )
			//		m_Main2DCamera.rect = new Rect( 0f, ( 1f - newHeight ) / 2f, 1f, newHeight );

			//	mDebugInfo.AppendLine( $"Rect B : {m_MainCamera.rect}" );
			//}

			CURRENT_ASPECT_RATIO.x = screen_size.x / BASE_RESOLUTION.x;
			CURRENT_ASPECT_RATIO.y = screen_size.y / BASE_RESOLUTION.y;

			if( m_ChildMainCameras.Count > 0 )
			{
				foreach( Camera other_main_cam in m_ChildMainCameras )
				{
					other_main_cam.fieldOfView = m_MainCamera.fieldOfView;
					other_main_cam.orthographic = m_MainCamera.orthographic;
					other_main_cam.orthographicSize = m_Main2DCamera.orthographicSize;
					//other_main_cam.rect = m_MainCamera.rect;
				}
			}

			if( Application.isEditor == false || is_window_changed == false )
				Debug.Log( mDebugInfo.ToString() );

			InvokeResultionEvent();
		}

		public static void InvokeResultionEvent()
		{
			UPDATE_RESOLUTION_EVENT?.Invoke();
		}

		public void ChangeResolution( int width, int height, bool fullScreen )
		{
			StartCoroutine( AdjustResolution( width, height, fullScreen ) );
		}

		// 같은 Frame에 전체화면 <-> 창모드, 해상도 변경이 동시에 동작되지 않아 모드 변경 후 1~2 프레임 대기
		private IEnumerator AdjustResolution( int width, int height, bool fullScreen )
		{
			yield return new WaitForEndOfFrame();

			Screen.SetResolution( width, height, fullScreen );
			
			if( fullScreen )
				CheckHoldAspect();
			else
				Win32Manager.SetCurrentResolution( width, height );
			
			yield return new WaitForEndOfFrame();

			UpdateResolutionFromWin32( width, height, fullScreen );
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Matrix4x4 tmp_matrix = Gizmos.matrix;
			Color tmp_color = Gizmos.color;

			if( m_MainCamera != null && UnityEditor.Selection.activeGameObject != m_MainCamera.gameObject )
			{
				Gizmos.color = m_MainCameraGizmoColor; ;
				Gizmos.matrix = Matrix4x4.TRS( m_MainCamera.transform.position, m_MainCamera.transform.rotation, Vector3.one );
				Gizmos.DrawFrustum( Vector3.zero, m_MainCamera.fieldOfView, m_MainCamera.farClipPlane, m_MainCamera.nearClipPlane, m_MainCamera.aspect );
			}

			Gizmos.color = tmp_color;
			Gizmos.matrix = tmp_matrix;

			if( m_UICamera != null && UnityEditor.Selection.activeGameObject != m_UICamera.gameObject )
			{
				Gizmos.color = m_UICameraGizmoColor;

				if( m_UICamera.orthographic )
				{
					float size = m_UICamera.orthographicSize;
					float spread = m_UICamera.farClipPlane - m_UICamera.nearClipPlane;
					float center = ( m_UICamera.farClipPlane + m_UICamera.nearClipPlane ) * 0.5f;

					Gizmos.DrawWireCube( new Vector3( 0f, 0f, center ), new Vector3( size * 2f * m_UICamera.aspect, size * 2f, spread ) );
				}
				else
				{
					Gizmos.matrix = Matrix4x4.TRS( m_UICamera.transform.position, m_UICamera.transform.rotation, Vector3.one );
					Gizmos.DrawFrustum( Vector3.zero, m_UICamera.fieldOfView, m_UICamera.farClipPlane, m_UICamera.nearClipPlane, m_UICamera.aspect );
				}
			}

			if( m_Main2DCamera != null && UnityEditor.Selection.activeGameObject != m_Main2DCamera.gameObject )
			{
				Gizmos.color = m_MainCameraGizmoColor;

				if( m_Main2DCamera.orthographic )
				{
					float size = m_Main2DCamera.orthographicSize;
					float spread = m_Main2DCamera.farClipPlane - m_Main2DCamera.nearClipPlane;
					float center = ( m_Main2DCamera.farClipPlane + m_Main2DCamera.nearClipPlane ) * 0.5f;

					Gizmos.DrawWireCube( new Vector3( 0f, 0f, center ), new Vector3( size * 2f * m_Main2DCamera.aspect, size * 2f, spread ) );
				}
				else
				{
					Gizmos.matrix = Matrix4x4.TRS( m_Main2DCamera.transform.position, m_Main2DCamera.transform.rotation, Vector3.one );
					Gizmos.DrawFrustum( Vector3.zero, m_Main2DCamera.fieldOfView, m_Main2DCamera.farClipPlane, m_Main2DCamera.nearClipPlane, m_Main2DCamera.aspect );
				}
			}

			Gizmos.color = tmp_color;
			Gizmos.matrix = tmp_matrix;
		}

		Vector2 tmp_last_gameview_size = Vector2.zero;
		private void LateUpdate()
		{
			if( Application.isPlaying )
			{
				Vector2 gameview_size = UnityEditor.Handles.GetMainGameViewSize();
				if( gameview_size != tmp_last_gameview_size )
				{
#if UNITY_EDITOR_WIN
					CheckHoldAspect();
#endif
					UpdateResolution( true );
				}
			}
		}
#endif
	}
}