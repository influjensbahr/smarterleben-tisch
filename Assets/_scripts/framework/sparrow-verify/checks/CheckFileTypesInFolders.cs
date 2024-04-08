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
using Object = UnityEngine.Object;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckFileTypesInFolders : VerifyCheckBase
    {
        public override string description => "File types in folders";
        public override string longDescription =>
            "Checks if specific folders only contain files of a specific type. Use this to make sure your 'music' folder only contains music etc.";

        [Serializable]
        public class SingleFolders
        {
            [SerializeField] public string folderPath;
            [SerializeField] public bool scripts;
            [SerializeField] public bool mp3;
            [SerializeField] public bool wav;
            [SerializeField] public bool useCustomTypes;
            [SerializeField] public string customTypes;
            [SerializeField] public bool checkSubfolders;

            public SingleFolders(string folderPath, bool scripts, bool mp3, bool wav, bool useCustomTypes, string customTypes,
                bool checkSubfolders)
            {
                this.folderPath = folderPath;
                this.scripts = scripts;
                this.mp3 = mp3;
                this.wav = wav;
                this.useCustomTypes = useCustomTypes;
                this.customTypes = customTypes;
                this.checkSubfolders = checkSubfolders;
            }
        }

        [SerializeField] List<SingleFolders> m_Folders = new List<SingleFolders>();
        

        public override bool DrawSpecificProfileEditor()
        {
            bool specificChanges = false;

            for (int i = 0; i < m_Folders.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                m_Folders[i].folderPath = EditorGUILayout.TextField("Folder Path", m_Folders[i].folderPath);
                if (GUILayout.Button("Select Folder", GUILayout.Width(100)))
                {
                    var selectedFolder = EditorUtility.OpenFolderPanel("Select Folder", m_Folders[i].folderPath, "");
                    if (!string.IsNullOrEmpty(selectedFolder))
                    {
                        if (selectedFolder.StartsWith(Application.dataPath))
                            m_Folders[i].folderPath = "Assets" + selectedFolder.Substring(Application.dataPath.Length);
                        else
                        {
                            Debug.LogError("Der ausgewählte Ordner liegt nicht im Projektordner.");
                        }
                        
                        specificChanges = true;
                    }
                    
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                m_Folders[i].checkSubfolders = EditorGUILayout.Toggle("Check Subfolders", m_Folders[i].checkSubfolders);
                m_Folders[i].useCustomTypes = EditorGUILayout.Toggle("Use Custom File Types", m_Folders[i].useCustomTypes);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Allowed File Types:", EditorStyles.boldLabel);
                m_Folders[i].scripts = EditorGUILayout.Toggle("Scripts (.cs)", m_Folders[i].scripts);
                m_Folders[i].mp3 = EditorGUILayout.Toggle("mp3 (.mp3)", m_Folders[i].mp3);
                m_Folders[i].wav = EditorGUILayout.Toggle("wav (.wav)", m_Folders[i].wav);

               
                if (m_Folders[i].useCustomTypes)
                {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Custom File Types (comma-separated):");
                        m_Folders[i].customTypes = EditorGUILayout.TextField(m_Folders[i].customTypes);
                        EditorGUILayout.EndHorizontal();
                        
                        var words = m_Folders[i].customTypes.Split(',').Select(word => word.Trim()).ToArray();
                        foreach (var word in words)
                        {
                            if (string.IsNullOrEmpty(word) || word.Contains(" "))
                            {
                                EditorGUILayout.HelpBox("Custom types must be comma-separated and not empty.", MessageType.Error);
                            }
                        }
                }
                

                if (GUILayout.Button("Remove Folder", GUILayout.Width(120)))
                {
                    m_Folders.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                
                if (GUI.changed)
                {
                    specificChanges = true;
                }
            }

            if (GUILayout.Button("Add Folder"))
            {
                m_Folders.Add(new SingleFolders("", false, false, false, false, "", false));
            }
            
            return specificChanges; 
        }

        public override void PerformCheckForProject()
        {
            MakeCheck();
        }

        private void MakeCheck()
        {
            foreach (var folder in m_Folders)
            {
                var customTypeList = new List<string>();
                if (folder.useCustomTypes)
                {
                    var allowedTypes = folder.customTypes.Split(',').Select(type => type.Trim()).ToArray();
                    customTypeList.AddRange(allowedTypes);
                }
 
                if (folder.wav) customTypeList.Add("wav");
                if (folder.mp3) customTypeList.Add("mp3");
                if (folder.scripts) customTypeList.Add("cs");
                customTypeList.Add("meta");
                
                var path = folder.folderPath;
                if (string.IsNullOrEmpty(path)) return;

                var dir = Directory.GetFiles(path, "*",
                    folder.checkSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (var file in dir)
                {
                    var pos = file.LastIndexOf("\\", StringComparison.Ordinal) + 1;
                    var name = file.Substring(pos, file.Length - pos);
                    
                    if (folder.wav) customTypeList.Add("wav");
                    if (folder.mp3) customTypeList.Add("mp3");
                    if (folder.scripts) customTypeList.Add("cs");
                    customTypeList.Add("meta");
                    var ending = name.Split('.').Last();

                    if (!customTypeList.Contains(ending)) 
                        AddFailedCheck("Wrong File Type!", AssetDatabase.LoadAssetAtPath<Object>(file));
                    
                }
            }
        }
    }
}
#endif