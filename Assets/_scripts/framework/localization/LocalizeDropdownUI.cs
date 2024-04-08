//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//


using OTBT.Framework.Utils;
using Sparrow.Verification;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if OTBT_ARABIC
using EasyAlphabetArabic;
#endif

namespace OTBT.Framework.Localization
{
    /// <summary>
    /// Put this on a game object with a dropdown to translate it
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class LocalizeDropdownUI : MonoBehaviour, IVerify
    {
        [SerializeField] TMP_Dropdown m_TargetDropdown = default;
        [SerializeField] TextMeshProUGUI m_Label = default;
        [SerializeField] TextMeshProUGUI m_Captions = default;
        [SerializeField, WideToggle] bool m_AutoUpdateOnLanguageChange = true;
        [SerializeField] List<LocalizedTextObject> m_LocalizedTextObject = null;
        
        public TMP_Dropdown tmPro => m_TargetDropdown;
        public bool autoUpdate => m_AutoUpdateOnLanguageChange;

        Language m_LocalLanguage;

         void OnValidate()
        {
            if(m_TargetDropdown == null) m_TargetDropdown = gameObject.GetComponent<TMP_Dropdown>();
        }

        int AlignmentToLeftRight(int oldAlign, bool toRight)
        {
            // strip out all right or left
            int newAlign = oldAlign;

            // if center, to nothing
            if ((newAlign & ((int)HorizontalAlignmentOptions.Center)) > 0) return newAlign;

            if (toRight) {
                // set these bits to 0
                newAlign &= ~((int)HorizontalAlignmentOptions.Left);
                newAlign &= ~((int)HorizontalAlignmentOptions.Justified);

                // set this to 1
                newAlign |= ((int)HorizontalAlignmentOptions.Right);
            } else
            {
                // set these bits to 0
                newAlign &= ~((int)HorizontalAlignmentOptions.Right);

                // set this to 1, if not already justified or left
                if((newAlign & (((int)HorizontalAlignmentOptions.Left) | ((int)HorizontalAlignmentOptions.Justified))) == 0)
                {
                    newAlign |= ((int)HorizontalAlignmentOptions.Left);
                }
            }
            
            return newAlign;
        }

        public void SetAutoUpdate(bool m)
        {
            m_AutoUpdateOnLanguageChange = m;
        }

        /// <summary>
        /// Set this object to use a different language than the global language used. The only use-case for this is showing language selection with localized language names.
        /// </summary>
        /// <param name="language">The language to display texts in.</param>
        public void SetLocalLanguage(Language language)
        {
            m_LocalLanguage = language;
        }

        public async void UpdateDisplay(Language language)
        {
            if (m_LocalizedTextObject.Count != m_TargetDropdown.options.Count) return;

            // arabic display needs rich text to be enabled
            if (!m_Label.richText) m_Label.richText = true;
            m_Label.alignment = (TextAlignmentOptions)AlignmentToLeftRight((int)m_Label.alignment, language.rightToLeft);
            m_Label.isRightToLeftText = language.rightToLeft;
            if (!m_Captions.richText) m_Captions.richText = true;
            m_Captions.alignment = (TextAlignmentOptions)AlignmentToLeftRight((int)m_Captions.alignment, language.rightToLeft);
            m_Captions.isRightToLeftText = language.rightToLeft;

            for (int i = 0; i < m_LocalizedTextObject.Count; i++)
            {
                string localizedString = await m_LocalizedTextObject[i].GetTranslation(language);
                if (language.correctForArabic)
                {
#if OTBT_ARABIC                
                    m_TargetDropdown.options[i].text = EasyArabicCore.CorrectTextMeshPro(localizedString);
#else
                    m_TargetDropdown.options[i].text = localizedString;
                    Dbg.Log(this, "Localized string in arabic is used but the plugin to render it correctly does not exist.");
#endif
                }
                else
                {
                    m_TargetDropdown.options[i].text = localizedString;
                }
            }
        }

         void Start()
        {
            if (m_AutoUpdateOnLanguageChange)
            {
                LocalizationDatabase.instance.onLanguageChange += (lang) =>
                {
                    UpdateDisplay(lang);
                };

                UpdateDisplay(LocalizationDatabase.instance.currentLanguage);
            }
        }
        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_LocalizedTextObject.Count == m_TargetDropdown.options.Count, "Localize options count must equal count of options in drowndown", this);
        }
    }
}