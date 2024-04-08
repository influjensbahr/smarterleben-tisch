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
    public class CheckLineEndings : VerifyCheckBase
    {
        public override string description => "Inconsistent line endings";
        public override string longDescription => "Checks for inconsistent line endings in your project.";

        public override void PerformCheck(MonoScript s)
        {
            if(HasInconsistentLineEndings(s.text))
            {
                AddFailedCheck("Inconsitent line endings found", s);
            }
        }

        private bool HasInconsistentLineEndings(string text)
        {
            bool rEndingFound = false;
            bool lEndingFound = false;

            var lines = text.Split(new[] { "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].EndsWith("\r"))
                {
                    if (lEndingFound) return true;
                    rEndingFound = true;
                }
                else if (i != lines.Length - 1)
                {
                    if (rEndingFound) return true;
                    lEndingFound = true;
                }
            }

            return false;
        }

    }
}
#endif