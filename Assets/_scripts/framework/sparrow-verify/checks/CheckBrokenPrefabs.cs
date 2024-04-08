//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckBrokenPrefabs : VerifyCheckBase
    {
        public override string longDescription => "Checks for prefabs that are missing, but still referenced within a scene or another prefab.";
        public override string description => "Missing Prefab Assets";


        public override void PerformCheck(GameObject gameObject)
        {
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(gameObject)) return;
            if (PrefabUtility.GetPrefabInstanceStatus(gameObject) != PrefabInstanceStatus.MissingAsset) return;

            AddFailedCheck("Missing Prefab Asset", gameObject);
        }
    }
}
#endif