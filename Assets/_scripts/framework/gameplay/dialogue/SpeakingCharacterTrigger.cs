using OTBT.Framework.Audio;
using OTBT.Framework.Localization;
using OTBT.Framework.Utils;
using Sparrow.Verification;
#if OTBT_LipSync
using RogoDigital.Lipsync;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static OTBT.Framework.Gameplay.SpeakingCharacter;

namespace OTBT.Framework.Gameplay
{
    public class SpeakingCharacterTrigger : MonoBehaviour, IVerify
    {
        [SerializeField] SpeakingCharacter m_SpeakingCharacter;
        public SpeakingCharacter speakingCharacter => m_SpeakingCharacter;

        internal Emotion m_CurrentEmotion = Emotion.NEUTRAL;

#if OTBT_LipSync
        [SerializeField] LipSync m_CharLipSync = null;
        public LipSync lipSync => m_CharLipSync;
#endif

        public virtual void SetSpeakingAnimState(bool speak, SpeakingCharacter.Emotion emotion = SpeakingCharacter.Emotion.NEUTRAL) { }
        public virtual void SetEmotionAnimState(SpeakingCharacter.Emotion emotion) { }

#if OTBT_LipSync
        public void SetLipSync(LipSync n)
        {
            m_CharLipSync = n;
            SetDirty();
        }

        public (float, AudioClip) SpeakWithLipSync(string text, Emotion emotion, LipSyncData lipSyncData = null, AudioClip clip = null, int lineId = -1)

        {
            if (text == null) return (0f, null);
            float duration = (((text).Length / 5f) / 120f) * 60f; // guess
            bool audioPlayed = false;
            m_CurrentEmotion = emotion;

            if (lipSync != null && text != null && clip != null)
            {
                LipSyncData ls = lipSyncData;// text.GetLipSync();
                if (ls != null)
                {
                    lipSync.Play(ls);
                    duration = ls.clip.length;
                    audioPlayed = true;
                }
            }

            if (!audioPlayed)
            {
                AudioClip audio = clip;// text.audioClip;
                if (audio != null)
                {
                    duration = audio.length;
                    AudioPlayer.instance.PlayOneShotSpeech(audio);
                    audioPlayed = true;
                }
            }

            SetSpeakingAnimState(true, emotion);
            DialogueManager.instance.AddLineDisplay(m_SpeakingCharacter, text, lineId);

            return (duration, clip);
        }
#endif

        public (float, AudioClip) Speak(string text, Emotion emotion, AudioClip clip = null, int lineId = -1)
        {
            if (text == null) return (0f, null);
            float duration = (((text).Length / 5f) / 120f) * 60f; // guess
            bool audioPlayed = false;
            m_CurrentEmotion = emotion;

            if (!audioPlayed)
            {
                if (clip != null)
                {
                    duration = clip.length;
                    AudioPlayer.instance.PlayOneShotSpeech(clip);
                    audioPlayed = true;
                }
            }

            SetSpeakingAnimState(true, emotion);
            DialogueManager.instance.AddLineDisplay(m_SpeakingCharacter, text, lineId);

            return (duration, clip);
        }

        public async Task<(float, AudioClip)> Speak(LocalizedTextObject text, Emotion emotion)
        {
#if OTBT_LipSync
            return SpeakWithLipSync(await text.localizedString, emotion, text.GetLipSync(), text.GetBestAudio(), text.textID);// (audioPlayed ? duration : ((text.localizedString.Length / 5f) / 120f * 60f), text.audioClip);
#else
            return Speak(await text.localizedString, emotion, text.GetBestAudio(), text.textID);// (audioPlayed ? duration : ((text.localizedString.Length / 5f) / 120f * 60f), text.audioClip);
#endif
        }

        public void StopSpeaking()
        {
            SetSpeakingAnimState(false);
        }

        public void SetEmotion(Emotion emotion)
        {
            m_CurrentEmotion = emotion;
            SetEmotionAnimState(emotion);
        }

        private void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        public void Verify(CheckVerifyInterface checker)
        {
            checker.CheckNotNull(m_SpeakingCharacter, "Speaking character", this);
        }
    }
}
