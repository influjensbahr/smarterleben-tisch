//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.ColorPalettes
{
    [RequireComponent(typeof(Graphic))]
    public class ColorGraphicByPalette : ColorByPalette
    {
        [SerializeField] Graphic m_Graphic;

        void OnValidate()
        {
            if (m_Graphic == null) m_Graphic = GetComponent<Graphic>();
        }
        
        protected override void ApplyColor(Color color)
        {
            m_Graphic.color = color;
        }
    }
}
