//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Sparrow.Verification.VerifyResult;

namespace Sparrow.Verification
{

    [CreateAssetMenu(menuName = "Sparrow/Verification/Add Profile", fileName="New Verify Profile")]
    public class VerifyProfile : ScriptableObject
    {
        [Serializable]
        public class SerializedChecks
        {
            public string typeName;
            public string jsonData;
        }

        [SerializeField, Tooltip("URL to an API endpoint that accepts a application/json via POST")] string m_SendToAPIEndpoint = "";
        [SerializeField] string m_Caption = "New Verify Profile";
        [SerializeField] List<SerializedChecks> m_SerializedChecks = new List<SerializedChecks>();
        [SerializeField] string m_LogLocation = "./verificationLog.txt";
        [SerializeField] bool m_RunOnBuild = false;
        [SerializeField] bool m_FailBuildOnError = false;
        [SerializeField] CheckType m_ScanType = CheckType.CurrentScene;

        public string logLocation => m_LogLocation;
        public bool autoCheckBuild => m_RunOnBuild;
        public bool autoFailBuild => m_FailBuildOnError;
        public CheckType checkType => m_ScanType;

        public string apiEndpoint => m_SendToAPIEndpoint;
        public string caption => m_Caption;
        public List<SerializedChecks> serializedChecks => m_SerializedChecks;

#if UNITY_EDITOR
        List<VerifyCheckBase> m_ChecksList = new List<VerifyCheckBase> ();

        public List<VerifyCheckBase> checks => m_ChecksList;

        private void OnValidate()
        {
            UpdateCheckList();
            foreach(VerifyCheckBase check in checks) 
                if(check is CheckAttributes)
                    (check as CheckAttributes).OnValidate(this);
        }

        public void SetCaption(string cap)
        {
            m_Caption = cap;
            Save();
        }

        public void SetApiEndpoint(string api)
        {
            m_SendToAPIEndpoint = api;
            Save();
        }

        public void Load()
        {
            foreach (var serializedCheck in serializedChecks)
            {
                var type = Type.GetType(serializedCheck.typeName);
                if (type == null) continue;
                var check = JsonUtility.FromJson(serializedCheck.jsonData, type) as VerifyCheckBase;
                m_ChecksList.Add(check);
            }
        }

        public void Save()
        {
            serializedChecks.Clear();
            foreach (var check in m_ChecksList)
            {
                var typeName = check.GetType().AssemblyQualifiedName;
                var jsonData = JsonUtility.ToJson(check);
                serializedChecks.Add(new SerializedChecks { typeName = typeName, jsonData = jsonData });
            }
            EditorUtility.SetDirty(this);
        }

        public void UpdateCheckList()
        {
            m_ChecksList.Clear();
            Load();
            // Get all non-abstract types that are derived from VerifyCheckBase
            var types = Assembly.GetAssembly(typeof(VerifyCheckBase))
                                .GetTypes()
                                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(VerifyCheckBase)))
                                .ToList();

            foreach (var type in types)
            {
                // Check if we already have a check of this type in our list
                if (!m_ChecksList.Any(check => check.GetType() == type))
                {
                    VerifyCheckBase instance = (VerifyCheckBase)Activator.CreateInstance(type);
                    m_ChecksList.Add(instance);
                } 
            }
            m_ChecksList.Sort((a, b) => a.description.CompareTo(b.description));
            Save();
        }
#endif
    }
}