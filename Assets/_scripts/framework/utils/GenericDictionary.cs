//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using Newtonsoft.Json;
using OTBT.Framework.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// A glorified key-value store which can hold various kinds of objects and can be serialized to JSON. Good as base for our save system.
    /// </summary>
    /// 

    [System.Serializable]
    public class SerializableList<T>
    {
        public T enumerable;
    }
    public class GenericDictionary 
    {
        Dictionary<string, object> m_InternalData = new Dictionary<string, object>();
        float m_LastChange = 0f;

        public Dictionary<string, object> internalData => m_InternalData;

        public float lastChange => m_LastChange;

        public void Clear()
        {
            m_InternalData.Clear();
            m_LastChange = Time.time;
        }

        public void SetToDictionary(Dictionary<string, object> loadingDict)
        {
            if (loadingDict == null) return;
            m_InternalData.Clear();
            foreach (string k in loadingDict.Keys)
                m_InternalData.Add(k, loadingDict[k]);
        }

        public void SetToDictionary(GenericDictionary loadingDict)
        {
            m_InternalData.Clear();
            foreach (string k in loadingDict.internalData.Keys)
                m_InternalData.Add(k, loadingDict.internalData[k]);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(m_InternalData, Formatting.None);
        }

        public void LoadFromJson(string json, Func<Dictionary<string, object>, Dictionary<string, object>> transform)
        {
            Dictionary<string, object> loadedDictionary = new Dictionary<string, object>();
            // try to parse dictionary
            try
            {
                loadedDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }
            catch (Exception e)
            {
                Dbg.Exception(null, e);
                loadedDictionary = new Dictionary<string, object>();
            }

            if(loadedDictionary == null) loadedDictionary = new Dictionary<string, object>();
            if (!loadedDictionary.ContainsKey("lastChange"))    
                loadedDictionary.Add("lastChange", 0f);
            SetToDictionary(transform(loadedDictionary));
        }

        public async Task LoadFromFile(string fileToLoad, Func<Dictionary<string, object>, Dictionary<string, object>> transform)
        {
            if (PlayerPrefs.GetInt("failsave_playerprefs", -1) >= 0)
            {
                LoadFromPlayerPrefs(transform);
                return;
            }
            try
            {
                Dictionary<string, object> loadedDictionary = new Dictionary<string, object>();
                if (File.Exists(fileToLoad))
                {
                    using (var fs = File.Open(fileToLoad, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            string json = await reader.ReadToEndAsync();

                            // try to parse dictionary
                            try
                            {
                                loadedDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                            }
                            catch (Exception e)
                            {
                                Dbg.Exception(null, e);
                                loadedDictionary = new Dictionary<string, object>();
                            }

                            if (!loadedDictionary.ContainsKey("lastChange"))
                                loadedDictionary.Add("lastChange", 0f);

                            reader.Close();
                        }
                        fs.Close();
                    }
                }
                SetToDictionary(transform(loadedDictionary));
            } catch(UnauthorizedAccessException uae)
            {
                Dbg.Exception(null, uae);
                LoadFromPlayerPrefs(transform);
                return;
            }
        }

        public async Task SaveToFile(string fileToSave)
        {
            bool silent = (PlayerPrefs.GetInt("failsave_playerprefs", -1) >= 0);
            try
            {
                if (!Directory.Exists(Application.persistentDataPath))
                    Directory.CreateDirectory(Application.persistentDataPath);

                using (var fs = File.Open(fileToSave, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        string json = ToJson();
                        await writer.WriteAsync(json);
                    }
                }

                // saving worked fine this time, turn off failsave mode
                if (silent) MatomoConnection.instance.TrackEvent("save_failsave_restored");
                PlayerPrefs.SetInt("failsave_playerprefs", -1);
            } catch(UnauthorizedAccessException e)
            {
                if(!silent) MatomoConnection.instance.TrackEvent("save_failsave_activated_unauthorized");
                SaveToPlayerPrefs();
                Debug.Log("Exception: " + e.Message);
            } catch(IOException r)
            {
                if(!silent) MatomoConnection.instance.TrackEvent("save_failsave_activated_ioexception");
                SaveToPlayerPrefs();
                Debug.Log("Exception: " + r.Message);
            }
        }

        public void SaveToPlayerPrefs()
        {
            string json = ToJson();
            int chunkSize = 1024;
            int chunks = Mathf.CeilToInt((float)json.Length / chunkSize);
            PlayerPrefs.SetInt("failsave_playerprefs", chunks);

            for (int i = 0; i < chunks; i++)
            {
                int start = i * chunkSize;
                int end = Mathf.Min(start + chunkSize, json.Length);
                string chunkData = json.Substring(start, end - start);
                PlayerPrefs.SetString($"failsave_chunk_{i}", chunkData);
            }
            PlayerPrefs.Save(); 
        }

        public void LoadFromPlayerPrefs(Func<Dictionary<string, object>, Dictionary<string, object>> transform)
        {
            int chunks = PlayerPrefs.GetInt("failsave_playerprefs", -1);
            if (chunks == -1)
            {
                PlayerPrefs.SetInt("failsave_playerprefs", 0);
                return;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < chunks; i++)
            {
                string chunkData = PlayerPrefs.GetString($"failsave_chunk_{i}", string.Empty);
                if (string.IsNullOrEmpty(chunkData))
                {
                    Dbg.Error(null, $"Failed to load chunk {i} from PlayerPrefs.");
                    return;
                }
                sb.Append(chunkData);
            }
            string json = sb.ToString();
            Dictionary<string, object> loadedDictionary = new Dictionary<string, object>();

            try
            {
                loadedDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }
            catch (Exception e)
            {
                Dbg.Exception(null, e);
                loadedDictionary = new Dictionary<string, object>();
            }

            if (!loadedDictionary.ContainsKey("lastChange"))
                loadedDictionary.Add("lastChange", 0f);

            SetToDictionary(transform(loadedDictionary));
        }

        // ------------
        // generic delete, set and get

        static T GetData<T>(string key, T defaultValue, Dictionary<string, object> dict)
        {

            if (dict.ContainsKey(key))
            {
                if (typeof(T).Equals(dict[key].GetType())) return (T)dict[key];
                return JsonConvert.DeserializeObject<T>(dict[key].ToString());
            }
            return defaultValue;
        }

        T GetData<T>(string key, T defaultValue)
        {
            return GetData(key, defaultValue, m_InternalData);
        }

        static void SetData<T>(string key, T value, Dictionary<string, object> dict)
        {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }

        void SetData<T>(string key, T value)
        {

            SetData(key, value, m_InternalData);
            m_LastChange = Time.time;
        }

        public void DeleteKey(string key)
        {
            m_InternalData.Remove(key);
        }

        // ------------
        // type-specific variants

        public T GetStruct<T>(string key, T defaultValue) where T : struct
        {
            return JsonUtility.FromJson<T>(GetData(key, JsonConvert.SerializeObject(defaultValue)));
        }

        public bool HasKey(string key)
        {
            return m_InternalData.ContainsKey(key);
        }

        public T GetObject<T>(string key, T defaultValue) where T : class
        {
            
            if (m_InternalData.ContainsKey(key))
            {
                return JsonConvert.DeserializeObject<T>(GetData(key,""));
            }
            return defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return GetData(key, defaultValue);
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            return GetData(key, defaultValue);
        }
        
        public Vector2 GetVector2(string key, Vector2 defaultValue = default)
        {
            return GetData(key, defaultValue);
        }
        
        public Transform GetTransform(string key, Transform defaultValue = default)
        {
            return GetData(key, defaultValue);
        }

        public string GetString(string key, string defaultValue = "")
        {
            return GetData(key, defaultValue);
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return GetData(key, defaultValue);
        }

        public void SetObject<T>(string key, T c) where T : class
        {
            SetData(key, JsonConvert.SerializeObject(c));
        }
        public void SetStruct<T>(string key, T c) where T : struct
        {
            SetData(key, JsonUtility.ToJson(c));
        }

        public void SetInt(string key, int value)
        {
            SetData(key, value);
        }

        public void SetFloat(string key, float value)
        {
            SetData(key, value);
        }
        
        public void SetVector2(string key, Vector2 value)
        {
            SetData(key, value);
        }
        
        public void SetTransform(string key, Transform value)
        {
            SetData(key, value);
        }

        public void SetString(string key, string value)
        {
            SetData(key, value);
        }

        public void SetBool(string key, bool value)
        {
            SetData(key, value);
        }
    }
}
