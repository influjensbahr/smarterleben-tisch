using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Sparrow.BugTracking
{
    [Serializable]
    public class APISettingsNotion : APISettings
    {
        public override string caption => "Notion";

        [SerializeField] private string m_NotionIntegrationToken = "secret_xxx";
        [SerializeField] private EmbeddedSaveElement m_DatabaseId;

        [SerializeField] private EmbeddedSaveElement m_TitleField;
        [SerializeField] private EmbeddedSaveElement m_CommentField;
        [SerializeField] private EmbeddedSaveElement m_FullLogField;

        EmbeddedSave[] m_LoadDatabases = new EmbeddedSave[4];

        // This method sends a report to a Notion database
        public override async Task<bool> SendReport(ReportData report)
        {
            var payload = PrepareReportProperties(report);
            var jsonPayload = JsonUtility.ToJson(payload);

            return await WebUtils.PostSingleObject("https://api.notion.com/v1/pages", jsonPayload, (s) =>
            {
                Debug.Log("[Notion] Report successfully sent.");
            }, (error) =>
            {
                Debug.LogError($"[Notion] Failed to send report: {error}");
            }, new List<(string, string)>()
            {
                ("Content-Type", "application/json"),
                ("Authorization", "Bearer " + m_NotionIntegrationToken),
                ("Notion-Version", "2022-02-22")
            });
        }

        private object PrepareReportProperties(ReportData report)
        {
            var properties = new Dictionary<string, object>();

            AddProperty(properties, m_TitleField.name, report.headline, m_TitleField.type);
            AddProperty(properties, m_CommentField.name, report.userComment, m_CommentField.type);
            AddProperty(properties, m_FullLogField.name, report.fullLog, m_FullLogField.type);

            return new { parent = new { database_id = m_DatabaseId.id }, properties };
        }

        private void AddProperty(Dictionary<string, object> properties, string fieldName, string content, string fieldType)
        {
            if (string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(content)) return;

            switch (fieldType)
            {
                case "title":
                    properties[fieldName] = new { title = new[] { new { text = new { content = content } } } };
                    break;
                case "rich_text":
                    properties[fieldName] = new { rich_text = new[] { new { text = new { content = content } } } };
                    break;
                default: break;
            }
        }



#if UNITY_EDITOR
        public override void DrawInternalEditor()
        {
            if (m_LoadDatabases[0] == null || !m_LoadDatabases[0].isSetup) m_LoadDatabases[0] = new EmbeddedSave("Database", TriggerDatabasesLoad);
            if (m_LoadDatabases[1] == null || !m_LoadDatabases[1].isSetup) m_LoadDatabases[1] = new EmbeddedSave("Headline Property", TriggerDatabaseStructureLoad);
            if (m_LoadDatabases[2] == null || !m_LoadDatabases[2].isSetup) m_LoadDatabases[2] = new EmbeddedSave("User Comment Property", TriggerDatabaseStructureLoad);
            if (m_LoadDatabases[3] == null || !m_LoadDatabases[3].isSetup) m_LoadDatabases[3] = new EmbeddedSave("Full Log Property", TriggerDatabaseStructureLoad);

            m_NotionIntegrationToken = EditorGUILayout.TextField("Integration Token", m_NotionIntegrationToken);
            m_DatabaseId = m_LoadDatabases[0].DrawEditor(m_DatabaseId);

            EditorUtils.Space();
            EditorUtils.SmallLabel("Select which Notion database fields to save the report data to.");
            m_TitleField = m_LoadDatabases[1].DrawEditor(m_TitleField);
            m_CommentField = m_LoadDatabases[2].DrawEditor(m_CommentField);
            m_FullLogField = m_LoadDatabases[3].DrawEditor(m_FullLogField);

            EditorUtils.Space();
            EditorUtils.SmallLabel("Note that Notion does not support uploading attachments via API, which is why no screenshot will be sent (we are working on a workaround like uploading the images to another service first). Your integration token is saved within the project. Ensure it is kept secure and maybe remove it from release builds.");
        }

        private void TriggerDatabasesLoad()
        {
            _ = WebUtils.PostSingleObject("https://api.notion.com/v1/search", "{\"filter\": {\"value\": \"database\", \"property\": \"object\"}}",
            s =>
            {
                DatabaseQueryWrappe result = JsonUtility.FromJson<DatabaseQueryWrappe>(s);
                List<EmbeddedSaveElement> list = new List<EmbeddedSaveElement>();
                foreach(DatabaseResult dbr in result.results)
                {
                    list.Add(new EmbeddedSaveElement()
                    {
                        id = dbr.id,
                        name = dbr.title.First().text.content
                    });
                }
                m_LoadDatabases[0].SetData(list);
                m_TestedWorking = true;
            },
            err => Debug.LogError($"Could not get {caption} list from endpoint: {err}"),
            headers: new List<(string, string)> {
                ("Content-Type", "application/json"),
                ("Authorization", "Bearer " + m_NotionIntegrationToken),
                ("Notion-Version", "2022-02-22")
            });
        }

        private void TriggerDatabaseStructureLoad()
        {
            _ = WebUtils.GetSimpleTextResponse($"https://api.notion.com/v1/databases/{m_DatabaseId.id}", 
            s =>
            {
                Debug.Log(s);
                List<EmbeddedSaveElement> list = new List<EmbeddedSaveElement>();
                JObject result = JObject.Parse(s);
                if (result.TryGetValue("properties", out var properties))
                {
                    var propertiesObject = properties.ToObject<JObject>();

                    foreach (var property in propertiesObject.Properties())
                    {
                        string propertyName = property.Name;
                        string propertyType = property.Value["type"].ToString();
                        if (!(propertyType.Equals("rich_text") || propertyType.Equals("title")))
                            continue;

                        list.Add(new EmbeddedSaveElement
                        {
                            name = propertyName,
                            type = propertyType
                        });
                    }
                }

                m_LoadDatabases[1].SetData(list);
                m_LoadDatabases[2].SetData(list);
                m_LoadDatabases[3].SetData(list);
                m_TestedWorking = true;
            },
            err => Debug.LogError($"Could not get {caption} list from endpoint: {err}"),
            headers: new List<(string, string)> {
                ("Content-Type", "application/json"),
                ("Authorization", "Bearer " + m_NotionIntegrationToken),
                ("Notion-Version", "2022-02-22")
            });
        }
#endif

        [Serializable]
        private class DatabaseQueryWrappe
        {
            public List<DatabaseResult> results;
        }

        [Serializable]
        private class DatabaseResult
        {
            public string id;
            public List<DatabaseTitleWrapper> title; 
        }

        [Serializable]
        private class DatabaseTitleWrapper
        {
            public DatabaseTitle text;
        }

        [Serializable]
        private class DatabaseTitle
        {
            public string content;
        }

    }
}
