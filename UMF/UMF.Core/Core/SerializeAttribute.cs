//////////////////////////////////////////////////////////////////////////
//
// SerializeAttribute
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
using System.Reflection;

namespace UMF.Core
{
	//------------------------------------------------------------------------	
	abstract public class SerializeAttribute : System.Attribute
	{
		abstract public bool IsSerializable { get; }
	}

	//------------------------------------------------------------------------	
	[System.AttributeUsage( System.AttributeTargets.Field )]
	public class ListAttribute : System.Attribute
	{
		public string KeyName { get; private set; }

		public ListAttribute( string keyName )
		{
			KeyName = keyName;
		}
	}
}
