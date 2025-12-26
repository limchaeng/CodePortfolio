//////////////////////////////////////////////////////////////////////////
//
// ColliderOptimizeEditor
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

using Collider2DOptimization;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UMF.Unity.EditorUtil
{
	public class ColliderOptimizeEditor : ScriptableWizard
	{
		public static ColliderOptimizeEditor instance;

		Collider2D mCollider = null;
		float mTolerance = 0f;
		float _mTolerancePrev = 0f;

		// for edge
		Vector2 mEdgeNormalOpposite = Vector2.down;
		int mRayBudget = 1000;
		Vector2 _mEdgeNormalOppositePrev = Vector2.down;
		int _mRayBudgetPrev = 1000;

		PolygonCollider2D mPolygonForEdgeCache = null;
		bool mPolygonForEdgeAdded = false;

		private List<List<Vector2>> mOriginalPaths = new List<List<Vector2>>();

		//------------------------------------------------------------------------	
		public static void Open()
		{
			Show( null );
		}

		//------------------------------------------------------------------------		
		public static void Show( Collider2D collider )
		{
			if( instance != null )
			{
				instance.Close();
				instance = null;
			}

			ColliderOptimizeEditor window = ScriptableWizard.DisplayWizard<ColliderOptimizeEditor>( "Collider Optimize" );
			window.mCollider = collider;
			window.mTolerance = 0f;
			window.mPolygonForEdgeCache = null;
			window.mPolygonForEdgeAdded = false;
			window.PathReset();
		}

		//------------------------------------------------------------------------		
		void OnEnable()
		{
			instance = this;
		}

		//------------------------------------------------------------------------		
		void OnDisable()
		{
			instance = null;
			mCollider = null;

			if( mPolygonForEdgeAdded && mPolygonForEdgeCache != null )
				DestroyImmediate( mPolygonForEdgeCache );

			mPolygonForEdgeCache = null;
			mPolygonForEdgeAdded = false;
		}

		//------------------------------------------------------------------------
		public void PathReset()
		{
			mOriginalPaths.Clear();
			if( mCollider != null )
			{
				if( mCollider is PolygonCollider2D )
				{
					PolygonCollider2D polygon = mCollider as PolygonCollider2D;
					for( int i = 0; i < polygon.pathCount; i++ )
					{
						List<Vector2> path = new List<Vector2>( polygon.GetPath( i ) );
						mOriginalPaths.Add( path );
					}
				}
				else if( mCollider is EdgeCollider2D )
				{
					if( mPolygonForEdgeCache == null )
						mPolygonForEdgeCache = mCollider.gameObject.GetComponent<PolygonCollider2D>();
					if( mPolygonForEdgeCache == null )
					{
						mPolygonForEdgeCache = mCollider.gameObject.AddComponent<PolygonCollider2D>();
						mPolygonForEdgeAdded = true;
					}
				}
			}

			UpdateOptimize();
		}

		//------------------------------------------------------------------------
		bool tmp_is_dirty = false;
		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			{
				if( GUILayout.Button( "Path Reset" ) )
				{
					PathReset();
				}
			}
			GUILayout.EndHorizontal();

			tmp_is_dirty = false;

			mCollider = EditorGUILayout.ObjectField( mCollider, typeof( Collider2D ), true ) as Collider2D;
			mTolerance = EditorGUILayout.FloatField( "Tolerance", mTolerance );

			GUI.enabled = ( mCollider != null );
			GUILayout.BeginHorizontal();
			{
				if( GUILayout.Button( "<<" ) ) { mTolerance -= 10f;	}
				if( GUILayout.Button( "<" ) ) {	mTolerance -= 1f; }
				if( GUILayout.Button( ">" ) ) {	mTolerance += 1f; }
				if( GUILayout.Button( ">>" ) ) { mTolerance += 10f; }
			}
			GUILayout.EndHorizontal();

			if( mTolerance != _mTolerancePrev )
				tmp_is_dirty = true;

			if( mCollider == null )
			{
				EditorGUILayout.HelpBox( "Collider is null", MessageType.Info );
			}
			else 
			{
				if( mCollider is EdgeCollider2D )
				{
					mEdgeNormalOpposite = EditorGUILayout.Vector2Field( "EdgeNormalOpposite", mEdgeNormalOpposite );
					mRayBudget = EditorGUILayout.IntField( "RayBudget", mRayBudget );

					if( mEdgeNormalOpposite != _mEdgeNormalOppositePrev || mRayBudget != _mRayBudgetPrev )
						tmp_is_dirty = true;
				}

				if( tmp_is_dirty )
					UpdateOptimize();
			}

			GUI.enabled = true;
		}

		//------------------------------------------------------------------------
		void UpdateOptimize()
		{
			_mTolerancePrev = mTolerance;
			_mEdgeNormalOppositePrev = mEdgeNormalOpposite;
			_mRayBudgetPrev = mRayBudget;

			if( mCollider is PolygonCollider2D )
			{
				PolygonCollider2D polygon = mCollider as PolygonCollider2D;

				if( mTolerance <= 0f )
				{
					for( int i = 0; i < mOriginalPaths.Count; i++ )
					{
						List<Vector2> path = mOriginalPaths[i];
						polygon.SetPath( i, path.ToArray() );
					}
				}
				else
				{
					for( int i = 0; i < mOriginalPaths.Count; i++ )
					{
						List<Vector2> path = mOriginalPaths[i];
						path = ShapeOptimizationHelper.DouglasPeuckerReduction( path, mTolerance );
						polygon.SetPath( i, path.ToArray() );
					}
				}
			}
			else if( mCollider is EdgeCollider2D )
			{
				if( mPolygonForEdgeCache == null )
				{
					EditorGUILayout.HelpBox( "Edge Collider need PolygonCollidr", MessageType.Info );
				}
				else
				{
					EdgeCollider2D edge = mCollider as EdgeCollider2D;

					List<Vector2> path = new List<Vector2>();
					Vector2 upperRight = mPolygonForEdgeCache.bounds.max;
					Vector2 upperLeft = mPolygonForEdgeCache.bounds.min;
					upperLeft.y = upperRight.y;
					for( int i = 0; i < mRayBudget; i++ )
					{
						float t = (float)i / (float)mRayBudget;
						//interpolate along the upper edge of the collider bounds
						Vector2 rayOrigin = Vector2.Lerp( upperLeft, upperRight, t );
						RaycastHit2D[] hits = Physics2D.RaycastAll( rayOrigin, mEdgeNormalOpposite, mPolygonForEdgeCache.bounds.size.y );

						for( int j = 0; j < hits.Length; j++ )
						{
							RaycastHit2D hit = hits[j];
							if( hit.collider == mPolygonForEdgeCache )
							{
								Vector2 localHitPoint = mCollider.transform.InverseTransformPoint( hit.point );
								path.Add( localHitPoint );
								break;
							}
						}
					}
					if( mTolerance > 0 ) path = ShapeOptimizationHelper.DouglasPeuckerReduction( path, mTolerance );
					edge.points = path.ToArray();
				}
			}
		}
	}
}