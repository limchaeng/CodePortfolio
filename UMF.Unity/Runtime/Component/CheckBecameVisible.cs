//////////////////////////////////////////////////////////////////////////
//
// CheckBecameVisible
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

public class CheckBecameVisible : MonoBehaviour
{
	[ContextMenu( "Check Visible" )]
	public void CheckVisible()
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes( Camera.main );
		Vector3 point = gameObject.transform.position;
		foreach(Plane plane in planes)
		{
			if( plane.GetDistanceToPoint(point) < 0f )
			{
				Debug.Log( "Invisible" );
				return;
			}
		}

		Debug.Log( "Is Visible" );
	}
}
