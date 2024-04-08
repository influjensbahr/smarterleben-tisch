//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckLargeTextureSizes : VerifyCheckBase
    {
        public override string description => "Large Texture Sizes";
        public override string longDescription => "Checks for extremely large texture sizes which might increase your build size.";
        // Maximum allowed texture size (in pixels). You can customize this value.
        [SerializeField] int m_MaxTextureSize = 2048;

        public override bool DrawSpecificProfileEditor()
        {
            int count = m_MaxTextureSize;
            m_MaxTextureSize = EditorGUILayout.IntField("Maximum Size", m_MaxTextureSize);
            return count != m_MaxTextureSize;
        }

        public override void PerformCheck(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            // Check if the texture width or height exceeds the maximum allowed size
            if (texture.width > m_MaxTextureSize || texture.height > m_MaxTextureSize)
            {
                AddFailedCheck("Large Texture Size", texture).WithSeverity(VerifyResult.Severity.Warning);
            }
        }
    }
}
#endif
