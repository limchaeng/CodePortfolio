//////////////////////////////////////////////////////////////////////////
//
// SimpleConfigAttribute
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

namespace UMF.Core.SimpleConfig
{
	//------------------------------------------------------------------------
	[AttributeUsage( AttributeTargets.Class )]
	public class SimpleConfigAttribute : Attribute
	{
		public string ResourceName { get; set; }
		public string FieldName { get; set; }
		public bool IgnoreFieldName { get; set; }

		public SimpleConfigAttribute( string resource_name = "", string field_name = "", bool ignore_field_name = false )
		{
			ResourceName = resource_name;
			FieldName = field_name;
			IgnoreFieldName = ignore_field_name;
		}
	}

	//------------------------------------------------------------------------
	[AttributeUsage( AttributeTargets.Field )]
	public class SimpleConfigFieldAttribute : Attribute
	{
		public string FieldName { get; set; }
		public bool UseCustomParse { get; set; }

		public SimpleConfigFieldAttribute( string field_name = "", bool use_custom_parse = false )
		{
			FieldName = field_name;
			UseCustomParse = use_custom_parse;
		}
	}

	//------------------------------------------------------------------------
	[AttributeUsage( AttributeTargets.Field )]
	public class SimpleConfigSepListFieldAttribute : SimpleConfigFieldAttribute
	{
		public char ListSeparator { get; set; }
		public SimpleConfigSepListFieldAttribute( string field_name = "", char list_separator = ',' )
			: base( field_name )
		{
			ListSeparator = list_separator;
		}
	}
}
