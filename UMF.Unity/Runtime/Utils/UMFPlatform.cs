//////////////////////////////////////////////////////////////////////////
//
// UMFPlatform
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

namespace UMF.Unity
{
    public static class UMFPlatform
    {
        public enum ePlatformGroup
        {
            Unknown,
            Windows,
            OSX,
            Linux,
            PS4,
            Switch,
            XboxOne,
            WebGL,
            iOS,
            Android,
            WindowsUniversal
        }

        internal static readonly Dictionary<RuntimePlatform, ePlatformGroup> s_RuntimeTargetMapping =
            new Dictionary<RuntimePlatform, ePlatformGroup>()
            {
                {RuntimePlatform.XboxOne, ePlatformGroup.XboxOne},
                {RuntimePlatform.Switch, ePlatformGroup.Switch},
                {RuntimePlatform.PS4, ePlatformGroup.PS4},
                {RuntimePlatform.IPhonePlayer, ePlatformGroup.iOS},
                {RuntimePlatform.Android, ePlatformGroup.Android},
                {RuntimePlatform.WebGLPlayer, ePlatformGroup.WebGL},
                {RuntimePlatform.WindowsPlayer, ePlatformGroup.Windows},
                {RuntimePlatform.OSXPlayer, ePlatformGroup.OSX},
                {RuntimePlatform.LinuxPlayer, ePlatformGroup.Linux},
                {RuntimePlatform.WindowsEditor, ePlatformGroup.Windows},
                {RuntimePlatform.OSXEditor, ePlatformGroup.OSX},
                {RuntimePlatform.LinuxEditor, ePlatformGroup.Linux},
                {RuntimePlatform.WSAPlayerARM, ePlatformGroup.WindowsUniversal},
                {RuntimePlatform.WSAPlayerX64, ePlatformGroup.WindowsUniversal},
                {RuntimePlatform.WSAPlayerX86, ePlatformGroup.WindowsUniversal},
            };


        public static ePlatformGroup GetPlatformGroup( RuntimePlatform platform )
        {
            if( s_RuntimeTargetMapping.ContainsKey( platform ) )
                return s_RuntimeTargetMapping[platform];
            
            return ePlatformGroup.Unknown;
        }
    }
}