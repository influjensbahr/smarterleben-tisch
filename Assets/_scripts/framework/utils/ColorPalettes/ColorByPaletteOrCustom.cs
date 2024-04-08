using OTBT.Framework.Gameplay;
using OTBT.Framework.Utils.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.ColorPalettes
{
    /// <summary>
    /// Class that can be used in other scripts as replacement for Color that lets us choose from a palette or use a custom color
    /// </summary>
    public class ColorByPaletteOrCustom 
    {
        [SerializeField] bool m_UseColorPalette = true;
        [SerializeField] Color m_DefaultColor = Color.black;
        [SerializeField] Color m_PaletteColor = Color.black;
        [SerializeField] int m_SelectedPaletteIndex = 0;

        public Color color => m_UseColorPalette ? m_PaletteColor : m_DefaultColor;

        public void Register()
        {
            if (!m_UseColorPalette) return;
            ColorPaletteManager.instance.onPaletteLoad += ColorObject;
        }

        public void Unregister()
        {
            if (!m_UseColorPalette) return;
            ColorPaletteManager.instance.onPaletteLoad -= ColorObject;
        }

        public void RecolorNow()
        {
            var manager = ColorPaletteManager.instance;
            if (manager == null) return;
            if (manager.palette == null) return;

            ColorObject(manager.palette);
        }

        public void ColorObject(ColorPalette palette)
        {
            m_PaletteColor = palette.GetColor(m_SelectedPaletteIndex);
        }

#if UNITY_EDITOR

        public void DrawGUI()
        {
            m_UseColorPalette = EditorGUILayout.Toggle("Use color palette", m_UseColorPalette);
            if (m_UseColorPalette)
            {
                //m_SelectedPaletteIndex = DrawColorSelector(m_SelectedPaletteIndex);
            }
            else
            {
                m_DefaultColor = EditorGUILayout.ColorField("Speaker color", m_DefaultColor);
            }
        }

      

#endif

    }
}
