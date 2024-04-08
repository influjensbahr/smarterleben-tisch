//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using OTBT.Framework.Utils;
using OTBT.Framework.Utils.Editor;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

namespace OTBT.Framework.Audio
{
    public static class AudioEditorMenus
    {
        [MenuItem("OTBT/Audio/SoundCue (in Scene)", false, 0)]
        [MenuItem("GameObject/OTBT/Audio/SoundCue", false, 0)]
        static void CreateSoundCue()
        {
            GameObject newObj = new GameObject();
            newObj.name = "New SoundCue";
            SoundCue cue = newObj.AddComponent<SoundCue>();
            foreach (System.Object obj in Selection.objects)
            {
                if (obj is SoundImporter)
                {
                    cue.AddSound(obj as SoundImporter);
                }
            }

            if (Selection.activeGameObject != null)
            {
                if(Selection.activeGameObject.scene != null)
                    newObj.transform.parent = Selection.activeGameObject.transform;
            }
            Selection.activeGameObject = newObj;
        }

        [MenuItem("Assets/Create/OTBT/Audio/SoundCue", false, 0)]
        static void CreateSoundCuePrefab()
        {
            string objName = "New SoundCue";
            GameObject newObj = new GameObject();
            SoundCue cue = newObj.AddComponent<SoundCue>();
            foreach (System.Object obj in Selection.objects)
            {
                if (obj is SoundImporter)
                {
                    cue.AddSound(obj as SoundImporter);
                    objName = (obj as SoundImporter).name;
                }
            }

            string targetPath = $"{GetCurrentFolderPath()}/{objName}.prefab";
            Selection.activeGameObject = PrefabUtility.SaveAsPrefabAsset(newObj, targetPath);
            GameObject.DestroyImmediate(newObj);
        }

        static string GetCurrentFolderPath()
        {
            Type projectWindowUtilType = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = getActiveFolderPath.Invoke(null, new object[0]);
            string pathToCurrentFolder = obj.ToString();
            return (pathToCurrentFolder);
        }

        [MenuItem("OTBT/Audio/SoundImporter", false, 0)]
        static void CreateSoundImporter()
        {
            SoundImporter lastCreation = null;
            foreach (System.Object obj in Selection.objects)
            {
                if (obj is AudioClip)
                {
                    lastCreation = CreateOneSoundImporter(obj as AudioClip);
                }
            }
            if(lastCreation == null)
            {
                lastCreation = CreateOneSoundImporter(null);
            }
        }

        public static SoundImporter CreateOneSoundImporter(AudioClip clip)
        {
            SoundImporter imp = ScriptableObject.CreateInstance<SoundImporter>();
            imp.SetClip(clip);
          
            string path = Path.ChangeExtension(AssetDatabase.GetAssetPath(clip), "asset");
            
            EditorUtility.SetDirty(imp);
            AssetDatabase.CreateAsset(imp, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return imp;
        }

        [CustomEditor(typeof(AudioClip), true)]
        public class AudioClipEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                GUI.enabled = true;
                if (GUILayout.Button(EditorUtils.LabelWithGlyphicon("Create SoundImporter", Glyphicons.Clean)))
                {
                    AudioClip clip = (AudioClip)target;
                    CreateOneSoundImporter(clip);
                }
            }
        }
    }
}
#endif