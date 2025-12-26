//////////////////////////////////////////////////////////////////////////
//
// AlphabetSortedAsset
// 
// Created by LCY.
//
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// 다량의 에셋을 이름별로 알파벳 소팅 및 캐싱하여 에디터에서 빠르게 사용할수 있는 유틸.
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UMF.Unity.EditorUtil
{
	//------------------------------------------------------------------------	
	public class AlphabetSortedAssetData<T> where T : UnityEngine.Object
	{
		public string short_name = "";
		public string asset_name = "";
		public string asset_guid = "";
		public string asset_path = "";
		public T asset = null;

		bool is_loaded = false;

		public void LoadAsset()
		{
			if( is_loaded || asset != null )
				return;

			is_loaded = true;
			asset = AssetDatabase.LoadAllAssetsAtPath( asset_path ).OfType<T>().FirstOrDefault();
		}
	}

	//------------------------------------------------------------------------	
	public class AlphabetSortedContainerData<T> where T : UnityEngine.Object
	{
		public string alphabet = "";
		public List<AlphabetSortedAssetData<T>> asset_list = new List<AlphabetSortedAssetData<T>>();

		public bool Has( string _name )
		{
			return asset_list.Exists( a => a.asset_name == _name );
		}
	}

	//------------------------------------------------------------------------
	public class AlphabetSortedContainer<T> where T : UnityEngine.Object
	{
		public List<AlphabetSortedContainerData<T>> list = new List<AlphabetSortedContainerData<T>>();

		public bool Has( string _name )
		{
			return list.Exists( a => a.Has( _name ) );
		}
	}

	//------------------------------------------------------------------------
	public static class AlphabetSortedAsset
	{
		//------------------------------------------------------------------------
		public static AlphabetSortedContainer<T> CreateFindedAssetContainer<T>( string asset_folder, string find_type, string ignore_prefix = "" ) where T : UnityEngine.Object
		{
			AlphabetSortedContainer<T> container = new AlphabetSortedContainer<T>();

			List<UMFEditorUtil.LoadAllAssetInfoData<T>> finded_list = UMFEditorUtil.LoadAllAssetInfoAtFolder<T>( asset_folder );
			if( finded_list == null )
				return container;

			foreach( UMFEditorUtil.LoadAllAssetInfoData<T> data in finded_list )
			{
				string _name = data.asset_name;
				if( string.IsNullOrEmpty( ignore_prefix ) == false && _name.StartsWith( ignore_prefix ) )
				{
					_name = _name.Substring( ignore_prefix.Length, _name.Length - ignore_prefix.Length );
				}

				if( _name.Length <= 0 )
					continue;

				string alphabet = _name[0].ToString().ToUpper();

				AlphabetSortedContainerData<T> exist_data = container.list.Find( a => a.alphabet == alphabet );
				if( exist_data == null )
				{
					exist_data = new AlphabetSortedContainerData<T>();
					exist_data.alphabet = alphabet;
					container.list.Add( exist_data );
				}

				AlphabetSortedAssetData<T> asset_data = new AlphabetSortedAssetData<T>();
				asset_data.asset_name = data.asset_name;
				asset_data.short_name = _name;
				asset_data.asset_guid = data.guid;
				asset_data.asset_path = data.asset_path;
				asset_data.asset = data.asset;

				exist_data.asset_list.Add( asset_data );
			}

			foreach( AlphabetSortedContainerData<T> data in container.list )
			{
				data.asset_list = data.asset_list.OrderBy( a => a.short_name ).ToList();
			}

			container.list = container.list.OrderBy( a => a.alphabet ).ToList();

			return container;
		}

		//------------------------------------------------------------------------
		public static AlphabetSortedContainer<T> CreateListContainer<T>( List<T> input_list, string ignore_prefix = "" ) where T : UnityEngine.Object
		{
			AlphabetSortedContainer<T> container = new AlphabetSortedContainer<T>();
			if( input_list == null )
				return container;

			foreach( T data in input_list )
			{
				string _name = data.name;
				if( string.IsNullOrEmpty( ignore_prefix ) == false && _name.StartsWith( ignore_prefix ) )
				{
					_name = _name.Substring( ignore_prefix.Length, _name.Length - ignore_prefix.Length );
				}

				if( _name.Length <= 0 )
					continue;

				string alphabet = _name[0].ToString().ToUpper();

				AlphabetSortedContainerData<T> exist_data = container.list.Find( a => a.alphabet == alphabet );
				if( exist_data == null )
				{
					exist_data = new AlphabetSortedContainerData<T>();
					exist_data.alphabet = alphabet;
					container.list.Add( exist_data );
				}

				AlphabetSortedAssetData<T> asset_data = new AlphabetSortedAssetData<T>();
				asset_data.short_name = _name;
				asset_data.asset = data;
				exist_data.asset_list.Add( asset_data );
			}

			foreach( AlphabetSortedContainerData<T> data in container.list )
			{
				data.asset_list = data.asset_list.OrderBy( a => a.short_name ).ToList();
			}

			container.list = container.list.OrderBy( a => a.alphabet ).ToList();

			return container;
		}
	}
}
