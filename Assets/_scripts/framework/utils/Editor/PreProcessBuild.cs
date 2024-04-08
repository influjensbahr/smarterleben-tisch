//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    public class PreProcessBuild : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                ProcessHierarchy(root.transform, report);
            }
        }

        private void ProcessHierarchy(Transform root, BuildReport report)
        {
            foreach (MonoBehaviour prep in root.GetComponentsInChildren<IPrepareOnBuild>())
            {
                if (prep is IPrepareOnBuild)
                {
                    (prep as IPrepareOnBuild).PrepareOnBuildOrAwake();
                    if (report != null) // this is null when script is executed in play mode on editor
                    {
                        if(prep) EditorUtility.SetDirty(prep);
                    }
                }
            }
        }

    }
}