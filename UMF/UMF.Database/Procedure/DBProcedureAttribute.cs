//////////////////////////////////////////////////////////////////////////
//
// DBProcedureAttribute
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

using UMF.Core;

namespace UMF.Database
{
	public enum eProcedureExecute
	{
		NonQuery,
		Reader,
	}

	public enum eProcedureValueType
	{
		None,
		Serialize,
		ReturnValue,
		OutputValue,
		SerializeEmptiable,
		SerializeNullable,
	}

	//------------------------------------------------------------------------	
	[System.AttributeUsage( System.AttributeTargets.Field )]
	public class ProcedureValueAttribute : SerializeAttribute
	{
		public eProcedureValueType Type { get; private set; }

		public bool IsNullable { get { return Type == eProcedureValueType.SerializeNullable; } }
		public ProcedureValueAttribute( eProcedureValueType type )
		{
			Type = type;
		}

		override public bool IsSerializable
		{
			get
			{
				switch( Type )
				{
					case eProcedureValueType.Serialize:
					case eProcedureValueType.SerializeEmptiable:
					case eProcedureValueType.SerializeNullable:
						return true;
				}
				return false;
			}
		}
	}

	//------------------------------------------------------------------------	
	[System.AttributeUsage( System.AttributeTargets.Class )]
	public class ProcedureAttribute : System.Attribute
	{
		public string SPName { get; private set; }
		public eProcedureExecute ExecuteType { get; private set; }
		public eCoreLogType LogType { get; private set; }

		public string GetString( System.Type procedureType )
		{
			return ".dbo." + SPName;
		}

		public ProcedureAttribute( string spname, eProcedureExecute executeType )
		{
			SPName = spname;
			ExecuteType = executeType;
			LogType = eCoreLogType.Detail;
		}

		public ProcedureAttribute( string spname, eProcedureExecute executeType, eCoreLogType logType )
		{
			SPName = spname;
			ExecuteType = executeType;
			LogType = logType;
		}
	}

	//------------------------------------------------------------------------	
	[System.AttributeUsage( System.AttributeTargets.Field )]
	public class ProcedureParamListAttribute : ListAttribute
	{
		public int StartIndex { get; private set; }
		public int Count { get; private set; }
		public object InvalidKey { get; private set; }

		public ProcedureParamListAttribute( int startIndex, int count, string keyName, object invalidKey )
			: base( keyName )
		{
			StartIndex = startIndex;
			Count = count;
			InvalidKey = invalidKey;
		}
	}
}
