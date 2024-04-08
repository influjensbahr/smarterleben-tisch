//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckMissingScripts : VerifyCheckBase
    {
        public override string description => "Missing scripts";
        public override string longDescription => "Checks for references to missing scripts on GameObjects. This happens when you delete a script file, but the script is still referenced somewhere in the project.";


        public override void PerformCheck(GameObject o)
        {
            foreach (Component c in o.GetComponents<Component>())
                if (c == null)
                    AddFailedCheck("Missing script", o);
        }
    }
}
#endif
