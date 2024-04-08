//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEngine;
using System;
using UnityEngine.Networking;
using OTBT.Framework.Utils;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace OTBT.Framework.Networking
{
    /// <summary>
    /// Simple connector to our analytics platform Matomo to collect simple game events
    /// </summary>
    public class MatomoConnection : Singleton<MatomoConnection>
    {
        private void Awake()
        {
            playerID = PlayerPrefs.GetString("matomo_playerId", "");
            if (playerID.Equals(""))
            {
                playerID = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("matomo_playerId", playerID);
            }
        }

        [HideInInspector]
        public string playerID = "";

        [HideInInspector]
        public string user = "-1";

        [SerializeField] int m_SiteID = 0;

        [SerializeField] bool m_UseMatomo = true;
        [SerializeField] bool m_AutoSendStartSignal = true;
        [SerializeField] string m_MatomoURL = "https://analytics.beatentrack.games/matomo.php";

        [SerializeField] bool m_OnlyInBuilds = true;

        private int m_Starts = 0;
        public int starts => m_Starts;

        private void Start()
        {
            if (m_OnlyInBuilds && Application.isEditor) return;
            m_Starts = PlayerPrefs.GetInt("matomo_starts", 0);
            user = "Unity/" + Application.unityVersion + "(" + SystemInfo.operatingSystem + " / " + Application.platform + ")";

            if (m_AutoSendStartSignal)
                TrackEvent("start");
        }

        public void TrackEvent(string eventName, int num = 0)
        {
            if (m_OnlyInBuilds && Application.isEditor) return;
            _ = TrackEventAsync(eventName, num);
        }

        private async Task TrackEventAsync(string eventName, int num = 0, int timeout = 1)
        {
            if (!m_UseMatomo) return;

            WWWForm form = new();
            if (eventName == "start")
            {
                m_Starts++;
                PlayerPrefs.SetInt("matomo_starts", m_Starts);
            }

            form.AddField("rec", 1);
            form.AddField("action_name", eventName);
            form.AddField("_cvar", "{\"1\":[\"OS\",\"" + Application.platform + "\"],\"2\":[\"Resolution\",\"" + Screen.width + "x" + Screen.height + "\"]}");
            form.AddField("res", Screen.width + "x" + Screen.height);
            form.AddField("ua", user);
            form.AddField("lang", "de-DE");
            form.AddField("idsite", m_SiteID);
            form.AddField("uid", playerID);
            form.AddField("op", eventName);
            form.AddField("num", num);

            UnityWebRequest request = UnityWebRequest.Post(m_MatomoURL, form);
            request.SetRequestHeader("ContentType", "application/json");
            request.timeout = timeout;

            UnityWebRequestAsyncOperation op = request.SendWebRequest();

            while (!op.isDone)
            {
                await Task.Yield();
            }
        }
    }
}