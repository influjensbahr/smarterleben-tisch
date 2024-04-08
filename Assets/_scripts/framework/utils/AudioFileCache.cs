// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using Sparrow.Verification;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    [CreateAssetMenu(menuName = "OTBT/Utils/Audio File Cache", fileName = "New Audio File Cache")]
    public class AudioFileCache : ScriptableObject, IExtendDefaultEditor, IVerify, IPrepareOnBuild
    {

        [SerializeField, VRequired] string m_AudioFolderPath;

        [System.Serializable]
        public class AudioClipEntry
        {
            [SerializeField] public int id;
            [SerializeField] public string path;
        }

        [SerializeField, VRequired, VNonNullElements] List<AudioClipEntry> m_GeneratedEntries = new List<AudioClipEntry>();

        public Dictionary<int, string> m_GeneratedVoiceDictionary = new Dictionary<int, string>();

        private void RefreshDictionary()
        {
            m_GeneratedVoiceDictionary.Clear();
            foreach (var entry in m_GeneratedEntries)
                m_GeneratedVoiceDictionary.Add(entry.id, entry.path);
        }

        public string GetAudio(int id, bool refreshList = true)
        {
            if (m_GeneratedVoiceDictionary.Count == 0) RefreshDictionary();
            if (m_GeneratedVoiceDictionary.ContainsKey(id))
                return m_GeneratedVoiceDictionary[id];
            if (!refreshList) return null;

            RebuildAudioEntries();
            RefreshDictionary();

            if (m_GeneratedVoiceDictionary.ContainsKey(id))
                return m_GeneratedVoiceDictionary[id];
            return null;
        }

        public void RebuildAudioEntries()
        {
#if UNITY_EDITOR
            m_GeneratedEntries.Clear();
            Debug.Log("----- REBUILDING AUDIO CACHE -----");

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new string[] { "Assets/Resources/" + m_AudioFolderPath });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetPath);
                string[] split = fileNameWithoutExtension.Split('_');
                if (split.Length < 2) continue;
                if (int.TryParse(split[split.Length - 1], out int id))
                {
                    m_GeneratedEntries.Add(new AudioClipEntry { id = id, path = assetPath });
                } else
                {
                    Debug.Log("Tried parsing int but failed: >" + split[split.Length - 1] + "<");
                }
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
            RefreshDictionary();
        }

#if UNITY_EDITOR
        public void ExtendDefaultEditor()
        {
            if(GUILayout.Button("Refresh"))
            {
                RebuildAudioEntries();
                RefreshDictionary();
            }
        }


#endif
        public void Verify(CheckVerifyInterface checker)
        {
        }


        public void PrepareOnBuildOrAwake()
        {
            RebuildAudioEntries();
        }
    }
}
