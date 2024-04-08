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
    public class CheckDuplicateComponents : VerifyCheckBase
    {
        public override string description => "Duplicate components";
        public override string longDescription => "Checks for duplicate components. Duplicate components are not neccessarily an issue, but you might use this check to scan your project for them to identify issues or optimization potential.";


        public override void PerformCheck(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();

            foreach (var component in components)
            {
                if (component == null) continue;
                if (!HasDuplicates(gameObject, component.GetType())) continue;
                AddFailedCheck($"Duplicate {component.GetType().Name} Component", gameObject);
            }
        }

        bool HasDuplicates(GameObject gameObject, Type type)
        {
            return gameObject.GetComponents(type).Length > 1;
        }
    }
}
#endif