//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Core;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using UnityEngine;
using UnityEngine.Audio;

namespace OTBT.Framework.Audio
{
    /// <summary>
    /// Handles all volume settings in the game and mixing/fading of volume values, as well as dialogue attenutaion.
    /// </summary>
    public class AudioVolumes : Singleton<AudioVolumes>, IVerify
    {
        public enum SoundCategory
        {
            MUSIC, SFX, UI, DIALOGUE, NOT_YET_SET
        }

        // exposed params of the audio mixer
        [SerializeField] string m_ParamNameMasterVolume = "masterVolume";
        [SerializeField] string m_ParamNameMusicVolume = "musicVolume";
        [SerializeField] string m_ParamNameSfxVolume = "sfxVolume";
        [SerializeField] string m_ParamNameSpeechVolume = "speechVolume";
        [SerializeField] string m_ParamNameDialogueAttenuation = "dialogueAttenuation";
        [SerializeField] string m_ParamNameUiSfxVolume = "uiSfxVolume";
        [SerializeField] float m_LerpSpeed = 2f;

        [Header("Mixer Groups")]
        [SerializeField] AudioMixer m_AudioMixer = default;
        [SerializeField] AudioMixerGroup m_AudioGrpSpeech;
        [SerializeField] AudioMixerGroup m_AudioGrpMusic;
        [SerializeField] AudioMixerGroup m_AudioGrpSfx;
        [SerializeField] AudioMixerGroup m_AudioGrpSfxUI;

        [Header("Game Settings")]
        [SerializeField] GameSetting m_SettingMasterVolume = default;
        [SerializeField] GameSetting m_SettingMusicVolume = default;
        [SerializeField] GameSetting m_SettingSFXVolume = default;
        [SerializeField] GameSetting m_SettingSFXUIVolume = default;
        [SerializeField] GameSetting m_SettingSpeechVolume = default;
        [SerializeField] GameSetting m_SettingDialogueAttenuation = default;
        [SerializeField] GameSetting m_SettingAudioChannels = default;


        // volume values
        public static float MasterVolume { set; get; }
        public static float MusicVolume { set; get; }
        public static float SoundVolume { set; get; }
        public static float UIVolume { set; get; }
        public static float SpeechVolume { set; get; }
        public static float DialogueAttenuationVolume { set; get; }
        public static int DialogueIsPlaying { set; get; }
        public static bool InMenus { set; get; }
        public static bool Mute { set; get; }

        float m_Master = 1f;
        float m_Music = 1f;
        float m_Sound = 1f;
        float m_UI = 1f;
        float m_Speech = 1f;
        float m_Attenuation = 1f;

        protected override void InitializeInherit()
        {
            m_SettingMasterVolume?.AddValueChangeAndExecute(x => MasterVolume = x / 10f);
            m_SettingMusicVolume?.AddValueChangeAndExecute(x => MusicVolume = x / 10f);
            m_SettingSFXVolume?.AddValueChangeAndExecute(x => SoundVolume = x / 10f);
            m_SettingSFXUIVolume?.AddValueChangeAndExecute(x => UIVolume = x / 10f);
            m_SettingSpeechVolume?.AddValueChangeAndExecute(x => SpeechVolume = x / 10f);
            m_SettingDialogueAttenuation?.AddValueChangeAndExecute(x => DialogueAttenuationVolume = x / 10f);
            m_SettingAudioChannels?.AddValueChangeAndExecute(x => SetSpeakerSetup((AudioChannels)x));
        }

        void Update()
        {
            if (m_AudioMixer == null) return;

            m_Master = Mathf.Lerp(m_Master, MasterVolume, Time.deltaTime * m_LerpSpeed);
            m_Music = Mathf.Lerp(m_Music, MusicVolume, Time.deltaTime * m_LerpSpeed);
            m_Sound = Mathf.Lerp(m_Sound, SoundVolume, Time.deltaTime * m_LerpSpeed);
            m_UI = Mathf.Lerp(m_UI, UIVolume, Time.deltaTime * m_LerpSpeed);
            m_Speech = Mathf.Lerp(m_Speech, SpeechVolume, Time.deltaTime * m_LerpSpeed);
            m_Attenuation = Mathf.Lerp(m_Attenuation, DialogueIsPlaying == 0 ? 1f : 1f - DialogueAttenuationVolume, Time.deltaTime * m_LerpSpeed);

            SetVolume(m_AudioMixer, m_ParamNameMasterVolume, Mute ? 0 : m_Master);
            SetVolume(m_AudioMixer, m_ParamNameMusicVolume, Mute ? 0 : m_Music);
            SetVolume(m_AudioMixer, m_ParamNameSfxVolume, Mute || InMenus ? 0 : m_Sound);
            SetVolume(m_AudioMixer, m_ParamNameUiSfxVolume, Mute ? 0 : m_UI);
            SetVolume(m_AudioMixer, m_ParamNameSpeechVolume, Mute ? 0 : m_Speech);
            SetVolume(m_AudioMixer, m_ParamNameDialogueAttenuation, Mute ? 0 : m_Attenuation);
        }

        public AudioMixerGroup GetAudioMixerGroup(SoundCategory category)
        {
            switch (category)
            {
                case SoundCategory.MUSIC: return m_AudioGrpMusic;
                case SoundCategory.DIALOGUE: return m_AudioGrpSpeech;
                case SoundCategory.UI: return m_AudioGrpSfxUI;
            }
            return m_AudioGrpSfx;
        }

        void SetVolume(AudioMixer mixer, string paramName, float value)
        {
            if (mixer == null) return;

            // Convert linear to decibels
            value = Mathf.Clamp(value, 0.0000001f, 1.0f);
            var db = 20 * Mathf.Log10(value);

            mixer.SetFloat(paramName, db);
        }

        public float GetVolume(AudioMixer mixer, string paramName)
        {
            if (mixer == null) return 1;

            if (!mixer.GetFloat(paramName, out float db))
                return 1;

            // Convert decibels to linear: https://youtu.be/Vjm--AqG04Y?t=9m53s
            // linear(x) = 10 ^ (x / 20)
            var value = Mathf.Clamp01(Mathf.Pow(10, db / 20));
            return Mathf.Clamp01(value);
        }

        public static void SetSpeakerSetup(AudioChannels x)
        {
            /*
            AudioConfiguration conf = UnityEngine.AudioSettings.GetConfiguration();
            AudioVolumes.Mute = x == AudioChannels.NONE;

            switch (x)
            {
                case AudioChannels.STEREO:
                    conf.speakerMode = AudioSpeakerMode.Stereo;
                    break;
                case AudioChannels.MONO:
                    conf.speakerMode = AudioSpeakerMode.Mono;
                    break;
                case AudioChannels.QUAD:
                    conf.speakerMode = AudioSpeakerMode.Quad;
                    break;
                case AudioChannels.SURROUND:
                    conf.speakerMode = AudioSpeakerMode.Surround;
                    break;
                case AudioChannels.FIVEPOINTONE:
                    conf.speakerMode = AudioSpeakerMode.Mode5point1;
                    break;
                case AudioChannels.SEVENPOINTONE:
                    conf.speakerMode = AudioSpeakerMode.Mode7point1;
                    break;
                case AudioChannels.PROLOGIC:
                    conf.speakerMode = AudioSpeakerMode.Prologic;
                    break;
            }

            UnityEngine.AudioSettings.Reset(conf);
            */
        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.CheckStringNotEmpty(m_ParamNameMasterVolume, "masterVolume", this);
            checker.CheckStringNotEmpty(m_ParamNameMusicVolume, "musicVolume", this);
            checker.CheckStringNotEmpty(m_ParamNameSfxVolume, "sfxVolume", this);
            checker.CheckStringNotEmpty(m_ParamNameSpeechVolume, "speechVolume", this);
            checker.CheckStringNotEmpty(m_ParamNameDialogueAttenuation, "dialogueAttenuation", this);
            checker.CheckStringNotEmpty(m_ParamNameUiSfxVolume, "uiSfxVolume", this);

            checker.CheckNotNull(m_AudioMixer, "m_AudioMixer", this);
            checker.CheckNotNull(m_AudioGrpSpeech, "m_AudioGrpSpeech", this);
            checker.CheckNotNull(m_AudioGrpMusic, "m_AudioGrpMusic", this);
            checker.CheckNotNull(m_AudioGrpSfx, "m_AudioGrpSfx", this);
            checker.CheckNotNull(m_AudioGrpSfxUI, "m_AudioGrpSfxUI", this);

            checker.CheckNotNull(m_SettingMasterVolume, "m_SettingMasterVolume", this);
            checker.CheckNotNull(m_SettingMusicVolume, "m_SettingMusicVolume", this);
            checker.CheckNotNull(m_SettingSFXVolume, "m_SettingSFXVolume", this);
            checker.CheckNotNull(m_SettingSFXUIVolume, "m_SettingSFXUIVolume", this);
            checker.CheckNotNull(m_SettingSpeechVolume, "m_SettingSpeechVolume", this);
            checker.CheckNotNull(m_SettingDialogueAttenuation, "m_SettingDialogueAttenuation", this);
            checker.CheckNotNull(m_SettingAudioChannels, "m_SettingAudioChannels", this);
        }
    }
}
