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
using UnityEngine;

namespace Sparrow.Verification
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VRequireComponentAttribute : Attribute, IVerifyAttribute
    {
        private readonly Type requiredComponentType;

        public VRequireComponentAttribute(Type requiredComponentType)
        {
            this.requiredComponentType = requiredComponentType;
        }

    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            GameObject gameObject = null;

            if (value is MonoBehaviour monoBehaviour)
            {
                gameObject = monoBehaviour.gameObject;
            }
            else if (value is GameObject go)
            {
                gameObject = go;
            }

            if (gameObject != null)
            {
                // Get the custom attribute
                VRequireComponentAttribute attribute = (VRequireComponentAttribute)Attribute.GetCustomAttribute(field, typeof(VRequireComponentAttribute));

                // Check if the required component is attached to the GameObject
                var requiredComponent = gameObject.GetComponent(attribute.requiredComponentType);
                checker.Check(requiredComponent != null, $"RequireComponent: Field {field.Name} requires GameObject with {attribute.requiredComponentType.Name} component attached", parentObject, category: "VRequireComponent Attribute");
            }
        }
    #endif
    }
}
