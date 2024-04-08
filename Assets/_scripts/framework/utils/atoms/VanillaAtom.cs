//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Utils;
using OTBT.Framework.Utils.Editor;
using Sparrow.Verification;
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OTBT.Framework.Core
{
    /// <summary>
    /// A simple scriptable object used as a reference of sorts. Can be used as an object-reference for events and other things
    /// to reduce the risk of typos and to have an object reference.
    /// </summary>
    [CreateAssetMenu(menuName = "OTBT/Atoms/Vanilla Atom", fileName = "New Vanilla Atom")]
    [Serializable]
    public class VanillaAtom : ScriptableObject, IVerify, IExtendDefaultEditor, IStringOrAtomReference
    {
        [SerializeField] string m_Identifier = "";

        public string identifier => m_Identifier;

        public VanillaAtom()
        {

            if (m_Identifier.Equals(""))
                m_Identifier = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return m_Identifier;
        }
        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
            checker.Check(!m_Identifier.Equals(""), "StringObject is empty", this, () =>
            {
                RegenerateGUID();
            });
#endif
        }



#if UNITY_EDITOR

        public void RegenerateGUID()
        {
            m_Identifier = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }

        public void ExtendDefaultEditor()
        {
            if(GUILayout.Button("Regenerate GUID"))
            {
                RegenerateGUID();
            }
            if (GUILayout.Button("Set GUID to filename"))
            {
                m_Identifier = this.name;
            }
        }
#endif
    }

    public interface IStringOrAtomReference
    {
        string identifier { get; }
    }

    [Serializable]
    public class StringOrAtomReference<T> : IStringOrAtomReference where T : VanillaAtom
    {
        [SerializeField] string m_Identifier = "";
        [SerializeField] T m_StringObject = null;

        public override string ToString() {
            return m_StringObject != null ? m_StringObject.ToString() : m_Identifier;
        }

        public T atom => m_StringObject;
        public string identifier => m_StringObject != null ? m_StringObject.ToString() : m_Identifier;
        public bool isEmpty => m_Identifier.Equals("") && m_StringObject == null;

#if UNITY_EDITOR
        public void ShowGUI(Object parent, string objName = "Reference")
        {
            string inputval = "";
            T inputObject = null;

            if (m_StringObject == null || !m_Identifier.Equals(""))
                inputval = (string)EditorGUILayout.TextField(objName + " string value: ", m_Identifier);
            if (m_StringObject != null || m_Identifier.Equals(""))
                inputObject = (T)EditorGUILayout.ObjectField(objName + " object ref: ", m_StringObject, typeof(T), false);

            if(!m_Identifier.Equals(inputval))
            {
                m_Identifier = inputval;
                EditorUtility.SetDirty(parent);
            }
            if (inputObject != m_StringObject)
            {
                m_StringObject = inputObject;
                EditorUtility.SetDirty(parent);
            }
        }
#endif
    }
}
