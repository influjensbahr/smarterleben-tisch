// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sparrow.BugTracking
{
    [CustomEditor(typeof(BugTrackingManager))]
    public class BugTrackingWindow : Editor
    {
        public override void OnInspectorGUI()
        {
            BugTrackingManager settings = (BugTrackingManager)target;
            EditorUtils.DrawLogoHeader("Feedback & Bug Reporting", "https://wiki.beatentrack.games/s/e32209b6-5300-4c88-88d6-fe638b29dd9a", true);
            
            EditorUtils.Separator(); EditorUtils.Space(8);

            // What to track?
            EditorUtils.DrawSectionHeader("What to track?", true);
            EditorGUILayout.HelpBox("Select here what to send with your reports. You can add your own data to be sent with the reports - check out the AddLog() function in our documentation.", MessageType.Info);
            EditorGUILayout.LabelField("Collected data", EditorStyles.boldLabel);
            EditorUtils.DrawPropertyEditor(serializedObject, "m_DefaultHeadline");
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_BuildNumber", "m_BundleId");
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_OpenSceneCount", "m_OpenSceneList");
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_StartTime", "m_CurrentTime");
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_Platform", "m_DeviceModel");
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_DeviceName", "m_DeviceType");
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_GraphicsDeviceName", "m_GraphicsDeviceMemorySize");
            EditorUtils.Space();
            EditorGUILayout.LabelField("Log data", EditorStyles.boldLabel);
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_IncludeDebugLog", "m_IncludeDebugException");
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_IncludeDebugError", "m_IncludeDebugWarnings");
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_IncludeDebugAssertion", "m_IncludeStackTraces");
            EditorUtils.Space();
            EditorUtils.DrawPropertyEditor(serializedObject, "m_IncludeCustomText");
            EditorUtils.Space(8); EditorUtils.Separator(); EditorUtils.Space(8);

            // When to send it?
            EditorUtils.DrawSectionHeader("When to send it?", true);
            EditorGUILayout.HelpBox("Reports can be sent automatically without player feedback, select here what triggers this. You can also trigger this on your own - check out SendReport() function in our documentation.", MessageType.Info);
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_AutoSendOnError", "m_AutoSendOnException");
            EditorUtils.DrawTwoPropertyEditors(serializedObject, "m_AutoSendOnWarning", "m_OnlySendInBuilds");
            EditorUtils.Space(8); EditorUtils.Separator(); EditorUtils.Space(8); 

            // Where to send it?
            EditorUtils.DrawSectionHeader("Where to send it?", true);
            EditorGUILayout.HelpBox("Use this section to set up the targets where the reports can be sent. You can activate multiple targets (e.g. to create a ticket in Jira and notify you on discord).", MessageType.Info);
            EditorUtils.Space();
            
            var activated = settings.apiSettingList.Where(api => api.isActive).ToList();
            if (activated.Count > 0) {
                EditorGUILayout.LabelField("Activated", EditorStyles.boldLabel);
                foreach (var api in activated) api.DrawEditor();
                EditorUtils.Space();
            }

            var deactivated = settings.apiSettingList.Where(api => !api.isActive).ToList();
            if (deactivated.Count > 0) {
                EditorGUILayout.LabelField("Deactivated", EditorStyles.boldLabel);
                foreach (var api in deactivated) api.DrawEditor();
            }
            
            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}
