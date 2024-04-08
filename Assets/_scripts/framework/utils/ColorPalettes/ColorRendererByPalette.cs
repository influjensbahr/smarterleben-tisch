//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using UnityEngine;

namespace OTBT.Framework.ColorPalettes
{
    [RequireComponent(typeof(Renderer))]
    public class ColorRendererByPalette : ColorByPalette
    {
        [SerializeField] Renderer m_Renderer;
        [SerializeField] string m_Property = "_Color";
        void OnValidate()
        {
            if (m_Renderer == null) m_Renderer = GetComponent<Renderer>();
        }
        
        protected override void ApplyColor(Color color)
        {
            var block = new MaterialPropertyBlock();
            m_Renderer.GetPropertyBlock(block);
            block.SetColor(m_Property, color);
            m_Renderer.SetPropertyBlock(block);
        }
    }
}
