//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Reflection;
#endif
using UnityEngine;

namespace Sparrow.Verification
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VRequireChildObjectAttribute : Attribute, IVerifyAttribute
    {
        public bool IncludeSelf { get; }

        public VRequireChildObjectAttribute(bool includeSelf = false)
        {
            IncludeSelf = includeSelf;
        }

    #if UNITY_EDITOR
        public void PerformCheck(VerifyCheckBase checker, object value, FieldInfo field, UnityEngine.Object parentObject, VerifyResult.Severity severity)
        {
            if (value == null) return;
            // Get the custom attribute and extract the IncludeSelf property
            VRequireChildObjectAttribute attribute = field.GetCustomAttribute<VRequireChildObjectAttribute>();
            bool includeSelf = attribute?.IncludeSelf ?? false;


            // Handle MonoBehaviour and GameObject
            if (value is GameObject goValue)
            {
                CheckTransformChild(checker, goValue.transform, field, parentObject, includeSelf);
            }
            else if (value is MonoBehaviour mbValue)
            {
                CheckTransformChild(checker, mbValue.transform, field, parentObject, includeSelf);
            }
            // Handle List and LinkedList
            else if (value is IEnumerable<object> enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is GameObject itemAsGO)
                    {
                        CheckTransformChild(checker, itemAsGO.transform, field, parentObject, includeSelf);
                    }
                    else if (item is MonoBehaviour itemAsMB)
                    {
                        CheckTransformChild(checker, itemAsMB.transform, field, parentObject, includeSelf);
                    }
                }
            }
            // Handle other collections like dictionaries and stacks
            // Similar to the above handling for List and LinkedList
        }

        private static void CheckTransformChild(VerifyCheckBase checker, Transform child, FieldInfo field, UnityEngine.Object parentObject, bool includeSelf)
        {
            if (parentObject is GameObject parentGameObject)
            {
                Transform parentTransform = parentGameObject.transform;

                bool isChild = includeSelf ? child == parentTransform || child.IsChildOf(parentTransform) : child.IsChildOf(parentTransform);

                checker.Check(isChild,
                    $"MustBeChild: Object {child.name} assigned to {field.Name} is not a child of {parentGameObject.name}",
                    parentObject,
                    category: "VMustBeChild Attribute");
            }
        }
    #endif
    }
}