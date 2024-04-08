//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using Sparrow.Verification;
using UnityEngine;

namespace OTBT.Framework.Animation
{
    [RequireComponent(typeof(RectTransform))]
    public class UIEndlessRandomRotate : MonoBehaviour, IVerify
    {
        [SerializeField] RectTransform m_RectTransform = null;
        [SerializeField] AnimationCurve m_AnimationCurve = new AnimationCurve();

        float m_Lerper = 0f;
        float m_TimeScale = 1f;
        float m_Degrees = 0f;

        private void OnValidate()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            NewTarget();
        }

        void Update()
        {
            float oldLerper = m_Lerper;
            m_Lerper += Time.deltaTime * m_TimeScale;
            // implemented to make sure it always rotates clockwise - Quaternion.Lerp etc would not do that
            m_RectTransform.rotation *= Quaternion.Euler(0f, 0f, - m_Degrees * (m_AnimationCurve.Evaluate(m_Lerper) - m_AnimationCurve.Evaluate(oldLerper)));
            if (m_Lerper > 1f) NewTarget();
        }

        void NewTarget()
        {
            m_Degrees = Random.Range(120f, 240f);
            m_Lerper = 0f;
            m_TimeScale = Random.Range(0.8f, 1.2f);
        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_RectTransform != null && GetComponent<RectTransform>() != null, "No Recttransform on Gameobject or set.", gameObject);
        }
    }
}
