//////////////////////////////////////////////////////////////////////////
//
// ExtensionUtil
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
using System.Reflection;

namespace UMF.Core
{
	public static class ExtensionUtil
	{
		//------------------------------------------------------------------------
		public static int AddOrInsertList<T>( this IList<T> list, T item, int insert_above_index = -1 )
		{
			if( list == null )
				return -1;

			int ret_index = 0;
			if( insert_above_index >= 0 )
			{
				if( list.Count >= insert_above_index )
				{
					list.Insert( insert_above_index, item );
					ret_index = insert_above_index;
				}
				else
				{
					ret_index = 0;
					list.Insert( 0, item );
				}
			}
			else
			{
				ret_index = list.Count;
				list.Add( item );
			}

			return ret_index;
		}

		//------------------------------------------------------------------------
		public static void ShuffleList<T>( this IList<T> list )
		{
			if( list == null )
				return;

			UMFRandom rnd = UMFRandom.Instance;
			int n = list.Count;
			while( n > 1 )
			{
				n--;
				int k = rnd.NextRange( 0, n );
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		//------------------------------------------------------------------------
		public static bool AddIfNotContain<T>( this IList<T> list, T value )
		{
			if( list == null )
				return false;

			if( list.Contains( value ) == false )
			{
				list.Add( value );
				return true;
			}

			return false;
		}

		//------------------------------------------------------------------------		
		public static T RandomValue<T>( this IList<T> list )
		{
			if( list == null || list.Count <= 0 )
				return default( T );

			if( list.Count > 1 )
				return list[UMFRandom.Instance.NextRange( 0, list.Count - 1 )];

			return list[0];
		}

		//------------------------------------------------------------------------
		public static T SafeParseIndexValue<T>( this IList<string> list, int idx, T def_value )
		{
			if( list == null || list.Count <= 0 || idx >= list.Count )
				return def_value;

			return StringUtil.SafeParse<T>( list[idx], def_value );
		}

		//------------------------------------------------------------------------
		public static Dictionary<K, V> ResetValues<K, V>( this Dictionary<K, V> dic )
		{
			if( dic == null )
				return dic;

			dic.Keys.ToList().ForEach( a => dic[a] = default( V ) );

			return dic;
		}

		public static void AddOrUpdate<K,V>( this Dictionary<K,V> dic, K key, V value )
		{
			if( dic.ContainsKey( key ) )
				dic[key] = value;
			else
				dic.Add( key, value );
		}

		//------------------------------------------------------------------------		
		public static bool CheckMethodEqual( MethodInfo a, MethodInfo b )
		{
			if( a.Name != b.Name )
				return false;

			ParameterInfo[] param_a = a.GetParameters();
			ParameterInfo[] param_b = b.GetParameters();
			if( param_a.Length != param_b.Length )
				return false;

			for( int i = 0; i < param_a.Length; i++ )
			{
				if( param_a[i].ParameterType != param_b[i].ParameterType )
					return false;
			}

			if( a.ReturnParameter.ParameterType != b.ReturnParameter.ParameterType )
				return false;

			return true;
		}

		//------------------------------------------------------------------------
		public static MethodInfo[] GetAllMethods( this Type _type, BindingFlags flags )
		{
			List<MethodInfo> list = new List<MethodInfo>();
			Type curr_type = _type;
			while( true )
			{
				MethodInfo[] methods = _type.GetMethods( flags );
				for( int i = 0; i < methods.Length; i++ )
				{
					if( list.Exists( a => CheckMethodEqual( a, methods[i] ) ) == false )
						list.Add( methods[i] );
				}

				_type = _type.BaseType;
				if( _type == null )
					break;
			}

			if( list.Count > 0 )
				return list.ToArray();

			return new MethodInfo[0];
		}
	}
}
