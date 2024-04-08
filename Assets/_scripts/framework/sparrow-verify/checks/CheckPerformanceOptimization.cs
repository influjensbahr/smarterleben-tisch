//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Marc Freitag
//

#if UNITY_EDITOR

using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckPerformanceOptimization : VerifyCheckBase
    {
        public override string description => "Performance Optimization";
        public override string longDescription => "Checks for performance optimization suggestions in Scripts.";

        static readonly Regex StringBasedInvocationRegex = new Regex(@"(\w+)\.Invoke\(", RegexOptions.Compiled);
        static readonly Regex UnityObjectComparisonRegex = new Regex(@"if\s*\(\s*(\w+)\s*==\s*null\s*\)", RegexOptions.Compiled);

        [SerializeField] bool m_CheckAddComponent = true;
        [SerializeField] bool m_CheckAddComponentUpdateOnly = true;
        [SerializeField] bool m_CheckFindMethod = true;
        [SerializeField] bool m_CheckFindMethodUpdateOnly = true;
        [SerializeField] bool m_CheckGetComponent = true;
        [SerializeField] bool m_CheckGetComponentUpdateOnly = true;
        [SerializeField] bool m_CheckDebugLogs = true;
        [SerializeField] bool m_CheckDebugLogsUpdateOnly = true;
        [SerializeField] bool m_CheckStringBasedInvokation = true;
        [SerializeField] bool m_CheckStringBasedInvokationUpdateOnly = true;
        [SerializeField] bool m_CheckCameraMain = true;
        [SerializeField] bool m_CheckCameraMainUpdateOnly = true;
        [SerializeField] bool m_CheckNullComparison = true;
        [SerializeField] bool m_CheckNullComparisonUpdateOnly = true;

        public override bool DrawSpecificProfileEditor()
        {
            bool anyValueChanged = false;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            m_CheckAddComponent = EditorGUILayout.Toggle("Check AddComponent", m_CheckAddComponent);
            m_CheckAddComponentUpdateOnly = EditorGUILayout.Toggle("Update Only", m_CheckAddComponentUpdateOnly);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            m_CheckFindMethod = EditorGUILayout.Toggle("Check Find Method", m_CheckFindMethod);
            m_CheckFindMethodUpdateOnly = EditorGUILayout.Toggle("Update Only", m_CheckFindMethodUpdateOnly);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            m_CheckGetComponent = EditorGUILayout.Toggle("Check GetComponent", m_CheckGetComponent);
            m_CheckGetComponentUpdateOnly = EditorGUILayout.Toggle("Update Only", m_CheckGetComponentUpdateOnly);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            m_CheckDebugLogs = EditorGUILayout.Toggle("Check Debug Logs", m_CheckDebugLogs);
            m_CheckDebugLogsUpdateOnly = EditorGUILayout.Toggle("Update Only", m_CheckDebugLogsUpdateOnly);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            m_CheckStringBasedInvokation = EditorGUILayout.Toggle("Check String Based Invocation", m_CheckStringBasedInvokation);
            m_CheckStringBasedInvokationUpdateOnly = EditorGUILayout.Toggle("Update Only", m_CheckStringBasedInvokationUpdateOnly);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            m_CheckCameraMain = EditorGUILayout.Toggle("Check Camera Main", m_CheckCameraMain);
            m_CheckCameraMainUpdateOnly = EditorGUILayout.Toggle("Update Only", m_CheckCameraMainUpdateOnly);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            m_CheckNullComparison = EditorGUILayout.Toggle("Check Null Comparison", m_CheckNullComparison);
            m_CheckNullComparisonUpdateOnly = EditorGUILayout.Toggle("Update Only", m_CheckNullComparisonUpdateOnly);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
            {
                anyValueChanged = true;
            }

            return anyValueChanged;
        }

        public override void PerformCheck(MonoScript script)
        {
            bool insideUpdateMethod = false; 
            int methodBlockDepth = 0;

            var lines = script.text.Split(new[] { "\n" }, StringSplitOptions.None);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (methodBlockDepth == 0 && (IsMethodStart(line, "Update") || IsMethodStart(line, "LateUpdate") || IsMethodStart(line, "FixedUpdate")))
                {
                    methodBlockDepth = 1;
                    insideUpdateMethod = true;
                    continue;
                }

                if (insideUpdateMethod)
                {
                    if (line.Contains("{")) methodBlockDepth++;
                    if (line.Contains("}")) methodBlockDepth--;
                    if (methodBlockDepth == 0) insideUpdateMethod = false;
                }

                if(m_CheckAddComponent) CheckAddComponentMethod(line, i, script, m_CheckAddComponentUpdateOnly ? insideUpdateMethod : true);
                if(m_CheckFindMethod) CheckFindMethod(line, i, script, m_CheckFindMethodUpdateOnly ? insideUpdateMethod : true);
                if(m_CheckGetComponent) CheckGetComponentMethod(line, i, script, m_CheckGetComponentUpdateOnly ? insideUpdateMethod : true);
                if(m_CheckDebugLogs) CheckDebugLogMethod(line, i, script, m_CheckDebugLogsUpdateOnly ? insideUpdateMethod : true);
                if(m_CheckStringBasedInvokation) CheckStringBasedMethodInvocation(line, i, script, m_CheckStringBasedInvokationUpdateOnly ? insideUpdateMethod : true);
                if(m_CheckCameraMain) CheckCameraMainProperty(line, i, script, m_CheckCameraMainUpdateOnly ? insideUpdateMethod : true);
                if(m_CheckNullComparison)  CheckUnityObjectComparisonWithNull(line, i, script, m_CheckNullComparisonUpdateOnly ? insideUpdateMethod : true);
            }
        }

        bool IsMethodStart(string line, string methodName)
        {
            line = line.Trim();

            if (!line.Contains("void " + methodName))
            {
                return false;
            }
            var methodSignatureRegex = new Regex(@"void\s+" + Regex.Escape(methodName) + @"\s*\(\s*\)");
            return methodSignatureRegex.IsMatch(line);
        }

       

        void CheckAddComponentMethod(string line, int lineNumber, MonoScript script, bool insideUpdateMethod)
        {
            if (line.Contains(".AddComponent(") && insideUpdateMethod)
            {
                AddFailedCheck($"Performance suggestion: don't use AddComponent in Line {lineNumber + 1}", script);
            }
        }

        void CheckFindMethod(string line, int lineNumber, MonoScript script, bool insideUpdateMethod)
        {
            if (line.Contains(".Find(") && insideUpdateMethod)
            {
                AddFailedCheck($"Performance suggestion: don't use Find in Line {lineNumber + 1}", script);
            }
        }

        void CheckGetComponentMethod(string line, int lineNumber, MonoScript script, bool insideUpdateMethod)
        {
            if (line.Contains(".GetComponent(") && insideUpdateMethod)
            {
                AddFailedCheck($"Performance suggestion: don't use GetComponent in Line {lineNumber + 1}", script);
            }
        }

        void CheckDebugLogMethod(string line, int lineNumber, MonoScript script, bool insideUpdateMethod)
        {
            if ((line.Contains("Debug.LogWarning(") || line.Contains("Debug.LogError(") || line.Contains("Debug.Log(")) && insideUpdateMethod)
            {
                AddFailedCheck($"Performance suggestion: don't use Debug log method in Line {lineNumber + 1}", script);
            }
        }

        void CheckStringBasedMethodInvocation(string line, int lineNumber, MonoScript script, bool insideUpdateMethod)
        {
            var match = StringBasedInvocationRegex.Match(line);
            if (match.Success && insideUpdateMethod)
            {
                AddFailedCheck($"Performance suggestion: don't use String-based method invocation in Line {lineNumber + 1}", script);
            }
        }

        void CheckCameraMainProperty(string line, int lineNumber, MonoScript script, bool insideUpdateMethod)
        {
            if (line.Contains("Camera.main") && insideUpdateMethod)
            {
                AddFailedCheck($"Performance suggestion: don't use Camera.main in Line {lineNumber + 1}", script);
            }
        }

        void CheckUnityObjectComparisonWithNull(string line, int lineNumber, MonoScript script, bool insideUpdateMethod)
        {
            var match = UnityObjectComparisonRegex.Match(line);
            if (match.Success && insideUpdateMethod)
            {
                AddFailedCheck($"Performance suggestion: don't use Unity object comparison with null in Line {lineNumber + 1}", script);
            }
        }
    }
}
#endif