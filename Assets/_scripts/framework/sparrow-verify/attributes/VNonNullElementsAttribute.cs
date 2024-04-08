//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
using System;
using System.Collections;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace Sparrow.Verification
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VNonNullElementsAttribute : Attribute, IVerifyAttribute
    {
    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            if (value is ICollection collection)
            {
                // Check if the collection contains null elements
                bool containsNonNull = true;
                foreach (var item in collection)
                {
                    if (item == null)
                    {
                        containsNonNull = false;
                        break;
                    }
                }
                checker.Check(containsNonNull, $"NullElement: {field.Name} contains null elements", parentObject, category: "VNonNullElements Attribute");
            }
            else
            {
                checker.Check(false, $"InvalidType: {field.Name} is not a collection type", parentObject, category: "VNonNullElements Attribute");
            }
        }
    #endif
    }
}