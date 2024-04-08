// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.UI
{
    public class UIButtonBlockWhileLoading : MonoBehaviour
    {
        [SerializeField] GameObject m_IconRegular = default;
        [SerializeField] GameObject m_IconLoading = default;
        [SerializeField] Button m_ButtonObject = default;
        [SerializeField] CanvasGroup m_CanvasGroup = default;

        private void Start()
        {
            SetRegular();
        }

        public void SetLoading(bool blockInteraction)
        {
            if (blockInteraction && m_ButtonObject != null)
                m_ButtonObject.interactable = false;
            if (m_CanvasGroup != null) m_CanvasGroup.alpha = 0.5f;
            m_IconRegular.SetActive(false);
            m_IconLoading.SetActive(true);
        }

        public void SetRegular()
        {
            if (m_ButtonObject != null)
                m_ButtonObject.interactable = true;
            if (m_CanvasGroup != null) m_CanvasGroup.alpha = 1f;
            m_IconRegular.SetActive(true);
            m_IconLoading.SetActive(false);
        }
    }
}
