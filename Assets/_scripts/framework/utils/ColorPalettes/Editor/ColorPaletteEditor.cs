//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using OTBT.Framework.Utils.Editor;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace OTBT.Framework.ColorPalettes.Editor
{
    [CustomEditor(typeof(ColorPalette))] [CanEditMultipleObjects]
    public class ColorPaletteEditor : UnityEditor.Editor
    {
        ReorderableList m_List;
        SerializedProperty m_Colors;

        int m_TintSelectionIndex = 0;
        Texture2D m_TintSource = null;
        string m_TintFilename = "generatedIcon";

        Menu m_Menu;

        void OnEnable()
        {
            m_Colors = serializedObject.FindProperty("m_Colors");
            m_List = new ReorderableList(serializedObject, m_Colors, true, true, false, false);

            m_List.drawElementCallback = DrawColorItem;
            m_List.drawHeaderCallback = DrawHeader;
            m_List.onRemoveCallback = RemoveItem;
        }

        void RemoveItem(ReorderableList list)
        {
            m_Colors.DeleteArrayElementAtIndex(list.index);
        }

        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, $"{m_Colors.arraySize} Colors");
        }

        void DrawColorItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_Colors.arraySize <= 0) return;
            var color = m_Colors.GetArrayElementAtIndex(index);
            var label = ColorPaletteManager.instance.colorSlotNames.Count > index ? ColorPaletteManager.instance.colorSlotNames[index] : "Color";

            var labelRect = rect;
            labelRect.width = 120;

            var colorRect = rect;
            colorRect.x += 125;
            colorRect.width -= 125;

            EditorGUI.LabelField(labelRect, label);
            color.colorValue = EditorGUI.ColorField(colorRect, color.colorValue);
        }



        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            serializedObject.Update();
            EditorUtils.DrawLogoHeader($"Palette: {target.name}", "https://wiki.beatentrack.games/doc/color-palettes-YntYuMnbcL");

            m_List.DoLayoutList();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Add")) (target as ColorPalette).Add();
                if (GUILayout.Button("- Remove")) (target as ColorPalette).Remove();
            }

            serializedObject.ApplyModifiedProperties();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy to Clipboard")) CopyToClipboard();
                if (GUILayout.Button("Load from Clipboard")) LoadFromClipboard();
            }

            EditorUtils.EndColoredEditor();

            EditorUtils.Space(50);
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawTinyLogoHeader("Generate tinted glyphicons");
            m_TintSelectionIndex = EditorGUILayout.IntField("Index in palette", m_TintSelectionIndex);
            m_TintSource = EditorGUILayout.ObjectField("Source graphic", m_TintSource, typeof(Texture2D), false) as Texture2D;
            m_TintFilename = EditorGUILayout.TextField("Target filename", m_TintFilename);
            if (GUILayout.Button("Tint"))
            {
                EditorUtils.GenerateTintedGlyphicon(m_TintSource, (target as ColorPalette).GetColor(m_TintSelectionIndex), m_TintFilename);
            }

            EditorUtils.EndColoredEditor();
        }

        void CopyToClipboard()
        {
            var output = new StringBuilder();

            for (int i = 0; i < m_Colors.arraySize; i++)
            {
                var color = m_Colors.GetArrayElementAtIndex(i);
                var hex = ColorUtility.ToHtmlStringRGBA(color.colorValue);
                output.AppendLine($"#{hex}");
            }

            GUIUtility.systemCopyBuffer = output.ToString();
        }

        void LoadFromClipboard()
        {
            var input = GUIUtility.systemCopyBuffer;
            var reader = new StringReader(input);

            var colors = new List<Color>();

            while (reader.Peek() > 0)
            {
                var text = reader.ReadLine();
                var colorString = text.Substring(0, 9);
                if (!UnityEngine.ColorUtility.TryParseHtmlString(colorString, out Color color)) continue;

                colors.Add(color);
            }

            for (int i = 0; i < m_Colors.arraySize; i++)
            {
                var color = m_Colors.GetArrayElementAtIndex(i);
                color.colorValue = colors[i];
            }
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var colorAmount = m_Colors.arraySize;

            var tex = new Texture2D(width, height);
            var colors = tex.GetPixels();

            for (int x = 0; x < tex.width; x++)
            {
                var colorIndex = Mathf.FloorToInt(x / (float)tex.width * colorAmount);
                var color = m_Colors.GetArrayElementAtIndex(colorIndex).colorValue;

                for (int y = 0; y < tex.height; y++)
                {
                    var position = tex.width * y + x;
                    colors[position] = color;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }
    }
}
