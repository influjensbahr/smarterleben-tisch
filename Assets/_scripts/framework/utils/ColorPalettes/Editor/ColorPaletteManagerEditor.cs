//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System;
using UnityEditor;
using UnityEngine;
using OTBT.Framework.Utils.Editor;

namespace OTBT.Framework.ColorPalettes.Editor
{
    [CustomEditor(typeof(ColorPaletteManager))]
    public class ColorPaletteManagerEditor : UnityEditor.Editor
    {
        ColorPaletteManager m_Manager;
        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader("Color Palettes", "https://wiki.beatentrack.games/doc/color-palettes-YntYuMnbcL");

            if (m_Manager == null)
                m_Manager = target as ColorPaletteManager;

            if (m_Manager.palette != null)
            {
                EditorGUILayout.LabelField("Current Palette", EditorStyles.boldLabel);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(m_Manager.palette.name, EditorStyles.centeredGreyMiniLabel);
                    m_Manager.palette.DrawEditorPreview();
                    EditorUtils.Space();
                }
            }

            base.OnInspectorGUI();

            EditorUtils.Space();

            EditorGUILayout.LabelField("Palettes", EditorStyles.boldLabel);
            foreach (ColorPalette pal in m_Manager.palettes)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(pal.name, EditorStyles.centeredGreyMiniLabel);
                    pal.DrawEditorPreview();
                    if (GUILayout.Button("Activate"))
                    {
                        m_Manager.SetPalette(pal);
                        RepaintAll();
                    }
                }
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Repaint All In Scene")) RepaintAll();

            EditorUtils.DrawVerify(m_Manager);
            EditorUtils.EndColoredEditor();
        }

        void RepaintAll()
        {
            foreach (ColorByPalette obj in FindObjectsOfType<ColorByPalette>())
                obj.RecolorNow();
        }
    }
}
