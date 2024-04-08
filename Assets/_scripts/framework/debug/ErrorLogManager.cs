// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Networking;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OTBT.Framework.Debugging
{
    public class ErrorLogManager : Singleton<ErrorLogManager>, IVerify
    {
        [SerializeField] bool m_LogToEmail = false;
        [SerializeField] bool m_LogToDiscord = false;
        [SerializeField] bool m_SendErrors = true;
        [SerializeField] bool m_SendExceptions = false;
        [SerializeField] bool m_OnlyInBuilds = true;

        [Header("Openproject sending")]
        [SerializeField] bool m_CreateTicket = false;
        [SerializeField] int m_ProjectID = 0;
        [SerializeField] string m_TicketSubject = "Ingame Bug Report: ";
        [SerializeField] string m_ReportDiscordChannel = "1091289105633124414";
        [SerializeField] string m_EmailHeader = "OTBT Gameplay Bug Report: ";

        private List<string> logs = new List<string>();

        void Start()
        {
            AddLog("Start time: " + System.DateTime.Now.ToString("O"));
            AddLog("Platform: " + Application.platform.ToString());
            AddLog("Device model: " + SystemInfo.deviceModel);
            AddLog("Device name: " + SystemInfo.deviceName);
            AddLog("Device type: " + SystemInfo.deviceType);
            AddLog("Graphics device name: " + SystemInfo.graphicsDeviceName);
            AddLog("Graphics memory size: " + SystemInfo.graphicsMemorySize);
        }

        public void AddLog(string log)
        {
            logs.Add(log);
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        public Task SendLog(string notes = "", Sprite screenshot = null)
        {
            // add open scenes to the log
            int sceneCount = SceneManager.sceneCount;
           
            logs.Add("Number of Open Scenes: " + sceneCount);
            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                logs.Add($"Scene {i + 1}: {scene.name} (Loaded: {scene.isLoaded})");
            }
            AddLog("Log send time: " + System.DateTime.Now.ToString("O"));

            List<Task> sendOps = new();

            if(m_CreateTicket)
                sendOps.Add(PeytonUtility.PeytonTicket(m_TicketSubject, notes + "\n\n" + string.Join("\n\n", logs), notes, m_ProjectID, screenshot, m_ReportDiscordChannel));
            if (m_LogToDiscord)
                sendOps.Add(PeytonUtility.PeytonPost(notes + "\n\n" + string.Join("\n\n", logs)));
            if (m_LogToEmail)
                sendOps.Add(PeytonUtility.PeytonMail(notes + "\n\n" + string.Join("<br>", logs), m_EmailHeader));

            return Task.WhenAll(sendOps);
        }

        public async void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (m_OnlyInBuilds && Application.isEditor) return;

            logs.Add(logString);

            if (type == LogType.Error || type == LogType.Exception || (logString.Contains("An error inside a tween callback was taken care of")))
            {
                logs.Add(stackTrace);
            }

            // add some additional info like open scenes, platform, etc

            if(((type == LogType.Exception) && m_SendExceptions) || ((type == LogType.Error || (logString.Contains("An error inside a tween callback was taken care of"))) && m_SendErrors)) {
                await SendLog();
            }
        }

        public void Verify(CheckVerifyInterface checker)
        {
        }
    }
}
