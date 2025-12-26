//////////////////////////////////////////////////////////////////////////
//
// ModuleManager
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
using System.Collections.Generic;
using System.Linq;

namespace UMF.Core.Module
{
	public class ModuleManager : Singleton<ModuleManager>
	{
		List<ModuleCoreBase> mModuleList = new List<ModuleCoreBase>();

		//------------------------------------------------------------------------
		public void AddModule( ModuleCoreBase module )
		{
			if( mModuleList.Exists( a => a.ModuleName == module.ModuleName ) )
				throw new System.Exception( $"!ModuleManager:Already added module : {module.ModuleName}" );

			mModuleList.Add( module );

			Log.WriteImportant( $"# ModuleManager Add:{module.ModuleName}" );
		}

		//------------------------------------------------------------------------
		public MT GetModule<MT>( string _name ) where MT : ModuleCoreBase
		{
			ModuleCoreBase module = mModuleList.Find( a => a.ModuleName == _name );
			if( module != null )
				return module as MT;

			return null;
		}

		//------------------------------------------------------------------------
		public List<T> FindModuleInterface<T>() where T : class
		{
			List<T> list = null;
			Type find_type = typeof( T );

			foreach( ModuleCoreBase module in mModuleList )
			{
				if( module.GetType().GetInterfaces().Contains( find_type ) )
				{
					if( list == null )
						list = new List<T>();

					list.Add( module as T );
				}
			}

			return list;
		}
		public T FindModuleInterface<T>( string _name ) where T : class
		{
			Type find_type = typeof( T );
			foreach( ModuleCoreBase module in mModuleList )
			{
				if( module.GetType().GetInterfaces().Contains( find_type ) && module.ModuleName == _name )
					return module as T;
			}

			return null;
		}
	}
}
