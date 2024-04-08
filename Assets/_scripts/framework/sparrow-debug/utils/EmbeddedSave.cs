// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
//

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Sparrow.BugTracking
{
    [Serializable]
    public class EmbeddedSave
    {
        List<EmbeddedSaveElement> m_Options = new List<EmbeddedSaveElement>();

        string m_Caption = "";
        UnityAction m_RefreshAction = null;

        private string[] m_ElementCaptions = null;
        public bool isSetup => m_RefreshAction != null && !m_Caption.Equals("");

        public EmbeddedSave(string caption, UnityAction refreshAction)
        {
            m_Caption = caption;
            m_RefreshAction = refreshAction;
        }

#if UNITY_EDITOR
        EmbeddedSaveElement m_CurrentElement = null;

        public EmbeddedSaveElement DrawEditor(EmbeddedSaveElement defaultValue)
        {
            m_CurrentElement = m_CurrentElement == null ? defaultValue : m_CurrentElement;
            EditorGUILayout.BeginHorizontal();
            if (m_Options == null || m_Options.Count == 0)
            {
                EditorGUILayout.LabelField(m_Caption + ": " + (m_CurrentElement == null ? "not yet loaded" : m_CurrentElement.name));
            }
            else
            {
                EditorGUILayout.LabelField(m_Caption);
                int selectedValue = m_Options.IndexOf(m_CurrentElement);
                int newSelectedValue = Math.Clamp(EditorGUILayout.Popup(selectedValue, GetElementCaptions()), 0, m_Options.Count);
                m_CurrentElement = m_Options[newSelectedValue];
            }

            if (GUILayout.Button("Load"))
                m_RefreshAction?.Invoke();

            EditorGUILayout.EndHorizontal();
            return m_CurrentElement;
        }
#endif

        public void SetData(List<EmbeddedSaveElement> elements)
        {
            m_Options = elements;
        }

        string[] GetElementCaptions()
        {
            if (m_ElementCaptions == null)
            {
                if (m_Options == null || m_Options.Count == 0)
                    return null;

                m_ElementCaptions = new string[m_Options.Count];
                for (int i = 0; i < m_Options.Count; i++)
                    m_ElementCaptions[i] = m_Options[i].name;
            }
            return m_ElementCaptions;
        }
    }

    [Serializable]
    public class EmbeddedSaveElement
    {
        [SerializeField] public string id;
        [SerializeField] public string name;
        [SerializeField] public string type;
    }
}