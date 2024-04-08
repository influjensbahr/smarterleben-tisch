//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEngine;
using OTBT.Framework.Utils.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace OTBT.Framework.Core
{
    [CustomEditor(typeof(SceneLoadManager))]
    public class SceneLoadManagerEditor : Editor
    {
 
        int m_SelectedScene;
        SceneLoadManager m_Manager;

        public static int DrawSceneDropdown(SceneLoadManager manager, int selectedScene)
        {
            int collectionToLoad = 0;
            using (new GUILayout.HorizontalScope())
            {
                collectionToLoad = DrawSceneDropdownInternal(manager, selectedScene);

                var managerScene = (collectionToLoad < 0 ||collectionToLoad >= manager.scenes.Count) ? null : manager.scenes[collectionToLoad];

                GUI.enabled = managerScene != null;
                if (GUILayout.Button("Switch to Scene"))
                {
                    LoadScenes(managerScene, manager, selectedScene);
                    return collectionToLoad;
                }
                GUI.enabled = true;
            }
            return collectionToLoad;
        }

        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader("Scene Manager", "https://wiki.beatentrack.games/doc/core-3FeByn3Zuj#h-scene-management");

            m_Manager = target as SceneLoadManager;
            if (m_Manager != null)
            {

                m_SelectedScene = DrawSceneDropdown(m_Manager, m_SelectedScene);

                EditorUtils.Separator(1);

                base.OnInspectorGUI();

                EditorUtils.DrawVerify(m_Manager);
            }
            EditorUtils.EndColoredEditor();
        }

        static void LoadScenes(SceneCollection collectionToLoad, SceneLoadManager manager, int selectedScene)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            var loadedFirstScene = false; //to make sure additional scenes are loaded additively
            if (!manager.baseScene.IsEmpty)
            {
                EditorSceneManager.OpenScene(manager.baseScene.ScenePath);
                loadedFirstScene = true;
            }

            if (!collectionToLoad.sceneTitle.Equals("null")) //why do we do this?
            {
                bool setSceneActive = false;
                foreach (SceneReference scene in collectionToLoad.scenesToLoad)
                {
                    if (scene.IsEmpty) continue;
                    var sceneObj = EditorSceneManager.OpenScene(scene.ScenePath, loadedFirstScene ? OpenSceneMode.Additive : OpenSceneMode.Single);
                    if (!setSceneActive)
                    {
                        EditorSceneManager.SetActiveScene(sceneObj);
                        setSceneActive = true;
                    }
                    loadedFirstScene = true;
                }
            } else
            {
                Debug.LogError($"Scene definition for {selectedScene} not found.");
            }
        }

        static int DrawSceneDropdownInternal(SceneLoadManager manager, int selectedScene)
        {
            if (manager.scenes.Count == 0) return -1;
            var options = new string[manager.scenes.Count];
            for (int i = 0; i < manager.scenes.Count; i++)
            {
                options[i] = manager.scenes[i] == null ? string.Empty : manager.scenes[i].sceneTitle;
            }
            selectedScene = EditorGUILayout.Popup(selectedScene, options);
            return selectedScene;
        }
    }
}
