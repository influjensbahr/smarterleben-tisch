// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 

using OTBT.Framework.Utils.Editor;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
namespace OTBT.Framework.Core
{
    public class SaveGameEditorWindow : EditorWindow
    {
        static Dictionary<string, object> s_Data = new();
        Vector2 m_ScrollPos = Vector2.zero;
        string m_Filter = String.Empty;

        [MenuItem("OTBT/Save Games/Save Game Inspector")]
        static void ShowWindow()
        {
            var window = GetWindow<SaveGameEditorWindow>();
            var title = new GUIContent(EditorGUIUtility.IconContent("d_Profiler.NetworkOperations"));
            title.text = "Save Game Inspector";
            window.titleContent = title;
            window.Show();
        }

        void DrawForData(Dictionary<string, object> data)
        {
            if (data == null) return;

            GUILayout.BeginVertical();
            m_Filter = EditorGUILayout.TextField(m_Filter, EditorStyles.toolbarSearchField);
            if (data.Count > 0)
            {
                using var scope = new EditorGUILayout.ScrollViewScope(m_ScrollPos, GUILayout.ExpandHeight(true));

                m_ScrollPos = scope.scrollPosition;
                foreach (var keyValuePair in data)
                {
                    if (!string.IsNullOrWhiteSpace(m_Filter) && !keyValuePair.Key.Contains(m_Filter, StringComparison.InvariantCultureIgnoreCase)) continue;

                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Height(24)))
                    {
                        EditorGUILayout.SelectableLabel(keyValuePair.Key);
                        EditorGUILayout.SelectableLabel(keyValuePair.Value.ToString(), EditorStyles.miniLabel);
                    }
                }
            }

            EditorGUILayout.LabelField($"{s_Data.Count} Keys are set in Metadata", EditorStyles.toolbar);
            GUILayout.EndVertical();
           
        }

        void OnGUI()
        {
            EditorUtils.BeginColoredEditor();
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Saves can only be seen in Play Mode.", MessageType.Info);
                EditorUtils.EndColoredEditor();
                return;
            }

            if (!SaveGame.exists) return;

            EditorGUILayout.BeginHorizontal();
            DrawForData(SaveGame.metadata.internalData);
            DrawForData(SaveGame.data.internalData);
            EditorGUILayout.EndHorizontal();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save", EditorStyles.miniButtonLeft)) SaveSaveGame();
                if (GUILayout.Button("Load", EditorStyles.miniButtonMid)) LoadSaveGame();
                if (GUILayout.Button("Delete", EditorStyles.miniButtonRight)) DeleteSaveGame();

            }
            EditorUtils.EndColoredEditor();
        }
        async void SaveSaveGame()
        {
            await SaveGame.instance.Save();
        }
        async void LoadSaveGame()
        {
            await SaveGame.instance.Load();
        }

        [MenuItem("OTBT/Save Games/Delete Save Game")]
        static void DeleteSaveGame()
        {
            s_Data = null;
            _ = SaveGame.instance.DeleteSave(null);
        }
    }
}
