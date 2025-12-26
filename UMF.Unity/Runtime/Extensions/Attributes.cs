//////////////////////////////////////////////////////////////////////////
//
// Attributes
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UMF.Unity
{
	//------------------------------------------------------------------------	
	// readonly field
	[AttributeUsage(AttributeTargets.Field)]
	public class UMFReadOnlyAttribute : PropertyAttribute
	{
		public readonly bool runtimeOnly;
		public UMFReadOnlyAttribute(bool runtimeOnly = false)
		{
			this.runtimeOnly = runtimeOnly;
		}
	}

	//------------------------------------------------------------------------	
	// array element title name attributes
	// 매우 느려질수 있음
	[AttributeUsage( AttributeTargets.Field )]
	public class ArrayElementTitleAttribute : PropertyAttribute
	{
		public string[] title_property_names;
		public bool Concat = false;
		public ArrayElementTitleAttribute( bool _concat, params string[] _title_property_names )
		{
			title_property_names = _title_property_names;
			Concat = _concat;
		}
	}

}
