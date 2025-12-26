//////////////////////////////////////////////////////////////////////////
//
// Win32Manager
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// ref 
//////////////////////////////////////////////////////////////////////////
#if !( UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN )
#define DISABLEWIN32API
#endif

using AOT;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This script enforces the set aspect ratio for the Unity game window. That means that you can resize the window but
/// it will always keep the aspect ratio you set.
/// 
/// This is done by intercepting window resize events (WindowProc callback) and modifying them accordingly.
/// You can also set a min/max width and height in pixel for the window.
/// Both the aspect ratio and the min/max resolutions refer to the game area, so, as you'd expect, the window
/// title bar and borders aren't included.
///
/// This script will also enforce the aspect ratio when the application is in fullscreen. When you switch to fullscreen,
/// the application will automatically be set to the maximum possible resolution on the current monitor while still keeping
/// the aspect ratio. If the monitor doesn't have the same aspect ratio, black bars will be added to the left/right or top/bottom.
///
/// Make sure you activate "Resizable Window" in the player settings, otherwise your window won't be resizable.
/// You might also want to uncheck all unsupported aspect ratios under "Supported Aspect Ratios" in the player settings.
/// 
/// NOTE: This uses WinAPI, so it will only work on Windows. Tested on Windows 10, but should work on all recent versions.
/// </summary>

namespace UMF.Unity.EXTERNAL
{
	public class Win32Manager : SingletonBehaviour<Win32Manager>
	{
		/// <summary>
		/// This event gets triggered every time the window resolution changes or the user toggles fullscreen.
		/// The parameters are the new width, height and fullscreen state (true means fullscreen).
		/// </summary>
		public ResolutionChangedEvent resolutionChangedEvent;
		[Serializable]
		public class ResolutionChangedEvent : UnityEvent<int, int, bool> { }
		protected static ResolutionChangedEvent resolutionChangedEventwnd;

		// If false, switching to fullscreen is blocked.
		[SerializeField]
		protected bool m_allowFullscreen = true;
		public bool AllowFullscreen { get => m_allowFullscreen; set => m_allowFullscreen = value; }

		// Aspect ratio width and height.
		[SerializeField]
		protected float m_aspectRatioWidth = 16;
		public float AspectRatioWidth { get => m_aspectRatioWidth; set => m_aspectRatioWidth = value; }

		[SerializeField]
		protected float m_aspectRatioHeight = 9;
		public float AspectRatioHeight { get => m_aspectRatioHeight; set => m_aspectRatioHeight = value; }

		// Currently locked aspect ratio.
		protected static float mLockedAspect;

		// Minimum and maximum values for window width/height in pixel.
		protected static int m_minWidthPixel = 512;
		public int MinWidthPixel { get => m_minWidthPixel; set => m_minWidthPixel = value; }
		public int MinHeightPixel { get { return Mathf.RoundToInt( m_minWidthPixel / mLockedAspect ); } }

		protected static int m_maxWidthPixel = 2048;
		public int MaxWidthPixel { get => m_maxWidthPixel; set => m_maxWidthPixel = value; }
		public int MaxHeightPixel { get { return Mathf.RoundToInt( m_maxWidthPixel / mLockedAspect ); } }

		// Width and height of game area. This does not include borders and the window title bar.
		// The values are set once the window is resized by the user.
		protected static int setWidth = -1;
		protected static int setHeight = -1;

		// Fullscreen state at last frame.
		protected bool wasFullscreenLastFrame;

		// Is the AspectRatioController initialized?
		// Gets set to true once the WindowProc callback is registered.
		protected bool started;

		// Width and height of active monitor. This is the monitor the window is currently on.
		protected int pixelHeightOfCurrentScreen;
		protected int pixelWidthOfCurrentScreen;

		// Gets set to true once user requests the application to be terminated.
		protected bool quitStarted;

		public static bool IsApplicationFocus { get; protected set; }
		public void OnApplicationFocus( bool focus )
		{
			IsApplicationFocus = focus;
		}

		protected static int mCurrentWidth;
		protected static int mCurrentHeight;

#if !DISABLEWIN32API
		// WinAPI related definitions.
		#region WINAPI

		// The WM_SIZING message is sent to a window through the WindowProc callback
		// when the window is resized.
		protected const int WM_SIZING = 0x214;
		protected const int WM_SIZE = 0x0005;
		protected const int WM_NCLBUTTONDBLCLK = 0x00A3;

		// Parameters for WM_SIZING message.
		protected const int WMSZ_LEFT = 1;
		protected const int WMSZ_RIGHT = 2;
		protected const int WMSZ_TOP = 3;
		protected const int WMSZ_BOTTOM = 6;

		// Parameters for WM_SIZING message.
		protected const int SIZE_MAXIMIZED = 2;

		// Retreives pointer to WindowProc function.
		protected const int GWLP_WNDPROC = -4;	

		// Delegate to set as new WindowProc callback function.
		protected delegate IntPtr WndProcDelegate( IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam );
		protected WndProcDelegate wndProcDelegate;

		// Retrieves the thread identifier of the calling thread.
		[DllImport( "kernel32.dll" )]
		protected static extern uint GetCurrentThreadId();

		[DllImport( "kernel32.dll" )]
		static extern IntPtr LoadLibrary( string lpLibFileName );
		// Retrieves the name of the class to which the specified window belongs.
		[DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
		protected static extern int GetClassName( IntPtr hWnd, StringBuilder lpString, int nMaxCount );

		// Enumerates all nonchild windows associated with a thread by passing the handle to
		// each window, in turn, to an application-defined callback function.
		public delegate bool EnumThreadDelegate( IntPtr Hwnd, IntPtr lParam );

		[DllImport( "user32.dll", CharSet = CharSet.Auto )]
		protected static extern bool EnumThreadWindows( uint dwThreadId, EnumThreadDelegate lpEnumFunc, IntPtr lParam );
		//protected delegate bool EnumWindowsProc( IntPtr hWnd, IntPtr lParam );

		// Passes message information to the specified window procedure.
		[DllImport( "user32.dll" )]
		protected static extern IntPtr CallWindowProc( IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam );

		// Retrieves the dimensions of the bounding rectangle of the specified window.
		// The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
		[DllImport( "user32.dll", SetLastError = true )]
		protected static extern bool GetWindowRect( IntPtr hwnd, ref RECT lpRect );

		// Retrieves the coordinates of a window's client area. The client coordinates specify the upper-left
		// and lower-right corners of the client area. Because client coordinates are relative to the upper-left
		// corner of a window's client area, the coordinates of the upper-left corner are (0,0).
		[DllImport( "user32.dll" )]
		protected static extern bool GetClientRect( IntPtr hWnd, ref RECT lpRect );

		// Changes an attribute of the specified window. The function also sets the 32-bit (long) value
		// at the specified offset into the extra window memory.
		[DllImport( "user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto )]
		protected static extern IntPtr SetWindowLong32( IntPtr hWnd, int nIndex, IntPtr dwNewLong );

		// Changes an attribute of the specified window. The function also sets a value at the specified
		// offset in the extra window memory. 
		[DllImport( "user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto )]
		protected static extern IntPtr SetWindowLongPtr64( IntPtr hWnd, int nIndex, IntPtr dwNewLong );

		[DllImport( "user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Unicode )]
		protected static extern bool SetWindowText( IntPtr hwnd, String lpString );


		// Name of the Unity window class used to find the window handle.
		protected const string UNITY_WND_CLASSNAME = "UnityWndClass";

		// Window handle of Unity window.
		protected static IntPtr unityHWnd;

		// Pointer to old WindowProc callback function.
		protected static IntPtr oldWndProcPtr;

		// Pointer to our own WindowProc callback function.
		protected static IntPtr newWndProcPtr;

		/// <summary>
		/// WinAPI RECT definition.
		/// </summary>
		[StructLayout( LayoutKind.Sequential )]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

#endregion

#endif
		public void Init()
		{
			// Apply aspect ratio to current resolution.
			SetAspectRatio( AspectRatioWidth, AspectRatioHeight );
			
			// Don't register WindowProc callback in Unity editor, because it would refer to the
			// Unity editor window, not the actual game window.
			if( Application.isEditor )
			{
				return;
			}

			resolutionChangedEventwnd = resolutionChangedEvent;

			Debug.Log( $"Win32Manager Init lockedAspect={mLockedAspect}" );

#if UNITY_WSA
			UnityEngine.WSA.Application.windowSizeChanged += OnWSAWindowChanged;
#if ENABLE_WINMD_SUPPORT
			Debug.Log( "- Support : ENABLE_WINMD_SUPPORT" );
#endif
#endif

#if !DISABLEWIN32API
			// Register callback for then application wants to quit.
			Application.wantsToQuit += ApplicationWantsToQuit;
			
			IntPtr kernel32 = LoadLibrary( "kernel32.dll" );

			uint threadid = GetCurrentThreadId();

			// Find window handle of main Unity window.
			EnumThreadWindows( threadid, EnumThreadWindowsCallback, IntPtr.Zero );

			// Save current fullscreen state.
			wasFullscreenLastFrame = Screen.fullScreen;

			// Register (replace) WindowProc callback. This gets called every time a window event is triggered,
			// such as resizing or moving the window.
			// Also save old WindowProc callback, as we will have to call it from our own new callback.
			setWndProc();

			newWndProcPtr = Marshal.GetFunctionPointerForDelegate( wndProcDelegate );
			oldWndProcPtr = SetWindowLong( unityHWnd, GWLP_WNDPROC, newWndProcPtr );
#endif
			// Initialization complete.
			started = true;
		}

		/// <summary>
		/// Sets the target aspect ratio to the given aspect ratio.
		/// </summary>
		/// <param name="newAspectWidth">New width of the aspect ratio.</param>
		/// <param name="newAspectHeight">New height of the aspect ratio.</param>
		/// <param name="apply">If true, the current window resolution is immediately adjusted to match the new
		/// aspect ratio. If false, this is only done the next time the window is resized manually.</param>
		protected void SetAspectRatio( float newAspectWidth, float newAspectHeight )
		{
			// Calculate new aspect ratio.
			AspectRatioWidth = newAspectWidth;
			AspectRatioHeight = newAspectHeight;
			mLockedAspect = AspectRatioWidth / AspectRatioHeight;
		}


#if !DISABLEWIN32API
		protected virtual void setWndProc()
		{
			wndProcDelegate = wndProc;
		}

		[MonoPInvokeCallback( typeof( EnumThreadDelegate ) )]
		protected static bool EnumThreadWindowsCallback( IntPtr hWnd, IntPtr lParam )
		{
			var classText = new StringBuilder( UNITY_WND_CLASSNAME.Length + 1 );
			GetClassName( hWnd, classText, classText.Capacity );

			if( classText.ToString() == UNITY_WND_CLASSNAME )
			{
				unityHWnd = hWnd;
				return false;
			}
			return true;
		}

		/// <summary>
		/// WindowProc callback. An application-defined function that processes messages sent to a window. 
		/// </summary>
		/// <param name="hWnd">A handle to the window. </param>
		/// <param name="msg">The message used to identify the event.</param>
		/// <param name="wParam">Additional message information. The contents of this parameter
		/// depend on the value of the uMsg parameter. </param>
		/// <param name="lParam">Additional message information. The contents of this parameter
		/// depend on the value of the uMsg parameter. </param>
		/// <returns></returns>
		[MonoPInvokeCallback( typeof( WndProcDelegate ) )]
		protected static IntPtr wndProc( IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam )
		{
			switch( msg )
			{
				case WM_SIZING:
					// Get window size struct.
					RECT rc = (RECT)Marshal.PtrToStructure( lParam, typeof( RECT ) );

					// Calculate window border width and height.
					RECT windowRect = new RECT();
					GetWindowRect( unityHWnd, ref windowRect );

					RECT clientRect = new RECT();
					GetClientRect( unityHWnd, ref clientRect );

					int borderWidth = windowRect.Right - windowRect.Left - ( clientRect.Right - clientRect.Left );
					int borderHeight = windowRect.Bottom - windowRect.Top - ( clientRect.Bottom - clientRect.Top );
					//Debug.LogFormat( "rc.Right = {0} / rc.Bottom = {1}", rc.Right, rc.Bottom );

					// Remove borders (including window title bar) before applying aspect ratio.
					rc.Right -= borderWidth;
					rc.Bottom -= borderHeight;

					// Clamp window size.
					int minHeightPixel = Mathf.RoundToInt( m_minWidthPixel / mLockedAspect );
					int maxHeightPixel = Mathf.RoundToInt( m_maxWidthPixel / mLockedAspect );

					int newWidth = Mathf.Clamp( rc.Right - rc.Left, m_minWidthPixel, m_maxWidthPixel );
					int newHeight = Mathf.Clamp( rc.Bottom - rc.Top, minHeightPixel, maxHeightPixel );

					//Debug.LogFormat( "newWidth = {0} / newHeight = {1}", newWidth, newHeight );

					// Resize according to aspect ratio and resize direction.
					switch( wParam.ToInt32() )
					{
						case WMSZ_LEFT:
							rc.Left = rc.Right - newWidth;
							rc.Bottom = rc.Top + Mathf.RoundToInt( newWidth / mLockedAspect );
							break;
						case WMSZ_RIGHT:
							rc.Right = rc.Left + newWidth;
							rc.Bottom = rc.Top + Mathf.RoundToInt( newWidth / mLockedAspect );
							break;
						case WMSZ_TOP:
							rc.Top = rc.Bottom - newHeight;
							rc.Right = rc.Left + Mathf.RoundToInt( newHeight * mLockedAspect );
							break;
						case WMSZ_BOTTOM:
							rc.Bottom = rc.Top + newHeight;
							rc.Right = rc.Left + Mathf.RoundToInt( newHeight * mLockedAspect );
							break;
						case WMSZ_RIGHT + WMSZ_BOTTOM:
							rc.Right = rc.Left + newWidth;
							rc.Bottom = rc.Top + Mathf.RoundToInt( newWidth / mLockedAspect );
							break;
						case WMSZ_RIGHT + WMSZ_TOP:
							rc.Right = rc.Left + newWidth;
							rc.Top = rc.Bottom - Mathf.RoundToInt( newWidth / mLockedAspect );
							break;
						case WMSZ_LEFT + WMSZ_BOTTOM:
							rc.Left = rc.Right - newWidth;
							rc.Bottom = rc.Top + Mathf.RoundToInt( newWidth / mLockedAspect );
							break;
						case WMSZ_LEFT + WMSZ_TOP:
							rc.Left = rc.Right - newWidth;
							rc.Top = rc.Bottom - Mathf.RoundToInt( newWidth / mLockedAspect );
							break;
					}

					// Save actual Unity game area resolution.
					// This does not include borders.
					setWidth = rc.Right - rc.Left;
					setHeight = rc.Bottom - rc.Top;

					SetCurrentResolution( setWidth, setHeight );
					
					// Add back borders.
					rc.Right += borderWidth;
					rc.Bottom += borderHeight;

					// Trigger resolution change event.
					//resolutionChangedEventwnd.Invoke( setWidth, setHeight, Screen.fullScreen );

					// Write back changed window parameters.
					Marshal.StructureToPtr( rc, lParam, true );
					
					break;
				
				case WM_NCLBUTTONDBLCLK:
					return IntPtr.Zero;
			}

			// Call original WindowProc function.
			return CallWindowProc( oldWndProcPtr, hWnd, msg, wParam, lParam );
		}

		/// <summary>
		/// Calls SetWindowLong32 or SetWindowLongPtr64, depending on whether the executable is 32 or 64 bit.
		/// With this, we can build both 32 and 64 bit executables without running into problems.
		/// </summary>
		/// <param name="hWnd">The window handle.</param>
		/// <param name="nIndex">The zero-based offset to the value to be set.</param>
		/// <param name="dwNewLong">The replacement value.</param>
		/// <returns>If the function succeeds, the return value is the previous value of the specified offset. Otherwise zero.</returns>
		protected static IntPtr SetWindowLong( IntPtr hWnd, int nIndex, IntPtr dwNewLong )
		{
			// Check how long the IntPtr is. 4 byte means we're on 32 bit, so call functions accordingly.
			if( IntPtr.Size == 4 )
			{
				return SetWindowLong32( hWnd, nIndex, dwNewLong );
			}
			return SetWindowLongPtr64( hWnd, nIndex, dwNewLong );
		}

		/// <summary>
		/// Called by Unity once application wants to quit.
		/// Returning false will abort and keep application alive. True will allow it to quit.
		/// </summary>
		protected bool ApplicationWantsToQuit()
		{
			// Only allow to quit once application is initialized.
			if( !started )
				return false;

			// Delay quitting so we can clean up.
			if( !quitStarted )
			{
				StartCoroutine( DelayedQuit() );
				return false;
			}

			return true;
		}

		/// <summary>
		/// Restores old WindowProc callback, then quits.
		/// </summary>
		private IEnumerator DelayedQuit()
		{
			// Re-set old WindowProc callback. Normally, this would be done in the new callback itself
			// once WM_CLOSE is detected. This seems to work fine on 64 bit, but when I build 32 bit
			// executables, this causes the application to crash on quitting.
			// This shouldn't really happen and I'm not sure why it does.
			// However, this solution right here seems to work fine on both 32 and 64 bit.
			SetWindowLong( unityHWnd, GWLP_WNDPROC, oldWndProcPtr );

			// Wait for end of frame (our callback is now un-registered), then allow application to quit.
			yield return new WaitForEndOfFrame();

			quitStarted = true;
			Application.Quit();
		}
#endif
		//------------------------------------------------------------------------
		public void UpdateWindowTitle( string title )
		{
#if !DISABLEWIN32API
			SetWindowText( unityHWnd, title );
#endif
		}

		//------------------------------------------------------------------------
		public static void SetCurrentResolution( int width, int height )
		{
			if( mCurrentWidth != width )
				mCurrentWidth = width;

			if( mCurrentHeight != height )
				mCurrentHeight = height;
		}
		
		//------------------------------------------------------------------------
		public static float LockedAspect()
		{
			return mLockedAspect;
		}

#if UNITY_WSA
		private void OnWSAWindowChanged( int width, int height )
		{
			Debug.Log( $"OnWSAWindowChanged : {width}x{height} full:{Screen.fullScreen}" );
			resolutionChangedEventwnd?.Invoke( width, height, Screen.fullScreen );
		}
#endif

	}
}
