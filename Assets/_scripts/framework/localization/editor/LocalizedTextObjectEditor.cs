//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Utils.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.Localization
{
    [CustomEditor(typeof(LocalizedTextObject))]
    [Serializable]
    public class LocalizedTextObjectEditor : Editor
    {
        private LocalizedTextObject smTarget;

        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            if (smTarget == null) smTarget = target as LocalizedTextObject;

            if (smTarget != null)
                EditorUtils.DrawTinyLogoHeader("Localized Text (ID: " + (smTarget.textID <= 0 ? "-" : smTarget.textID) + ")" + (smTarget.needsProcessing ? "*" : ""));
            else
                EditorUtils.DrawTinyLogoHeader("Localized Text");


            EditorUtils.Space(10);

            LocalizationEditorHelpers.DrawLocalizedTextEditor(smTarget, serializedObject);

            EditorUtils.Space(5);

            GUI.enabled = smTarget.textID > 0;
            if(GUILayout.Button("Rename object"))
            {
                smTarget.RenameFile();
            }
            GUI.enabled = true;

            EditorUtils.DrawVerify(smTarget);
            EditorUtils.EndColoredEditor();
        }
    }
}
