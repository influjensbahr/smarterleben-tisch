//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
using OTBT.Framework.Core;
using System;
using System.Collections;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace Sparrow.Verification
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VRequiredAtomAttribute : Attribute, IVerifyAttribute
    {
    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            if (value == null)
            {
                // Check if the object is null
                checker.Check(false, $"NullRef: {field.Name} is not assigned", parentObject, category: "VRequiredAtom Attribute");
            }
            else if (value is IStringOrAtomReference atomRef)
            {
                // Check if the string is empty
                checker.Check(atomRef.identifier.Length > 0, $"Atom: {field.Name} is empty", parentObject, category: "VRequiredAtom Attribute");
            }
        }
    #endif
    }
}
