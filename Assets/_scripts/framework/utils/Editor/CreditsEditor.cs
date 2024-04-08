// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using OTBT.Framework.Utils.Editor;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace OTBT.Framework.Utils
{
    [CustomEditor(typeof(CreditsScreen))]
    public class CreditsScreenEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var credits = serializedObject.FindProperty("m_Credits").objectReferenceValue as Credits;
            var parent = serializedObject.FindProperty("m_Content").objectReferenceValue as RectTransform;
            var header = serializedObject.FindProperty("m_HeaderPrefab").objectReferenceValue as TMP_Text;
            var text = serializedObject.FindProperty("m_LinePrefab").objectReferenceValue as TMP_Text;

            EditorUtils.DrawVerify(credits);

            using var disabledScope = new EditorGUI.DisabledScope(parent == null || header == null || text == null);
            using var horizontalScope = new EditorGUILayout.HorizontalScope();

            if (GUILayout.Button("Build Credits"))
                BuildCredits(credits, parent, header.gameObject, text.gameObject);
            if (GUILayout.Button("Clear & Build"))
            {
                while (parent.childCount > 0)
                {
                    var tx = parent.GetChild(0);
                    DestroyImmediate(tx.gameObject);
                }

                BuildCredits(credits, parent, header.gameObject, text.gameObject);
            }

        }

        void BuildCredits(Credits credits, RectTransform parent, GameObject header, GameObject text)
        {
            foreach (var creditsGroup in credits.credits)
            {
                CreateHeader(parent, header, creditsGroup.title);
                foreach (var groupName in creditsGroup.names)
                {
                    CreateHeader(parent, text, groupName);
                }
            }
        }
        public void CreateHeader(Transform parent, GameObject prefab, string title)
        {
            var header = ((GameObject)PrefabUtility.InstantiatePrefab(prefab, parent)).GetComponent<TMP_Text>();
            header.transform.SetAsLastSibling();
            header.gameObject.name = title;

            header.text = title;
        }
    }
}
