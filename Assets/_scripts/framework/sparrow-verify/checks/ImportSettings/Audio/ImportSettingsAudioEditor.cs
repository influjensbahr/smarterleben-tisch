//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Marc Freitag
//

#if UNITY_EDITOR
using UnityEditor;

namespace Sparrow.Verification
{
    [CustomEditor(typeof(ImportSettingsAudio))]
    public class ImportSettingsAudioEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ImportSettingsAudio importSettingsAudio = (ImportSettingsAudio) target;
            ImportSettingsAudio t = (ImportSettingsAudio)target;
            if(t == null) return;
            bool isChanged = false;
            
            isChanged |= t.forceToMono.OnCustomInspectorChange(serializedObject.FindProperty("forceToMono.value"), "Force To Mono");
            isChanged |= t.normalize.OnCustomInspectorChange(serializedObject.FindProperty("normalize.value"), "Normalize");
            isChanged |= t.loadInBackground.OnCustomInspectorChange(serializedObject.FindProperty("loadInBackground.value"), "Load In Background");
            isChanged |= t.ambisonic.OnCustomInspectorChange(serializedObject.FindProperty("ambisonic.value"), "Ambisonic");
            isChanged |= t.loadType.OnCustomInspectorChange(serializedObject.FindProperty("loadType.value"), "Load Type");
            isChanged |= t.preloadAudio.OnCustomInspectorChange(serializedObject.FindProperty("preloadAudio.value"), "Preload Audio");
            isChanged |= t.compressionFormat.OnCustomInspectorChange(serializedObject.FindProperty("compressionFormat.value"), "Compression Format");
            isChanged |= t.quality.OnCustomInspectorChange(serializedObject.FindProperty("quality.value"), "Quality");
            isChanged |= t.sampleRateSettings.OnCustomInspectorChange(serializedObject.FindProperty("sampleRateSettings.value"), "Sample Rate Settings");
            isChanged |= t.sampleRateOverride.OnCustomInspectorChange(serializedObject.FindProperty("sampleRateOverride.value"), "Sample Rate Override");
            
            serializedObject.ApplyModifiedProperties();
            
            if(isChanged) EditorUtility.SetDirty(serializedObject.targetObject);
        }
    }
}
#endif