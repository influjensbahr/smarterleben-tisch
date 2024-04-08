//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using OTBT.Framework.Utils.Editor;
using UnityEditor.SceneManagement;

namespace OTBT.Framework.Core
{
    /// <summary>
    /// This is our editor window for all scene checks. It gathers all checks implemented in the project and provides
    /// ways to run these checks
    /// </summary>
    public class SceneSelectWindow : EditorWindow
    {
        int m_SelectedScene = 0;

        [MenuItem("OTBT/Scenes/Open Base Scene")]
        static void OpenBaseScene()
        {
            EditorSceneManager.OpenScene("Assets/_scenes/base.unity");
        }

        [MenuItem("OTBT/Scenes/Open Scene Selection &s")]
        static void OpenSceneSelection()
        {
            var manager = FindObjectOfType<SceneLoadManager>();
            if (manager == null) return;
            Selection.activeObject = manager;
        }


        [MenuItem("OTBT/Scenes/Open scene select window _&E", false, 100)]
        public static void ShowWindow()
        {
            //open or focus
            SceneSelectWindow window = GetWindow<SceneSelectWindow>("Scene selector");
        }
        public void OnGUI()
        {
            this.titleContent = EditorUtils.LabelWithGlyphicon("Scene load", "otbt.png", "OTBT Scene Load System", true);
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawTinyLogoHeader("Scene selector");

            if (Application.isPlaying)
            {
                GUILayout.Label("Not available in play mode.");
            } else
            {
                var objs = FindObjectsOfType(typeof(SceneLoadManager)) as SceneLoadManager[];
                if (objs.Length == 0)
                {
                    EditorGUILayout.HelpBox("It seems like the base scene is not open. Ensure that it's always open!", MessageType.Warning);
                    if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(" Load base scene", "circle-right.png")))
                    {
                        OpenBaseScene();
                    }
                }
                else
                {
                    EditorUtils.Space();
                    m_SelectedScene = SceneLoadManagerEditor.DrawSceneDropdown(objs[0], m_SelectedScene);
                }
            }
            EditorUtils.EndColoredEditor();
        }
    }
}
#endif
