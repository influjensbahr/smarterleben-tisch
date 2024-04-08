//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using DG.Tweening;
using OTBT.Framework.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework
{
    public class CloudSaveDecryptionFailed : MonoBehaviour
    {
        [SerializeField] Button m_TryDecrypt = default;
        [SerializeField] Button m_UseLocalButton = default;
        [SerializeField] TMP_InputField m_CodeInput = default;
        [SerializeField] GameObject m_WrongPassword = default;

        CloudSaveScreen m_CloudSaveScreen;
        string m_CurrentTestString = "";

        public void Show(CloudSaveScreen screen, string testString)
        {
            m_CurrentTestString = testString;
            gameObject.SetActive(true);
            m_CodeInput.text = SaveGame.instance.cloudSaveEncryption;
            m_WrongPassword.SetActive(false);
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
            m_TryDecrypt.onClick.AddListener(TryDecrypt);
            m_UseLocalButton.onClick.AddListener(UseLocal);
        }

        private void DeActivateListeners()
        {
            m_TryDecrypt.onClick.RemoveListener(TryDecrypt);
            m_UseLocalButton.onClick.RemoveListener(UseLocal);
        }

        void UseLocal()
        {
            m_CloudSaveScreen.SetDecryptionResult(true, m_CodeInput.text);
            Hide();
        }

        void TryDecrypt()
        {
            if(SaveGame.CheckDecryption(m_CurrentTestString, m_CodeInput.text))
            {
                m_CloudSaveScreen.SetDecryptionResult(false, m_CodeInput.text);
                Hide();
            } else
            {
                m_WrongPassword.SetActive(true);
                m_WrongPassword.transform.DOPunchPosition(new Vector3(4f, 0f, 0), 1f, 10, .5f);
            }
        }
    }
}
