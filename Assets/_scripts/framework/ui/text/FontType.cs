// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 


using OTBT.Framework.Localization;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.UI
{
    [CreateAssetMenu(menuName = "OTBT/UI/Font Type", fileName = "New Font Type")]
    [Serializable]
    public class FontType : ScriptableObject, IVerify, IExtendDefaultEditor
    {
        public TMP_FontAsset font => m_LatinFont;
        public float autoSizeMin => m_AutoSizeMin;
        public float autoSizeMax => m_AutoSizeMax;
        public float defaultSize => m_DefaultSize;
        public FontWeight fontWeight => m_FontWeight;


        [SerializeField, WideToggle] bool m_ActivateFontSizeSetting = true;
        [SerializeField] float m_DefaultSize = 40;
        [SerializeField] float m_AutoSizeMin = 40;
        [SerializeField] float m_AutoSizeMax = 65;


        [SerializeField, WideToggle] bool m_ActivateFontWeightSetting = true;
        [SerializeField] FontWeight m_FontWeight = FontWeight.Regular;


        [SerializeField, WideToggle] bool m_ActivateFontAssetSetting = true;
        [SerializeField] TMP_FontAsset m_LatinFont;

        public bool doFont => m_ActivateFontAssetSetting;
        public bool doSize => m_ActivateFontSizeSetting;
        public bool doWeight => m_ActivateFontWeightSetting;

        public TMP_FontAsset GetFont() {
            return m_LatinFont;
        }
        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_LatinFont != null, "FontType Object misses Font", this);
        }
#if UNITY_EDITOR

        public void ExtendDefaultEditor()
        {
            EditorGUILayout.LabelField("This Font weight is equal to " + (int)fontWeight);
        }
#endif

    }
}