//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Utils;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace OTBT.Framework.Networking
{
    public class WebDownloadProgress 
    {
        public const ulong k_DefaultBytesAssumed = (ulong) (5f / 0.000001f); // assume 5MB if we dont have size info yet

        enum DownloadStatus { LOADING, DONE, FAILED };
        Dictionary<string, ProgressInformation> m_Progress = new Dictionary<string, ProgressInformation>();

        int m_Completed = 0;
        int m_Failed = 0;
        ulong m_DownloadedBytes = 0;
        ulong m_TotalBytes = 0;

        public int count => m_Progress.Count;
        public int completed => m_Completed;
        public int fails => m_Failed;
        public ulong bytesLoaded => m_DownloadedBytes;
        public ulong bytesTotal => m_TotalBytes;
        public float currentPercent => m_TotalBytes == 0 ? AverageReportedProgress() : ((float) m_DownloadedBytes / (float) m_TotalBytes);

        class ProgressInformation
        {
            public DownloadStatus status;
            public float progress;
            public ulong bytesLoaded;
            public ulong bytesTotal;
            public bool hasBytesTotalInfo;
        }

        public void Clear()
        {
            m_Progress.Clear();
            UpdateProgress();
            UpdateTotalBytes();
            m_Completed = 0;
            m_Failed = 0;
        }

        public void StartDownload(string filename)
        {
            if (m_Progress.ContainsKey(filename)) m_Progress.Remove(filename);
            ProgressInformation newProgress = new()
            {
                status = DownloadStatus.LOADING,
                progress = 0f,
                bytesTotal = 0,
                bytesLoaded = 0,
                hasBytesTotalInfo = false
            };
            m_Progress.Add(filename, newProgress);
            UpdateProgress();
        }

        public void FinishDownload(string filename, string url, bool success = true)
        {
            if (!m_Progress.ContainsKey(filename)) StartDownload(filename);
            if (m_Progress[filename].status != DownloadStatus.LOADING) return;
            m_Progress[filename].status = success ? DownloadStatus.DONE : DownloadStatus.FAILED;
            m_Progress[filename].bytesLoaded = m_Progress[filename].bytesTotal;
            if (success) m_Completed++;
            if (!success)
            {
                Dbg.Log(null, "Download fail: " + url + " to " + filename);
                m_Failed++;
            }
            UpdateProgress();
        }

        public void ReportProgress(string filename, UnityWebRequest request)
        {
            if (!m_Progress.ContainsKey(filename)) StartDownload(filename);
            ProgressInformation info = m_Progress[filename];
            if (info.status != DownloadStatus.LOADING) return;
            info.progress = request.downloadProgress;
            info.bytesLoaded = request.downloadedBytes;

            if (!info.hasBytesTotalInfo)
            {
                string headerValue = request.GetResponseHeader("Content-Length");
                if (headerValue != null)
                {
                    info.bytesTotal = ulong.Parse(headerValue);
                    info.hasBytesTotalInfo = true;
                    UpdateTotalBytes();
                }
            }
            UpdateProgress();
        }

        void UpdateTotalBytes()
        {
            m_TotalBytes = 0;
            foreach (ProgressInformation inf in m_Progress.Values)
                m_TotalBytes += inf.hasBytesTotalInfo ? inf.bytesTotal : k_DefaultBytesAssumed;
        }

        void UpdateProgress()
        {
            m_DownloadedBytes = 0;
            foreach (ProgressInformation inf in m_Progress.Values)
                m_DownloadedBytes += inf.bytesLoaded;
        }

        float AverageReportedProgress()
        {
            float ret = 0;
            foreach (ProgressInformation inf in m_Progress.Values)
                ret += (inf.progress) / (float) m_Progress.Count;
            return ret;
        }
    }
}
