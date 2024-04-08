// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
//

using OTBT.Framework.Core;
using Sparrow.Verification;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.UI
{
    [RequireComponent(typeof(Slider))]
    public class ZeroToTenSettingSlider : MonoBehaviour, IVerify
    {
        [SerializeField, VRequired] Slider m_Slider;
        [SerializeField, VRequired] GameSetting m_Setting;

        void OnValidate()
        {
            if (m_Slider == null) m_Slider = GetComponent<Slider>();

            m_Slider.minValue = 0f;
            m_Slider.maxValue = 10f;
            m_Slider.wholeNumbers = true;
        }

        void Start()
        {
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            m_Slider.value = m_Setting.GetIntValue();
        }

        void OnEnable()
        {
            m_Slider.onValueChanged.AddListener(SetValue);
            m_Slider.value = m_Setting.GetIntValue();
        }

        void OnDisable()
        {
            m_Slider.onValueChanged.RemoveListener(SetValue);
        }

        void SetValue(float value) => m_Setting.SetIntValue(Mathf.RoundToInt(value));


        public void Verify(CheckVerifyInterface checker)
        {
        }
    }
}
