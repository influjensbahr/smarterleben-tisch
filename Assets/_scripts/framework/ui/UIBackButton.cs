// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 

using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.UI
{
    [AddComponentMenu("OTBT/Atoms/UI Back Button")]
    [RequireComponent(typeof(Button))]
    public class UIBackButton : MonoBehaviour
    {
        [SerializeField] Button m_Button;

        void OnValidate()
        {
            if (m_Button == null) m_Button = GetComponent<Button>();
        }

        void OnEnable() => m_Button.onClick.AddListener(Hide);
        void OnDisable() => m_Button.onClick.RemoveListener(Hide);

        public void Hide() => UIScreenController.instance.HideCurrent();
    }
}
