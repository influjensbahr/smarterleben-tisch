//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using Sparrow.Verification;
using UnityEngine;

namespace OTBT.Framework.ColorPalettes
{
    public abstract class ColorByPalette : MonoBehaviour, IVerify
    {
        [SerializeField] protected int m_Index;

        void Awake() => ColorPaletteManager.instance.onPaletteLoad += ColorObject;
        void OnDestroy() => ColorPaletteManager.instance.onPaletteLoad -= ColorObject;

        void ColorObject(ColorPalette palette)
        {
            var color = palette.GetColor(m_Index);
            ApplyColor(color);
        }

        private void OnDrawGizmos() {}
        private void OnDrawGizmosSelected() {}

        [ContextMenu("Recolor now")]
        public void RecolorNow()
        {
            var manager = ColorPaletteManager.instance;
            if (manager == null) return;
            if (manager.palette == null) return;

            ColorObject(manager.palette);
        }

        protected abstract void ApplyColor(Color color);


        public void Verify(CheckVerifyInterface checker)
        {
            
        }
    }
}
