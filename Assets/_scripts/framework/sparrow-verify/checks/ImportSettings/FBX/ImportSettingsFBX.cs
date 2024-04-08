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
using UnityEngine.Rendering;

namespace Sparrow.Verification
{
    [CreateAssetMenu(fileName = "importSettingsFBX", menuName = "Sparrow/Verification/Import-Settings/Import Settings FBX", order = 0)]
    [Serializable]
    public class ImportSettingsFBX : ImportSettingsBase
    {
        public string tab = "Model";
        // Model
        [Header("Scene")]
        [SerializeField] public CustomPropertyField<float> m_ScaleFactor = new CustomPropertyField<float>(1f);
        [SerializeField] public CustomPropertyField<bool> m_ConvertUnits = new CustomPropertyField<bool>(true);
        [SerializeField] public CustomPropertyField<bool> m_BakeAxisConversion = new CustomPropertyField<bool>(false);
        [SerializeField] public CustomPropertyField<bool> m_ImportBlendShapes = new CustomPropertyField<bool>(true);
        [SerializeField] public CustomPropertyField<bool> m_ImportVisibility = new CustomPropertyField<bool>(true);
        [SerializeField] public CustomPropertyField<bool> m_ImportCameras = new CustomPropertyField<bool>(true);
        [SerializeField] public CustomPropertyField<bool> m_ImportLights = new CustomPropertyField<bool>(true);
        [SerializeField] public CustomPropertyField<bool> m_PreserveHierarchy = new CustomPropertyField<bool>(false);
        [SerializeField] public CustomPropertyField<bool> m_SortHierarchyByName = new CustomPropertyField<bool>(true);
        [Header("Meshes")]
        [SerializeField] public CustomPropertyField<ModelImporterMeshCompression> m_MeshCompression = new CustomPropertyField<ModelImporterMeshCompression> (ModelImporterMeshCompression.Off);
        [SerializeField] public CustomPropertyField<bool> m_ReadWriteEnabled = new CustomPropertyField<bool> (false);
        [SerializeField] public CustomPropertyField<MeshOptimizationFlags> m_OptimizeMeshPolygons = new CustomPropertyField<MeshOptimizationFlags> (MeshOptimizationFlags.Everything); 
        [SerializeField] public CustomPropertyField<bool> m_GenerateColliders = new CustomPropertyField<bool>(false);
        [Header("Geometry")]
        [SerializeField] public CustomPropertyField<bool> m_KeepQuads = new CustomPropertyField<bool>(false);
        [SerializeField] public CustomPropertyField<bool> m_WeldVertices = new CustomPropertyField<bool>(true);
        [SerializeField] public CustomPropertyField<ModelImporterIndexFormat> m_IndexFormat = new CustomPropertyField<ModelImporterIndexFormat>(ModelImporterIndexFormat.Auto);
        [SerializeField] public CustomPropertyField<bool> m_LegacyBlendShapeNormals = new CustomPropertyField<bool>(false);
        [SerializeField] public CustomPropertyField<ModelImporterNormals> m_Normals = new CustomPropertyField<ModelImporterNormals>(ModelImporterNormals.Import);
        [SerializeField] public CustomPropertyField<ModelImporterNormals> m_BlendShapeNormals = new CustomPropertyField<ModelImporterNormals>(ModelImporterNormals.Import);
        [SerializeField] public CustomPropertyField<ModelImporterNormalCalculationMode> m_NormalCalculationMode = new CustomPropertyField<ModelImporterNormalCalculationMode>(ModelImporterNormalCalculationMode.AreaAndAngleWeighted);
        [SerializeField] public CustomPropertyField<ModelImporterNormalSmoothingSource> m_SmoothnessSource = new CustomPropertyField<ModelImporterNormalSmoothingSource>(ModelImporterNormalSmoothingSource.FromSmoothingGroups);
        [Range(0, 180)] [SerializeField] public CustomPropertyField<float> m_NormalSmoothingAngle = new CustomPropertyField<float>(60f);
        [SerializeField] public CustomPropertyField<ModelImporterTangents> m_Tangents = new CustomPropertyField<ModelImporterTangents>(ModelImporterTangents.CalculateMikk);
        [SerializeField] public CustomPropertyField<bool> m_SwapUVs = new CustomPropertyField<bool>(false);
        [SerializeField] public CustomPropertyField<bool> m_GenerateLightmapUVs = new CustomPropertyField<bool>(false);
        // Rig
        [SerializeField] public CustomPropertyField<ModelImporterAnimationType> m_AnimationType = new CustomPropertyField<ModelImporterAnimationType>(ModelImporterAnimationType.None);
        [SerializeField] public CustomPropertyField<ModelImporterAvatarSetup> m_AvatarDefinition = new CustomPropertyField<ModelImporterAvatarSetup>(ModelImporterAvatarSetup.CreateFromThisModel);
        [SerializeField] public CustomPropertyField<bool> m_OptimizeMeshVertices = new CustomPropertyField<bool>(true);
        [SerializeField] public CustomPropertyField<ModelImporterSkinWeights> m_SkinWeights = new CustomPropertyField<ModelImporterSkinWeights>(ModelImporterSkinWeights.Standard);
        [SerializeField] public CustomPropertyField<bool> m_StripBones = new CustomPropertyField<bool>(true);
        // Animation
        [SerializeField] public CustomPropertyField<bool> m_ImportConstraints = new CustomPropertyField<bool>(true);
        [SerializeField] public CustomPropertyField<bool> m_ImportAnimation = new CustomPropertyField<bool>(true);
        // Materials
        [SerializeField] public CustomPropertyField<ModelImporterMaterialImportMode> m_MaterialCreationMode = new CustomPropertyField<ModelImporterMaterialImportMode>(ModelImporterMaterialImportMode.ImportStandard);
        [SerializeField] public CustomPropertyField<ModelImporterMaterialLocation> m_Location = new CustomPropertyField<ModelImporterMaterialLocation>(ModelImporterMaterialLocation.External);
        [SerializeField] public CustomPropertyField<ModelImporterMaterialName> m_MaterialName = new CustomPropertyField<ModelImporterMaterialName>(ModelImporterMaterialName.BasedOnTextureName);
        [SerializeField] public CustomPropertyField<ModelImporterMaterialSearch> m_MaterialSearch = new CustomPropertyField<ModelImporterMaterialSearch>(ModelImporterMaterialSearch.RecursiveUp);
    }
}
#endif