//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using OTBT.Framework.Utils.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.ColorPalettes
{
    [CreateAssetMenu(menuName = "OTBT/Utils/Color Palette", fileName = "new Color Palette")]
    public class ColorPalette : ScriptableObject
    {
#if UNITY_EDITOR
        public void DrawEditorPreview()
        {
            if (ColorPaletteManager.instance.colorSlotNames.Count == 0) return;

            using var colors = new EditorGUILayout.HorizontalScope();
            for (int i = 0; i < ColorPaletteManager.instance.colorSlotNames.Count; i++)
            {
                if (m_Colors.Count <= i) continue;
                
                using var scope = new EditorGUILayout.VerticalScope();

                Rect labelRect = EditorGUILayout.GetControlRect(false);
                EditorGUI.LabelField(labelRect, ColorPaletteManager.instance.colorSlotNames[i], EditorStyles.centeredGreyMiniLabel);

                Rect colorRect = EditorGUILayout.GetControlRect(false);
                EditorUtils.GUIDrawRect(colorRect, m_Colors[i]);
            }
        }

        public void Add()
        {
            m_Colors.Add(new Color());
            EditorUtility.SetDirty(this);
        }

        public void Remove()
        {
            if (m_Colors.Count > 1)
                m_Colors.RemoveAt(m_Colors.Count - 1);
        }
#endif

        static readonly Color ErrorColor = new(1, 0, 1);

        [SerializeField] List<Color> m_Colors = new() {
            Color.white,
        };

        public int count => m_Colors.Count;

        public Color GetColor(int index)
        {
            if (index < 0 || index >= m_Colors.Count)
            {
                return ErrorColor;
            }

            return m_Colors[index];
        }

        public Color GetColor(string identifier)
        {
            var index = ColorPaletteManager.instance.colorSlotNames.FindIndex(id => string.Equals(id, identifier, StringComparison.OrdinalIgnoreCase));
            return GetColor(index);
        }
    }
}
