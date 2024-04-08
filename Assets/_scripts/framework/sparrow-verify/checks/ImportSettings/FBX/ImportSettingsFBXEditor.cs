//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Marc Freitag
//

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [CustomEditor(typeof(ImportSettingsFBX))]
    public class ImportSettingsFBXEditor : Editor
    {
        private enum Tab
        {
            Model,
            Rig,
            Animation,
            Materials
        }
        
        private Tab selectedTab = Tab.Model;
        bool isChanged = false;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var importSettingsMesh = (ImportSettingsFBX) target;
            var t = (ImportSettingsFBX)target;
            if (t == null) return;
            
            GUILayout.BeginHorizontal();

            if (GUILayout.Toggle(selectedTab == Tab.Model, "Model", "Button"))
            {
                selectedTab = Tab.Model;
            }

            if (GUILayout.Toggle(selectedTab == Tab.Rig, "Rig", "Button"))
            {
                selectedTab = Tab.Rig;
            }

            if (GUILayout.Toggle(selectedTab == Tab.Animation, "Animation", "Button"))
            {
                selectedTab = Tab.Animation;
            }

            if (GUILayout.Toggle(selectedTab == Tab.Materials, "Materials", "Button"))
            {
                selectedTab = Tab.Materials;
            }

            GUILayout.EndHorizontal();

            switch (selectedTab)
            {
                case Tab.Model:
                    DrawModelTab(t);
                    break;
                case Tab.Rig:
                    DrawRigTab(t);
                    break;
                case Tab.Animation:
                    DrawAnimationTab(t);
                    break;
                case Tab.Materials:
                    DrawMaterialsTab(t);
                    break;
            }
            
            serializedObject.ApplyModifiedProperties();
            
            if(isChanged) EditorUtility.SetDirty(serializedObject.targetObject);
        }
        
        private void DrawModelTab(ImportSettingsFBX t)
        {
            t.tab = "Model";
            isChanged |= t.m_ScaleFactor.OnCustomInspectorChange(serializedObject.FindProperty("m_ScaleFactor.value"), "Scale Factor");
            isChanged |= t.m_ConvertUnits.OnCustomInspectorChange(serializedObject.FindProperty("m_ConvertUnits.value"), "Convert Units");
            isChanged |= t.m_BakeAxisConversion.OnCustomInspectorChange(serializedObject.FindProperty("m_BakeAxisConversion.value"), "Bake Axis Conversion");
            isChanged |= t.m_ImportBlendShapes.OnCustomInspectorChange(serializedObject.FindProperty("m_ImportBlendShapes.value"), "Import Blend Shapes");
            isChanged |= t.m_ImportVisibility.OnCustomInspectorChange(serializedObject.FindProperty("m_ImportVisibility.value"), "Import Visibility");
            isChanged |= t.m_ImportCameras.OnCustomInspectorChange(serializedObject.FindProperty("m_ImportCameras.value"), "Import Cameras");
            isChanged |= t.m_ImportLights.OnCustomInspectorChange(serializedObject.FindProperty("m_ImportLights.value"), "Import Lights");
            isChanged |= t.m_PreserveHierarchy.OnCustomInspectorChange(serializedObject.FindProperty("m_PreserveHierarchy.value"), "Preserve Hierarchy");
            isChanged |= t.m_SortHierarchyByName.OnCustomInspectorChange(serializedObject.FindProperty("m_SortHierarchyByName.value"), "Sort Hierarchy By Name");
            isChanged |= t.m_MeshCompression.OnCustomInspectorChange(serializedObject.FindProperty("m_MeshCompression.value"), "Mesh Compression");
            isChanged |= t.m_ReadWriteEnabled.OnCustomInspectorChange(serializedObject.FindProperty("m_ReadWriteEnabled.value"), "Read/Write Enabled");
            isChanged |= t.m_OptimizeMeshPolygons.OnCustomInspectorChange(serializedObject.FindProperty("m_OptimizeMeshPolygons.value"), "Optimize Mesh Polygons");
            isChanged |= t.m_GenerateColliders.OnCustomInspectorChange(serializedObject.FindProperty("m_GenerateColliders.value"), "Generate Colliders");
            isChanged |= t.m_KeepQuads.OnCustomInspectorChange(serializedObject.FindProperty("m_KeepQuads.value"), "Keep Quads");
            isChanged |= t.m_WeldVertices.OnCustomInspectorChange(serializedObject.FindProperty("m_WeldVertices.value"), "Weld Vertices");
            isChanged |= t.m_IndexFormat.OnCustomInspectorChange(serializedObject.FindProperty("m_IndexFormat.value"), "Index Format");
            isChanged |= t.m_LegacyBlendShapeNormals.OnCustomInspectorChange(serializedObject.FindProperty("m_LegacyBlendShapeNormals.value"), "Legacy Blend Shape Normals");
            isChanged |= t.m_Normals.OnCustomInspectorChange(serializedObject.FindProperty("m_Normals.value"), "Normals");
            isChanged |= t.m_BlendShapeNormals.OnCustomInspectorChange(serializedObject.FindProperty("m_BlendShapeNormals.value"), "Blend Shape Normals");
            isChanged |= t.m_NormalCalculationMode.OnCustomInspectorChange(serializedObject.FindProperty("m_NormalCalculationMode.value"), "Normal Calculation Mode");
            isChanged |= t.m_SmoothnessSource.OnCustomInspectorChange(serializedObject.FindProperty("m_SmoothnessSource.value"), "Smoothness Source");
            isChanged |= t.m_NormalSmoothingAngle.OnCustomInspectorChange(serializedObject.FindProperty("m_NormalSmoothingAngle.value"), "Normal Smoothing Angle");
            isChanged |= t.m_Tangents.OnCustomInspectorChange(serializedObject.FindProperty("m_Tangents.value"), "Tangents");
            isChanged |= t.m_SwapUVs.OnCustomInspectorChange(serializedObject.FindProperty("m_SwapUVs.value"), "Swap UVs");
            isChanged |= t.m_GenerateLightmapUVs.OnCustomInspectorChange(serializedObject.FindProperty("m_GenerateLightmapUVs.value"), "Generate Lightmap UVs");
        }

        private void DrawRigTab(ImportSettingsFBX t)
        {
            t.tab = "Rig";
            isChanged |= t.m_AnimationType.OnCustomInspectorChange(serializedObject.FindProperty("m_AnimationType.value"), "Animation Type");
            isChanged |= t.m_AvatarDefinition.OnCustomInspectorChange(serializedObject.FindProperty("m_AvatarDefinition.value"), "Avatar Definition");
           isChanged |= t.m_SkinWeights.OnCustomInspectorChange(serializedObject.FindProperty("m_SkinWeights.value"), "Skin Weights");
           isChanged |= t.m_StripBones.OnCustomInspectorChange(serializedObject.FindProperty("m_StripBones.value"), "Strip Bones");
        }

        private void DrawAnimationTab(ImportSettingsFBX t)
        {
            t.tab = "Animation";
            isChanged |= t.m_ImportConstraints.OnCustomInspectorChange(serializedObject.FindProperty("m_ImportConstraints.value"), "Import Constraints");
            isChanged |= t.m_ImportAnimation.OnCustomInspectorChange(serializedObject.FindProperty("m_ImportAnimation.value"), "Import Animation");
        }

        private void DrawMaterialsTab(ImportSettingsFBX t)
        {
            t.tab = "Materials";
            isChanged |= t.m_MaterialCreationMode.OnCustomInspectorChange(serializedObject.FindProperty("m_MaterialCreationMode.value"), "Material Creation Mode");
            isChanged |= t.m_Location.OnCustomInspectorChange(serializedObject.FindProperty("m_Location.value"), "Location");
            isChanged |= t.m_MaterialName.OnCustomInspectorChange(serializedObject.FindProperty("m_MaterialName.value"), "Material Name");
            isChanged |= t.m_MaterialSearch.OnCustomInspectorChange(serializedObject.FindProperty("m_MaterialSearch.value"), "Material Search");
        }
    }
}
#endif