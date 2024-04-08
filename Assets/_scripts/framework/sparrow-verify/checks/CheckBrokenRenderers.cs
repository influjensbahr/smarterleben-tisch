//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckBrokenRenderers : VerifyCheckBase
    {
        public override string description => "Broken renderers";
        public override string longDescription => "Checks for renderers that no longer work, for example due to missing materials.";

        public override void PerformCheckForProject() { }
        public override void PerformCheck(GameObject gameObject)
        {
            var renderers = gameObject.GetComponents<MeshRenderer>();

            foreach (var meshRenderer in renderers)
            {
                if (meshRenderer.sharedMaterials.Length == 0) AddFailedCheck("MeshRenderer has no Materials assigned.", meshRenderer);
                if (HasMissingMaterials(meshRenderer)) AddFailedCheck("One or Multiple Materials Missing on MeshRenderer", meshRenderer);
            }
        }

        bool HasMissingMaterials(MeshRenderer meshRenderer)
        {
            return meshRenderer.sharedMaterials.Any(material => material == null);
        }

        public override void PerformCheck(ScriptableObject sobj)
        {
        }
    }
}
#endif