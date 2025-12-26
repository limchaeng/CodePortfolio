//////////////////////////////////////////////////////////////////////////
//
// SpriteGrid
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
using UnityEngine.Events;

namespace UMF.Unity
{
	[AddComponentMenu("UMF/Sprite/Sprite Grid")]
	public class SpriteGrid : MonoBehaviour
	{
		public enum eArrangement
		{
			Horizontal,
			Vertical,
			CellSnap,
		}

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

		public eArrangement arrangement = eArrangement.Horizontal;
		public bool directionReverseByMaxPerLine = false;
		public eSorting sorting = eSorting.None;
		public ePivot pivot = ePivot.Center;
		public int maxPerLine = 0;
		public float cellWidth = 200f;
		public float cellHeight = 200f;
		public bool hideInactive = false;
		public UnityEvent m_OnRepositionEvent = new UnityEvent();
		public System.Comparison<Transform> onCustomSort;

		[HideInInspector] [SerializeField] Rect mRect = Rect.zero;
		public Rect GetRect { get { return mRect; } }
		[HideInInspector] [SerializeField] int mChildCount = 0;

		protected bool mReposition = false;
		protected bool mInitDone = false;

		public bool repositionNow { set { if( value ) { mReposition = true; enabled = true; } } }

		public List<Transform> GetChildList()
		{
			Transform myTrans = transform;
			List<Transform> list = new List<Transform>();

			for( int i = 0; i < myTrans.childCount; ++i )
			{
				Transform t = myTrans.GetChild( i );
				if( !hideInactive || ( t && t.gameObject.activeInHierarchy ) )
					list.Add( t );
			}

			if( sorting != eSorting.None && arrangement != eArrangement.CellSnap )
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

		protected virtual void Init()
		{
			mInitDone = true;
		}

		protected virtual void Start()
		{
			if( !mInitDone ) Init();
			Reposition();
			enabled = false;
		}

		protected virtual void Update()
		{
			Reposition();
			enabled = false;
		}

		void OnValidate() { if( !Application.isPlaying && gameObject.activeInHierarchy ) Reposition(); }

		static public int SortByName( Transform a, Transform b ) { return string.Compare( a.name, b.name ); }

		protected virtual void Sort( List<Transform> list ) { }

		[ContextMenu( "Execute" )]
		public virtual void Reposition()
		{
			if( Application.isPlaying && !mInitDone && gameObject.activeInHierarchy ) 
				Init();

			List<Transform> list = GetChildList();
			ResetPosition( list );

			m_OnRepositionEvent.Invoke();
		}

		protected virtual void ResetPosition( List<Transform> list )
		{
			mReposition = false;
			mRect = Rect.zero;

			int x = 0;
			int y = 0;
			int maxX = 0;
			int maxY = 0;
			Transform myTrans = transform;

			for( int i = 0, imax = list.Count; i < imax; ++i )
			{
				Transform t = list[i];
				Vector3 pos = t.localPosition;
				float depth = pos.z;

				if( arrangement == eArrangement.CellSnap )
				{
					if( cellWidth > 0 ) pos.x = Mathf.Round( pos.x / cellWidth ) * cellWidth;
					if( cellHeight > 0 ) pos.y = Mathf.Round( pos.y / cellHeight ) * cellHeight;
				}
				else pos = ( arrangement == eArrangement.Horizontal ) ?
					new Vector3( cellWidth * x, -cellHeight * y, depth ) :
					new Vector3( cellWidth * y, -cellHeight * x, depth );

				t.localPosition = pos;

				mRect.x = Mathf.Min( mRect.x, pos.x - cellWidth * 0.5f );
				mRect.xMax = Mathf.Max( mRect.xMax, pos.x + cellWidth * 0.5f );
				mRect.y = Mathf.Min( mRect.y, pos.y - cellHeight * 0.5f );
				mRect.yMax = Mathf.Max( mRect.yMax, pos.y + cellHeight * 0.5f );

				maxX = Mathf.Max( maxX, x );
				maxY = Mathf.Max( maxY, y );

				if( ++x >= maxPerLine && maxPerLine > 0 )
				{
					x = 0;

					if( directionReverseByMaxPerLine == true )
						--y;
					else
						++y;

				}
			}

			if( pivot != ePivot.TopLeft )
			{
				Vector2 po = GetPivotOffset( pivot );

				float fx, fy;

				if( arrangement == eArrangement.Horizontal )
				{
					fx = Mathf.Lerp( 0f, maxX * cellWidth, po.x );
					fy = Mathf.Lerp( -maxY * cellHeight, 0f, po.y );
				}
				else
				{
					fx = Mathf.Lerp( 0f, maxY * cellWidth, po.x );
					fy = Mathf.Lerp( -maxX * cellHeight, 0f, po.y );
				}

				for( int i = 0; i < myTrans.childCount; ++i )
				{
					Transform t = myTrans.GetChild( i );
					Vector3 pos = t.localPosition;
					pos.x -= fx;
					pos.y -= fy;
					t.localPosition = pos;

					mRect.x = Mathf.Min( mRect.x, pos.x - cellWidth * 0.5f );
					mRect.xMax = Mathf.Max( mRect.xMax, pos.x + cellWidth * 0.5f );
					mRect.y = Mathf.Min( mRect.y, pos.y - cellHeight * 0.5f );
					mRect.yMax = Mathf.Max( mRect.yMax, pos.y + cellHeight * 0.5f );
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

		public Vector3 GetNextPosition( bool isLocal )
		{
			int x = 0;
			int y = 0;

			int size = GetChildList().Count + 1;
			Vector3 retPos = Vector3.zero;
			retPos.z = transform.localPosition.z;

			for( int i = 0, imax = size; i < imax; ++i )
			{
				if( arrangement == eArrangement.Horizontal )
				{
					retPos.x = cellWidth * x;
					retPos.y = -cellHeight * y;
				}
				else
				{
					retPos.x = cellWidth * y;
					retPos.y = -cellHeight * x;
				}

				if( ++x >= maxPerLine && maxPerLine > 0 )
				{
					x = 0;
					++y;

					if( directionReverseByMaxPerLine == true )
						y *= -1;
				}
			}

			if( isLocal )
				return retPos;
			else
				return transform.TransformPoint( retPos );
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

			// ->
			Vector3 from = new Vector3( vpos.x + mRect.x, vpos.y + mRect.y, vpos.z );
			Vector3 to = new Vector3( vpos.x + mRect.xMax, from.y, vpos.z );
			Gizmos.DrawLine( from, to );

			Vector3 begin = from;

			// |
			from = to;
			to = new Vector3( from.x, vpos.y + mRect.yMax, vpos.z );
			Gizmos.DrawLine( from, to );

			// <-
			from = to;
			to = new Vector3( vpos.x + mRect.x, from.y, vpos.z );
			Gizmos.DrawLine( from, to );

			// |
			from = to;
			to = new Vector3( from.x, vpos.y + mRect.y, vpos.z );
			Gizmos.DrawLine( from, to );

			if( mChildCount > 0 )
			{
				for( int i = 1; i <= mChildCount - 1; i++ )
				{
					if( maxPerLine > 0 && i > maxPerLine )
						break;

					from = begin;

					if( arrangement == eArrangement.Horizontal )
						from.x += i * cellWidth;
					else if( arrangement == eArrangement.Vertical )
						from.y -= i * cellHeight;

					to = from;

					if( arrangement == eArrangement.Horizontal )
						to.y = vpos.y + mRect.yMax;
					else if( arrangement == eArrangement.Vertical )
						to.x = vpos.x + mRect.xMax;

					Gizmos.DrawLine( from, to );
				}
			}

			if( maxPerLine > 0 )
			{
				int line_count = mChildCount / maxPerLine;

				for(int i=1; i<=line_count; i++ )
				{
					from = begin;
					from.y -= i * cellHeight;
					to = from;
					to.x = vpos.x + mRect.xMax;

					Gizmos.DrawLine( from, to );
				}
			}
		}
#endif
	}
}
