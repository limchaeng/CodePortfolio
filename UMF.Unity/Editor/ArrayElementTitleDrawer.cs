//////////////////////////////////////////////////////////////////////////
//
// ArrayElementTitleDrawer
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
	[CustomPropertyDrawer( typeof( ArrayElementTitleAttribute ) )]
	public class ArrayElementTitleDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
		{
			return EditorGUI.GetPropertyHeight( property, label, true );
		}

		protected virtual ArrayElementTitleAttribute Attribute => attribute as ArrayElementTitleAttribute;

		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			ArrayElementTitleAttribute attr = Attribute;
			if( attr.title_property_names != null )
			{
				if( attr.Concat )
				{
					string concat_label = "";
					foreach( string tp_name in attr.title_property_names )
					{
						string FullPathName = property.propertyPath + "." + tp_name;
						string label_text = tp_name;
                        SerializedProperty t_prop = property.serializedObject.FindProperty( FullPathName );
						if( t_prop != null )
						{
							object boxed_value = t_prop.boxedValue;
							if( boxed_value != null )
							{
								label_text = boxed_value.ToString();
							}
						}

                        concat_label += label_text;
                    }

                    if( string.IsNullOrEmpty( concat_label ) == false )
					{
                        EditorGUI.PropertyField( position, property, new GUIContent( concat_label, label.tooltip ), true );
                        return;
                    }
                }
                else
				{
					foreach( string tp_name in attr.title_property_names )
					{
						string FullPathName = property.propertyPath + "." + tp_name;
                        string newLabel = tp_name;

                        SerializedProperty t_prop = property.serializedObject.FindProperty( FullPathName );
						if( t_prop != null )
						{
							object boxed_value = t_prop.boxedValue;
							if( boxed_value != null )
								newLabel = boxed_value.ToString();
						}

						if( string.IsNullOrEmpty( newLabel ) == false )
						{
							EditorGUI.PropertyField( position, property, new GUIContent( newLabel, label.tooltip ), true );
							return;
						}
					}
				}
			}

			base.OnGUI( position, property, label );
		}
	}
}