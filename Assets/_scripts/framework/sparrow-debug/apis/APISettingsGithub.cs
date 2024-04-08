// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Marc Freitag // Jens Bahr
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Sparrow.BugTracking
{
    [Serializable]
    public class APISettingsGithub : APISettings
    {
        public override string caption => "Github";

        [SerializeField] string m_ApiKey;
        [SerializeField] string m_RepoOwner;
        [SerializeField] string m_RepoName;

        public override async Task<bool> SendReport(ReportData report)
        {
            var json = JsonUtility.ToJson(new GitHubIssue() { title = report.headline, body = report.userComment });

            return await WebUtils.PostSingleObject($"https://api.github.com/repos/{m_RepoOwner}/{m_RepoName}/issues", json,
                (success) =>
                {
                    Debug.Log($"[Github] Success sending github report.");
                }, (error) =>
                {
                    Debug.Log($"[Github] Error sending github issue: {error}");
                }, new List<(string, string)>
                {
                    ("Authorization", "token " + m_ApiKey),
                    ("Content-Type", "application/json")
                });
        }


#if UNITY_EDITOR
        public override void DrawInternalEditor()
        {
            string changeTest = m_RepoName + m_RepoOwner + m_ApiKey;
            m_RepoOwner = EditorGUILayout.TextField("Repo Owner", m_RepoOwner);
            m_RepoName = EditorGUILayout.TextField("Repo Name", m_RepoName);
            m_ApiKey = EditorGUILayout.PasswordField("API Token", m_ApiKey);

            EditorUtils.SmallLabel("Note that github does not support uploading attachments via API, which is why no screenshot will be sent (we are working on a workaround like uploading the images to another service first). Try to use fine grained access tokens. If you use a personal access token, make sure it has the repo scope. Note that these credentials will be included in builds, maybe remove them for release builds.");

            if (!changeTest.Equals(m_RepoName + m_RepoOwner + m_ApiKey))
                m_TestedWorking = false;
        }
#endif
        [Serializable]
        private class GitHubIssue
        {
            public string title;
            public string body;
        }
    }
}