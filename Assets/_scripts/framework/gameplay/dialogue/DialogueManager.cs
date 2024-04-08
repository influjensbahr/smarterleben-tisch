// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using System;
using OTBT.Framework.Localization;
using OTBT.Framework.UI;
using OTBT.Framework.Utils;
using Sparrow.Verification;
#if OTBT_INK
using OTBT.Framework.Ink;
#endif
using System.Collections.Generic;
using UnityEngine;
using OTBT.Framework.Core;
using OTBT.Framework.Networking;
using UnityEditor;

namespace OTBT.Framework.Gameplay
{
    public class DialogueManager : Singleton<DialogueManager>, IVerify
    {
        [System.Serializable]
        public enum VoiceOption
        {
            PRIMARY, SECONDARY, TERTIARY, FOUR, FIVE, SIX
        }

        [Serializable]
        private class SkippedDialogueData
        {
            public string passwd;
            public int projectID;
            public string skippedIDs;
        }

        [Header("Defaults")]
        [SerializeField] SpeakingCharacter m_DefaultPlayer = null;
        SpeakingCharacter m_CurrentPlayer = null;
        [SerializeField] SpeakingCharacterTrigger m_DefaultPlayerTrigger = null;
        [SerializeField] DialogueDisplayUI m_DefaultDialogueDisplayUI = null;
        [SerializeField] NotificationDisplayUI m_DefaultNotificationDisplayUI = null;

        DialogueDisplayUI m_CurrentDisplayUI = null;
        NotificationDisplayUI m_CurrentNotificationUI = null;

        [Header("Options")]

        [SerializeField] List<DialogueDisplayUI> m_DialogueDisplayUIList = new List<DialogueDisplayUI>();
        [SerializeField] bool m_PrintToLog = false;

        Func<string, string> m_LineTransformOverride;

        Queue<ActiveDialogue> m_ActiveDialogueObjectPool = new Queue<ActiveDialogue>();
        List<ActiveDialogue> m_CurrentlySpeaking = new List<ActiveDialogue>();

        SpeakingCharacterTrigger m_CurrentPlayerTrigger = null;

        Dictionary<SpeakingCharacter, VoiceOption> m_PreferredVoices = new Dictionary<SpeakingCharacter, VoiceOption>();

        public Dictionary<SpeakingCharacter, VoiceOption> preferredVoices => m_PreferredVoices;

        public List<DialogueDisplayUI> dialogueSystems => m_DialogueDisplayUIList;
        public SpeakingCharacter player => m_CurrentPlayer == null ? m_DefaultPlayer : m_CurrentPlayer;
        public SpeakingCharacterTrigger playerTrigger => m_CurrentPlayerTrigger == null ? m_DefaultPlayerTrigger : m_CurrentPlayerTrigger;
        public DialogueDisplayUI display => m_CurrentDisplayUI == null ? m_DefaultDialogueDisplayUI : m_CurrentDisplayUI;
        public NotificationDisplayUI notifications => m_CurrentNotificationUI == null ? m_DefaultNotificationDisplayUI : m_CurrentNotificationUI;

        /*public List<SpeakingCharacter> speakers => m_SpeakingCharacters;*/
        public event Action onLineEnd;
        public bool showingDialogue => display.showingDialogue || display.showingChoiceDisplay || m_CurrentlySpeaking.Count > 0;

        string m_SkippedDialogues = ""; 

        // send info to server for statistical reasons
        public void SkippedDialogue(int id)
        {
            m_SkippedDialogues = m_SkippedDialogues + (m_SkippedDialogues.Length > 0 ? ";" : "") + id;
        }

        private void SendSkippedDialogue()
        {
            if(LocalizationDatabase.instance.useWebServer && m_SkippedDialogues.Length > 0)
            {
                SkippedDialogueData skippedDialogueData = new SkippedDialogueData()
                {
                    passwd = LocalizationDatabase.instance.serverPassword,
                    projectID = LocalizationDatabase.instance.projectID,
                    skippedIDs = m_SkippedDialogues
                };
                _ = NodeJsonDownloader.PostSingleObject(LocalizationDatabase.instance.serverAddress + "/skippedDialogue", JsonUtility.ToJson(skippedDialogueData), (a) =>
                {
                    m_SkippedDialogues = "";
                }, (error) =>
                {
                    Debug.Log("Error sending dialogue skips to server: " + error);
                });
            }
        }

        public void SetPlayerTrigger(SpeakingCharacterTrigger speakingCharacter)
        {
            m_CurrentPlayerTrigger = speakingCharacter;
        }

        void Start()
        {
            for(int i = 0; i < 5; i++) m_ActiveDialogueObjectPool.Enqueue(new ActiveDialogue());
            EventManager.instance.StartListening(EventManager.spacePressed, SkipPressed);
            if (LocalizationDatabase.instance.useWebServer)
                EventManager.instance.StartListening(EventManager.eventEverySecond, SendSkippedDialogue);
        }
        
        public void ShowDialogueChoices(List<SingleDialogueChoice> choices)
        {
            if (display) display.ShowChoiceDisplay(choices);
        }

        public void HideDialogueChoices()
        {
            if(display) display.HideChoiceDisplay();
        }

        public string TransformLine(string line)
        {
            if (m_LineTransformOverride == null) return line;
            return m_LineTransformOverride(line);
        }

        public void SetLineTransform(Func<string, string> transformFunction)
        {
            m_LineTransformOverride = transformFunction;
        }

#if OTBT_INK
        public void PlayInkDialogue(SpeakingCharacter speaker,bool isPlayer, string line, ILocalizedText gatheredText = null, AudioClip audioClip = null)
        {
            //TODO: Not sure how this system works. Convert ILocalizedText to LocalizedTextObject or change the PlayDialogue function
            InkpieceController.instance.currentDialogueTime = PlayDialogue(speaker, line,isPlayer).duration;
        }
#endif

        public ActiveDialogue PlayDialogue(SpeakingCharacterTrigger speaker, string line, bool isPlayer, LocalizedTextObject localizedText = null, SpeakingCharacter.Emotion emotion = SpeakingCharacter.Emotion.NEUTRAL)
        {
            m_ActiveDialogueObjectPool.TryDequeue(out ActiveDialogue active);
            if (active == null) active = new ActiveDialogue();

            active.PlayDialogue(speaker, line, isPlayer, localizedText, emotion);
            m_CurrentlySpeaking.Add(active);
            return active;
        }

        public void ReleaseDialogue(ActiveDialogue dialogue)
        {
            dialogue.Reset();
            if (m_CurrentlySpeaking.Contains(dialogue))
                m_CurrentlySpeaking.Remove(dialogue);
            m_ActiveDialogueObjectPool.Enqueue(dialogue);
        }

        public void AddLineDisplay(SpeakingCharacter character, string line, int lineId = -1, ActiveDialogue activeDialogue = null)
        {
            if (m_PrintToLog) Dbg.Log(this, "Start -- " + (character != null ? character.speakerName + ": " : "") + line);
            display.AddLine(character, line, lineId, activeDialogue);
        }

        public void EndLineDisplay(SpeakingCharacterTrigger character, string line, ActiveDialogue activeDialogue = null)
        {
            if (m_PrintToLog) Dbg.Log(this, "End -- " + (character != null ? character.speakingCharacter.speakerName + ": " : "") + line);
            if (character != null) character.StopSpeaking();
            foreach(DialogueDisplayUI displayUI in m_DialogueDisplayUIList)
                displayUI.EndLine(character.speakingCharacter, line, activeDialogue);
            onLineEnd?.Invoke();
        }

        public void SetCurrentPlayer(SpeakingCharacter character)
        {
            m_CurrentPlayer = character;
        }

        public void SetCurrentDialogueDisplay(DialogueDisplayUI disp)
        {
            m_CurrentDisplayUI = disp;
        }

        public void SetCurrentNotificationDisplay(NotificationDisplayUI disp)
        {
            m_CurrentNotificationUI = disp;
        }

        public void Verify(CheckVerifyInterface checker) 
        {
#if UNITY_EDITOR
            checker.Check(m_DialogueDisplayUIList.Contains(m_DefaultDialogueDisplayUI), "Dialogue Display UI List should contain default", this, () =>
            {
                m_DialogueDisplayUIList.Add(m_DefaultDialogueDisplayUI);
                EditorUtility.SetDirty(this);
            });
#endif
        }

        internal void SkipPressed()
        {

            for(int i = m_CurrentlySpeaking.Count - 1; i >= 0; i--)
            {
                ActiveDialogue cur = m_CurrentlySpeaking[i];
                cur.AbortDialogue();
                if (m_CurrentlySpeaking.Contains(cur))
                    m_CurrentlySpeaking.Remove(cur);
                m_ActiveDialogueObjectPool.Enqueue(cur);
            }
        }
    }
}
