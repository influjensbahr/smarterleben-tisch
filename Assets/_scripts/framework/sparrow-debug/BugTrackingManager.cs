// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
//

using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor;
#endif
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace Sparrow.BugTracking
{
    public class BugTrackingManager : Singleton<BugTrackingManager>
    {
        private List<(DateTime, string)> logs = new List<(DateTime, string)>();

        // -- What to track
        [Tooltip("Default headline to be sent with the logs, if none is provided by the users")]
        [SerializeField] string m_DefaultHeadline = "Unity Bug Report";
        [Tooltip("Includes the current build version from your build settings in your reports.")]
        [SerializeField] bool m_BuildNumber = true;
        [Tooltip("Includes the bundle id from your build settings in your reports.")]
        [SerializeField] bool m_BundleId = true;
        [Tooltip("Includes the number of open scenes in your reports.")]
        [SerializeField] bool m_OpenSceneCount = true;
        [Tooltip("Includes a list of open scenes in your reports.")]
        [SerializeField] bool m_OpenSceneList = true;
        [Tooltip("Includes the time when the game is started.")]
        [SerializeField] bool m_StartTime = true;
        [Tooltip("Includes the current time when the log is sent.")]
        [SerializeField] bool m_CurrentTime = true;
        [Tooltip("Includes the platform the game is run on (Android/iOS etc).")]
        [SerializeField] bool m_Platform = true;
        [Tooltip("Includes the device model information.")]
        [SerializeField] bool m_DeviceModel = false;
        [Tooltip("Includes the device name information.")]
        [SerializeField] bool m_DeviceName = false;
        [Tooltip("Includes the device type information.")]
        [SerializeField] bool m_DeviceType = false;
        [Tooltip("Includes the graphics device name information.")]
        [SerializeField] bool m_GraphicsDeviceName = false;
        [Tooltip("Includes the graphics device memory size information.")]
        [SerializeField] bool m_GraphicsDeviceMemorySize = false;

        [Tooltip("Includes all information that was logged using Debug.Log()")]
        [SerializeField] bool m_IncludeDebugLog = false;
        [Tooltip("Includes all information that was logged using Debug.LogException()")]
        [SerializeField] bool m_IncludeDebugException = true;
        [Tooltip("Includes all information that was logged using Debug.LogError()")]
        [SerializeField] bool m_IncludeDebugError = true;
        [Tooltip("Includes all information that was logged using Debug.LogWarning()")]
        [SerializeField] bool m_IncludeDebugWarnings = false;
        [Tooltip("Includes all information that was logged using Debug.LogAssertion()")]
        [SerializeField] bool m_IncludeDebugAssertion = true;
        [Tooltip("Includes stack traces for Exceptions and Erros (when available)")]
        [SerializeField] bool m_IncludeStackTraces = true;
        [Tooltip("You can set a custom text to be included, for example to identify different builds and versions")]
        [SerializeField] string m_IncludeCustomText= "";

        // -- When to track
        [Tooltip("Automatically send a report when an error occurs")]
        [SerializeField] bool m_AutoSendOnError = true;
        [Tooltip("Automatically send a report when an exception occurs")]
        [SerializeField] bool m_AutoSendOnException = false;
        [Tooltip("Automatically send a report when a warning occurs")]
        [SerializeField] bool m_AutoSendOnWarning = false;
        [Tooltip("Only send reports in builds and not in editor")]
        [SerializeField] bool m_OnlySendInBuilds = true;

        // -- Where to send it
        [SerializeField] public APISettingsTrello trello;
        [SerializeField] public APISettingsGithub github;
        [SerializeField] public APISettingsOpenProject openproject;
        [SerializeField] public APISettingsNotion notion;
        [SerializeField] public APISettingsEmail email;
        [SerializeField] public APISettingsDiscord discord;
        [SerializeField] public APISettingsSlack slack;
        [SerializeField] public APISettingsUnityCloudDiagnostics unityCloud;
        [SerializeField] public APISettingsOwnEndpoint ownEndpoint;
        [SerializeField] public APISettingsJira jira;


        public List<APISettings> apiSettingList => new ()
        {
            github, trello, openproject, jira, notion, email, unityCloud, discord, slack, ownEndpoint
        };

        public static ReportData GetTestReport()
        {
            return new ReportData() {
                headline = "Test report",
                log = "This is a test report sent by the Sparrow Feedback & Debug System. If you see this, the test is successful!",
                userComment = "This is a test user comment",
                screenshot = Resources.Load<Sprite>("sparrow_logos/bug_track_logo")
            };
        }

        public bool buildNumber  => m_BuildNumber;
        public bool bundleId  => m_BundleId;
        public bool openSceneCount  => m_OpenSceneCount;
        public bool openSceneList  => m_OpenSceneList;
        public bool startTime  => m_StartTime;
        public bool currentTime  => m_CurrentTime;
        public bool platform  => m_Platform;
        public bool deviceModel  => m_DeviceModel;
        public bool deviceName  => m_DeviceName;
        public bool deviceType  => m_DeviceType;
        public bool graphicsDeviceName  => m_GraphicsDeviceName;
        public bool graphicsDeviceMemorySize  => m_GraphicsDeviceMemorySize;
        public bool includeDebugLog  => m_IncludeDebugLog;
        public bool includeDebugException  => m_IncludeDebugException;
        public bool includeDebugError  => m_IncludeDebugError;
        public bool includeDebugWarnings  => m_IncludeDebugWarnings;
        public bool includeDebugAssertion => m_IncludeDebugAssertion;
        public bool includeStackTraces  => m_IncludeStackTraces;
        public string includeCustomText => m_IncludeCustomText;
        public bool autoSendOnError  => m_AutoSendOnError;
        public bool autoSendOnException  => m_AutoSendOnException;
        public bool autoSendOnWarning  => m_AutoSendOnWarning;
        public bool onlySendInBuilds  => m_OnlySendInBuilds;

        // -- save bundle id etc during builds
        [SerializeField] public int buildNumberInternal = 0;
        [SerializeField] public string bundleIdSaved = "";
        [SerializeField] public string buildNumberSaved = "";

#if UNITY_EDITOR
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
#if UNITY_IOS
            instance.bundleIdSaved = Application.identifier;
            instance.buildNumberSaved = Application.version;
#elif UNITY_ANDROID
            instance.bundleIdSaved = Application.identifier;
            instance.buildNumberSaved = Application.version.ToString();
#else
            instance.buildNumberInternal++;
            instance.buildNumberSaved = "#" + instance.buildNumberInternal;
            instance.bundleIdSaved = Application.companyName + " / " + Application.productName;
#endif
            EditorUtility.SetDirty(instance);
        }
#endif

        void Start()
        {
            AddLogInternal("-----------------------------------");
            AddLogInternal("General Application Data");
            AddLogInternal("-----------------------------------");
            AddLogInternal("");
            if (startTime)
                AddLogInternal("Start time: " + System.DateTime.Now.ToString("O"));
            if (platform)
                AddLogInternal("Platform: " + Application.platform.ToString());
            if (deviceModel)
                AddLogInternal("Device model: " + SystemInfo.deviceModel);
            if (deviceName)
                AddLogInternal("Device name: " + SystemInfo.deviceName);
            if (deviceType)
                AddLogInternal("Device type: " + SystemInfo.deviceType);
            if (graphicsDeviceName)
                AddLogInternal("Graphics device name: " + SystemInfo.graphicsDeviceName);
            if (graphicsDeviceMemorySize)
                AddLogInternal("Graphics memory size: " + SystemInfo.graphicsMemorySize);
            if (buildNumber)
                AddLogInternal("BuildNumber: " + buildNumberSaved);
            if (bundleId)
                AddLogInternal("BuildNumber: " + bundleIdSaved);
            if (!includeCustomText.Equals(""))
                AddLogInternal("Custom Text: " + includeCustomText);
            AddLogInternal("");
            AddLogInternal("");
            AddLogInternal("-----------------------------------");
            AddLogInternal("Application Log");
            AddLogInternal("-----------------------------------");
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        /// <summary>
        /// Add a custom log entry to the log that is gathered here, e.g. for custom events in your game. 
        /// Note that this does not send a report by itself.
        /// </summary>
        /// <param name="log">Text to be added to the log</param>
        public static void AddLog(string log)
        {
            instance.AddLogInternal(log);
        }
        private void AddLogInternal(string log)
        {
            logs.Add((DateTime.Now, log));
        }

        /// <summary>
        /// Saves log data that is otherwise collected in the console. Also can trigger the auto-send functions
        /// </summary>
        private async void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (onlySendInBuilds && Application.isEditor) return;
            if (!includeDebugError && type == LogType.Error) return;
            if (!includeDebugException && type == LogType.Exception) return;
            if (!includeDebugWarnings && type == LogType.Warning) return;
            if (!includeDebugLog && type == LogType.Log) return;
            if (!includeDebugAssertion && type == LogType.Assert) return;

            AddLogInternal(logString);

            if (includeStackTraces && !stackTrace.Equals("") && type != LogType.Log)
                AddLogInternal(stackTrace);

            if ((autoSendOnError && type == LogType.Error) ||
                (autoSendOnException && type == LogType.Exception) ||
                (autoSendOnWarning && type == LogType.Warning))
                await SendReport();
        }

        /// <summary>
        /// Sends a log to all the endpoints that have been activated and configured
        /// </summary>
        /// <param name="notes">Optional notes you can add to the report, e.g. an error report or user feedback</param>
        /// <param name="screenshot">Optional screenshot that can be sent to some api endpoints</param>
        /// <returns></returns>
        public Task SendReport(string headline = "", string notes = "", Sprite screenshot = null)
        {
            AddLogInternal("");
            AddLogInternal("-----------------------------------");

            // add open scenes to the log
            int sceneCount = SceneManager.sceneCount;


            if (openSceneCount)
                AddLogInternal("Number of Open Scenes: " + sceneCount);
            if (openSceneList)
            {
                for (int i = 0; i < sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    AddLogInternal($"Scene {i + 1}: {scene.name} (Loaded: {scene.isLoaded})");
                }
            }
            AddLogInternal("");
            AddLogInternal("-----------------------------------");
            AddLogInternal("A report has been sent.");
            AddLogInternal("This report was gathered using the Sparrow Feedback & Debug Report system.");
            AddLogInternal("-----------------------------------");

            // build the log to be sent
            ReportData report = new ReportData();
            report.log = string.Join("\n\n", logs);
            report.userComment = notes;
            report.screenshot = screenshot;
            report.headline = headline.Equals("") ? m_DefaultHeadline : headline;

            List <Task> sendOps = new();

            foreach (APISettings apiSetting in apiSettingList)
                if (apiSetting.isWorking && apiSetting.isActive)
                    sendOps.Add(apiSetting.SendReport(report));

            return Task.WhenAll(sendOps);
        }
    }
}
