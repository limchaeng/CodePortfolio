//////////////////////////////////////////////////////////////////////////
//
// RadialLayoutExt
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
using UnityEngine.UI;

namespace UMF.Unity.UI
{
    [AddComponentMenu( "UMF/UI/Radial Layout Ext", 30 )]
    public class RadialLayoutExt : LayoutGroup
    {
        public float m_Radius;
        [Range( 0f, 360f )]
        public float MinAngle = 0f;
        [Range( 0f, 360f )]
        public float MaxAngle = 360f;
        [Range( 0f, 360f )]
        public float StartAngle = 90f;
        public bool OnlyLayoutVisible = true;

        protected override void OnEnable() { base.OnEnable(); CalculateRadial(); }
        
        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
        }

        public override void CalculateLayoutInputVertical()
        {
        }
        public override void CalculateLayoutInputHorizontal()
        {
            CalculateRadial();
        }

//#if UNITY_EDITOR
//        protected override void OnValidate()
//        {
//            base.OnValidate();
//            CalculateRadial();
//        }
//#endif

        protected override void OnDisable()
        {
            m_Tracker.Clear(); // key change - do not restore - false
            LayoutRebuilder.MarkLayoutForRebuild( rectTransform );
        }

        List<RectTransform> tmpActiveChilds = new List<RectTransform>();
        void CalculateRadial()
        {
            m_Tracker.Clear();
            if( transform.childCount == 0 )
                return;

            int ChildrenToFormat = 0;
            if( OnlyLayoutVisible )
            {
                for( int i = 0; i < transform.childCount; i++ )
                {
                    RectTransform child = (RectTransform)transform.GetChild( i );
                    if( ( child != null ) && child.gameObject.activeSelf )
                        ++ChildrenToFormat;
                }
            }
            else
            {
                ChildrenToFormat = transform.childCount;
            }

            float fOffsetAngle = ( MaxAngle - MinAngle ) / ChildrenToFormat;
            float fAngle = StartAngle;

            tmpActiveChilds.Clear();
            for( int i = 0; i < transform.childCount; i++ )
            {
                RectTransform child = (RectTransform)transform.GetChild( i );
                if( ( child != null ) && ( !OnlyLayoutVisible || child.gameObject.activeSelf ) )
                {
                    //Adding the elements to the tracker stops the user from modifying their positions via the editor.
                    m_Tracker.Add( this, child,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.Pivot );
                    
                    Vector3 vPos = new Vector3( Mathf.Cos( fAngle * Mathf.Deg2Rad ), Mathf.Sin( fAngle * Mathf.Deg2Rad ), 0 );
                    child.localPosition = vPos * m_Radius;
                    //Force objects to be center aligned, this can be changed however I'd suggest you keep all of the objects with the same anchor points.
                    child.anchorMin = child.anchorMax = child.pivot = new Vector2( 0.5f, 0.5f );
                    fAngle += fOffsetAngle;

                    tmpActiveChilds.Add( child );
                }
            }

            // preferred 사이즈 계산
            Vector3[] corners = new Vector3[4];
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            for( int i = 0; i < tmpActiveChilds.Count; i++ )
            {
                tmpActiveChilds[i].GetWorldCorners( corners );

                if( i == 0 )
                {
                    min = corners[0];
                    max = corners[2];
                }
                else
                {
                    min = Vector3.Min( min, corners[0] );
                    max = Vector3.Max( max, corners[2] );
                }
            }
            tmpActiveChilds.Clear();

            Vector3 localMin = transform.InverseTransformPoint( min );
            Vector3 localMax = transform.InverseTransformPoint( max );
            Vector2 size = localMax - localMin;

            SetLayoutInputForAxis( size.x, size.x, -1, 0 );
            SetLayoutInputForAxis( size.y, size.y, -1, 1 );
        }
    }
}