//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Sparrow.Verification
{
    /// <summary>
    /// This is the base class that we use to store all verify check results
    /// </summary>
    public class VerifyResult
    {
        public enum Severity
        {
            Error = 0, Warning = 1, Info = 2
        }

        public enum CheckType
        {
            Selection,
            CurrentScene,
            Project,
            Scenes,
            All,
            Assets
        }


        VerifyCheckBase m_Checker;
        public string category = "";
        public string description = "";
        public UnityEngine.Object obj;
        public string objName = "";
        //public bool isSelected = false;
        public UnityAction fixAction;
        public Severity severity = Severity.Warning;

        public CheckType checkType;
        public string checkSource;

        public string sceneName => 
                 ((checkType == CheckType.CurrentScene || checkType == CheckType.Scenes) 
                    ? Path.GetFileName(checkSource)
                    : checkType.ToString());

            
        public string toString => description + (obj == null && objName.Equals("") ? string.Empty : $"(Object: {objName})");
        public string ToJson() => JsonUtility.ToJson(this, true);
        public string tooltip => (m_Checker == null ? "" : m_Checker.longDescription);


        public VerifyResult WithCheckType(CheckType s)
        {
            checkType = s;
            return this;
        }

        public VerifyResult WithCheckSource(string s)
        {
            checkSource = s;
            return this;
        }
        public VerifyResult WithSeverity(Severity s)
        {
            severity = s;
            return this;
        }

        public VerifyResult WithCategory(string cat)
        {
            category = cat;
            return this;
        }

        public VerifyResult WithDescription(string desc)
        {
            description = desc;
            return this;
        }

        public VerifyResult WithFix(UnityAction action)
        {
            fixAction = action;
            return this;
        }

        public VerifyResult ForObject(UnityEngine.Object o)
        {
            obj = o;
            this.objName = obj == null ? "NULL" : obj.name;
            return this;
        }

        public VerifyResult(VerifyCheckBase checker)
        {
            m_Checker = checker;
        }



#if UNITY_EDITOR
        public VerifyResult(VerifyCheckBase checker, string description, UnityEngine.Object obj, UnityAction fixAction)
            : this(checker)
        {
            WithDescription(description)
                .ForObject(obj)
                .WithFix(fixAction);

            if (EditorPrefs.GetBool("useConsole", false))
                Debug.LogError(description, obj);
        }

        public void Print()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (obj != null)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), true);
                GUI.enabled = true;
                
                if (GUILayout.Button("Select"))
                {
                    Selection.SetActiveObjectWithContext(obj, obj);
                    EditorGUIUtility.PingObject(obj);
                }
            }
            if (fixAction != null && GUILayout.Button("Fix"))
            {
                fixAction?.Invoke();
                if (m_Checker != null) m_Checker.RemoveCheck(this);
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
#endif
        public void Fix() => fixAction?.Invoke();
    }
}
