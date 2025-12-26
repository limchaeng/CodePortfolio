//////////////////////////////////////////////////////////////////////////
//
// UIExtensions
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

namespace UMF.Unity
{
    public static class UIExtensions
    {
        public class LayoutSizeFitterData
        {
            List<LayoutGroup> layout_groups = null;
            List<ContentSizeFitter> fitters = null;

            MonoBehaviour mMono = null;

            public LayoutSizeFitterData( MonoBehaviour mono )
            {
                mMono = mono;
            }

            public void AddLayout( LayoutGroup lg )
            {
                if( layout_groups == null )
                    layout_groups = new List<LayoutGroup>();

                layout_groups.Add( lg );
                ContentSizeFitter ft = lg.gameObject.GetComponent<ContentSizeFitter>();
                if( ft != null )
                {
                    if( fitters == null )
                        fitters = new List<ContentSizeFitter>();

                    fitters.Add( ft );
                }
            }

            public void Enable()
            {
                if( layout_groups == null )
                    return;

                layout_groups.ForEach( a => a.enabled = true );

                if( fitters != null )
                    fitters.ForEach( a => a.enabled = true );
            }
            public void Disable( bool immediate = false )
            {
                if( layout_groups == null )
                    return;

                if( immediate || mMono == null )
                {
                    _Disable();
                }
                else
                {
                    mMono.StartCoroutine( DelayedDisable() );
                }
            }

            IEnumerator DelayedDisable()
            {
                yield return new WaitForEndOfFrame();
                _Disable();
            }

            void _Disable()
            {
                if( fitters != null )
                    fitters.ForEach( a => a.enabled = false );

                layout_groups.ForEach( a => a.enabled = false );
            }

            public void OnDestroy()
            {
                if( layout_groups != null )
                    layout_groups.Clear();
                layout_groups = null;

                if( fitters != null )
                    fitters.Clear();
                fitters = null;
            }
        }

        //------------------------------------------------------------------------
        public static void SetAlphaExt( this Image image, float alpha )
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
        public static void SetAlphaExt( this Graphic graphic, float alpha )
        {
            Color c = graphic.color;
            c.a = alpha;
            graphic.color = c;
        }

        //------------------------------------------------------------------------
        public static void LayoutRebuild( this LayoutGroup layout )
        {
            RectTransform rt = layout.GetComponent<RectTransform>();
            if( rt != null )
                LayoutRebuilder.ForceRebuildLayoutImmediate( rt );
        }
        public static void LayoutRebuild( this RectTransform rect_t )
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate( rect_t );
        }

        //------------------------------------------------------------------------
        public static void LayoutGroupScaling( this LayoutGroup layout, RectTransform bounds_rect )
        {
            layout.LayoutGroupScaling( bounds_rect, Vector2.one );
        }

        public static void LayoutGroupScaling( this LayoutGroup layout, RectTransform bounds_rect, Vector2 def_scale )
        {
            RectTransform rt = layout.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate( rt );

            Vector2 _scale = def_scale;
            if( layout.preferredHeight > bounds_rect.rect.height )
                _scale.y = bounds_rect.rect.height / layout.preferredHeight;

            if( layout.preferredWidth > bounds_rect.rect.width )
                _scale.x = bounds_rect.rect.width / layout.preferredWidth;

            Vector3 vscale = rt.localScale;
            vscale.x = vscale.y = Mathf.Min( _scale.x, _scale.y );
            rt.localScale = vscale;
        }

        public static void KeepInsideParentRect( RectTransform rt, RectTransform parent )
        {
            Vector2 parentSize = parent.rect.size;
            Vector2 imgSize = rt.rect.size * rt.localScale; // 스케일 적용

            Vector2 pos = rt.anchoredPosition;

            // 부모 영역 경계 계산
            float minX = -parentSize.x / 2 + imgSize.x / 2;
            float maxX = parentSize.x / 2 - imgSize.x / 2;
            float minY = -parentSize.y / 2 + imgSize.y / 2;
            float maxY = parentSize.y / 2 - imgSize.y / 2;

            // 좌표 제한
            pos.x = Mathf.Clamp( pos.x, minX, maxX );
            pos.y = Mathf.Clamp( pos.y, minY, maxY );

            rt.anchoredPosition = pos;
        }

        //------------------------------------------------------------------------
        public static void SetNativeSizePixelPerUnit( this Image img )
        {
            img.SetNativeSize();
            Vector2 size = img.rectTransform.sizeDelta;
            size *= img.canvas.referencePixelsPerUnit;
            img.rectTransform.sizeDelta = size;
        }
    }
}