//////////////////////////////////////////////////////////////////////////
//
// UIScrollPullRefresh
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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UMF.Unity.UI
{
    [AddComponentMenu( "UMF/UI/UI ScrollView Pull Refresh" )]
    public class UIScrollPullRefresh : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ScrollRect m_ScrollRect;
        public float m_RefreshThreshold = 100f; // 당길 거리 ? lcy : contents 상단 포지션의 y 값임.
        public RectTransform m_Content;
        public GameObject[] m_RefreshIndicator; // 새로고침 아이콘 또는 텍스트
        public UnityEvent m_RefreshEvent;
        public UnityEvent m_DragBeginEvent;
        public UnityEvent m_DragEndEvent;

        private bool mIsDragging = false;
        private bool mIsRefreshing = false;
        private Vector2 mDragStartPos;

        private void Start()
        {
            RefreshIndicatorActive( false );
        }

        private void Update()
        {
            if( m_Content.anchoredPosition.y <= m_RefreshThreshold )
            {
                Vector2 anchoredPosition = m_Content.anchoredPosition;
                anchoredPosition.y = m_RefreshThreshold;
                m_Content.anchoredPosition = anchoredPosition;
            }
        }

        public void OnBeginDrag( PointerEventData eventData )
        {
            mIsDragging = true;
            mDragStartPos = m_Content.anchoredPosition;
            m_DragBeginEvent.Invoke();
        }

        public void OnDrag( PointerEventData eventData )
        {
            if( mIsDragging && !mIsRefreshing )
            {
                float dragDistance = m_Content.anchoredPosition.y;
                if( dragDistance <= m_RefreshThreshold )
                {
                    RefreshIndicatorActive( true );
                }
                else
                {
                    RefreshIndicatorActive( false );
                }
            }
        }

        public void OnEndDrag( PointerEventData eventData )
        {
            if( mIsDragging && !mIsRefreshing )
            {
                float dragDistance = m_Content.anchoredPosition.y;
                if( dragDistance <= m_RefreshThreshold )
                {
                    StartCoroutine( DoRefresh() );
                }
                else
                {
                    RefreshIndicatorActive( false );
                }
            }
            mIsDragging = false;
            m_DragEndEvent.Invoke();
        }

        private IEnumerator DoRefresh()
        {
            mIsRefreshing = true;

            m_ScrollRect.StopMovement();

            yield return new WaitForSeconds( 0.2f );
            m_RefreshEvent.Invoke();
            RefreshIndicatorActive( false );
            mIsRefreshing = false;
        }

        private void RefreshIndicatorActive( bool isActive )
        {
            foreach( var item in m_RefreshIndicator )
            {
                item.SetActive( isActive );
            }
        }
    }
}