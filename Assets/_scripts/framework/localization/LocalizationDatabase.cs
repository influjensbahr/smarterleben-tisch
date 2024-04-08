// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Core;
using OTBT.Framework.Networking;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Localization
{
    [CreateAssetMenu(menuName = "OTBT/Localization/Database Collection", fileName = "New Scene Text DB Collection")]
    public class LocalizationDatabase : ScriptableObject, IVerify
    {
        public static LocalizationDatabase instance => ReferenceManager.instance.locaDB;

        [SerializeField] LocalizedTextCollection m_TextCollection = null;

        [Header("Directory paths")]
        [SerializeField] string m_TranscriptsPath = "Transcripts";
        [SerializeField] string m_AudioPath = "Voice";
        [SerializeField] string m_GeneratedAudioPath = "GeneratedVoice";
        [SerializeField] string m_LipSyncPath = "Lipsync";

        [Header("Available Languages Settings")]

        [SerializeField] Language m_BaseLanguage = new Language(0, "German");
        [SerializeField] List<Language> m_AdditionalLanguages = new List<Language>();
        Language m_CurrentLanguage = null;
        Language m_SystemLanguage = null;

        [Header("Web Server Sync")]
        [SerializeField] bool m_UseWebServer = false;
        [SerializeField] string m_ServerAddress = "";
        [SerializeField] string m_ServerPassword = "";
        [SerializeField] int m_ProjectID = 0;

        [Header("Behaviour")]
        [SerializeField] bool m_GenerateTranscripts = false;
#if OTBT_LipSync
        [SerializeField] bool m_GenerateLipSyncFiles = false;
#endif

        // Directories
#if UNITY_EDITOR
        public string transcriptFullPathEditor => Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + Path.DirectorySeparatorChar + m_TranscriptsPath;
        public string audioFullPathEditor => Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + Path.DirectorySeparatorChar + m_AudioPath;
        public string generatedAudioFullPathEditor => Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + Path.DirectorySeparatorChar + m_GeneratedAudioPath;
        public string lipSyncFullPathEditor => Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + Path.DirectorySeparatorChar + m_LipSyncPath;
#endif
        public string audioPath => m_AudioPath;
        public string generatedAudioPath => m_GeneratedAudioPath;
        public string lipSyncPath => m_LipSyncPath;
        public string transcriptsPath => m_TranscriptsPath;

        // Avaiable Languages
        public Language baseLanguage => m_BaseLanguage;
        public List<Language> additionalLanguages => m_AdditionalLanguages;


        // Web Server Sync
        public string serverAddress => m_ServerAddress;
        public string serverPassword => m_ServerPassword;
        public bool useWebServer => m_UseWebServer;
        public int projectID => m_ProjectID;

        // Behaviour
        public bool generateTranscripts => m_GenerateTranscripts;
#if OTBT_LipSync
        public bool generateLipsync => m_GenerateLipSyncFiles;
#endif

        // working structures
        public UnityAction<Language> onLanguageChange;
        public static int baseLanguageID => LocalizationDatabase.instance.baseLanguage.id;
        public List<Language> languages => LocalizationDatabase.instance == null ? null : LocalizationDatabase.instance.allLanguages;
        public Language systemLanguage => GetSystemLanguage();
        public Language currentLanguage => m_CurrentLanguage == null ? LocalizationDatabase.instance.baseLanguage : m_CurrentLanguage;

        public LocalizedTextCollection textCollection => m_TextCollection;

        private void OnValidate()
        {
            m_SystemLanguage = null;
            m_CurrentLanguage = null;
        }

        public void SetGlobalLanguage(Language l)
        {
            if (l == null) return;
            m_CurrentLanguage = l;
            onLanguageChange?.Invoke(m_CurrentLanguage);
        }


        public List<Language> allLanguages
        {
            get
            {
                var lang = new List<Language>() {
                    baseLanguage
                };
                lang.AddRange(additionalLanguages);
                return lang;
            }
        }

        public Language GetLanguageByShortcode(string i)
        {
            i = i.Replace("_", "-");
            if (baseLanguage.languageShortCode.Replace("_", "-").Equals(i)) return baseLanguage;
            foreach (Language d in m_AdditionalLanguages) 
            {
                if (d.languageShortCode.Replace("_", "-").Equals(i)) return d;
            }
            return null;
        }


        public Language GetLanguageByID(int i)
        {
            if (baseLanguage.id == i) return baseLanguage;
            foreach (Language d in m_AdditionalLanguages)
                if (d.id == i) return d;
            return null;
        }

        public LocalizedTextObject GetByID(int i)
        {
            if (i <= 0) return null;
            return m_TextCollection.GetByID(i);
        }

        public bool ContainsID(int i)
        {
            return GetByID(i) != null;
        }



        Language GetSystemLanguage()
        {
            foreach (Language l in m_AdditionalLanguages)
            {
                if (l.systemLanguageKey.Equals(Application.systemLanguage.ToString()))
                {
                    m_SystemLanguage = l;
                    break;
                }
            }
            if (m_SystemLanguage == null) m_SystemLanguage = baseLanguage;
            Debug.Log("Determined system langugae: " + Application.systemLanguage + " // " + m_SystemLanguage.systemLanguageKey);
            return m_SystemLanguage;
        }
        public void Verify(CheckVerifyInterface checker)
        {
            foreach (Language g in m_AdditionalLanguages)
            {
                checker.Check(g.id != 0, "Additional Language ID should not be 0!", this);
            }
            checker.Check(this.name == "LocaDB", "LocaDB should keep its name LocaDB", this);
        }

        public async Task DownloadAllFromServer()
        {
#if UNITY_EDITOR
            textCollection.RefreshList();
#endif
            foreach (LocalizedTextObject o in textCollection.textObjects)
                await o.LoadFromServer();
        }


        public async Task UpdateLanguages(UnityAction successCallback = null, UnityAction failureCallback = null)
        {
            await NodeJsonDownloader.GetArrayResponse<Language>(serverAddress + "/getLanguage?passwd=" + serverPassword + "&projectID=" + LocalizationDatabase.instance.projectID + "&",
            (a) =>
            {
                additionalLanguages.Clear();

                foreach (Language l in a.items)
                {
                    if (!l.isBaseLanguage)
                    {
                        additionalLanguages.Add(l);
                    }
                    else
                    {
                        baseLanguage.SetTo(l);
                    }
                }

                successCallback?.Invoke();
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    EditorUtility.SetDirty(this);
#endif
            }, (error) =>
                    {
                        failureCallback?.Invoke();
                        Dbg.Log(this, "Error when connecting to DB: " + error);
                    }, augmentArrayNotation: true);
        }

#if UNITY_EDITOR

        internal int GetFreeID()
        {
            int ret = textCollection.textObjects.Count;
            while (GetByID(ret) != null)
                ret++;
            return ret;
        }

        public void CheckIfLipSyncDirectoryExists() => OTBT.Framework.Utils.Editor.EditorUtils.CheckIfDirectoryExists(AssetDatabase.GetAssetPath(this), m_LipSyncPath);
        public void CheckIfTranscriptDirectoryExists() => OTBT.Framework.Utils.Editor.EditorUtils.CheckIfDirectoryExists(AssetDatabase.GetAssetPath(this), m_TranscriptsPath);
        public void CheckIfAudioDirectoryExists() => OTBT.Framework.Utils.Editor.EditorUtils.CheckIfDirectoryExists(AssetDatabase.GetAssetPath(this), m_AudioPath);
        public void CheckIfGeneratedAudioDirectoryExists() => OTBT.Framework.Utils.Editor.EditorUtils.CheckIfDirectoryExists(AssetDatabase.GetAssetPath(this), m_GeneratedAudioPath);



#endif
    }
}
