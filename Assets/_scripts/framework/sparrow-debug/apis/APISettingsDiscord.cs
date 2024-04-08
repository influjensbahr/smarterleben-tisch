// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
//

using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Sparrow.BugTracking
{
    [Serializable]
    public class APISettingsDiscord : APISettings
    {
        public override string caption => "Discord";

        [SerializeField] string m_WebhookURL = "https://discord.com/api/webhooks/";
        
        public async override Task<bool> SendReport(ReportData report)
        {
            using (HttpClient client = new HttpClient())
            {
                var multipartContent = new MultipartFormDataContent();
                multipartContent.Add(new StringContent("**"+report.headline + "**\n"+report.fullLog), "content");

                if (report.screenshot != null)
                {
                    byte[] imageBytes = report.GetScreenshotByteArray();
                    multipartContent.Add(new ByteArrayContent(imageBytes, 0, imageBytes.Length), "file", "screenshot.png");
                }

                var result = await client.PostAsync(m_WebhookURL, multipartContent);

                if (result.IsSuccessStatusCode)
                {
                    Debug.Log("[Discord] Success sending a report via discord.");
                    return true;
                }
                else
                {
                    Debug.Log("[Discord] Error sending discord report: " + result.StatusCode);
                    return false;
                }
            }
        }


#if UNITY_EDITOR
        public override void DrawInternalEditor()
        {
            m_WebhookURL = EditorGUILayout.TextField("Webhook URL", m_WebhookURL);
            EditorUtils.Space();
            EditorUtils.SmallLabel("Note that the webhook entered here will be saved in your game build. For security reasons, we suggest removing it for release builds.");
        }
#endif
    }
}
