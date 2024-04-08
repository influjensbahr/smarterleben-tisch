// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Core;
using OTBT.Framework.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Animation
{
    /// <summary>
    /// Helper class to handle the lerping between -1, 0 and 1 for UI States
    /// </summary>
    public class UIAnimationStateLerper : IUpdateFrame
    {
        // animation states go from -1 to 0 (shown) for IN animations, and from 0 to 1 for OUT animations.
        // if inverted, this is inverted.
        public static float START = -1f;
        public static float SHOWN = 0f;
        public static float OUT = 1f;

        // we lerp from start to target, both within [-1,1], over the course
        // of fadeTime seconds
        float m_LerpStartValue = START;
        float m_LerpTargetValue = 0f;
        float m_LerpCurrentValue = 0f;

        float m_FadeTime = 0f;
        float m_TimePassed = 0f;
        bool m_Fading = false;
        float m_Delay = 0f;
        bool m_IsInverted = false;

        AnimationCurve m_AnimationCurve = null;

        public float remainingTime => (m_Delay < 0f ? Mathf.Abs(m_Delay) : 0f) + (m_FadeTime - m_TimePassed);

        public void SetAnimationCurve(AnimationCurve c)
        {
            m_AnimationCurve = c;
        }


        public UnityAction<float> setState = null;
        UnityAction m_Callback = null;

        public void SetInverted(bool i)
        {
            m_IsInverted = i;
        }

        public void ResetState()
        {
            m_LerpStartValue = m_IsInverted ? OUT : START;
            m_LerpTargetValue = SHOWN;
            m_LerpCurrentValue = m_LerpStartValue;

            m_FadeTime = 0f;
            m_TimePassed = 0f;
            m_Fading = false;

            setState?.Invoke(m_IsInverted ? OUT: START);
        }

        float GetAnimationCurveAdjusted(float t)
        {
            if (m_AnimationCurve == null) return t;

            if (t <= 0)
                return m_AnimationCurve.Evaluate(t + 1f) - 1f;

            return 1f - m_AnimationCurve.Evaluate(1f - t);
        }

        public void AnimateToState(float target, float time, float delay = 0f, UnityAction callback = null)
        {
            if (time <= 0f && delay == 0f)
            {
                m_LerpCurrentValue = GetAnimationCurveAdjusted(target);
                setState?.Invoke(m_LerpCurrentValue);
                m_Delay = 0f;
                m_Fading = false;
                m_LerpCurrentValue = GetAnimationCurveAdjusted(target);
                callback?.Invoke();
                return;
            }

            m_Delay = delay;
            m_LerpStartValue = m_LerpCurrentValue;
            m_LerpTargetValue = m_IsInverted ? -target : target;
            m_FadeTime = time;
            m_TimePassed = 0f;
            m_Callback = callback;
            m_Fading = true;
            EventManager.instance.StartListening(this);
        }

        public void UpdateFrame(float timePassed)
        {
            float remainingFrameTime = timePassed;
            if (m_Delay < 0f)
            {
                remainingFrameTime -= Mathf.Abs(m_Delay);
                m_Delay += timePassed;
                if(remainingFrameTime <= 0f) return;
            }
            if (!m_Fading)
            {
                EventManager.instance.StopListening(this);
                return;
            }

            m_TimePassed += remainingFrameTime;
            float fadePercent = m_TimePassed / m_FadeTime;
            m_LerpCurrentValue = GetAnimationCurveAdjusted(Mathf.Lerp(m_LerpStartValue, m_LerpTargetValue, fadePercent));
            setState?.Invoke(m_LerpCurrentValue);

            if (fadePercent >= 1f)
            {
                m_Fading = false;
                m_Callback?.Invoke();
            }
        }
    }
}