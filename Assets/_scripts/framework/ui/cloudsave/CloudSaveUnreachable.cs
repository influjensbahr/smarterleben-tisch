//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework
{
    public class CloudSaveUnreachable : MonoBehaviour
    {
        [SerializeField] Button m_DiscardButton = default;
        CloudSaveScreen m_CloudSaveScreen;


        public void Show(CloudSaveScreen screen)
        {
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
            m_DiscardButton.onClick.AddListener(DiscardClick);
        }

        private void DeActivateListeners()
        {
            m_DiscardButton.onClick.RemoveListener(DiscardClick);
        }

        void DiscardClick()
        {
            Hide();
        }
    }
}
