//////////////////////////////////////////////////////////////////////////
//
// SingletonBehaviourInspector
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
using UnityEditor;

namespace UMF.Unity.EditorUtil
{
    [CustomEditor( typeof( SingletonBehaviourBase ), true )]
    public class SingletonBehaviourInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            Color g_color = GUI.color;

            GUI.color = Color.red;
            GUILayout.Button( "This is Singleton Behaviour", EditorStyles.miniButtonMid );
            GUI.color = g_color;            

            base.OnInspectorGUI();
        }
    }
}