//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr 
//

#if UNITY_EDITOR
using System;
using UnityEditor;
using OTBT.Framework.Utils.Editor;
using UnityEngine;

namespace OTBT.Framework.Core
{
    [CustomEditor(typeof(GameSetting))]
    public class GameSettingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader("Game Setting", "https://wiki.beatentrack.games/doc/core-3FeByn3Zuj#h-game-settings");

            var enumTypeIndex = serializedObject.FindProperty("m_EnumTypeIndex");

            string[] enumTypes = new string[GameSetting.EnumTypes.Length];
            for (int i = 0; i < GameSetting.EnumTypes.Length; i++)
                enumTypes[i] = GameSetting.EnumTypes[i].Name;

            enumTypeIndex.intValue = EditorGUILayout.Popup("Type", enumTypeIndex.intValue, enumTypes);
            var enumType = GameSetting.EnumTypes[enumTypeIndex.intValue];
            if (enumType != null)
            {
                var defaultValue = serializedObject.FindProperty("m_DefaultValue");

                string[] enumNames = Enum.GetNames(enumType);
                defaultValue.intValue = EditorGUILayout.Popup("Default Value", defaultValue.intValue, enumNames);
            }
            serializedObject.ApplyModifiedProperties();

            GameSetting gameSetting = (GameSetting)target;
            EditorGUILayout.LabelField("Value: " + gameSetting.GetIntValue());

            base.OnInspectorGUI();

            if(GUILayout.Button("Set to default value"))
            {
                (target as GameSetting).ResetToDefault();
                EditorUtility.SetDirty((target as GameSetting));
            }
            EditorUtils.DrawVerify(target as GameSetting);
            EditorUtils.EndColoredEditor();
        }
    }
}
#endif
