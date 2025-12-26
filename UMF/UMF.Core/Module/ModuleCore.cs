//////////////////////////////////////////////////////////////////////////
//
// ModuleCore
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

namespace UMF.Core.Module
{
	//------------------------------------------------------------------------	
	public abstract class ModuleCoreBase
	{
		public abstract string ModuleName { get; }

		public ModuleCoreBase()
		{
			ModuleManager.Instance.AddModule( this );
		}
	}
}
