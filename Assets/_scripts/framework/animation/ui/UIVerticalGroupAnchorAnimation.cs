// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Animation
{
    public class UIVerticalGroupAnchorAnimation : MonoBehaviour, IUIAnimationWithCallback<Transform>
    {
        [Header("Own anchors")]
        [SerializeField] Transform m_BottomAnchor = default;

        [Header("Target anchor")]
        [SerializeField] Transform m_TargetAnchor = default;
        Transform m_OldAnchor = default;
        float m_Lerper = 0f;
        float m_Timescale = 1f;
        float m_Delay = 0f;
        bool m_IsInverted = false;
        public void SetInverted(bool inverted) { m_IsInverted = inverted; }

        private void Awake()
        {
            ResetState();
        }

        public float RemainingTime()
        {
            return (m_Delay < 0f ? Mathf.Abs(m_Delay) : 0f) + (1f - m_Lerper) * m_Timescale;
        }


        public void AnimateToState(Transform target, float time, float delay = 0f, UnityAction callback = null)
        {
            m_Delay = - delay;
            m_OldAnchor = m_TargetAnchor;
            m_TargetAnchor = target;
            m_Lerper = 0f;
            m_Timescale = 1f / time;
        }

        public void ResetState()
        {
            m_Delay = 0f;
            m_Lerper = 1f;
            m_OldAnchor = null;
            //m_TargetAnchor = null;
            m_Timescale = 1f;
        }

        private void Update()
        {
            if(m_Delay < 0f)
            {
                m_Delay += Time.deltaTime;
                return;
            }
            if (m_Lerper < 1f)
                m_Lerper += Time.deltaTime * m_Timescale;
            if(m_TargetAnchor != null && m_BottomAnchor != null)
            {
                transform.position += ((m_OldAnchor == null ? m_TargetAnchor.position : Vector3.Lerp(m_OldAnchor.position, m_TargetAnchor.position, Mathf.SmoothStep(0.0f, 1.0f, m_Lerper))) - m_BottomAnchor.position);
            }
        }

       
    }
}
