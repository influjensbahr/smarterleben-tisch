// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 

using OTBT.Framework.Utils;
using Sparrow.Verification;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class AdjustFontSize : MonoBehaviour, IVerify, IExtendDefaultEditor
    {
        [SerializeField] FontType m_FontType;
        [SerializeField] TMP_Text m_Text;
        [SerializeField] float m_FontSizeMultiplier = 1f;
        [SerializeField, WideToggle] bool m_ScalesWithGameSetting = true;
        [SerializeField, WideToggle] bool m_IgnoreSize = true;
        [SerializeField, WideToggle] bool m_IgnoreFont = true;
        [SerializeField, WideToggle] bool m_IgnoreWeight = true;

        Vector2 m_BaseFontSize;

        void OnValidate()
        {
            if (m_Text == null) m_Text = GetComponent<TMP_Text>();
        }

        void Start()
        {
            if (!CheckForText()) return;
            if (m_FontType != null)
            {
                if(!m_IgnoreFont && m_FontType.doFont)
                    m_Text.font = m_FontType.font;
                m_BaseFontSize = m_Text.enableAutoSizing ?
                        new Vector2(m_FontType.autoSizeMin, m_FontType.autoSizeMax) :
                        new Vector2(m_FontType.defaultSize, m_FontType.defaultSize);
            } else
            {
                m_BaseFontSize = m_Text.enableAutoSizing ?
                        new Vector2(m_Text.fontSizeMin, m_Text.fontSizeMax) :
                        new Vector2(m_Text.fontSize, m_Text.fontSize);
            }

            OnFontSizeUpdate(FontTypeManager.instance.GetMultiplier());
            if(m_ScalesWithGameSetting)
                FontTypeManager.instance.onSettingChange += OnFontSizeUpdate;
        }

        void OnDestroy()
        {
            if (m_ScalesWithGameSetting)
                FontTypeManager.instance.onSettingChange -= OnFontSizeUpdate;
        }

        public void UpdateFontNow()
        {
            if (!CheckForText()) return;
            if (m_FontType != null)
            {
                if(!m_IgnoreFont && m_FontType.doFont)
                    m_Text.font = m_FontType.font;
                if (!m_IgnoreSize && m_FontType.doSize) { 
                    m_Text.fontSize = m_FontType.defaultSize * m_FontSizeMultiplier;
                    m_Text.fontSizeMin = m_FontType.autoSizeMin * m_FontSizeMultiplier;
                    m_Text.fontSizeMax = m_FontType.autoSizeMax * m_FontSizeMultiplier;
                }
                if(!m_IgnoreWeight && m_FontType.doWeight)
                    m_Text.fontWeight = m_FontType.fontWeight;
            }
        }

        private bool CheckForText()
        {
            if (m_Text == null)
            {
                m_Text = GetComponent<TMP_Text>();
            }
            return m_Text != null;
        }

        public void OnFontSizeUpdate(float multiplier)
        {
            if (!CheckForText()) return;
            if (!m_IgnoreSize)
            {
                if (m_Text.enableAutoSizing)
                {
                    m_Text.fontSizeMin = m_BaseFontSize.x * (m_ScalesWithGameSetting ? multiplier : 1f);
                    m_Text.fontSizeMax = m_BaseFontSize.y * (m_ScalesWithGameSetting ? multiplier : 1f);
                }
                else
                {
                    m_Text.fontSize = m_BaseFontSize.x * (m_ScalesWithGameSetting ? multiplier : 1f);
                }
            }
            if (!m_IgnoreWeight && m_FontType != null)
                m_Text.fontWeight = m_FontType.fontWeight;
            Canvas.ForceUpdateCanvases();
        }
        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_Text != null, "AdjustFontSize misses Textfield", gameObject);
            checker.Check(m_FontType != null || (m_IgnoreFont && m_IgnoreSize && m_IgnoreWeight), "AdjustFontSizer misses fonttype", gameObject);
        }

#if UNITY_EDITOR

        public void ExtendDefaultEditor()
        {
            if (GUILayout.Button("Redraw now"))
            {
                UpdateFontNow();
                EditorUtility.SetDirty(gameObject);
            }
        }
#endif
    }
}