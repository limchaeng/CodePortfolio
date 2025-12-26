//////////////////////////////////////////////////////////////////////////
//
// UIButtonExtEditor
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

using UMF.Unity.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UMF.Unity.EditorUtil
{
    [CustomEditor( typeof( UIButtonExt ), true )]
    [CanEditMultipleObjects]

    public class UIButtonExtEditor : ButtonEditor
    {
        SerializedProperty m_SelectedStateEnableProperty;
        SerializedProperty m_StateClearOnClickedProperty;
        SerializedProperty m_OnStateChangeProperty;
        SerializedProperty m_ColorChangeTargetsProperty;
        SerializedProperty m_ButtonColorApplyTargetsProperty;
        SerializedProperty m_UseBlinkColorProperty;
        SerializedProperty m_BlinkColorProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_SelectedStateEnableProperty = serializedObject.FindProperty( "m_SelectedStateEnable" );
            m_StateClearOnClickedProperty = serializedObject.FindProperty( "m_StateClearOnClicked" );
            m_OnStateChangeProperty = serializedObject.FindProperty( "m_StateChangeEvent" );
            m_ColorChangeTargetsProperty = serializedObject.FindProperty( "m_ColorChangeTargets" );
            m_ButtonColorApplyTargetsProperty = serializedObject.FindProperty( "m_ButtonColorApplyTargets" );
            m_UseBlinkColorProperty = serializedObject.FindProperty( "m_UseBlinkColor" );
            m_BlinkColorProperty = serializedObject.FindProperty( "m_BlinkColor" );
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UIButtonExt button = target as UIButtonExt;

            serializedObject.Update();
            EditorGUILayout.PropertyField( m_SelectedStateEnableProperty );
            EditorGUILayout.PropertyField( m_StateClearOnClickedProperty );
            EditorGUILayout.PropertyField( m_OnStateChangeProperty );
            EditorGUILayout.PropertyField( m_ColorChangeTargetsProperty );
            EditorGUILayout.PropertyField( m_ButtonColorApplyTargetsProperty );
            EditorGUILayout.PropertyField( m_UseBlinkColorProperty );
            if( m_UseBlinkColorProperty.boolValue )
                EditorGUILayout.PropertyField( m_BlinkColorProperty );

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer( typeof( UIButtonExt.ColorChangeGraphic ))]
    public class ColorChangeGraphicDrawer : PropertyDrawer
    {
        public override void OnGUI( Rect rect, SerializedProperty prop, GUIContent label )
        {
            Rect drawRect = rect;
            drawRect.height = EditorGUIUtility.singleLineHeight;

            SerializedProperty target_graphic = prop.FindPropertyRelative( "m_Target" );
            SerializedProperty is_disable = prop.FindPropertyRelative( "m_Disable" );
            SerializedProperty normalColor = prop.FindPropertyRelative( "m_NormalColor" );
            SerializedProperty highlighted = prop.FindPropertyRelative( "m_PressedColor" );
            SerializedProperty pressedColor = prop.FindPropertyRelative( "m_HighlightedColor" );
            SerializedProperty selectedColor = prop.FindPropertyRelative( "m_SelectedColor" );
            SerializedProperty disabledColor = prop.FindPropertyRelative( "m_DisabledColor" );

            SerializedProperty _show_color_property = prop.FindPropertyRelative( "mShowColorProperty" );

            EditorGUI.PropertyField( drawRect, target_graphic );
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField( drawRect, is_disable );
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            _show_color_property.boolValue = EditorGUI.BeginFoldoutHeaderGroup( drawRect, _show_color_property.boolValue, "Color Property" );
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if( _show_color_property.boolValue )
            {
                GUI.enabled = is_disable.boolValue == false;
                Rect btn_rect = drawRect;
                if( GUI.Button( btn_rect, "Set Current Color" ) )
                {
                    Graphic g = target_graphic.objectReferenceValue as Graphic;
                    if( g != null )
                    {
                        normalColor.colorValue = g.color;
                        highlighted.colorValue = g.color;
                        pressedColor.colorValue = g.color;
                        selectedColor.colorValue = g.color;
                        disabledColor.colorValue = new Color32( 200, 200, 200, 255 );
                    }
                }
                drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField( drawRect, normalColor );
                drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField( drawRect, highlighted );
                drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField( drawRect, pressedColor );
                drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField( drawRect, selectedColor );
                drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField( drawRect, disabledColor );
                GUI.enabled = true;
            }
            EditorGUI.EndFoldoutHeaderGroup();
        }

        public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
        {
            SerializedProperty _show_color_property = property.FindPropertyRelative( "mShowColorProperty" );

            int count = 9;
            if( _show_color_property.boolValue == false )
                count = 3;

            return count * EditorGUIUtility.singleLineHeight + 10 * EditorGUIUtility.standardVerticalSpacing;
        }
    }
}