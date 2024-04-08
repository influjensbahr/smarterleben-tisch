using OTBT.Framework.Localization;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static OTBT.Framework.Audio.SoundImporterDefaults;

namespace OTBT.Framework.Audio
{
    public class AudioAssetProcessor : AssetPostprocessor
    {
        // create importer for all audio in the corresponding folder
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            string[] guids = AssetDatabase.FindAssets("t:SoundImporter", null);
            foreach (string str in importedAssets)
            {
                if (str.Contains("_audio"))
                {
                    var audioClip = AssetDatabase.LoadAssetAtPath(str, typeof(AudioClip)) as AudioClip;
                    if (audioClip == null) continue;

                    bool foundOne = false;
                    // for assets in _audio, create Importer if not exists
                    foreach (string guid in guids)
                    {
                        SoundImporter importer = AssetDatabase.LoadAssetAtPath<SoundImporter>(AssetDatabase.GUIDToAssetPath(guid));
                        if (importer.clip == audioClip && audioClip != null)
                        {
                            foundOne = true;
                            break;
                        }
                    }
                    if(!foundOne)
                    {
                        AudioEditorMenus.CreateOneSoundImporter(audioClip);
                    }
                }
            }
        }

        private void OnPreprocessAudio()
        {
            if(assetPath.Contains("Resources") && assetPath.Contains("Voice"))
            {
                AudioImporter audioImporter = (AudioImporter)assetImporter;
                AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
                AudioImportSettings presetToUse = SoundImporterDefaults.defaults[AudioImportPreset.VOICE_OVER];

                audioImporter.loadInBackground = true;

                settings.preloadAudioData = true;
                settings.loadType = presetToUse.loadType;
                settings.compressionFormat = presetToUse.compressionFormat;
                settings.sampleRateSetting = presetToUse.sampleRate;
                settings.quality = presetToUse.quality;

                audioImporter.defaultSampleSettings = settings;
            } 
        }

        private void OnPostprocessAudio(AudioClip newClip)
        {
            if (assetPath.Contains("Resources") && assetPath.Contains("Voice"))
            {
                AudioImporter audioImporter = (AudioImporter)assetImporter;
                AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
                AudioImportSettings presetToUse = SoundImporterDefaults.defaults[AudioImportPreset.VOICE_OVER];

                bool settingsChanged = settings.loadType != presetToUse.loadType || settings.compressionFormat != presetToUse.compressionFormat ||
                                       settings.sampleRateSetting != presetToUse.sampleRate || settings.quality != presetToUse.quality;

                if (settingsChanged)
                {
                    Debug.LogWarning($"Settings for audio clip '{newClip.name}' did not import correctly. Please reimport the asset manually.");
                }
            }
        }
    }
}
