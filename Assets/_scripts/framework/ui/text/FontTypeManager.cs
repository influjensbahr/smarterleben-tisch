
// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 


using System;
using System.Collections.Generic;
using OTBT.Framework.Core;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.UI
{
    public class FontTypeManager : Singleton<FontTypeManager>, IVerify
    {
        [SerializeField] List<FontType> m_FontTypes = new List<FontType>();

        [Header("Font size scaling")]
        [SerializeField] GameSetting m_FontSizeSetting;
        [SerializeField] Vector2 m_FontSizeMultiplier = new Vector2(1f, 1.5f);

        public List<FontType> fontTypes => m_FontTypes;
        public event Action<float> onSettingChange; //no int argument because the setting is only relevant for the manager
        public GameSetting fontSetting => m_FontSizeSetting;
        bool m_ListenersSetup = false;

        protected override void OnDestroy()
        {
            onSettingChange = null;
            m_FontSizeSetting.ClearEvents();
            base.OnDestroy();
        }

        void Start()
        {
            if(m_FontSizeSetting != null)
                m_FontSizeSetting.AddValueChangeAndExecute(NotifyListeners);
            m_ListenersSetup = true;
        }

        void NotifyListeners(int value)
        {
            onSettingChange?.Invoke(GetMultiplier());
            LayoutRebuilder.ForceRebuildLayoutImmediate(UIScreenController.instance.gameObject.transform as RectTransform);
        }

        public void SetMultiplier(float f)
        {
            if (m_FontSizeSetting == null) return;
            m_FontSizeSetting.SetIntValue((int)(f * 10f));
            if (!m_ListenersSetup)
                NotifyListeners(m_FontSizeSetting.GetIntValue());
        }

        public float GetMultiplier()
        {
            if (m_FontSizeSetting == null) return 1f;
            var settingNormalized = (float) (m_FontSizeSetting.GetIntValue()) / 10f;
            return Mathf.Lerp(m_FontSizeMultiplier.x, m_FontSizeMultiplier.y, settingNormalized);
        }


        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(gameObject.GetComponent<TextMeshProUGUI>() == null, "FontTypeManager should not be added to individual text components", this);
        }

    }
}
