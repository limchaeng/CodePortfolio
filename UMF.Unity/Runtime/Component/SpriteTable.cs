//////////////////////////////////////////////////////////////////////////
//
// SpriteTable
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
	[AddComponentMenu( "UMF/Sprite/Sprite Table" )]
	public class SpriteTable : MonoBehaviour
	{
		public delegate void OnReposition();

		public enum eSorting
		{
			None,
			Alphabetic,
			Custom,
		}

		public enum ePivot
		{
			TopLeft,
			Top,
			TopRight,
			Left,
			Center,
			Right,
			BottomLeft,
			Bottom,
			BottomRight,
		}


		public Vector2 m_Size = new Vector2( 100f, 100f );
		public bool m_AutoSize = true;
		public int columns = 0;
		public eSorting sorting = eSorting.None;
		public ePivot pivot = ePivot.TopLeft;
		public ePivot cellAlignment = ePivot.TopLeft;
		public bool hideInactive = true;
		public Vector2 padding = Vector2.zero;
		public Transform m_CenterObject = null; // align by this object to center
		public bool m_CenterHorizontal = false;
		public bool m_CenterVertical = false;
		public Transform[] m_LockPosYObjects = new Transform[0];
		public OnReposition onReposition;
		public System.Comparison<Transform> onCustomSort;

		protected bool mInitDone = false;
		protected bool mReposition = false;

		public float TableWidth { get; private set; }
		public float TableHeight { get; private set; }

		public bool repositionNow { set { if( value ) { mReposition = true; enabled = true; } } }

		[HideInInspector] [SerializeField] protected int mChildCount = 0;
		[HideInInspector] [SerializeField] protected List<Vector2> mGizmoLineData = new List<Vector2>();

		public List<Transform> GetChildList()
		{
			Transform myTrans = transform;
			List<Transform> list = new List<Transform>();

			for( int i = 0; i < myTrans.childCount; ++i )
			{
				Transform t = myTrans.GetChild( i );
				if( t == null || ( hideInactive && t.gameObject.activeInHierarchy == false ) )
					continue;

				SpriteRenderer sp_renderer = t.gameObject.GetComponent<SpriteRenderer>();
				if( sp_renderer == null || sp_renderer.sprite == null )
					continue;

				list.Add( t );
			}

			if( sorting != eSorting.None )
			{
				if( sorting == eSorting.Alphabetic ) list.Sort( SortByName );
				else if( onCustomSort != null ) list.Sort( onCustomSort );
				else Sort( list );
			}

			mChildCount = list.Count;

			return list;
		}

		public Transform GetChild( int index )
		{
			List<Transform> list = GetChildList();
			return ( index < list.Count ) ? list[index] : null;
		}

		protected virtual void Sort( List<Transform> list ) { list.Sort( SortByName ); }
		static public int SortByName( Transform a, Transform b ) { return string.Compare( a.name, b.name ); }

		protected virtual void Start()
		{
			Init();
			Reposition();
			enabled = false;
		}
		protected virtual void Init()
		{
			mInitDone = true;
		}

		protected virtual void LateUpdate()
		{
			if( mReposition ) Reposition();
			enabled = false;
		}

		void OnValidate() { if( !Application.isPlaying && gameObject.activeInHierarchy ) Reposition(); }

		protected void RepositionVariableSize( List<Transform> children )
		{
			float xOffset = 0;
			float yOffset = 0;

			int cols = columns > 0 ? children.Count / columns + 1 : 1;
			int rows = columns > 0 ? columns : children.Count;

			Bounds[,] bounds = new Bounds[cols, rows];
			Bounds[] boundsRows = new Bounds[rows];
			Bounds[] boundsCols = new Bounds[cols];

			int x = 0;
			int y = 0;
			TableWidth = 0f;
			TableHeight = 0f;

			mGizmoLineData.Clear();

			for( int i = 0, imax = children.Count; i < imax; ++i )
			{
				Transform t = children[i];
				SpriteRenderer sp_renderer = t.gameObject.GetComponent<SpriteRenderer>();
				Bounds b = sp_renderer.sprite.bounds;

				Vector3 scale = t.localScale;
				b.min = Vector3.Scale( b.min, scale );
				b.max = Vector3.Scale( b.max, scale );
				bounds[y, x] = b;

				boundsRows[x].Encapsulate( b );	// for max
				boundsCols[y].Encapsulate( b ); // for max

				if( ++x >= columns && columns > 0 )
				{
					x = 0;
					++y;
				}
			}

			x = 0;
			y = 0;

			Vector2 po = GetPivotOffset( cellAlignment );

			for( int i = 0, imax = children.Count; i < imax; ++i )
			{
				Transform t = children[i];
				Bounds b = bounds[y, x];
				Bounds br = boundsRows[x];
				Bounds bc = boundsCols[y];

				Vector3 pos = t.localPosition;
				pos.x = xOffset + b.extents.x - b.center.x;
				pos.x -= Mathf.Lerp( 0f, b.max.x - b.min.x - br.max.x + br.min.x, po.x ) - padding.x;

				if( columns > 0 || m_LockPosYObjects == null || m_LockPosYObjects.Length <= 0 || System.Array.Exists( m_LockPosYObjects, lock_y => lock_y == t ) == false )
				{
					pos.y = -yOffset - b.extents.y - b.center.y;
					pos.y += Mathf.Lerp( b.max.y - b.min.y - bc.max.y + bc.min.y, 0f, po.y ) - padding.y;
				}

				float size_x = br.size.x + padding.x * 2f;
				float size_y = bc.size.y + padding.y * 2f;

				xOffset += size_x;
				t.localPosition = pos;

				TableWidth = Mathf.Max( TableWidth, xOffset );
				TableHeight = Mathf.Max( TableHeight, yOffset + size_y );

				Vector2 gizmo_v2 = Vector2.zero;
				gizmo_v2.x = size_x;
				gizmo_v2.y = size_y;
				mGizmoLineData.Add( gizmo_v2 );

				if( ++x >= columns && columns > 0 )
				{
					x = 0;
					++y;

					xOffset = 0f;
					yOffset += size_y;
				}
			}

			if( m_AutoSize )
			{
				m_Size.x = TableWidth;
				m_Size.y = TableHeight;
			}

			// Apply the origin offset
			if( pivot != ePivot.TopLeft )
			{
				po = GetPivotOffset( pivot );

				float fx, fy;

				fx = Mathf.Lerp( 0f, TableWidth, po.x );
				fy = Mathf.Lerp( -TableHeight, 0f, po.y );

				for( int i = 0, imax = children.Count; i < imax; ++i )
				{
					Transform t = children[i];

					Vector3 pos = t.localPosition;
					pos.x -= fx;
					if( columns > 0 || m_LockPosYObjects == null || m_LockPosYObjects.Length <= 0 || System.Array.Exists( m_LockPosYObjects, lock_y => lock_y == t ) == false )
						pos.y -= fy;

					t.localPosition = pos;

					if( pivot == ePivot.Center && columns == 0 && m_CenterObject != null )
					{
						fx = 0f;
						fy = 0f;
						if( m_CenterHorizontal )
							fx = m_CenterObject.localPosition.x;

						if( m_CenterVertical )
							fy = m_CenterObject.localPosition.y;

						if( m_CenterHorizontal || m_CenterVertical )
						{
							pos.x -= fx;
							pos.y -= fy;
							t.localPosition = pos;
						}
					}
				}
			}
		}

		Vector2 GetPivotOffset( ePivot pv )
		{
			Vector2 v = Vector2.zero;
			if( pv == ePivot.Top || pv == ePivot.Center || pv == ePivot.Bottom ) v.x = 0.5f;
			else if( pv == ePivot.TopRight || pv == ePivot.Right || pv == ePivot.BottomRight ) v.x = 1f;
			else v.x = 0f;

			if( pv == ePivot.Left || pv == ePivot.Center || pv == ePivot.Right ) v.y = 0.5f;
			else if( pv == ePivot.TopLeft || pv == ePivot.Top || pv == ePivot.TopRight ) v.y = 1f;
			else v.y = 0f;
			return v;
		}

		/// <summary>
		/// Recalculate the position of all elements within the table, sorting them alphabetically if necessary.
		/// </summary>

		[ContextMenu( "Execute" )]
		public virtual void Reposition()
		{
			TableHeight = 0;

			if( Application.isPlaying && !mInitDone && gameObject.activeInHierarchy ) Init();

			mReposition = false;
			Transform myTrans = transform;
			List<Transform> ch = GetChildList();
			if( ch.Count > 0 ) RepositionVariableSize( ch );

			if( onReposition != null )
				onReposition();
		}

#if UNITY_EDITOR
		//------------------------------------------------------------------------			
		private void OnDrawGizmosSelected()
		{
			if( UnityEditor.Selection.activeGameObject != gameObject )
				return;

			Gizmos.color = Color.green;

			Vector3 vpos = Vector3.zero;
			Gizmos.matrix = transform.localToWorldMatrix;

			Vector2 pv = GetPivotOffset( pivot );

			float fx = Mathf.Lerp( 0f, m_Size.x, pv.x );
			float fy = Mathf.Lerp( -m_Size.y, 0f, pv.y );

			// ->
			Vector3 from = new Vector3( vpos.x - fx, vpos.y - fy, vpos.z );
			Vector3 to = new Vector3( from.x + m_Size.x, from.y, vpos.z );
			Gizmos.DrawLine( from, to );

			Vector3 begin = from;

			// |
			from = to;
			to = new Vector3( from.x, from.y - m_Size.y, vpos.z );
			Gizmos.DrawLine( from, to );

			// <-
			from = to;
			to = new Vector3( vpos.x - fx, from.y, vpos.z );
			Gizmos.DrawLine( from, to );

			// |
			from = to;
			to = new Vector3( from.x, from.y + m_Size.y, vpos.z );
			Gizmos.DrawLine( from, to );

			from = begin;
			to = begin;
				
			if( m_AutoSize && mChildCount > 0 && mGizmoLineData.Count == mChildCount )
			{
				int x = 0;
				int y = 0;

				for( int i = 0; i < mChildCount; i++ )
				{
					Vector2 v2 = mGizmoLineData[i];

					from.x += v2.x;
					to = from;
					to.y -= v2.y;

					if( ++x >= columns && columns > 0 )
					{
						x = 0;
						++y;

						from.x = begin.x;
						from.y = to.y;
					}

					Gizmos.DrawLine( from, to );
				}
			}
		}
#endif
	}
}
