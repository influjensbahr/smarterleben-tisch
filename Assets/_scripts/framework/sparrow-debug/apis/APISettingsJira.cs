// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;

namespace Sparrow.BugTracking
{
    [Serializable]
    public class APISettingsJira : APISettings
    {
        public override string caption => "Jira";

        [SerializeField] string m_JiraURL = "https://jira.atlassian.com";
        [SerializeField] string m_UserEmail = "text@test.com";
        [SerializeField] string m_APIToken = "abcdefghijklmnop";
        [SerializeField] EmbeddedSaveElement m_ProjectKey = null;
        [SerializeField] EmbeddedSaveElement m_IssueType = null;

        EmbeddedSave[] m_LoadedData = new EmbeddedSave[2];

        public string url => m_JiraURL.EndsWith("/") ? m_JiraURL.Substring(0, m_JiraURL.LastIndexOf("/")) : m_JiraURL;
        private string apiKey => Convert.ToBase64String(Encoding.UTF8.GetBytes(m_UserEmail + ":" + m_APIToken));

        public override async Task<bool> SendReport(ReportData report)
        {
            string ticketURL = await CreateIssue(report);

            if(!ticketURL.Equals("") && report.screenshot != null)
                return await UploadScreenshot(ticketURL, report);
            return !ticketURL.Equals("");
        }

        private async Task<bool> UploadScreenshot(string issueKey, ReportData report)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", apiKey);
            httpClient.DefaultRequestHeaders.Add("X-Atlassian-Token", "no-check");

            var requrl = $"{url}/rest/api/3/issue/{issueKey}/attachments";

            var metadataPart = new StringContent(JsonUtility.ToJson(new UploadDescription { fileName = "screenshot.png", description = "Beschreibung des Anhangs" }), Encoding.UTF8, "application/json");

            var fileContent = new ByteArrayContent(report.GetScreenshotByteArray());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            var contentDispositionHeader = $"form-data; name=\"file\"; filename=\"{Path.GetFileName("screenshot.png")}\"";
            fileContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(contentDispositionHeader);

            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(metadataPart, "metadata");
            multipartContent.Add(fileContent, "file");

            var response = await httpClient.PostAsync(requrl, multipartContent);
            if (!response.IsSuccessStatusCode)
                Debug.Log("[Jira] Upload screenshot error: " + response);

            httpClient.Dispose();
            return response.IsSuccessStatusCode;
        }

        private async Task<string> CreateIssue(ReportData report)
        {
            var serializedPayload = JsonConvert.SerializeObject(new CreateIssueRequest
            {
                fields = new CreateIssueRequestInner
                {
                    project = new ProjectListResult
                    {
                        id = m_ProjectKey.id
                    },
                    summary = report.headline,
                    description = report.fullLog,
                    issuetype = new IssueType
                    {
                        id = m_IssueType.id
                    }
                }
            });

            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            await WebUtils.PostSingleObject($"{url}/rest/api/2/issue/", serializedPayload,
            s =>
            {
                try
                {
                    CreateIssueResponse response = JsonConvert.DeserializeObject<CreateIssueResponse>(s);
                    Debug.Log("[Jira] Success sending a report to Jira.");
                    tcs.SetResult(response.key);
                }
                catch
                {
                    tcs.SetResult("");
                }
            },
            err =>
            {
                Debug.LogError($"[Jira] Error creating report on Jira: {err}");
                tcs.SetResult("");
            },
            headers: new List<(string, string)> {
                ("Content-Type", "application/json"),
                ("Authorization", "Basic " + apiKey)
            });

            return await tcs.Task;
        }

#if UNITY_EDITOR
        public override void DrawInternalEditor()
        {
            if (m_LoadedData[0] == null || !m_LoadedData[0].isSetup) m_LoadedData[0] = new EmbeddedSave("Project", () => TriggerProjectListLoad(0, "project"));
            if (m_LoadedData[1] == null || !m_LoadedData[1].isSetup) m_LoadedData[1] = new EmbeddedSave("Issue Type", () => TriggerIssueTypeLoad(1, "issue/createmeta/{projectIdOrKey}/issuetypes"));

            string changetest = m_JiraURL + m_APIToken + m_ProjectKey + m_UserEmail;
            
            m_JiraURL = EditorGUILayout.TextField("Endpoint URL", m_JiraURL);
            m_APIToken = EditorGUILayout.PasswordField("API Key", m_APIToken);
            m_UserEmail = EditorGUILayout.TextField("User Email", m_UserEmail);
            m_ProjectKey = m_LoadedData[0].DrawEditor(m_ProjectKey);// (0, "Project", "project", 0);
            GUI.enabled = !m_ProjectKey.Equals("");
            m_IssueType = m_LoadedData[1].DrawEditor(m_IssueType);// (0, "Project", "project", 0);
            GUI.enabled = true;

            EditorUtils.Space();
            EditorUtils.SmallLabel("Note that your API key will be saved in the Unity build which might be a security concern. Make sure to restrict the API key used here and maybe remove it for release versions.");

            if (!(m_JiraURL + m_APIToken + m_ProjectKey + m_UserEmail).Equals(changetest))
            {
                m_TestedWorking = false;
            }
        }

        private void TriggerProjectListLoad(int index, string urlEndpoint)
        {
            _ = WebUtils.GetArrayResponse<ProjectListResult>($"{url}/rest/api/2/{urlEndpoint}/",
            s =>
            {
                List<EmbeddedSaveElement> result = new List<EmbeddedSaveElement>();
                foreach (ProjectListResult elem in s.items)
                {
                    result.Add(new EmbeddedSaveElement() { id = elem.id, name = elem.name });
                }
                m_LoadedData[index].SetData(result);
                m_TestedWorking = true;
            },
            err => Debug.LogError($"Could not get {caption} list from endpoint: {err}"),
            headers: new List<(string, string)> {
                ("Content-Type", "application/json"),
                ("Authorization", "Basic " + apiKey)
            }, augmentArrayNotation: true);
        }

        private void TriggerIssueTypeLoad(int index, string urlEndpoint)
        {
            _ = WebUtils.GetSingleObject<IssueTypeListResult>($"{url}/rest/api/2/{urlEndpoint.Replace("{projectIdOrKey}", m_ProjectKey.id)}/",
            s =>
            {
                List<EmbeddedSaveElement> result = new List<EmbeddedSaveElement>();
                foreach (IssueType elem in s.issueTypes)
                {
                    result.Add(new EmbeddedSaveElement() { id = elem.id, name = elem.untranslatedName });
                }
                m_LoadedData[index].SetData(result);
                m_TestedWorking = true;
            },
            err => Debug.LogError($"Could not get {caption} list from endpoint: {err}"),
            headers: new List<(string, string)> {
                ("Content-Type", "application/json"),
                ("Authorization", "Basic " + apiKey)
            });
        }
#endif

        [Serializable]
        private class ProjectListResult
        {
            public string id;
            public string key;
            public string name;
        }

        [Serializable]
        private class IssueTypeListResult
        {
            public List<IssueType> issueTypes;
        }

        [Serializable]
        private class IssueType
        {
            public string untranslatedName;
            public string id;
        }

        [SerializeField]
        private class CreateIssueRequest
        {
            public CreateIssueRequestInner fields;
        }

        [SerializeField]
        private class CreateIssueRequestInner
        {
            public string summary;
            public string description;
            public IssueType issuetype;
            public ProjectListResult project;
        }

        [SerializeField]
        private class CreateIssueResponse
        {
            public string key;
        }
    }
}