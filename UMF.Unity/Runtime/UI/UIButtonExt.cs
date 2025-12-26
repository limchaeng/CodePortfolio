//////////////////////////////////////////////////////////////////////////
//
// UIButtonExt
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
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UMF.Unity.UI
{
    [AddComponentMenu( "UMF/UI/UIButtonExt", 30 )]
    public class UIButtonExt : Button
    {
        public enum ePublicSelectionState
        {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled,
        }

        public bool m_SelectedStateEnable = false;
        public bool m_StateClearOnClicked = true;
        public UnityEvent<ePublicSelectionState> m_StateChangeEvent = new UnityEvent<ePublicSelectionState>();

        [System.Serializable]
        public class ColorChangeGraphic
        {
            public Graphic m_Target = null;
            public bool m_Disable = false;
            public Color m_NormalColor = Color.white;
            public Color m_HighlightedColor = Color.white;
            public Color m_PressedColor = Color.white;
            public Color m_SelectedColor = Color.white;
            public Color m_DisabledColor = Color.white;

            // use only editor
            [HideInInspector]
            [SerializeField]
            private bool mShowColorProperty = false;
        }

        public ColorChangeGraphic[] m_ColorChangeTargets = new ColorChangeGraphic[0];
        public Graphic[] m_ButtonColorApplyTargets = new Graphic[0];
        public bool m_UseBlinkColor = false;
        public Color m_BlinkColor = Color.white;

             
        ePublicSelectionState ConvertSelectionState( SelectionState state )
        {
            switch( state )
            {
                case SelectionState.Normal: return ePublicSelectionState.Normal;
                case SelectionState.Highlighted: return ePublicSelectionState.Highlighted;
                case SelectionState.Pressed: return ePublicSelectionState.Pressed;
                case SelectionState.Selected: return ePublicSelectionState.Selected;
                case SelectionState.Disabled: return ePublicSelectionState.Disabled;
                default:
                    return ePublicSelectionState.Normal;
            }
        }

        SelectionState ConvertSelectionState( ePublicSelectionState state )
        {
            switch( state )
            {
                case ePublicSelectionState.Normal: return SelectionState.Normal;
                case ePublicSelectionState.Highlighted: return SelectionState.Highlighted;
                case ePublicSelectionState.Pressed: return SelectionState.Pressed;
                case ePublicSelectionState.Selected: return SelectionState.Selected;
                case ePublicSelectionState.Disabled: return SelectionState.Disabled;
                default:
                    return SelectionState.Normal;
            }
        }

        public bool IgnoreStateChange { get; set; } = false;
        protected RectTransform mRT = null;
        public RectTransform rectTransform
        {
            get
            {
                if( mRT == null )
                    mRT = gameObject.GetComponent<RectTransform>();

                return mRT;
            }
        }

        protected override void Awake()
        {
            if( m_StateClearOnClicked )
            {
                onClick.AddListener( OnClicked );
            }

            if( m_SelectedStateEnable == false )
            {
                Navigation nav = navigation;
                nav.mode = Navigation.Mode.None;
                navigation = nav;
            }
        }

        private void OnClicked()
        {
            if( m_StateClearOnClicked )
                InstantClearState_Public();
        }

        public void CheckStateClearOnClicked()
        {
            if( m_StateClearOnClicked )
            {
                onClick.RemoveListener( OnClicked );
                onClick.AddListener( OnClicked );
            }
        }

        //------------------------------------------------------------------------
        public void DoStateTransition_Public( ePublicSelectionState p_state, bool instant )
        {
            SelectionState state = ConvertSelectionState( p_state );
            DoStateTransition( state, instant );
        }
        protected override void DoStateTransition( SelectionState state, bool instant )
        {
            if( IgnoreStateChange )
                return;

            base.DoStateTransition( state, instant );

            if( !gameObject.activeInHierarchy )
                return;

            if( Application.isPlaying )
            {
                if( m_ColorChangeTargets.Length > 0 )
                {
                    foreach( ColorChangeGraphic g in m_ColorChangeTargets )
                    {
                        if( g.m_Target == null || g.m_Disable )
                            continue;

                        Color t_color = g.m_Target.color;
                        switch( state )
                        {
                            case SelectionState.Normal: t_color = g.m_NormalColor; break;
                            case SelectionState.Highlighted: t_color = g.m_HighlightedColor; break;
                            case SelectionState.Pressed: t_color = g.m_PressedColor; break;
                            case SelectionState.Selected: t_color = g.m_SelectedColor; break;
                            case SelectionState.Disabled: t_color = g.m_DisabledColor; break;
                        }

                        g.m_Target.color = t_color;
                    }
                }

                m_StateChangeEvent.Invoke( ConvertSelectionState( state ) );

                if( m_ButtonColorApplyTargets.Length > 0 )
                {
                    foreach( Graphic g in m_ButtonColorApplyTargets )
                    {
                        Color t_color = g.color;
                        switch( state )
                        {
                            case SelectionState.Normal: t_color = colors.normalColor * colors.colorMultiplier; break;
                            case SelectionState.Highlighted: t_color = colors.highlightedColor * colors.colorMultiplier; break;
                            case SelectionState.Pressed: t_color = colors.pressedColor * colors.colorMultiplier; break;
                            case SelectionState.Selected: t_color = colors.selectedColor * colors.colorMultiplier; break;
                            case SelectionState.Disabled: t_color = colors.disabledColor * colors.colorMultiplier; break;
                        }

                        g.CrossFadeColor( t_color, instant ? 0f : colors.fadeDuration, true, true );
                    }
                }
            }
        }

        //------------------------------------------------------------------------
        public void InstantClearState_Public()
        {
            InstantClearState();
            if( m_SelectedStateEnable )
                EventSystem.current.SetSelectedGameObject( null );
        }

        protected override void InstantClearState()
        {
            if( IgnoreStateChange )
                return;

            base.InstantClearState();
        }

        //------------------------------------------------------------------------
        Coroutine _blank_routine = null;
        bool _blink_on = false;
        public void BeginBlink( float interval = 0.5f )
        {
            StopBlink();
            _blank_routine = StartCoroutine( DoBlink( interval ) );
        }

        public void StopBlink()
        {
            _blink_on = false;
            if( _blank_routine != null )
            {
                StopCoroutine( _blank_routine );
                _blank_routine = null;

                IgnoreStateChange = false;
                if( m_UseBlinkColor )
                    image.color = colors.normalColor;
                else
                    DoStateTransition_Public( ePublicSelectionState.Normal, true );
            }
        }

        IEnumerator DoBlink( float _interval )
        {
            float interval = _interval;
            float time = interval;
            while( true )
            {
                yield return null;
                time -= Time.deltaTime;
                if( time <= 0f )
                {
                    time = interval;

                    if( _blink_on )
                    {
                        IgnoreStateChange = false;
                        if( m_UseBlinkColor )
                            image.color = m_BlinkColor;
                        else
                            DoStateTransition_Public( ePublicSelectionState.Normal, true );
                    }
                    else
                    {
                        if( m_UseBlinkColor )
                            image.color = colors.normalColor;
                        else
                            DoStateTransition_Public( ePublicSelectionState.Highlighted, true );
                        IgnoreStateChange = true;
                    }

                    _blink_on = !_blink_on;
                }
            }
        }
    }
}