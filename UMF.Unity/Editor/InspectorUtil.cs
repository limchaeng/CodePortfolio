//////////////////////////////////////////////////////////////////////////
//
// InspectorUtil
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
using System.IO;
using UnityEngine.Events;
using System.Linq;

namespace UMF.Unity.EditorUtil
{
    public static class InspectorUtil
    {
        //------------------------------------------------------------------------
        public class ToggleBaseInspectorEditor : Editor
        {
            protected bool mHideBaseGUI = false;

            public override void OnInspectorGUI()
            {
                mHideBaseGUI = InspectorUtil.DrawHeaderFoldable( mHideBaseGUI ? "Show base GUI" : "Hide base GUI", mHideBaseGUI );
                if( mHideBaseGUI == false )
                    base.OnInspectorGUI();

                InspectorUtil.DrawLine();
            }
        }

        //------------------------------------------------------------------------
        public static string[] DrawDragAndDropArea( string message, float width, float height, Color area_color, DragAndDropVisualMode v_mode )
        {
            Object[] objects = null;
            return DrawDragAndDropArea( message, width, height, area_color, v_mode, ref objects );
        }
        /// <summary>
        ///   width > 0 : fixed width
        ///   width = 0 : expand width
        ///   return paths
        /// </summary>
        public static string[] DrawDragAndDropArea( string message, float width, float height, Color area_color, DragAndDropVisualMode v_mode, ref Object[] refs )
        {
            Event evt = Event.current;
            Rect drop_area = Rect.zero;
            if( width > 0 )
                drop_area = GUILayoutUtility.GetRect( width, height );
            else
                drop_area = GUILayoutUtility.GetRect( 0f, height, GUILayout.ExpandWidth( true ) );

            Color gui_color = GUI.color;
            GUI.color = area_color;
            GUI.Box( drop_area, message );
            GUI.color = gui_color;

            switch( evt.type )
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if( drop_area.Contains( evt.mousePosition ) == false )
                        return null;

                    DragAndDrop.visualMode = v_mode;

                    if( evt.type == EventType.DragPerform )
                    {
                        DragAndDrop.AcceptDrag();
                        refs = DragAndDrop.objectReferences;
                        return DragAndDrop.paths;
                    }
                    break;
            }

            return null;
        }

        //------------------------------------------------------------------------
        public class DynamicBtnListBoxData
        {
            public GUIContent gui_content;
            public bool is_selected = false;
            public object custom_data = null;
            public float size_width = -1f;
            public float size_height = -1f;
        }
        [System.Flags]
        public enum eDynamicBtnListBoxFlag
        {
            None = 0x0000,
            DropDownBtn = 0x0001,
            AlignLeft = 0x0002,
            Toggle = 0x0004,
        }
        public static DynamicBtnListBoxData DrawDynamicButtonListBox( List<DynamicBtnListBoxData> data_list )
        {
            return DrawDynamicButtonListBox( data_list, eDynamicBtnListBoxFlag.None );
        }
        public static DynamicBtnListBoxData DrawDynamicButtonListBox( List<DynamicBtnListBoxData> data_list, eDynamicBtnListBoxFlag flags, float padding = 4f )
        {
            return DrawDynamicButtonListBox( data_list, flags, GUI.color, padding );
        }
        public static DynamicBtnListBoxData DrawDynamicButtonListBox( List<DynamicBtnListBoxData> data_list, eDynamicBtnListBoxFlag flags, Color selected_color, float padding = 4f )
        {
            DynamicBtnListBoxData clicked_data = null;

            float view_width = EditorGUIUtility.currentViewWidth;
            float curr_width = 0f;
            float drop_down_size_right_size = 10f;

            Color gui_color = GUI.color;

            bool align_left = ( flags & eDynamicBtnListBoxFlag.AlignLeft ) != 0;

            GUILayout.BeginHorizontal();
            if( align_left == false )
                GUILayout.FlexibleSpace();

            foreach( DynamicBtnListBoxData data in data_list )
            {
                GUIContent gui_content = data.gui_content;
                Vector2 size = GUI.skin.button.CalcSize( gui_content );

                if( data.size_width > 0f )
                    size.x = data.size_width;

                if( data.size_height > 0f )
                    size.y = data.size_height;

                if( ( flags & eDynamicBtnListBoxFlag.DropDownBtn ) != 0 )
                    size.x += drop_down_size_right_size;

                if( curr_width + size.x + padding > view_width )
                {
                    if( align_left )
                        GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if( align_left == false )
                        GUILayout.FlexibleSpace();
                    curr_width = 0f;
                }

                curr_width += size.x + padding;

                if( data.is_selected )
                    GUI.color = selected_color;

                float width = size.x;
                float height = size.y;

                bool clicked = false;
                if( ( flags & eDynamicBtnListBoxFlag.DropDownBtn ) != 0 )
                {
                    clicked = EditorGUILayout.DropdownButton( gui_content, FocusType.Passive, GUILayout.Width( width ), GUILayout.Height( height ) );
                }
                else
                {
                    clicked = GUILayout.Button( gui_content, GUILayout.Width( width ), GUILayout.Height( height ) );
                }

                if( clicked )
                {
                    clicked_data = data;

                    if( ( flags & eDynamicBtnListBoxFlag.Toggle ) != 0 )
                    {
                        data.is_selected = !data.is_selected;
                    }
                    else
                    {
                        data_list.ForEach( a => a.is_selected = false );
                        data.is_selected = true;
                    }
                }

                GUI.color = gui_color;
            }

            if( align_left )
                GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            return clicked_data;
        }

        //------------------------------------------------------------------------
        public static void DrawProperty( SerializedProperty property, string prefix, string tooltip = "" )
        {
            EditorGUILayout.PropertyField( property, new GUIContent( prefix + property.displayName, tooltip ) );
        }


        //------------------------------------------------------------------------
        public static void DrawHeader( string text )
        {
            GUILayout.Space( 3f );
            GUILayout.BeginHorizontal();
            GUI.changed = false;

            text = "\u2588 " + "<b>" + text + "</b>";
            GUILayout.Toggle( true, text, "dragtab", GUILayout.MinWidth( 20f ) );

            GUILayout.Space( 2f );
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        public static bool DrawHeaderFoldable( string text, bool bOn, string added_text = "" )
        {
            bool state = bOn;

            GUILayout.Space( 3f );
            if( !state )
                GUI.backgroundColor = new Color( 0.8f, 0.8f, 0.8f );
            GUILayout.BeginHorizontal();
            GUILayout.Space( 3f );

            GUI.changed = false;
            text = "<b>" + text + "</b>" + added_text;
            if( state )
                text = "\u25B2 " + text;
            else
                text = "\u25BC " + text;
            if( !GUILayout.Toggle( true, text, "dragtab", GUILayout.MinWidth( 20f ) ) )
                state = !state;

            GUILayout.Space( 2f );
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if( !state )
                GUILayout.Space( 3f );
            return state;
        }

        //------------------------------------------------------------------------
        public static void BeginContents()
        {
            BeginContents( null );
        }

        public static void BeginContents( string title )
        {
            float view_width = EditorGUIUtility.currentViewWidth;

            if( string.IsNullOrEmpty( title ) == false )
            {
                GUILayout.Space( 3f );
                GUILayout.BeginHorizontal();
                GUILayout.Space( 3f );

                string text = "\u25C8 " + title;
                GUILayout.Toggle( true, text, "dragtab", GUILayout.MinWidth( 20f ) );
                GUILayout.Space( 2f );
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space( 4f );
            EditorGUILayout.BeginHorizontal( "TextArea", GUILayout.MinHeight( 10f ) );
            GUILayout.BeginVertical();
            GUILayout.Space( 2f );
        }

        public static void EndContents()
        {
            GUILayout.Space( 3f );
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space( 3f );
            GUILayout.EndHorizontal();
            GUILayout.Space( 3f );
        }

        //------------------------------------------------------------------------	
        public static void BeginContentsScope( UnityAction action )
        {
            BeginContentsScope( "", action );
        }
        public static void BeginContentsScope( string title, UnityAction action )
        {
            float view_width = EditorGUIUtility.currentViewWidth;

            if( string.IsNullOrEmpty( title ) == false )
            {
                GUILayout.Space( 3f );
                GUILayout.BeginHorizontal();
                GUILayout.Space( 3f );

                string text = "\u25C8 " + title;
                GUILayout.Toggle( true, text, "dragtab", GUILayout.MinWidth( 20f ) );
                GUILayout.Space( 2f );
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space( 4f );
            EditorGUILayout.BeginHorizontal( "TextArea", GUILayout.MinHeight( 10f ) );
            GUILayout.BeginVertical();
            GUILayout.Space( 2f );

            if( action != null )
                action();

            GUILayout.Space( 3f );
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space( 3f );
            GUILayout.EndHorizontal();
            GUILayout.Space( 3f );
        }

        //------------------------------------------------------------------------
        public static void DrawLine()
        {
            DrawLine( Color.gray );
        }

        public static void DrawLine( Color color, int thickness = 2, int padding = 10 )
        {
            Rect r = EditorGUILayout.GetControlRect( GUILayout.Height( padding + thickness ) );
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect( r, color );
        }

        //------------------------------------------------------------------------
        public static void Draw_PrefabControl( GameObject game_object, System.Action<bool> saved_callback )
        {
            Draw_PrefabControl( game_object, Application.dataPath, saved_callback, null );
        }
        public static void Draw_PrefabControl( GameObject game_object, System.Action<bool> saved_callback, System.Func<bool, GameObject> valid_checker )
        {
            Draw_PrefabControl( game_object, Application.dataPath, saved_callback, null );
        }
        public static void Draw_PrefabControl( GameObject game_object, string save_root_path, System.Action<bool> saved_callback, System.Func<bool, GameObject> valid_checker )
        {
            Color g_color = GUI.color;
            DrawHeader( "Prefab" );
            if( Application.isPlaying == false && game_object != null )
            {
                if( PrefabUtility.GetPrefabAssetType( game_object ) == PrefabAssetType.NotAPrefab )
                {
                    GUI.color = Color.yellow;
                    if( GUILayout.Button( "Create Prefab", GUILayout.Height( 20f ) ) )
                    {
                        string last_path = save_root_path;
                        string path = EditorUtility.OpenFolderPanel( "UMF Prefab Save", last_path, "" );
                        if( Directory.Exists( path ) )
                        {
                            path += "/" + game_object.name + ".prefab";
                            PrefabUtility.SaveAsPrefabAssetAndConnect( game_object, path, InteractionMode.AutomatedAction );
                            if( saved_callback != null )
                                saved_callback( false );
                        }
                    }
                    GUI.color = g_color;
                }
                else
                {
                    if( PrefabUtility.HasPrefabInstanceAnyOverrides( game_object, true ) == false )
                        GUI.color = Color.gray;
                    else
                        GUI.color = Color.cyan;

                    GUILayout.BeginHorizontal();
                    if( GUILayout.Button( "Prefab Apply", GUILayout.Height( 20f ) ) )
                    {
                        DoPrefabApply( game_object, saved_callback, valid_checker, false );
                    }
                    if( GUILayout.Button( "Apply & Remove", GUILayout.Height( 20f ) ) )
                    {
                        DoPrefabApply( game_object, saved_callback, valid_checker, true );
                    }
                    if( GUILayout.Button( "Remove", GUILayout.Height( 20f ) ) )
                    {
                        DoPrefabApply( game_object, saved_callback, valid_checker, true, true );
                    }
                    GUILayout.EndHorizontal();
                    GUI.color = g_color;
                }
            }
        }

        //------------------------------------------------------------------------
        static void DoPrefabApply( GameObject game_object, System.Action<bool> saved_callback, System.Func<bool, GameObject> valid_checker, bool is_remove, bool donot_apply = false )
        {
            bool is_valid = true;
            if( valid_checker != null )
                is_valid = valid_checker( game_object );

            if( is_valid )
            {
                string confirm = string.Format( "Prefab ({0}) Apply?", game_object.name );
                if( is_remove && donot_apply )
                    confirm = string.Format( "Prefab ({0}) Remove?", game_object.name );

                if( EditorUtility.DisplayDialog( "Q", confirm, "ok", "cancel" ) )
                {
                    if( is_remove == false || donot_apply == false )
                    {
                        PrefabUtility.ApplyPrefabInstance( game_object, InteractionMode.AutomatedAction );
                    }

                    if( saved_callback != null )
                        saved_callback( is_remove );

                    if( is_remove )
                        GameObject.DestroyImmediate( game_object );
                }
            }
            else
            {
                EditorUtility.DisplayDialog( "W", "valid check failed!", "OK" );
                return;
            }
        }

        //------------------------------------------------------------------------
        public static string[] GetResourcePrefabNameList( string path )
        {
            List<string> name_list = new List<string>();

            string res_path = string.Format( "Assets/Resources/{0}", path );
            if( Directory.Exists( res_path ) )
            {
                string[] files = Directory.GetFiles( res_path, "*.prefab" );
                if( files != null )
                {
                    foreach( string file_path in files )
                    {
                        name_list.Add( Path.GetFileNameWithoutExtension( file_path ) );
                    }
                }
            }

            return name_list.OrderBy( a => a ).ToArray();
        }

        //------------------------------------------------------------------------
        public static void DoInstantiatePrefab( string _path, string _name, GameObject root_obj )
        {
            if( root_obj == null )
                return;

            int child_count = root_obj.transform.childCount;
            if( child_count > 0 )
            {
                for( int i = 0; i < child_count; i++ )
                {
                    Transform child = root_obj.transform.GetChild( i );
                    if( child.gameObject.name == _name )
                    {
                        Debug.LogWarning( $"DoInstantiatePrefab: already instantiate prefab {_name}" );
                        EditorGUIUtility.PingObject( child.gameObject );
                        return;
                    }
                }
            }

            string full_path = string.Format( "{0}/{1}", _path, _name );
            GameObject prefab_obj = Resources.Load<GameObject>( full_path );
            if( prefab_obj == null )
            {
                EditorUtility.DisplayDialog( "DoInstantiatePrefab", $"Can not load prefab {full_path}", "OK" );
                return;
            }

            Object new_obj = PrefabUtility.InstantiatePrefab( prefab_obj, root_obj.transform );
            EditorGUIUtility.PingObject( new_obj );
        }

        //------------------------------------------------------------------------
        public static Vector3 DrawVector3ToggleButton( Vector3 v3, string label, Color on_color )
        {
            Vector3 value = Vector3.zero;
            GUILayout.BeginHorizontal();
            GUILayout.Label( label );

            Color _color = GUI.color;

            for( int i = 0; i < 3; i++ )
            {
                bool is_on = false;
                string btn_text = "";
                if( i == 0 ) { is_on = v3.x == 1f; btn_text = "X"; }
                else if( i == 1 ) { is_on = v3.y == 1f; btn_text = "Y"; }
                else if( i == 2 ) { is_on = v3.z == 1f; btn_text = "Z"; }

                if( is_on )
                {
                    GUI.color = on_color;
                    btn_text += " (ON)";
                }
                else
                {
                    btn_text += " (OFF)";
                }

                if( GUILayout.Button( btn_text ) )
                {
                    is_on = !is_on;
                }

                if( i == 0 ) { value.x = is_on ? 1f : 0f; }
                else if( i == 1 ) { value.y = is_on ? 1f : 0f; }
                else if( i == 2 ) { value.z = is_on ? 1f : 0f; }

                GUI.color = _color;
            }
            GUILayout.EndHorizontal();

            return value;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    ///
    [CustomPropertyDrawer( typeof( UMFReadOnlyAttribute ), true )]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
        {
            return EditorGUI.GetPropertyHeight( property, label, true );
        }

        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
        {
            GUI.enabled = Application.isPlaying == false && ( (UMFReadOnlyAttribute)attribute ).runtimeOnly;
            EditorGUI.PropertyField( position, property, label, true );
            GUI.enabled = true;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    ///
    public static class MaterialInspectorUtil
    {
        //------------------------------------------------------------------------	
        public static bool BoolFloatProperty( MaterialProperty property, string name )
        {
            bool toggle = property.floatValue == 0 ? false : true;
            toggle = EditorGUILayout.Toggle( name, toggle );
            property.floatValue = toggle ? 1 : 0;

            return toggle;
        }

        //------------------------------------------------------------------------	
        public static Vector4 Vector4Property( string label, MaterialProperty property, int v_count )
        {
            Vector4 v4 = property.vectorValue;

            switch( v_count )
            {
                case 1:
                    v4.x = EditorGUILayout.FloatField( label, v4.x );
                    break;

                case 2:
                    {
                        Vector2 v2 = new Vector2( v4.x, v4.y );
                        v2 = EditorGUILayout.Vector2Field( label, v2 );
                        v4.x = v2.x;
                        v4.y = v2.y;
                    }
                    break;

                case 3:
                    {
                        Vector3 v3 = new Vector3( v4.x, v4.y, v4.z );
                        v3 = EditorGUILayout.Vector3Field( label, v3 );
                        v4.x = v3.x;
                        v4.y = v3.y;
                        v4.z = v3.z;
                    }
                    break;

                default:
                    v4 = EditorGUILayout.Vector4Field( label, v4 );
                    break;
            }

            property.vectorValue = v4;

            return v4;
        }
    }
}
