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
    public class VCustomValidationAttribute : Attribute, IVerifyAttribute
    {

        private readonly Type _validationType;
        private readonly string _validationMethodName;

        public VCustomValidationAttribute(Type validationType, string validationMethodName)
        {
            _validationType = validationType;
            _validationMethodName = validationMethodName;
        }

    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            // Get the custom attribute
            VCustomValidationAttribute attribute = (VCustomValidationAttribute)Attribute.GetCustomAttribute(field, typeof(VCustomValidationAttribute));

            if (attribute != null)
            {
                // Get the type and method name
                Type validationType = attribute._validationType;
                string validationMethodName = attribute._validationMethodName;

                // Get the method from the type
                MethodInfo method = validationType.GetMethod(validationMethodName, BindingFlags.Static | BindingFlags.Public);

                if (method != null)
                {
                    // Invoke the method with the value to be validated
                    bool isValid = (bool)method.Invoke(null, new object[] { value });

                    // Report the result
                    checker.Check(isValid, $"CustomValidation: {field.Name} failed custom validation", parentObject, category: "VCustomValidation Attribute");
                }
                else
                {
                    checker.Check(false, $"CustomValidation: Validation method not found for {field.Name}", parentObject, category: "VCustomValidation Attribute");
                }
            }
        }
    #endif
    }
}