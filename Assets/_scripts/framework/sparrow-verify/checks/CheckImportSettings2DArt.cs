//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Marc Freitag
//

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckImportSettings2DArt : VerifyCheckBase
    {
        public override string description => "Import settings: 2D art";
        public override string longDescription =>
            "Checks the import settings for sprites in your project. First, you'll need to provide some options on a per-folder basis.";
 
        [SerializeField] List<SingleFolder<ImportSettings2DArt>> m_Folders = new List<SingleFolder<ImportSettings2DArt>>();
        [SerializeField] List<ImportSettingsMismatch<ImportSettings2DArt>> m_MismatchedSettings = new List<ImportSettingsMismatch<ImportSettings2DArt>>();
        private const float m_TOLERANCE = 0.01f;

        List<string> m_ProcessedPaths = new List<string>();

        public override void PerformCheckForProject()
        {
            m_ProcessedPaths.Clear();
        }

        public override bool DrawSpecificProfileEditor()
        {
            var hasChanged = false;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Folder", GUILayout.Width(100)))
            {
                m_Folders.Add(new SingleFolder<ImportSettings2DArt>("", null));
            }
            
            EditorGUILayout.LabelField("Path", GUILayout.MinWidth(50), GUILayout.MaxWidth(200));
            EditorGUILayout.LabelField("Import Settings", GUILayout.MinWidth(50), GUILayout.MaxWidth(200));
            EditorGUILayout.LabelField("", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();
            
            // Add a default folder if there are none
            if (m_Folders.Count == 0) m_Folders.Add(new SingleFolder<ImportSettings2DArt>("", null));
            
            for (int i = 0; i < m_Folders.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var folderPath = m_Folders[i].folderPath;
                var importSettings = m_Folders[i].importSettings; 
        
                if (GUILayout.Button("Select Folder", GUILayout.Width(100)))
                {
                    var selectedFolder = EditorUtility.OpenFolderPanel("Select Folder", m_Folders[i].folderPath, "");
                    if (!string.IsNullOrEmpty(selectedFolder)) 
                    {
                        if (selectedFolder.StartsWith(Application.dataPath))
                            m_Folders[i].folderPath = "Assets" + selectedFolder.Substring(Application.dataPath.Length);
                        else
                            Debug.LogError("Folder must be in Assets folder");
                        
                        return true;
                    }
                    GUIUtility.ExitGUI();
                }
                m_Folders[i].folderPath = EditorGUILayout.TextField(m_Folders[i].folderPath, GUILayout.MinWidth(50), GUILayout.MaxWidth(200));
                m_Folders[i].importSettings = EditorGUILayout.ObjectField(m_Folders[i].importSettings, typeof(ImportSettings2DArt), false, GUILayout.MinWidth(50), GUILayout.MaxWidth(200)) as ImportSettings2DArt;
                
                if (!folderPath.Equals(m_Folders[i].folderPath) || importSettings != m_Folders[i].importSettings) hasChanged = true;
                 
                if (m_Folders.Count > 1 && GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    m_Folders.RemoveAt(i);
                }
                
                EditorGUILayout.EndHorizontal(); 
            }
            
            return hasChanged;
        }
        
        public override void PerformCheck(TextureImporter textureImporter)
        {
            var assetPath = AssetDatabase.GetAssetPath(textureImporter);
            if (m_ProcessedPaths.Contains(assetPath)) return;
            m_ProcessedPaths.Add(assetPath);

            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            assetPath = assetPath.Substring(0, assetPath.LastIndexOf("/", StringComparison.Ordinal));

            foreach (var folder in m_Folders)
            {
                var propertyMap = new Dictionary<string, Action>();
                var folderPath = folder.folderPath;

                m_MismatchedSettings.Clear();
                if(!assetPath.Equals(folderPath)) continue;
                
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) continue;
                CheckImportSettingsSprite(folder.importSettings, textureImporter, propertyMap);

                if (m_MismatchedSettings.Count <= 0) continue;

                foreach (var mismatch in m_MismatchedSettings)
                {
                    var combinedList = string.Join(", ", propertyMap.Keys.ToArray());
                    AddFailedCheck($"Inconsistent import settings: {combinedList}", asset,
                        () => { ApplyCorrectImportSettings(mismatch.TextureImporter, propertyMap); });
                }
            }
        }

        private void ApplyCorrectImportSettings(TextureImporter textureImporter, Dictionary<string, Action> pmap)
        {
             foreach (var kvp in pmap)
             {
                 kvp.Value.Invoke();
             }
             
             // Reimport the asset to apply the changes
             AssetDatabase.ImportAsset(textureImporter.assetPath, ImportAssetOptions.ForceUpdate);
        }

        private ImportSettingsMismatch<ImportSettings2DArt> CheckImportSettingsSprite(ImportSettings2DArt presetToUse, TextureImporter textureImporter, Dictionary<string, Action> pmap)
        {
            var assetPath = textureImporter.assetPath;
            var textureImporterSettings = new TextureImporterSettings();
            var mismatch = new ImportSettingsMismatch<ImportSettings2DArt>(presetToUse, textureImporter);
        
            if (textureImporter == null)
            {
                Debug.LogWarning($"Could not load AssetImporter for: {assetPath}");
                return mismatch;
            }
        
            textureImporter.ReadTextureSettings(textureImporterSettings);
        
            if (presetToUse == null)
            {
                var cutPath = assetPath.Substring(0, assetPath.LastIndexOf("/", StringComparison.Ordinal));
                Debug.LogWarning($"No preset for Import Settings 2D Art found. Select a Import Settings Object for {cutPath}");
                return mismatch;
            }
            
            //if (!importSettings.textureType.toggle) return m_wrongSettings.Count <= 0;
            if (textureImporter.textureType != presetToUse.textureType.value)
            {
                mismatch.AddMismatchedSetting("textureType");
                pmap.Add("TextureType", () => textureImporter.textureType = presetToUse.textureType.value);
            }
        
            switch (textureImporter.textureType)
            {
                case TextureImporterType.Sprite:
                {
                    if (textureImporter.spriteImportMode != presetToUse.spriteMode.value &&
                        presetToUse.spriteMode.toggle)
                    {
                        mismatch.AddMismatchedSetting("spriteMode");
                        pmap.Add("SpriteImportMode", () => textureImporter.spriteImportMode = presetToUse.spriteMode.value);
                    }

                    if (Math.Abs(textureImporter.spritePixelsPerUnit - presetToUse.pixelsPerUnit.value) > m_TOLERANCE &&
                        presetToUse.pixelsPerUnit.toggle)
                    {
                        mismatch.AddMismatchedSetting("pixelsPerUnit");
                        pmap.Add("SpritePixelsPerUnit", () => textureImporter.spritePixelsPerUnit = presetToUse.pixelsPerUnit.value);
                    }

                    if (textureImporterSettings.spriteExtrude != presetToUse.extrudeEdges.value &&
                        presetToUse.extrudeEdges.toggle)
                    {
                        mismatch.AddMismatchedSetting("extrudeEdges");
                        pmap.Add("SpriteExtrude", () => textureImporterSettings.spriteExtrude = presetToUse.extrudeEdges.value);
                    }
        
                    switch (textureImporter.spriteImportMode)
                    {
                        case SpriteImportMode.Single:
                        case SpriteImportMode.Multiple:
                            if (textureImporterSettings.spriteMeshType != presetToUse.meshType.value &&
                                presetToUse.meshType.toggle)
                            {
                                mismatch.AddMismatchedSetting("meshType");
                                pmap.Add("SpriteMeshType", () => textureImporterSettings.spriteMeshType = presetToUse.meshType.value);
                            }

                            if (!textureImporterSettings.spritePivot.Equals(presetToUse.pivot.value) &&
                                presetToUse.pivot.toggle)
                            {
                                mismatch.AddMismatchedSetting("pivot");
                                pmap.Add("SpritePivot", () => textureImporter.spritePivot = presetToUse.pivot.value);
                            }
                            break;
                        case SpriteImportMode.Polygon:
                            if (textureImporterSettings.spriteMeshType != SpriteMeshType.FullRect)
                            {
                                mismatch.AddMismatchedSetting("meshType");
                                pmap.Add("SpriteMeshType", () => textureImporterSettings.spriteMeshType = SpriteMeshType.FullRect);
                            }

                            if (!textureImporterSettings.spritePivot.Equals(new Vector2(0.5f, 0.5f)))
                            {
                                mismatch.AddMismatchedSetting("pivot");
                                pmap.Add("SpritePivot", () => textureImporter.spritePivot = new Vector2(0.5f, 0.5f));
                            }
                            break;
                        case SpriteImportMode.None:
                            Debug.Log("Sprite Import Mode is set to None.");
                            break;
                    }
                        
                    // MipMaps checks
                    CheckMipMapProperties(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    // WrapMode checks
                    CheckWrapMode(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    break;
                }
                case TextureImporterType.Default:
                    if (textureImporter.isReadable != presetToUse.readWriteEnabled.value && presetToUse.readWriteEnabled.toggle) mismatch.AddMismatchedSetting("readWriteEnabled");
                        
                    // TODO: Adding Checks for different Texture Shapes
                        
                    // MipMaps checks
                    CheckMipMapProperties(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    CheckWrapMode(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    break;
                case TextureImporterType.NormalMap:
                    if (textureImporterSettings.ignorePngGamma != presetToUse.ignorePNGfileGamma.value &&
                        presetToUse.ignorePNGfileGamma.toggle)
                    {
                        mismatch.AddMismatchedSetting("ignorePNGfileGamma");
                        pmap.Add("IgnorePNGGamma", () => textureImporter.ignorePngGamma = presetToUse.ignorePNGfileGamma.value);
                    }

                    if (textureImporterSettings.convertToNormalMap != presetToUse.createFromGrayscale.value &&
                        presetToUse.createFromGrayscale.toggle)
                    {
                        mismatch.AddMismatchedSetting("createFromGrayscale");
                        pmap.Add("ConvertToNormalMap", () => textureImporterSettings.convertToNormalMap = presetToUse.createFromGrayscale.value);
                    }
                    // MipMaps checks
                    CheckMipMapProperties(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    CheckWrapMode(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    break;
                case TextureImporterType.GUI:
                {
                    CheckMipMapProperties(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    CheckWrapMode(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    break;
                }
                case TextureImporterType.Cursor:
                {
                    CheckMipMapProperties(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    CheckWrapMode(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    break;
                }
                case TextureImporterType.Cookie:
                {
                    CheckMipMapProperties(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    CheckWrapMode(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    break;
                }
                case TextureImporterType.Lightmap:
                {
                    CheckMipMapProperties(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    CheckWrapMode(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    break;
                }
                case TextureImporterType.DirectionalLightmap:
                {
                    CheckMipMapProperties(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    CheckWrapMode(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    break;
                }
                case TextureImporterType.Shadowmask:
                {
                    CheckMipMapProperties(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    CheckWrapMode(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    break;
                }
                case TextureImporterType.SingleChannel:
                {
                    if (textureImporterSettings.singleChannelComponent != presetToUse.channel.value &&
                        presetToUse.channel.toggle)
                    {
                        mismatch.AddMismatchedSetting("channel");
                        pmap.Add("SingleChannelComponent", () => textureImporterSettings.singleChannelComponent = presetToUse.channel.value);
                    }
                    CheckMipMapProperties(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    CheckWrapMode(presetToUse, textureImporter, textureImporterSettings, mismatch, pmap);
                    break;
                }
                default:
                    Debug.Log("Texture Type not supported.");
                    break;
            }
            
            CheckCommonImportSettings(presetToUse, textureImporter, mismatch, pmap);
            
            if (mismatch.MismatchedSettings.Count > 0)
            {
                m_MismatchedSettings.Add(mismatch);
            }
            
            return mismatch;
        }

        private void CheckCommonImportSettings(ImportSettings2DArt importSettings, TextureImporter textureImporter, ImportSettingsMismatch<ImportSettings2DArt> mismatch, Dictionary<string, Action> pmap)
        {
            if (textureImporter.textureShape != importSettings.textureShape.value && importSettings.textureShape.toggle)
            {
                mismatch.AddMismatchedSetting("textureShape");
                pmap.Add("TextureShape", () => textureImporter.textureShape = importSettings.textureShape.value);
            }

            if (textureImporter.sRGBTexture != importSettings.sRGBTexture.value && importSettings.sRGBTexture.toggle)
            {
                mismatch.AddMismatchedSetting("sRGBTexture"); 
                pmap.Add("SRGBTexture", () => textureImporter.sRGBTexture = importSettings.sRGBTexture.value);
            }

            if (textureImporter.alphaSource != importSettings.alphaSource.value && importSettings.alphaSource.toggle)
            {
                mismatch.AddMismatchedSetting("alphaSource");
                pmap.Add("AlphaSource", () => textureImporter.alphaSource = importSettings.alphaSource.value);
            }

            if (textureImporter.alphaIsTransparency != importSettings.alphaIsTransparency.value &&
                importSettings.alphaIsTransparency.toggle)
            {
                mismatch.AddMismatchedSetting("alphaIsTransparency");
                pmap.Add("AlphaIsTransparency", () => textureImporter.alphaIsTransparency = importSettings.alphaIsTransparency.value);
            }

            if (textureImporter.ignorePngGamma != importSettings.ignorePNGGamma.value &&
                importSettings.ignorePNGGamma.toggle)
            {
                mismatch.AddMismatchedSetting("ignorePNGGamma");
                pmap.Add("IgnorePNGGamma", () => textureImporter.ignorePngGamma = importSettings.ignorePNGGamma.value);
            }

            if (textureImporter.isReadable != importSettings.readWriteEnabled.value &&
                importSettings.readWriteEnabled.toggle)
            {
                mismatch.AddMismatchedSetting("readWriteEnabled");
                pmap.Add("IsReadable", () => textureImporter.isReadable = importSettings.readWriteEnabled.value);
            }

            if (textureImporter.filterMode != importSettings.filterMode.value && importSettings.filterMode.toggle)
            {
                mismatch.AddMismatchedSetting("filterMode");
                pmap.Add("FilterMode", () => textureImporter.filterMode = importSettings.filterMode.value);
            }

            if (textureImporter.anisoLevel != importSettings.anisoLevel.value && importSettings.anisoLevel.toggle)
            {
                mismatch.AddMismatchedSetting("anisoLevel");
                pmap.Add("AnisoLevel", () => textureImporter.anisoLevel = importSettings.anisoLevel.value);
            }
        }

        private void CheckMipMapProperties(ImportSettings2DArt importSettings, TextureImporter textureImporter, TextureImporterSettings textureImporterSettings, ImportSettingsMismatch<ImportSettings2DArt> mismatch, Dictionary<string, Action> pmap)
        {
            if (textureImporterSettings.streamingMipmaps != importSettings.streamingMipMaps.value &&
                importSettings.streamingMipMaps.toggle)
            {
                mismatch.AddMismatchedSetting("streamingMipMaps");
                pmap.Add("StreamingMipMaps", () => textureImporterSettings.streamingMipmaps = importSettings.streamingMipMaps.value);
            }
            else
            {
                if (textureImporterSettings.streamingMipmapsPriority != importSettings.streamingMipMapPriority.value &&
                    importSettings.streamingMipMapPriority.toggle)
                {
                    mismatch.AddMismatchedSetting("streamingMipMapPriority");
                    pmap.Add("StreamingMipMapPriority", () => textureImporterSettings.streamingMipmapsPriority = importSettings.streamingMipMapPriority.value);
                }
            }

            if (textureImporter.mipmapEnabled != importSettings.generateMipMap.value &&
                importSettings.generateMipMap.toggle)
            {
                mismatch.AddMismatchedSetting("generateMipMap");
                pmap.Add("GenerateMipMaps", () => textureImporter.mipmapEnabled = importSettings.generateMipMap.value);
            }
            
            else
            {
                if (textureImporterSettings.borderMipmap != importSettings.borderMipMaps.value &&
                    importSettings.borderMipMaps.toggle)
                {
                    mismatch.AddMismatchedSetting("borderMipMaps");
                    pmap.Add("BorderMipMap", () => textureImporterSettings.borderMipmap = importSettings.borderMipMaps.value);
                }

                if (textureImporterSettings.mipmapFilter != importSettings.mipmapFilter.value &&
                    importSettings.mipmapFilter.toggle)
                {
                    mismatch.AddMismatchedSetting("mipmapFilter");
                    pmap.Add("MipMapFilter", () => textureImporterSettings.mipmapFilter = importSettings.mipmapFilter.value);
                }

                if (textureImporterSettings.mipMapsPreserveCoverage != importSettings.mipMapsPreserveCoverage.value &&
                    importSettings.mipMapsPreserveCoverage.toggle)
                {
                    mismatch.AddMismatchedSetting("mipMapsPreserveCoverage");
                    pmap.Add("MipMapPreserveCoverage", () => textureImporterSettings.mipMapsPreserveCoverage = importSettings.mipMapsPreserveCoverage.value);
                }
        
                // Fadeout MipMaps
                if (textureImporterSettings.fadeOut != importSettings.fadeoutMipMaps.value &&
                    importSettings.fadeoutMipMaps.toggle)
                {
                    mismatch.AddMismatchedSetting("fadeoutMipMaps");
                    pmap.Add("FadeoutMipMaps", () => textureImporterSettings.fadeOut = importSettings.fadeoutMipMaps.value);
                }
                else
                {
                    if (textureImporterSettings.mipmapFadeDistanceStart == importSettings.start.value &&
                        importSettings.start.toggle)
                    {
                        mismatch.AddMismatchedSetting("mipmapFadeDistanceStart");
                        pmap.Add("MipMapFadeDistanceStart", () => textureImporterSettings.mipmapFadeDistanceStart = importSettings.start.value);
                    }

                    if (textureImporterSettings.mipmapFadeDistanceEnd != importSettings.end.value &&
                        importSettings.end.toggle)
                    {
                        mismatch.AddMismatchedSetting("mipmapFadeDistanceEnd");
                        pmap.Add("MipMapFadeDistanceEnd", () => textureImporterSettings.mipmapFadeDistanceEnd = importSettings.end.value);
                    }
                }
            }
        }
        
        private void CheckWrapMode(ImportSettings2DArt importSettings, TextureImporter textureImporter, TextureImporterSettings textureImporterSettings, ImportSettingsMismatch<ImportSettings2DArt> mismatch, Dictionary<string, Action> pmap)
        {
            if (textureImporter.wrapMode != importSettings.wrapMode.value && importSettings.wrapMode.toggle)
            {
                mismatch.AddMismatchedSetting("wrapMode");
                pmap.Add("WrapMode", () => textureImporter.wrapMode = importSettings.wrapMode.value);
            }
            else
            {
                if(importSettings.wrapMode.toggle && 
                    (importSettings.wrapMode.value is TextureWrapMode.Clamp ||
                        importSettings.wrapMode.value is TextureWrapMode.Repeat ||
                        importSettings.wrapMode.value is TextureWrapMode.Mirror ||
                        importSettings.wrapMode.value is TextureWrapMode.MirrorOnce))
                    return;
                
                // Check for WrapMode "Per-Axis"
                if (textureImporterSettings.wrapModeU != importSettings.wrapModeU.value &&
                    importSettings.wrapModeU.toggle)
                {
                    mismatch.AddMismatchedSetting("wrapModeU");
                    pmap.Add("WrapModeU", () => textureImporterSettings.wrapModeU = importSettings.wrapModeU.value);
                }

                if (textureImporterSettings.wrapModeV != importSettings.wrapModeV.value &&
                    importSettings.wrapModeV.toggle)
                {
                    mismatch.AddMismatchedSetting("wrapModeV");
                    pmap.Add("WrapModeV", () => textureImporterSettings.wrapModeV = importSettings.wrapModeV.value);
                }
            }
        }
        
    }
}
#endif