//////////////////////////////////////////////////////////////////////////
//
// PBX_ComponentPool
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace UMF.Unity
{
    public class UMFComponentPoolManager
    {
        Dictionary<System.Type, UMFComponentPoolBase> mManagedPools = new Dictionary<System.Type, UMFComponentPoolBase>();

        public int Count { get { return mManagedPools.Count; } }
        public override string ToString()
        {
            return $"UMFComponentPoolManager {string.Join( ",", mManagedPools.Select( a => $"[{a.ToString()}]" ) )}";
        }

        ~UMFComponentPoolManager()
        {
            mManagedPools.Clear();
        }

        public void Create<T>( T src, GameObject parent = null ) where T : Component
        {
            if( mManagedPools.ContainsKey( typeof( T ) ) )
                throw new UnityException( $"already exist type : {typeof( T )}" );

            UMFComponentPool<T> container = new UMFComponentPool<T>();
            container.Create( src, parent );

            mManagedPools.Add( typeof( T ), container );
            Debug.Log( $"UMFComponentPoolManager new Created {typeof( T )} : {mManagedPools.Count}" );
        }

        public T Get<T>( int sibiling_idx ) where T : Component
        {
            T ret = Get<T>();
            ret.transform.SetSiblingIndex( sibiling_idx );

            return ret;
        }

        public T Get<T>() where T : Component
        {
            UMFComponentPoolBase container;
            if( mManagedPools.TryGetValue( typeof( T ), out container ) )
            {
                return container.Get() as T;
            }

            throw new UnityException( $"not found type : {typeof( T )}" );
        }

        public void Release<T>( T comp ) where T : Component
        {
            UMFComponentPoolBase container;
            if( mManagedPools.TryGetValue( typeof( T ), out container ) )
            {
                container.Release( comp );
            }
        }

        public void CurrentRelease<T>() where T : Component
        {
            UMFComponentPoolBase container;
            if( mManagedPools.TryGetValue( typeof( T ), out container ) )
            {
                container.CurrentRelease();
            }
        }

        public void CurrentReleaseAll()
        {
            foreach( UMFComponentPoolBase pool in mManagedPools.Values )
            {
                pool.CurrentRelease();
            }
        }

        public void Clear<T>() where T : Component
        {
            UMFComponentPoolBase container;
            if( mManagedPools.TryGetValue( typeof( T ), out container ) )
            {
                container.Clear();
                mManagedPools.Remove( typeof( T ) );
            }

            Debug.Log( $"UMFComponentPoolManager Clear {typeof( T )} : {mManagedPools.Count}" );
        }

        public void ClearAll()
        {
            foreach( UMFComponentPoolBase pool in mManagedPools.Values )
            {
                pool.Clear();
            }
            mManagedPools.Clear();

            Debug.Log( $"UMFComponentPoolManager ClearAll : {mManagedPools.Count}" );
        }

        public List<T> CurrentList<T>() where T : Component
        {
            UMFComponentPoolBase container;
            if( mManagedPools.TryGetValue( typeof( T ), out container ) )
            {
                return container.CurrentList() as List<T>;
            }

            throw new UnityException( $"not found type : {typeof( T )}" );
        }

        // static
        //------------------------------------------------------------------------
        public static ObjectPool<GameObject> CreateSimplePool( GameObject source )
        {
            return CreateSimplePool( source.transform.parent.gameObject, source );
        }
        public static ObjectPool<GameObject> CreateSimplePool( GameObject parent, GameObject source )
        {
            ObjectPool<GameObject> pool = new ObjectPool<GameObject>( () =>
            {
                GameObject go = parent.AddChild( source );
                go.gameObject.SetActive( false );
                return go;

            }, ( GameObject go ) =>
            {
                go.SetActive( true );
            }, ( GameObject go ) =>
            {
                go.SetParent( parent );
                go.SetActive( false );
            }, ( GameObject go ) =>
            {
                GameObject.Destroy( go );
            } );

            return pool;
        }
        public static ObjectPool<T> CreateSimplePool<T>( T source, bool src_inactive = true ) where T : Component
        {
            return CreateSimplePool<T>( source.transform.parent.gameObject, source, src_inactive );
        }
        public static ObjectPool<T> CreateSimplePool<T>( GameObject parent, T source, bool src_inactive = true ) where T : Component
        {
            if( src_inactive )
                source.gameObject.SetActive( false );

            ObjectPool<T> pool = new ObjectPool<T>( () =>
            {
                T comp = parent.AddChildWithComponent<T>( source.gameObject );
                comp.gameObject.SetActive( false );
                return comp;

            }, ( T comp ) =>
            {
                comp.gameObject.SetActive( true );
            }, ( T comp ) =>
            {
                comp.gameObject.SetParent( parent );
                comp.gameObject.SetActive( false );
            }, ( T comp ) =>
            {
                GameObject.Destroy( comp.gameObject );
            } );

            return pool;
        }
    }

    //------------------------------------------------------------------------
    public abstract class UMFComponentPoolBase
    {
        public abstract void Create( Component src, GameObject parent );
        public abstract void CurrentRelease();
        public abstract Component Get();
        public abstract void Release( Component comp );
        public abstract IList CurrentList();
        public abstract void Clear();
    }

    public class UMFComponentPool<T> : UMFComponentPoolBase where T : Component
    {
        protected ObjectPool<T> mPool = null;
        protected List<T> mCurrentList = new List<T>();

        public override void Create( Component src, GameObject parent )
        {
            if( parent == null )
                mPool = UMFComponentPoolManager.CreateSimplePool<T>( src as T );
            else
                mPool = UMFComponentPoolManager.CreateSimplePool<T>( parent, src as T );
        }

        public override void CurrentRelease()
        {
            if( mPool != null )
            {
                mCurrentList.ForEach( a => mPool.Release( a ) );
                mCurrentList.Clear();
            }
        }

        public override void Clear()
        {
            if( mPool != null )
            {
                CurrentRelease();
                mPool.Clear();
            }
        }

        public override Component Get()
        {
            T obj = mPool.Get();
            mCurrentList.Add( obj );
            return obj;
        }

        public override void Release( Component comp )
        {
            T obj = comp as T;
            mCurrentList.Remove( obj );
            mPool.Release( obj );
        }

        public override IList CurrentList()
        {
            return mCurrentList;
        }

        public override string ToString()
        {
            if( mPool == null )
                return "Empty";
            
            return $"{typeof(T)} p={mPool.CountAll} c={mCurrentList.Count}";
        }
    }
}
