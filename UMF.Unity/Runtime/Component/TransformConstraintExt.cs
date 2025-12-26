//////////////////////////////////////////////////////////////////////////
//
// TransformConstraintExt
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
    public class TransformConstraintExt : MonoBehaviour
    {
        public Transform m_Source;

        public bool m_RotationLock = false;

        Quaternion mRotationInitial = Quaternion.identity;

        private void Awake()
        {
            mRotationInitial = transform.rotation;
        }

        void LateUpdate()
        {
            if( m_Source == null )
                return;

            if( m_RotationLock )
            {
                transform.rotation = mRotationInitial;
            }
        }
    }
}