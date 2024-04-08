//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.UI
{
    [CustomEditor(typeof(UIScreen), true), CanEditMultipleObjects]
    public class UIScreenEditor : Editor
    {
        bool m_ShowEvents = false;
        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader("UI Screen", "https://wiki.beatentrack.games/doc/ui-58hkPeqDvy#h-defining-and-switching-between-ui-screens");
            UIScreen screen = (UIScreen)target;

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(50)))
                {
                    if (GUILayout.Button("Show", EditorStyles.miniButtonLeft))
                        UIScreenController.instance.ShowScreen(screen.title);

                    if (GUILayout.Button("Hide", EditorStyles.miniButtonRight))
                        UIScreenController.instance.HideScreen(screen.title);
                }
            }

            screen.title.ShowGUI(screen, "Screen Title");
            if (!screen.title.isEmpty)
            {
                var targetName = $"Screen - {screen.title}";
                using (new EditorGUI.DisabledScope(targetName.Equals(screen.gameObject.name)))
                {
                    if (GUILayout.Button($"Rename to \"{targetName}\""))
                    {
                        Undo.RecordObject(screen.gameObject,
                            "Rename Screen");
                        screen.gameObject.name = targetName;
                    }
                }
            }

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject,
                "onBeforeShow",
                "onBeforeHide",
                "onCompleteShow",
                "onCompleteHide",
                "m_DisableGameObjectWhenHidden",
                "m_DisableCanvasWhenHidden",
                "m_DisableRaycasterWhenHidden",
                "m_StretchToScreenSpace",
                "m_OnStartInstantHide");

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("On Hide",EditorStyles.centeredGreyMiniLabel);
                serializedObject.DrawPropertyField("m_DisableGameObjectWhenHidden");
                serializedObject.DrawPropertyField("m_DisableCanvasWhenHidden");
                serializedObject.DrawPropertyField("m_DisableRaycasterWhenHidden");
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("On Start",EditorStyles.centeredGreyMiniLabel);
                serializedObject.DrawPropertyField("m_StretchToScreenSpace");
                serializedObject.DrawPropertyField("m_OnStartInstantHide");
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                m_ShowEvents = EditorGUILayout.Foldout(m_ShowEvents, "Events");
                if (m_ShowEvents)
                {
                    EditorGUILayout.LabelField("Show Events", EditorStyles.boldLabel);
                    serializedObject.DrawPropertyField("onBeforeShow");
                    serializedObject.DrawPropertyField("onCompleteShow");
                    EditorGUILayout.LabelField("Hide Events", EditorStyles.boldLabel);
                    serializedObject.DrawPropertyField("onBeforeHide");
                    serializedObject.DrawPropertyField("onCompleteHide");
                }
            }


            serializedObject.ApplyModifiedProperties();

            EditorUtils.DrawVerify(screen);
            EditorUtils.EndColoredEditor();
        }
    }
}
