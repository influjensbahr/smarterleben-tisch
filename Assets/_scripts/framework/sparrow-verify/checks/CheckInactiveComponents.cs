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
    public class CheckInactiveComponents : VerifyCheckBase
    {
        public override string description => "Inactive Components";
        public override string longDescription => "Checks for inactive components. Inactive components are not an issue, but you might use this check to find and remove unneccessary components when needed.";

        public override void PerformCheck(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();

            foreach (var component in components)
            {
                if (component == null) continue;

                // Check if the component is disabled
                if (IsComponentEnabled(component)) continue;
                AddFailedCheck($"Inactive {component.GetType().Name} Component", gameObject);
            }
        }
        
        private bool IsComponentEnabled(Component component)
        {
            return component.GetType().GetProperty("enabled")?.GetValue(component) as bool? ?? true;
        }
    }
}
#endif