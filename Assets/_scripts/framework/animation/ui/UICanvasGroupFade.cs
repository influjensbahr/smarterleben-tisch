//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//


using Sparrow.Verification;
using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Animation
{
    public class UICanvasGroupFade : MonoBehaviour, IUIAnimationWithCallback<float>, IVerify
    {
        [SerializeField] CanvasGroup m_CanvasGroup = null;
        [SerializeField, WideToggle] bool m_DisableInteractionWhenInvisible = true;
        [SerializeField, WideToggle] bool m_Invert = false;
        [SerializeField] float m_TimeScale = 1f;


        UIAnimationStateLerper m_Lerper = new UIAnimationStateLerper();

        public void SetCanvasGroup(CanvasGroup canvasGroup)
        {
            m_CanvasGroup = canvasGroup;
        }

        public float RemainingTime()
        {
            return m_Lerper.remainingTime;
        }

        void Awake()
        {
            if (m_CanvasGroup == null) m_CanvasGroup = GetComponent<CanvasGroup>();

            m_Lerper.setState += (float t) =>
            {
                if (m_CanvasGroup == null) m_CanvasGroup = GetComponent<CanvasGroup>();
                if (m_CanvasGroup == null) return;

                // t = -1 and t = 1 are hidden, so alpha = 0
                // t = 0 is visible, so alpha = 1
                m_CanvasGroup.alpha = m_Invert ? Mathf.Abs(t) : (1f - Mathf.Abs(t));
                if (m_DisableInteractionWhenInvisible)
                {
                    m_CanvasGroup.interactable = m_CanvasGroup.alpha > 0.5f;
                    m_CanvasGroup.blocksRaycasts = m_CanvasGroup.alpha > 0.5f;
                }
            };

            ResetState();
        }

        public void ResetState()
        {
            m_Lerper.ResetState();
        }

        public void AnimateToState(float target, float time, float delay = 0f, UnityAction callback = null)
        {
            m_Lerper.AnimateToState(target, time * m_TimeScale, delay, callback);
        }

        public void SetInverted(bool inverted) 
        { 
            m_Lerper.SetInverted(inverted); 
        }


        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(GetComponent<CanvasGroup>() != null && m_CanvasGroup != null, "No CanvasGroup on GameObject or set.", gameObject);
        }
    }
}
