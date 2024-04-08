//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEngine;
using System;
using Sparrow.Verification;
#if UNITY_EDITOR
using UnityEditor;
using static OTBT.Framework.Audio.SoundImporterDefaults;
#endif
using static OTBT.Framework.Audio.AudioVolumes;

namespace OTBT.Framework.Audio
{
    /// <summary>
    /// Handles import settings and closed captions for all sound files in the game.
    /// </summary>
    [CreateAssetMenu(menuName = "OTBT/Audio/SoundImporter", fileName = "New SoundImporter")]
    [Serializable]
    public class SoundImporter : ScriptableObject, IVerify
    {
        [SerializeField] AudioClip m_AudioClip = default;

        [Header("Ingame behavior")]
        [SerializeField] SoundCategory m_SoundCategory = SoundCategory.NOT_YET_SET;
        // TODO: add closed captions support
       // [SerializeField] string m_ClosedCaptionTextPlaceholder = "";
       // [SerializeField] int m_ClosedCaptionLangID = -1;

        public SoundCategory category => m_SoundCategory;
        public AudioClip clip => m_AudioClip;


#if UNITY_EDITOR
        [Header("Import Settings")]
        [SerializeField] AudioImportPreset m_ImportSettingsPreset = AudioImportPreset.NOT_YET_SET;
        [Tooltip("Check this when the sound has to be available when a scene starts. Increases scene load time, so use sparingly.")]
        [SerializeField, WideToggle] bool m_MustBeAvailableAtSceneStart = false;
        [Tooltip("When this option is active, it's ok for this sound to take a few ms before starting to optimize overall performance.")]
        [SerializeField, WideToggle] bool m_WhenPlayedCanHaveSmallDelay = false;
        [Tooltip("Turn this on for sounds that are used in the 3D world. This saves us resources.")]
        [SerializeField, WideToggle] bool m_IsUsedOnlyIn3D = false;

        public void SetClip(AudioClip c)
        {
            m_AudioClip = c;
            EditorUtility.SetDirty(this);
        }

        public bool CheckIfShouldAdjustImportSettings()
        {
            if (m_AudioClip == null)
            {
                Debug.Log("Found missing audio clip in SoundImporter: " + name);
            }
            if (!CheckClipImportSettingsValid(m_AudioClip))
                return true;
            return false;
        }

        public void AdjustImportSettings()
        {
            AdjustClipImportSettings(m_AudioClip);
        }


        public bool CheckClipImportSettingsValid(AudioClip clip)
        {
            if (m_ImportSettingsPreset == AudioImportPreset.NOT_YET_SET) return false;
            string path = AssetDatabase.GetAssetPath(clip);
            AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
            if (audioImporter == null || clip == null)
            {
                Debug.LogError("Could not load AssetImporter for: " + path + " // " + clip);
                return false;
            }

            AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
            AudioImportSettings presetToUse = SoundImporterDefaults.defaults[m_ImportSettingsPreset];

            if(audioImporter.loadInBackground != !m_MustBeAvailableAtSceneStart) return false;
            if (audioImporter.forceToMono != m_IsUsedOnlyIn3D) return false;

            if (settings.preloadAudioData != !m_WhenPlayedCanHaveSmallDelay) return false;
            if (settings.loadType != presetToUse.loadType) return false;
            if (settings.compressionFormat != presetToUse.compressionFormat) return false;
            if (settings.sampleRateSetting != presetToUse.sampleRate) return false;
            if (settings.quality != presetToUse.quality) return false;

            return true;
        }

        public void AdjustClipImportSettings(AudioClip clip, AudioImportPreset presetOverride = AudioImportPreset.NOT_YET_SET)
        {
            string path = AssetDatabase.GetAssetPath(clip);
            AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
            AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
            AudioImportSettings presetToUse = SoundImporterDefaults.defaults[presetOverride == AudioImportPreset.NOT_YET_SET ? m_ImportSettingsPreset : presetOverride];

            audioImporter.loadInBackground = !m_MustBeAvailableAtSceneStart;
            audioImporter.forceToMono = m_IsUsedOnlyIn3D;

            settings.loadType = presetToUse.loadType;
            settings.preloadAudioData = !m_WhenPlayedCanHaveSmallDelay;
            settings.compressionFormat = presetToUse.compressionFormat;
            settings.sampleRateSetting = presetToUse.sampleRate;
            settings.quality = presetToUse.quality;

            audioImporter.defaultSampleSettings = settings;
            Debug.Log("Adjusted import settings for: " + path + " (type: " + (presetOverride == AudioImportPreset.NOT_YET_SET ? m_ImportSettingsPreset : presetOverride) + ")");
            AssetDatabase.ImportAsset(path);
        }

        public bool HasBeenSet()
        {
            return m_ImportSettingsPreset != AudioImportPreset.NOT_YET_SET && m_SoundCategory != SoundCategory.NOT_YET_SET;
        }


#endif
        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
            checker.Check(m_ImportSettingsPreset != AudioImportPreset.NOT_YET_SET, "Import settings preset has not been set yet", this);
            checker.Check(m_SoundCategory != SoundCategory.NOT_YET_SET, "SoundCategory has not been set yet", this);
            checker.Check(clip != null, "Audio importer without audio clip", this);
#endif
        }
    }
}