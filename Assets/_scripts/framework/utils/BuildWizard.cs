// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 


#if UNITY_EDITOR
using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using OTBT.Framework.Utils.Editor;

namespace OTBT.Framework.Utils
{
    public class BuildWizard : EditorWindow
    {
        string m_VersionNumber;
        string m_BundleIdentifier;
        BuildOptions m_BuildOptions;
        string m_Path = "Builds/";

        [MenuItem("OTBT/Build Wizard &#b", false, 10000)]
        static void CreateWizard()
        {
            var wizard = GetWindow<BuildWizard>();
            wizard.titleContent = new GUIContent("Build Wizard");
        }

        [MenuItem("OTBT/Build Game _#b", true, 10000)]
        static bool ValidateWindowOpen() => !Application.isPlaying;
        void OnGUI()
        {
            m_VersionNumber = PlayerSettings.bundleVersion;
            m_BundleIdentifier = PlayerSettings.applicationIdentifier;

            EditorUtils.BeginColoredEditor();
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                EditorGUILayout.LabelField($"Current Target: {buildTarget}", EditorStyles.centeredGreyMiniLabel);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);
                    DrawVersionNumber();
                    DrawBundleIdentifier();
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Additional Options", EditorStyles.boldLabel);
                    DrawPath();
                    m_BuildOptions = (BuildOptions)EditorGUILayout.EnumFlagsField("Build Flags", m_BuildOptions);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Build Settings")) OpenBuildSettings();
                if (GUILayout.Button("Open Player Settings")) OpenPlayerSettings();
            }

            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(40)))
            {
                if (GUILayout.Button("Build", GUILayout.ExpandHeight(true))) Build();
            }

            EditorUtils.EndColoredEditor();
        }

        void DrawPath()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                m_Path = EditorGUILayout.TextField("Output Path", m_Path);
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    m_Path = EditorUtility.OpenFolderPanel("Select Build Output Path", m_Path, string.Empty);
                }
            }

            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var path = System.IO.Path.Combine(m_Path, buildTarget.ToString());
            if (GUILayout.Button($"Reveal {path}", EditorStyles.miniLabel)) Process.Start(m_Path);
        }

        void DrawBundleIdentifier()
        {
            m_BundleIdentifier = EditorGUILayout.TextField("Bundle Identifier", m_BundleIdentifier);
            PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, m_BundleIdentifier);
        }

        void DrawVersionNumber()
        {
            void IncrementVersionNumber(ref string versionNumber)
            {
                Regex regex = new Regex(@"(\d+)\D*$");
                Match match = regex.Match(versionNumber);

                if (match.Success)
                {
                    Group group = match.Groups[1];
                    int lastNumber = int.Parse(group.Value);
                    lastNumber++;
                    string prefix = versionNumber.Substring(0, group.Index);
                    string suffix = versionNumber.Substring(group.Index + group.Length);
                    versionNumber = prefix + lastNumber + suffix;
                }
            }

            using var scope = new EditorGUILayout.HorizontalScope();
            m_VersionNumber = EditorGUILayout.TextField("Version Number", m_VersionNumber);
            if (GUILayout.Button("++", GUILayout.Width(30)))
            {
                IncrementVersionNumber(ref m_VersionNumber);
            }

            PlayerSettings.bundleVersion = m_VersionNumber;
        }

        void Build()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

            string[] scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray();
            BuildPipeline.BuildPlayer(scenes, m_Path, buildTarget, m_BuildOptions);
        }

        void OpenPlayerSettings() => SettingsService.OpenProjectSettings("Project/Player");
        void OpenBuildSettings() => GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
    }
}
#endif