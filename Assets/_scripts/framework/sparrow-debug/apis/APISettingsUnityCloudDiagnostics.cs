// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
//

using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Build;
#endif

#if SPARROW_UNITY_CLOUD_DIAGNOSTICS
using Unity.Services.UserReporting;
using Unity.Services.UserReporting.Client;
using Unity.Services.Core;
#else
#if UNITY_EDITOR
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System.Linq;
#endif
#endif

namespace Sparrow.BugTracking
{
    [Serializable]
    public class APISettingsUnityCloudDiagnostics : APISettings
    {
        public override string caption => "Unity Cloud Diagnostics";

        [SerializeField] bool m_EnableProjectIDOverride = false;
        [SerializeField] string m_ProjectIDOverride = "";
        [SerializeField] bool m_TakeAdditionalScreenshot = false;


        public async override Task<bool> SendReport(ReportData report)
        {
#if SPARROW_UNITY_CLOUD_DIAGNOSTICS
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            await UnityServices.InitializeAsync();
            var customConfig = new UserReportingClientConfiguration(5, 5, 100, 100, MetricsGatheringMode.Manual);
            UserReportingService.Instance.Configure(customConfig);

            if (report.screenshot != null)
            {
                byte[] attachment = report.GetScreenshotByteArray();
                UserReportingService.Instance.AddAttachmentToReport("Screenshot", "Screenshot.png", report.GetScreenshotByteArray(), "image/png");
            }

            UserReportingService.Instance.CreateNewUserReport();

            if (m_TakeAdditionalScreenshot)
                UserReportingService.Instance.TakeScreenshot(1920, 1080);

            UserReportingService.Instance.SetReportSummary(report.headline);
            UserReportingService.Instance.SetReportDescription(report.fullLog);
            if(m_EnableProjectIDOverride)
                UserReportingService.Instance.SetProjectIdentifier(m_ProjectIDOverride);

            UserReportingService.Instance.SendUserReport((f) => {}, (b) => {
                Debug.Log("[UnityCloudDiagnostics] A user report has " + (b ? "" : "NOT") + " been sent.");
                tcs.SetResult(b);
            });

            return await tcs.Task;
#else
            if(m_EnableProjectIDOverride && m_ProjectIDOverride.Equals(""))
                await Task.Delay(1);
            await Task.Delay(1);
            return false;
#endif
        }


#if UNITY_EDITOR

#if !SPARROW_UNITY_CLOUD_DIAGNOSTICS
        ListRequest packagesLoading = null;
        bool loadButNotCompared = true;
        (string, string, bool)[] m_PackagesToInstall = null;

        private void RefreshPackageList()
        {
            m_PackagesToInstall = new (string, string, bool)[]
            {
                ("User Reporting","com.unity.services.user-reporting", false),
                ("Cloud Diagnostics","com.unity.services.cloud-diagnostics", false),
            };
            packagesLoading = Client.List(true);
            loadButNotCompared = true;
        }
#endif

        public override void DrawInternalEditor()
        {
#if SPARROW_UNITY_CLOUD_DIAGNOSTICS
            string projectIDOverride = m_ProjectIDOverride;
            m_EnableProjectIDOverride = EditorGUILayout.Toggle("Override Project ID", m_EnableProjectIDOverride);
            if (m_EnableProjectIDOverride)
                m_ProjectIDOverride = EditorGUILayout.TextField("Project ID Override", m_ProjectIDOverride);
            m_TakeAdditionalScreenshot = EditorGUILayout.Toggle("Take additional screenshot", m_TakeAdditionalScreenshot);
            
            m_TestedWorking = true;
#else
            EditorGUILayout.HelpBox("Please import the \"Unity Cloud Diagnostics\" and \"User Reporting\" packages to your project and then add the scripting symbol SPARROW_UNITY_CLOUD_DIAGNOSTICS to enable this section.", MessageType.Info);

            m_TestedWorking = false;
            if(m_PackagesToInstall == null || packagesLoading == null || GUILayout.Button("Refresh package list"))
                 RefreshPackageList();

            EditorUtils.Space();

            if (packagesLoading.IsCompleted)
            {
                if(loadButNotCompared)
                {
                    for(int i = 0; i < m_PackagesToInstall.Length; i++)
                    {
                        foreach(var info in packagesLoading.Result.AsEnumerable())
                        {
                            if (info.assetPath.Contains(m_PackagesToInstall[i].Item2))
                            {
                                m_PackagesToInstall[i].Item3 = true;
                                break;
                            }
                        }
                    }
                }

                foreach (var a in m_PackagesToInstall)
                {
                    if (a.Item3) continue;
                    
                    if (GUILayout.Button($"Install {a.Item1}"))
                    {
                        if (EditorUtility.DisplayDialog("Are you sure?",  $"This will install {a.Item1} package in your project. Proceed?", "Yes", "No"))
                        {
                            Client.Add(a.Item2);
                        }
                    }
                }

                if(m_PackagesToInstall.All(q => q.Item3 == true))
                {
                    EditorGUILayout.LabelField("All packages installed, now activate this module!");
                    if (GUILayout.Button("Add scripting define to all build targets"))
                    {
                        if (EditorUtility.DisplayDialog("Are you sure?", "This will add the 'SPARROW_UNITY_CLOUD_DIAGNOSTICS' define to all build target groups. Are you sure you want to do this?", "Yes", "No"))
                        {
                            foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
                            {
                                try
                                {
                                    var target = NamedBuildTarget.FromBuildTargetGroup(group);
                                    var defines = PlayerSettings.GetScriptingDefineSymbols(target);
                                    if (!defines.Contains("SPARROW_UNITY_CLOUD_DIAGNOSTICS"))
                                    {
                                        defines += ";SPARROW_UNITY_CLOUD_DIAGNOSTICS";
                                        PlayerSettings.SetScriptingDefineSymbols(target, defines);
                                    }
                                }catch(Exception) {}
                            }
                        }
                    }
                }
            }
#endif

#if SPARROW_UNITY_CLOUD_DIAGNOSTICS
            EditorUtils.Space();
            EditorUtils.SmallLabel("If you choose to remove Unity Cloud Diagnostics from your project, hit this button to make this package independent of it again.");
            // Button to remove the scripting define
            if (GUILayout.Button("Remove scripting define from all build targets"))
            {
                foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
                {
                    try
                    {
                        var target = NamedBuildTarget.FromBuildTargetGroup(group);
                        var defines = PlayerSettings.GetScriptingDefineSymbols(target);
                        if (defines.Contains("SPARROW_UNITY_CLOUD_DIAGNOSTICS"))
                        {
                            defines = defines.Replace("SPARROW_UNITY_CLOUD_DIAGNOSTICS;", "").Replace("SPARROW_UNITY_CLOUD_DIAGNOSTICS", "");
                            PlayerSettings.SetScriptingDefineSymbols(target, defines);
                        }
                    } catch (Exception) { }
                    
                }
            }
#endif
        }
#endif
    }
}
