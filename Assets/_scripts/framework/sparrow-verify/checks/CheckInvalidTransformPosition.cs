//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckInvalidTransformPosition : VerifyCheckBase
    {
        public override string description => "Invalid transform positions";
        public override string longDescription => "Checks for transform positions out of the ordinary, which might indicate that something went wrong with the positioning of objects.";

        public override void PerformCheckForProject() { }
        public override void PerformCheck(GameObject gameObject)
        {
            var transform = gameObject.transform;

            if (float.IsNaN(transform.position.x)
                || float.IsNaN(transform.position.y)
                || float.IsNaN(transform.position.z))
                AddFailedCheck("Invalid Transform Position", gameObject);
        }

        public override void PerformCheck(ScriptableObject sobj)
        {
        }
    }
}
#endif
