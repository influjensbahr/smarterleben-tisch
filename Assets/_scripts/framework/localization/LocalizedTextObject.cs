//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Gameplay;
using OTBT.Framework.Networking;
#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
#endif
#if OTBT_LipSync
using RogoDigital.Lipsync;
#endif
using Sparrow.Verification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static OTBT.Framework.Gameplay.DialogueManager;
using static OTBT.Framework.Networking.TranslationServer;

namespace OTBT.Framework.Localization
{
    /// <summary>
    /// Scriptable object that holds info on a single localized Text
    /// </summary>
    [CreateAssetMenu(menuName = "OTBT/Localization/Localized Text", fileName = "New Localized Text")]
    [Serializable]
    public class LocalizedTextObject : ScriptableObject, IVerify, ILocalizedText
    {
        // Web database and localization
        [Serializable]
        public struct Translation
        {
            public int languageId;
            public string txt;

            public Translation(int languageID, string stringText, AudioClip clip = null)
            {
                this.languageId = languageID;
                this.txt = stringText;
            }
            public void SetText(string t) { this.txt = t; }
        }

        [SerializeField] string m_OriginalString = "";

        [SerializeField, HideInInspector] int m_TextID = -1;
        [SerializeField] SpeakingCharacter m_SpeakingCharacter;
        [SerializeField, Tooltip("If true, this scriptable object will look for a runtime translation before using its own value")] 
        bool m_AllowRuntimeOverride = false;
        [SerializeField, Tooltip("If true, this will force loading an online update if runtime translation is not yet found")]
        bool m_ForceSingleRuntimeUpdate = false;

        public SpeakingCharacter speakingCharacter => m_SpeakingCharacter;
        public bool allowRuntimeOverride => m_AllowRuntimeOverride;
        public bool forceSingleRuntimeUpdate => m_ForceSingleRuntimeUpdate;
        public void SetSpeakingCharacter(SpeakingCharacter sc)
        {
            m_SpeakingCharacter = sc;
        }

        bool m_HasBeenUpdated = false;
        public void UpdatedValue()
        {
            m_HasBeenUpdated = true;
        }
        public void Synchronized()
        {
            m_HasBeenUpdated = false;
        }

        [SerializeField] List<Translation> m_Translations = new List<Translation>();



        // VOICED parts ---------

        [HideInInspector][SerializeField] string m_BaseFileIdentifier = "";
#if OTBT_LipSync
        [HideInInspector][SerializeField] LipSyncData m_LipSyncOverride = null;
#endif

        [HideInInspector][SerializeField] bool m_IsVoiced = true;
        [HideInInspector][SerializeField] bool m_TranscriptCreated = false;

#if UNITY_EDITOR
        [HideInInspector] public bool showEditor = false;
#endif

        struct TTSResult
        {
            public int voiceID;
            public string voiceName;
            public string transcript;
            public string filename;

#if UNITY_EDITOR
            public string audioPath => LocalizationDatabase.instance.generatedAudioFullPathEditor + Path.DirectorySeparatorChar + filename + ".mp3";
#endif
        }
        [HideInInspector][SerializeField] List<TTSResult> m_TTSResults = new List<TTSResult>();

        public Task<string> localizedString => GetTranslation(LocalizationDatabase.instance.currentLanguage);
        public List<Translation> translations => m_Translations;
        public string originalString => m_OriginalString;
        public int textID => m_TextID;
        public bool temporaryID => m_TextID <= 0;
        public Task<bool> hasMissingLanguages => HasMissingTranslations();
        public bool needsProcessing => m_HasBeenUpdated;
        public string baseFileName => m_BaseFileIdentifier + "_" + textID;
        public string transcriptFileName => baseFileName + ".txt";
        public string lipsyncFileName => baseFileName + ".asset";
        public bool transcriptCreated => m_TranscriptCreated;
        public bool isVoiced => m_IsVoiced;

        public AudioClip GetBestAudio()
        {
            VoiceOption preferredVoice = DialogueManager.instance.preferredVoices.ContainsKey(m_SpeakingCharacter) ? DialogueManager.instance.preferredVoices[m_SpeakingCharacter] : VoiceOption.PRIMARY;

            string path = "";
            string optionPath = "";
            for (int i = 0; i < m_SpeakingCharacter.ttsConfigs.Count; i++)
            {
                Language lang = LocalizationDatabase.instance.currentLanguage;
                if (lang != null && lang.ttsLocale == m_SpeakingCharacter.ttsConfigs[i].language)
                {
                    if (m_SpeakingCharacter.ttsConfigs[i].optionNumber == preferredVoice)
                    {
                        path = GetVoicePath(lang.id, i);
                    } else
                    {
                        optionPath = GetVoicePath(LocalizationDatabase.instance.currentLanguage.id, i);
                    }
                }
            }

            AudioClip ret = null;
            ret = LoadAudioClipFromPath(LocalizationDatabase.instance.audioPath + Path.DirectorySeparatorChar + path);
            if (ret) return ret;

            ret = LoadAudioClipFromPath(LocalizationDatabase.instance.generatedAudioPath + Path.DirectorySeparatorChar + path);
            if (ret) return ret;


            ret = LoadAudioClipFromPath(LocalizationDatabase.instance.audioPath + Path.DirectorySeparatorChar + optionPath);
            if (ret) return ret;

            ret = LoadAudioClipFromPath(LocalizationDatabase.instance.generatedAudioPath + Path.DirectorySeparatorChar + optionPath);
            return ret;
        }

        // paths for EDITOR use, file creation etc
#if UNITY_EDITOR
        public string lisyncFullPath => LocalizationDatabase.instance.lipSyncFullPathEditor + Path.DirectorySeparatorChar + lipsyncFileName;

        public void OnDestroy()
        {
            CheckBeforeDestruction();
        }

        public void CheckBeforeDestruction()
        {
            if (m_TextID > 0)
            {
                if (EditorUtility.DisplayDialog("Remove from database",
                                        $"You are deleting a localized object. Shall we also delete the entry FROM THE DATABASE?", "Delete from DB", "Do Not Delete"))
                {
                    TranslationServer.RemoveFromServer(this, LocalizationDatabase.instance.projectID);
                }
            }
            if(m_TTSResults.Count > 0)
            {
                if (EditorUtility.DisplayDialog("Remove audio clip",
                                        $"You are deleting a localized object. Shall we also delete the GENERATED TTS VOICED AUDIO FILES? Usually you'll want to do this.", "Delete audio clips", "Do Not Delete"))
                {
                    for(int i = 0; i < m_TTSResults.Count; i++)
                    {
                        AssetDatabase.DeleteAsset(m_TTSResults[i].audioPath);
                    }
                }
            }
        }
#endif


        public void SetIsVoiced(bool v)
        {
            if (v != m_IsVoiced)
            {
                m_IsVoiced = v;
                Save();
            }
        }

        public void Save()
        {
#if UNITY_EDITOR
            OTBT.Framework.Utils.Editor.EditorUtils.Save(this);
#endif
        }

        public void SetBaseIdentifier(string bas)
        {
            m_BaseFileIdentifier = bas;
        }

        public LocalizedTextObject(int databaseID = -1)
        {
            m_TextID = databaseID;
            if (textID >= 0)
            {
                RenameFile();
            }
            Save();
        }


#if UNITY_EDITOR
        public virtual void Process(UnityAction callback = null)
        {
            if (LocalizationDatabase.instance.useWebServer)
            {
                if (m_TextID <= 0)
                {
                    // connect us to server and process again!
                    _ = TranslationServer.AddToServer(this, false, LocalizationDatabase.instance.projectID, () =>
                    {
                        // push all the translations as well
                        _ = TranslationServer.UpdateToServer(this, callback);
                    });
                    return;
                }
            }

            if (isVoiced)
            {
                if (LocalizationDatabase.instance.generateTranscripts)
                {
                    string directory = LocalizationDatabase.instance.transcriptFullPathEditor;
                    LocalizationDatabase.instance.CheckIfTranscriptDirectoryExists();

                    StreamWriter writer = File.CreateText(directory + Path.DirectorySeparatorChar + transcriptFileName);
                    writer.WriteLine(originalString);
                    writer.Close();
                    m_TranscriptCreated = true;
                }

                LocalizationDatabase.instance.CheckIfAudioDirectoryExists();
            }
            Save();
        }


        public async Task PushToServer()
        {
            if (LocalizationDatabase.instance.useWebServer)
            {
                if (m_TextID <= 0)
                {
                    // connect us to server and process again!
                    await TranslationServer.AddToServer(this, false, LocalizationDatabase.instance.projectID, async () =>
                    {
                        // push all the translations as well
                        await TranslationServer.UpdateToServer(this);
                        m_HasBeenUpdated = false;
                    });
                    return;
                }
                else
                {
                    await TranslationServer.UpdateToServer(this);
                    m_HasBeenUpdated = false;
                }
            }
        }

#endif

        public async Task LoadFromServer()
        {
            if (LocalizationDatabase.instance.useWebServer)
            {
                translations.Clear();
                await TranslationServer.UpdateFromServer(this, null);
                UpdatedValue();
            }
        }

        /// <summary>
        /// Returns the localized version of a string from the database. Note that arabic strings might still need to be corrected, which should be handled when assigning the text to the text components
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public async Task<string> GetTranslation(Language language, bool forceExternalUpdate = false)
        {
            if (allowRuntimeOverride)
            {
                if(await RuntimeLocalization.instance.ContainsTranslation(textID, language.id))
                {
                    var t = await RuntimeLocalization.instance.GetLocalized(textID, forceExternalUpdate);
                    if (t != null) 
                        return await t.GetTranslation(language);
                }
            }
            if (translations == null) m_Translations = new List<Translation>();
            foreach (Translation t in translations)
            {
                if (t.languageId == language.id) return t.txt;
            }
            return originalString;
        }

        public async Task<bool> HasTranslation(Language language)
        {
            if (translations == null) return false;
            foreach (Translation t in translations)
            {
                if (t.languageId == language.id) return true;
            }
            if (allowRuntimeOverride)
                return await RuntimeLocalization.instance.ContainsTranslation(textID, language.id);
            return false;
        }

        private async Task<bool> HasMissingTranslations()
        {
            foreach (Language l in LocalizationDatabase.instance.additionalLanguages)
            {
                if (!await HasTranslation(l))
                    return true;
            }
            return false;
        }

        public void UpdateLanguage(int languageID, string stringText)
        {
            if (languageID == LocalizationDatabase.baseLanguageID)
            {
                m_OriginalString = stringText;
                UpdatedValue();
            }
            else
            {
                for (int i = 0; i < translations.Count; i++)
                {
                    if (translations[i].languageId == languageID)
                    {
                        translations[i].SetText(stringText);
                        UpdatedValue();
#if UNITY_EDITOR
                        Save();
#endif
                        return;
                    }
                }
                translations.Add(new Translation(languageID, stringText));
            }

#if UNITY_EDITOR
            Save();
#endif
        }

        public async Task AddMissingLanguages()
        {
            foreach (Language l in LocalizationDatabase.instance.additionalLanguages)
            {
                if (! await HasTranslation(l))
                    UpdateLanguage(l.id, "[T]" + m_OriginalString);
            }
            UpdatedValue();
        }

        public void SetDatabaseID(int textID)
        {
            m_TextID = textID;
            if (m_TextID >= 0)
            {
                RenameFile();
            }
            m_TextID = textID;
            Save();
            UpdatedValue();
        }
        
        public void RenameFile()
        {
#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(this.GetInstanceID());
            AssetDatabase.RenameAsset(assetPath, m_TextID.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
        
        public void SetOriginalString(string txt)
        {
            m_OriginalString = txt;
            UpdatedValue();
        }

    
        public static LocalizedTextObject Create(string originalString, string creationNote, string folder = "Localization", UnityAction callback = null)
        {
            LocalizedTextObject segment = ScriptableObject.CreateInstance<LocalizedTextObject>();
            segment.SetDatabaseID(-1);
            segment.SetOriginalString(originalString);
#if UNITY_EDITOR
            segment.SetAutomaticNote(creationNote);


            string filename = ((("tmp_" + LocalizationDatabase.instance.GetFreeID())).ToLower().Replace(" ", "-"));
            if (!Directory.Exists("Assets/Resources/" + folder))
                Directory.CreateDirectory("Assets/Resources/" + folder);
            AssetDatabase.CreateAsset(segment, "Assets/Resources/" + folder + "/" + filename + ".asset");
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(segment);

             callback?.Invoke();
#endif
            return segment;
        }


        AudioClip LoadAudioClipFromPath(string assetPath)
        {
            if (assetPath == null || assetPath.Equals("") || assetPath.Length < 5) return null;
            try
            {
                string resourcePath = assetPath;
                if (resourcePath.StartsWith("Assets/Ressources/"))
                    resourcePath = resourcePath.Substring(assetPath.IndexOf("Resources/") + 10);
                //Debug.Log(assetPath);
                // Assuming the path is something like "Assets/Resources/Audio/clip_123.wav"
                // We strip it down to "Audio/clip_123"
                resourcePath = resourcePath.Replace(".wav", "").Replace(".mp3", "");

                return Resources.Load<AudioClip>(resourcePath);
            } catch(Exception e)
            {
                Debug.Log(e.Message + assetPath);
                return null;
            }
        }

#if OTBT_LipSync
        public LipSyncData GetLipSync()
        {
            if (m_LipSyncOverride != null) return m_LipSyncOverride;
            return Resources.Load<LipSyncData>(LocalizationDatabase.instance.lipSyncPath + Path.DirectorySeparatorChar + baseFileName);
        }
#endif


        public string GetVoicePath(int languageID, int voiceNumber)
        {
            string langName = LocalizationDatabase.instance.GetLanguageByID(languageID).ttsLocale.ToString();
            return langName + Path.DirectorySeparatorChar + m_SpeakingCharacter.speakerID + Path.DirectorySeparatorChar + voiceNumber + Path.DirectorySeparatorChar + baseFileName;
        }

#if UNITY_EDITOR
        [SerializeField] string m_ManualNote = "";
        [SerializeField, HideInInspector] string m_AutomaticNote = "";
        [SerializeField, HideInInspector] LocalizedTextObject m_PreviousObject = null;
        public string manualNote => m_ManualNote;
        public string automaticNote => m_AutomaticNote;
        public void SetAutomaticNote(string s)
        {
            m_AutomaticNote = s;
            EditorUtility.SetDirty(this);
            _ = TranslationServer.UpdateVoiceInfoToServer(this, "automaticDesc", m_AutomaticNote);
        }
        public void SetManualNote(string s)
        {
            m_ManualNote = s;
            EditorUtility.SetDirty(this);
            _ = TranslationServer.UpdateVoiceInfoToServer(this, "manualDesc", m_ManualNote);
        }

        public void SetPreviousObject(LocalizedTextObject s)
        {
            m_PreviousObject = s;
            EditorUtility.SetDirty(this);
            _ = TranslationServer.UpdateVoiceInfoToServer(this, "previousTextID", m_PreviousObject == null ? "0" : m_PreviousObject.textID.ToString());
        }

        public async Task GenerateTTS(bool clear = false)
        {
            if (m_SpeakingCharacter == null) return;
            if (m_TTSResults == null) m_TTSResults = new List<TTSResult>();
            if(clear) m_TTSResults.Clear();

            for (int i = 0; i < m_SpeakingCharacter.ttsConfigs.Count; i++)
            {
                for (int j = -1; j < translations.Count; j++)
                {
                    bool foundValidTranslation = false;
                    int translationLanguageID = j == -1 ? LocalizationDatabase.instance.baseLanguage.id : translations[j].languageId;
                    string translationText = j == -1 ? m_OriginalString : translations[j].txt;
                
                    Language lang = LocalizationDatabase.instance.GetLanguageByID(translationLanguageID);
                    if (lang != null && lang.ttsLocale == m_SpeakingCharacter.ttsConfigs[i].language)
                    {
                        if(!clear)
                        {
                            // check if we already have an up-to-date TTS for this!
                            for (int o = m_TTSResults.Count - 1; o >= 0; o--) {
                                TTSResult res = m_TTSResults[o];
                                if (res.voiceID.Equals(i)) // we have one, but is it up to date?
                                {
                                    if (res.transcript.Equals(translationText) && res.voiceName.Equals(m_SpeakingCharacter.ttsConfigs[i].voiceID))
                                    {
                                        foundValidTranslation = true;
                                        break;
                                    } else
                                    {
                                        m_TTSResults.RemoveAt(o);
                                    }
                                }
                                
                            }
                            if (foundValidTranslation)
                                break;
                        }

                        if (m_SpeakingCharacter.ttsConfigs[i].voiceID.Length > 0)
                        {
                            string newFilename = GetVoicePath(translationLanguageID, i);
                            await TranslationServer.GetTTS(translationText, newFilename, m_SpeakingCharacter.ttsConfigs[i], (a) =>
                            {
                                m_TTSResults.Add(new TTSResult()
                                {
                                    voiceID = i,
                                    voiceName = m_SpeakingCharacter.ttsConfigs[i].voiceID,
                                    transcript = translationText,
                                    filename = newFilename
                                }); 
                            });
                        }
                        else
                        {
                            Debug.LogError($"Speaker {name} is missing voice ID for language {lang.ttsLocale}");
                        }
                    }
                }
            }
            await PushToServer();
        }
#endif

        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_TextID > 0, "LocalizeTextObject has no textID assigned.", this);
        }


#if UNITY_EDITOR

        public void SetTextID(int id)
        {
            m_TextID = id;
            Save();
        }
#endif

    }
}