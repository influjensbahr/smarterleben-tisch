//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Alex Brühl
//

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OTBT.Framework.Utils.Editor;
using static OTBT.Framework.Audio.MusicTheme;
using OTBT.Framework.Utils;

namespace OTBT.Framework.Audio
{
    [CustomEditor(typeof(MusicTheme))]
    public class MusicThemeEditor : Editor
    {
        string m_TempStateName = "State";
        bool m_ShowLayers = true;
        bool m_ShowStates = true;
        List<bool> m_ShowSingleLayer = new List<bool>();
        List<bool> m_ShowSingleState = new List<bool>();

        static readonly Color LayerColor = new(82 / 255f, 156 / 255f, 202 / 255f, .5f);
        static readonly Color StateColor = new(77 / 255f, 171 / 255f, 154 / 255f, .5f);

        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            //DATA
            MusicTheme themeScript = target as MusicTheme;
            SerializedProperty layerProperty = serializedObject.FindProperty("layers");
            //LOGO
            EditorUtils.DrawLogoHeader();
            EditorUtils.DrawWikiLinkButton("https://wiki.beatentrack.games/doc/audio-Ffch4FrHDN#h-musicthemes-used-to-play-multi-layer-music-like-in-our-old-music-system");


            var style = new GUIStyle(EditorStyles.helpBox);
            style.padding = new RectOffset(10, 10, 10, 10);
            style.normal.background = EditorUtils.TextureFromColor(LayerColor);
            using (new EditorGUILayout.VerticalScope(style))
            {
                //Layers
                m_ShowLayers = EditorGUILayout.Foldout(m_ShowLayers, EditorUtils.LabelWithGlyphicon(" Layers (" + layerProperty.arraySize + ")", Glyphicons.Layers), true);
                if (m_ShowLayers)
                {
                    ShowLayerEditor(themeScript, layerProperty);
                }
            }

            EditorUtils.Separator(1);

            style.normal.background = EditorUtils.TextureFromColor(StateColor);
            using (new EditorGUILayout.VerticalScope(style))
            {
                //States
                m_ShowStates = EditorGUILayout.Foldout(m_ShowStates, EditorUtils.LabelWithGlyphicon(" States (" + themeScript.states.Count + ")", Glyphicons.Speakers), true);
                if (m_ShowStates)
                {
                    ShowStateEditor(themeScript);
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(themeScript);

            EditorUtils.DrawVerify(themeScript);
            EditorUtils.EndColoredEditor();
        }

        void ShowStateEditor(MusicTheme themeScript)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            m_TempStateName = EditorGUILayout.TextField(m_TempStateName);
            if (GUILayout.Button("Add State"))
            {
                themeScript.states.Add(new MusicState(m_TempStateName, themeScript.layers.Count));
            }

            EditorGUILayout.EndHorizontal();

            int numOfStates = themeScript.states.Count;
            while (m_ShowSingleState.Count < numOfStates) m_ShowSingleState.Add(false);
            for (int i = 0; i < numOfStates; i++)
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    m_ShowSingleState[i] = EditorGUILayout.Foldout(m_ShowSingleState[i], themeScript.states[i].name, true);
                    if (m_ShowSingleState[i])
                    {
                        themeScript.states[i].name = EditorGUILayout.TextField("Name", themeScript.states[i].name);
                        themeScript.states[i].blendTime =
                            EditorGUILayout.FloatField("Fade Duration", themeScript.states[i].blendTime);

                        for (int k = 0; k <= themeScript.layers.Count / 4; k++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            for (int j = k * 4; j <= k * 4 + 3 && j < themeScript.layers.Count; j++)
                            {
                                EditorGUILayout.BeginVertical("GroupBox");
                                GUILayout.Label($"{themeScript.layers[j].name} Volume");
                                themeScript.states[i].layerVolumes[j] =
                                    EditorGUILayout.Slider(themeScript.states[i].layerVolumes[j], 0f, 1f);

                                EditorGUILayout.EndVertical();
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button($"Remove", GUILayout.Width(100)))
                        {
                            themeScript.states.Remove(themeScript.states[i]);
                        }


                        EditorGUILayout.EndHorizontal();
                    }
                }
                GUILayout.Space(5);
            }
        }

        private void ShowLayerEditor(MusicTheme themeScript, SerializedProperty property)
        {
            //ADD LAYER
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Layer"))
                {
                    themeScript.layers.Add(new MusicLayer());
                    foreach (MusicState state in themeScript.states)
                    {
                        state.layerVolumes.Add(0);
                    }
                }
            }

            serializedObject.Update();
            while (m_ShowSingleLayer.Count < themeScript.layers.Count) m_ShowSingleLayer.Add(false);
            for (int i = 0; i < property.arraySize; i++)
            {
                MusicLayer layer = themeScript.layers[i];
                using (new GUILayout.VerticalScope("Box"))
                {
                    m_ShowSingleLayer[i] = EditorGUILayout.Foldout(m_ShowSingleLayer[i], layer.name, true);
                    if (m_ShowSingleLayer[i])
                    {
                        layer.name = EditorGUILayout.TextField("Name", layer.name);
                        layer.cue =
                            (SoundCue)EditorGUILayout.ObjectField("Sound Cue", layer.cue, typeof(SoundCue),
                                false);
                        //EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Remove", GUILayout.Width(100)))
                            {
                                themeScript.layers.Remove(layer);
                                foreach (MusicState state in themeScript.states)
                                {
                                    state.layerVolumes.RemoveAt(i);
                                }

                                break;
                            }
                        }
                    }
                }
                GUILayout.Space(5);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
