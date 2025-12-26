//////////////////////////////////////////////////////////////////////////
//
// ISqlCommand
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

namespace UMF.Database
{
	//------------------------------------------------------------------------
	public interface ISqlCommand
	{
		void Begin<CallT, ReadT>( string sp_name, int cmd_timeout, CallT callObject, DBHandlerObject data ) where ReadT : PROCEDURE_READ_BASE;
		IAsyncResult Execute( eProcedureExecute execute_type );
		PROCEDURE_READ_BASE Read<ReadT>( eProcedureExecute execute_type, IAsyncResult result ) where ReadT : PROCEDURE_READ_BASE;
		void End();
		void Close();
		void Dump( string prefix );

		// Command.Parameter Write
		void AddWithValue( string parameterName, object value );
		void AddTableValued( string parameterName, string type_name, object value );

		/// <summary>
		///   none_min_max : 0=none, 1=min, 2=max
		/// </summary>
		void AddDateTime( string parameterName, int none_min_max, object value );
		void AddReturnValue( string parameterName, Type field_type );
		void AddOutputValue( string parameterName, Type field_type );

		// Read
	}
}
