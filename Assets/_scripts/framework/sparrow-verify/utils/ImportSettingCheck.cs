//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Sparrow.Verification
{

    public interface IInterfaceSettingCheck
    {
        public bool DrawEditor();
    }

    public class ImportSettingCheck<T, T2> : IInterfaceSettingCheck
    {
        [SerializeField] bool activateOverride = false;
        [SerializeField] string caption = "";
        [SerializeField] T settingValue;
        [SerializeField] Func<T2, T, bool> checkIfSet;
        [SerializeField] UnityAction<T2, T> applySetting;

        public ImportSettingCheck(string _caption, T _default, Func<T2, T, bool> checker, UnityAction<T2, T> setter)
        {
            caption = _caption;
            settingValue = _default;
        }

        public bool DrawEditor()
        {
            T oldValue = settingValue;
            bool oldOverride = activateOverride;

            // Draw checkbox for activateOverride
            activateOverride = EditorGUILayout.Toggle(caption, activateOverride);

            // If the override is not active, no need to draw further settings
            if (!activateOverride) return (oldOverride == activateOverride);

            // Draw the appropriate editor depending on the type of T
            if (typeof(T) == typeof(int))
            {
                settingValue = (T)(object)EditorGUILayout.IntField((int)(object)settingValue);
            }
            else if (typeof(T) == typeof(float))
            {
                settingValue = (T)(object)EditorGUILayout.FloatField((float)(object)settingValue);
            }
            else if (typeof(T) == typeof(string))
            {
                settingValue = (T)(object)EditorGUILayout.TextField((string)(object)settingValue);
            }
            else if (typeof(T) == typeof(bool))
            {
                settingValue = (T)(object)EditorGUILayout.Toggle((bool)(object)settingValue);
            }
            else if (typeof(T).IsEnum)
            {
                settingValue = (T)(object)EditorGUILayout.EnumPopup((Enum)(object)settingValue);
            }
            else
            {
                EditorGUILayout.HelpBox($"Type {typeof(T)} is not supported", MessageType.Warning);
            }

            return !((activateOverride == oldOverride) && (oldValue.Equals(settingValue)));
        }

        public bool PerformCheck()
        {
            throw new NotImplementedException();
        }
    }
}
#endif