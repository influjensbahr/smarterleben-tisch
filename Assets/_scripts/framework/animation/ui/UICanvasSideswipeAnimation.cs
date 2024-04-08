//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.UI;
using Sparrow.Verification;
using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Animation
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UICanvasSideswipeAnimation : MonoBehaviour, IUIAnimationWithCallback<float>, IVerify
    {
        [SerializeField] CanvasGroup m_CanvasGroup = null;
        [SerializeField] bool m_DisableInteractionWhenInvisible = true;


        UIAnimationStateLerper m_Lerper = new UIAnimationStateLerper();

        public void SetInverted(bool inverted) { m_Lerper.SetInverted(inverted); }

        [SerializeField] bool m_ActivateFading = true;
        [SerializeField] RectTransform m_RectTransform;
        [SerializeField] float m_MaxOffsetX = 750f;
        [SerializeField] float m_MaxOffsetY = 0f;

        [SerializeField] AnimationCurve m_AnimationCurve = null;

        Vector2 m_OffsetStart = Vector2.zero;
        Vector2 m_OffsetShown = Vector2.zero;
        Vector2 m_OffsetOut = Vector2.zero;

        public float RemainingTime()
        {
            return m_Lerper.remainingTime;
        }


        private void OnValidate()
        {
            if (m_CanvasGroup == null) m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
            UIScreen uiScreen = GetComponent<UIScreen>();
            if(uiScreen != null)
            {
                uiScreen.SetAnimation(this);
            }
        }

        void Awake()
        {
            if (m_CanvasGroup == null) m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();

            m_OffsetStart = new Vector2(m_MaxOffsetX, m_MaxOffsetY);
            m_OffsetOut = new Vector2(-m_MaxOffsetX, -m_MaxOffsetY);

            m_Lerper.SetAnimationCurve(m_AnimationCurve);

            m_Lerper.setState += (float t) =>
            {
                if (m_CanvasGroup == null) m_CanvasGroup = GetComponent<CanvasGroup>();
                if (m_CanvasGroup != null && m_ActivateFading)
                {
                    // t = -1 and t = 1 are hidden, so alpha = 0
                    // t = 0 is visible, so alpha = 1
                    m_CanvasGroup.alpha = 1f - Mathf.Abs(t);
                    if (m_DisableInteractionWhenInvisible) m_CanvasGroup.interactable = m_CanvasGroup.alpha > 0.5f;
                }

                if(m_RectTransform != null)
                {
                    if(t <= 0)
                    {
                        m_RectTransform.offsetMin = Vector2.Lerp(m_OffsetStart, m_OffsetShown, t + 1f);
                        m_RectTransform.offsetMax = Vector2.Lerp(m_OffsetStart, m_OffsetShown, t + 1f);
                    } else
                    {
                        m_RectTransform.offsetMin = Vector2.Lerp(m_OffsetShown, m_OffsetOut, t);
                        m_RectTransform.offsetMax = Vector2.Lerp(m_OffsetShown, m_OffsetOut, t);
                    }
                }
            };
        }

        public void ResetState()
        {
            m_Lerper.ResetState();
        }

        public void AnimateToState(float target, float time, float delay = 0f, UnityAction callback = null)
        {
            m_Lerper.AnimateToState(target, time, delay, callback);
        }
        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_AnimationCurve!= null, "Animation Curve is not set!", this);
        }
    }
}
