// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 


using OTBT.Framework.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OTBT.Framework.Mobile
{

    public class MobileSimulatorAspectRatioUpdater : MonoBehaviour, IExtendDefaultEditor
    {
        [SerializeField] AspectRatioFitter m_AspectRatio = default;

        public event UnityAction<float> onAspectRatioChange;

        float m_OriginalAspect = 1f;
        int m_OriginalWidth = -1;
        int m_OriginalHeight = -1;

#if UNITY_EDITOR
        public void ExtendDefaultEditor()
        {

            float newAspect = (float)m_OriginalWidth / (float)m_OriginalHeight;
            EditorGUILayout.LabelField(newAspect + " // " + m_OriginalAspect);
        }
#endif

        private void OnValidate()
        {
            if (m_AspectRatio == null) m_AspectRatio = GetComponent<AspectRatioFitter>();
        }

        private void Start()
        {
            m_OriginalAspect = m_AspectRatio.aspectRatio;
        }

        void Update()
        {
            if (Screen.width == m_OriginalWidth && Screen.height == m_OriginalHeight) return;

            m_OriginalHeight = Screen.height;
            m_OriginalWidth = Screen.width;

            float newAspect = (float)m_OriginalWidth / (float)m_OriginalHeight;
            m_AspectRatio.aspectRatio = Mathf.Min(newAspect, m_OriginalAspect);
            onAspectRatioChange?.Invoke(newAspect);
        }


    }
}