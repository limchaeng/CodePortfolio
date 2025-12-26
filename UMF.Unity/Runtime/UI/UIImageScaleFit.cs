//////////////////////////////////////////////////////////////////////////
//
// UIImageScaleFit
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

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UMF.Unity.UI
{
    [RequireComponent( typeof( Image ) )]
    public class UIImageScaleFit : MonoBehaviour
    {
        public RectTransform m_Bounds;

        Image mImage;
        Image GetImage 
        { 
            get
            {
                if( mImage == null )
                    mImage = gameObject.GetComponent<Image>();

                return mImage;
            }
        }




        private void Awake()
        {
            mImage = gameObject.GetComponent<Image>();
        }

        public void SetSprite( Sprite sprite )
        {
            GetImage.sprite = sprite;
            GetImage.SetNativeSize();

            UpdateScale();
        }

        public void UpdateScale()
        {
            Vector2 size = GetImage.rectTransform.sizeDelta;
            size *= GetImage.canvas.referencePixelsPerUnit;
            GetImage.rectTransform.sizeDelta = size;

            if( m_Bounds != null )
            {
                Vector3 scale = Vector3.one;
                if( GetImage.rectTransform.sizeDelta.y > m_Bounds.rect.height )
                    scale.y = m_Bounds.rect.height / GetImage.rectTransform.sizeDelta.x;

                if( GetImage.rectTransform.sizeDelta.x > m_Bounds.rect.width )
                    scale.x = m_Bounds.rect.width / GetImage.rectTransform.sizeDelta.x;

                Vector3 vscale = GetImage.rectTransform.localScale;
                vscale.x = vscale.y = Mathf.Min( scale.x, scale.y );
                GetImage.rectTransform.localScale = vscale;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof( UIImageScaleFit ) )]
    public class UIImageScaleFitInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            UIImageScaleFit comp = target as UIImageScaleFit;

            if( GUILayout.Button( "Apply" ) )
            {
                comp.UpdateScale();
            }

            base.OnInspectorGUI();
        }
    }
#endif

}
