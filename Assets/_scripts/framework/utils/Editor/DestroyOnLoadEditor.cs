//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Utils.Editor;
using UnityEditor;

namespace OTBT.Framework.Utils
{
    [CustomEditor(typeof(DestroyOnLoad))]
    public class DestroyOnLoadEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader("This object is destroyed on load", small: true);
            EditorGUILayout.HelpBox("This object is destroyed once the game starts, thus it's probably only relevant for development purposes", MessageType.Info);
            EditorUtils.EndColoredEditor();
        }
    }
}