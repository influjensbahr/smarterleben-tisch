//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if !SPARROW_VERIFICATION
#define SPARROW_VERIFICATION
#endif

using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.Events;
using static Sparrow.Verification.VerifyResult;

namespace Sparrow.Verification
{
    /// <summary>
    /// Base interface for our project checks. Will automatically be added to project check window when implemented.
    /// All you need to overwrite here is m_Description, m_Icon and PerformCheck()
    /// </summary>
    [Serializable]
    public abstract class VerifyCheckBase
    {
        public List<VerifyResult> failedChecks = new List<VerifyResult>();
        protected bool m_ShowFoldout = false;

        public virtual string description => "Verify check";
        public virtual string longDescription => "This check inherits from VerifyCheckBase, a long description has not yet been set.";
        public int NumberOfErrors() => failedChecks.Count;


        // options for this specific check object
        [SerializeField] internal bool m_Active = true;
        [SerializeField] internal VerifyResult.Severity m_Severity = VerifyResult.Severity.Warning;

        /// <summary>
        /// Performs the check on a single game object
        /// </summary>
        public virtual void PerformCheckForProject() { }
        public virtual void PerformCheck(GameObject obj) { }
        public virtual void PerformCheck(MonoBehaviour obj) { }
        public virtual void PerformCheck(ScriptableObject sobj) { }
        public virtual void PerformCheck(Mesh sobj) { }
        public virtual void PerformCheck(Texture2D obj) { }
        public virtual void PerformCheck(Sprite obj) { }
        public virtual void PerformCheck(AudioClip obj) { }
        public virtual void PerformCheck(PhysicMaterial obj) { }
        public virtual void PerformCheck(PhysicsMaterial2D obj) { }
        public virtual void PerformCheck(Shader obj) { }
        public virtual void PerformCheck(Material obj) { }

        public VerifyCheckBase() { }
        public static bool IsPrefab(GameObject a_Object)
        {
            return a_Object.scene.rootCount == 0;
        }

        public List<VerifyResult> PerformCheckWrapper(List<UnityEngine.Object> obj, CheckType type, string checkSource, bool reset=true, bool thisObjectOnly = false, bool progressBar = false, float progressBarOffset = 0f, float progressBarScale = 1f)
        {
            if (failedChecks == null) failedChecks = new List<VerifyResult>();
            if (reset) failedChecks.Clear();

#if UNITY_EDITOR
            if (m_Active)
            {
                if(!thisObjectOnly) PerformCheckForProject();

                for(int i = 0; i < obj.Count; i++)
                {
                    var o = obj[i];
                    if (progressBar) EditorUtility.DisplayProgressBar("Sparrow Verification", "Checking: " + o.name, 
                        progressBarOffset + ((float) i / (float) obj.Count) * progressBarScale);
                    var modelImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(o)) as ModelImporter;
                    if (modelImporter != null)
                    {
                        PerformCheck(modelImporter);
                    }
                    
                    var textureImport = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(o)) as TextureImporter;
                    if (textureImport != null)
                    {
                        PerformCheck(textureImport);
                    }

                    if (o is MonoScript script) PerformCheck(script);
                    if (o is MonoBehaviour m) PerformCheck(m);
                    if (o is GameObject go) PerformCheck(go);
                    if (o is ScriptableObject so) PerformCheck(so);
                    if (o is Mesh mo) PerformCheck(mo);
                    if (o is Texture2D to) PerformCheck(to);
                    if (o is Sprite sprite) PerformCheck(sprite);
                    if (o is AudioClip audio) PerformCheck(audio);
                    if (o is LightingDataAsset light) PerformCheck(light);
                    if (o is PhysicMaterial phys) PerformCheck(phys);
                    if (o is PhysicsMaterial2D phys2d) PerformCheck(phys2d);
                    if (o is AnimatorController anim) PerformCheck(anim);
                    if (o is Shader shader) PerformCheck(shader);
                    if (o is Material material) PerformCheck(material);
                }
            }

            foreach (VerifyResult result in failedChecks)
                result.WithCheckType(type).WithCheckSource(checkSource);
#endif
            return failedChecks;
        }

        public void RemoveCheck(VerifyResult r)
        {
            failedChecks.Remove(r);
        }

        /// <summary>
        /// Performs a simple check - if true, the check has passed
        /// </summary>
        /// <param name="passes">if true, check has passed</param>
        /// <param name="errorMessage"></param>
        /// <param name="obj"></param>
        /// <param name="fixAction"></param>
        public void Check(bool passes, string errorMessage, UnityEngine.Object obj, UnityAction fixAction = null, string category = "")
        {
            if (!Application.isEditor) return;
            if (!passes)
                AddFailedCheck(errorMessage, obj, fixAction).WithCategory(category.Equals("") ? description : category);
        }

        public void CheckNotNull(UnityEngine.Object checkedObj, string refTitle, UnityEngine.Object obj, UnityAction fixAction = null, string category = "")
        {
            Check(checkedObj != null, $"NullRef: Object {refTitle} is not assigned", obj, fixAction, category);
        }

        public void CheckStringNotWhitespace(string checkedString, string refTitle, UnityEngine.Object obj, UnityAction fixAction = null, string category = "")
        {
            Check(!string.IsNullOrWhiteSpace(checkedString), $"String {refTitle} is empty or null", obj, fixAction, category);
        }

        public void CheckStringNotEmpty(string checkedString, string refTitle, UnityEngine.Object obj, UnityAction fixAction = null, string category = "")
        {
            Check(!string.IsNullOrEmpty(checkedString), $"String {refTitle} is empty or null", obj, fixAction, category);
        }

        public void CheckElementsNotNull<T>(IEnumerable<T> checkedObj, string refTitle, UnityEngine.Object obj, UnityAction fixAction = null, string category = "")
        {
            foreach (T o in checkedObj)
                Check(o != null, $"IEnumerable NullRef: {refTitle} contains a null element", obj, fixAction, category);
        }

        public VerifyResult AddFailedCheck(string errorMessage = "", UnityEngine.Object obj = null, UnityAction fixAction = null, string category = "")
        {
            VerifyResult ret = new VerifyResult(this)
                .WithCategory(category.Equals("") ? description : category)
                .WithDescription(errorMessage)
                .ForObject(obj)
                .WithSeverity(m_Severity)
                .WithFix(fixAction);
            failedChecks.Add(ret);
            return ret;
        }


#if UNITY_EDITOR

        bool m_EditorExtended = false;

        public bool DrawProfileEditor()
        {
            bool preActive = m_Active;
            VerifyResult.Severity preSeverity = m_Severity;
            bool specificChanges = false;

            EditorGUILayout.BeginVertical("box");
            
            m_EditorExtended = EditorGUILayout.Foldout(m_EditorExtended, (m_Active ? "✔ " : "✘ ") + description, true);

            if (m_EditorExtended)
            {
                if(longDescription.Length > 0)
                {
                    EditorGUILayout.HelpBox(longDescription, MessageType.Info);
                }

                m_Active = GUILayout.Toolbar(m_Active ? 0 : 1, new[] { "Active", "Not active" }) == 0;
                m_Severity = (VerifyResult.Severity) GUILayout.Toolbar((int) m_Severity, new[] { "Error", "Warning", "Info" });
                specificChanges = DrawSpecificProfileEditor();

            }
            EditorGUILayout.EndVertical();

            // Check if anything was changed
            if (preActive != m_Active || preSeverity != m_Severity || specificChanges)
            {
                return true;
            }
            return false;
        }

        public virtual bool DrawSpecificProfileEditor() {
            return false;
        }
        public virtual void PerformCheck(LightingDataAsset obj) { }
        public virtual void PerformCheck(AnimatorController obj) { }
        public virtual void PerformCheck(ModelImporter model) { }
        public virtual void PerformCheck(MonoScript model) { }
        public virtual void PerformCheck(TextureImporter texture) { }

        public bool ShowFoldout()
        {
            return m_ShowFoldout;
        }

        public void SetShowFoldout(bool b)
        {
            m_ShowFoldout = b;
        }

        public void DrawEditorGUI(bool useConsole = false)
        {
            if (failedChecks.Count <= 0) return;

            for (int i = failedChecks.Count - 1; i >= 0; i--)
            {
                failedChecks[i].Print();
                if (useConsole)
                    Debug.LogError(failedChecks[i].ToString(), failedChecks[i].obj);
            }
        }

#endif

    }
}