// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
//

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Sparrow.BugTracking
{
    [Serializable]
    public class APISettingsOpenProject : APISettings
    {
        public override string caption => "Openproject";

        [SerializeField] string m_OpenProjectApiUrl = "https://myopenproject.url";
        [SerializeField] string m_APIKey = "abcdefghijklmnop";
        [SerializeField] EmbeddedSaveElement m_UserID;
        [SerializeField] EmbeddedSaveElement m_TicketTypeID;
        [SerializeField] EmbeddedSaveElement m_StatusID;
        [SerializeField] EmbeddedSaveElement m_PriorityID;
        [SerializeField] EmbeddedSaveElement m_ProjectID;

        EmbeddedSave[] m_LoadedData = new EmbeddedSave[5];


        public string url => m_OpenProjectApiUrl.EndsWith("/") ? m_OpenProjectApiUrl.Substring(0, m_OpenProjectApiUrl.LastIndexOf("/")) : m_OpenProjectApiUrl;

        public override async Task<bool> SendReport(ReportData report)
        {
            var taskData = new TaskData
            {
                subject = report.headline,
                description = new Description { format = "textile", raw = report.fullLog },
                _links = new Links
                {
                    type = new Link { href = $"/api/v3/types/{m_TicketTypeID.id}" },
                    status = new Link { href = $"/api/v3/statuses/{m_StatusID.id}" },
                    priority = new Link { href = $"/api/v3/priorities/{m_PriorityID.id}" },
                    assignee = new Link { href = $"/api/v3/users/{m_UserID.id}" },
                    project = new Link { href = $"/api/v3/projects/{m_ProjectID.id}" }
                }
            };
            var headers = new List<(string, string)>
            {
                ("Content-Type", "application/json"),
                ("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("apikey:" + m_APIKey)))
            };
            var body = JsonUtility.ToJson(taskData);

            string workPackageID = "";
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            bool reportSent = await WebUtils.PostSingleObject(url + "/api/v3/work_packages", body, 
                s =>
                {
                    Element response = JsonUtility.FromJson<Element>(s);
                    if (response._type.Equals("WorkPackage"))
                    {
                        workPackageID = response.id;
                        Debug.Log("[Openproject] Success sending a report to Openproject.");
                        tcs.SetResult(workPackageID);
                    } 
                },
                err =>
                {
                    Debug.LogError("[Openproject] Error sending the report: " + err);
                    tcs.SetResult("");
                }, headers);

            if(workPackageID.Equals(""))
            {
                Debug.LogError("[Openproject] The report was sent but something went wrong.");
                return false;
            }

            await tcs.Task;
            bool screenshotUploadSuccess = await UploadScreenshot(tcs.Task.Result, report);
            return reportSent && screenshotUploadSuccess;
        }

        private async Task<bool> UploadScreenshot(string workPackageID, ReportData report)
        {
            if (report.screenshot == null) return true;
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("apikey:" + m_APIKey)));

            var url = $"{m_OpenProjectApiUrl}/api/v3/work_packages/{workPackageID}/attachments";

            var metadataPart = new StringContent(JsonUtility.ToJson(new UploadDescription { fileName = "screenshot.png", description = "Beschreibung des Anhangs" }), Encoding.UTF8, "application/json");

            var fileContent = new ByteArrayContent(report.GetScreenshotByteArray());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            var contentDispositionHeader = $"form-data; name=\"file\"; filename=\"{Path.GetFileName("screenshot.png")}\"";
            fileContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(contentDispositionHeader);

            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(metadataPart, "metadata");
            multipartContent.Add(fileContent, "file");

            var response = await httpClient.PostAsync(url, multipartContent);
            if (!response.IsSuccessStatusCode)
                Debug.Log("Upload screenshot error: " + response);

            httpClient.Dispose();
            return response.IsSuccessStatusCode;
        }


#if UNITY_EDITOR
        public override void DrawInternalEditor()
        {
            string changetest = m_OpenProjectApiUrl + m_APIKey;
            m_OpenProjectApiUrl = EditorGUILayout.TextField("Endpoint URL", m_OpenProjectApiUrl);
            m_APIKey = EditorGUILayout.PasswordField("API Key", m_APIKey);

            EditorUtils.Space();
            EditorUtils.BoldLabel("Ticket defaults");

            if (m_LoadedData[0] == null || !m_LoadedData[0].isSetup) m_LoadedData[0] = new EmbeddedSave("User", () => TriggerDataLoad(0, "users"));
            if (m_LoadedData[1] == null || !m_LoadedData[1].isSetup) m_LoadedData[1] = new EmbeddedSave("TicketType", () => TriggerDataLoad(1, "types"));
            if (m_LoadedData[2] == null || !m_LoadedData[2].isSetup) m_LoadedData[2] = new EmbeddedSave("Status", () => TriggerDataLoad(2, "statuses"));
            if (m_LoadedData[3] == null || !m_LoadedData[3].isSetup) m_LoadedData[3] = new EmbeddedSave("Priority", () => TriggerDataLoad(3, "priorities"));
            if (m_LoadedData[4] == null || !m_LoadedData[4].isSetup) m_LoadedData[4] = new EmbeddedSave("Project", () => TriggerDataLoad(4, "projects"));

            m_UserID = m_LoadedData[0].DrawEditor(m_UserID);
            m_TicketTypeID = m_LoadedData[1].DrawEditor(m_TicketTypeID); 
            m_StatusID = m_LoadedData[2].DrawEditor(m_StatusID);
            m_PriorityID = m_LoadedData[3].DrawEditor(m_PriorityID);
            m_ProjectID = m_LoadedData[4].DrawEditor(m_ProjectID);

            EditorUtils.Space();
            EditorUtils.SmallLabel("Note that your API key will be saved in the Unity build which might be a security concern. Make sure to restrict the API key used here and maybe remove it for release versions.");

            if (!(m_OpenProjectApiUrl + m_APIKey).Equals(changetest))
            {
                m_TestedWorking = false;
            }
        }

        private void TriggerDataLoad(int index, string urlEndpoint)
        {
            _ = WebUtils.GetSingleObject<Root>($"{url}/api/v3/{urlEndpoint}/",
            s =>
            {
                List<EmbeddedSaveElement> result = new List<EmbeddedSaveElement>();
                foreach(Element elem in s._embedded.elements)
                {
                    result.Add(new EmbeddedSaveElement() { id = elem.id, name = elem.name });
                }
                m_LoadedData[index].SetData(result);
                m_TestedWorking = true;
            },
            err => Debug.LogError($"Could not get {caption} list from endpoint: {err}"),
            headers: new List<(string, string)> {
                ("Content-Type", "application/json"),
                ("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("apikey:" + m_APIKey)))
            });
        }
#endif
    }

    /**
     * Classes for JSON formatting to the Openproject API
     */
    [Serializable]
    class TaskData
    {
        public string subject;
        public Description description;
        public Links _links;
    }
    [Serializable]
    class Description
    {
        public string format;
        public string raw;
    }
    [Serializable]
    class Links
    {
        public Link type;
        public Link status;
        public Link priority;
        public Link assignee;
        public Link project;
    }
    [Serializable]
    class Link
    {
        public string href;
    }
    [Serializable]
    class Root
    {
        public string _type;
        public string total;
        public string count;
        public Embedded _embedded;
    }
    [Serializable]
    class UploadResponseEmbed
    {
        public string id;
    }

    [Serializable]
    class UploadDescription
    {
        public string fileName;
        public string description;
    }

    [Serializable]
    class Embedded
    {
        public List<Element> elements;        
    }
    [Serializable]
    class Element
    {
        public string _type;
        public string id;
        public string name;
    }


    
}