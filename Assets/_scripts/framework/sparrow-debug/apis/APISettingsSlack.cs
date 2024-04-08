// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
//

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Sparrow.BugTracking
{
    [Serializable]
    public class APISettingsSlack : APISettings
    {
        public override string caption => "Slack";

        [SerializeField] bool m_UseWebhook = false;
        [SerializeField] string m_WebhookURL = "https://hooks.slack.com/services/";
        [SerializeField] bool m_UseOAuthToken = false;
        [SerializeField] string m_OAuthToken = "your-oauth-token";
        [SerializeField] string m_ChannelID = "C123456789";

        public async override Task<bool> SendReport(ReportData report)
        {
            bool oauth = (m_UseOAuthToken && report.screenshot != null ? await SendOAuth(report) : true);
            bool webhook = (m_UseWebhook && !oauth ? await SendWebhook(report) : true);
            return oauth && webhook;
        }

        private async Task<bool> SendOAuth(ReportData report) {
            using (HttpClient client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent();
                // Hier tragen Sie Ihren OAuth Access Token ein
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_OAuthToken);

                byte[] byteArray = report.GetScreenshotByteArray();
                requestContent.Add(new StreamContent(new MemoryStream(byteArray)), "file", "screenshot.png");
                requestContent.Add(new StringContent(m_ChannelID), "channels");
                requestContent.Add(new StringContent(report.fullLog), "initial_comment");
                requestContent.Add(new StringContent("png"), "filetype");

                HttpResponseMessage response = await client.PostAsync("https://slack.com/api/files.upload", requestContent);
                string responseBody = await response.Content.ReadAsStringAsync();
                SlackResponse slackResponse = JsonUtility.FromJson<SlackResponse>(responseBody);

                if (slackResponse.ok)
                {
                    Debug.Log("[Slack] Success sending a report via slack oauth.");
                    return true;
                }
                else
                {
                    Debug.Log($"[Slack] Error sending slack report via OAuth:  {responseBody}");
                    return false;
                }
            }
        }

        private async Task<bool> SendWebhook(ReportData report)
        {
            string json = JsonUtility.ToJson(new SlackWebhook() { text = report.fullLog });
            return await WebUtils.PostSingleObject(m_WebhookURL, json, (success) =>
            {
                Debug.Log("[Slack] Success sending a report via slack webhook." + success);
            }, (error) =>
            {
                Debug.Log($"[Slack] Error sending report via slack webhook: {error}");
            }, expectedValue: "ok");
        }

        [System.Serializable]
        public class SlackResponse
        {
            public bool ok;
            public string error;
        }

        [System.Serializable]
        public class SlackWebhook
        {
            public string text;
        }

#if UNITY_EDITOR
        public override void DrawInternalEditor()
        {
            EditorUtils.SmallLabel("You can use a simple webhook or provide an OAuth token. Note that webhooks don't support sending screenhots, but are easier to setup. If no screenshot is provided, the webhook will be used to send the report.");
            m_UseWebhook = EditorGUILayout.Toggle("Use Webhook", m_UseWebhook);
            if(m_UseWebhook)
                m_WebhookURL = EditorGUILayout.TextField("Webhook URL", m_WebhookURL);
            EditorUtils.Space();
            m_UseOAuthToken = EditorGUILayout.Toggle("Use OAuth Token", m_UseOAuthToken);
            if(m_UseOAuthToken)
            {
                m_OAuthToken = EditorGUILayout.TextField("OAuth Token", m_OAuthToken);
                m_ChannelID = EditorGUILayout.TextField("ChannelID", m_ChannelID);
                EditorUtils.SmallLabel("Make sure that your slack bot has the 'files:write' scope and has been added to the channel where you want to post.");
            }
            EditorUtils.Space();
            EditorUtils.SmallLabel("Note that the webhook entered here will be saved in your game build. For security reasons, we suggest removing it for release builds.");

            if (!m_UseOAuthToken && !m_UseWebhook) m_TestedWorking = false;
        }
#endif
    }
}
