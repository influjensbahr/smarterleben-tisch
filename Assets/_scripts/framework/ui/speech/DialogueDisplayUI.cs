// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

#if OTBT_AC
using AC;
#endif
using OTBT.Framework.Gameplay;
using Sparrow.Verification;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using static OTBT.Framework.Utils.Subtitles;

namespace OTBT.Framework.UI
{
    public class DialogueDisplayUI : MonoBehaviour, IVerify
    {
        [SerializeField] UIScreen m_Screen;

        [Header("Single Line Displays")]
        [SerializeField] GameObject m_PrefabPlayerLine = null;
        [SerializeField] GameObject m_PrefabNPCLine = null;
        [SerializeField] Transform m_DialougeLinesParent = null;
        [SerializeField] Transform m_DialogueLinesPositionAnchor = null;

        [Header("Dialogue Option Display")]
        [SerializeField] DialogueChoiceDisplayUI m_DialogueChoiceUI = null;

        [Header("Display options")]
        [Tooltip("If true, lines will be removed immediately after they have finished playing. Turn off to allow for fade outs etc")]
        [SerializeField] bool m_RemoveImmediately = false;
        [Tooltip("If true, lines will not be removed automatically (e.g. for permanent display and not subtitles)")]
        [SerializeField] bool m_DontRemoveAtAll = false;
        [SerializeField] int m_DefaultObjectPoolCapacity = 3;

        SingleDialogueLineUI m_LastLine = null;
        List<SingleDialogueLineUI> m_Lines = new List<SingleDialogueLineUI>();
        ObjectPool<SingleDialogueLineUI> m_LinePoolPlayer = null;
        ObjectPool<SingleDialogueLineUI> m_LinePoolNPC = null;

        public GameObject prefabPlayerLine => m_PrefabPlayerLine == null ? m_PrefabNPCLine : m_PrefabPlayerLine;
        public GameObject prefabNPCLine => m_PrefabNPCLine == null ? m_PrefabPlayerLine : m_PrefabNPCLine;

        public bool showingChoiceDisplay => m_DialogueChoiceUI.showing;
        public bool showingDialogue => m_DontRemoveAtAll ? (m_Screen == null ? true : m_Screen.isShowing) : m_Lines.Count > 0;

        private void OnValidate()
        {
            if (m_Screen == null) m_Screen = GetComponent<UIScreen>();
        }

        public void ClearDisplay()
        {
            for(int i = m_Lines.Count -1; i >= 0; i--)
                RemoveFromDisplay(m_Lines[i]);
        }

        public void ShowChoiceDisplay(List<SingleDialogueChoice> choices)
        {
            m_DialogueChoiceUI.SetupChoices(choices);
            m_DialogueChoiceUI.Show();
        }

        public void HideChoiceDisplay()
        {
            m_DialogueChoiceUI.Hide();
        }

        public void SetAsCurrentDialogueDisplay()
        {
            DialogueManager.instance.SetCurrentDialogueDisplay(this);
        }
        public void RemoveCurrentDialogueDisplay()
        {
            DialogueManager.instance.SetCurrentDialogueDisplay(null);
        }

        public void RemoveFromDisplay(SingleDialogueLineUI lin)
        {
            if (m_LinePoolPlayer == null) InitObjectPools();
            bool isPlayer = lin.isPlayer;
            (isPlayer ? m_LinePoolPlayer : m_LinePoolNPC).Release(lin);
        }

        public void AddLine(SingleDialogueLineUI ui, SpeakingCharacter speaker, string line, bool isPlayer, int lineId = -1, ActiveDialogue activeDialogue = null)
        {
            ui.transform.SetAsFirstSibling();
            ui.Setup(this, m_LastLine, isPlayer, m_DialogueLinesPositionAnchor, lineId, activeDialogue: activeDialogue);
            ui.UpdateLine(speaker, line);
            m_LastLine = ui;
        }

        public void AddLine(SpeakingCharacter speaker, string line, int lineId = -1, ActiveDialogue activeDialogue = null)
        {
            if (m_LinePoolPlayer == null) InitObjectPools();
            bool isPlayer = DialogueManager.instance.player == speaker;
            SingleDialogueLineUI ui = (isPlayer ? m_LinePoolPlayer : m_LinePoolNPC).Get();
            AddLine(ui, speaker, line, isPlayer, lineId, activeDialogue);
        }

        public void EndLine(SpeakingCharacter speaker, string line, ActiveDialogue activeDialogue = null)
        {
            if (m_DontRemoveAtAll) return;
            foreach (SingleDialogueLineUI lin in m_Lines)
            {
                if ((lin.activeDialogue == activeDialogue) || (lin.speaker == speaker && lin.internalLine.Equals(line)))
                {
                    if (m_RemoveImmediately)
                    {
                        RemoveFromDisplay(lin);
                    }
                    else
                    {
                        lin.TriggerEnding(() => RemoveFromDisplay(lin));
                    }
                    return;
                }
            }
        }

        public void Verify(CheckVerifyInterface checker)
        {
            if(m_DontRemoveAtAll)
                checker.Check(m_Screen != null, "When DontRemoveAtAll is on, we need a screen reference", gameObject);
            checker.Check(m_DialougeLinesParent != null, "Dialogue system has no parent to add lines to", gameObject);
        }


        ObjectPool<SingleDialogueLineUI> CreateObjectPool(GameObject prefab)
        {
            return new ObjectPool<SingleDialogueLineUI>(() =>
            {
                GameObject newInstance = Instantiate(prefab, m_DialougeLinesParent);
                SingleDialogueLineUI ui = newInstance.GetComponent<SingleDialogueLineUI>();
                return ui;
            },
            (ui) =>
            {
                m_Lines.Add(ui);
                ui.gameObject.SetActive(true);
            },
            (ui) =>
            {
                m_Lines.Remove(ui);
                ui.ResetState();
                ui.gameObject.SetActive(false);
            }, defaultCapacity: m_DefaultObjectPoolCapacity);
        }

        internal void InitObjectPools()
        {
            m_LinePoolPlayer = CreateObjectPool(prefabPlayerLine);
            m_LinePoolNPC = CreateObjectPool(prefabNPCLine);
        }
    }
}
