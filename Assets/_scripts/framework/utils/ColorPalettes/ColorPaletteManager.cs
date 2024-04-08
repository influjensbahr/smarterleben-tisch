//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using System;
using System.Collections.Generic;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using UnityEngine;

namespace OTBT.Framework.ColorPalettes
{
    public class ColorPaletteManager : Singleton<ColorPaletteManager>, IVerify
    {
        [SerializeField, HideInInspector] ColorPalette m_ColorPalette;
        [SerializeField] List<ColorPalette> m_ColorPaletteList = new();
        [SerializeField] List<string> m_ColorSlotNames = new() {
            "Primary",
            "Secondary",
            "Tertiary",
        };

        public Action<ColorPalette> onPaletteLoad;
        public ColorPalette palette => m_ColorPalette == null ? (m_ColorPaletteList.Count > 0 ? m_ColorPaletteList[0] : null) : m_ColorPalette;
        public List<ColorPalette> palettes => m_ColorPaletteList;
        public List<string> colorSlotNames => m_ColorSlotNames;

        void Awake()
        {
            WhenReady(() => SetPalette(palette));
        }

        public void SetPalette(ColorPalette palette)
        {
            m_ColorPalette = palette;
            onPaletteLoad?.Invoke(palette);
        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.CheckElementsNotNull(m_ColorPaletteList, nameof(m_ColorPaletteList), this);
        }
    }
}
