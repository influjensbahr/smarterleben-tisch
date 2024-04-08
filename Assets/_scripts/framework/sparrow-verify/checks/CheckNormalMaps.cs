//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR

using UnityEngine;
using System;
using UnityEditor;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckNormalMaps : VerifyCheckBase
    {
        public override string description => "Normal Map Validity";
        public override string longDescription => "Checks if textures assigned in normal map slots have their TextureType set to NormalMap.";

        public override void PerformCheck(Material mat)
        {
            for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
            {
                if ((mat.shader.GetPropertyFlags(i) & UnityEngine.Rendering.ShaderPropertyFlags.Normal) != 0)
                {
                    var propertyName = mat.shader.GetPropertyName(i);
                    var assignedTexture = mat.GetTexture(propertyName);

                    if (assignedTexture == null)
                        continue;

                    var texturePath = AssetDatabase.GetAssetPath(assignedTexture);
                    var textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                    if (textureImporter.textureType != TextureImporterType.NormalMap)
                    {
                        AddFailedCheck($"Normal map with wrong TextureType assigned found: {textureImporter.name} assigned to material {mat.name}", mat);
                    }
                }
            }
        }

    }
}
#endif