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

namespace Sparrow.Verification
{
    [CustomEditor(typeof(VerifyProfile))]
    public class VerifyProfileEditor : Editor
    {
        private string searchQuery = "";

        public override void OnInspectorGUI()
        {
            VerifyProfile profile = (VerifyProfile)target;

            // Draw the API endpoint
            EditorGUILayout.LabelField("Profile settings", EditorStyles.boldLabel);
            string caption = EditorGUILayout.TextField("Caption", profile.caption);
            if (!caption.Equals(profile.caption)) profile.SetCaption(caption);

            string apiEndpoint = EditorGUILayout.TextField("API Endpoint", profile.apiEndpoint);
            if (!apiEndpoint.Equals(profile.apiEndpoint)) profile.SetApiEndpoint(apiEndpoint);

            // Draw hooks
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Automatic hooks", EditorStyles.boldLabel);
            EditorUtils.VerifyLabelDescription("Verification can run automatically when you build the project. This will create a 'verificationReport.txt' file with the results. You can also make the build fail if Verification finds errors in your project.");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ScanType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RunOnBuild"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FailBuildOnError"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LogLocation"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            // Draw the Checks
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Individual checks", EditorStyles.boldLabel);
            searchQuery = UnityEditor.EditorGUILayout.TextField("", searchQuery, "SearchTextField");
            foreach (VerifyCheckBase check in profile.checks)
            {
                if ((check.description + check.longDescription).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (check.DrawProfileEditor())
                        profile.Save(); // Each check knows how to draw its own editor
                }
            }

            EditorGUILayout.Space(15);
            if (GUILayout.Button("Refresh Checks"))
                profile.UpdateCheckList();
        }
    }
}
#endif