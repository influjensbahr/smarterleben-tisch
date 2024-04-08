//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckDefaultCursor : VerifyCheckBase
    {
        public override string description => "Default cursor setting";
        public override string longDescription => "This is an OTBT-internal check which makes sure our projects adhere with the needs of our build server.";

        public override void PerformCheckForProject()
        {
            if (PlayerSettings.defaultCursor != null)
                AddFailedCheck("DefaultCursor for the project needs to be null, because our build server breaks otherwise", null, () =>
                {
                    PlayerSettings.defaultCursor = null;
                });
        }

    }
}
#endif