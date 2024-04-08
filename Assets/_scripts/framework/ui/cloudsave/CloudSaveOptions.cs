//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework
{
    public class CloudSaveOptions : MonoBehaviour
    {
        [SerializeField] Button m_Confirm = default;
        [SerializeField] Button m_ToggleUseCloudSave = default;
        [SerializeField] TMP_InputField m_CodeInput = default;
        [SerializeField] TextMeshProUGUI m_CodeDisplay = default;
        [SerializeField] Color m_CloudSaveOn = Color.green;
        [SerializeField] Color m_CloudSaveOff = Color.red;
        [SerializeField] TextMeshProUGUI m_CloudSaveOnOffText = default;
        [SerializeField] Image m_CloudSaveIcon = default;
        [SerializeField] Sprite m_CloudSaveIconOn = default;
        [SerializeField] Sprite m_CloudSaveIconOff = default;
        CloudSaveScreen m_CloudSaveScreen;

        bool m_CloudSaveToggleOn = false;

        void UpdateToggleDisplay()
        {
            m_CloudSaveIcon.sprite = m_CloudSaveToggleOn ? m_CloudSaveIconOn : m_CloudSaveIconOff;
            m_CloudSaveOnOffText.text = m_CloudSaveToggleOn ? "CloudSave ist aktiv" : "CloudSave ist nicht aktiv";
            m_ToggleUseCloudSave.GetComponent<Image>().color = m_CloudSaveToggleOn ? m_CloudSaveOn : m_CloudSaveOff;
        }

        public void Show(CloudSaveScreen screen, string id)
        {
            m_CloudSaveToggleOn = SaveGame.instance.cloudSaveActive;
            m_CodeInput.text = SaveGame.instance.cloudSaveEncryption;
            m_CodeDisplay.text = "Deine ID ist: " + id;
            UpdateToggleDisplay();
            gameObject.SetActive(true);
            ActivateListeners();
            m_CloudSaveScreen = screen;
        }
        void Hide()
        {
            DeActivateListeners();
            m_CloudSaveScreen.DoneShowing();
        }


        private void ActivateListeners()
        {
            m_Confirm.onClick.AddListener(Confirm);
            m_ToggleUseCloudSave.onClick.AddListener(UseCloudSaveToggle);
        }

        private void DeActivateListeners()
        {
            m_Confirm.onClick.RemoveListener(Confirm);
            m_ToggleUseCloudSave.onClick.RemoveListener(UseCloudSaveToggle);
        }

        void UseCloudSaveToggle()
        {
            m_CloudSaveToggleOn = !m_CloudSaveToggleOn;
            UpdateToggleDisplay();
        }

        void Confirm()
        {
            m_CloudSaveScreen.SetOptionsResult(m_CloudSaveToggleOn, m_CodeInput.text);
            Hide();
        }
    }
}
