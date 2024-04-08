//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer:Alex Brühl
//

#if OTBT_INK

using System.Collections.Generic;
using DG.Tweening;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.Ink
{
    public class InkpieceDisplay : MonoBehaviour
    {
        [SerializeField] TMP_Text m_StoryText;
        [SerializeField] List<Button> m_ChoiceButtons;
        List<TMP_Text> m_ButtonLabels;
        private InkpieceController m_InkController;

        void Awake()
        {
            m_ButtonLabels = new List<TMP_Text>(m_ChoiceButtons.Count);
            for (int i = 0; i < m_ChoiceButtons.Count; i++)
            {
                var button = m_ChoiceButtons[i];
                m_ButtonLabels.Add(button.GetComponentInChildren<TMP_Text>());
            }

            DisableChoices();
            m_InkController = InkpieceController.instance;
        }

        void OnEnable()
        {
            //m_InkController.onContinueText -= UpdateText;
            //m_InkController.onChoicesAvailable -= UpdateChoices;
            //m_InkController.onContinueText += UpdateText;
            //m_InkController.onChoicesAvailable += UpdateChoices;
        }

        void UpdateText(string text, List<string> tags)
        {
            m_StoryText.text = text;
            m_StoryText.DOFade(1f, .75f).From(0f);
            DisableChoices();

            m_StoryText.gameObject.SetActive(!text.Equals(""));
        }

        void UpdateChoices(List<Choice> choices)
        {
            DisableChoices();
            for (int i = 0; i < choices.Count; i++)
            {
                var text = choices[i].text.Trim();
                var index = choices[i].index;
                m_ChoiceButtons[i].onClick.RemoveAllListeners();
                m_ChoiceButtons[i].onClick.AddListener(delegate
                {
                    m_InkController.MakeChoice(index);
                });

                m_ButtonLabels[i].text = text;
                m_ChoiceButtons[i].gameObject.SetActive(true);

              //  m_ChoiceButtons[i].transform.DOLocalMoveY(-100, .35f).From().SetEase(Ease.OutCubic).SetDelay((i + 1) * 0.35f);
                m_ButtonLabels[i].DOFade(1f, .75f).From(0f).SetDelay((i + 1) * 0.35f);
            }
        }

        void DisableChoices()
        {
            for (int i = 0; i < m_ChoiceButtons.Count; i++)
            {
                m_ChoiceButtons[i].gameObject.SetActive(false);
            }
        }
    }
}

#endif