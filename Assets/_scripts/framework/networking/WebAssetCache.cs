//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr

using OTBT.Framework.Core;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Path = System.IO.Path;

namespace OTBT.Framework.Networking
{
    /// <summary>
    /// Used to download assets onto the device so they don't have to be redownloaded all the time.
    /// </summary>
    public class WebAssetCache : Singleton<WebAssetCache>, IVerify
    {
        [SerializeField] bool m_ForceMemoryCacheOnly = false;

        string savePath => Application.persistentDataPath + Path.DirectorySeparatorChar;
        private bool useInMemoryCache => m_ForceMemoryCacheOnly ||PlayerPrefs.GetInt("failsave_playerprefs", -1) >= 0;

        Dictionary<string, string> m_URLtoFilename = new Dictionary<string, string>();
        Dictionary<string, Sprite> m_CachedSprites = new Dictionary<string, Sprite>();

        bool m_StopAllRunningDownloads = false;

        protected override void InitializeInherit()
        {
            base.InitializeInherit();

            LoadDictionary();
        }

        public void SetStopSignal(bool signal)
        {
            m_StopAllRunningDownloads = signal;
        }

        private string GetFilename(string url, bool resetCache, string extensionOverride = "")
        {
            if (resetCache && m_URLtoFilename.ContainsKey(url))
            {
                if (File.Exists(savePath + m_URLtoFilename[url]))
                {
                    Dbg.Log(this, "Deleting: " + savePath + m_URLtoFilename[url]);
                    File.Delete(savePath + m_URLtoFilename[url]);
                }
                m_URLtoFilename.Remove(url);
            }

            if(!m_URLtoFilename.ContainsKey(url)) {
                string extension = (extensionOverride.Equals("") ? GetFileExtensionFromUrl(url) : extensionOverride);
                return Guid.NewGuid() + extension;
            }
            return m_URLtoFilename[url];
        }

        /// <summary>
        /// Use this when you want to setup a video or audio player that shall stream from the web if data has not been preloaded
        /// instead of downloading the whole file to disk before playback
        /// </summary>
        public string GetLocalPathOrReturnStreamURL(string assetURL)
        {
            if (m_URLtoFilename.ContainsKey(assetURL))
                return "file://" + savePath + m_URLtoFilename[assetURL];
            return assetURL;
        }

        public bool ContainsURL(string assetURL)
        {
            return (m_URLtoFilename.ContainsKey(assetURL));
        }

        public Task LoadImage(string assetURL, Action<Texture2D> onSuccessTexture2D, Action<Sprite> onSuccessSprite, Action<Exception> onFailure, bool resetCache = false, bool preloadOnly = false, WebDownloadProgress progress = null, string useSAS = "", bool nonReadableRaw = false, int scaleToSize = -1)
        {
            string fileName = GetFilename(assetURL, resetCache);
            if(m_CachedSprites.ContainsKey(fileName) && onSuccessSprite != null && m_CachedSprites[fileName] != null) {
                onSuccessSprite?.Invoke(m_CachedSprites[fileName]);
                return Task.Delay(0);
            } else if(m_CachedSprites.ContainsKey(fileName))
            {
                m_CachedSprites.Remove(fileName);
            }
            if (useInMemoryCache)
            {
                return GenericMemoryCacher(assetURL, fileName, UnityWebRequestTexture.GetTexture(assetURL + useSAS, scaleToSize > 0 ? false : nonReadableRaw), new DownloadHandlerTexture(),
                    (downloadHandler) => ImageLoadSuccessHandler(downloadHandler, onSuccessTexture2D, onSuccessSprite, onFailure, fileName, scaleToSize),
                    exception => onFailure?.Invoke(exception), progress) ;
            }
            else
            {
                return GenericDownloader(assetURL, fileName,
                UnityWebRequestTexture.GetTexture("file://" + savePath + fileName, scaleToSize > 0 ? false : nonReadableRaw),
                (downloadHandler) => ImageLoadSuccessHandler(downloadHandler, onSuccessTexture2D, onSuccessSprite, onFailure, fileName, scaleToSize),
                exception => onFailure?.Invoke(exception), progress, preloadOnly, useSAS);
            }
        }

        public Task LoadAudioClip(string assetURL, Action<AudioClip> onSuccess, Action<Exception> onFailure, bool resetCache = false, bool preloadOnly = false, WebDownloadProgress progress = null, string useSAS = "", string typeOverride = "")
        {
            string fileName = GetFilename(assetURL, resetCache, typeOverride);

            AudioType guessedType = AudioType.UNKNOWN;
            if (assetURL.Contains(".wav") || (typeOverride.Equals(".wav"))) guessedType = AudioType.WAV;
            if (assetURL.Contains(".mp3") || (typeOverride.Equals(".mp3"))) guessedType = AudioType.MPEG;

            if (useInMemoryCache)
            {
                return GenericMemoryCacher(assetURL, fileName, UnityWebRequestMultimedia.GetAudioClip(assetURL + useSAS, guessedType), new DownloadHandlerAudioClip(assetURL + useSAS, guessedType),
                    (downloadHandler) => AudioLoadSuccessHandler(downloadHandler, onSuccess, onFailure),
                    exception => onFailure?.Invoke(exception), progress);
            }
            else
            {
                return GenericDownloader(assetURL, fileName,
                    UnityWebRequestMultimedia.GetAudioClip("file://" + savePath + fileName, guessedType),
                    (downloadHandler) => AudioLoadSuccessHandler(downloadHandler as DownloadHandlerAudioClip, onSuccess, onFailure),
                    exception => onFailure?.Invoke(exception), progress, preloadOnly, useSAS);
            }
        }

        public Task LoadGenericAsset(string assetURL, Action<DownloadHandler> onSuccess, Action<Exception> onFailure, bool resetCache = false, bool preloadOnly = false, WebDownloadProgress progress = null, string useSAS = "")
        {
            string fileName = GetFilename(assetURL, resetCache);

            if(useInMemoryCache)
            {
                return GenericMemoryCacher(assetURL, fileName, UnityWebRequest.Get(assetURL + useSAS), new DownloadHandlerBuffer(),
                    (downloadHandler) => LoadSuccessHandler(downloadHandler, onSuccess, onFailure),
                    exception => onFailure?.Invoke(exception), progress);
            } else
            {
                return GenericDownloader(assetURL, fileName,
                    UnityWebRequest.Get("file://" + savePath + fileName),
                    (downloadHandler) => LoadSuccessHandler(downloadHandler, onSuccess, onFailure),
                    exception => onFailure?.Invoke(exception), progress, preloadOnly, useSAS);
            }
        }


        private void ImageLoadSuccessHandler(DownloadHandler downloadHandler, Action<Texture2D> onSuccessTexture2D, Action<Sprite> onSuccessSprite, Action<Exception> onFailure, string fileName = "", int scaleToSize = -1)
        {
            DownloadHandlerTexture textureLoader = (DownloadHandlerTexture)downloadHandler;
            if (textureLoader == null)
            {
                onFailure?.Invoke(new Exception("asset not available"));
                return;
            }

            
            if(textureLoader.texture == null)
            {
                onFailure?.Invoke(new Exception("asset not available"));
                return;
            }

            Texture2D data; 
            if (scaleToSize > 0)
            {
                // Find the longest side of the image
                int longestSide = Mathf.Max(textureLoader.texture.width, textureLoader.texture.height);
                // Calculate the scale factor
                float scaleFactor = scaleToSize / (float)longestSide;

                // Scale the width and height
                int newWidth = Mathf.RoundToInt(textureLoader.texture.width * scaleFactor);
                int newHeight = Mathf.RoundToInt(textureLoader.texture.height * scaleFactor);

                if (textureLoader.texture.width != newWidth || textureLoader.texture.height != newHeight)
                {
                    data = ScaleTextureData(textureLoader.texture, newWidth, newHeight);
                    textureLoader.Dispose();
                } else
                {
                    data = textureLoader.texture;
                }
                if(data.width % 4 == 0 && data.height % 4 == 0)
                    data.Compress(false);
            } else
            {
                data = textureLoader.texture;
            }

            if (onSuccessTexture2D != null)
            {    
                onSuccessTexture2D?.Invoke(data);
                Texture2DPool.instance.AddTexture(data);
            }

            if(onSuccessSprite != null)
            {
                Sprite s = Sprite.Create(data, new Rect(0, 0, data.width, data.height), Vector2.one * .5f);
                m_CachedSprites.Add(fileName, s);
                Texture2DPool.instance.AddTexture(s);
                onSuccessSprite?.Invoke(s);
            }

        }

        private void AudioLoadSuccessHandler(DownloadHandlerAudioClip downloadHandler, Action<AudioClip> onSuccess, Action<Exception> onFailure)
        {
            if (downloadHandler == null)
            {
                onFailure?.Invoke(new Exception("asset not available"));
                return;
            }

            AudioClip data = (downloadHandler.audioClip);
            if (data == null)
            {
                onFailure?.Invoke(new Exception("asset not available"));
                return;
            }
            onSuccess?.Invoke(data);
        }

        private void LoadSuccessHandler<T>(T t, Action<T> onSuccess, Action<Exception> onFailure)
        {
            if (t == null)
            {
                onFailure?.Invoke(new Exception("asset not available"));
                return;
            }
            onSuccess?.Invoke(t);
        }

        // ----------

        private async Task GenericDownloader(string url, string filename, UnityWebRequest request_local, Action<DownloadHandler> onSuccess, Action<Exception> onFailure, WebDownloadProgress progress = null, bool preloadOnly = false, string useSAS = "")
        {
            try
            {
                // see if the local file exists - if not, download from internets
                if (!File.Exists(savePath + filename))
                {
                    // create web request and send
                    UnityWebRequest request_internet = UnityWebRequest.Get(url + useSAS);
                    request_internet.downloadHandler = new DownloadHandlerFile(savePath + filename);
                    request_internet.timeout = 1;
                    UnityWebRequestAsyncOperation op = request_internet.SendWebRequest();
                    if (progress != null) progress.StartDownload(filename);

                    ulong lastReportedBytes = 0;
                    float timeSinceProgressChange = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (progress != null) progress.ReportProgress(filename, request_internet);
                        if(lastReportedBytes != request_internet.downloadedBytes)
                        {
                            lastReportedBytes = request_internet.downloadedBytes;
                            timeSinceProgressChange = Time.realtimeSinceStartup;
                        } 
                        if(m_StopAllRunningDownloads)
                            request_internet.Abort();
                        await Task.Delay(1000 / 30);
                    }

                    if (progress != null) progress.FinishDownload(filename, url, request_internet.result == UnityWebRequest.Result.Success);
                    if (request_internet.result != UnityWebRequest.Result.Success)
                    {
                        onFailure?.Invoke(new Exception(request_internet.error));
                        return;
                    }
                    else
                    {
                        if (m_URLtoFilename.ContainsKey(url)) m_URLtoFilename.Remove(url);
                        m_URLtoFilename.Add(url, filename);
                        SaveDictionary();
                        if (preloadOnly)
                        {
                            onSuccess?.Invoke(null);
                            return;
                        }
                    }
                }

                // now attempt to load local file
                UnityWebRequestAsyncOperation op_cache = request_local.SendWebRequest();
                if (progress != null) progress.StartDownload(filename);
                while (!op_cache.isDone)
                {
                    if (m_StopAllRunningDownloads)
                        request_local.Abort();
                    if (progress != null) progress.ReportProgress(filename, request_local);
                    await Task.Yield();
                }

                // check if local download succeeded
                if (progress != null) progress.FinishDownload(filename, url, request_local.result == UnityWebRequest.Result.Success);
                if (request_local.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(request_local.downloadHandler);
                }
                else
                {
                    onFailure?.Invoke(new Exception(request_local.error));
                }
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

       
        private async Task GenericMemoryCacher<T>(string url, string filename, UnityWebRequest request_internet, T downloadHandler, Action<T> onSuccess, Action<Exception> onFailure, WebDownloadProgress progress = null) where T : DownloadHandler
        {
            // otherwise download anew
            try
            {
                // create web request and send
                request_internet.downloadHandler = downloadHandler;
                UnityWebRequestAsyncOperation op = request_internet.SendWebRequest();
                if (progress != null) progress.StartDownload(filename);

                ulong lastReportedBytes = 0;
                float timeSinceProgressChange = Time.realtimeSinceStartup;

                while (!op.isDone)
                {
                    if (progress != null) progress.ReportProgress(filename, request_internet);
                    if (lastReportedBytes != request_internet.downloadedBytes)
                    {
                        lastReportedBytes = request_internet.downloadedBytes;
                        timeSinceProgressChange = Time.realtimeSinceStartup;
                    }
                    else
                    {
                        if (Mathf.Abs(timeSinceProgressChange - Time.realtimeSinceStartup) > 10)
                            request_internet.timeout = 1;
                    }
                    if (m_StopAllRunningDownloads)
                        request_internet.Abort();
                    await Task.Delay(1000 / 30);
                }

                if (progress != null) progress.FinishDownload(filename, url, request_internet.result == UnityWebRequest.Result.Success);
                if (request_internet.result != UnityWebRequest.Result.Success)
                {
                    onFailure?.Invoke(new Exception(request_internet.error));
                    return;
                }
                onSuccess?.Invoke(request_internet.downloadHandler as T);
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e);
            }
        }

#if UNITY_EDITOR
        public async static Task Download<T>(string webURL, string savePath, UnityAction<T> onSuccess, UnityAction<Exception> onFailure) where T : UnityEngine.Object
        {
            // create web request and send
            UnityWebRequest request_internet = UnityWebRequest.Get(webURL);
            request_internet.downloadHandler = new DownloadHandlerFile(savePath);
            UnityWebRequestAsyncOperation op = request_internet.SendWebRequest();

            while (!op.isDone)
                await Task.Delay(1000 / 30);

            if (request_internet.result != UnityWebRequest.Result.Success)
            {
                onFailure?.Invoke(new Exception(request_internet.error));
            }
            else
            {
                AssetDatabase.ImportAsset(savePath);
                AssetDatabase.Refresh();
                T c = AssetDatabase.LoadAssetAtPath<T>(savePath);
                onSuccess?.Invoke(c);
            }
        }
#endif

        // ----
        // load and save

        void LoadDictionary()
        {
            m_URLtoFilename.Clear();
            m_URLtoFilename = SaveGame.instance.metaSaveData.GetObject("filename_database", new Dictionary<string, string>());
        }

        void SaveDictionary()
        {
            SaveGame.instance.metaSaveData.SetObject("filename_database", m_URLtoFilename);
            _ = SaveGame.instance.Save();
        }

        static string GetFileExtensionFromUrl(string url)
        {
            url = url.Split('?')[0];
            url = url.Split('/').Last();
            return url.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";
        }

        public void Clear()
        {
            if (!useInMemoryCache)
            {
                foreach (string filename in m_URLtoFilename.Values)
                {
                    if (File.Exists(savePath + filename))
                    {
                        Dbg.Log(this, "Deleting: " + savePath + filename);
                        try
                        {
                            File.Delete(savePath + filename);
                        }
                        catch (Exception e)
                        {
                            Dbg.Exception(this, e);
                        }
                    }
                }

                m_URLtoFilename.Clear();
                SaveDictionary();
            }
        }


        public static Texture2D ScaleTextureData(Texture2D source, int targetWidth, int targetHeight)
        {
            // Create a new render texture of the target size
            RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 24);
            RenderTexture currentActiveRT = RenderTexture.active;

            // Set the created render texture as active and blit the source texture into it
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            // Create a new 2D texture and read the active render texture into it
            Texture2D result = new Texture2D(targetWidth, targetHeight);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();

            // Restore the active render texture
            RenderTexture.active = currentActiveRT;

            // Clean up render texture
            rt.Release();
            Destroy(rt);
            return result;
        }

        public void Verify(CheckVerifyInterface checker)
        {
        }
    }
}