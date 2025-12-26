//////////////////////////////////////////////////////////////////////////
//
// AbstractFactory
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

using System.Collections.Generic;
using System;
using System.Linq;

namespace UMF.Core
{
	//------------------------------------------------------------------------
	public abstract class FactoryBase<KeyType, ValueBaseType, ManagerType> : Singleton<ManagerType>
		where ValueBaseType : class
		where ManagerType : class, new()
	{
		protected Dictionary<KeyType, Type> mFactories = new Dictionary<KeyType, Type>();

		//------------------------------------------------------------------------
		public virtual ValueBaseType Create( KeyType key )
		{
			Type createType;
			if( mFactories.TryGetValue( key, out createType ) == false )
				throw new Exception( string.Format( "Can't find key({0}) in {1}", key, this.ToString() ) );

			return (ValueBaseType)Activator.CreateInstance( createType );
		}

		//------------------------------------------------------------------------
		public List<KeyType> GetKeyList()
		{
			return mFactories.Keys.ToList();
		}
	}

	//------------------------------------------------------------------------	
	public abstract class AbstractFactory<KeyType, ValueBaseType, ManagerType> : FactoryBase<KeyType, ValueBaseType, ManagerType>
		where ValueBaseType : class
		where ManagerType : class, new()
	{
		//------------------------------------------------------------------------
		protected void AddFactory<T>( KeyType key ) where T : class
		{
			if( typeof( T ).IsSubclassOf( typeof( ValueBaseType ) ) == false && typeof( T ) != typeof( ValueBaseType ) )
				throw new Exception( typeof( T ).Name + " is not driven from " + typeof( ValueBaseType ).Name );

			mFactories.Add( key, typeof( T ) );
		}
	}

	//------------------------------------------------------------------------	
	public class InterfaceFactory<KeyType, InterfaceType, ManagerType> : FactoryBase<KeyType, InterfaceType, ManagerType>
		where InterfaceType : class
		where ManagerType : class, new()
	{
		//------------------------------------------------------------------------		
		public void AddFactory<T>( KeyType key ) where T : class
		{
			Type it = typeof( T ).GetInterface( typeof( InterfaceType ).Name );
			if( it == null )
				throw new Exception( typeof( T ).Name + " is not interface from " + typeof( InterfaceType ).Name );

			mFactories.Add( key, typeof( T ) );
		}
	}
}
