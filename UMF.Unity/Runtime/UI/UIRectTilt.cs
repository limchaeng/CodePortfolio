//////////////////////////////////////////////////////////////////////////
//
// UIRectTilt
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

namespace UMF.Unity.UI
{
    [ExecuteInEditMode]
    [RequireComponent( typeof(RectTransform) )]
    public class UIRectTilt : MonoBehaviour
    {
        public RectTransform m_Target = null;
        public float m_TiltAmount = 10f;   // 최대 기울기 각도
        public float m_SmoothTime = 0.15f; // 복원 속도

        Vector3 mLastPosition;
        Quaternion mAwakeRotation = Quaternion.identity;
        float mCurrentTilt = 0f;
        float mVelocity = 0f;

        private void Awake()
        {
            if( m_Target == null )
                m_Target = gameObject.GetComponent<RectTransform>();

            mLastPosition = m_Target.anchoredPosition;
            mAwakeRotation = m_Target.localRotation;
        }

        private void OnDisable()
        {
            m_Target.localRotation = mAwakeRotation;
        }

        void Update()
        {
            if( m_Target == null )
                return;

            // 이동량 계산
            float deltaX = m_Target.anchoredPosition.x - mLastPosition.x;
            mLastPosition = m_Target.anchoredPosition;

            // 이동 속도 기반 tilt 목표각 (왼쪽 = 음수, 오른쪽 = 양수)
            float targetTilt = Mathf.Clamp( -deltaX * 0.5f, -m_TiltAmount, m_TiltAmount );

            // 부드럽게 보정 (감속 복원)
            mCurrentTilt = Mathf.SmoothDamp( mCurrentTilt, targetTilt, ref mVelocity, m_SmoothTime );

            // 회전 적용 (z축 회전)
            m_Target.localRotation = Quaternion.Euler( 0f, 0f, mCurrentTilt );
        }
    }
}