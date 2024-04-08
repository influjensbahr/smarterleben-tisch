// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Audio;
using OTBT.Framework.Core;
using OTBT.Framework.Localization;
using OTBT.Framework.Utils;
using System;
using OTBT.Framework.Networking;
using UnityEngine;
using UnityEngine.Events;
using static OTBT.Framework.Core.EventManager;
using System.Threading.Tasks;

namespace OTBT.Framework.Gameplay
{
    public class ActiveDialogue : PausingGameplay
    {
        bool m_IsActive = false;
        string m_Line = "";
        SpeakingCharacterTrigger m_Speaker = null;
        LocalizedTextObject m_LocalizedText = null;
        float m_Duration = 0f;
        AudioClip m_AudioClip = null;
        SpeakingCharacter.Emotion m_Emotion = SpeakingCharacter.Emotion.NEUTRAL;
        bool m_IsInit = false;
        TimedEvent m_EndingTrigger = null;

        public UnityEvent OnFinish = new UnityEvent();
        
        public bool isActive => m_IsActive;
        public float duration => m_Duration;


        void Init()
        {
            m_IsInit = true;
            RegisterPauseEvents();
        }

        public override void OnPause()
        {
            if (!isActive) return;
            m_EndingTrigger.SetPaused(true);
        }

        public override void OnResume()
        {
            if (!isActive) return;
            m_EndingTrigger.SetPaused(false);
        }

        public void Reset()
        {
            m_IsActive = false;
            m_Line = "";
            m_Speaker = null;
            m_LocalizedText = null;
            m_Duration = 0f;
            m_AudioClip = null;
            OnFinish = new UnityEvent();
        }

        public async void PlayDialogue(SpeakingCharacterTrigger speaker, string line, bool isPlayer, LocalizedTextObject localizedText = null, SpeakingCharacter.Emotion emotion = SpeakingCharacter.Emotion.NEUTRAL)
        {
            if (!m_IsInit) Init();
            this.m_Speaker = speaker;
            this.m_LocalizedText = localizedText;
            this.m_Line = line;
            this.m_Emotion = emotion;

            if (speaker != null)
            {
                if (localizedText != null && !localizedText.localizedString.Equals(""))
                {
                    (m_Duration, m_AudioClip) = await speaker.Speak(localizedText, emotion);
                }
                else
                {
                    (m_Duration, m_AudioClip) = speaker.Speak(line, emotion, lineId: -1);
                }
            }
            else
            {
                DialogueManager.instance.AddLineDisplay(speaker.speakingCharacter, localizedText == null ? line : await localizedText.localizedString, localizedText == null ? -1 : localizedText.textID, this);
                m_AudioClip = (localizedText == null ? null : localizedText.GetBestAudio());
                AudioPlayer.instance.PlayOneShotSpeech(m_AudioClip);
                m_Duration = m_AudioClip == null
                    ? (((localizedText == null ? line : await localizedText.localizedString).Length / 5f) / 120f) * 60f
                    : m_AudioClip.length;
            }
            m_Duration = (isPlayer && m_AudioClip == null) ? 1.5f : m_Duration;
            m_EndingTrigger = EventManager.instance.TriggerInTime(m_Duration, StopDialogue);
#if UNITY_EDITOR
            m_EndingTrigger.debugDescription = "Dialogue: " + (localizedText == null ? line : localizedText.localizedString);
#endif
            m_IsActive = true;

            if (!EventManager.instance.gameRunning)
                OnPause();
        }

        public async void AbortDialogue()
        {
            if (!isActive) return;

            EventManager.instance.RemoveTimeTrigger(m_EndingTrigger);
            m_EndingTrigger = null;

            m_IsActive = false;
            AudioPlayer.instance.StopSpeech(m_AudioClip);
            DialogueManager.instance.EndLineDisplay(m_Speaker, m_LocalizedText == null ? m_Line : await m_LocalizedText.localizedString, this);
            OnFinish?.Invoke();
            // TODO: Data Tracking. Remove later
            if (m_LocalizedText != null)
                DialogueManager.instance.SkippedDialogue(m_LocalizedText.textID);
        }

        public async void StopDialogue()
        {
            if (isActive)
            {
                DialogueManager.instance.EndLineDisplay(m_Speaker, m_LocalizedText == null ? m_Line : await m_LocalizedText.localizedString, this);
                OnFinish?.Invoke();
            }

            m_IsActive = false;
            DialogueManager.instance.ReleaseDialogue(this);
        }
    }

}
