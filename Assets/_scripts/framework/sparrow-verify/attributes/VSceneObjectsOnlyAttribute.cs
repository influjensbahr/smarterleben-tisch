//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
using UnityEditor;
using System;
#if UNITY_EDITOR
using System.Reflection;
#endif
using System.Collections.Generic;

namespace Sparrow.Verification
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VSceneObjectsOnlyAttribute : Attribute, IVerifyAttribute
    {
    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            if (value == null) return;

            // Handle individual Unity Object
            if (value is UnityEngine.Object unityObject)
            {
                CheckUnityObject(checker, unityObject, field, parentObject);
            }
            // Handle List and LinkedList
            else if (value is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is UnityEngine.Object itemAsUnityObject)
                    {
                        CheckUnityObject(checker, itemAsUnityObject, field, parentObject);
                    }
                }
            }
            // Handle Dictionary
            else if (value is System.Collections.IDictionary dictionary)
            {
                foreach (var item in dictionary.Values)
                {
                    if (item is UnityEngine.Object itemAsUnityObject)
                    {
                        CheckUnityObject(checker, itemAsUnityObject, field, parentObject);
                    }
                }
            }
            // Handle Stack
            else if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(Stack<>))
            {
                foreach (var item in (System.Collections.IEnumerable)value)
                {
                    if (item is UnityEngine.Object itemAsUnityObject)
                    {
                        CheckUnityObject(checker, itemAsUnityObject, field, parentObject);
                    }
                }
            }
        }

        private static void CheckUnityObject(VerifyCheckBase checker, UnityEngine.Object unityObject, FieldInfo field, UnityEngine.Object parentObject)
        {
            if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                if (unityObject == null)
                {
                    checker.Check(false, $"VSceneObjectsOnly: {field.Name} is not assigned",
                        parentObject,
                        category: "VSceneObjectsOnly Attribute");
                }
                else
                {
                    string assetPath = AssetDatabase.GetAssetPath(unityObject);
                    bool isSceneObject = string.IsNullOrEmpty(assetPath) && unityObject;

                    checker.Check(isSceneObject, $"VSceneObjectsOnly: {field.Name} is not a scene object",
                        parentObject,
                        category: "VSceneObjectsOnly Attribute");
                }
            }
        }
    #endif
    }
}
