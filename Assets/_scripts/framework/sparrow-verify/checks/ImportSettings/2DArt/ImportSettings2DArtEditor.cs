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
    [CustomEditor(typeof(ImportSettings2DArt))]
    public class ImportSettingsSpritesEditor : Editor
    {
        private bool m_advancedSettingsFoldout;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ImportSettings2DArt t = (ImportSettings2DArt)target;
            if(t == null) return;
            bool isChanged = false;
            
            isChanged |= t.textureType.OnCustomInspectorChange(serializedObject.FindProperty("textureType.value"), "Texture Type");
            
            switch (t.textureType.value)
            {
                case TextureImporterType.Sprite:
                {
                    EditorGUI.BeginDisabledGroup(true);
                    isChanged |= t.textureShape.OnCustomInspectorChange(serializedObject.FindProperty("textureShape.value"), "Texture Shape");
                    EditorGUI.EndDisabledGroup();
                    isChanged |= t.spriteMode.OnCustomInspectorChange(serializedObject.FindProperty("spriteMode.value"), "Sprite Mode");
                    
                    // Display settings based on Sprite Mode
                    switch (t.spriteMode.value)
                    {
                        case SpriteImportMode.Single:
                            isChanged |= t.pixelsPerUnit.OnCustomInspectorChange(serializedObject.FindProperty("pixelsPerUnit.value"), "Pixels Per Unit");
                            isChanged |= t.meshType.OnCustomInspectorChange(serializedObject.FindProperty("meshType.value"), "Mesh Type");
                            isChanged |= t.pivot.OnCustomInspectorChange(serializedObject.FindProperty("pivot.value"), "Pivot");
                            isChanged |= t.generatePhysicsShape.OnCustomInspectorChange(serializedObject.FindProperty("generatePhysicsShape.value"), "Generate Physics Shape");
                            break;
                        case SpriteImportMode.Multiple:
                            isChanged |= t.pixelsPerUnit.OnCustomInspectorChange(serializedObject.FindProperty("pixelsPerUnit.value"), "Pixels Per Unit");
                            isChanged |= t.meshType.OnCustomInspectorChange(serializedObject.FindProperty("meshType.value"), "Mesh Type");
                            isChanged |= t.extrudeEdges.OnCustomInspectorChange(serializedObject.FindProperty("extrudeEdges.value"), "Extrude Edges");
                            isChanged |= t.generatePhysicsShape.OnCustomInspectorChange(serializedObject.FindProperty("generatePhysicsShape.value"), "Generate Physics Shape");
                            break;
                        case SpriteImportMode.None:
                            break;
                        case SpriteImportMode.Polygon:
                            isChanged |= t.pixelsPerUnit.OnCustomInspectorChange(serializedObject.FindProperty("pixelsPerUnit.value"), "Pixels Per Unit");
                            isChanged |= t.extrudeEdges.OnCustomInspectorChange(serializedObject.FindProperty("extrudeEdges.value"), "Extrude Edges");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    // Display Advanced settings in a foldout
                    m_advancedSettingsFoldout = EditorGUILayout.Foldout(m_advancedSettingsFoldout, "Advanced");
                    isChanged |= ShowAdvancedSettings(TextureImporterType.Sprite, t);

                    if (t.generateMipMap.value)
                        isChanged |= GenerateMipMaps(t);
                    
                    break;
                }
                case TextureImporterType.Default:
                    isChanged |= t.textureShape.OnCustomInspectorChange(serializedObject.FindProperty("textureShape.value"), "Texture Shape");
                    isChanged |= t.sRGBTexture.OnCustomInspectorChange(serializedObject.FindProperty("sRGBTexture.value"), "sRGB Texture");
                    isChanged |= t.alphaSource.OnCustomInspectorChange(serializedObject.FindProperty("alphaSource.value"), "Alpha Source");
                    EditorGUI.BeginDisabledGroup(t.alphaSource.value == TextureImporterAlphaSource.None);
                    isChanged |= t.alphaIsTransparency.OnCustomInspectorChange(serializedObject.FindProperty("alphaIsTransparency.value"), "Alpha Is Transparency");
                    EditorGUI.EndDisabledGroup();
                    isChanged |= t.ignorePNGfileGamma.OnCustomInspectorChange(serializedObject.FindProperty("ignorePNGGamma.value"), "Ignore PNG Gamma");
                    
                    // Display Advanced settings in a foldout
                    m_advancedSettingsFoldout = EditorGUILayout.Foldout(m_advancedSettingsFoldout, "Advanced");
                    isChanged |= ShowAdvancedSettings(TextureImporterType.Default, t);
                    
                    if (t.generateMipMap.value)
                        isChanged |= GenerateMipMaps(t);
                    
                    break;
                case TextureImporterType.NormalMap:
                    isChanged |= t.textureShape.OnCustomInspectorChange(serializedObject.FindProperty("textureShape.value"), "Texture Shape");
                    isChanged |= t.ignorePNGGamma.OnCustomInspectorChange(serializedObject.FindProperty("ignorePNGGamma.value"), "Ignore PNG Gamma");
                    isChanged |= t.createFromGrayscale.OnCustomInspectorChange(serializedObject.FindProperty("createFromGrayscale.value"), "Create From Grayscale");
                    
                    // Display Advanced settings in a foldout
                    m_advancedSettingsFoldout = EditorGUILayout.Foldout(m_advancedSettingsFoldout, "Advanced");
                    isChanged |= ShowAdvancedSettings(TextureImporterType.NormalMap, t);
                    
                    if (t.generateMipMap.value)
                        isChanged |= GenerateMipMaps(t);
                    break;
                case TextureImporterType.GUI:
                    
                    // Display Advanced settings in a foldout
                    m_advancedSettingsFoldout = EditorGUILayout.Foldout(m_advancedSettingsFoldout, "Advanced");
                    isChanged |= ShowAdvancedSettings(TextureImporterType.GUI, t);
                    
                    if (t.generateMipMap.value)
                        isChanged |= GenerateMipMaps(t);
                    break;
                case TextureImporterType.Cursor:
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("textureShape"));
                    isChanged |= t.textureShape.OnCustomInspectorChange(serializedObject.FindProperty("textureShape.value"), "Texture Shape");
                    EditorGUI.EndDisabledGroup();
                    
                    // Display Advanced settings in a foldout
                    m_advancedSettingsFoldout = EditorGUILayout.Foldout(m_advancedSettingsFoldout, "Advanced");
                    isChanged |= ShowAdvancedSettings(TextureImporterType.Cursor, t);
                    
                    if (t.generateMipMap.value)
                        isChanged |= GenerateMipMaps(t);
                    break;
                case TextureImporterType.Cookie:
                    EditorGUI.BeginDisabledGroup(true);
                    isChanged |= t.textureShape.OnCustomInspectorChange(serializedObject.FindProperty("textureShape.value"), "Texture Shape");
                    EditorGUI.EndDisabledGroup();
                    
                    isChanged |= t.lightType.OnCustomInspectorChange(serializedObject.FindProperty("lightType.value"), "Light Type");
                    isChanged |= t.alphaSource.OnCustomInspectorChange(serializedObject.FindProperty("alphaSource.value"), "Alpha Source");
                    
                    EditorGUI.BeginDisabledGroup(t.alphaSource.value == TextureImporterAlphaSource.None);
                    isChanged |= t.alphaIsTransparency.OnCustomInspectorChange(serializedObject.FindProperty("alphaIsTransparency.value"), "Alpha Is Transparency");
                    EditorGUI.EndDisabledGroup();
                    
                    isChanged |= t.ignorePNGGamma.OnCustomInspectorChange(serializedObject.FindProperty("ignorePNGGamma.value"), "Ignore PNG Gamma");
                    
                    // Display Advanced settings in a foldout
                    m_advancedSettingsFoldout = EditorGUILayout.Foldout(m_advancedSettingsFoldout, "Advanced");
                    isChanged |= ShowAdvancedSettings(TextureImporterType.Cookie, t);
                    
                    if (t.generateMipMap.value)
                        isChanged |= GenerateMipMaps(t);
                    
                    break;
                case TextureImporterType.Lightmap:
                    EditorGUI.BeginDisabledGroup(true);
                    isChanged |= t.textureShape.OnCustomInspectorChange(serializedObject.FindProperty("textureShape.value"), "Texture Shape");
                    EditorGUI.EndDisabledGroup();
                    
                    // Display Advanced settings in a foldout
                    m_advancedSettingsFoldout = EditorGUILayout.Foldout(m_advancedSettingsFoldout, "Advanced");
                    isChanged |= ShowAdvancedSettings(TextureImporterType.Lightmap, t);
                    
                    if (t.generateMipMap.value)
                        isChanged |= GenerateMipMaps(t);
                    
                    break;
                case TextureImporterType.DirectionalLightmap:
                    EditorGUI.BeginDisabledGroup(true);
                    isChanged |= t.textureShape.OnCustomInspectorChange(serializedObject.FindProperty("textureShape.value"), "Texture Shape");
                    EditorGUI.EndDisabledGroup();
                    
                    // Display Advanced settings in a foldout
                    m_advancedSettingsFoldout = EditorGUILayout.Foldout(m_advancedSettingsFoldout, "Advanced");
                    isChanged |= ShowAdvancedSettings(TextureImporterType.DirectionalLightmap, t);
                    
                    if (t.generateMipMap.value)
                        isChanged |= GenerateMipMaps(t);
                    
                    break;
                case TextureImporterType.Shadowmask:
                    EditorGUI.BeginDisabledGroup(true);
                    isChanged |= t.textureShape.OnCustomInspectorChange(serializedObject.FindProperty("textureShape.value"), "Texture Shape");
                    EditorGUI.EndDisabledGroup();
                    
                    // Display Advanced settings in a foldout
                    m_advancedSettingsFoldout = EditorGUILayout.Foldout(m_advancedSettingsFoldout, "Advanced");
                    isChanged |= ShowAdvancedSettings(TextureImporterType.Shadowmask, t);
                    
                    if (t.generateMipMap.value)
                        isChanged |= GenerateMipMaps(t);
                    
                    break;
                case TextureImporterType.SingleChannel:
                    isChanged |= t.textureShape.OnCustomInspectorChange(serializedObject.FindProperty("textureShape.value"), "Texture Shape");
                    isChanged |= t.channel.OnCustomInspectorChange(serializedObject.FindProperty("channel.value"), "Channel");
                    isChanged |= t.alphaSource.OnCustomInspectorChange(serializedObject.FindProperty("alphaSource.value"), "Alpha Source");
                    EditorGUI.BeginDisabledGroup(t.alphaSource.value == TextureImporterAlphaSource.None);
                    isChanged |= t.alphaIsTransparency.OnCustomInspectorChange(serializedObject.FindProperty("alphaIsTransparency.value"), "Alpha Is Transparency");
                    EditorGUI.EndDisabledGroup();
                    isChanged |= t.ignorePNGGamma.OnCustomInspectorChange(serializedObject.FindProperty("ignorePNGGamma.value"), "Ignore PNG Gamma");
                    
                    // Display Advanced settings in a foldout
                    m_advancedSettingsFoldout = EditorGUILayout.Foldout(m_advancedSettingsFoldout, "Advanced");
                    isChanged |= ShowAdvancedSettings(TextureImporterType.SingleChannel, t);
                    
                    if (t.generateMipMap.value)
                        isChanged |= GenerateMipMaps(t);
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            isChanged |= t.wrapMode.OnCustomInspectorChange(serializedObject.FindProperty("wrapMode.value"), "Wrap Mode");
            isChanged |= t.filterMode.OnCustomInspectorChange(serializedObject.FindProperty("filterMode.value"), "Filter Mode");
            
            EditorGUI.BeginDisabledGroup(t.textureType.value is TextureImporterType.Cursor || t.textureType.value is TextureImporterType.DirectionalLightmap);
            isChanged |= t.anisoLevel.OnCustomInspectorChange(serializedObject.FindProperty("anisoLevel.value"), "Aniso Level");
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
            
            if(isChanged) EditorUtility.SetDirty(serializedObject.targetObject);
        }

        private bool GenerateMipMaps(ImportSettings2DArt t)
        { 
            var isChanged = false;
            isChanged |= t.borderMipMaps.OnCustomInspectorChange(serializedObject.FindProperty("borderMipMaps.value"), "Border Mip Maps");
            isChanged |= t.mipmapFilter.OnCustomInspectorChange(serializedObject.FindProperty("mipmapFilter.value"), "Mip Map Filter");
            isChanged |= t.mipMapsPreserveCoverage.OnCustomInspectorChange(serializedObject.FindProperty("mipMapsPreserveCoverage.value"), "Mip Maps Preserve Coverage");
            if (t.mipMapsPreserveCoverage.value)
            {
                isChanged |= t.alphaCutoffValue.OnCustomInspectorChange(serializedObject.FindProperty("alphaCutoffValue.value"), "Alpha Cutoff Value");
            }
            
            isChanged |= t.fadeoutMipMaps.OnCustomInspectorChange(serializedObject.FindProperty("fadeoutMipMaps.value"), "Fadeout Mip Maps");

            if (t.fadeoutMipMaps.value)
            {
                isChanged |= t.start.OnCustomInspectorChange(serializedObject.FindProperty("start.value"), "Start");
                isChanged |= t.end.OnCustomInspectorChange(serializedObject.FindProperty("end.value"), "End");
            }
            
            GUILayout.Space(10);
            
            return isChanged;
        }

        private bool ShowAdvancedSettings(TextureImporterType textureType, ImportSettings2DArt t)
        {
            if (!m_advancedSettingsFoldout) return false;
            var isChanged = false;
            if(!(textureType is TextureImporterType.Cursor) &&
               !(textureType is TextureImporterType.Cookie) &&
               !(textureType is TextureImporterType.Lightmap) &&
               !(textureType is TextureImporterType.DirectionalLightmap) &&
               !(textureType is TextureImporterType.Shadowmask) &&
               !(textureType is TextureImporterType.SingleChannel) &&
               !(textureType is TextureImporterType.NormalMap) &&
               !(textureType is TextureImporterType.Default))
                isChanged |= t.sRGBTexture.OnCustomInspectorChange(serializedObject.FindProperty("sRGBTexture.value"), "sRGB Texture");

            if (!(textureType is TextureImporterType.Cookie) &&
                !(textureType is TextureImporterType.DirectionalLightmap) &&
                !(textureType is TextureImporterType.SingleChannel) &&
                !(textureType is TextureImporterType.NormalMap) &&
                !(textureType is TextureImporterType.Default))
            {
                if (!(textureType is TextureImporterType.Lightmap) &&
                    !(textureType is TextureImporterType.Shadowmask))
                {
                    isChanged |= t.alphaSource.OnCustomInspectorChange(serializedObject.FindProperty("alphaSource.value"), "Alpha Source");
                    EditorGUI.BeginDisabledGroup(t.alphaSource.value == TextureImporterAlphaSource.None);
                    isChanged |= t.alphaIsTransparency.OnCustomInspectorChange(serializedObject.FindProperty("alphaIsTransparency.value"), "Alpha Is Transparency");
                    EditorGUI.EndDisabledGroup();
                }
                
                isChanged |= t.ignorePNGGamma.OnCustomInspectorChange(serializedObject.FindProperty("ignorePNGGamma.value"), "Ignore PNG Gamma");
            }
            
            isChanged |= t.readWriteEnabled.OnCustomInspectorChange(serializedObject.FindProperty("readWriteEnabled.value"), "Read/Write Enabled");
            isChanged |= t.streamingMipMaps.OnCustomInspectorChange(serializedObject.FindProperty("streamingMipMaps.value"), "Streaming Mip Maps");
            if (t.streamingMipMaps.value)
            {
                isChanged |= t.streamingMipMapPriority.OnCustomInspectorChange(serializedObject.FindProperty("streamingMipMapPriority.value"), "Streaming Mip Map Priority");
            }

            if (textureType is TextureImporterType.NormalMap ||textureType is TextureImporterType.Default)
            {
                isChanged |= t.virtualTextureOnly.OnCustomInspectorChange(serializedObject.FindProperty("virtualTextureOnly.value"), "Virtual Texture Only");
            }
            isChanged |= t.generateMipMap.OnCustomInspectorChange(serializedObject.FindProperty("generateMipMap.value"), "Generate Mip Map");
            
            EditorGUILayout.Space(10);
            
            return isChanged;
        }
    }
}
#endif