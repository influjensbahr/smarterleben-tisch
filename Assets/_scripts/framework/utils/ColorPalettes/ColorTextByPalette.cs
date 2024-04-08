//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.ColorPalettes
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ColorTextByPalette : ColorByPalette
    {
        [SerializeField] TextMeshProUGUI m_Text;

        void OnValidate()
        {
            if (m_Text == null) m_Text = GetComponent<TextMeshProUGUI>();
        }
        
        protected override void ApplyColor(Color color)
        {
            m_Text.color = color;
        }
    }
}
