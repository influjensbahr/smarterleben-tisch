
//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//


#if UNITY_EDITOR
using UnityEditor;
using OTBT.Framework.Utils.Editor;

namespace OTBT.Framework.Core
{
    [CustomEditor(typeof(SceneCollection))]
    public class SceneCollectionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader("Scene collection", "https://wiki.beatentrack.games/doc/core-3FeByn3Zuj#h-scene-management", true);

            SceneCollection col = (SceneCollection)target;
            col.objectRef.ShowGUI(col, "Scene title");

            base.OnInspectorGUI();

            EditorUtils.DrawVerify(col);
            EditorUtils.EndColoredEditor();
        }
    }
}
#endif