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
    public class VRequiredAttribute : Attribute, IVerifyAttribute
    {
    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            if (value == null)
            {
                // Check if the object is null
                checker.Check(false, $"NullRef: {field.Name} is not assigned", parentObject, category: "VRequired Attribute");
            }
            else if (value is string stringValue)
            {
                // Check if the string is empty
                checker.Check(stringValue.Length > 0, $"StringValue: {field.Name} is empty", parentObject, category: "VRequired Attribute");
            }
            else if (value is ICollection collection)
            {
                // Check if the collection (e.g. List, Array) is not empty and doesn't contain null elements
                bool nonEmpty = collection.Count > 0;
                bool containsNonNull = false;
                foreach (var item in collection)
                {
                    if (item != null)
                    {
                        containsNonNull = true;
                        break;
                    }
                }
                checker.Check(nonEmpty && containsNonNull, $"Collection: {field.Name} is empty or contains null elements", parentObject, category: "VRequired Attribute");
            }
            else if (value is UnityEngine.Object unityObject)
            {
                // Check if Unity Object is not null
                checker.Check(unityObject != null, $"NullRef: Object {field.Name} is not assigned", parentObject, category: "VRequired Attribute");
            }
        }
    #endif
    }
}
