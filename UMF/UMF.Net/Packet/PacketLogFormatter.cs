//////////////////////////////////////////////////////////////////////////
//
// PacketLogFormatter
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

using System.IO;
using System.Reflection;
using System.Collections;
using System;
using UMF.Core;
using System.Collections.Generic;

namespace UMF.Net
{
	//------------------------------------------------------------------------	
	public class PacketLogFormatter : ValueLogFormatter
	{
		public override SerializeAttribute GetAttribute( FieldInfo info ) { return info.GetCustomAttribute<PacketValueAttribute>(); }

		//------------------------------------------------------------------------	
		PacketLogFormatter() : base()
		{
		}

		//------------------------------------------------------------------------	
		protected override void MemberSerializer( StringWriter stream, object obj, ValueInfo valueInfo )
		{
			stream.Write( "\"" + valueInfo.name + "\":" );

			if( obj == null )
			{
				PacketValueAttribute attr = (PacketValueAttribute)valueInfo.value_attr;
				if( attr != null && attr.IsNullable == true )
					stream.Write( "\"null\"" );
				else
					stream.Write( "\"null(error)\"" );
			}
			else
			{
				stream.Write( "{" );
				bool bFirstMember = true;
				foreach( ValueInfo info in valueInfo.builder )
				{
					if( bFirstMember == true )
						bFirstMember = false;
					else
						stream.Write( "," );

					object member = info.fieldInfo.GetValue( obj );
					( (Serializer)info.serializer )( stream, member, info );
				}
				stream.Write( "}" );
			}
		}
		
		//------------------------------------------------------------------------	
		public new static PacketLogFormatter Instance = new PacketLogFormatter();
	}
}
