#if UNITY_EDITOR
#if OTBT_LipSync

using System;
using System.Collections.Generic;
using System.IO;
using OTBT.Framework.Utils;
using OTBT.Framework.Utils.Editor;
using RogoDigital.Lipsync;
using RogoDigital.Lipsync.AutoSync;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.Localization
{
    public class AutoLipSync
    {
        bool m_ProcessMissingOnly = false;
        int selectedPreset = 0;
        string[] presetNames;
        bool m_HasRun = false;

        public void ShowEditorGUI()
        {
            EditorUtils.GuiLine();
            EditorUtils.DrawSubHeader("LipSync options");
            if (presets == null || presets.Length == 0)
            {
                presets = AutoSyncUtility.GetPresets();
                presetNames = new string[presets.Length];
                for (int i = 0; i < presetNames.Length; i++)
                    presetNames[i] = presets[i].name;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AutoSync Preset");
            selectedPreset = EditorGUILayout.Popup(selectedPreset, presetNames);
            EditorGUILayout.EndHorizontal();

            m_ProcessMissingOnly = EditorGUILayout.Toggle("Process missing only", m_ProcessMissingOnly);
            if (GUILayout.Button(EditorUtils.LabelWithGlyphicon("Generate LipSync", Glyphicons.QuotationLeft)))
            {
                LoadPreset(selectedPreset);
                LocalizationDatabase.instance.CheckIfLipSyncDirectoryExists();
                List<ClipItem> list = new List<ClipItem>();
               /* foreach (LocalizeTextCoupling tgc in stgm.elements)
                {
                    if (m_ProcessMissingOnly)
                    {
                        if (tgc.Item1.GetLipSync() != null)
                            continue;
                    }
                    if (!tgc.Item1.audioFound) continue;
                    list.Add(new ClipItem(tgc.Item1));
                }*/
                Debug.Log("Processing " + list.Count + " clips");
                ProcessClips(list);
            }

            if (m_HasRun)
            {
                EditorGUILayout.LabelField("Batch was " + (batchIncomplete ? "NOT" : "successfully") + "completed");
            }
        }

        public void ShowEditorGUI(LocalizedTextObject stg)
        {
            GUI.enabled = stg.GetBestAudio() != null;
            if (GUILayout.Button("Generate LipSync"))
            {
                ProcessClip(stg);
            }
            GUI.enabled = true;
        }

        public void ProcessClip(LocalizedTextObject stg)
        {
            List<ClipItem> list = new List<ClipItem>();
            list.Add(new ClipItem(stg));
            ProcessClips(list);
        }

        enum OutputMode
        {
            ToClipEditor,
            ToLipSyncData,
            ToXML,
            AppendToPrevious,
        }

        class ClipItem
        {
            public ClipItem(LocalizedTextObject stg)
            {
                audioClip = stg.GetBestAudio();
                transcript = stg.originalString;
                outputPath = stg.lisyncFullPath;
            }

            public ClipItem() { }

            public AudioClip audioClip;

            public string transcript;
            public string outputPath;

            public LipSyncData outputClip;
            public bool hasBeenProcessed;
            public LipSyncData[] appendedClips = new LipSyncData[0];
        }

        AutoSyncPreset[] presets;

        int currentPreset = -1;
        List<AutoSyncModule> currentModules = new List<AutoSyncModule>();
        PhonemeMarker phonemeTemplate;
        EmotionMarker emotionTemplate;
        List<ClipItem> clips = new List<ClipItem>();
        int currentClip = 0;
        bool batchIncomplete = true;

        void LoadPreset(int presetIndex)
        {
            currentModules.Clear();
            currentPreset = presetIndex;

            if (presetIndex >= 0)
            {
                for (int i = 0; i < presets[presetIndex].modules.Length; i++)
                {
                    AddModule(presets[presetIndex].modules[i], presets[presetIndex].moduleSettings[i]);
                }
            }

            phonemeTemplate = new PhonemeMarker(0, 0) {
                intensity = EditorPrefs.GetFloat("LipSync_DefaultPhonemeIntensity", 1f),
                useRandomness = EditorPrefs.GetBool("LipSync_DefaultUseRandomness", false),
                intensityRandomness = EditorPrefs.GetFloat("LipSync_DefaultIntensityRandomness", 0.1f),
                blendableRandomness = EditorPrefs.GetFloat("LipSync_DefaultBlendableRandomness", 0.3f),
                bonePositionRandomness = EditorPrefs.GetFloat("LipSync_DefaultBonePositionRandomness", 0.3f),
                boneRotationRandomness = EditorPrefs.GetFloat("LipSync_DefaultBoneRotationRandomness", 0.3f),
            };

            emotionTemplate = new EmotionMarker("", 0, 0, 0, 0, false, false, true, true) {
                intensity = EditorPrefs.GetFloat("LipSync_DefaultEmotionIntensity", 1f),
                continuousVariation = EditorPrefs.GetBool("LipSync_DefaultContinuousVariation", false),
                variationFrequency = EditorPrefs.GetFloat("LipSync_DefaultVariationFrequency", 0.5f),
                intensityVariation = EditorPrefs.GetFloat("LipSync_DefaultIntensityVariation", 0.35f),
                blendableVariation = EditorPrefs.GetFloat("LipSync_DefaultBlendableVariation", 0.1f),
                bonePositionVariation = EditorPrefs.GetFloat("LipSync_DefaultBonePositionVariation", 0.1f),
                boneRotationVariation = EditorPrefs.GetFloat("LipSync_DefaultBoneRotationVariation", 0.1f),
            };
        }

        private void AddModule(string name, string jsonData)
        {
            var module = (AutoSyncModule)ScriptableObject.CreateInstance(name);

            if (!module)
                return;

            if (!string.IsNullOrEmpty(jsonData))
            {
                JsonUtility.FromJsonOverwrite(jsonData, module);
            }

            module.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

            currentModules.Add(module);
        }

        AutoSync autoSyncInstance;


        void ProcessClips(List<ClipItem> clipsToProcess)
        {
            clips = clipsToProcess;
            batchIncomplete = false;

            if (autoSyncInstance == null)
                autoSyncInstance = new AutoSync();

            currentClip = 0;

            for (int i = 0; i < clips.Count; i++)
            {
                LipSyncData tempData = ScriptableObject.CreateInstance<LipSyncData>();
                tempData.clip = clips[i].audioClip;
                tempData.length = tempData.clip.length;
                tempData.transcript = clips[i].transcript;
                clips[i].outputClip = tempData;
            }

            autoSyncInstance.RunSequence(currentModules.ToArray(), FinishedClip, clips[currentClip].outputClip, phonemeTemplate, emotionTemplate);
        }

        private void FinishedClip(LipSyncData outputData, AutoSync.ASProcessDelegateData data)
        {
            if (data.success)
            {
                // Successfully processed clip, store data for output once the batch is complete
                clips[currentClip].outputClip = outputData;

                clips[currentClip].hasBeenProcessed = true;
            } else
            {
                // AutoSync was unsuccessful, log error and either continue or stop depending on skipOnFailValue
                string clipName = "Undefined";
                if (outputData.clip)
                {
                    clipName = outputData.clip.name;
                }

                batchIncomplete = true;
                clips[currentClip].hasBeenProcessed = false;
                Debug.LogErrorFormat("AutoSync: Processing failed on clip '{0}'. Continuing with batch.", clipName);
            }

            // There are more clips to process in the batch, so process the next one
            if (currentClip < clips.Count - 1)
            {
                currentClip++;
                autoSyncInstance = new AutoSync();
                autoSyncInstance.RunSequence(currentModules.ToArray(), FinishedClip, clips[currentClip].outputClip, phonemeTemplate, emotionTemplate);
                return;
            }

            m_HasRun = true;
            // This was the last clip, move on to output
            var outputs = new List<ClipItem>();

            for (int i = 0; i < clips.Count; i++)
            {
                var output = new ClipItem();

                output.outputClip = clips[i].outputClip;
                output.outputPath = clips[i].outputPath;

                var temp = (TemporaryLipSyncData)clips[i].outputClip;
                ArrayUtility.Add(ref output.appendedClips, (LipSyncData)temp);

                outputs.Add(output);
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                var settings = LipSyncEditorExtensions.GetProjectFile();
                var outputPath = "";

                outputPath = outputs[i].outputPath;

                outputPath = Path.ChangeExtension(outputPath, "asset");

                try
                {
                    LipSyncClipSetup.SaveFile(settings, outputPath, false, outputs[i].outputClip.transcript, outputs[i].outputClip.length, outputs[i].outputClip.phonemeData, outputs[i].outputClip.emotionData, outputs[i].outputClip.gestureData, outputs[i].outputClip.clip);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.StackTrace);
                }

            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

        }
    }
}
#endif
#endif
