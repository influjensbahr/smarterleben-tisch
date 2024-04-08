//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable] 
    public class CheckAttributes : VerifyCheckBase
    {
        public override string description => "Attribute checks";
        public override string longDescription => "This check performs the validation of the attributes you can use when programming your own scripts like VRequired and so on. You can turn individual attributes on and off, as well as set their severity.";
        
        [Serializable]
        public class AttributeSeverity
        {
            [SerializeField, HideInInspector] public string TypeName;
            [SerializeField] public VerifyResult.Severity SeverityLevel;
            [SerializeField] public bool Activated;
            bool m_EditorExtended = false;

            public bool DrawProfileEditor()
            {
                bool preActive = Activated;
                VerifyResult.Severity preSeverity = SeverityLevel;
                bool specificChanges = false;

                Type attributeType = Type.GetType(TypeName);

                EditorGUILayout.BeginVertical("box");

                m_EditorExtended = EditorGUILayout.Foldout(m_EditorExtended, (Activated ? "✔ " : "✘ ") + attributeType.Name, true);

                if (m_EditorExtended)
                {
                    Activated = GUILayout.Toolbar(Activated ? 0 : 1, new[] { "Active", "Not active" }) == 0;
                    SeverityLevel = (VerifyResult.Severity)GUILayout.Toolbar((int)SeverityLevel, new[] { "Error", "Warning", "Info" });
                }
                EditorGUILayout.EndVertical();

                // Check if anything was changed
                if (preActive != Activated || preSeverity != SeverityLevel || specificChanges)
                {
                    return true;
                }
                return false;
            }
        }

        [SerializeField]
        private List<AttributeSeverity> attributeSeverities = new List<AttributeSeverity>();

        private static AttributeSeverity s_DefaultSeverity = new AttributeSeverity
        {
            SeverityLevel = VerifyResult.Severity.Warning,
            Activated = true
        };

        public override bool DrawSpecificProfileEditor()
        {
            bool changedOne = false;
            for (int i = 0; i < attributeSeverities.Count; i++)
            {
                changedOne = changedOne || attributeSeverities[i].DrawProfileEditor();
            }
            return changedOne;
        }

        // Initialize the list with default severities
        public void OnValidate(VerifyProfile profile)
        {
            var types = ReflectionUtil.TypesImplementingInterface(typeof(IVerifyAttribute));
            foreach (var type in types)
            {
                if (!attributeSeverities.Exists(x => x.TypeName == type.FullName) && !type.IsInterface)
                {
                    attributeSeverities.Add(new AttributeSeverity
                    {
                        TypeName = type.FullName,
                        SeverityLevel = VerifyResult.Severity.Warning, 
                        Activated = true
                    });
                }

            }
            EditorUtility.SetDirty(profile);
        }

        public AttributeSeverity GetSeverityForAttribute(Type attributeType)
        {
            AttributeSeverity attrSeverity = attributeSeverities.Find(x => x.TypeName == attributeType.FullName);
            return attrSeverity == null ? s_DefaultSeverity : attrSeverity;
        }

        public void SetSeverityForAttribute(Type attributeType, VerifyResult.Severity severity)
        {
            AttributeSeverity attrSeverity = attributeSeverities.Find(x => x.TypeName == attributeType.FullName);
            if (attrSeverity != null)
            {
                attrSeverity.SeverityLevel = severity;
            }
            else
            {
                attributeSeverities.Add(new AttributeSeverity
                {
                    TypeName = attributeType.FullName,
                    SeverityLevel = severity
                });
            }
        }

        public override void PerformCheck(GameObject obj)
        {
            foreach (var component in obj.GetComponents<MonoBehaviour>())
            {
                if (component == null) continue;

                PerformCheck(component as MonoBehaviour);
            }
        }

        public override void PerformCheck(MonoBehaviour obj)
        {
            foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Attribute[] requiredAttributes = (Attribute[])Attribute.GetCustomAttributes(field);//, typeof(IVerifyAttribute));

                foreach (Attribute requiredAttribute in requiredAttributes)
                {
                    if (requiredAttribute != null && requiredAttribute is IVerifyAttribute)
                    {
                        if (GetSeverityForAttribute(requiredAttribute.GetType()).Activated == false) continue;
                        object value = field.GetValue(obj);
                        (requiredAttribute as IVerifyAttribute).PerformCheck(this, value, field, obj, GetSeverityForAttribute(requiredAttribute.GetType()).SeverityLevel);
                    }
                }
            }
        }

        public override void PerformCheck(ScriptableObject obj)
        {
            foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Attribute[] requiredAttributes = (Attribute[])Attribute.GetCustomAttributes(field);//, typeof(IVerifyAttribute));

                foreach (Attribute requiredAttribute in requiredAttributes)
                {
                    if (requiredAttribute != null && requiredAttribute is IVerifyAttribute)
                    {
                        if (GetSeverityForAttribute(requiredAttribute.GetType()).Activated == false) continue;
                        object value = field.GetValue(obj);
                        (requiredAttribute as IVerifyAttribute).PerformCheck(this, value, field, obj, GetSeverityForAttribute(requiredAttribute.GetType()).SeverityLevel);
                    }
                }
            }
        }
    }
}
#endif