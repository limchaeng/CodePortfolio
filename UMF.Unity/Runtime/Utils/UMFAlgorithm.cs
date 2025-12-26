//////////////////////////////////////////////////////////////////////////
//
// UMFAlgorithm
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
using UnityEngine;

namespace UMF.Unity
{
    public static class UMFAlgorithm
    {
        //------------------------------------------------------------------------
        public static List<T> ArrayToList<T>( T[,] arr )
        {
            List<T> list = new List<T>();
            int row = arr.GetLength( 0 );
            int col = arr.GetLength( 1 );

            for( int r = 0; r < row; r++ )
            {
                for( int c = 0; c < col; c++ )
                {
                    list.Add( arr[r, c] );
                }
            }

            return list;
        }

        //------------------------------------------------------------------------
        public static T[,] ArrayRotateRight<T>( T[,] arr )
        {
            // a: 세로(행), b: 가로(열)
            int a = arr.GetLength( 0 );
            int b = arr.GetLength( 1 );

            T[,] rot = new T[b, a];
            for( int y = 0; y < a; y++ )
                for( int x = 0; x < b; x++ )
                    rot[x, a - 1 - y] = arr[y, x];
            return rot;
        }

        public static T[,] ArrayRotateLeft<T>( T[,] arr )
        {
            int a = arr.GetLength( 0 );
            int b = arr.GetLength( 1 );

            T[,] rot = new T[b, a];
            for( int y = 0; y < a; y++ )
                for( int x = 0; x < b; x++ )
                    rot[b - 1 - x, y] = arr[y, x];
            return rot;
        }

        public static T[,] ArrayRotate180<T>( T[,] arr )
        {
            int a = arr.GetLength( 0 );
            int b = arr.GetLength( 1 );

            T[,] rot = new T[a, b];
            for( int y = 0; y < a; y++ )
                for( int x = 0; x < b; x++ )
                    rot[a - 1 - y, b - 1 - x] = arr[y, x];
            return rot;
        }

        // 순열 찾기
        public static void Permute<T>( T[] arr, int start, List<T[]> results )
        {
            if( start == arr.Length )
            {
                // 배열 복사 후 저장
                results.Add( (T[])arr.Clone() );
                return;
            }

            for( int i = start; i < arr.Length; i++ )
            {
                // swap
                (arr[start], arr[i]) = (arr[i], arr[start]);
                Permute( arr, start + 1, results );
                // backtrack
                (arr[start], arr[i]) = (arr[i], arr[start]);
            }
        }
    }
}