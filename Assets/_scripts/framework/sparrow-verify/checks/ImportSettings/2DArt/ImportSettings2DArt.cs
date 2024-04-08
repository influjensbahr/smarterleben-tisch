//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Marc Freitag
//

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [CreateAssetMenu(fileName = "importSettingsSprite", menuName = "Sparrow/Verification/Import-Settings/Import Settings 2D Art",
        order = 0)]
    [Serializable]
    public class ImportSettings2DArt : ImportSettingsBase 
    {
        [SerializeField]public CustomPropertyField<TextureImporterType> textureType = new CustomPropertyField<TextureImporterType>(TextureImporterType.Sprite);
        [SerializeField]public CustomPropertyField<TextureImporterShape> textureShape = new CustomPropertyField<TextureImporterShape>(TextureImporterShape.Texture2D);
        [SerializeField]public CustomPropertyField<SpriteImportMode> spriteMode = new CustomPropertyField<SpriteImportMode>(SpriteImportMode.Single);
        [SerializeField]public CustomPropertyField<int> pixelsPerUnit; 
        [SerializeField]public CustomPropertyField<SpriteMeshType> meshType = new CustomPropertyField<SpriteMeshType>(SpriteMeshType.FullRect);
        [SerializeField][Range(0.0f, 32.0f)] public CustomPropertyField<uint> extrudeEdges = new CustomPropertyField<uint>(1);
        [SerializeField]public CustomPropertyField<Vector2> pivot = new CustomPropertyField<Vector2>(new Vector2(0.5f, 0.5f));
        [SerializeField]public CustomPropertyField<bool> generatePhysicsShape = new CustomPropertyField<bool>(false);
        [SerializeField]public CustomPropertyField<bool> sRGBTexture = new CustomPropertyField<bool>(true);
        [SerializeField]public CustomPropertyField<TextureImporterAlphaSource> alphaSource = new CustomPropertyField<TextureImporterAlphaSource>(TextureImporterAlphaSource.FromInput);
        [SerializeField]public CustomPropertyField<bool> alphaIsTransparency = new CustomPropertyField<bool>(true);
        [SerializeField]public CustomPropertyField<bool> ignorePNGGamma;
        [SerializeField]public CustomPropertyField<bool> readWriteEnabled;
        [SerializeField]public CustomPropertyField<bool> generateMipMap;
        [SerializeField]public CustomPropertyField<bool> borderMipMaps;
        [SerializeField]public CustomPropertyField<TextureImporterMipFilter> mipmapFilter = new CustomPropertyField<TextureImporterMipFilter>(TextureImporterMipFilter.BoxFilter);
        [SerializeField]public CustomPropertyField<bool> mipMapsPreserveCoverage;
        [SerializeField]public CustomPropertyField<float> alphaCutoffValue;
        [SerializeField]public CustomPropertyField<bool> fadeoutMipMaps;
        [SerializeField][Range(0.0f, 1.0f)] public CustomPropertyField<int> start;
        [SerializeField][Range(0.0f, 1.0f)] public CustomPropertyField<int> end;
        [SerializeField]public CustomPropertyField<TextureWrapMode> wrapMode = new CustomPropertyField<TextureWrapMode>(TextureWrapMode.Clamp);
        [SerializeField]public CustomPropertyField<TextureWrapMode> wrapModeU = new CustomPropertyField<TextureWrapMode>(TextureWrapMode.MirrorOnce);
        [SerializeField]public CustomPropertyField<TextureWrapMode> wrapModeV = new CustomPropertyField<TextureWrapMode>(TextureWrapMode.MirrorOnce);
        [SerializeField]public CustomPropertyField<bool> streamingMipMaps;
        [SerializeField]public CustomPropertyField<int> streamingMipMapPriority;
        [SerializeField]public CustomPropertyField<bool> virtualTextureOnly;
        [SerializeField]public CustomPropertyField<LightTypes> lightType = new CustomPropertyField<LightTypes>(LightTypes.Spotlight);
        [SerializeField]public CustomPropertyField<bool> ignorePNGfileGamma;
        [SerializeField]public CustomPropertyField<bool> createFromGrayscale;
        [SerializeField]public CustomPropertyField<TextureImporterSingleChannelComponent> channel = new CustomPropertyField<TextureImporterSingleChannelComponent>(TextureImporterSingleChannelComponent.Red);
        [SerializeField]public CustomPropertyField<FilterMode> filterMode = new CustomPropertyField<FilterMode>(FilterMode.Bilinear);
        [SerializeField][Range(0.0f, 10.0f)] public CustomPropertyField<int> anisoLevel = new CustomPropertyField<int>(1);
    }

    public enum LightTypes
    {
        Spotlight = 0,
        Directional = 1,
        Point = 2,
    }
}
#endif