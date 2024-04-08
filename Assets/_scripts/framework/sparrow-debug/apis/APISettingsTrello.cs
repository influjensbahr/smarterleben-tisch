// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Marc Freitag // Jens Bahr
//

using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Sparrow.BugTracking
{
    [Serializable]
    public class APISettingsTrello : APISettings
    {
        public override string caption => "Trello";

        [SerializeField] string m_ApiKey = "";
        [SerializeField] string m_ApiToken = "";

        [SerializeField] EmbeddedSaveElement m_Board = null;
        [SerializeField] EmbeddedSaveElement m_List = null;

        EmbeddedSave[] m_LoadedData = new EmbeddedSave[2];

#if UNITY_EDITOR
        public override void DrawInternalEditor() 
        {
            string oldKeys = m_ApiKey + m_ApiToken;

            GUILayout.BeginHorizontal();
            m_ApiKey = EditorGUILayout.PasswordField("API Key", m_ApiKey);
            if (GUILayout.Button("Get"))
                Application.OpenURL("https://trello.com/power-ups/admin");
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            m_ApiToken = EditorGUILayout.PasswordField("API Token", m_ApiToken);
            GUI.enabled = !m_ApiKey.Equals("");
            if (GUILayout.Button("Get"))
                Application.OpenURL($"https://trello.com/1/authorize?expiration=never&name=BugTrack&scope=read,write&response_type=token&key={m_ApiKey}");
            GUILayout.EndHorizontal();

            // when something changed, remove the "tested working" flag
            if (!oldKeys.Equals(m_ApiKey + m_ApiToken)) m_TestedWorking = false;

            if (m_LoadedData[0] == null || !m_LoadedData[0].isSetup) m_LoadedData[0] = new EmbeddedSave("Board", () => GetTrelloData($"https://api.trello.com/1/members/me/boards?key={m_ApiKey}&token={m_ApiToken}", 0));
            if (m_LoadedData[1] == null || !m_LoadedData[1].isSetup) m_LoadedData[1] = new EmbeddedSave("List", () => GetTrelloData($"https://api.trello.com/1/boards/{m_Board.id}/lists?key={m_ApiKey}&token={m_ApiToken}", 1));

            GUI.enabled = (!m_ApiToken.Equals("") && !m_ApiKey.Equals(""));
            m_Board = m_LoadedData[0].DrawEditor(m_Board);
            m_List = m_LoadedData[1].DrawEditor(m_List);
            GUI.enabled = true;
        }

        private void GetTrelloData(string url, int index)
        {
            _ = WebUtils.GetArrayResponse<TrelloListData>(url,
                (results) =>
                {
                    m_TestedWorking = true;
                    List<EmbeddedSaveElement> lists = new List<EmbeddedSaveElement>();
                    foreach(var resu in results.items)
                        lists.Add(new EmbeddedSaveElement() { id = resu.id, name = resu.name });
                    m_LoadedData[index].SetData(lists);
                }, (error) =>
                {
                    Debug.Log($"[Trello] Error loading trello lists: {error}");
                }, augmentArrayNotation: true);
        }

#endif
        public override async Task<bool> SendReport(ReportData report)
        {
            TaskCompletionSource<TrelloCardData> tcs = new TaskCompletionSource<TrelloCardData>();
            bool listCreated = await WebUtils.PostSingleObject<TrelloCardData>(
                $"https://api.trello.com/1/cards?name={UnityWebRequest.EscapeURL(report.headline)}&idList={m_List.id}&key={m_ApiKey}&token={m_ApiToken}&desc={UnityWebRequest.EscapeURL(report.userComment)}",
                "", (success) =>
                {
                    Debug.Log($"[Trello] Success sending a report to Trello.");
                    m_TestedWorking = true;
                    tcs.SetResult(success);
                }, (error) =>
                {
                    Debug.Log($"[Trello] Error creating trello card to board {m_Board.name}: {error}");
                    tcs.SetResult(null);
                });
            await tcs.Task;

            bool screenshotUploaded = tcs.Task.Result == null ? false : await UploadScreenshot(tcs.Task.Result, report);
            return screenshotUploaded && listCreated;
        }

        private async Task<bool> UploadScreenshot(TrelloCardData cardData, ReportData report)
        {
            if (report.screenshot == null) return true;

            byte[] screenshotBytes = report.GetScreenshotByteArray();

            var attachmentUrl = $"https://api.trello.com/1/cards/{cardData.id}/attachments?key={m_ApiKey}&token={m_ApiToken}";

            WWWForm form = new WWWForm();
            form.AddBinaryData("file", screenshotBytes, "screenshot.png", "image/png");

            var attachmentRequest = UnityWebRequest.Post(attachmentUrl, form);
            var attachmentOperation = attachmentRequest.SendWebRequest();

            while (!attachmentOperation.isDone)
            {
                await Task.Yield();
            }

            if (attachmentRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"[Trello] Error uploading screenshot to Trello: {attachmentRequest.error}");
                return false;
            }
            return true;
        }
    }

    [Serializable]
    public class TrelloBoardData
    {
        public string id;
        public string name;
        public bool closed;
    }

    [Serializable]
    public class TrelloListData
    {
        public string id;
        public string name;
        public bool closed;
    }

    [Serializable]
    public class TrelloCardData
    {
        public string id;
        public string name;
        public string desc;
        public bool closed;
    }
}