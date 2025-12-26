//////////////////////////////////////////////////////////////////////////
//
// UMFEditorMenu
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

using System.Collections.Generic;
using System.IO;
using UMF.Core;
using UMF.Core.I18N;
using UMF.Unity.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UMF.Unity.EditorUtil
{
    //------------------------------------------------------------------------	
    public static class UMFEditorMenu
	{
		//------------------------------------------------------------------------		
		[MenuItem( "UMF/Editor/Hide DisplayProgress" )]
		static void Hide_DisplayProgress()
		{
			EditorUtility.ClearProgressBar();
		}

		//------------------------------------------------------------------------		
		[MenuItem( "UMF/Editor/Open Editor Log" )]
		static void Env_OpenEditorLog()
		{
			Debug.Log( "LOG FILE DOC : https://docs.unity3d.com/Manual/LogFiles.html" );
#if UNITY_EDITOR_WIN
			string local_app_data_path = System.Environment.GetFolderPath( System.Environment.SpecialFolder.LocalApplicationData );
			System.Diagnostics.Process.Start( Path.Combine( local_app_data_path, "Unity", "Editor", "Editor.log" ) );
#endif
		}

		[MenuItem( "UMF/Environment/Create default folder structure" )]
		public static void Env_CreateFolderStructure()
		{
			Env_CreateFolderStructure( "Assets/_DATA", new List<string>() { "NE" }, null );
		}
		public static void Env_CreateFolderStructure( string base_data_path, List<string> global_types, List<string> service_types )
		{
			foreach( string g_type in global_types )
			{
				string data_tbl = $"{base_data_path}/{g_type}/{TBLInfoBase.DEFAULT_PATH_ROOT}";
				string data_l10n = $"{data_tbl}/{I18NTextConst.DEFAULT_PATH_ROOT}";
				string data_net_config = $"{base_data_path}/{g_type}/_env_net_config"; // TODO : const

				string res_tbl = $"Assets/Resources/{g_type}/{TBLInfoBase.DEFAULT_PATH_ROOT}";
				string res_l10n = $"{res_tbl}/{I18NTextConst.DEFAULT_PATH_ROOT}";
				string res_net_config = $"Assets/Resources/{g_type}/_env_net_config"; // TODO : const

				List<string> make_single_folders = new List<string>();
				make_single_folders.Add( data_tbl );
				make_single_folders.Add( data_l10n );
				make_single_folders.Add( res_tbl );
				make_single_folders.Add( res_l10n );

				foreach( string folder in make_single_folders )
					UMFEditorUtil.CreateFolder( folder );

				List<string> make_service_folders = new List<string>();
				make_service_folders.Add( data_net_config );
				make_service_folders.Add( res_net_config );

				foreach( string folder in make_single_folders )
					UMFEditorUtil.CreateFolder( folder );

				if( service_types != null )
				{
					foreach( string stype in service_types )
					{
						foreach( string folder in make_service_folders )
							UMFEditorUtil.CreateFolder( $"{folder}/{stype}" );
					}
				}
			}

			AssetDatabase.Refresh();
		}

		//------------------------------------------------------------------------
		[MenuItem( "GameObject/UMF/CHECK SpriteRenderer URP Material to Built-In Material" )]
		static void ConvertSpriteRendererMaterialURP2BuiltIn_CHECK()
		{
			_ConvertSpriteRendererMaterialURP2BuiltIn( true );
		}
		[MenuItem( "GameObject/UMF/Convert SpriteRenderer URP Material to Built-In Material" )]
		static void ConvertSpriteRendererMaterialURP2BuiltIn_CONVERT()
		{
			_ConvertSpriteRendererMaterialURP2BuiltIn( false );
		}
		static void _ConvertSpriteRendererMaterialURP2BuiltIn( bool check_only )
		{
			GameObject go = Selection.activeGameObject;
			if( go == null )
				return;

			Material urp_sprite_default_mat = AssetDatabase.LoadAssetAtPath<Material>( "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat" );
			Material urp_mask_mat = AssetDatabase.LoadAssetAtPath<Material>( "Packages/com.unity.render-pipelines.universal/Runtime/Materials/SpriteMask-Default.mat" );

			Material sprite_default_mat = AssetDatabase.GetBuiltinExtraResource<Material>( "Sprites-Default.mat" );
			Material sprite_mask_mat = AssetDatabase.GetBuiltinExtraResource<Material>( "Sprites-Mask.mat" );

			if( check_only || EditorUtility.DisplayDialog( "Fix", "All SpriteRenderer material URP to Built-in", "ok", "cancel" ) )
			{
				Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
				foreach( Renderer renderer in renderers )
				{
					int materialCount = renderer.sharedMaterials.Length;
					Material[] newMaterials = null;
					if( check_only == false )
						newMaterials = new Material[materialCount];

					bool updateMaterials = false;

					for( int matIndex = 0; matIndex < materialCount; matIndex++ )
					{
						if( renderer.sharedMaterials[matIndex] == urp_sprite_default_mat )
						{
							if( check_only == false )
								newMaterials[matIndex] = sprite_default_mat;
							updateMaterials = true;

							Debug.Log( $"{renderer.gameObject.name} has URP Sprite materials" );
						}
						else if( renderer.sharedMaterials[matIndex] == urp_mask_mat )
						{
							if( check_only == false )
								newMaterials[matIndex] = sprite_mask_mat;

							updateMaterials = true;
							Debug.Log( $"{renderer.gameObject.name} has URP Sprite Mask materials" );
						}
						else
						{
							if( check_only == false )
								newMaterials[matIndex] = renderer.sharedMaterials[matIndex];
						}
					}

					if( check_only )
					{

					}
					else
					{
						if( updateMaterials )
							renderer.sharedMaterials = newMaterials;
					}
				}
			}
		}

		//------------------------------------------------------------------------		
		[MenuItem( "GameObject/UMF/Find/Missing components" )]
		static void Find_MissingComponents()
		{
			GameObject go = Selection.activeGameObject;
			if( go == null )
				return;

			List<Object> list = null;
			Component[] comps = go.GetComponentsInChildren<Component>();
			foreach( Component comp in comps )
			{
				if( comp == null )
				{
					Debug.LogFormat( "Finded missing component {0}", go.name );
					if( list == null )
						list = new List<Object>();

					list.Add( go );
				}
			}

			if( list != null )
				UMFSelectionWindow.Show( list );
		}

		//------------------------------------------------------------------------
		[MenuItem( "CONTEXT/PolygonCollider2D/[UMF] Polygon Collider Optimize", false, -10000 )]
		[MenuItem( "CONTEXT/EdgeCollider2D/[UMF] Edge Collider Optimize", false, -10000 )]
		static void OpenColliderOptimizeEditor( MenuCommand command )
		{
			Collider2D collider = command.context as Collider2D;
			ColliderOptimizeEditor.Show( collider );
		}

		//------------------------------------------------------------------------
		[MenuItem( "CONTEXT/PolygonCollider2D/[UMF] Convert to Edge2D Collider", false, -10000 )]
		static void Polygon2EdgeCollider( MenuCommand command )
		{
			PolygonCollider2D polygon = command.context as PolygonCollider2D;
			if( polygon == null || polygon.pathCount < 1 )
				return;

			List<Vector2> edge_point_list = new List<Vector2>();
			Vector2[] first_path_points = polygon.GetPath( 0 );
			foreach( Vector2 v2 in first_path_points )
			{
				if( edge_point_list.Exists( a => a.x == v2.x && a.y == v2.y ) )
					continue;

				edge_point_list.Add( v2 );
			}

			EdgeCollider2D edge = polygon.gameObject.GetComponent<EdgeCollider2D>();
			if( edge == null )
				edge = polygon.gameObject.AddComponent<EdgeCollider2D>();

			edge.Reset();
			edge.SetPoints( edge_point_list );

			EditorUtility.SetDirty( polygon.gameObject );
		}

		//------------------------------------------------------------------------
		//static Object tmp_copy_component_properties_source = null;
		//[MenuItem( "CONTEXT/Component/[UMF] Copy Component Properties", false, -10000 )]
		//static void CopyComponentProperties( MenuCommand command )
		//{
		//	tmp_copy_component_properties_source = command.context;
		//}
		//[MenuItem( "CONTEXT/Component/[UMF] Paste Component Properties", true, -10000 )]
		//static bool PasteComponentPropertiesValidate( MenuCommand command )
		//{
		//	return ( tmp_copy_component_properties_source != null );
		//}
		//[MenuItem( "CONTEXT/Component/[UMF] Paste Component Properties", false, -10000 )]
		//static void PasteComponentProperties( MenuCommand command )
		//{
		//	Object dest = command.context;
		//	if( dest == tmp_copy_component_properties_source )
		//		return;

		//	if( tmp_copy_component_properties_source != null && dest != null )
		//	{
		//		EditorUtility.CopySerializedManagedFieldsOnly( tmp_copy_component_properties_source, dest );
		//	}
		//}

		//------------------------------------------------------------------------
		[MenuItem( "CONTEXT/Transform/[UMF] Transform Info" )]
		static void TransformInfo( MenuCommand command )
		{
			Transform t = command.context as Transform;
			if( t == null )
				return;

			Debug.Log( $"local:{t.localPosition} world:{t.position} rot:{t.localRotation}/{t.rotation} localscale:{t.localScale} lossyscale:{t.lossyScale}" );
			Debug.Log( $"sibiling index={t.GetSiblingIndex()}" );
			Debug.Log( $"cam : S2W={Camera.main.ScreenToWorldPoint( t.position )}" );
		}

        [MenuItem( "CONTEXT/Transform/[UMF] MinMax Z Pos" )]
        static void TransformZPosition( MenuCommand command )
        {
            Transform t = command.context as Transform;
            if( t == null )
                return;

			Vector2 min_max_z = Vector2.zero;
			List<Transform> childs = t.gameObject.GetComponentsInChildrenIncludeThis<Transform>();
			foreach( Transform t2 in childs )
			{
				Vector3 t_pos = t.InverseTransformPoint( t.TransformPoint( t2.localPosition ) );
				min_max_z.x = Mathf.Min( min_max_z.x, t_pos.z );
				min_max_z.y = Mathf.Max( min_max_z.y, t_pos.z );
			}

			Debug.Log( $"MinMax Z min={min_max_z.x} max={min_max_z.y}" );
        }

		[MenuItem( "CONTEXT/RectTransform/[UMF] Rect Transform Info")]
        static void RectTransformInfo( MenuCommand command )
        {
            RectTransform t = command.context as RectTransform;
            if( t == null )
                return;

            Debug.Log( $"local:{t.localPosition} world:{t.position} rot:{t.localRotation}/{t.rotation} localscale:{t.localScale} lossyscale:{t.lossyScale}" );
            Debug.Log( $"sibiling index={t.GetSiblingIndex()}" );
            Debug.Log( $"cam : S2W={Camera.main.ScreenToWorldPoint( t.position )}" );
			Debug.Log( $"Rect : {t.rect} sizeDelta:{t.sizeDelta} apos={t.anchoredPosition}" );
        }

		// 원하는대로 작동 안함.
        [MenuItem( "CONTEXT/RectTransform/[UMF] Size fit by childs" )]
        static void RectSizeFit( MenuCommand command )
		{
            RectTransform parent = command.context as RectTransform;
            if( parent == null )
                return;

            RectTransform[] children = parent.GetComponentsInChildren<RectTransform>();

            if( children.Length <= 1 )
                return; // 자식 없음

            // 월드 좌표 기준 최소/최대 구하기
            Vector3 min = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
            Vector3 max = new Vector3( float.MinValue, float.MinValue, float.MinValue );

            foreach( RectTransform child in children )
            {
                if( child == parent ) continue;

                Vector3[] corners = new Vector3[4];
                child.GetWorldCorners( corners );

                foreach( var corner in corners )
                {
                    min = Vector3.Min( min, corner );
                    max = Vector3.Max( max, corner );
                }
            }

            // 부모 기준으로 변환 (앵커와 무관하게 실제 위치 계산)
            Vector3[] parentCorners = new Vector3[4];
            parent.GetWorldCorners( parentCorners );

            // 부모 pivot을 기준으로 로컬 변환
            Vector3 localMin = parent.InverseTransformPoint( min );
            Vector3 localMax = parent.InverseTransformPoint( max );

            Vector2 newSize = new Vector2( localMax.x - localMin.x, localMax.y - localMin.y );
            Vector2 newCenter = ( localMin + localMax ) / 2f;

            // pivot 고려해서 anchoredPosition 보정
            Vector2 pivotOffset = new Vector2(
                ( 0.5f - parent.pivot.x ) * newSize.x,
                ( 0.5f - parent.pivot.y ) * newSize.y
            );

            parent.sizeDelta = newSize;
            parent.anchoredPosition += (Vector2)newCenter + pivotOffset;
        }

        //------------------------------------------------------------------------
        [MenuItem( "CONTEXT/Button/[UMF] To UIButtonExt", true )]
		static bool Button2UIButtonExtValid( MenuCommand command )
		{
			UIButtonExt b = command.context as UIButtonExt;
			return b == null;
				
		}
        [MenuItem( "CONTEXT/Button/[UMF] To UIButtonExt" )]
		static void Button2UIButtonExt( MenuCommand command )
		{
			Button b = command.context as Button;
			if( b == null )
				return;

			GameObject go = b.gameObject;

			SerializedObject btn_serialize = new SerializedObject( b );
			Button.ButtonClickedEvent click_event = b.onClick;

			Button.Transition transition = b.transition;
			GameObject.DestroyImmediate( b );

			UIButtonExt b_ext = go.AddComponent<UIButtonExt>();
			SerializedObject ext_serialize = new SerializedObject( b_ext );
            
			// copy
			SerializedProperty p_iterator = btn_serialize.GetIterator();
			if( p_iterator.NextVisible( true) )
			{
				while( p_iterator.NextVisible(true) )
				{
					SerializedProperty p = ext_serialize.FindProperty( p_iterator.name );
					if( p != null && p.propertyType == p_iterator.propertyType )
					{
						ext_serialize.CopyFromSerializedProperty( p_iterator );
					}
				}
			}

			b_ext.transition = transition;
			ext_serialize.ApplyModifiedProperties();
        }
    }
}
