//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace Sparrow.Verification
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VUniqueElementsAttribute : Attribute, IVerifyAttribute
    {
    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            // Check if the value is a collection
            if (value is ICollection collection)
            {
                // Use a HashSet to keep track of unique elements
                HashSet<object> uniqueElements = new HashSet<object>();
                bool hasDuplicates = false;

                // Iterate through each element in the collection
                foreach (var element in collection)
                {
                    // If the element is already in the HashSet, it's a duplicate
                    if (!uniqueElements.Add(element))
                    {
                        hasDuplicates = true;
                        break;
                    }
                }

                // If duplicates are found, report it using the checker
                checker.Check(!hasDuplicates, $"Collection: {field.Name} contains duplicate elements", parentObject, category: "VUniqueElements Attribute" );
            }
        }
    #endif
    }
}