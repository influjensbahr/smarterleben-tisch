//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using System;
using UnityEditor;
using UnityEngine;
using OTBT.Framework.Utils.Editor;
using UnityEngine.UI;
using Unity.VisualScripting;

namespace OTBT.Framework.ColorPalettes.Editor
{
    [CustomEditor(typeof(ColorByPalette), true), CanEditMultipleObjects]
    public class ColorByPaletteEditor : UnityEditor.Editor
    {
        public static int DrawColorSelector(int currentIndex)
        {
            CheckForManager();

            var manager = FindObjectOfType<ColorPaletteManager>();
            if (manager != null && manager.palette != null)
            {
                manager.palette.DrawEditorPreview();
                EditorUtils.Space();
            }

            var selectedIndex = currentIndex;

            selectedIndex = EditorGUILayout.Popup("Color", selectedIndex, ColorPaletteManager.instance.colorSlotNames.ToArray());

            ShowPreview(currentIndex);

            return selectedIndex;
        }

        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var slotProperty = serializedObject.FindProperty("m_Index");
                var currentIndex = slotProperty.intValue;
                var selectedIndex = DrawColorSelector(currentIndex);

                if (slotProperty.intValue != selectedIndex)
                {
                    slotProperty.intValue = selectedIndex;
                }

                if (GUILayout.Button("Recolor now"))
                {
                    Undo.RecordObject(target, $"Recoloring {target.name} by Palette");
                    ((ColorByPalette)target).RecolorNow();
                    EditorUtility.SetDirty(((ColorByPalette)target).gameObject);
                }
            }

            EditorGUILayout.Space(12);
            DrawPropertiesExcluding(serializedObject, "m_Index", "m_Script");
            serializedObject.ApplyModifiedProperties();


            EditorUtils.DrawVerify(target as ColorByPalette);
            EditorUtils.EndColoredEditor();
        }

        static void ShowPreview(int index)
        {
            var manager = FindObjectOfType<ColorPaletteManager>();
            if (manager == null) return;
            if (manager.palette == null) return;

            var color = manager.palette.GetColor(index);

            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawRect(rect, color);

            if (index >= manager.palette.count)
            {
                GUIStyle errorStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                errorStyle.normal.textColor = Color.black;
                EditorGUI.LabelField(rect, "Color does not exist!", errorStyle);
            }
        }

        static void CheckForManager()
        {
            var manager = FindObjectOfType<ColorPaletteManager>();

            if (manager == null)
            {
                EditorGUILayout.HelpBox("No Color Palette Manager in scene; preview is disabled.", MessageType.Warning, true);
                if (GUILayout.Button("Create Manager in Scene")) CreateManager();
                return;
            }

            if (manager.palette == null)
            {
                EditorGUILayout.HelpBox("No Color Palette is assigned in Manager, fix for preview.", MessageType.Warning, true);
                if (GUILayout.Button("Take Me There!")) Selection.activeObject = manager;
                return;
            }
        }
        static void CreateManager()
        {
            var go = new GameObject("ColorPalette Manager");
            go.AddComponent<ColorPaletteManager>();
        }

        #region Context Menu Additions

        const string k_GraphicPath = "CONTEXT/Graphic/🧭 Color By Palette";
        const string k_ButtonPath = "CONTEXT/Button/🧭 Color By Palette";
        const string k_RendererPath = "CONTEXT/Renderer/🧭 Color By Palette";

        [MenuItem(k_GraphicPath, true)]
        [MenuItem(k_ButtonPath, true)]
        [MenuItem(k_RendererPath, true)]
        static bool CanBeRecolored(MenuCommand menuCommand)
        {
            var gameObject = ((Component)menuCommand.context).gameObject;
            return gameObject.GetComponent<ColorByPalette>() == null;
        }

        [MenuItem(k_GraphicPath)]
        static void AddComponentToGraphic(MenuCommand command) => AddComponent<ColorGraphicByPalette>((Component)command.context);

        [MenuItem(k_ButtonPath)]
        static void AddComponentToButton(MenuCommand command) => AddComponent<ColorButtonByPaletteDetailed>((Component)command.context);

        [MenuItem(k_RendererPath)]
        static void AddComponentToRenderer(MenuCommand command) => AddComponent<ColorRendererByPalette>((Component)command.context);

        static void AddComponent<T>(Component target) where T : ColorByPalette
        {
            if (target.gameObject.GetComponent<ColorByPalette>() != null) return;
            Undo.AddComponent<T>(target.gameObject);
        }

        #endregion
    }
}
