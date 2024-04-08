// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using UnityEditor;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Sparrow.BugTracking
{
    [Serializable]
    public abstract class APISettings 
    {
        public virtual string caption => "API Setting";
        public bool isWorking
        {
            get => m_TestedWorking;
            set => m_TestedWorking = value;
        }
        public bool isActive
        {
            get => m_IsActive;
            set => m_IsActive = value;
        }

        public abstract Task<bool> SendReport(ReportData report);

        public async Task<bool> SendTestReport()
        {
            return await SendReport(BugTrackingManager.GetTestReport());
        }

#if UNITY_EDITOR
        public abstract void DrawInternalEditor();
        bool m_Foldout = false;

        public async void DrawEditor()
        {
            GUILayout.BeginVertical("HelpBox");
            m_Foldout = EditorGUILayout.Foldout(m_Foldout, EditorUtils.LabelWithIcon(" " + caption, m_TestedWorking ? "check" : "no-symbol"), true);
            if (m_Foldout)
            {
                m_IsActive = GUILayout.Toolbar(m_IsActive ? 0 : 1, new[] { "Active", "Not active" }) == 0;
                EditorUtils.SmallLabel("This endpoint " + (m_TestedWorking ? "appears to be functional!" : "is not yet fully setup and working."));

                EditorUtils.Space();
                DrawInternalEditor();
                EditorUtils.Space();
                if(GUILayout.Button("Create test report"))
                {
                    m_TestedWorking = await SendTestReport();
                    return;
                }
            }
            GUILayout.EndVertical();
        }
#endif

        [SerializeField] internal bool m_TestedWorking = false;
        [SerializeField] bool m_IsActive = false;

    }
}