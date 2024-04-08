//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Sparrow.Verification
{
    /// <summary>
    /// Various functionality for building our Editors.
    /// </summary>
    public static class EditorUtils
    {
        public static Color ActionColor = Color.blue;
        public static Color SingletonColor = Color.green;
        public static Color DarkActionColor = Color.Lerp(ActionColor, Color.black, 0.4f);
        public static Color DarkErrorColor = new Color(16f/255f * 0.4f, 185f / 255f * 0.4f, 178f / 255f * 0.4f);
        public static Color SecondaryActionColor = Color.cyan;

        public static void VerifyLabel(string txt)
        {
            GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel);
            textStyle.wordWrap = true;
            EditorGUILayout.LabelField(txt, textStyle);
        }

        public static void VerifyLabelDescription(string txt)
        {
            GUIStyle textStyle = new GUIStyle(EditorStyles.label);
            textStyle.wordWrap = true;
            EditorGUILayout.LabelField(txt, textStyle);
        }

        public static void RenameScriptableObject(ScriptableObject targetObject, string targetName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var baseName = targetName;

            foreach (var character in invalidChars)
            {
                baseName = baseName.Replace(character.ToString(), string.Empty);
            }
            baseName = baseName.Replace(" ", "-");
            baseName = baseName.ToLowerInvariant();

            targetObject.name = baseName;
            var path = AssetDatabase.GetAssetPath(targetObject);
            AssetDatabase.RenameAsset(path, baseName);

            AssetDatabase.SaveAssets();
        }

       

        /// <summary>
        /// Generates a one pixel texture for use in Editor item backgrounds.
        /// </summary>
        /// <param name="color">Color for the texture.</param>
        public static Texture2D TextureFromColor(Color color)
        {
            var colors = new Color[1];
            colors[0] = color;

            var tex = new Texture2D(1, 1);
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        public static void DrawPropertyField(this SerializedObject serializedObject, string propertyName, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }
        
        public static void Save(UnityEngine.Object document)
        {
            EditorUtility.SetDirty(document);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif