// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 


using OTBT.Framework.Utils.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.UI
{
    [CustomEditor(typeof(FontTypeManager))]
    public class FontTypeManagerEditor : UnityEditor.Editor
    {
        FontTypeManager fontManager = null;
        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            if (fontManager == null) fontManager = target as FontTypeManager;
            if (fontManager.GetComponent<TextMeshProUGUI>() != null)
            {
                EditorGUILayout.HelpBox("This manager should not be added to individual text components. You probably want to replace this with an AdjustFontSize-Component.", MessageType.Error);
                return;
            }

            EditorUtils.DrawLogoHeader("Font Types");


            DrawDefaultInspector();

            EditorUtils.Space();

            EditorGUILayout.LabelField("Font scale: " + fontManager.GetMultiplier());

            GUI.enabled = (Application.isPlaying && fontManager.fontSetting != null);
            EditorUtils.Header("Debug: Increase and decrease at runtime");
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+"))
            {
                fontManager.fontSetting.Increase();
            } 
            if(GUILayout.Button("-"))
            {
                fontManager.fontSetting.Decrease();
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;


            EditorUtils.DrawVerify(fontManager);
            EditorUtils.EndColoredEditor();
        }
    }
}
