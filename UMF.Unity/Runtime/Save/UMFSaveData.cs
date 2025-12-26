//////////////////////////////////////////////////////////////////////////
//
// UMFSaveData
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
using UMF.Core;

namespace UMF.Unity
{
	//------------------------------------------------------------------------
	public interface IUMFSaveModule
	{
		string RootPath { get; }
		string FileExtension { get; }
		string EncFileExtension { get; }
		object Load( UMFSaveSettingBase setting, string _base_path, string encrypt_key, bool use_encrypt );
		void Save( object data, UMFSaveSettingBase setting, string _base_path, string encrypt_key, bool use_encrypt );
	}

	//------------------------------------------------------------------------
	public abstract class UMFSaveSettingBase
	{
		public virtual bool ReloadContruct { get; } = true;
		public abstract string SAVE_FILE { get; }
		public abstract Type DATA_TYPE { get; }
		public abstract void LoadDataObject( object obj );
		protected abstract void DoSave();
		public virtual bool CheckInitialData() { return false; }
		protected virtual void OnLoaded() { }

		public virtual bool IsPersistent { get { return false; } }

		protected bool NeedSave { get; private set; }
		public virtual void SAVE()
		{
			if( mIgnoreSave == false )
				DoSave();
			else
				NeedSave = true;
		}

		protected bool mIgnoreSave = false;

		public void SetSaveIgnore()
		{
			mIgnoreSave = true;
		}
		public void UnsetSaveIgnore( bool forced_save = false )
		{
			mIgnoreSave = false;
			if( NeedSave || forced_save )
			{
				NeedSave = false;
				SAVE();
			}
		}

		protected UMFSaveSettingBase()
		{
			if( ReloadContruct )
				Reload();
		}

		public void Reload()
		{
			NeedSave = false;
			object data = UMFSaveManager.Instance.Load( this );
			LoadDataObject( data );
		}

		public bool ReloadCustomPath( string path )
		{
			NeedSave = false;
			object data = UMFSaveManager.Instance.LoadCustomPath( this, path );
			if( data != null )
			{
				LoadDataObject( data );
				return true;
			}

			return false;
		}

		//public virtual void LoadSaveSetting()
		//{
		//	NeedSave = false;
		//	object data = UMFSaveManager.Instance.Load( this );
		//	LoadDataObject( data );
		//}
	}

	//------------------------------------------------------------------------	
	public abstract class TUMFSaveSettingBase<DT> : UMFSaveSettingBase where DT : class, new()
	{
		public sealed override Type DATA_TYPE => typeof( DT );

		protected DT mData = new DT();
		public DT DATA { get { return mData; } }

		//------------------------------------------------------------------------
		public sealed override void LoadDataObject( object obj )
		{
			bool is_dirty = false;
			if( obj != null )
			{
				mData = obj as DT;
				is_dirty = true;
			}
			
			if( CheckInitialData() )
				is_dirty = true;

			if( is_dirty )
				DoSave();

			OnLoaded();
		}

		//------------------------------------------------------------------------
		protected override void DoSave()
		{
			UMFSaveManager.Instance.Save( mData, this );
		}
	}

	//------------------------------------------------------------------------
	public abstract class UMFSaveSettingBaseSingleton<T, DT> : TUMFSaveSettingBase<DT>
		where T : TUMFSaveSettingBase<DT>, new()
		where DT : class, new()
	{
		static T _Instance = null;
		static object _lock = new object();

		static public T Instance
		{
			get
			{
				if( _Instance == null )
				{
					lock( _lock )
					{
						_Instance = new T();
					}
				}

				return _Instance;
			}
		}
	}

	//------------------------------------------------------------------------	
	public class UMFSaveManager : Singleton<UMFSaveManager>
	{
		IUMFSaveModule mSaveModule = null;
		public IUMFSaveModule SaveModule { get { return mSaveModule; } }
		public string BasePath { get; private set; } = "";
		public string EncryptKey { get; private set; } = "";
		public bool UseEncrypt { get; private set; } = false;

		public string CurrentSavePath { get; set; } = "";

		//------------------------------------------------------------------------
		public void Init( IUMFSaveModule save_module, string base_path, string encrypt_key, bool use_encrypt )
		{
			mSaveModule = save_module;
			BasePath = base_path;
			EncryptKey = encrypt_key;
			UseEncrypt = use_encrypt;
		}

		//------------------------------------------------------------------------
		public object Load( UMFSaveSettingBase setting )
		{
			string curr_path = BasePath;

			if( setting.IsPersistent == false && string.IsNullOrEmpty( CurrentSavePath ) == false )
				curr_path = $"{BasePath}/{CurrentSavePath}";

			return mSaveModule.Load( setting, curr_path, EncryptKey, UseEncrypt );
		}

		//------------------------------------------------------------------------
		public object LoadCustomPath( UMFSaveSettingBase setting, string path )
		{
			string curr_path = BasePath;

			if( setting.IsPersistent == false && string.IsNullOrEmpty( path ) == false )
				curr_path = $"{BasePath}/{path}";

			return mSaveModule.Load( setting, curr_path, EncryptKey, UseEncrypt );
		}

		//------------------------------------------------------------------------
		public void Save( object data, UMFSaveSettingBase setting )
		{
			string curr_path = BasePath;
			if( setting.IsPersistent == false && string.IsNullOrEmpty( CurrentSavePath ) == false )
				curr_path = $"{BasePath}/{CurrentSavePath}";

			mSaveModule.Save( data, setting, curr_path, EncryptKey, UseEncrypt );
		}
	}
}
