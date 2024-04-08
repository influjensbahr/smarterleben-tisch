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
    public class CheckBrokenShaders : VerifyCheckBase
    {
        public override string description => "Shader compile errors";
        public override string longDescription => "Checks for shaders with compile errors.";

        public override void PerformCheck(Shader s) {
            if (ShaderUtil.ShaderHasError(s)) AddFailedCheck($"Shader {s.name} has Compilation Errors", s);
        }
    }
}
#endif