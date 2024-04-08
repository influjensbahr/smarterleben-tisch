//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using System.Reflection;
#endif

namespace Sparrow.Verification
{    
     /// <summary>
     /// Base interface for attributes that are part of the OTBT Verify system. Implement this and the attribute is automatically added to the attribute checks.
     /// </summary>
    public interface IVerifyAttribute 
    {
#if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object fieldObject, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity);
#endif
    }
}
