//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using Newtonsoft.Json;
using OTBT.Framework.Debugging;
using OTBT.Framework.Gameplay;
using OTBT.Framework.Networking;
using OTBT.Framework.UI;
using OTBT.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace OTBT.Framework.Core
{
    /// <summary>
    /// This class handles the file operations to store and load our save game to disk - or some other place, if we later desire to add the functions.
    ///  It's also our main wrapper for all savegame-related storage.
    /// </summary>
    public class SaveGame : Singleton<SaveGame>
    {
        [Header("Cloud Save")]
        [SerializeField] bool m_CloudSaveActive = false;
        [SerializeField] string m_CloudSaveServerAddress = "";
        [SerializeField] string m_CloudSaveServerPassword = "";
        [SerializeField] string m_CloudSavePlayerIdentifier = "";
        [SerializeField] string m_CloudSaveSubversionID = "";
        [SerializeField] ResolveMode m_CloudSaveResolveMode = ResolveMode.Playtime;

        private enum ResolveMode
        {
            Playtime, Version, Date, AskAlways, AskCritical
        }

        public string cloudSaveEncryption => PlayerPrefs.GetString("cloudsave_key", "");
        public bool cloudSaveActive => m_CloudSaveActive;

        const string k_SavegameFileName = "savegame";
        const string k_MetaSavegameFileName = "savegame_meta";

        public string savegamePath => Application.persistentDataPath + Path.DirectorySeparatorChar + k_SavegameFileName;
        public string savegameMetaPath => Application.persistentDataPath + Path.DirectorySeparatorChar + k_SavegameFileName;

        UnityAction m_PreSaveActions;
        UnityAction m_PostLoadActions;
        bool m_LoadComplete = false;

        int m_SaveVersion = 0;
        float m_LastSaveTime = 0f;
        int m_Playtime = 0;
        static SimpleRaceCondition s_LocalDiskAccess = new SimpleRaceCondition();

        GenericDictionary m_Storage = new GenericDictionary();
        GenericDictionary m_MetaStorage = new GenericDictionary();


        GenericDictionary m_LoadLocalMain = new GenericDictionary();
        GenericDictionary m_LoadLocalMeta = new GenericDictionary();
        GenericDictionary m_LoadRemoteMain = new GenericDictionary();
        GenericDictionary m_LoadRemoteMeta = new GenericDictionary();


        [Serializable]
        public class RemoteSaveGame
        {
            public string savegame;
            public string savegameMeta;
        }


        public GenericDictionary metaSaveData => m_MetaStorage;
        public GenericDictionary saveData => m_Storage;
        public static GenericDictionary data => SaveGame.instance.saveData;
        public static GenericDictionary metadata => SaveGame.instance.metaSaveData;

        

        public void WhenReady(UnityAction action)
        {
            if (m_LoadComplete)
            {
                action.Invoke();
                return;
            }
            m_PostLoadActions += action;
        }

        public void AddPreSaveAction(UnityAction action)
        {
            m_PreSaveActions += action;
        }

        void Awake()
        {
            m_LastSaveTime = Time.time;
        }

        private void Start()
        {
            ErrorLogManager.instance.AddLog("Persistent data path: " + Application.persistentDataPath);
        }

        private async Task LoadLocalSaves()
        {
            m_LoadLocalMain.Clear();
            m_LoadLocalMeta.Clear();
            await m_LoadLocalMain.LoadFromFile(k_SavegameFileName, MigrateSaveVersion);
            await m_LoadLocalMeta.LoadFromFile(k_MetaSavegameFileName, MigrateMetaSaveVersion);
        }

        public async Task Load()
        {
            await s_LocalDiskAccess.WaitForTurn();
            AlwaysOnCanvas.instance.ShowSaveIcon();

            // load local and remote saves to temp storages
            await LoadLocalSaves();
            await LoadCloudSave();

            // resolve which save to load now
            bool loadRemote = false;
            switch (m_CloudSaveResolveMode)
            {
                case ResolveMode.Version:
                    loadRemote = m_LoadLocalMeta.GetInt("saveVersion", 0) < m_LoadRemoteMeta.GetInt("saveVersion", 0);
                    break;
                case ResolveMode.Playtime:
                    loadRemote = m_LoadLocalMeta.GetFloat("playtime", 0) < m_LoadRemoteMeta.GetFloat("playtime", 0);
                    break;
                case ResolveMode.Date:
                    loadRemote = string.Compare(m_LoadLocalMeta.GetString("saveDate", "2020-01-0100:00:00"), m_LoadRemoteMeta.GetString("saveDate", "2020-01-0100:00:00")) < 0;
                    break;
                case ResolveMode.AskAlways:
                    await CloudSaveScreen.instance.Resolve(m_LoadLocalMeta, m_LoadRemoteMeta);
                    loadRemote = CloudSaveScreen.instance.resolveResult;
                    break;
                case ResolveMode.AskCritical:
                    // only ask if cloud save would overwrite local storage
                    bool version = m_LoadLocalMeta.GetInt("saveVersion", 0) < m_LoadRemoteMeta.GetInt("saveVersion", 0);
                    bool playtime = m_LoadLocalMeta.GetFloat("playtime", 0) < m_LoadRemoteMeta.GetFloat("playtime", 0);
                    string localDate = m_LoadLocalMeta.GetString("saveDate", "2020-01-0100:00:00");
                    string remoteDate = m_LoadRemoteMeta.GetString("saveDate", "2020-01-0100:00:00");
                    bool date = string.Compare(localDate, remoteDate) < 0;
                    if (version != playtime || playtime != date)
                    {
                        await CloudSaveScreen.instance.Resolve(m_LoadLocalMeta, m_LoadRemoteMeta);
                        loadRemote = CloudSaveScreen.instance.resolveResult;
                    }
                    else
                    {
                        loadRemote = version;
                    }
                    break;
            }

            // apply the loaded save data now
            m_MetaStorage = !loadRemote ? m_LoadLocalMeta : m_LoadRemoteMeta;
            m_Storage = !loadRemote ? m_LoadLocalMain : m_LoadRemoteMain;
            Debug.Log("[CloudSave] - Load finished, using remote version: " + loadRemote + " with playtimes local/remote: " + m_LoadLocalMeta.GetInt("playtime", 0) + " // " + m_LoadRemoteMeta.GetInt("playtime", 0));

            m_Playtime = m_MetaStorage.GetInt("playtime", 0);
            m_SaveVersion = m_MetaStorage.GetInt("saveVersion", 0);

            AlwaysOnCanvas.instance.HideSaveIcon();
            s_LocalDiskAccess.Release();

            m_LastSaveTime = Time.time;
            m_LoadComplete = true;
            foreach (ISaveData data in ReflectionUtil.FindInterfaceImplementations<ISaveData>())
                data.WhenLoadReady();
            if (m_PostLoadActions != null) m_PostLoadActions.Invoke();
        }

        public async Task Save(bool skipPresaveActions = false)
        {
            if (!m_LoadComplete) return;

            if (!skipPresaveActions)
            {
                foreach (ISaveData data in ReflectionUtil.FindInterfaceImplementations<ISaveData>())
                    data.PreSaveAction();
                m_PreSaveActions?.Invoke();
            }
            m_Playtime += (int)(Time.time - m_LastSaveTime);
            m_LastSaveTime = Time.time;
            m_SaveVersion++;
            m_MetaStorage.SetInt("saveVersion", m_SaveVersion);
            m_MetaStorage.SetFloat("playtime", m_Playtime);
            m_MetaStorage.SetFloat("lastChange", m_Storage.lastChange);
            m_MetaStorage.SetString("appVersion", m_CloudSaveSubversionID);
            m_MetaStorage.SetString("saveDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // we store two versions for safety, when application gets killed during the write
            await s_LocalDiskAccess.WaitForTurn();
                AlwaysOnCanvas.instance.ShowSaveIcon();

                await m_Storage.SaveToFile(savegamePath);
                await m_MetaStorage.SaveToFile(savegameMetaPath);

                AlwaysOnCanvas.instance.HideSaveIcon();
            s_LocalDiskAccess.Release();

            // next upload to the server
            await UploadCloudSave();
        }

        public void ForceDeleteSave()
        {
            if (File.Exists(k_SavegameFileName))
                File.Delete(k_SavegameFileName);
            if (File.Exists(k_MetaSavegameFileName))
                File.Delete(k_MetaSavegameFileName);
        }
        
        public async Task DeleteSave(UnityAction callback)
        {
            await s_LocalDiskAccess.WaitForTurn();

                try
                {
                    if (File.Exists(savegamePath)) File.Delete(savegamePath);
                    if (File.Exists(savegameMetaPath)) File.Delete(savegameMetaPath);
                } catch(IOException e)
                {
                    Dbg.Exception(this, e);
                }
                m_LastSaveTime = Time.time;
                m_Playtime = 0;
                data.Clear();
                m_Storage.Clear();
                m_MetaStorage.Clear();

                await m_Storage.SaveToFile(savegamePath);
                await m_MetaStorage.SaveToFile(savegameMetaPath);

            s_LocalDiskAccess.Release();
            
            callback?.Invoke();
        }

        private async Task LoadCloudSave(int maxRetries = 3, int maxTimeout = 2, float secBetweenTries = 0.5f)
        {
            m_LoadRemoteMain.Clear();
            m_LoadRemoteMeta.Clear();

            if (!m_CloudSaveActive) return;
            if (m_CloudSaveServerAddress.Equals("") || m_CloudSaveServerPassword.Equals("") || m_CloudSavePlayerIdentifier.Equals("")) return;

            string remoteMain = "";
            string remoteMeta = "";
            bool valuesSet = false;

            string request_url = m_CloudSaveServerAddress + "/loadSavegame?passwd=" + m_CloudSaveServerPassword + "&token=" + m_CloudSavePlayerIdentifier + "&";
            int attempt = 0;
            while (attempt < maxRetries && valuesSet == false)
            {
                UnityWebRequest request = UnityWebRequest.Get(request_url);
                request.timeout = maxTimeout;
                request.SetRequestHeader("Content-Type", "application/json");
                UnityWebRequestAsyncOperation op = request.SendWebRequest();

                while (!op.isDone)
                    await Task.Yield();

                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    attempt++;
                    if (attempt < maxRetries)
                        await Task.Delay((int) (secBetweenTries * 1000f));
                }
                else
                {
                    try
                    {
                        RemoteSaveGame result = JsonUtility.FromJson<RemoteSaveGame>(request.downloadHandler.text);
                        remoteMain = UnityWebRequest.UnEscapeURL(result.savegame);
                        remoteMeta = UnityWebRequest.UnEscapeURL(result.savegameMeta);
                        
                        if (CheckDecryption(remoteMeta, cloudSaveEncryption))
                        {
                            m_LoadRemoteMain.LoadFromJson(EncryptionUtility.Decrypt(remoteMain, cloudSaveEncryption, "fbt"), MigrateSaveVersion);
                            m_LoadRemoteMeta.LoadFromJson(EncryptionUtility.Decrypt(remoteMeta, cloudSaveEncryption, "fbt"), MigrateMetaSaveVersion);
                            return; // success
                        } else { 
                            await CloudSaveScreen.instance.DecryptionFailed(remoteMeta);
                            if(CloudSaveScreen.instance.decryptionAborted)
                            {
                                m_LoadRemoteMain.Clear();
                                m_LoadRemoteMeta.Clear();
                                return; // fail
                            } else if (CheckDecryption(remoteMeta, CloudSaveScreen.instance.decryptionCode))
                            {
                                SetServerEncryption(CloudSaveScreen.instance.decryptionCode);
                                m_LoadRemoteMain.LoadFromJson(EncryptionUtility.Decrypt(remoteMain, cloudSaveEncryption, "fbt"), MigrateSaveVersion);
                                m_LoadRemoteMeta.LoadFromJson(EncryptionUtility.Decrypt(remoteMeta, cloudSaveEncryption, "fbt"), MigrateMetaSaveVersion);
                                return; // success
                            }
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Debug.Log("CloudSave Load error: " + request.downloadHandler.text + " // " + e.Message);
                        attempt++;
                        if (attempt < maxRetries)
                            await Task.Delay((int)(secBetweenTries * 1000f));
                    }
                }
            }
            DialogueManager.instance.notifications.AddLine("CloudSave konnte nicht geladen werden. Es wird der lokale Spielstand verwendet.");
        }

        public static bool CheckDecryption(string metaSavegame, string key)
        {
            try
            {
                string decrypt = EncryptionUtility.Decrypt(metaSavegame, key, "fbt");
                JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypt);
                return true;
            }
            catch (Exception ex)
            {
                // Optional: Hier können Sie etwas mit der Ausnahme machen, wie z. B. sie loggen.
                Debug.Log($"Error during decryption or deserialization: {ex.Message}");
                return false;
            }
        }

        private async Task UploadCloudSave(int maxRetries  =3, int maxTimeout = 2, float secBetweenTries = 0.5f, bool silent = false)
        {
            if (!m_CloudSaveActive) return;
            if (m_CloudSaveServerAddress.Equals("") || m_CloudSaveServerPassword.Equals("") || m_CloudSavePlayerIdentifier.Equals("")) return;

            // Convert save data and metadata to JSON strings
            string savegameData = m_Storage.ToJson();
            string savegameMetaData = m_MetaStorage.ToJson();

            WWWForm form = new WWWForm();
            form.AddField("passwd", m_CloudSaveServerPassword);
            form.AddField("token", m_CloudSavePlayerIdentifier);
            form.AddField("savegame", UnityWebRequest.EscapeURL(EncryptionUtility.Encrypt(savegameData, cloudSaveEncryption, "fbt")));
            form.AddField("savegameMeta", UnityWebRequest.EscapeURL(EncryptionUtility.Encrypt(savegameMetaData, cloudSaveEncryption, "fbt")));
            form.AddField("gameVersion", m_SaveVersion);
            form.AddField("progress", 0);
            form.AddField("playtime", m_MetaStorage.GetInt("playtime", 0));

            int attempt = 0;
            while (attempt < maxRetries)
            {
                using (UnityWebRequest www = UnityWebRequest.Post(m_CloudSaveServerAddress + "/sendSavegame", form))
                {
                    www.timeout = maxTimeout;
                    // Send the request and await the response
                    UnityWebRequestAsyncOperation op = www.SendWebRequest();

                    while (!op.isDone)
                        await Task.Yield();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        attempt++;
                        if (attempt < maxRetries)
                        {
                            await Task.Delay((int)(secBetweenTries * 1000f));
                        }
                    }
                    else
                    {
                        // Handle the response from the server if needed
                        Debug.Log("Cloud save success.");
                        CloudSaveScreen.instance.Success();
                        return;
                    }
                }
            }
            if(!silent)
                _ = CloudSaveScreen.instance.Unreachable();
            EventManager.instance.TriggerInTime(5f, () => { _ = Save(); });
        }

        static Dictionary<string, object> MigrateSaveVersion(Dictionary<string, object> dict)
        {
            //string appVersion = GetData<string>("appVersion", "", dict);
            // perform migration of the save game here, depending on the app version!
            return dict;
        }

        static Dictionary<string, object> MigrateMetaSaveVersion(Dictionary<string, object> dict)
        {
            //string appVersion = GetData<string>("appVersion", "", dict);
            // perform migration of the save game here, depending on the app version!
            return dict;
        }

        public void SetServerAddress(string address = "")
        {
            m_CloudSaveServerAddress = address;
        }
        public void SetCloudSaveStatus(bool useCloudSave)
        {
            m_CloudSaveActive = useCloudSave;
        }
        public void SetServerPassword(string pw = "")
        {
            m_CloudSaveServerPassword = pw;
        }
        public void SetServerIdentifier(string id = "")
        {
            m_CloudSavePlayerIdentifier = id;
        }
        public void SetServerEncryption(string id = "")
        {
            PlayerPrefs.SetString("cloudsave_key", id);
        }
        public void ToggleCloudSaveActive(bool valu)
        {
            m_CloudSaveActive = valu;
        }
    }
}