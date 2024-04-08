//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
using System;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace Sparrow.Verification
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VRangeAttribute : Attribute, IVerifyAttribute
    {
        public float? MinValue { get; }
        public float? MaxValue { get; }

        public VRangeAttribute(float min = float.MinValue, float max = float.MaxValue)
        {
            MinValue = min;
            MaxValue = max;
        }

    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            var rangeAttribute = (VRangeAttribute)Attribute.GetCustomAttribute(field, typeof(VRangeAttribute));
            if (rangeAttribute != null)
            {
                float? minValue = rangeAttribute.MinValue;
                float? maxValue = rangeAttribute.MaxValue;

                if (value is IConvertible) // Checks if value can be converted to a numerical type (int, float, double, etc.)
                {
                    double numericalValue = Convert.ToDouble(value);

                    bool withinMinRange = minValue == null || numericalValue >= minValue;
                    bool withinMaxRange = maxValue == null || numericalValue <= maxValue;

                    bool withinRange = withinMinRange && withinMaxRange;

                    string rangeString = (minValue != null ? $"[{minValue}," : "[?,") + (maxValue != null ? $"{maxValue}]" : "?]");

                    checker.Check(withinRange, $"VRange: {field.Name} is not within the range {rangeString}", parentObject, category: "VRange Attribute");
                }
                else
                {
                    checker.Check(false, $"InvalidType: {field.Name} is not a numerical type", parentObject, category: "VRange Attribute");
                }
            }
        }
    #endif
    }
}