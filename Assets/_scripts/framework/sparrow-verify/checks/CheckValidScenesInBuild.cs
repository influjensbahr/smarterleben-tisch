//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckValidScenesInBuild : VerifyCheckBase
    {
        public override string description => "Scenes in build settings";
        public override string longDescription => "Checks if all the scenes in your build settings really exist or might have been deleted at some point.";


        public override void PerformCheckForProject()
        {
            var buildScenes = EditorBuildSettings.scenes;

            if(buildScenes.Length == 0)
            {
                AddFailedCheck($"No scenes found in build settings. These should be at least one scene.", null, category: "Scenes in Build Settings").WithSeverity(VerifyResult.Severity.Error);
                return;
            }

            for (int i = 0; i < buildScenes.Length; i++)
            {
                var scene = buildScenes[i];

                if (!scene.enabled)
                {
                    // If you want to check only for enabled scenes in build settings
                    continue;
                }

                if (string.IsNullOrEmpty(scene.path) || !System.IO.File.Exists(scene.path))
                {
                    AddFailedCheck($"Scene at index {i} in build settings is invalid or missing.", null, category: "Scenes in Build Settings");
                }
            }
        }

        public override void PerformCheck(GameObject gameObject) { }

        public override void PerformCheck(ScriptableObject sobj) { }
    }
}
#endif
