//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Gameplay;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.GraphicsBuffer;

#if OTBT_ARABIC
using EasyAlphabetArabic;
#endif

namespace OTBT.Framework.Localization
{
    /// <summary>
    /// Put this on a game object with a UI text to automagically translate it
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizeTextUI : MonoBehaviour, IVerify, ILocalizedTextLink
    {
        [HideInInspector]
        [SerializeField] TMP_Text m_TargetText;
        
        [SerializeField, WideToggle] bool m_AutoUpdateOnLanguageChange = true;
        [SerializeField, FormerlySerializedAs("m_TextGatherObject")] 
        LocalizedTextObject m_LocalizedTextObject = null;
        [HideInInspector]
        [SerializeField] bool m_SetAtRuntime = false; 

        RuntimeLocalizedText m_RuntimeLocalizedText = null;

        string[] args = null;
        public void SetEditorText(string text)
        {
            tmPro.text = text;
        }
        public string currentEditorText => tmPro.text;
        public TMP_Text tmPro => m_TargetText;
        public bool setAtRuntime => m_SetAtRuntime;
        public bool autoUpdate => m_AutoUpdateOnLanguageChange;
        public bool hasTextGatherObject => m_LocalizedTextObject != null || m_RuntimeLocalizedText != null;
        public LocalizedTextObject localizedText => m_LocalizedTextObject;
        public void SetLocalizedTextObject(LocalizedTextObject obj) => SetLocalizedText(obj, true);
        public string creationNote => "ui " + gameObject.scene.name;
        Language m_LocalLanguage;

         void OnValidate()
        {
            if(m_TargetText == null) m_TargetText = gameObject.GetComponent<TMP_Text>();
        }

        public void SetLocalizedText(RuntimeLocalizedText std, bool updateDisplay = true)
        {
            m_RuntimeLocalizedText = std;
            if (updateDisplay) UpdateDisplay(LocalizationDatabase.instance.currentLanguage);
        }

        public void SetLocalizedText(LocalizedTextObject std, bool updateDisplay = true)
        {
            m_LocalizedTextObject = std;
            if (updateDisplay) UpdateDisplay(LocalizationDatabase.instance.currentLanguage);
        }

        public void SetTextGatherObject(LocalizedTextObject std, bool updateDisplay = true)
        {
            SetLocalizedText(std, updateDisplay);
        }

        public void SetAtRuntime(bool setAtRuntime)
        {
            m_SetAtRuntime = setAtRuntime;
        }

        public void SetStringFormatArgs(string[] newArgs, bool updateDisplay = true)
        {
            args = newArgs;
            if (updateDisplay) UpdateDisplay(LocalizationDatabase.instance.currentLanguage);
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
            if (m_TargetText == null || (m_RuntimeLocalizedText == null && localizedText == null)) return;

            if (m_LocalLanguage != null) language = m_LocalLanguage;

            // if alignment is not center, change it!
            if (language.rightToLeft && (m_TargetText.alignment | TextAlignmentOptions.Left) == 0)
            {
                m_TargetText.alignment = (TextAlignmentOptions)AlignmentToLeftRight((int)m_TargetText.alignment, language.rightToLeft);
                m_TargetText.isRightToLeftText = language.rightToLeft;
            }

            // check if runtime localization has an override
            ILocalizedText locaSource = m_RuntimeLocalizedText == null ? localizedText : m_RuntimeLocalizedText;
            
            if (locaSource != null)
            {
                if (await RuntimeLocalization.instance.ContainsID(locaSource.textID, localizedText.forceSingleRuntimeUpdate))
                {
                    locaSource = await RuntimeLocalization.instance.GetLocalized(locaSource.textID, localizedText.forceSingleRuntimeUpdate);
                }
                string localizedString = await locaSource.GetTranslation(language);
                if (args != null && args.Length > 0)
                {
                    localizedString = string.Format(localizedString, args);
                }
                if (language.correctForArabic)
                {
                    // arabic display needs rich text to be enabled
                    if (!m_TargetText.richText) m_TargetText.richText = true;

#if OTBT_ARABIC                
                    m_TargetText.text = EasyArabicCore.CorrectTextMeshPro(localizedString);
#else
                    m_TargetText.text = localizedString;
                    Dbg.Log(this, "Localized string in arabic is used but the plugin to render it correctly does not exist.");
#endif
                }
                else
                {
                m_TargetText.text = localizedString;
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
            if (!m_SetAtRuntime)
            {
                checker.Check(localizedText != null, "LocalizeText has no linked target", this);
            }
        }
#if UNITY_EDITOR
        public void AttemptLocaSystemUpdate()
        {
            if(!hasTextGatherObject && localizedText != null)
            {
                m_LocalizedTextObject = LocalizationDatabase.instance.GetByID(localizedText.textID);
                EditorUtility.SetDirty(this);
            }
        }

        public void UpdateAllVoicedInformation() {}

        
#endif
    }
}