//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using UnityEngine;
using System;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckEmptyPrefabs : VerifyCheckBase
    {
        public override string description => "Empty Prefabs";
        public override string longDescription => "Checks for empty prefabs in your project.";

        public override void PerformCheck(GameObject gameObject)
        {
            if(IsPrefab(gameObject))
            {
                if (gameObject.GetComponents<Component>().Length == 1 && gameObject.transform.childCount == 0)
                {
                    AddFailedCheck("Empty Prefab Asset found", gameObject);
                }
            }
        }

    }
}
#endif