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
    public class VElementCountAttribute : Attribute, IVerifyAttribute
    {
        public int? Min { get; }
        public int? Max { get; }

        public VElementCountAttribute(int min = int.MinValue, int max = int.MaxValue)
        {
            Min = min;
            Max = max;
        }

    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            if (value is ICollection collection)
            {
                int count = collection.Count;

                // Fetch the Min and Max properties from the attribute itself.
                var attribute = (VElementCountAttribute)field.GetCustomAttribute(typeof(VElementCountAttribute));

                // Check if the count of elements in the collection is within the desired range.
                bool isWithinRange = (count >= attribute.Min || attribute.Min == null) && (count <= attribute.Max || attribute.Max == null);

                checker.Check(isWithinRange,
                    $"ElementCount: {field.Name} contains {count} elements, which is not within the specified range",
                    parentObject,
                    category: "VElementCount Attribute");
            }
            else
            {
                checker.Check(false, $"UnsupportedType: {field.Name} is not a collection. VElementCountAttribute can only be applied to collections.", parentObject, category: "VElementCount Attribute");
            }
        }
    #endif
    }
}
