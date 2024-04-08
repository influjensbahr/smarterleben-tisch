//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
using System;
#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
#endif

namespace Sparrow.Verification
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VDisallowTypeAttribute : Attribute, IVerifyAttribute
    {
        private readonly Type[] disallowedTypes;
        public VDisallowTypeAttribute(params Type[] disallowedTypes)
        {
            this.disallowedTypes = disallowedTypes;
        }

    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            if (value == null) return;

            // Retrieve the attribute from the FieldInfo
            var attribute = (VDisallowTypeAttribute)field.GetCustomAttribute(typeof(VDisallowTypeAttribute));
            if (attribute != null)
            {
                bool isOfDisallowedType = attribute.disallowedTypes.Any(type => type.IsAssignableFrom(value.GetType()));
                checker.Check(!isOfDisallowedType, $"InvalidType: {field.Name} is of a disallowed type", parentObject, category: "VDisallowType Attribute");
            }
        }
    #endif
    }
}
