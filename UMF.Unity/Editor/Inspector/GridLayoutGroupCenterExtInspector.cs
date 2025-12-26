//////////////////////////////////////////////////////////////////////////
//
// GridLayoutGroupCenterExtInspector
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
using UMF.Unity.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace UMF.Unity.EditorUtil
{
    [CustomEditor( typeof( GridLayoutGroupCenterExt ), true )]
    public class GridLayoutGroupCenterExtInspector : GridLayoutGroupEditor
    {
        SerializedProperty m_ActiveCenterAlign;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ActiveCenterAlign = serializedObject.FindProperty( "m_ActiveCenterAlign" );
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUILayout.PropertyField( m_ActiveCenterAlign, true );
            serializedObject.ApplyModifiedProperties();

        }
    }
}