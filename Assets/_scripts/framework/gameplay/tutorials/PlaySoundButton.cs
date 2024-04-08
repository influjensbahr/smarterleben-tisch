// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 


using OTBT.Framework.Audio;
using OTBT.Framework.Localization;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace OTBT.Framework.Gameplay
{
    public class PlaySoundButton : MonoBehaviour
    {
        [SerializeField] Button m_PlaySoundButton = default;
        [SerializeField] LocalizeTextUI m_ButtonCaption = default;
        [SerializeField] Image m_ButtonIcon = default;

        [SerializeField] Sprite m_RegularSprite = default;
        [SerializeField] Sprite m_StopSprite = default;

        [SerializeField] LocalizedTextObject m_RegularLocaText = default;
        [SerializeField] LocalizedTextObject m_StopLocaText = default;

        [SerializeField] bool m_IgnoreToggleMode = false;

        SingleActiveSound m_CurrentSound = null;
        int m_TextID = -1;

        Func<string, string> m_FormURLFunction;

        private void OnEnable()
        {
            m_PlaySoundButton.onClick.AddListener(PlaySoundPressed);
            UpdateDisplay();
        }

        private void OnDisable()
        {
            m_PlaySoundButton.onClick.RemoveListener(PlaySoundPressed);
        }

        public void SetTextID(int i, Func<string, string> urlFunc)
        {
            if (urlFunc != null)
                m_FormURLFunction = urlFunc;
            m_TextID = i;
        }

        public void StopSound()
        {
            if (m_CurrentSound != null)
                m_CurrentSound.StopAudio();
        }

        public void PlaySoundPressed()
        {
            if (m_CurrentSound == null)
            {
                PlayCurrentSound();
            }
            else if (m_CurrentSound != null)
            {
                m_CurrentSound.StopAudio();
            }
            UpdateDisplay();
        }

        public void PlayCurrentSound()
        {
            if (m_CurrentSound != null)
            {
                m_CurrentSound.StopAudio();
            }

            if (m_TextID >= 1)
            {
                StartCoroutine(LoadHelperTest(m_FormURLFunction(m_TextID.ToString())));
            }
        }

        IEnumerator LoadHelperTest(string uri)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(uri);
                    Debug.Log(www.error);
                }
                else
                {
                    AudioClip clip = ((DownloadHandlerAudioClip)www.downloadHandler).audioClip;
                    if (clip != null)
                    {
                        clip.LoadAudioData();
                        AudioPlayer.instance.PlayOneShotSpeech(clip, onComplete: PlaySoundComplete, onStart: StartPlaySound);
                    }
                }
            }
        }


        void StartPlaySound(SingleActiveSound s)
        {
            m_CurrentSound = s;
            UpdateDisplay();
        }

        void PlaySoundComplete(SingleActiveSound s)
        {
            if (m_CurrentSound == s)
                m_CurrentSound = null;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            m_ButtonIcon.sprite = m_CurrentSound == null ? m_RegularSprite : m_StopSprite;
            m_ButtonCaption.SetLocalizedTextObject(m_CurrentSound == null ? m_RegularLocaText : m_StopLocaText);
        }
    }
}