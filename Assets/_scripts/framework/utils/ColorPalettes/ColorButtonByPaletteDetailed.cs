// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using System;
using UnityEngine;
using UnityEngine.UI;
namespace OTBT.Framework.ColorPalettes
{
    [RequireComponent(typeof(Button))]
    public class ColorButtonByPaletteDetailed : ColorByPalette
    {
        [Flags]
        enum Slot
        {
            Normal = 1,
            Highlighted = 2,
            Pressed = 4,
            Selected = 8,
            Disabled = 16
        }

        [SerializeField] Button m_Button;
        [SerializeField] Slot m_Slot;

        void OnValidate()
        {
            if (m_Button == null) m_Button = GetComponent<Button>();
        }

        protected override void ApplyColor(Color color)
        {
            var block = m_Button.colors;

            if (m_Slot.HasFlag(Slot.Normal)) block.normalColor = color;
            if (m_Slot.HasFlag(Slot.Highlighted)) block.highlightedColor = color;
            if (m_Slot.HasFlag(Slot.Pressed)) block.pressedColor = color;
            if (m_Slot.HasFlag(Slot.Selected)) block.selectedColor = color;
            if (m_Slot.HasFlag(Slot.Disabled)) block.disabledColor = color;

            m_Button.colors = block;
        }
    }
}
