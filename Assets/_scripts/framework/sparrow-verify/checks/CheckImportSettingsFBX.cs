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
    public class CheckImportSettingsFBX : VerifyCheckBase
    {
        public override string description => "Import Settings: FBX";
        public override string longDescription =>
            "Checks if FBX import settings have been set according to your project-wide preferences. You can set this check up with your preferences on a per-folder basis.";

        [SerializeField] List<SingleFolder<ImportSettingsFBX>> m_Folders = new List<SingleFolder<ImportSettingsFBX>>();
        [SerializeField] List<ImportSettingsMismatch<ImportSettingsFBX>> m_mismatchedSettings = new List<ImportSettingsMismatch<ImportSettingsFBX>>();
        private const float TOLERANCE = 0.01f;

        public override bool DrawSpecificProfileEditor()
        {
            var hasChanged = false;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Folder", GUILayout.Width(100)))
            {
                m_Folders.Add(new SingleFolder<ImportSettingsFBX>("", null));
            }

            EditorGUILayout.LabelField("Path", GUILayout.MinWidth(50), GUILayout.MaxWidth(200));
            EditorGUILayout.LabelField("Import Settings", GUILayout.MinWidth(50), GUILayout.MaxWidth(200));
            EditorGUILayout.LabelField("", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            if (m_Folders.Count == 0) m_Folders.Add(new SingleFolder<ImportSettingsFBX>("", null));

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

                m_Folders[i].folderPath = EditorGUILayout.TextField(m_Folders[i].folderPath, GUILayout.MinWidth(50),
                    GUILayout.MaxWidth(200));
                m_Folders[i].importSettings = EditorGUILayout.ObjectField(m_Folders[i].importSettings,
                    typeof(ImportSettingsFBX), false, GUILayout.MinWidth(50),
                    GUILayout.MaxWidth(200)) as ImportSettingsFBX;

                if (folderPath != null && folderPath != m_Folders[i].folderPath ||
                    importSettings != m_Folders[i].importSettings) hasChanged = true;

                if (m_Folders.Count > 1 && GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    m_Folders.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }
            
            return hasChanged;
        }

        public override void PerformCheck(ModelImporter model)
        {
            foreach (var folder in m_Folders)
            {
                var propertyMap = new Dictionary<string, Action>();
                var folderPath = folder.folderPath;
                var assetPath = AssetDatabase.GetAssetPath(model);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                
                m_mismatchedSettings.Clear();
                assetPath = assetPath.Substring(0, assetPath.LastIndexOf("/", StringComparison.Ordinal));
                if(!assetPath.Equals(folderPath)) continue;
                
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) continue;
                CheckMeshImportSettings(folder.importSettings, model, propertyMap);

                if (m_mismatchedSettings.Count <= 0) continue;

                foreach (var mismatch in m_mismatchedSettings)
                {
                    var combinedList = string.Join(", ", propertyMap.Keys.ToArray());
                    AddFailedCheck($"Wrong FBX Preset Settings: {combinedList}", asset,
                        () => { ApplyCorrectImportSettings(mismatch.ModelImporter, propertyMap); });
                }
            }
        }

        private void ApplyCorrectImportSettings(ModelImporter modelImporter, Dictionary<string, Action> pMap)
        {
            foreach (var kvp in pMap)
            {
                kvp.Value.Invoke();
            }
        
            // Reimport the asset to apply the changes
            AssetDatabase.ImportAsset(modelImporter.assetPath, ImportAssetOptions.ForceUpdate);
        
        }

        public ImportSettingsMismatch<ImportSettingsFBX> CheckMeshImportSettings(ImportSettingsFBX presetToUse, ModelImporter modelImporter, Dictionary<string, Action> pmap)
        {
            var assetPath = modelImporter.assetPath;
            var mismatch = new ImportSettingsMismatch<ImportSettingsFBX>(presetToUse, modelImporter);
            
            if (modelImporter == null)
            {
                Debug.LogError("AssetImporter could not be loaded: " + modelImporter.assetPath);
                return mismatch;
            }

            if (presetToUse == null)
            {
                var cutPath = assetPath.Substring(0, assetPath.LastIndexOf("/", StringComparison.Ordinal));
                Debug.LogWarning($"Import Settings for FBX at {cutPath} is not set.");
                return mismatch;
            }

            // Scene
            if (!Mathf.Approximately(presetToUse.m_ScaleFactor.value, modelImporter.globalScale) && presetToUse.m_ScaleFactor.toggle)
            {
                mismatch.AddMismatchedSetting("ScaleFactor");
                pmap.Add("ScaleFactor", () => modelImporter.globalScale = presetToUse.m_ScaleFactor.value);
            }

            if (presetToUse.m_ConvertUnits.value != modelImporter.useFileScale && presetToUse.m_ConvertUnits.toggle)
            {
                mismatch.AddMismatchedSetting("ConvertUnits");
                pmap.Add("ConvertUnits", () => modelImporter.useFileScale = presetToUse.m_ConvertUnits.value);
            }

            if (presetToUse.m_BakeAxisConversion.value != modelImporter.bakeIK &&
                presetToUse.m_BakeAxisConversion.toggle)
            {
                mismatch.AddMismatchedSetting("BakeAxisConversion");
                pmap.Add("BakeAxisConversion", () => modelImporter.bakeIK = presetToUse.m_BakeAxisConversion.value);
            }

            if (presetToUse.m_ImportBlendShapes.value != modelImporter.importBlendShapes &&
                presetToUse.m_ImportBlendShapes.toggle)
            {
                mismatch.AddMismatchedSetting("ImportBlendShapes");
                pmap.Add("ImportBlendShapes", () => modelImporter.importBlendShapes = presetToUse.m_ImportBlendShapes.value);
            }

            if (presetToUse.m_ImportVisibility.value != modelImporter.importVisibility &&
                presetToUse.m_ImportVisibility.toggle)
            {
                mismatch.AddMismatchedSetting("ImportVisibility");
                pmap.Add("ImportVisibility", () => modelImporter.importVisibility = presetToUse.m_ImportVisibility.value);
            }

            if (presetToUse.m_ImportCameras.value != modelImporter.importCameras && presetToUse.m_ImportCameras.toggle)
            {
                mismatch.AddMismatchedSetting("ImportCameras");
                pmap.Add("ImportCameras", () => modelImporter.importCameras = presetToUse.m_ImportCameras.value);
            }

            if (presetToUse.m_ImportLights.value != modelImporter.importLights && presetToUse.m_ImportLights.toggle)
            {
                mismatch.AddMismatchedSetting("ImportLights");
                pmap.Add("ImportLights", () => modelImporter.importLights = presetToUse.m_ImportLights.value);
            }

            if (presetToUse.m_PreserveHierarchy.value != modelImporter.preserveHierarchy &&
                presetToUse.m_PreserveHierarchy.toggle)
            {
                mismatch.AddMismatchedSetting("PreserveHierarchy");
                pmap.Add("PreserveHierarchy", () => modelImporter.preserveHierarchy = presetToUse.m_PreserveHierarchy.value);
            }

            if (presetToUse.m_SortHierarchyByName.value != modelImporter.sortHierarchyByName &&
                presetToUse.m_SortHierarchyByName.toggle)
            {
                mismatch.AddMismatchedSetting("SortHierarchyByName");
                pmap.Add("SortHierarchyByName", () => modelImporter.sortHierarchyByName = presetToUse.m_SortHierarchyByName.value);
            }

            // Meshes
            if (presetToUse.m_MeshCompression.value != modelImporter.meshCompression &&
                presetToUse.m_MeshCompression.toggle)
            {
                mismatch.AddMismatchedSetting("MeshCompression");
                pmap.Add("MeshCompression", () => modelImporter.meshCompression = presetToUse.m_MeshCompression.value);
            }

            if (presetToUse.m_ReadWriteEnabled.value != modelImporter.isReadable &&
                presetToUse.m_ReadWriteEnabled.toggle)
            {
                mismatch.AddMismatchedSetting("ReadWriteEnabled");
                pmap.Add("ReadWriteEnabled", () => modelImporter.isReadable = presetToUse.m_ReadWriteEnabled.value);
            }

            if (presetToUse.m_OptimizeMeshPolygons.value != modelImporter.meshOptimizationFlags &&
                presetToUse.m_OptimizeMeshPolygons.toggle)
            {
                mismatch.AddMismatchedSetting("OptimizeMeshPolygons");
                pmap.Add("OptimizeMeshPolygons", () => modelImporter.meshOptimizationFlags = presetToUse.m_OptimizeMeshPolygons.value);
            }

            if (presetToUse.m_GenerateColliders.value != modelImporter.addCollider)
            {
                mismatch.AddMismatchedSetting("GenerateColliders");
                pmap.Add("GenerateColliders", () => modelImporter.addCollider = presetToUse.m_GenerateColliders.value);
            }

            // Geometry
            if (presetToUse.m_KeepQuads.value != modelImporter.keepQuads && presetToUse.m_KeepQuads.toggle)
            {
                mismatch.AddMismatchedSetting("KeepQuads");
                pmap.Add("KeepQuads", () => modelImporter.keepQuads = presetToUse.m_KeepQuads.value);
            }

            if (presetToUse.m_WeldVertices.value != modelImporter.weldVertices && presetToUse.m_WeldVertices.toggle)
            {
                mismatch.AddMismatchedSetting("WeldVertices");
                pmap.Add("WeldVertices", () => modelImporter.weldVertices = presetToUse.m_WeldVertices.value);
            }

            if (presetToUse.m_IndexFormat.value != modelImporter.indexFormat && presetToUse.m_IndexFormat.toggle)
            {
                mismatch.AddMismatchedSetting("IndexFormat");
                pmap.Add("IndexFormat", () => modelImporter.indexFormat = presetToUse.m_IndexFormat.value);
            }

            if (presetToUse.m_Normals.value != modelImporter.importNormals && presetToUse.m_Normals.toggle)
            {
                mismatch.AddMismatchedSetting("Normals");
                pmap.Add("Normals", () => modelImporter.importNormals = presetToUse.m_Normals.value);
            }

            if (presetToUse.m_BlendShapeNormals.value != modelImporter.importBlendShapeNormals &&
                presetToUse.m_BlendShapeNormals.toggle)
            {
                mismatch.AddMismatchedSetting("BlendShapeNormals");
                pmap.Add("BlendShapeNormals", () => modelImporter.importBlendShapeNormals = presetToUse.m_BlendShapeNormals.value);
            }

            if (presetToUse.m_NormalCalculationMode.value != modelImporter.normalCalculationMode &&
                presetToUse.m_NormalCalculationMode.toggle)
            {
                mismatch.AddMismatchedSetting("NormalCalculationMode");
                pmap.Add("NormalCalculationMode", () => modelImporter.normalCalculationMode = presetToUse.m_NormalCalculationMode.value);
            }

            if (presetToUse.m_SmoothnessSource.value != modelImporter.normalSmoothingSource &&
                presetToUse.m_SmoothnessSource.toggle)
            {
                mismatch.AddMismatchedSetting("SmoothnessSource");
                pmap.Add("SmoothnessSource", () => modelImporter.normalSmoothingSource = presetToUse.m_SmoothnessSource.value);
            }

            if (Math.Abs(presetToUse.m_NormalSmoothingAngle.value - modelImporter.normalSmoothingAngle) > TOLERANCE &&
                presetToUse.m_NormalSmoothingAngle.toggle)
            {
                mismatch.AddMismatchedSetting("NormalSmoothingAngle");
                pmap.Add("NormalSmoothingAngle", () => modelImporter.normalSmoothingAngle = presetToUse.m_NormalSmoothingAngle.value);
            }
            if (presetToUse.m_Tangents.value != modelImporter.importTangents && presetToUse.m_Tangents.toggle)
            {
                mismatch.AddMismatchedSetting("Tangents");
                pmap.Add("Tangents", () => modelImporter.importTangents = presetToUse.m_Tangents.value);
            }

            if (presetToUse.m_SwapUVs.value != modelImporter.swapUVChannels && presetToUse.m_SwapUVs.toggle)
            {
                mismatch.AddMismatchedSetting("SwapUVs");
                pmap.Add("SwapUVs", () => modelImporter.swapUVChannels = presetToUse.m_SwapUVs.value);
            }

            if (presetToUse.m_GenerateLightmapUVs.value != modelImporter.generateSecondaryUV &&
                presetToUse.m_GenerateLightmapUVs.toggle)
            {
                mismatch.AddMismatchedSetting("GenerateLightmapUVs");
                pmap.Add("GenerateLightmapUVs", () => modelImporter.generateSecondaryUV = presetToUse.m_GenerateLightmapUVs.value);
            }

            // Rig
            if (presetToUse.m_AnimationType.value != modelImporter.animationType && presetToUse.m_AnimationType.toggle)
            {
                mismatch.AddMismatchedSetting("AnimationType");
                pmap.Add("AnimationType", () => modelImporter.animationType = presetToUse.m_AnimationType.value);
            }

            if (presetToUse.m_AvatarDefinition.value != modelImporter.avatarSetup &&
                presetToUse.m_AvatarDefinition.toggle)
            {
                mismatch.AddMismatchedSetting("AvatarDefinition");
                pmap.Add("AvatarDefinition", () => modelImporter.avatarSetup = presetToUse.m_AvatarDefinition.value);
            }

            if (presetToUse.m_OptimizeMeshVertices.value != modelImporter.optimizeMeshVertices &&
                presetToUse.m_OptimizeMeshVertices.toggle)
            {
                mismatch.AddMismatchedSetting("OptimizeMeshVertices");
                pmap.Add("OptimizeMeshVertices", () => modelImporter.optimizeMeshVertices = presetToUse.m_OptimizeMeshVertices.value);
            }

            if (presetToUse.m_SkinWeights.value != modelImporter.skinWeights && presetToUse.m_SkinWeights.toggle)
            {
                mismatch.AddMismatchedSetting("SkinWeights");
                pmap.Add("SkinWeights", () => modelImporter.skinWeights = presetToUse.m_SkinWeights.value);
            }

            if (presetToUse.m_StripBones.value != modelImporter.importConstraints && presetToUse.m_StripBones.toggle)
            {
                mismatch.AddMismatchedSetting("StripBones");
                pmap.Add("StripBones", () => modelImporter.importConstraints = presetToUse.m_StripBones.value);
            }

            // Animation
            if (presetToUse.m_ImportConstraints.value != modelImporter.importConstraints &&
                presetToUse.m_ImportConstraints.toggle)
            {
                mismatch.AddMismatchedSetting("ImportConstraints");
                pmap.Add("ImportConstraints", () => modelImporter.importConstraints = presetToUse.m_ImportConstraints.value);
            }

            if (presetToUse.m_ImportAnimation.value != modelImporter.importAnimation &&
                presetToUse.m_ImportAnimation.toggle)
            {
                mismatch.AddMismatchedSetting("ImportAnimation");
                pmap.Add("ImportAnimation", () => modelImporter.importAnimation = presetToUse.m_ImportAnimation.value);
            }

            // Materials
            if (presetToUse.m_MaterialCreationMode.value != modelImporter.materialImportMode &&
                presetToUse.m_MaterialCreationMode.toggle)
            {
                mismatch.AddMismatchedSetting("MaterialCreationMode");
                pmap.Add("MaterialCreationMode", () => modelImporter.materialImportMode = presetToUse.m_MaterialCreationMode.value);
            }

            if (presetToUse.m_Location.value != modelImporter.materialLocation && presetToUse.m_Location.toggle)
            {
                mismatch.AddMismatchedSetting("Location");
                pmap.Add("Location", () => modelImporter.materialLocation = presetToUse.m_Location.value);
            }

            if (presetToUse.m_MaterialName.value != modelImporter.materialName && presetToUse.m_MaterialName.toggle)
            {
                mismatch.AddMismatchedSetting("MaterialName");
                pmap.Add("MaterialName", () => modelImporter.materialName = presetToUse.m_MaterialName.value);
            }

            if (presetToUse.m_MaterialSearch.value != modelImporter.materialSearch &&
                presetToUse.m_MaterialSearch.toggle)
            {
                mismatch.AddMismatchedSetting("MaterialSearch");
                pmap.Add("MaterialSearch", () => modelImporter.materialSearch = presetToUse.m_MaterialSearch.value);
            }

            if (mismatch.MismatchedSettings.Count > 0)
            {
                m_mismatchedSettings.Add(mismatch);
            }
            
            return mismatch;
        }
    }
}
#endif