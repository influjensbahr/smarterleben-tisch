//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Localization;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using UnityEditor;
using System;
using OTBT.Framework.Utils;
using System.IO;
using System.Collections.Generic;
using OTBT.Framework.Gameplay;
using static OTBT.Framework.Gameplay.DialogueManager;

namespace OTBT.Framework.Networking
{
    [Serializable]
    public struct JsonNewDBEntryResponse
    {
        public int textID;
    }

    [Serializable]
    public struct JsonLanguageResponse
    {
        public int id;
        public int languageID;
        public int textID;
        public string stringText;
    }

    [Serializable]
    public struct JsonChoiceStatisticsResponse
    {
        public int id;
        public int projectID;
        public string choiceHash;
        public int choice0;
        public int choice1;
        public int choice2;
        public int choice3;
        public int choice4;
        public int choice5;
        public int choice6;
        public int choice7;
        public int choice8;
        public int choice9;
    }

    public class TranslationServer 
    {
        static string serverAddress => LocalizationDatabase.instance.serverAddress;
        static string serverPassword => LocalizationDatabase.instance.serverPassword;
        static int projectID => LocalizationDatabase.instance.projectID;
        static bool useWebServer => LocalizationDatabase.instance.useWebServer;

        [System.Serializable]
        public enum TTSService
        {
            AWS, AZURE, GOOGLE
        }


        [System.Serializable]
        public class GoogleVoice
        {
            public string name;
            public string ssmlGender;
        }

        [System.Serializable]
        public class GoogleVoiceList
        {
            public GoogleVoice[] voices;
        }

        [System.Serializable]
        public class AwsVoice
        {
            public string Gender;
            public string Id;
            public string[] SupportedEngines;
        }

        [System.Serializable]
        public class AwsVoiceList
        {
            public AwsVoice[] Voices;
        }

        [System.Serializable]
        public class AzureVoice
        {
            public string[] privStyleList;
            public string privShortName;
            public int privGender;
        }


        [Serializable]
        public class TTSConfigData
        {
            public TTSLocale language;
            public string voiceID;
            public string gender;
            public string infos;
            public TTSService service;

            public VoiceOption optionNumber;

#if UNITY_EDITOR
            public int selectedVoiceIDIndex = 0;
            public List<(string, string, string)> voiceIDs = new List<(string, string, string)>();

            public void DrawVoiceIDEditor(SpeakingCharacter setDirtyTarget)
            {
                if (voiceIDs.Count > 0)
                {
                    string[] labels = new string[voiceIDs.Count];
                    for (int i = 0; i < voiceIDs.Count; i++) { labels[i] = voiceIDs[i].Item1 + " (" + voiceIDs[i].Item2 + ", " + voiceIDs[i].Item3 + ")"; }
                    selectedVoiceIDIndex = EditorGUILayout.Popup("Voice ID", selectedVoiceIDIndex, labels);
                    if (selectedVoiceIDIndex >= 0 && selectedVoiceIDIndex < voiceIDs.Count)
                    {
                        voiceID = voiceIDs[selectedVoiceIDIndex].Item1;
                        gender = voiceIDs[selectedVoiceIDIndex].Item2;
                        gender = voiceIDs[selectedVoiceIDIndex].Item3;
                        EditorUtility.SetDirty(setDirtyTarget);
                    }
                }
            }

            public async void LoadVoiceIDsAndGenders()
            {
                if (voiceIDs == null) voiceIDs = new List<(string, string, string)>();
                voiceIDs.Clear();

                if (service == TTSService.AZURE)
                {
                    await NodeJsonDownloader.GetArrayResponse<AzureVoice>(LocalizationDatabase.instance.serverAddress + "/voices?passwd=" + LocalizationDatabase.instance.serverPassword + "&service=azure&language=" + language.ToString() + "&", (t) =>
                    {
                        foreach (AzureVoice voiceID in t.items)
                        {
                            string infos = "";
                            foreach (string s in voiceID.privStyleList) infos += (infos.Length > 0 ? "-" : "") + s;
                            voiceIDs.Add((voiceID.privShortName, voiceID.privGender == 1 ? "Female" : "Male", infos));
                        }
                    }, (error) =>
                    {
                        Debug.Log(error);
                    }, augmentArrayNotation: true);
                }
                else if (service == TTSService.AWS)
                {
                    await NodeJsonDownloader.GetSingleObject<AwsVoiceList>(LocalizationDatabase.instance.serverAddress + "/voices?passwd=" + LocalizationDatabase.instance.serverPassword + "&service=aws&language=" + language.ToString() + "&", (t) =>
                    {
                        foreach (AwsVoice voiceID in t.Voices)
                        {
                            string infos = "";
                            foreach (string s in voiceID.SupportedEngines) infos += (infos.Length > 0 ? "-" : "") + s;
                            voiceIDs.Add((voiceID.Id, voiceID.Gender, infos));
                        }
                    }, (error) =>
                    {
                        Debug.Log(error);
                    });
                }
                else if (service == TTSService.GOOGLE)
                {
                    await NodeJsonDownloader.GetSingleObject<GoogleVoiceList>(LocalizationDatabase.instance.serverAddress + "/voices?passwd=" + LocalizationDatabase.instance.serverPassword + "&service=google&language=" + language.ToString() + "&", (t) =>
                    {
                        foreach (GoogleVoice voiceID in t.voices)
                        {
                            voiceIDs.Add((voiceID.name, voiceID.ssmlGender, ""));
                        }
                    }, (error) =>
                    {
                        Debug.Log(error);
                    });
                }
            }
#endif
        }

#if UNITY_EDITOR
        public static string GetTTSAddress(string text, TTSConfigData config)
        {
            string ret = $"{serverAddress}/tts?passwd={serverPassword}&sex={config.gender}&text={text}&language={config.language.ToString()}&voiceID={config.voiceID}&service={config.service.ToString().ToLower()}&";
            return ret;
        }
        //https://api.jensbahr.com/tts?passwd=tSagnm5mC4b276BR3jt&service=google&language=de_DE&text=Dies%20ist%20ein%20test&sex=FEMALE&voiceID=de-DE-Neural-F&
       
        public static async Task GetTTS(string originalString, string baseFileName, TTSConfigData config, UnityAction<AudioClip> onSuccess = null)
        {
            if (!useWebServer) return;
           // Debug.Log(GetTTSAddress(originalString, config));
            LocalizationDatabase.instance.CheckIfGeneratedAudioDirectoryExists();
            await WebAssetCache.Download(GetTTSAddress(originalString, config),
                LocalizationDatabase.instance.generatedAudioFullPathEditor + Path.DirectorySeparatorChar + baseFileName + ".mp3",
                onSuccess,
                (e) => Dbg.Exception(null, e));
        }



#endif

        public static Task SendDialogueChoice(string hash, int id, UnityAction callback = null, UnityAction failurecallback = null)
        {
            if (!useWebServer) return null;
            return NodeJsonDownloader.SendSingleObject(serverAddress + "/dialogueChoice?passwd=" + serverPassword + "&projectID=" + projectID + "&choiceHash=" + hash + "&choiceSelectedId=" + id + "&",
                (a) =>
                {
                    callback?.Invoke();
                }, (error) =>
                {
                    failurecallback?.Invoke();
                    Debug.Log("Error wehen UPDATING DialogueChoiceStats FROM DB: " + error);
                });
        }

        public static Task LoadDialogueStats(string hash, UnityAction<JsonChoiceStatisticsResponse> callback = null, UnityAction failurecallback = null)
        {
            if (!useWebServer) return null;
            return NodeJsonDownloader.GetArrayResponse<JsonChoiceStatisticsResponse>(serverAddress + "/getDialogueStats?passwd=" + serverPassword + "&projectID=" + projectID + "&choiceHash=" + hash + "&",
                (a) =>
                {
                    foreach (JsonChoiceStatisticsResponse o in a.items)
                    {
                        callback?.Invoke(o);
                    }
                }, (error) =>
                {
                    failurecallback?.Invoke();
                    Debug.Log("No stats found for that conv object");
                }, augmentArrayNotation: true);
        }

        public static Task LoadSingleRuntimeTranslation(int textID, bool newlineToHTML = false, UnityAction callback = null, UnityAction failurecallback = null)
        {
            if (!useWebServer) return null;
            return NodeJsonDownloader.GetArrayResponse<JsonLanguageResponse>(serverAddress + "/getSingleLoca?passwd=" + serverPassword + "&projectID=" + projectID + "&textID=" + textID + "&",
                (a) =>
                {
                    foreach (JsonLanguageResponse o in a.items)
                    {
                        RuntimeLocalization.instance.AddLanguageEntry(o.textID, newlineToHTML ? o.stringText.Replace("\\n", "<br>") : o.stringText, o.languageID);
                    }
                    LocalizationDatabase.instance.onLanguageChange.Invoke(LocalizationDatabase.instance.currentLanguage);
                    callback?.Invoke();
                }, (error) =>
                {
                    failurecallback?.Invoke();
                    Debug.Log("Error wehen UPDATING loca FROM DB: " + error);
                }, augmentArrayNotation: true);
        }

        public static Task LoadRuntimeTranslations(UnityAction callback = null, UnityAction failurecallback = null)
        {
            if (!useWebServer) return null;
            return NodeJsonDownloader.GetArrayResponse<JsonLanguageResponse>(serverAddress + "/getAllLoca?passwd=" + serverPassword + "&projectID=" + projectID + "&",
                (a) =>
                {
                    foreach (JsonLanguageResponse o in a.items)
                    {
                        RuntimeLocalization.instance.AddLanguageEntry(o.textID, o.stringText, o.languageID);
                    }
                    LocalizationDatabase.instance.onLanguageChange.Invoke(LocalizationDatabase.instance.currentLanguage);
                    callback?.Invoke();
                }, (error) =>
                {
                    failurecallback?.Invoke();
                    Debug.Log("Error wehen UPDATING loca FROM DB: " + error);
                }, augmentArrayNotation: true);
        }

#if UNITY_EDITOR
        [Obsolete]
        public static async Task AddToServer(LocalizedTextObject stg, bool mergeDoubles, string serverAddress, string serverPassword, int projectID, UnityAction callbackSuccess = null, UnityAction callbackFailure = null)
        {
            if (!useWebServer) return;
            await AddToServer(stg, mergeDoubles, projectID, callbackSuccess, callbackFailure);
        }

        /// <summary>
        /// Removes a text entry from the server
        /// </summary>
        /// <param name="stg"></param>
        public static async void RemoveFromServer(LocalizedTextObject stg, int projectID, UnityAction callbackSuccess = null, UnityAction callbackFailure = null)
        {
            if (!useWebServer) return;
            if (serverAddress.Equals("")) return;
            if (stg.textID <= 0) return;
            await NodeJsonDownloader.GetSingleObject<JsonNewDBEntryResponse>($"{serverAddress}/removeLoca?passwd={serverPassword}&textID={stg.textID}&projectID={projectID}&",
                (a) => {
                    callbackSuccess?.Invoke();
                }, (error) => {
                    callbackFailure?.Invoke();
                });
        }

        /// <summary>
        /// Pushes a single text gather to the server
        /// </summary>
        /// <param name="stg"></param>
        public static async Task AddToServer(LocalizedTextObject stg, bool mergeDoubles, int projectID, UnityAction callbackSuccess = null, UnityAction callbackFailure = null)
        {
            if (!useWebServer) return;
            if (serverAddress.Equals("")) return;
            if (stg.textID > 0) return;

            await NodeJsonDownloader.GetSingleObject<JsonNewDBEntryResponse>(serverAddress + "/addLoca?passwd=" + serverPassword +
                "&mergeDoubles=" + (mergeDoubles ? 1 : 0) + "&originalLanguage=" + stg.originalString + "&projectID=" + projectID + "&baseLanguageID=" + LocalizationDatabase.baseLanguageID + "&",
                (a) => {
                    stg.SetDatabaseID(a.textID);
                    EditorUtility.SetDirty(stg);
                    stg.Synchronized();
                    callbackSuccess?.Invoke();
                }, (error) => {
                    Debug.Log("Error wehen adding new loca to DB: " + error);
                    callbackFailure?.Invoke();
                }, timeout: 5);
        }

        /// <summary>
        /// Pushes voice info to the server
        /// </summary>
        /// <param name="stg"></param>
        public static async Task UpdateVoiceInfoToServer(LocalizedTextObject stg, string fieldName, string setValue)
        {
            if (!useWebServer) return;
            if (stg.textID <= 0) return;

            await NodeJsonDownloader.SendSingleObject($"{serverAddress}/voiceOverInfo?textID={stg.textID}&passwd={serverPassword}" +
                $"&setValue={setValue}&fieldName={fieldName}&projectID={projectID}&",
                (a) => {}, (error) => {});
        }

        [Obsolete]
        public static async Task UpdateToServer(LocalizedTextObject stg, string serverAddress, string serverPassword, UnityAction callbackSuccess = null, UnityAction callbackFailure = null)
        {
            await UpdateToServer(stg, callbackSuccess, callbackFailure);
        }

        /// <summary>
        /// Pushes a single text gather to the server
        /// </summary>
        /// <param name="stg"></param>
        public static async Task UpdateToServer(LocalizedTextObject stg, UnityAction callbackSuccess = null, UnityAction callbackFailure = null)
        {
            if (!useWebServer) return;
            if (serverAddress.Equals("")) return;

            if(stg != null && stg.translations != null)
                foreach (var t in stg.translations)
                    await NodeJsonDownloader.SendSingleObject(serverAddress + "/updateLoca?textID=" + stg.textID + "&passwd=" + serverPassword +
                    "&languageID=" + t.languageId + "&stringText=" + t.txt + "&projectID=" + projectID + "&",
                    (a) => {
                        callbackSuccess?.Invoke();
                    }, (error) => {
                        Debug.Log("Error wehen UPDATING loca TO DB: " + error);
                        callbackFailure?.Invoke();
                    });

            await NodeJsonDownloader.SendSingleObject(serverAddress + "/updateLoca?textID=" + stg.textID + "&passwd=" + serverPassword +
                "&languageID="+ LocalizationDatabase.baseLanguageID + "&stringText=" + stg.originalString + "&projectID=" + projectID + "&",
                (a) => {
                    callbackSuccess?.Invoke();
                }, (error) => {
                    Debug.Log("Error wehen UPDATING loca TO DB: " + error);
                    callbackFailure?.Invoke();
                });
            stg.Synchronized();
        }
#endif

        [Obsolete]
        public static async Task UpdateFromServer(LocalizedTextObject stg, string serverAddress, string serverPassword, UnityAction callback, bool saveDB = true, UnityAction callbackFailure = null)
        {
            await UpdateFromServer(stg, callback, saveDB, callbackFailure);
        }

        /// <summary>
        /// Dowloads the current translation state from the Server
        /// </summary>
        /// <param name="stg"></param>
        public static async Task UpdateFromServer(LocalizedTextObject stg, UnityAction callback, bool saveDB = true, UnityAction callbackFailure = null)
        {
            if (!useWebServer) return;
            if (serverAddress.Equals("")) return;

            await NodeJsonDownloader.GetArrayResponse<JsonLanguageResponse>(serverAddress + "/getLoca?textID=" + stg.textID + "&passwd=" + serverPassword,
            (a) =>
            {
                foreach (JsonLanguageResponse o in a.items)
                {
                    stg.UpdateLanguage(o.languageID, o.stringText);
                }
#if UNITY_EDITOR
                if (saveDB) stg.Save();
#endif
                stg.Synchronized();
                callback?.Invoke();
            }, (error) => {
                Debug.Log("Error wehen UPDATING loca FROM DB: " + error);
                callbackFailure?.Invoke();
            }, augmentArrayNotation: true);
        }
    }
}
