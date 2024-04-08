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
    public class APISettingsOwnEndpoint : APISettings
    {
        public override string caption => "Own API Endpoint";

        [SerializeField] string m_EndpointURL = "https://my.api.com/api/webhook/";
        
        public async override Task<bool> SendReport(ReportData report)
        {
            using (HttpClient client = new HttpClient())
            {
                var multipartContent = new MultipartFormDataContent();
                multipartContent.Add(new StringContent(report.log), "content");
                multipartContent.Add(new StringContent(report.headline), "headline");
                multipartContent.Add(new StringContent(report.userComment), "userComment");

                if (report.screenshot != null)
                {
                    byte[] imageBytes = report.GetScreenshotByteArray();
                    multipartContent.Add(new ByteArrayContent(imageBytes, 0, imageBytes.Length), "file", "screenshot.png");
                }

                var result = await client.PostAsync(m_EndpointURL, multipartContent);

                if (result.IsSuccessStatusCode)
                {
                    Debug.Log("[Own API] Success sending a report via own API.");
                    return true;
                }
                else
                {
                    Debug.Log("[Own API] Error sending report to your own API: " + result.StatusCode);
                    return false;
                }
            }
        }


#if UNITY_EDITOR
        public override void DrawInternalEditor()
        {
            m_EndpointURL = EditorGUILayout.TextField("API Endpoint URL", m_EndpointURL);
            EditorUtils.Space();
            EditorUtils.SmallLabel("Refer to documentation for implementation details. This API endpoint works much like a webhook for discord would, but sends data in separate fields. Note that the webhook entered here will be saved in your game build. For security reasons, we suggest removing it for release builds.");
        }
#endif
    }
}
