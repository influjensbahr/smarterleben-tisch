//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Marc Freitag
//

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckImportSettingsAudio : VerifyCheckBase
    {
        public override string description => "Import settings: Audio";
        public override string longDescription => "Checks if audio import settings have been set according to your project-wide preferences. You can set this check up with your preferences on a per-folder basis.";
        
        [SerializeField] List<SingleFolder<ImportSettingsAudio>> m_Folders = new List<SingleFolder<ImportSettingsAudio>>();
        [SerializeField] List<ImportSettingsMismatch<ImportSettingsAudio>> m_mismatchedSettings = new List<ImportSettingsMismatch<ImportSettingsAudio>>();
        private const float m_TOLERANCE = 0.01f;
        
        public override bool DrawSpecificProfileEditor()
        {
            var hasChanged = false;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Folder", GUILayout.Width(100)))
            {
                m_Folders.Add(new SingleFolder<ImportSettingsAudio>("", null));
            }
            EditorGUILayout.LabelField("Path", GUILayout.MinWidth(50), GUILayout.MaxWidth(200));
            EditorGUILayout.LabelField("Import Settings", GUILayout.MinWidth(50), GUILayout.MaxWidth(200));
            EditorGUILayout.LabelField("", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();
            
            if(m_Folders.Count == 0)
                m_Folders.Add(new SingleFolder<ImportSettingsAudio>("", null));
            
            for (var i = 0; i < m_Folders.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var folderPath = m_Folders[i].folderPath;
                var importSettings = m_Folders[i].importSettings;
        
                if (GUILayout.Button("Select Folder", GUILayout.Width(100)))
                {
                    var selectedFolder = EditorUtility.OpenFolderPanel("Select Folder", m_Folders[i].folderPath, "");
                    if (!string.IsNullOrEmpty(selectedFolder))
                    {
                        if (selectedFolder.StartsWith(Application.dataPath))
                            m_Folders[i].folderPath = "Assets" + selectedFolder.Substring(Application.dataPath.Length);
                        else
                            Debug.LogError("Folder must be in Assets folder");
                        
                        return true;
                    }
                    GUIUtility.ExitGUI();
                }
        
                m_Folders[i].folderPath = EditorGUILayout.TextField(m_Folders[i].folderPath, GUILayout.MinWidth(50), GUILayout.MaxWidth(200));
                m_Folders[i].importSettings = EditorGUILayout.ObjectField(m_Folders[i].importSettings, typeof(ImportSettingsAudio), false, GUILayout.MinWidth(50), GUILayout.MaxWidth(200)) as ImportSettingsAudio;
                
                if (folderPath != null && folderPath != m_Folders[i].folderPath || importSettings != m_Folders[i].importSettings) hasChanged = true;
                
                if (m_Folders.Count > 1 && GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    m_Folders.RemoveAt(i); 
                } 
                
                EditorGUILayout.EndHorizontal();
            }
            
            return hasChanged;
        }

        public override void PerformCheck(AudioClip clip)
        {
            foreach (var folder in m_Folders)
            {
                var propertyMap = new Dictionary<string, Action>();
                var folderPath = folder.folderPath;
                var assetPath = AssetDatabase.GetAssetPath(clip);
                var audioImporter = AssetImporter.GetAtPath(assetPath) as AudioImporter;
                
                m_mismatchedSettings.Clear();
                assetPath = assetPath.Substring(0, assetPath.LastIndexOf("/", StringComparison.Ordinal));
                if(!assetPath.Equals(folderPath)) continue;
                
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) continue;
                CheckAudioImportSettings(folder.importSettings, audioImporter, propertyMap);

                if (m_mismatchedSettings.Count <= 0) continue;
                
                foreach (var mismatch in m_mismatchedSettings)
                {
                    var combinedList = string.Join(", ", propertyMap.Keys.ToArray());
                    AddFailedCheck($"Wrong Preset Settings: {combinedList}", clip, 
                        () => { ApplyCorrectImportSettings(mismatch.AudioImporter, propertyMap); });
                }
            }
        }
        
        private void ApplyCorrectImportSettings(AudioImporter audioImporter, Dictionary<string, Action> pMap)
        {
            //Debug.Log("Check AUDIO: Apply Property Count: " + pMap.Count);
            if (audioImporter == null)
            {
                Debug.LogError("Could not load AssetImporter for: " + audioImporter.assetPath);
                return;
            }
        
            foreach (var kvp in pMap)
            {
                kvp.Value.Invoke();
               // Debug.Log("Invoking: " + kvp.Key + " - " + kvp.Value);
            }
        
            // Reimport the asset to apply the changes
            AssetDatabase.ImportAsset(audioImporter.assetPath, ImportAssetOptions.ForceUpdate);
        }
        
        private ImportSettingsMismatch<ImportSettingsAudio> CheckAudioImportSettings(ImportSettingsAudio presetToUse, AudioImporter audioImporter, Dictionary<string, Action> pMap)
        {
            var assetPath = audioImporter.assetPath;
            var mismatch = new ImportSettingsMismatch<ImportSettingsAudio>(presetToUse, audioImporter);
            
            if (audioImporter == null)
            {
                Debug.LogError("AssetImporter could not be loaded: " + audioImporter.assetPath);
                return mismatch;
            }
            
            if (presetToUse == null)
            {
                var cutPath = assetPath.Substring(0, assetPath.LastIndexOf("/", StringComparison.Ordinal));
                //Debug.LogWarning($"Import Settings 2D Art for {cutPath} is not set.");
                return mismatch;
            }
            
            var settings = audioImporter.defaultSampleSettings;

            if (audioImporter.loadInBackground != presetToUse.loadInBackground.value && presetToUse.loadInBackground.toggle)
            {
                mismatch.AddMismatchedSetting("LoadInBackground");
                pMap.Add("LoadInBackground", () => { audioImporter.loadInBackground = presetToUse.loadInBackground.value; });
            }
            if (audioImporter.forceToMono != presetToUse.forceToMono.value && presetToUse.forceToMono.toggle)
            {
                mismatch.AddMismatchedSetting("ForceToMono");
                pMap.Add("ForceToMono", () => { audioImporter.forceToMono = presetToUse.forceToMono.value; });
            }

#if UNITY_2019 || UNITY_2020 || UNITY_2021
            if (audioImporter.preloadAudioData != presetToUse.preloadAudio.value && presetToUse.preloadAudio.toggle)
            {
                mismatch.AddMismatchedSetting("PreloadAudioData");
                pMap.Add("PreloadAudioData", () => { audioImporter.preloadAudioData = presetToUse.preloadAudio.value; });
            }
#else
            if (audioImporter.defaultSampleSettings.preloadAudioData != presetToUse.preloadAudio.value && presetToUse.preloadAudio.toggle)
            {
                mismatch.AddMismatchedSetting("PreloadAudioData");
                pMap.Add("PreloadAudioData", () => {
                    AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
                    settings.preloadAudioData = presetToUse.preloadAudio.value;
                    audioImporter.defaultSampleSettings = settings;
                });
            }
#endif

            if (settings.loadType != presetToUse.loadType.value && presetToUse.loadType.toggle)
            {
                mismatch.AddMismatchedSetting("LoadType");
                pMap.Add("LoadType", () => { settings.loadType = presetToUse.loadType.value; });
            }

            if (settings.compressionFormat != presetToUse.compressionFormat.value && presetToUse.compressionFormat.toggle)
            {
                mismatch.AddMismatchedSetting("CompressionFormat");
                pMap.Add("CompressionFormat", () => { settings.compressionFormat = presetToUse.compressionFormat.value; });
            }

            if (settings.sampleRateSetting != presetToUse.sampleRateSettings.value && presetToUse.sampleRateSettings.toggle)
            {
                mismatch.AddMismatchedSetting("SampleRateSetting");
                pMap.Add("SampleRateSetting", () => { settings.sampleRateSetting = presetToUse.sampleRateSettings.value; });
            }

            if (Math.Abs(settings.quality - presetToUse.quality.value) > m_TOLERANCE && presetToUse.quality.toggle)
            {
                mismatch.AddMismatchedSetting("Quality");
                pMap.Add("Quality", () => { settings.quality = presetToUse.quality.value; });
            }
            
            //Debug.Log("Check AUDIO: " + mismatch.MismatchedSettings.Count);
            
            if (mismatch.MismatchedSettings.Count > 0)
            {
                m_mismatchedSettings.Add(mismatch);
            }
            
            return mismatch;
        }
    }
}
#endif