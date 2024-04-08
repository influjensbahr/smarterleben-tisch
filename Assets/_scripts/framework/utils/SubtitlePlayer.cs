// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using System;
using Sparrow.Verification;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.Utils
{
    public class SubtitlePlayer : MonoBehaviour, IVerify
    {
        [SerializeField] TMP_Text m_TextDisplay;

        Subtitles m_Subtitles;
        Subtitles.Line m_CurrentLine;

        void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Initialize(string subtitles)
        {
            var parser = new SubtitleParserSRT();
            m_Subtitles = parser.ParseString(subtitles);
        }

        public void Show(float time)
        {
            var line = m_Subtitles.GetLine(time);

            if (!line.Equals(m_CurrentLine)) //empty frame (good practice for subtitles)
            {
                gameObject.SetActive(false);
                m_CurrentLine = line;
                m_TextDisplay.text = m_CurrentLine.text;
                return;
            }

            if (gameObject.activeSelf) return;

            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void Verify(CheckVerifyInterface verify)
        {
            verify.CheckNotNull(m_TextDisplay, nameof(m_TextDisplay), this);
        }
    }
}
