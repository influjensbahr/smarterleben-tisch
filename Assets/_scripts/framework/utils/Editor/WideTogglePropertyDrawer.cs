// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using UnityEditor;
using UnityEngine;
namespace OTBT.Framework
{
    [CustomPropertyDrawer(typeof(WideToggleAttribute))]
    public class WideTogglePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                EditorGUI.LabelField(position, label.text, "Use WideToggle with boolean.");
                return;
            }
            property.boolValue = EditorGUI.ToggleLeft(position, property.displayName, property.boolValue);
        }
    }
}
