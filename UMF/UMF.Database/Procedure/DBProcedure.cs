//////////////////////////////////////////////////////////////////////////
//
// DBProcedure
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

namespace UMF.Database
{
	//------------------------------------------------------------------------	
	public class PROCEDURE<T>
	{
		static ProcedureAttribute m_Attr = null;
		static public ProcedureAttribute Attr
		{
			get
			{
				if( m_Attr == null )
				{
					MemberInfo info = typeof( T );
					if( info == null )
						throw new System.Exception( "procedure type is wrong" );

					m_Attr = info.GetCustomAttribute<ProcedureAttribute>();
					if( m_Attr == null )
						throw new System.Exception( "procedure type is wrong" );
				}
				return m_Attr;
			}
		}
	};

	//------------------------------------------------------------------------	
	abstract public class PROCEDURE_READ_BASE
	{
		[ProcedureValue( eProcedureValueType.ReturnValue )]
		public int return_code;
	};

	//------------------------------------------------------------------------	
	public class PROCEDURE_READ_DEFAULT : PROCEDURE_READ_BASE
	{
	};
}
